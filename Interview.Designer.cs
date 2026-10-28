namespace EasyGarry
{
    partial class Interview
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Interview));
            this.listHistory = new System.Windows.Forms.ListBox();
            this.textAnswer = new EasyGarry.Interview.NoScrollRichTextBox();
            this.textScript = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // listHistory
            // 
            this.listHistory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listHistory.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listHistory.FormattingEnabled = true;
            this.listHistory.IntegralHeight = false;
            this.listHistory.ItemHeight = 19;
            this.listHistory.Location = new System.Drawing.Point(608, 52);
            this.listHistory.Name = "listHistory";
            this.listHistory.Size = new System.Drawing.Size(180, 537);
            this.listHistory.TabIndex = 1;
            this.listHistory.SelectedIndexChanged += new System.EventHandler(this.listBoxHistory_SelectedIndexChanged);
            // 
            // textAnswer
            // 
            this.textAnswer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textAnswer.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAnswer.Location = new System.Drawing.Point(12, 52);
            this.textAnswer.Name = "textAnswer";
            this.textAnswer.ReadOnly = true;
            this.textAnswer.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.textAnswer.Size = new System.Drawing.Size(590, 445);
            this.textAnswer.TabIndex = 0;
            this.textAnswer.Text = "";
            // 
            // textScript
            // 
            this.textScript.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textScript.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textScript.Location = new System.Drawing.Point(12, 503);
            this.textScript.Multiline = true;
            this.textScript.Name = "textScript";
            this.textScript.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textScript.Size = new System.Drawing.Size(590, 86);
            this.textScript.TabIndex = 2;
            // 
            // Interview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 601);
            this.Controls.Add(this.textScript);
            this.Controls.Add(this.listHistory);
            this.Controls.Add(this.textAnswer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Interview";
            this.Opacity = 0.9D;
            this.Text = "EasyGarry";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox listHistory;
        private System.Windows.Forms.TextBox textScript;
        private NoScrollRichTextBox textAnswer;
    }
}

