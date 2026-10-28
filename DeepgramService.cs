using Deepgram;
using Deepgram.Clients.Interfaces.v1;
using Deepgram.Models.Listen.v2.WebSocket;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Garry
{
    public class DeepgramService
    {
        private static DeepgramService _instance;
        private readonly string _apiKey;
        private readonly string STREAM_URL = "https://playerservices.streamtheworld.com/api/livestream-redirect/CSPANRADIOAAC.aac";
        private static readonly System.Text.RegularExpressions.Regex WhitespaceRegex = new System.Text.RegularExpressions.Regex("\\s+", System.Text.RegularExpressions.RegexOptions.Compiled);

        private ListenWebSocketClient _liveClient;
        private WasapiLoopbackCapture _waveIn;
        private CancellationTokenSource _keepaliveCts;
        private Task _keepaliveTask;

        private bool _isTranscribing = false;
        private DateTime _lastAudioAtUtc;
        private int _interimStartIndex = -1;
        private string _currentInterim = string.Empty;
        private int _incrementalPrintLength = 0;

        // Events for UI callbacks
        public event Action<string> OnPartialTranscription;
        public event Action<string> OnFinalTranscription;
        public event Action<string> OnError;

        public DeepgramService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public static DeepgramService GetInstance(string apiKey)
        {
            if (_instance == null)
            {
                _instance = new DeepgramService(apiKey);
            }
            return _instance;
        }

        public async Task StartTranscriptionAsync()
        {
            // Validate API key
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                OnError?.Invoke("Deepgram API key missing. Please provide a valid API key.");
                return;
            }

            try
            {
                // Create Deepgram WS client
                Library.Initialize();

                _liveClient = new ListenWebSocketClient(_apiKey);
                _incrementalPrintLength = 0;

                await _liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
                {
                    HandleTranscriptionResult(e);
                }));

                // Prepare system audio capture
                _waveIn = new WasapiLoopbackCapture();
                var sourceFormat = _waveIn.WaveFormat;
                int sampleRate = sourceFormat.SampleRate;

                var liveSchema = new LiveSchema
                {
                    Model = "nova-3",
                    Encoding = "linear16",
                    SampleRate = sampleRate,
                    Punctuate = true,
                    Diarize = true,
                    Language = "en-US",
                    SmartFormat = true,
                    InterimResults = true,
                    VadEvents = true,
                    UtteranceEnd = "1500"
                };

                if (!await _liveClient.Connect(liveSchema))
                {
                    OnError?.Invoke("Failed to connect to Deepgram service");
                    return;
                }

                _lastAudioAtUtc = DateTime.UtcNow;

                _waveIn.DataAvailable += (s, a) => HandleAudioData(a, sourceFormat);
                _waveIn.RecordingStopped += async (s, e) =>
                {
                    try { await _liveClient.Stop(); } catch { }
                };

                // Start keepalive task
                StartKeepalive(sampleRate);

                _isTranscribing = true;
                _waveIn.StartRecording();
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Error starting transcription: {ex.Message}");
            }
        }

        public async void StopTranscription()
        {
            try { _isTranscribing = false; } catch { }
            try { _keepaliveCts?.Cancel(); } catch { }
            try { _waveIn?.StopRecording(); } catch { }
            try
            {
                if (_liveClient != null)
                {
                    await _liveClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
                    {
                        Console.WriteLine("Connection closed.");
                    }));
                    await _liveClient.Stop();
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Error stopping transcription: {ex.Message}");
            }
        }

        private void HandleTranscriptionResult(ResultResponse result)
        {
            try
            {
                var alt = result.Channel?.Alternatives != null && result.Channel.Alternatives.Count > 0 
                    ? result.Channel.Alternatives[0] 
                    : null;
                var raw = alt?.Transcript ?? string.Empty;

                // Normalize whitespace and strip newlines for smoother captioning
                string normalized = string.IsNullOrWhiteSpace(raw) 
                    ? string.Empty 
                    : WhitespaceRegex.Replace(raw.Replace('\n', ' ').Replace('\r', ' '), " ").TrimStart();

                if (result.IsFinal == true)
                {
                    // Invoke final transcription event
                    if (!string.IsNullOrEmpty(normalized))
                    {
                        OnFinalTranscription?.Invoke(normalized);
                    }

                    // Reset interim tracking
                    _interimStartIndex = -1;
                    _currentInterim = string.Empty;
                    _incrementalPrintLength = 0;
                }
                else
                {
                    // Interim: invoke partial transcription event
                    if (!string.IsNullOrEmpty(normalized))
                    {
                        OnPartialTranscription?.Invoke(normalized);
                        _currentInterim = normalized;
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Error processing transcription: {ex.Message}");
            }
        }

        private void HandleAudioData(WaveInEventArgs args, WaveFormat sourceFormat)
        {
            if (!_isTranscribing || args.BytesRecorded <= 0)
                return;

            try
            {
                byte[] audioBuffer = ConvertAudioFormat(args, sourceFormat);
                if (audioBuffer != null && audioBuffer.Length > 0)
                {
                    _liveClient.Send(audioBuffer);
                    _lastAudioAtUtc = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Error sending audio data: {ex.Message}");
            }
        }

        private byte[] ConvertAudioFormat(WaveInEventArgs args, WaveFormat sourceFormat)
        {
            if (sourceFormat.Encoding == WaveFormatEncoding.IeeeFloat && sourceFormat.BitsPerSample == 32)
            {
                return ConvertFloatToPcm(args, sourceFormat);
            }
            else if (sourceFormat.Encoding == WaveFormatEncoding.Pcm && sourceFormat.BitsPerSample == 16)
            {
                return ConvertPcmToMono(args, sourceFormat);
            }
            
            return null;
        }

        private byte[] ConvertFloatToPcm(WaveInEventArgs args, WaveFormat sourceFormat)
        {
            int bytesPerSample = 4;
            int channels = sourceFormat.Channels;
            int sampleCount = args.BytesRecorded / bytesPerSample;
            int frameCount = sampleCount / channels;
            byte[] outBuffer = new byte[frameCount * 2];

            int inOffset = 0;
            int outOffset = 0;
            for (int i = 0; i < frameCount; i++)
            {
                float sum = 0f;
                for (int c = 0; c < channels; c++)
                {
                    float sample = BitConverter.ToSingle(args.Buffer, inOffset + (c * bytesPerSample));
                    sum += sample;
                }
                float mono = sum / channels;
                mono = Math.Max(-1f, Math.Min(1f, mono));
                short pcm = (short)Math.Round(mono * short.MaxValue);
                outBuffer[outOffset] = (byte)(pcm & 0xFF);
                outBuffer[outOffset + 1] = (byte)((pcm >> 8) & 0xFF);

                inOffset += channels * bytesPerSample;
                outOffset += 2;
            }

            return outBuffer;
        }

        private byte[] ConvertPcmToMono(WaveInEventArgs args, WaveFormat sourceFormat)
        {
            int channels = sourceFormat.Channels;
            if (channels == 1)
            {
                return args.Buffer.AsSpan(0, args.BytesRecorded).ToArray();
            }

            int bytesPerSample = 2;
            int sampleCount = args.BytesRecorded / bytesPerSample;
            int frameCount = sampleCount / channels;
            byte[] outBuffer = new byte[frameCount * 2];

            int inOffset = 0;
            int outOffset = 0;
            for (int i = 0; i < frameCount; i++)
            {
                int sum = 0;
                for (int c = 0; c < channels; c++)
                {
                    short s16 = (short)(args.Buffer[inOffset] | (args.Buffer[inOffset + 1] << 8));
                    sum += s16;
                    inOffset += 2;
                }
                short mono = (short)(sum / channels);
                outBuffer[outOffset] = (byte)(mono & 0xFF);
                outBuffer[outOffset + 1] = (byte)((mono >> 8) & 0xFF);
                outOffset += 2;
            }

            return outBuffer;
        }

        private void StartKeepalive(int sampleRate)
        {
            _keepaliveCts = new CancellationTokenSource();
            var token = _keepaliveCts.Token;
            _keepaliveTask = Task.Run(async () =>
            {
                int frameCount = (int)(sampleRate * 0.02);
                byte[] silence = new byte[frameCount * 2];
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (_isTranscribing && (DateTime.UtcNow - _lastAudioAtUtc) > TimeSpan.FromMilliseconds(600))
                        {
                            _liveClient.Send(silence);
                        }
                    }
                    catch { }
                    try { await Task.Delay(300, token); } catch { }
                }
            }, token);
        }

        public bool IsTranscribing => _isTranscribing;
    }
}