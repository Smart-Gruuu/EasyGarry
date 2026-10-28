using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyGarry
{
    /// <summary>
    /// Pre-session screen: supply the API keys once, paste the resume and job description,
    /// then hand off to the main window. Nothing can start until all four are present.
    /// </summary>
    public partial class Setup : Form
    {
        private static readonly Color AccentColor = Color.FromArgb(0, 120, 215);
        private static readonly Color AccentDisabledColor = Color.FromArgb(205, 205, 205);
        private static readonly Color HintColor = Color.FromArgb(100, 100, 100);
        private static readonly Color WarningColor = Color.FromArgb(176, 88, 0);

        private const int KeysPanelExpandedHeight = 84;
        private const int KeysPanelCollapsedHeight = 30;

        public Setup()
        {
            InitializeComponent();
            ApplyAppIcon();
            LoadSavedKeys();
            LoadExistingInputs();

            textResume.TextChanged += (s, e) => UpdateReadyState();
            textJobDescription.TextChanged += (s, e) => UpdateReadyState();
            textOpenAiKey.TextChanged += (s, e) => UpdateReadyState();
            textDeepgramKey.TextChanged += (s, e) => UpdateReadyState();
            checkShowKeys.CheckedChanged += CheckShowKeys_CheckedChanged;
            buttonChangeKeys.Click += ButtonChangeKeys_Click;
            buttonStart.Click += ButtonStart_Click;
            buttonExit.Click += ButtonExit_Click;

            UpdateReadyState();
        }

        /// <summary>
        /// Takes the icon from the executable itself so this screen needs no asset file on disk.
        /// </summary>
        private void ApplyAppIcon()
        {
            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                // No icon is better than failing to open the setup screen.
            }
        }

        /// <summary>
        /// First run shows the key fields; afterwards they collapse to a one-line
        /// confirmation with a Change button, so the keys are only ever pasted once.
        /// </summary>
        private void LoadSavedKeys()
        {
            var saved = ApiKeyStore.Load();
            if (saved == null)
            {
                ShowKeyEditor(true);
                return;
            }

            textOpenAiKey.Text = saved.OpenAi;
            textDeepgramKey.Text = saved.Deepgram;
            ShowKeyEditor(false);
        }

        private void ShowKeyEditor(bool editing)
        {
            keysLayout.Visible = editing;
            keysSavedPanel.Visible = !editing;
            keysPanel.Height = editing ? KeysPanelExpandedHeight : KeysPanelCollapsedHeight;
        }

        /// <summary>
        /// Pre-fills both boxes from the existing files so a repeat run is a single click.
        /// </summary>
        private void LoadExistingInputs()
        {
            textResume.Text = InterviewInputs.ReadOrEmpty(InterviewInputs.ResumePath);
            textJobDescription.Text = InterviewInputs.ReadOrEmpty(InterviewInputs.JobDescriptionPath);
        }

        private void CheckShowKeys_CheckedChanged(object sender, EventArgs e)
        {
            textOpenAiKey.UseSystemPasswordChar = !checkShowKeys.Checked;
            textDeepgramKey.UseSystemPasswordChar = !checkShowKeys.Checked;
        }

        private void ButtonChangeKeys_Click(object sender, EventArgs e)
        {
            ShowKeyEditor(true);
            textOpenAiKey.Focus();
            textOpenAiKey.SelectAll();
        }

        private void UpdateReadyState()
        {
            bool hasOpenAiKey = !string.IsNullOrWhiteSpace(textOpenAiKey.Text);
            bool hasDeepgramKey = !string.IsNullOrWhiteSpace(textDeepgramKey.Text);
            bool hasResume = !string.IsNullOrWhiteSpace(textResume.Text);
            bool hasJobDescription = !string.IsNullOrWhiteSpace(textJobDescription.Text);
            bool isReady = hasOpenAiKey && hasDeepgramKey && hasResume && hasJobDescription;

            buttonStart.Enabled = isReady;
            buttonStart.BackColor = isReady ? AccentColor : AccentDisabledColor;
            buttonStart.ForeColor = isReady ? Color.White : Color.FromArgb(120, 120, 120);

            if (isReady)
            {
                labelStatus.ForeColor = HintColor;
                labelStatus.Text = string.Format(
                    "Resume: {0:N0} characters  |  Job description: {1:N0} characters",
                    textResume.Text.Length,
                    textJobDescription.Text.Length);
                return;
            }

            labelStatus.ForeColor = WarningColor;
            if (!hasOpenAiKey && !hasDeepgramKey)
                labelStatus.Text = "Enter your OpenAI and Deepgram API keys to continue.";
            else if (!hasOpenAiKey)
                labelStatus.Text = "Enter your OpenAI API key to continue.";
            else if (!hasDeepgramKey)
                labelStatus.Text = "Enter your Deepgram API key to continue.";
            else if (!hasResume && !hasJobDescription)
                labelStatus.Text = "Paste your resume and the job description to continue.";
            else if (!hasResume)
                labelStatus.Text = "Paste your resume to continue.";
            else
                labelStatus.Text = "Paste the job description to continue.";
        }

        /// <summary>
        /// A TextBox selects all of its text when it first receives focus. With the boxes
        /// pre-filled that puts the whole resume one keystroke away from being wiped, so
        /// drop the selection and park both boxes at the top once the form is up.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (keysLayout.Visible)
                textOpenAiKey.Focus();
            else
                textResume.Focus();

            foreach (var box in new[] { textResume, textJobDescription })
            {
                box.SelectionStart = 0;
                box.SelectionLength = 0;
            }
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            if (!TrySaveKeys())
                return;

            if (!TrySaveInputs())
                return;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool TrySaveKeys()
        {
            var keys = new ApiKeys
            {
                OpenAi = textOpenAiKey.Text.Trim(),
                Deepgram = textDeepgramKey.Text.Trim()
            };

            if (!keys.IsComplete)
            {
                ShowKeyEditor(true);
                UpdateReadyState();
                return false;
            }

            try
            {
                ApiKeyStore.Save(keys);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Could not save the API keys to {0}\r\n\r\n{1}", ApiKeyStore.StorageDirectory, ex.Message),
                    "Save failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private bool TrySaveInputs()
        {
            string resume = textResume.Text.Trim();
            string jobDescription = textJobDescription.Text.Trim();

            if (string.IsNullOrEmpty(resume) || string.IsNullOrEmpty(jobDescription))
            {
                UpdateReadyState();
                return false;
            }

            try
            {
                InterviewInputs.Write(InterviewInputs.ResumePath, resume);
                InterviewInputs.Write(InterviewInputs.JobDescriptionPath, jobDescription);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Could not save to {0}\r\n\r\n{1}", InterviewInputs.BaseDirectory, ex.Message),
                    "Save failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Enter belongs to the multiline boxes, so Ctrl+Enter is the keyboard shortcut to start.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Enter))
            {
                if (buttonStart.Enabled)
                    ButtonStart_Click(buttonStart, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
