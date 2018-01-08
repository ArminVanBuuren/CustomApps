namespace Script
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;



        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.buttonStartStop = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusBarLable = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusBarDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.TabConfig = new System.Windows.Forms.TabPage();
            this.TabLog = new System.Windows.Forms.TabPage();
            this.SXML_Config = new FastColoredTextBoxNS.FastColoredTextBox();
            this.LogTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.TabConfig.SuspendLayout();
            this.TabLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SXML_Config)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogTextBox)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonStartStop
            // 
            this.buttonStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStartStop.Location = new System.Drawing.Point(693, 566);
            this.buttonStartStop.Name = "buttonStartStop";
            this.buttonStartStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStartStop.TabIndex = 0;
            this.buttonStartStop.Text = "Start";
            this.buttonStartStop.UseVisualStyleBackColor = true;
            this.buttonStartStop.Click += new System.EventHandler(this.Start_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(612, 566);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Open";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Open_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusBarLable,
            this.StatusBarDesc});
            this.statusStrip1.Location = new System.Drawing.Point(0, 592);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(780, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // StatusBarLable
            // 
            this.StatusBarLable.Name = "StatusBarLable";
            this.StatusBarLable.Size = new System.Drawing.Size(0, 17);
            // 
            // StatusBarDesc
            // 
            this.StatusBarDesc.Name = "StatusBarDesc";
            this.StatusBarDesc.Size = new System.Drawing.Size(0, 17);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.TabConfig);
            this.tabControl1.Controls.Add(this.TabLog);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(756, 548);
            this.tabControl1.TabIndex = 4;
            // 
            // TabConfig
            // 
            this.TabConfig.Controls.Add(this.SXML_Config);
            this.TabConfig.Location = new System.Drawing.Point(4, 22);
            this.TabConfig.Name = "TabConfig";
            this.TabConfig.Padding = new System.Windows.Forms.Padding(3);
            this.TabConfig.Size = new System.Drawing.Size(748, 522);
            this.TabConfig.TabIndex = 0;
            this.TabConfig.Text = "Config";
            this.TabConfig.UseVisualStyleBackColor = true;
            // 
            // TabLog
            // 
            this.TabLog.Controls.Add(this.LogTextBox);
            this.TabLog.Location = new System.Drawing.Point(4, 22);
            this.TabLog.Name = "TabLog";
            this.TabLog.Padding = new System.Windows.Forms.Padding(3);
            this.TabLog.Size = new System.Drawing.Size(748, 522);
            this.TabLog.TabIndex = 1;
            this.TabLog.Text = "Log";
            this.TabLog.UseVisualStyleBackColor = true;
            // 
            // SXML_Config
            // 
            this.SXML_Config.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.SXML_Config.AutoScrollMinSize = new System.Drawing.Size(0, 14);
            this.SXML_Config.BackBrush = null;
            this.SXML_Config.CharHeight = 14;
            this.SXML_Config.CharWidth = 8;
            this.SXML_Config.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.SXML_Config.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.SXML_Config.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.SXML_Config.IsReplaceMode = false;
            this.SXML_Config.Location = new System.Drawing.Point(0, 0);
            this.SXML_Config.Name = "SXML_Config";
            this.SXML_Config.Paddings = new System.Windows.Forms.Padding(0);
            this.SXML_Config.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.SXML_Config.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("SXML_Config.ServiceColors")));
            this.SXML_Config.Size = new System.Drawing.Size(745, 522);
            this.SXML_Config.TabIndex = 2;
            this.SXML_Config.WordWrap = true;
            this.SXML_Config.Zoom = 100;
            // 
            // LogTextBox
            // 
            this.LogTextBox.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.LogTextBox.AutoScrollMinSize = new System.Drawing.Size(27, 14);
            this.LogTextBox.BackBrush = null;
            this.LogTextBox.CharHeight = 14;
            this.LogTextBox.CharWidth = 8;
            this.LogTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.LogTextBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.LogTextBox.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.LogTextBox.IsReplaceMode = false;
            this.LogTextBox.Location = new System.Drawing.Point(0, 0);
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.Paddings = new System.Windows.Forms.Padding(0);
            this.LogTextBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.LogTextBox.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("LogTextBox.ServiceColors")));
            this.LogTextBox.Size = new System.Drawing.Size(748, 522);
            this.LogTextBox.TabIndex = 0;
            this.LogTextBox.Zoom = 100;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(780, 614);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.buttonStartStop);
            this.Name = "MainWindow";
            this.Text = "Script";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.TabConfig.ResumeLayout(false);
            this.TabLog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SXML_Config)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogTextBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStartStop;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusBarLable;
        private System.Windows.Forms.ToolStripStatusLabel StatusBarDesc;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage TabConfig;
        private System.Windows.Forms.TabPage TabLog;
        private FastColoredTextBoxNS.FastColoredTextBox SXML_Config;
        private FastColoredTextBoxNS.FastColoredTextBox LogTextBox;
    }
}

