

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.buttonStartStop = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusBarLable = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusBarDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.TabProcess = new System.Windows.Forms.TabPage();
            this.grid = new System.Windows.Forms.DataGridView();
            this.GridViewID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GridViewName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GridViewStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GridViewSetMode = new System.Windows.Forms.DataGridViewButtonColumn();
            this.TabConfig = new System.Windows.Forms.TabPage();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.SXML_Config = new FastColoredTextBoxNS.FastColoredTextBox();
            this.TabLog = new System.Windows.Forms.TabPage();
            this.LogTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.TabProcess.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.TabConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SXML_Config)).BeginInit();
            this.TabLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LogTextBox)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonStartStop
            // 
            this.buttonStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStartStop.Location = new System.Drawing.Point(667, 522);
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
            this.button2.Location = new System.Drawing.Point(586, 522);
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
            this.tabControl1.Controls.Add(this.TabProcess);
            this.tabControl1.Controls.Add(this.TabConfig);
            this.tabControl1.Controls.Add(this.TabLog);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(756, 577);
            this.tabControl1.TabIndex = 4;
            // 
            // TabProcess
            // 
            this.TabProcess.Controls.Add(this.grid);
            this.TabProcess.Location = new System.Drawing.Point(4, 22);
            this.TabProcess.Name = "TabProcess";
            this.TabProcess.Size = new System.Drawing.Size(748, 551);
            this.TabProcess.TabIndex = 2;
            this.TabProcess.Text = "Process";
            this.TabProcess.UseVisualStyleBackColor = true;
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AllowUserToResizeRows = false;
            this.grid.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GridViewID,
            this.GridViewName,
            this.GridViewStatus,
            this.GridViewSetMode});
            this.grid.Dock = System.Windows.Forms.DockStyle.Top;
            this.grid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.Name = "grid";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grid.RowHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.grid.Size = new System.Drawing.Size(748, 545);
            this.grid.TabIndex = 0;
            this.grid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.grid_DataError);
            // 
            // GridViewID
            // 
            this.GridViewID.HeaderText = "ID";
            this.GridViewID.Name = "GridViewID";
            this.GridViewID.Width = 50;
            // 
            // GridViewName
            // 
            this.GridViewName.HeaderText = "Configuration";
            this.GridViewName.Name = "GridViewName";
            this.GridViewName.Width = 120;
            // 
            // GridViewStatus
            // 
            this.GridViewStatus.HeaderText = "Status";
            this.GridViewStatus.Name = "GridViewStatus";
            // 
            // GridViewSetMode
            // 
            this.GridViewSetMode.HeaderText = "Set Mode";
            this.GridViewSetMode.Name = "GridViewSetMode";
            // 
            // TabConfig
            // 
            this.TabConfig.Controls.Add(this.progressBar1);
            this.TabConfig.Controls.Add(this.SXML_Config);
            this.TabConfig.Controls.Add(this.button2);
            this.TabConfig.Controls.Add(this.buttonStartStop);
            this.TabConfig.Location = new System.Drawing.Point(4, 22);
            this.TabConfig.Name = "TabConfig";
            this.TabConfig.Padding = new System.Windows.Forms.Padding(3);
            this.TabConfig.Size = new System.Drawing.Size(748, 551);
            this.TabConfig.TabIndex = 0;
            this.TabConfig.Text = "Config";
            this.TabConfig.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(7, 522);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(573, 23);
            this.progressBar1.TabIndex = 4;
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
            this.SXML_Config.AutoIndentCharsPatterns = "";
            this.SXML_Config.AutoScrollMinSize = new System.Drawing.Size(0, 16);
            this.SXML_Config.BackBrush = null;
            this.SXML_Config.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SXML_Config.BracketsHighlightStrategy = FastColoredTextBoxNS.BracketsHighlightStrategy.Strategy2;
            this.SXML_Config.CharHeight = 16;
            this.SXML_Config.CharWidth = 8;
            this.SXML_Config.CommentPrefix = null;
            this.SXML_Config.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.SXML_Config.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.SXML_Config.Font = new System.Drawing.Font("Consolas", 10.725F);
            this.SXML_Config.IsReplaceMode = false;
            this.SXML_Config.Language = FastColoredTextBoxNS.Language.XML;
            this.SXML_Config.LeftBracket = '<';
            this.SXML_Config.LeftBracket2 = '(';
            this.SXML_Config.Location = new System.Drawing.Point(0, 0);
            this.SXML_Config.Name = "SXML_Config";
            this.SXML_Config.Paddings = new System.Windows.Forms.Padding(0);
            this.SXML_Config.RightBracket = '>';
            this.SXML_Config.RightBracket2 = ')';
            this.SXML_Config.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.SXML_Config.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("SXML_Config.ServiceColors")));
            this.SXML_Config.Size = new System.Drawing.Size(748, 516);
            this.SXML_Config.TabIndex = 3;
            this.SXML_Config.WordWrap = true;
            this.SXML_Config.Zoom = 110;
            // 
            // TabLog
            // 
            this.TabLog.Controls.Add(this.LogTextBox);
            this.TabLog.Location = new System.Drawing.Point(4, 22);
            this.TabLog.Name = "TabLog";
            this.TabLog.Padding = new System.Windows.Forms.Padding(3);
            this.TabLog.Size = new System.Drawing.Size(748, 551);
            this.TabLog.TabIndex = 1;
            this.TabLog.Text = "Log";
            this.TabLog.UseVisualStyleBackColor = true;
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
            this.LogTextBox.AutoScrollMinSize = new System.Drawing.Size(2, 15);
            this.LogTextBox.BackBrush = null;
            this.LogTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LogTextBox.CharHeight = 15;
            this.LogTextBox.CharWidth = 7;
            this.LogTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.LogTextBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.LogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogTextBox.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.LogTextBox.IsReplaceMode = false;
            this.LogTextBox.Location = new System.Drawing.Point(3, 3);
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.Paddings = new System.Windows.Forms.Padding(0);
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.LogTextBox.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("LogTextBox.ServiceColors")));
            this.LogTextBox.Size = new System.Drawing.Size(742, 545);
            this.LogTextBox.TabIndex = 5;
            this.LogTextBox.Zoom = 100;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(780, 614);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "MainWindow";
            this.Text = "Script";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.TabProcess.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.TabConfig.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SXML_Config)).EndInit();
            this.TabLog.ResumeLayout(false);
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
        private System.Windows.Forms.TabPage TabProcess;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridColumnID;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridColumnStatus;
        private System.Windows.Forms.DataGridViewButtonColumn GridColumnSetMode;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridViewID;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridViewName;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridViewStatus;
        private System.Windows.Forms.DataGridViewButtonColumn GridViewSetMode;
        private System.Windows.Forms.DataGridView grid;
    }
}