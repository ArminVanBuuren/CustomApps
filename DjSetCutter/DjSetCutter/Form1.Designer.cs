namespace DjSetCutter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.textBoxDirPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ButtonDirPath = new System.Windows.Forms.Button();
            this.textBoxFormat = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ButtonStartStop = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tempstat = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusTextLable = new System.Windows.Forms.ToolStripStatusLabel();
            this.exceptionMessage = new System.Windows.Forms.RichTextBox();
            this.deleteSourceCUE = new System.Windows.Forms.CheckBox();
            this.deleteSourceMP3 = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxDirPath
            // 
            this.textBoxDirPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDirPath.Location = new System.Drawing.Point(83, 100);
            this.textBoxDirPath.Name = "textBoxDirPath";
            this.textBoxDirPath.Size = new System.Drawing.Size(892, 20);
            this.textBoxDirPath.TabIndex = 0;
            this.textBoxDirPath.Text = "D:\\TEST";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 103);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Source:";
            // 
            // ButtonDirPath
            // 
            this.ButtonDirPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonDirPath.Location = new System.Drawing.Point(981, 98);
            this.ButtonDirPath.Name = "ButtonDirPath";
            this.ButtonDirPath.Size = new System.Drawing.Size(45, 23);
            this.ButtonDirPath.TabIndex = 2;
            this.ButtonDirPath.Text = "Open";
            this.ButtonDirPath.UseVisualStyleBackColor = true;
            this.ButtonDirPath.Click += new System.EventHandler(this.ButtonDirPath_Click);
            // 
            // textBoxFormat
            // 
            this.textBoxFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFormat.Location = new System.Drawing.Point(83, 126);
            this.textBoxFormat.Name = "textBoxFormat";
            this.textBoxFormat.Size = new System.Drawing.Size(892, 20);
            this.textBoxFormat.TabIndex = 3;
            this.textBoxFormat.Tag = "";
            this.textBoxFormat.Text = "..\\%DIR_NAME%\\[ASOT %EPISODE%] %TRACK%. %PERFORMER% - %TITLE%.mp3";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 129);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Destination:";
            // 
            // ButtonStartStop
            // 
            this.ButtonStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonStartStop.Location = new System.Drawing.Point(936, 310);
            this.ButtonStartStop.Name = "ButtonStartStop";
            this.ButtonStartStop.Size = new System.Drawing.Size(90, 23);
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
            this.statusStrip1.Location = new System.Drawing.Point(0, 343);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1038, 22);
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
            // exceptionMessage
            // 
            this.exceptionMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.exceptionMessage.BackColor = System.Drawing.SystemColors.ControlLight;
            this.exceptionMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.exceptionMessage.ForeColor = System.Drawing.Color.Red;
            this.exceptionMessage.Location = new System.Drawing.Point(15, 178);
            this.exceptionMessage.Name = "exceptionMessage";
            this.exceptionMessage.ReadOnly = true;
            this.exceptionMessage.Size = new System.Drawing.Size(1011, 127);
            this.exceptionMessage.TabIndex = 7;
            this.exceptionMessage.Text = "";
            // 
            // deleteSourceCUE
            // 
            this.deleteSourceCUE.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.deleteSourceCUE.AutoSize = true;
            this.deleteSourceCUE.Location = new System.Drawing.Point(12, 314);
            this.deleteSourceCUE.Name = "deleteSourceCUE";
            this.deleteSourceCUE.Size = new System.Drawing.Size(126, 17);
            this.deleteSourceCUE.TabIndex = 8;
            this.deleteSourceCUE.Text = "Delete source CUE ?";
            this.deleteSourceCUE.UseVisualStyleBackColor = true;
            // 
            // deleteSourceMP3
            // 
            this.deleteSourceMP3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.deleteSourceMP3.AutoSize = true;
            this.deleteSourceMP3.Location = new System.Drawing.Point(144, 314);
            this.deleteSourceMP3.Name = "deleteSourceMP3";
            this.deleteSourceMP3.Size = new System.Drawing.Size(126, 17);
            this.deleteSourceMP3.TabIndex = 14;
            this.deleteSourceMP3.Text = "Delete source MP3 ?";
            this.deleteSourceMP3.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Separator:";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(68, 19);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(892, 20);
            this.textBox1.TabIndex = 16;
            this.textBox1.Tag = "";
            this.textBox1.Text = "TRACK.+?INDEX\\s*[0-9 :]+";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBox2);
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(15, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1011, 80);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Track\'s regex parser from CUE";
            // 
            // checkBox2
            // 
            this.checkBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(966, 48);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(35, 17);
            this.checkBox2.TabIndex = 21;
            this.checkBox2.Text = "M";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.Location = new System.Drawing.Point(68, 45);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(892, 20);
            this.textBox2.TabIndex = 20;
            this.textBox2.Tag = "";
            this.textBox2.Text = "TRACK\\s*(?<TRACK>\\d+).+?PERFORMER\\s*(\\\"\"|)(?<PERFORMER>.+?)(\\\"\"|)TITLE\\s*(\\\"\"|)(?" +
    "<TITLE>.+?)(\\\"\"|)INDEX\\s*01\\s*(?<INDEX>\\d+:\\d+:\\d+)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "Track Info:";
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(966, 22);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(35, 17);
            this.checkBox1.TabIndex = 17;
            this.checkBox1.Text = "M";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 155);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "Dir Parser:";
            // 
            // textBox3
            // 
            this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox3.Location = new System.Drawing.Point(83, 152);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(892, 20);
            this.textBox3.TabIndex = 18;
            this.textBox3.Tag = "";
            this.textBox3.Text = "(?<DIR_NAME>(?<EPISODE>\\d+)\\s*\\(\\d+\\-\\d+\\-\\d+\\))";
            // 
            // checkBox3
            // 
            this.checkBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(981, 155);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(35, 17);
            this.checkBox3.TabIndex = 23;
            this.checkBox3.Text = "M";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1038, 365);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBoxFormat);
            this.Controls.Add(this.deleteSourceMP3);
            this.Controls.Add(this.deleteSourceCUE);
            this.Controls.Add(this.exceptionMessage);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.ButtonStartStop);
            this.Controls.Add(this.ButtonDirPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxDirPath);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(390, 214);
            this.Name = "Form1";
            this.Text = "Dj Set Cutter";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.RichTextBox exceptionMessage;
        private System.Windows.Forms.CheckBox deleteSourceCUE;
        private System.Windows.Forms.CheckBox deleteSourceMP3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.CheckBox checkBox3;
    }
}

