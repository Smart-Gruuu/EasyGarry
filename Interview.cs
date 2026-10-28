using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Garry;

namespace EasyGarry
{
    public partial class Interview : Form
    {
        private ChatGptService _chatGptService;
        private DeepgramService _deepgramService;
        private List<string> messageHistory = new List<string>();
        private List<string> scriptHistory = new List<string>();
        private List<string> messageTitles = new List<string>();

        private float FontSize = 12.0f;

        // Cached images to avoid repeated disk I/O
        private Dictionary<string, Image> _imageCache = new Dictionary<string, Image>();
        private readonly object _imageCacheLock = new object();

        private const int ResizeBorderSize = 8;
        private const int TitleBarHeight = 40;
        private const int ContentTop = TitleBarHeight + 12;
        private const int ContentBottomMargin = 12;
        private static readonly Size MinimumWindowSize = new Size(720, 480);

        private const int WM_NCHITTEST = 0x0084;
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        private bool _isPinned = false;
        private bool _isLight = true;
        private bool _isTranscribing = false;
        private Panel titleBar;
        private Button modeButton;
        private Button pinButton;
        private Button closeButton;
        private Button minimizeButton;
        private Button stealthButton;
        private Button transcribeButton;
        TrackBar opacitySlider;

        // Cached controls for efficient iteration
        private TextBoxBase[] _textBoxControls;
        private ListBox[] _listBoxControls;

        // Application open time for history file naming
        private readonly DateTime _appOpenTime;

        // Whether user is viewing history item
        private bool _isViewingHistory = false;

        // Stealth mode
        private bool _isStealth = false;

        // Win32 constants for click-through & hotkey
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        // Display affinity values for preventing screen capture
        private const uint WDA_NONE = 0x00000000;
        private const uint WDA_MONITOR = 0x00000011; // protect window from capture

        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 0x1000;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private extern static int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int EM_GETFIRSTVISIBLELINE = 0x00CE;
        private const int EM_LINESCROLL = 0x00B6;
        private const int WM_SETREDRAW = 0x000B;
        private const int WM_SYSCOMMAND = 0x112;
        private const int SC_MOVE = 0xF012;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        private const byte VK_CONTROL = 0x11;
        private const int KEYEVENTF_KEYUP = 0x0002;

        // Custom RichTextBox that ignores mouse wheel and keyboard scrolling
        public class NoScrollRichTextBox : RichTextBox
        {
            protected override void OnMouseWheel(MouseEventArgs e)
            {
                // ignore mouse wheel to prevent scrolling
                return;
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                // suppress keys that cause scrolling
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ||
                    e.KeyCode == Keys.Left || e.KeyCode == Keys.Right ||
                    e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown ||
                    e.KeyCode == Keys.Home || e.KeyCode == Keys.End)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
                base.OnKeyDown(e);
            }
        }

        public Interview()
        {
            InitializeComponent();
            InitializeCustomTitleBar();
            CacheControls();
            SetMode(_isLight);
            // Supplied by the user on first run and stored per Windows account; the setup
            // screen will not let this window open until both keys are present.
            var keys = ApiKeyStore.Load() ?? new ApiKeys();
            _chatGptService = ChatGptService.GetInstance(keys.OpenAi);
            _deepgramService = DeepgramService.GetInstance(keys.Deepgram);
            SubscribeToDeepgramEvents();
            _ = ReadyInterview(); // Start async task without blocking constructor

            // Record open time immediately when form constructed
            _appOpenTime = DateTime.Now;

            this.MinimumSize = MinimumWindowSize;
            this.MouseWheel += HandleMouseScroll;
            this.Resize += (s, e) => ApplyClientLayout();

            // If user focuses the script box, they are not viewing history anymore
            try { this.textScript.Enter += (s, e) => { _isViewingHistory = false; }; } catch { }
        }

        // Append text to RichTextBox without changing the visible viewport.
        private void AppendTextPreserveScroll(RichTextBox rtb, string text)
        {
            if (rtb == null || text == null) return;
            try
            {
                int oldFirst = SendMessage(rtb.Handle, EM_GETFIRSTVISIBLELINE, 0, 0);
                int selStart = rtb.SelectionStart;
                int selLen = rtb.SelectionLength;

                // Suspend redraw to avoid visual jump to caret/end
                SendMessage(rtb.Handle, WM_SETREDRAW, 0, 0);
                rtb.AppendText(text);

                // Restore previous selection so caret stays where it was
                if (selStart <= rtb.Text.Length)
                {
                    rtb.SelectionStart = selStart;
                    rtb.SelectionLength = selLen;
                }

                // Resume redraw and refresh
                SendMessage(rtb.Handle, WM_SETREDRAW, 1, 0);
                rtb.Invalidate();

                int newFirst = SendMessage(rtb.Handle, EM_GETFIRSTVISIBLELINE, 0, 0);
                int delta = oldFirst - newFirst;
                if (delta != 0)
                {
                    SendMessage(rtb.Handle, EM_LINESCROLL, 0, delta);
                }
            }
            catch
            {
                try { rtb.AppendText(text); } catch { }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                ToggleStealthMode();
                return;
            }

            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                if ((int)m.Result == HTCLIENT)
                {
                    int hit = HitTestClientArea(m);
                    if (hit != HTCLIENT)
                        m.Result = (IntPtr)hit;
                }
                return;
            }

            base.WndProc(ref m);
        }

        private int HitTestClientArea(Message m)
        {
            int x = (short)(m.LParam.ToInt32() & 0xFFFF);
            int y = (short)((m.LParam.ToInt32() >> 16) & 0xFFFF);
            Point clientPoint = PointToClient(new Point(x, y));

            int borderHit = HitTestResizeBorder(clientPoint);
            if (borderHit != HTCLIENT)
                return borderHit;

            if (clientPoint.Y < TitleBarHeight && !IsOverTitleBarControl(clientPoint))
                return HTCAPTION;

            return HTCLIENT;
        }

        private int HitTestResizeBorder(Point clientPoint)
        {
            int grip = ResizeBorderSize;
            int w = ClientSize.Width;
            int h = ClientSize.Height;

            bool onLeft = clientPoint.X < grip;
            bool onRight = clientPoint.X >= w - grip;
            bool onTop = clientPoint.Y < grip;
            bool onBottom = clientPoint.Y >= h - grip;

            if (onTop && onLeft) return HTTOPLEFT;
            if (onTop && onRight) return HTTOPRIGHT;
            if (onBottom && onLeft) return HTBOTTOMLEFT;
            if (onBottom && onRight) return HTBOTTOMRIGHT;
            if (onTop) return HTTOP;
            if (onBottom) return HTBOTTOM;
            if (onLeft) return HTLEFT;
            if (onRight) return HTRIGHT;

            return HTCLIENT;
        }

        private bool IsOverTitleBarControl(Point clientPoint)
        {
            if (titleBar == null || clientPoint.Y >= TitleBarHeight)
                return false;

            Point titlePoint = titleBar.PointToClient(PointToScreen(clientPoint));
            foreach (Control control in titleBar.Controls)
            {
                if (control.Visible && control.Bounds.Contains(titlePoint))
                    return true;
            }

            return false;
        }

        private void ToggleStealthMode()
        {
            _isStealth = !_isStealth;
            if (_isStealth)
            {
                // Make window click-through and hide from taskbar (toolwindow)
                int ex = GetWindowLong(this.Handle, GWL_EXSTYLE);
                ex |= WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_TOOLWINDOW;
                SetWindowLong(this.Handle, GWL_EXSTYLE, ex);
                this.ShowInTaskbar = false;
                stealthButton.Image = GetCachedImage(_isLight ? "image/stealth_on_light.png" : "image/stealth_on_dark.png");

                // Prevent window content from being captured by screen capture / screen sharing
                try { SetWindowDisplayAffinity(this.Handle, WDA_MONITOR); } catch { }
            }
            else
            {
                int ex = GetWindowLong(this.Handle, GWL_EXSTYLE);
                ex &= ~WS_EX_TRANSPARENT;
                ex &= ~WS_EX_TOOLWINDOW;
                SetWindowLong(this.Handle, GWL_EXSTYLE, ex);
                this.ShowInTaskbar = true;
                stealthButton.Image = GetCachedImage(_isLight ? "image/stealth_off_light.png" : "image/stealth_off_dark.png");

                // Remove display affinity so window can be captured again
                try { SetWindowDisplayAffinity(this.Handle, WDA_NONE); } catch { }
            }
        }

        private void CacheControls()
        {
            var textBoxes = new List<TextBoxBase>(4);
            var listBoxes = new List<ListBox>(2);
            foreach (Control ctrl in this.Controls)
            {
                switch (ctrl)
                {
                    case TextBoxBase tb:
                        textBoxes.Add(tb);
                        break;
                    case ListBox lb:
                        listBoxes.Add(lb);
                        break;
                }
            }
            _textBoxControls = textBoxes.ToArray();
            _listBoxControls = listBoxes.ToArray();
        }

        /// <summary>
        /// Loads a themed icon. Relative paths resolve against the executable's folder, not
        /// the working directory, so launching via a shortcut or debugger behaves the same.
        /// Callers spell the same file both "image/x.png" and "./image/x.png", so the
        /// resolved full path is the cache key - otherwise the same PNG loads twice.
        /// </summary>
        private Image GetCachedImage(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            string fullPath;
            try
            {
                fullPath = Path.IsPathRooted(path)
                    ? path
                    : Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
            }
            catch
            {
                return null;
            }

            lock (_imageCacheLock)
            {
                if (_imageCache.TryGetValue(fullPath, out var img))
                    return img;

                try
                {
                    img = Image.FromFile(fullPath);
                }
                catch
                {
                    // Not on disk: fall back to the copy embedded in the exe, which is what
                    // makes the single-file build work with nothing beside it. A PNG placed
                    // next to the exe still wins, so icons stay overridable.
                    img = LoadEmbeddedImage(Path.GetFileName(fullPath));
                }

                _imageCache[fullPath] = img;
                return img;
            }
        }

        private static Image LoadEmbeddedImage(string fileName)
        {
            try
            {
                using (var stream = EmbeddedAssemblies.OpenImage(fileName))
                {
                    if (stream == null)
                        return null;

                    // Image.FromStream keeps the stream alive for the image's lifetime, so
                    // copy into memory the image can own.
                    var buffer = new MemoryStream();
                    stream.CopyTo(buffer);
                    buffer.Position = 0;
                    return Image.FromStream(buffer);
                }
            }
            catch
            {
                return null;
            }
        }

        private void SubscribeToDeepgramEvents()
        {
            _deepgramService.OnFinalTranscription += (text) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (_isViewingHistory == true)
                        {
                            textScript.Clear();
                            _isViewingHistory = false;
                        }
                        if (!textScript.Text.EndsWith(" "))
                        {
                            textScript.AppendText(" ");
                        }
                        textScript.AppendText(text);

                    }
                }));
            };

            _deepgramService.OnError += (errorMessage) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show($"Transcription Error: {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            };
        }

        private void InitializeCustomTitleBar()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;

            titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Width = this.Width,
                BackColor = Color.FromArgb(255, 255, 255, 255)
            };

            closeButton = CreateTitleBarButton("image/close_light.png", new Point(titleBar.Width - 50, 0), new Size(50,40), CloseButton_Click);
            closeButton.Click += CloseButton_Click;
            titleBar.Controls.Add(closeButton);

            minimizeButton = new Button
            {
                Image = GetCachedImage("image/minimize_light.png"),
                Location = new Point(700, 0),
                Size = new Size(50, 40),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            minimizeButton.Click += MinimizeButton_Click;
            titleBar.Controls.Add(minimizeButton);

            pinButton = CreateTitleBarButton("image/pin_off_light.png", new Point(650,0), new Size(50,40), PinButton_Click);
            titleBar.Controls.Add(pinButton);

            modeButton = CreateTitleBarButton("image/light.png", new Point(600,0), new Size(50,40), ModeButton_Click);
            titleBar.Controls.Add(modeButton);

            stealthButton = CreateTitleBarButton(null, new Point(550,0), new Size(50,40), StealthButton_Click);
            stealthButton.Image = GetCachedImage(_isLight ? "image/stealth_off_light.png" : "image/stealth_off_dark.png");
            titleBar.Controls.Add(stealthButton);

            transcribeButton = CreateTitleBarButton("image/play_light.png", new Point(100,0), new Size(50,40), TranscribeButton_Click);
            transcribeButton.Font = new Font("Arial", 16);
            titleBar.Controls.Add(transcribeButton);

            opacitySlider = new TrackBar
            {
                Minimum = 10,
                Maximum = 100,
                Value = 90,
                Location = new System.Drawing.Point(400, 10),
                Size = new System.Drawing.Size(150, 30),
                TickStyle = TickStyle.None
            };
            opacitySlider.ValueChanged += OpacitySlider_ValueChanged;
            titleBar.Controls.Add(opacitySlider);

            this.Controls.Add(titleBar);
            titleBar.MouseDown += TitleBar_MouseDown;

            // helper to create consistent title bar buttons
            Button CreateTitleBarButton(string imagePath, Point location, Size size, EventHandler onClick)
            {
                var btn = new Button
                {
                    Image = string.IsNullOrEmpty(imagePath) ? null : GetCachedImage(imagePath),
                    Location = location,
                    Size = size,
                    FlatStyle = FlatStyle.Flat,
                };
                btn.FlatAppearance.BorderSize = 0;
                if (onClick != null) btn.Click += onClick;
                return btn;
            }

            // Register global hotkey Ctrl+Shift+S for stealth toggle
            try
            {
                RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, (int)Keys.S);
            }
            catch { }

            ApplyClientLayout();
        }

        private void ApplyClientLayout()
        {
            if (titleBar == null)
                return;

            int contentHeight = Math.Max(0, ClientSize.Height - ContentTop - ContentBottomMargin);

            titleBar.Width = ClientSize.Width;

            const int buttonWidth = 50;
            closeButton.Left = titleBar.Width - buttonWidth;
            minimizeButton.Left = titleBar.Width - buttonWidth * 2;
            pinButton.Left = titleBar.Width - buttonWidth * 3;
            modeButton.Left = titleBar.Width - buttonWidth * 4;
            stealthButton.Left = titleBar.Width - buttonWidth * 5;
            opacitySlider.Left = Math.Max(buttonWidth * 2, titleBar.Width - 400);
            transcribeButton.Left = 100;

            textScript.Width = ClientSize.Width * 3 / 10;
            textScript.Height = contentHeight;
            textScript.Top = ContentTop;

            textAnswer.Width = ClientSize.Width * 5 / 10;
            textAnswer.Height = contentHeight;
            textAnswer.Top = ContentTop;
            textAnswer.Left = ClientSize.Width * 3 / 10 + 12;

            listHistory.Left = ClientSize.Width * 8 / 10 + 12;
            listHistory.Width = Math.Max(0, ClientSize.Width * 2 / 10 - 24);
            listHistory.Height = contentHeight;
        }

        private async void TranscribeButton_Click(object sender, EventArgs e)
        {
            if (_deepgramService.IsTranscribing)
            {
                _isTranscribing = !_isTranscribing;
                _deepgramService.StopTranscription();
                transcribeButton.Image = GetCachedImage(_isLight ? "image/play_light.png" : "image/play_dark.png");
            }
            else
            {
                _isTranscribing = !_isTranscribing;
                transcribeButton.Image = GetCachedImage(_isLight ? "image/pause_light.png" : "image/pause_dark.png");
                await _deepgramService.StartTranscriptionAsync();
            }
        }

        private void ModeButton_Click(object sender, EventArgs e)
        {
            _isLight = !_isLight;
            SetMode(_isLight);
            modeButton.Image = GetCachedImage(_isLight ? "image/light.png" : "image/dark.png");
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (_deepgramService.IsTranscribing)
            {
                _deepgramService.StopTranscription();
            }
            this.Close();
        }
        
        private void StealthButton_Click(object sender, EventArgs e)
        {
            ToggleStealthMode();
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || e.Y < ResizeBorderSize)
                return;

            ReleaseCapture();
            SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE, 0);
        }

        private void PinButton_Click(object sender, EventArgs e)
        {
            _isPinned = !_isPinned;
            TopMost = _isPinned;
            string imagePath = _isPinned ? (_isLight ? "image/pin_on_light.png" : "image/pin_on_dark.png") : (_isLight ? "image/pin_off_light.png" : "image/pin_off_dark.png");
            pinButton.Image = GetCachedImage(imagePath);
        }

        private void OpacitySlider_ValueChanged(object sender, EventArgs e)
        {
            TrackBar opacitySlider = (TrackBar)sender;
            this.Opacity = opacitySlider.Value / 100.0;
        }

        private void SetMode(bool mode)
        {
            if (mode)
            {
                titleBar.BackColor = Color.FromArgb(255, 230, 230, 230);
                this.BackColor = Color.FromArgb(255, 255, 255, 255);
                this.ForeColor = Color.FromArgb(255, 31, 31, 31);
                transcribeButton.ForeColor = Color.FromArgb(255, 31, 31, 31);
                Color lightBg = Color.FromArgb(255, 255, 255, 255);
                Color lightFg = Color.FromArgb(255, 31, 31, 31);
                foreach (var tb in _textBoxControls)
                {
                    tb.BackColor = lightBg;
                    tb.ForeColor = lightFg;
                }
                foreach (var lb in _listBoxControls)
                {
                    lb.BackColor = lightBg;
                    lb.ForeColor = lightFg;
                }
                minimizeButton.Image = GetCachedImage("./image/minimize_light.png");
                closeButton.Image = GetCachedImage("./image/close_light.png");
                string pinPath = _isPinned ? "./image/pin_on_light.png" : "./image/pin_off_light.png";
                pinButton.Image = GetCachedImage(pinPath);
                stealthButton.Image = GetCachedImage(_isStealth ? (_isLight ? "image/stealth_on_light.png" : "image/stealth_on_dark.png") : (_isLight ? "image/stealth_off_light.png" : "image/stealth_off_dark.png"));
                transcribeButton.Image = GetCachedImage(_isTranscribing ? "image/pause_light.png" : "image/play_light.png");
            }
            else
            {
                titleBar.BackColor = Color.FromArgb(255, 10, 10, 10);
                this.BackColor = Color.FromArgb(255, 31, 31, 31);
                this.ForeColor = Color.White;
                transcribeButton.ForeColor = Color.White;
                Color darkBg = Color.FromArgb(255, 31, 31, 31);
                foreach (var tb in _textBoxControls)
                {
                    tb.BackColor = darkBg;
                    tb.ForeColor = Color.White;
                }
                foreach (var lb in _listBoxControls)
                {
                    lb.BackColor = darkBg;
                    lb.ForeColor = Color.White;
                }
                minimizeButton.Image = GetCachedImage("./image/minimize_dark.png");
                closeButton.Image = GetCachedImage("./image/close_dark.png");
                string pinPath = _isPinned ? "./image/pin_on_dark.png" : "./image/pin_off_dark.png";
                pinButton.Image = GetCachedImage(pinPath);
                stealthButton.Image = GetCachedImage(_isStealth ? (_isLight ? "image/stealth_on_light.png" : "image/stealth_on_dark.png") : (_isLight ? "image/stealth_off_light.png" : "image/stealth_off_dark.png"));
                transcribeButton.Image = GetCachedImage(_isTranscribing ? "image/pause_dark.png" : "image/play_dark.png");
            }
        }

        private void HandleMouseScroll(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                // If scroll up, increase font size; scroll down, decrease font size
                if (e.Delta > 0)
                    FontSize += 1;
                else if (e.Delta < 0)
                    FontSize -= 1;

                // Ensure the font size stays within a reasonable range
                FontSize = Math.Max(FontSize, 6); // Minimum font size = 6
                FontSize = Math.Min(FontSize, 36); // Maximum font size = 36

                // Apply the new font size to all controls
                SetFontSize(FontSize);
            }
        }

        private void SetFontSize(float newFontSize)
        {
            // Apply the new font size using cached controls
            foreach (var tb in _textBoxControls)
            {
                tb.Font = new Font(tb.Font.FontFamily, newFontSize);
            }
            foreach (var lb in _listBoxControls)
            {
                lb.Font = new Font(lb.Font.FontFamily, newFontSize);
            }
        }

        private async Task ReadyInterview()
        {
            // Written by the setup screen before this window opens.
            string resumeText = InterviewInputs.ReadOrEmpty(InterviewInputs.ResumePath);
            string jobDescriptionText = InterviewInputs.ReadOrEmpty(InterviewInputs.JobDescriptionPath);
            if (!string.IsNullOrWhiteSpace(resumeText) && !string.IsNullOrWhiteSpace(jobDescriptionText))
            {
                string message = $"I applied for this job with the resume content below:\n{resumeText}\n\n" +
                                "Refer these blogs to help me\nhttps://www.linkedin.com/posts/chrismmullen_most-people-dont-bomb-interviews-from-lack-activity-7341454828268638210-1E31?utm_source=share&utm_medium=member_desktop&rcm=ACoAAFpFqCcBcO9cYUngVrbpkFs5k9KXZOh4P-s \nhttps://www.linkedin.com/posts/benjamin-olden_i-see-this-interview-mistake-over-and-over-activity-7341344116229693440-rvSS?utm_source=share&utm_medium=member_desktop&rcm=ACoAAFpFqCcBcO9cYUngVrbpkFs5k9KXZOh4P-s\nhttps://www.linkedin.com/posts/dikshaarora-careercoach_resumetips-interviewcoach-jobsearch-activity-7340962725583298561-Th2O?utm_source=share&utm_medium=member_desktop&rcm=ACoAAFpFqCcBcO9cYUngVrbpkFs5k9KXZOh4P-s\nhttps://www.linkedin.com/posts/benjamin-olden_ive-interviewed-20-candidates-so-far-and-activity-7341857486523490304-498-?utm_source=share&utm_medium=member_desktop&rcm=ACoAAFpFqCcBcO9cYUngVrbpkFs5k9KXZOh4P-s\nhttps://www.linkedin.com/pulse/what-hiring-managers-really-look-job-interviews-beyond-kc3yf?trk=article-ssr-frontend-pulse\nhttps://growthhackyourcareer.com/questions-to-ask-interviewer/\nhttps://johnclements.com/the-looking-glass/career-advice/what-you-know-about-company/\nhttps://www.themuse.com/advice/4-steps-for-answering-what-are-you-looking-for-in-a-new-position\nhttps://www.thebalancemoney.com/why-are-you-the-best-person-for-the-job-2062760\nhttps://www.talentport.com/blog/why-are-you-interested-in-this-position\nhttps://www.themuse.com/advice/tell-me-about-yourself-interview-question-answer-examples\nhttps://www.linkedin.com/feed/update/urn:li:activity:7366802768830840832/\n\n" +
                                 "You are a senior-level job candidate in a spoken interview. Answer everything in natural, conversational American English - like you're actually talking to someone face to face. Use casual words, slang, short sentences, and occasional laugh sounds (like \"haha\" or \"you know\"). Keep it simple and easy to read out loud. For non-technical questions, give short, real-life answers - no deep explanations. For technical questions, explain in plain everyday language, not in system, no categorized, easy to understand with no code. No code, no complex syntax, no hard-to-read details. If the question isn't covered in a resume, ignore the resume and answer confidently as a senior. Add light humor when it fits naturally. Keep each response under about 1 minute of speaking time. When you understand and are ready to begin, just say: ready!\r\n" + 
                                 $"This is the job description I applied for:\n{jobDescriptionText}";

                await _chatGptService.StreamChatGptResponse(message, (partialResponse) =>
                {
                    string formattedResponse = partialResponse.Replace("\\n", Environment.NewLine);
                    // use RichTextBox append that preserves scroll
                    AppendTextPreserveScroll(textAnswer, formattedResponse);
                });
                StoreMessage("Interview Preparation", textAnswer.Text);
                AppendTextPreserveScroll(textAnswer, Environment.NewLine + Environment.NewLine);
            }
            else
            {
                textAnswer.Text = $"Error: {InterviewInputs.ResumeFileName} or {InterviewInputs.JobDescriptionFileName} is empty or unreadable. " +
                                  "Restart EasyGarry and paste both into the setup screen.";
            }
        }

        private void StoreMessage(string script, string fullMessage)
        {
            if (string.IsNullOrWhiteSpace(fullMessage)) return;

            messageHistory.Add(fullMessage);
            scriptHistory.Add(script);

            // Trim title for ListBox
            string shortTitle = script.Length > 22 ? script.Substring(0, 20) + "..." : script;
            messageTitles.Add(shortTitle);

            listHistory.Items.Add(shortTitle);
        }

        private void listBoxHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = listHistory.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < messageHistory.Count)
            {
                textAnswer.Text = messageHistory[selectedIndex];
                textScript.Text = scriptHistory[selectedIndex];
                _isViewingHistory = true;
            }
        }

        private void Interview_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_deepgramService.IsTranscribing)
                {
                    _deepgramService.StopTranscription();
                }
            }
            catch { }

            try
            {
                UnregisterHotKey(this.Handle, HOTKEY_ID);
            }
            catch { }

            Environment.Exit(0);
        }

        private async void HandleEnterKeyPress()
        {
            if (!string.IsNullOrWhiteSpace(textScript.Text))
            {
                string message = $"{textScript.Text}";
                textAnswer.Clear();
                // Hide scrollbars during streaming (RichTextBox uses different property)
                bool showScroll = true;
                try { showScroll = (textAnswer.ScrollBars != RichTextBoxScrollBars.None); } catch { }
                // Stream and append without changing visible viewport
                await _chatGptService.StreamChatGptResponse(message, (partialResponse) =>
                {
                    string formattedResponse = partialResponse.Replace("\\n", Environment.NewLine);
                    AppendTextPreserveScroll(textAnswer, formattedResponse);
                });
                StoreMessage(textScript.Text, textAnswer.Text);
                textScript.Clear();
                AppendTextPreserveScroll(textAnswer, Environment.NewLine + Environment.NewLine);
                // RichTextBox scrollbars handled automatically
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter) // Only trigger when textAnswer is focused
            {
                HandleEnterKeyPress();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
