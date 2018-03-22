namespace ASOTCutter
{
    partial class Form1
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
            this.textBoxDirPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ButtonDirPath = new System.Windows.Forms.Button();
            this.textBoxFormat = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ButtonStartStop = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tempstat = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusTextLable = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxDirPath
            // 
            this.textBoxDirPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDirPath.Location = new System.Drawing.Point(67, 12);
            this.textBoxDirPath.Name = "textBoxDirPath";
            this.textBoxDirPath.Size = new System.Drawing.Size(355, 20);
            this.textBoxDirPath.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Directory:";
            // 
            // ButtonDirPath
            // 
            this.ButtonDirPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonDirPath.Location = new System.Drawing.Point(428, 10);
            this.ButtonDirPath.Name = "ButtonDirPath";
            this.ButtonDirPath.Size = new System.Drawing.Size(75, 23);
            this.ButtonDirPath.TabIndex = 2;
            this.ButtonDirPath.Text = "Open";
            this.ButtonDirPath.UseVisualStyleBackColor = true;
            this.ButtonDirPath.Click += new System.EventHandler(this.ButtonDirPath_Click);
            // 
            // textBoxFormat
            // 
            this.textBoxFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFormat.Location = new System.Drawing.Point(67, 39);
            this.textBoxFormat.Name = "textBoxFormat";
            this.textBoxFormat.Size = new System.Drawing.Size(355, 20);
            this.textBoxFormat.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Format:";
            // 
            // ButtonStartStop
            // 
            this.ButtonStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonStartStop.Location = new System.Drawing.Point(429, 35);
            this.ButtonStartStop.Name = "ButtonStartStop";
            this.ButtonStartStop.Size = new System.Drawing.Size(75, 23);
            this.ButtonStartStop.TabIndex = 5;
            this.ButtonStartStop.Text = "Start";
            this.ButtonStartStop.UseVisualStyleBackColor = true;
            this.ButtonStartStop.Click += new System.EventHandler(this.ButtonStartStop_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tempstat,
            this.StatusTextLable});
            this.statusStrip1.Location = new System.Drawing.Point(0, 98);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(515, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tempstat
            // 
            this.tempstat.Name = "tempstat";
            this.tempstat.Size = new System.Drawing.Size(42, 17);
            this.tempstat.Text = "Status:";
            // 
            // StatusTextLable
            // 
            this.StatusTextLable.Name = "StatusTextLable";
            this.StatusTextLable.Size = new System.Drawing.Size(0, 17);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(515, 120);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.ButtonStartStop);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxFormat);
            this.Controls.Add(this.ButtonDirPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxDirPath);
            this.Name = "Form1";
            this.Text = "ASOT Cutter";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxDirPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ButtonDirPath;
        private System.Windows.Forms.TextBox textBoxFormat;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ButtonStartStop;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tempstat;
        private System.Windows.Forms.ToolStripStatusLabel StatusTextLable;
    }
}

