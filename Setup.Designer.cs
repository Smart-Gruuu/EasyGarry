namespace EasyGarry
{
    partial class Setup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.labelHeader = new System.Windows.Forms.Label();
            this.labelHint = new System.Windows.Forms.Label();
            this.keysPanel = new System.Windows.Forms.Panel();
            this.keysLayout = new System.Windows.Forms.TableLayoutPanel();
            this.labelOpenAiKey = new System.Windows.Forms.Label();
            this.labelDeepgramKey = new System.Windows.Forms.Label();
            this.textOpenAiKey = new System.Windows.Forms.TextBox();
            this.textDeepgramKey = new System.Windows.Forms.TextBox();
            this.checkShowKeys = new System.Windows.Forms.CheckBox();
            this.keysSavedPanel = new System.Windows.Forms.Panel();
            this.labelKeysSaved = new System.Windows.Forms.Label();
            this.buttonChangeKeys = new System.Windows.Forms.Button();
            this.bodyLayout = new System.Windows.Forms.TableLayoutPanel();
            this.labelResume = new System.Windows.Forms.Label();
            this.labelJobDescription = new System.Windows.Forms.Label();
            this.textResume = new System.Windows.Forms.TextBox();
            this.textJobDescription = new System.Windows.Forms.TextBox();
            this.footerPanel = new System.Windows.Forms.Panel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonExit = new System.Windows.Forms.Button();
            this.rootLayout.SuspendLayout();
            this.keysPanel.SuspendLayout();
            this.keysLayout.SuspendLayout();
            this.keysSavedPanel.SuspendLayout();
            this.bodyLayout.SuspendLayout();
            this.footerPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // rootLayout
            //
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.labelHeader, 0, 0);
            this.rootLayout.Controls.Add(this.labelHint, 0, 1);
            this.rootLayout.Controls.Add(this.keysPanel, 0, 2);
            this.rootLayout.Controls.Add(this.bodyLayout, 0, 3);
            this.rootLayout.Controls.Add(this.footerPanel, 0, 4);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(0, 0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.Padding = new System.Windows.Forms.Padding(16);
            this.rootLayout.RowCount = 5;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.rootLayout.Size = new System.Drawing.Size(944, 724);
            this.rootLayout.TabIndex = 0;
            //
            // labelHeader
            //
            this.labelHeader.AutoSize = true;
            this.labelHeader.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold);
            this.labelHeader.Location = new System.Drawing.Point(19, 16);
            this.labelHeader.Margin = new System.Windows.Forms.Padding(3, 0, 3, 4);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Size = new System.Drawing.Size(163, 25);
            this.labelHeader.TabIndex = 0;
            this.labelHeader.Text = "Interview setup";
            //
            // labelHint
            //
            this.labelHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.labelHint.Location = new System.Drawing.Point(19, 45);
            this.labelHint.Margin = new System.Windows.Forms.Padding(3, 0, 3, 12);
            this.labelHint.Name = "labelHint";
            this.labelHint.Size = new System.Drawing.Size(906, 34);
            this.labelHint.TabIndex = 1;
            this.labelHint.Text = "EasyGarry runs on your own API keys. They are saved once, encrypted for your Windo" +
                "ws account. The resume and job description are saved next to the app as resume.tx" +
                "t and jobdescription.txt.";
            //
            // keysPanel
            //
            this.keysPanel.Controls.Add(this.keysLayout);
            this.keysPanel.Controls.Add(this.keysSavedPanel);
            this.keysPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.keysPanel.Location = new System.Drawing.Point(16, 91);
            this.keysPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.keysPanel.Name = "keysPanel";
            this.keysPanel.Size = new System.Drawing.Size(912, 84);
            this.keysPanel.TabIndex = 2;
            //
            // keysLayout
            //
            this.keysLayout.ColumnCount = 2;
            this.keysLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.keysLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.keysLayout.Controls.Add(this.labelOpenAiKey, 0, 0);
            this.keysLayout.Controls.Add(this.labelDeepgramKey, 1, 0);
            this.keysLayout.Controls.Add(this.textOpenAiKey, 0, 1);
            this.keysLayout.Controls.Add(this.textDeepgramKey, 1, 1);
            this.keysLayout.Controls.Add(this.checkShowKeys, 0, 2);
            this.keysLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.keysLayout.Location = new System.Drawing.Point(0, 0);
            this.keysLayout.Margin = new System.Windows.Forms.Padding(0);
            this.keysLayout.Name = "keysLayout";
            this.keysLayout.RowCount = 3;
            this.keysLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.keysLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.keysLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.keysLayout.Size = new System.Drawing.Size(912, 84);
            this.keysLayout.TabIndex = 0;
            //
            // labelOpenAiKey
            //
            this.labelOpenAiKey.AutoSize = true;
            this.labelOpenAiKey.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelOpenAiKey.Location = new System.Drawing.Point(0, 0);
            this.labelOpenAiKey.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
            this.labelOpenAiKey.Name = "labelOpenAiKey";
            this.labelOpenAiKey.Size = new System.Drawing.Size(110, 17);
            this.labelOpenAiKey.TabIndex = 0;
            this.labelOpenAiKey.Text = "OpenAI API key";
            //
            // labelDeepgramKey
            //
            this.labelDeepgramKey.AutoSize = true;
            this.labelDeepgramKey.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelDeepgramKey.Location = new System.Drawing.Point(464, 0);
            this.labelDeepgramKey.Margin = new System.Windows.Forms.Padding(8, 0, 0, 4);
            this.labelDeepgramKey.Name = "labelDeepgramKey";
            this.labelDeepgramKey.Size = new System.Drawing.Size(129, 17);
            this.labelDeepgramKey.TabIndex = 1;
            this.labelDeepgramKey.Text = "Deepgram API key";
            //
            // textOpenAiKey
            //
            this.textOpenAiKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textOpenAiKey.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textOpenAiKey.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textOpenAiKey.Location = new System.Drawing.Point(0, 21);
            this.textOpenAiKey.Margin = new System.Windows.Forms.Padding(0, 0, 8, 6);
            this.textOpenAiKey.Name = "textOpenAiKey";
            this.textOpenAiKey.Size = new System.Drawing.Size(448, 23);
            this.textOpenAiKey.TabIndex = 2;
            this.textOpenAiKey.UseSystemPasswordChar = true;
            //
            // textDeepgramKey
            //
            this.textDeepgramKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textDeepgramKey.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textDeepgramKey.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textDeepgramKey.Location = new System.Drawing.Point(464, 21);
            this.textDeepgramKey.Margin = new System.Windows.Forms.Padding(8, 0, 0, 6);
            this.textDeepgramKey.Name = "textDeepgramKey";
            this.textDeepgramKey.Size = new System.Drawing.Size(448, 23);
            this.textDeepgramKey.TabIndex = 3;
            this.textDeepgramKey.UseSystemPasswordChar = true;
            //
            // checkShowKeys
            //
            this.checkShowKeys.AutoSize = true;
            this.checkShowKeys.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.keysLayout.SetColumnSpan(this.checkShowKeys, 2);
            this.checkShowKeys.Location = new System.Drawing.Point(0, 50);
            this.checkShowKeys.Margin = new System.Windows.Forms.Padding(0);
            this.checkShowKeys.Name = "checkShowKeys";
            this.checkShowKeys.Size = new System.Drawing.Size(84, 19);
            this.checkShowKeys.TabIndex = 4;
            this.checkShowKeys.Text = "Show keys";
            this.checkShowKeys.UseVisualStyleBackColor = true;
            //
            // keysSavedPanel
            //
            this.keysSavedPanel.Controls.Add(this.labelKeysSaved);
            this.keysSavedPanel.Controls.Add(this.buttonChangeKeys);
            this.keysSavedPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.keysSavedPanel.Location = new System.Drawing.Point(0, 0);
            this.keysSavedPanel.Margin = new System.Windows.Forms.Padding(0);
            this.keysSavedPanel.Name = "keysSavedPanel";
            this.keysSavedPanel.Size = new System.Drawing.Size(912, 84);
            this.keysSavedPanel.TabIndex = 1;
            this.keysSavedPanel.Visible = false;
            //
            // labelKeysSaved
            //
            this.labelKeysSaved.AutoSize = true;
            this.labelKeysSaved.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(124)))), ((int)(((byte)(16)))));
            this.labelKeysSaved.Location = new System.Drawing.Point(0, 7);
            this.labelKeysSaved.Name = "labelKeysSaved";
            this.labelKeysSaved.Size = new System.Drawing.Size(268, 15);
            this.labelKeysSaved.TabIndex = 0;
            this.labelKeysSaved.Text = "API keys saved for this Windows account.";
            //
            // buttonChangeKeys
            //
            this.buttonChangeKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChangeKeys.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.buttonChangeKeys.FlatAppearance.BorderSize = 0;
            this.buttonChangeKeys.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonChangeKeys.Location = new System.Drawing.Point(772, 1);
            this.buttonChangeKeys.Name = "buttonChangeKeys";
            this.buttonChangeKeys.Size = new System.Drawing.Size(140, 28);
            this.buttonChangeKeys.TabIndex = 1;
            this.buttonChangeKeys.Text = "Change keys";
            this.buttonChangeKeys.UseVisualStyleBackColor = false;
            //
            // bodyLayout
            //
            this.bodyLayout.ColumnCount = 2;
            this.bodyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bodyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.bodyLayout.Controls.Add(this.labelResume, 0, 0);
            this.bodyLayout.Controls.Add(this.labelJobDescription, 1, 0);
            this.bodyLayout.Controls.Add(this.textResume, 0, 1);
            this.bodyLayout.Controls.Add(this.textJobDescription, 1, 1);
            this.bodyLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bodyLayout.Location = new System.Drawing.Point(16, 187);
            this.bodyLayout.Margin = new System.Windows.Forms.Padding(0);
            this.bodyLayout.Name = "bodyLayout";
            this.bodyLayout.RowCount = 2;
            this.bodyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.bodyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bodyLayout.Size = new System.Drawing.Size(912, 453);
            this.bodyLayout.TabIndex = 3;
            //
            // labelResume
            //
            this.labelResume.AutoSize = true;
            this.labelResume.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelResume.Location = new System.Drawing.Point(0, 0);
            this.labelResume.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
            this.labelResume.Name = "labelResume";
            this.labelResume.Size = new System.Drawing.Size(58, 17);
            this.labelResume.TabIndex = 0;
            this.labelResume.Text = "Resume";
            //
            // labelJobDescription
            //
            this.labelJobDescription.AutoSize = true;
            this.labelJobDescription.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelJobDescription.Location = new System.Drawing.Point(464, 0);
            this.labelJobDescription.Margin = new System.Windows.Forms.Padding(8, 0, 0, 4);
            this.labelJobDescription.Name = "labelJobDescription";
            this.labelJobDescription.Size = new System.Drawing.Size(107, 17);
            this.labelJobDescription.TabIndex = 1;
            this.labelJobDescription.Text = "Job description";
            //
            // textResume
            //
            this.textResume.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textResume.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textResume.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textResume.Location = new System.Drawing.Point(0, 21);
            this.textResume.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.textResume.MaxLength = 0;
            this.textResume.Multiline = true;
            this.textResume.Name = "textResume";
            this.textResume.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textResume.Size = new System.Drawing.Size(448, 432);
            this.textResume.TabIndex = 2;
            //
            // textJobDescription
            //
            this.textJobDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textJobDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textJobDescription.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textJobDescription.Location = new System.Drawing.Point(464, 21);
            this.textJobDescription.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.textJobDescription.MaxLength = 0;
            this.textJobDescription.Multiline = true;
            this.textJobDescription.Name = "textJobDescription";
            this.textJobDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textJobDescription.Size = new System.Drawing.Size(448, 432);
            this.textJobDescription.TabIndex = 3;
            //
            // footerPanel
            //
            this.footerPanel.Controls.Add(this.labelStatus);
            this.footerPanel.Controls.Add(this.buttonExit);
            this.footerPanel.Controls.Add(this.buttonStart);
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.footerPanel.Location = new System.Drawing.Point(16, 652);
            this.footerPanel.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(912, 56);
            this.footerPanel.TabIndex = 4;
            //
            // labelStatus
            //
            this.labelStatus.AutoSize = true;
            this.labelStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.labelStatus.Location = new System.Drawing.Point(0, 20);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(0, 15);
            this.labelStatus.TabIndex = 0;
            //
            // buttonStart
            //
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStart.FlatAppearance.BorderSize = 0;
            this.buttonStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStart.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.buttonStart.Location = new System.Drawing.Point(742, 9);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(170, 38);
            this.buttonStart.TabIndex = 2;
            this.buttonStart.Text = "Start interview";
            this.buttonStart.UseVisualStyleBackColor = false;
            //
            // buttonExit
            //
            this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.buttonExit.FlatAppearance.BorderSize = 0;
            this.buttonExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonExit.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.buttonExit.Location = new System.Drawing.Point(624, 9);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(110, 38);
            this.buttonExit.TabIndex = 1;
            this.buttonExit.Text = "Exit";
            this.buttonExit.UseVisualStyleBackColor = false;
            //
            // Setup
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(944, 724);
            this.Controls.Add(this.rootLayout);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.MinimumSize = new System.Drawing.Size(760, 600);
            this.Name = "Setup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EasyGarry - Interview Setup";
            this.rootLayout.ResumeLayout(false);
            this.rootLayout.PerformLayout();
            this.keysPanel.ResumeLayout(false);
            this.keysLayout.ResumeLayout(false);
            this.keysLayout.PerformLayout();
            this.keysSavedPanel.ResumeLayout(false);
            this.keysSavedPanel.PerformLayout();
            this.bodyLayout.ResumeLayout(false);
            this.bodyLayout.PerformLayout();
            this.footerPanel.ResumeLayout(false);
            this.footerPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Panel keysPanel;
        private System.Windows.Forms.TableLayoutPanel keysLayout;
        private System.Windows.Forms.Label labelOpenAiKey;
        private System.Windows.Forms.Label labelDeepgramKey;
        private System.Windows.Forms.TextBox textOpenAiKey;
        private System.Windows.Forms.TextBox textDeepgramKey;
        private System.Windows.Forms.CheckBox checkShowKeys;
        private System.Windows.Forms.Panel keysSavedPanel;
        private System.Windows.Forms.Label labelKeysSaved;
        private System.Windows.Forms.Button buttonChangeKeys;
        private System.Windows.Forms.TableLayoutPanel bodyLayout;
        private System.Windows.Forms.Label labelResume;
        private System.Windows.Forms.Label labelJobDescription;
        private System.Windows.Forms.TextBox textResume;
        private System.Windows.Forms.TextBox textJobDescription;
        private System.Windows.Forms.Panel footerPanel;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonExit;
    }
}
