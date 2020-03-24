using FastColoredTextBoxNS;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using LogsReader.Properties;

namespace LogsReader
{
    sealed partial class LogsReaderForm
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
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Servers");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Types");
            this.dgvFiles = new System.Windows.Forms.DataGridView();
            this.PrivateID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsMatched = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Server = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Trace = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.txtPattern = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.ParentSplitContainer = new System.Windows.Forms.SplitContainer();
            this.SchemePanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.trvMain = new LogsReader.TreeViewImproved();
            this.maxLinesStackText = new System.Windows.Forms.TextBox();
            this.serversText = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.logDirText = new System.Windows.Forms.TextBox();
            this.fileNames = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.maxThreadsText = new System.Windows.Forms.TextBox();
            this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.EnumSplitContainer = new System.Windows.Forms.SplitContainer();
            this.descriptionText = new System.Windows.Forms.RichTextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusTextLable = new System.Windows.Forms.ToolStripStatusLabel();
            this.useRegex = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.traceLikeText = new System.Windows.Forms.TextBox();
            this.dateTimePickerEnd = new System.Windows.Forms.DateTimePicker();
            this.label10 = new System.Windows.Forms.Label();
            this.dateTimePickerStart = new System.Windows.Forms.DateTimePicker();
            this.traceNotLikeText = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonFilter = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.msgFilterText = new System.Windows.Forms.TextBox();
            this.groupBoxFilter = new System.Windows.Forms.GroupBox();
            this.filterPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ParentSplitContainer)).BeginInit();
            this.ParentSplitContainer.Panel1.SuspendLayout();
            this.ParentSplitContainer.Panel2.SuspendLayout();
            this.ParentSplitContainer.SuspendLayout();
            this.SchemePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EnumSplitContainer)).BeginInit();
            this.EnumSplitContainer.Panel1.SuspendLayout();
            this.EnumSplitContainer.Panel2.SuspendLayout();
            this.EnumSplitContainer.SuspendLayout();
            this.groupBoxFilter.SuspendLayout();
            this.filterPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvFiles
            // 
            this.dgvFiles.AllowUserToAddRows = false;
            this.dgvFiles.AllowUserToDeleteRows = false;
            this.dgvFiles.AllowUserToOrderColumns = true;
            this.dgvFiles.AllowUserToResizeRows = false;
            this.dgvFiles.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PrivateID,
            this.IsMatched,
            this.ID,
            this.Server,
            this.Trace,
            this.Date,
            this.FileName});
            this.dgvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvFiles.GridColor = System.Drawing.SystemColors.ControlLight;
            this.dgvFiles.Location = new System.Drawing.Point(0, 0);
            this.dgvFiles.MultiSelect = false;
            this.dgvFiles.Name = "dgvFiles";
            this.dgvFiles.ReadOnly = true;
            this.dgvFiles.RowHeadersVisible = false;
            this.dgvFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvFiles.Size = new System.Drawing.Size(525, 326);
            this.dgvFiles.TabIndex = 1;
            this.dgvFiles.SelectionChanged += new System.EventHandler(this.dgvFiles_SelectionChanged);
            this.dgvFiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dgvFiles_MouseDown);
            // 
            // PrivateID
            // 
            this.PrivateID.DataPropertyName = "PrivateID";
            this.PrivateID.HeaderText = "PrivateID";
            this.PrivateID.MinimumWidth = 10;
            this.PrivateID.Name = "PrivateID";
            this.PrivateID.ReadOnly = true;
            this.PrivateID.Visible = false;
            this.PrivateID.Width = 10;
            // 
            // IsMatched
            // 
            this.IsMatched.DataPropertyName = "IsMatched";
            this.IsMatched.HeaderText = "IsMatched";
            this.IsMatched.MinimumWidth = 10;
            this.IsMatched.Name = "IsMatched";
            this.IsMatched.ReadOnly = true;
            this.IsMatched.Visible = false;
            this.IsMatched.Width = 10;
            // 
            // ID
            // 
            this.ID.DataPropertyName = "ID";
            this.ID.HeaderText = "ID";
            this.ID.MinimumWidth = 40;
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            this.ID.Width = 45;
            // 
            // Server
            // 
            this.Server.DataPropertyName = "Server";
            this.Server.HeaderText = "Server";
            this.Server.MinimumWidth = 40;
            this.Server.Name = "Server";
            this.Server.ReadOnly = true;
            this.Server.Width = 80;
            // 
            // Trace
            // 
            this.Trace.DataPropertyName = "Trace";
            this.Trace.HeaderText = "Trace";
            this.Trace.MinimumWidth = 40;
            this.Trace.Name = "Trace";
            this.Trace.ReadOnly = true;
            this.Trace.Width = 140;
            // 
            // Date
            // 
            this.Date.DataPropertyName = "Date";
            this.Date.HeaderText = "Date";
            this.Date.MinimumWidth = 40;
            this.Date.Name = "Date";
            this.Date.ReadOnly = true;
            this.Date.Width = 140;
            // 
            // FileName
            // 
            this.FileName.DataPropertyName = "FileName";
            this.FileName.HeaderText = "FileName";
            this.FileName.MinimumWidth = 40;
            this.FileName.Name = "FileName";
            this.FileName.ReadOnly = true;
            this.FileName.Width = 500;
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Image = global::LogsReader.Properties.Resources.Search;
            this.btnSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSearch.Location = new System.Drawing.Point(747, 2);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(91, 24);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "      Search [F5]";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.ButtonStartStop_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Image = global::LogsReader.Properties.Resources.Symbol_Refresh_3;
            this.btnClear.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClear.Location = new System.Drawing.Point(844, 2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(87, 24);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "      Clear [F6]";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // txtPattern
            // 
            this.txtPattern.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPattern.Location = new System.Drawing.Point(3, 3);
            this.txtPattern.Name = "txtPattern";
            this.txtPattern.Size = new System.Drawing.Size(653, 23);
            this.txtPattern.TabIndex = 4;
            this.txtPattern.TextChanged += new System.EventHandler(this.txtPattern_TextChanged);
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(0, 498);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(940, 10);
            this.progressBar.TabIndex = 6;
            // 
            // ParentSplitContainer
            // 
            this.ParentSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ParentSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ParentSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.ParentSplitContainer.Location = new System.Drawing.Point(0, 95);
            this.ParentSplitContainer.Name = "ParentSplitContainer";
            // 
            // ParentSplitContainer.Panel1
            // 
            this.ParentSplitContainer.Panel1.Controls.Add(this.SchemePanel);
            // 
            // ParentSplitContainer.Panel2
            // 
            this.ParentSplitContainer.Panel2.Controls.Add(this.MainSplitContainer);
            this.ParentSplitContainer.Size = new System.Drawing.Size(940, 403);
            this.ParentSplitContainer.SplitterDistance = 175;
            this.ParentSplitContainer.TabIndex = 31;
            // 
            // SchemePanel
            // 
            this.SchemePanel.Controls.Add(this.label1);
            this.SchemePanel.Controls.Add(this.trvMain);
            this.SchemePanel.Controls.Add(this.maxLinesStackText);
            this.SchemePanel.Controls.Add(this.serversText);
            this.SchemePanel.Controls.Add(this.label6);
            this.SchemePanel.Controls.Add(this.label3);
            this.SchemePanel.Controls.Add(this.logDirText);
            this.SchemePanel.Controls.Add(this.fileNames);
            this.SchemePanel.Controls.Add(this.label5);
            this.SchemePanel.Controls.Add(this.label4);
            this.SchemePanel.Controls.Add(this.maxThreadsText);
            this.SchemePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SchemePanel.Location = new System.Drawing.Point(0, 0);
            this.SchemePanel.Name = "SchemePanel";
            this.SchemePanel.Size = new System.Drawing.Size(171, 399);
            this.SchemePanel.TabIndex = 15;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Servers:";
            // 
            // trvMain
            // 
            this.trvMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trvMain.CheckBoxes = true;
            this.trvMain.Location = new System.Drawing.Point(2, 139);
            this.trvMain.Name = "trvMain";
            treeNode7.Name = "trvServers";
            treeNode7.Text = "Servers";
            treeNode8.Name = "trvTypes";
            treeNode8.Text = "Types";
            this.trvMain.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode7,
            treeNode8});
            this.trvMain.Size = new System.Drawing.Size(166, 260);
            this.trvMain.TabIndex = 2;
            this.trvMain.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.trvMain_AfterCheck);
            // 
            // maxLinesStackText
            // 
            this.maxLinesStackText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.maxLinesStackText.Location = new System.Drawing.Point(85, 84);
            this.maxLinesStackText.Name = "maxLinesStackText";
            this.maxLinesStackText.Size = new System.Drawing.Size(83, 23);
            this.maxLinesStackText.TabIndex = 14;
            this.maxLinesStackText.TextChanged += new System.EventHandler(this.maxLinesStackText_TextChanged);
            this.maxLinesStackText.Leave += new System.EventHandler(this.maxLinesStackText_Leave);
            // 
            // serversText
            // 
            this.serversText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serversText.Location = new System.Drawing.Point(85, 6);
            this.serversText.Name = "serversText";
            this.serversText.Size = new System.Drawing.Size(83, 23);
            this.serversText.TabIndex = 4;
            this.serversText.TextChanged += new System.EventHandler(this.serversText_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 87);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 15);
            this.label6.TabIndex = 13;
            this.label6.Text = "Max Lines:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "File Types:";
            // 
            // logDirText
            // 
            this.logDirText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logDirText.Location = new System.Drawing.Point(85, 32);
            this.logDirText.Name = "logDirText";
            this.logDirText.Size = new System.Drawing.Size(83, 23);
            this.logDirText.TabIndex = 12;
            this.logDirText.TextChanged += new System.EventHandler(this.logDirText_TextChanged);
            // 
            // fileNames
            // 
            this.fileNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileNames.Location = new System.Drawing.Point(85, 58);
            this.fileNames.Name = "fileNames";
            this.fileNames.Size = new System.Drawing.Size(83, 23);
            this.fileNames.TabIndex = 8;
            this.fileNames.TextChanged += new System.EventHandler(this.typesText_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 36);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 15);
            this.label5.TabIndex = 11;
            this.label5.Text = "Logs folder:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 113);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 15);
            this.label4.TabIndex = 9;
            this.label4.Text = "Max Threads:";
            // 
            // maxThreadsText
            // 
            this.maxThreadsText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.maxThreadsText.Location = new System.Drawing.Point(85, 110);
            this.maxThreadsText.Name = "maxThreadsText";
            this.maxThreadsText.Size = new System.Drawing.Size(83, 23);
            this.maxThreadsText.TabIndex = 10;
            this.maxThreadsText.TextChanged += new System.EventHandler(this.maxThreadsText_TextChanged);
            this.maxThreadsText.Leave += new System.EventHandler(this.maxThreadsText_Leave);
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainSplitContainer.Name = "MainSplitContainer";
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.Controls.Add(this.EnumSplitContainer);
            this.MainSplitContainer.Size = new System.Drawing.Size(761, 403);
            this.MainSplitContainer.SplitterDistance = 529;
            this.MainSplitContainer.TabIndex = 0;
            // 
            // EnumSplitContainer
            // 
            this.EnumSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EnumSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.EnumSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.EnumSplitContainer.Name = "EnumSplitContainer";
            this.EnumSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // EnumSplitContainer.Panel1
            // 
            this.EnumSplitContainer.Panel1.Controls.Add(this.dgvFiles);
            // 
            // EnumSplitContainer.Panel2
            // 
            this.EnumSplitContainer.Panel2.Controls.Add(this.descriptionText);
            this.EnumSplitContainer.Size = new System.Drawing.Size(525, 399);
            this.EnumSplitContainer.SplitterDistance = 326;
            this.EnumSplitContainer.TabIndex = 2;
            // 
            // descriptionText
            // 
            this.descriptionText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.descriptionText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.descriptionText.Location = new System.Drawing.Point(0, 0);
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.ReadOnly = true;
            this.descriptionText.Size = new System.Drawing.Size(525, 69);
            this.descriptionText.TabIndex = 0;
            this.descriptionText.Text = "";
            // 
            // statusStrip
            // 
            this.statusStrip.GripMargin = new System.Windows.Forms.Padding(1);
            this.statusStrip.Location = new System.Drawing.Point(0, 508);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(940, 22);
            this.statusStrip.TabIndex = 8;
            this.statusStrip.Text = "statusStrip1";
            // 
            // StatusTextLable
            // 
            this.StatusTextLable.Name = "StatusTextLable";
            this.StatusTextLable.Size = new System.Drawing.Size(0, 17);
            // 
            // useRegex
            // 
            this.useRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.useRegex.AutoSize = true;
            this.useRegex.Location = new System.Drawing.Point(662, 6);
            this.useRegex.Name = "useRegex";
            this.useRegex.Size = new System.Drawing.Size(79, 19);
            this.useRegex.TabIndex = 9;
            this.useRegex.Text = "Use Regex";
            this.useRegex.UseVisualStyleBackColor = true;
            this.useRegex.CheckedChanged += new System.EventHandler(this.useRegex_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(236, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 15);
            this.label7.TabIndex = 24;
            this.label7.Text = "Trace Like:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 47);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(57, 15);
            this.label9.TabIndex = 23;
            this.label9.Text = "Date End:";
            // 
            // traceLikeText
            // 
            this.traceLikeText.Location = new System.Drawing.Point(326, 15);
            this.traceLikeText.Name = "traceLikeText";
            this.traceLikeText.Size = new System.Drawing.Size(328, 23);
            this.traceLikeText.TabIndex = 25;
            this.traceLikeText.TextChanged += new System.EventHandler(this.traceLikeText_TextChanged);
            // 
            // dateTimePickerEnd
            // 
            this.dateTimePickerEnd.Checked = false;
            this.dateTimePickerEnd.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            this.dateTimePickerEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerEnd.Location = new System.Drawing.Point(74, 44);
            this.dateTimePickerEnd.Name = "dateTimePickerEnd";
            this.dateTimePickerEnd.ShowCheckBox = true;
            this.dateTimePickerEnd.ShowUpDown = true;
            this.dateTimePickerEnd.Size = new System.Drawing.Size(156, 23);
            this.dateTimePickerEnd.TabIndex = 21;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(236, 47);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(85, 15);
            this.label10.TabIndex = 26;
            this.label10.Text = "Trace Not Like:";
            // 
            // dateTimePickerStart
            // 
            this.dateTimePickerStart.Checked = false;
            this.dateTimePickerStart.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            this.dateTimePickerStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerStart.Location = new System.Drawing.Point(74, 15);
            this.dateTimePickerStart.Name = "dateTimePickerStart";
            this.dateTimePickerStart.ShowCheckBox = true;
            this.dateTimePickerStart.ShowUpDown = true;
            this.dateTimePickerStart.Size = new System.Drawing.Size(156, 23);
            this.dateTimePickerStart.TabIndex = 20;
            // 
            // traceNotLikeText
            // 
            this.traceNotLikeText.Location = new System.Drawing.Point(326, 43);
            this.traceNotLikeText.Name = "traceNotLikeText";
            this.traceNotLikeText.Size = new System.Drawing.Size(328, 23);
            this.traceNotLikeText.TabIndex = 27;
            this.traceNotLikeText.TextChanged += new System.EventHandler(this.traceNotLikeText_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 18);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 15);
            this.label8.TabIndex = 22;
            this.label8.Text = "Date Start:";
            // 
            // buttonFilter
            // 
            this.buttonFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFilter.Image = global::LogsReader.Properties.Resources.filter;
            this.buttonFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonFilter.Location = new System.Drawing.Point(745, 44);
            this.buttonFilter.Name = "buttonFilter";
            this.buttonFilter.Size = new System.Drawing.Size(91, 24);
            this.buttonFilter.TabIndex = 28;
            this.buttonFilter.Text = "      Filter [F7]";
            this.buttonFilter.UseVisualStyleBackColor = true;
            this.buttonFilter.Click += new System.EventHandler(this.buttonFilter_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReset.Image = global::LogsReader.Properties.Resources.Symbol_Refresh;
            this.buttonReset.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonReset.Location = new System.Drawing.Point(842, 44);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(87, 24);
            this.buttonReset.TabIndex = 29;
            this.buttonReset.Text = "      Reset [F8]";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(660, 18);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(56, 15);
            this.label11.TabIndex = 30;
            this.label11.Text = "Message:";
            // 
            // msgFilterText
            // 
            this.msgFilterText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.msgFilterText.Location = new System.Drawing.Point(722, 15);
            this.msgFilterText.Name = "msgFilterText";
            this.msgFilterText.Size = new System.Drawing.Size(207, 23);
            this.msgFilterText.TabIndex = 31;
            this.msgFilterText.TextChanged += new System.EventHandler(this.msgFilterText_TextChanged);
            // 
            // groupBoxFilter
            // 
            this.groupBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFilter.Controls.Add(this.msgFilterText);
            this.groupBoxFilter.Controls.Add(this.label11);
            this.groupBoxFilter.Controls.Add(this.buttonReset);
            this.groupBoxFilter.Controls.Add(this.buttonFilter);
            this.groupBoxFilter.Controls.Add(this.label8);
            this.groupBoxFilter.Controls.Add(this.traceNotLikeText);
            this.groupBoxFilter.Controls.Add(this.dateTimePickerStart);
            this.groupBoxFilter.Controls.Add(this.label10);
            this.groupBoxFilter.Controls.Add(this.dateTimePickerEnd);
            this.groupBoxFilter.Controls.Add(this.traceLikeText);
            this.groupBoxFilter.Controls.Add(this.label9);
            this.groupBoxFilter.Controls.Add(this.label7);
            this.groupBoxFilter.Location = new System.Drawing.Point(2, 19);
            this.groupBoxFilter.Name = "groupBoxFilter";
            this.groupBoxFilter.Size = new System.Drawing.Size(935, 76);
            this.groupBoxFilter.TabIndex = 28;
            this.groupBoxFilter.TabStop = false;
            // 
            // filterPanel
            // 
            this.filterPanel.Controls.Add(this.txtPattern);
            this.filterPanel.Controls.Add(this.useRegex);
            this.filterPanel.Controls.Add(this.btnSearch);
            this.filterPanel.Controls.Add(this.btnClear);
            this.filterPanel.Controls.Add(this.groupBoxFilter);
            this.filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterPanel.Location = new System.Drawing.Point(0, 0);
            this.filterPanel.MinimumSize = new System.Drawing.Size(940, 95);
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Size = new System.Drawing.Size(940, 95);
            this.filterPanel.TabIndex = 30;
            // 
            // LogsReaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ParentSplitContainer);
            this.Controls.Add(this.filterPanel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.MinimumSize = new System.Drawing.Size(0, 25);
            this.Name = "LogsReaderForm";
            this.Size = new System.Drawing.Size(940, 530);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).EndInit();
            this.ParentSplitContainer.Panel1.ResumeLayout(false);
            this.ParentSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ParentSplitContainer)).EndInit();
            this.ParentSplitContainer.ResumeLayout(false);
            this.SchemePanel.ResumeLayout(false);
            this.SchemePanel.PerformLayout();
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
            this.MainSplitContainer.ResumeLayout(false);
            this.EnumSplitContainer.Panel1.ResumeLayout(false);
            this.EnumSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.EnumSplitContainer)).EndInit();
            this.EnumSplitContainer.ResumeLayout(false);
            this.groupBoxFilter.ResumeLayout(false);
            this.groupBoxFilter.PerformLayout();
            this.filterPanel.ResumeLayout(false);
            this.filterPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView dgvFiles;
        private TreeViewImproved trvMain;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TextBox txtPattern;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.SplitContainer ParentSplitContainer;
        private System.Windows.Forms.SplitContainer MainSplitContainer;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusTextLable;
        private System.Windows.Forms.CheckBox useRegex;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox serversText;
        private System.Windows.Forms.TextBox fileNames;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox maxThreadsText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox logDirText;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox maxLinesStackText;
        private System.Windows.Forms.Label label6;
        private Label label7;
        private Label label9;
        private TextBox traceLikeText;
        private DateTimePicker dateTimePickerEnd;
        private Label label10;
        private DateTimePicker dateTimePickerStart;
        private TextBox traceNotLikeText;
        private Label label8;
        private Button buttonFilter;
        private Button buttonReset;
        private Label label11;
        private TextBox msgFilterText;
        private GroupBox groupBoxFilter;
        private SplitContainer EnumSplitContainer;
        private RichTextBox descriptionText;
        private Panel SchemePanel;
        private Panel filterPanel;
        private DataGridViewTextBoxColumn PrivateID;
        private DataGridViewTextBoxColumn IsMatched;
        private DataGridViewTextBoxColumn ID;
        private DataGridViewTextBoxColumn Server;
        private DataGridViewTextBoxColumn Trace;
        private DataGridViewTextBoxColumn Date;
        private DataGridViewTextBoxColumn FileName;
    }
}

