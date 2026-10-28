using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.IO;
namespace Garry
{
    public class ChatGptService
    {
        private static ChatGptService _instance;
        private const string ApiUrl = "https://api.openai.com/v1/chat/completions";
        private readonly string _apiKey;
        private const int MaxChatHistorySize = 50; // Limit chat history to prevent unbounded memory growth

        private List<ChatMessage> _chatHistory;

        private class ChatMessage
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        public ChatGptService(string apiKey)
        {
            _apiKey = apiKey;
            _chatHistory = new List<ChatMessage>();
        }

        public static ChatGptService GetInstance(string apiKey)
        {
            if (_instance == null)
            {
                _instance = new ChatGptService(apiKey);
            }
            return _instance;
        }

        public async Task<string> GetChatGptResponse(string userInput)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                _chatHistory.Add(new ChatMessage { role = "user", content = userInput });
                
                // Trim history if it exceeds max size
                if (_chatHistory.Count > MaxChatHistorySize)
                {
                    _chatHistory.RemoveRange(0, _chatHistory.Count - MaxChatHistorySize);
                }

                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = _chatHistory
                };

                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(ApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic responseJson = JsonConvert.DeserializeObject(responseBody);

                    string chatGptResponse = responseJson.choices[0].message.content.ToString().Trim();

                    _chatHistory.Add(new ChatMessage { role = "assistant", content = chatGptResponse });
                    return chatGptResponse;
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    return errorBody;
                }
            }
        }

        public async Task StreamChatGptResponse(string userInput, Action<string> onResponseReceived)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
                _chatHistory.Add(new ChatMessage { role = "user", content = userInput });
                
                // Trim history if it exceeds max size
                if (_chatHistory.Count > MaxChatHistorySize)
                {
                    _chatHistory.RemoveRange(0, _chatHistory.Count - MaxChatHistorySize);
                }

                // Create the request body with streaming enabled
                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = _chatHistory,
                    stream = true,
                };

                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
                {
                    Content = content
                };

                HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();

                    // Use StreamReader to read chunks of data from the response stream
                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        string completeResponse = "";
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            // Skip empty lines and the "data: " prefix
                            line = line.Trim();
                            if (string.IsNullOrEmpty(line) || line == "data: [DONE]")
                                continue;

                            if (line.StartsWith("data: "))
                            {
                                line = line.Substring(6); // Remove "data: " prefix
                            }

                            // Parse each chunk of data to get the content
                            if (line.Contains("\"content\":"))
                            {
                                var message = ExtractMessageContent(line);
                                if (!string.IsNullOrEmpty(message))
                                {
                                    completeResponse += message;
                                    onResponseReceived?.Invoke(message);  // Callback with the partial response
                                }
                            }
                        }
                        _chatHistory.Add(new ChatMessage { role = "assistant", content = completeResponse });
                    }
                }
                else
                {
                    onResponseReceived?.Invoke("Error: Unable to fetch response from API.");
                }
            }
        }

        // Extract message content from streamed response using JSON parsing
        private string ExtractMessageContent(string responseLine)
        {
            try
            {
                // Parse the JSON line
                dynamic json = JsonConvert.DeserializeObject(responseLine);
                
                // Navigate through the response structure: choices[0].delta.content
                if (json?.choices != null && json.choices.Count > 0)
                {
                    var delta = json.choices[0].delta;
                    if (delta?.content != null)
                    {
                        string content = delta.content.ToString();
                        return content;
                    }
                }
            }
            catch
            {
                // If JSON parsing fails, return empty string
            }

            return string.Empty;
        }

        public void ClearChatHistory()
        {
            _chatHistory.Clear();
        }
    }
}
