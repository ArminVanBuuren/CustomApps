

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.CreateServ = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusBarLable = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusBarDesc = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.TabProcess = new System.Windows.Forms.TabPage();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.grid = new System.Windows.Forms.DataGridView();
            this.GridViewID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GridViewName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GridViewDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GridViewStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GridViewSetMode = new System.Windows.Forms.DataGridViewButtonColumn();
            this.TabConfig = new System.Windows.Forms.TabPage();
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
            // CreateServ
            // 
            this.CreateServ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CreateServ.Location = new System.Drawing.Point(652, 525);
            this.CreateServ.Name = "CreateServ";
            this.CreateServ.Size = new System.Drawing.Size(93, 23);
            this.CreateServ.TabIndex = 2;
            this.CreateServ.Text = "Create Service";
            this.CreateServ.UseVisualStyleBackColor = true;
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
            this.TabProcess.Controls.Add(this.progressBar1);
            this.TabProcess.Controls.Add(this.grid);
            this.TabProcess.Controls.Add(this.CreateServ);
            this.TabProcess.Location = new System.Drawing.Point(4, 22);
            this.TabProcess.Name = "TabProcess";
            this.TabProcess.Size = new System.Drawing.Size(748, 551);
            this.TabProcess.TabIndex = 2;
            this.TabProcess.Text = "Process";
            this.TabProcess.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(3, 525);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(643, 23);
            this.progressBar1.TabIndex = 4;
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AllowUserToResizeRows = false;
            this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grid.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GridViewID,
            this.GridViewName,
            this.GridViewDescription,
            this.GridViewStatus,
            this.GridViewSetMode});
            this.grid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.Name = "grid";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grid.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.grid.Size = new System.Drawing.Size(748, 519);
            this.grid.TabIndex = 0;
            this.grid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.grid_DataError);
            // 
            // GridViewID
            // 
            dataGridViewCellStyle1.Format = "N0";
            dataGridViewCellStyle1.NullValue = null;
            this.GridViewID.DefaultCellStyle = dataGridViewCellStyle1;
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
            // GridViewDescription
            // 
            this.GridViewDescription.HeaderText = "Description";
            this.GridViewDescription.Name = "GridViewDescription";
            // 
            // GridViewStatus
            // 
            this.GridViewStatus.HeaderText = "Status";
            this.GridViewStatus.Name = "GridViewStatus";
            // 
            // GridViewSetMode
            // 
            this.GridViewSetMode.HeaderText = "Set";
            this.GridViewSetMode.Name = "GridViewSetMode";
            this.GridViewSetMode.Width = 50;
            // 
            // TabConfig
            // 
            this.TabConfig.Controls.Add(this.SXML_Config);
            this.TabConfig.Location = new System.Drawing.Point(4, 22);
            this.TabConfig.Name = "TabConfig";
            this.TabConfig.Padding = new System.Windows.Forms.Padding(3);
            this.TabConfig.Size = new System.Drawing.Size(748, 551);
            this.TabConfig.TabIndex = 0;
            this.TabConfig.Text = "Config";
            this.TabConfig.UseVisualStyleBackColor = true;
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
            this.SXML_Config.AutoScrollMinSize = new System.Drawing.Size(0, 14);
            this.SXML_Config.BackBrush = null;
            this.SXML_Config.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SXML_Config.BracketsHighlightStrategy = FastColoredTextBoxNS.BracketsHighlightStrategy.Strategy2;
            this.SXML_Config.CharHeight = 14;
            this.SXML_Config.CharWidth = 7;
            this.SXML_Config.CommentPrefix = null;
            this.SXML_Config.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.SXML_Config.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.SXML_Config.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SXML_Config.Font = new System.Drawing.Font("Consolas", 9F);
            this.SXML_Config.IsReplaceMode = false;
            this.SXML_Config.Language = FastColoredTextBoxNS.Language.XML;
            this.SXML_Config.LeftBracket = '<';
            this.SXML_Config.LeftBracket2 = '(';
            this.SXML_Config.Location = new System.Drawing.Point(3, 3);
            this.SXML_Config.Name = "SXML_Config";
            this.SXML_Config.Paddings = new System.Windows.Forms.Padding(0);
            this.SXML_Config.RightBracket = '>';
            this.SXML_Config.RightBracket2 = ')';
            this.SXML_Config.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.SXML_Config.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("SXML_Config.ServiceColors")));
            this.SXML_Config.Size = new System.Drawing.Size(742, 545);
            this.SXML_Config.TabIndex = 3;
            this.SXML_Config.WordWrap = true;
            this.SXML_Config.Zoom = 100;
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
            this.LogTextBox.AutoScrollMinSize = new System.Drawing.Size(25, 14);
            this.LogTextBox.BackBrush = null;
            this.LogTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LogTextBox.CharHeight = 14;
            this.LogTextBox.CharWidth = 7;
            this.LogTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.LogTextBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.LogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogTextBox.Font = new System.Drawing.Font("Consolas", 9F);
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
        private System.Windows.Forms.Button CreateServ;
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
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridViewID;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridViewName;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridViewDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridViewStatus;
        private System.Windows.Forms.DataGridViewButtonColumn GridViewSetMode;
    }
}