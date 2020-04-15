
using Utils.WinForm.Notepad;

namespace LogsReader.Reader
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Servers");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Types");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogsReaderForm));
            this.dgvFiles = new System.Windows.Forms.DataGridView();
            this.PrivateID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsMatched = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Server = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TraceName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DateOfTrace = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.File = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSearch = new Telerik.WinControls.UI.RadButton();
            this.btnClear = new Telerik.WinControls.UI.RadButton();
            this.txtPattern = new Telerik.WinControls.UI.RadTextBox();
            this.progressBar = new Telerik.WinControls.UI.RadProgressBar();
            this.label12 = new Telerik.WinControls.UI.RadLabel();
            this.orderByText = new Telerik.WinControls.UI.RadTextBox();
            this.label2 = new Telerik.WinControls.UI.RadLabel();
            this.rowsLimitText = new Telerik.WinControls.UI.RadTextBox();
            this.label1 = new Telerik.WinControls.UI.RadLabel();
            this.trvMain = new Utils.WinForm.CustomTreeView();
            this.maxLinesStackText = new Telerik.WinControls.UI.RadTextBox();
            this.serversText = new Telerik.WinControls.UI.RadTextBox();
            this.label6 = new Telerik.WinControls.UI.RadLabel();
            this.label3 = new Telerik.WinControls.UI.RadLabel();
            this.logDirText = new Telerik.WinControls.UI.RadTextBox();
            this.fileNames = new Telerik.WinControls.UI.RadTextBox();
            this.label5 = new Telerik.WinControls.UI.RadLabel();
            this.label4 = new Telerik.WinControls.UI.RadLabel();
            this.maxThreadsText = new Telerik.WinControls.UI.RadTextBox();
            this.descriptionText = new System.Windows.Forms.RichTextBox();
            this.StatusTextLable = new System.Windows.Forms.ToolStripStatusLabel();
            this.useRegex = new Telerik.WinControls.UI.RadCheckBox();
            this.label7 = new Telerik.WinControls.UI.RadLabel();
            this.label9 = new Telerik.WinControls.UI.RadLabel();
            this.traceNameFilter = new Telerik.WinControls.UI.RadTextBox();
            this.dateEndFilter = new Telerik.WinControls.UI.RadDateTimePicker();
            this.dateStartFilter = new Telerik.WinControls.UI.RadDateTimePicker();
            this.label8 = new Telerik.WinControls.UI.RadLabel();
            this.buttonFilter = new Telerik.WinControls.UI.RadButton();
            this.buttonReset = new Telerik.WinControls.UI.RadButton();
            this.label11 = new Telerik.WinControls.UI.RadLabel();
            this.traceMessageFilter = new Telerik.WinControls.UI.RadTextBox();
            this.groupBoxFilter = new Telerik.WinControls.UI.RadGroupBox();
            this.traceMessageFilterComboBox = new System.Windows.Forms.ComboBox();
            this.traceNameFilterComboBox = new System.Windows.Forms.ComboBox();
            this.alreadyUseFilter = new Telerik.WinControls.UI.RadCheckBox();
            this.buttonExport = new Telerik.WinControls.UI.RadButton();
            this.filterPanel = new Telerik.WinControls.UI.RadPanel();
            this.ParentRadSplitContainer = new Telerik.WinControls.UI.RadSplitContainer();
            this.mainPanel = new Telerik.WinControls.UI.SplitPanel();
            this.MainRadSplitContainer = new Telerik.WinControls.UI.RadSplitContainer();
            this.schemePanel = new Telerik.WinControls.UI.SplitPanel();
            this.enumPanel = new Telerik.WinControls.UI.SplitPanel();
            this.dgvDescContainer = new Telerik.WinControls.UI.RadSplitContainer();
            this.dgvPanel = new Telerik.WinControls.UI.SplitPanel();
            this.descriptionPanel = new Telerik.WinControls.UI.SplitPanel();
            this.radStatusStrip = new Telerik.WinControls.UI.RadStatusStrip();
            this.splitPanel2 = new Telerik.WinControls.UI.SplitPanel();
            this.notepad = new Utils.WinForm.Notepad.NotepadControl();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClear)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPattern)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.orderByText)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rowsLimitText)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxLinesStackText)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.serversText)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.logDirText)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileNames)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxThreadsText)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.useRegex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.traceNameFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateEndFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateStartFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonReset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label11)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.traceMessageFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxFilter)).BeginInit();
            this.groupBoxFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.alreadyUseFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonExport)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.filterPanel)).BeginInit();
            this.filterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ParentRadSplitContainer)).BeginInit();
            this.ParentRadSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainPanel)).BeginInit();
            this.mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainRadSplitContainer)).BeginInit();
            this.MainRadSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.schemePanel)).BeginInit();
            this.schemePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.enumPanel)).BeginInit();
            this.enumPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDescContainer)).BeginInit();
            this.dgvDescContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPanel)).BeginInit();
            this.dgvPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.descriptionPanel)).BeginInit();
            this.descriptionPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radStatusStrip)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel2)).BeginInit();
            this.splitPanel2.SuspendLayout();
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
            this.TraceName,
            this.DateOfTrace,
            this.File});
            this.dgvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvFiles.GridColor = System.Drawing.SystemColors.ControlLight;
            this.dgvFiles.Location = new System.Drawing.Point(0, 0);
            this.dgvFiles.MultiSelect = false;
            this.dgvFiles.Name = "dgvFiles";
            this.dgvFiles.ReadOnly = true;
            this.dgvFiles.RowHeadersVisible = false;
            this.dgvFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvFiles.Size = new System.Drawing.Size(415, 332);
            this.dgvFiles.TabIndex = 1;
            this.dgvFiles.SelectionChanged += new System.EventHandler(this.DgvFiles_SelectionChanged);
            this.dgvFiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DgvFiles_MouseDown);
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
            this.ID.MinimumWidth = 30;
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            this.ID.Width = 30;
            // 
            // Server
            // 
            this.Server.DataPropertyName = "Server";
            this.Server.HeaderText = "Server";
            this.Server.MinimumWidth = 40;
            this.Server.Name = "Server";
            this.Server.ReadOnly = true;
            this.Server.Width = 50;
            // 
            // TraceName
            // 
            this.TraceName.DataPropertyName = "TraceName";
            this.TraceName.HeaderText = "TraceName";
            this.TraceName.MinimumWidth = 40;
            this.TraceName.Name = "TraceName";
            this.TraceName.ReadOnly = true;
            this.TraceName.Width = 80;
            // 
            // DateOfTrace
            // 
            this.DateOfTrace.DataPropertyName = "DateOfTrace";
            this.DateOfTrace.HeaderText = "Date";
            this.DateOfTrace.MinimumWidth = 40;
            this.DateOfTrace.Name = "DateOfTrace";
            this.DateOfTrace.ReadOnly = true;
            this.DateOfTrace.Width = 140;
            // 
            // File
            // 
            this.File.DataPropertyName = "File";
            this.File.HeaderText = "File";
            this.File.MinimumWidth = 40;
            this.File.Name = "File";
            this.File.ReadOnly = true;
            this.File.Width = 1000;
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Image = global::LogsReader.Properties.Resources.find;
            this.btnSearch.Location = new System.Drawing.Point(724, 1);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Padding = new System.Windows.Forms.Padding(1, 0, 3, 0);
            this.btnSearch.Size = new System.Drawing.Size(79, 24);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "Search [F5]";
            this.btnSearch.TextAlignment = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Image = global::LogsReader.Properties.Resources.clear1;
            this.btnClear.Location = new System.Drawing.Point(809, 1);
            this.btnClear.Name = "btnClear";
            this.btnClear.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.btnClear.Size = new System.Drawing.Size(79, 24);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear [F6]";
            this.btnClear.TextAlignment = System.Drawing.ContentAlignment.MiddleRight;
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // txtPattern
            // 
            this.txtPattern.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPattern.Location = new System.Drawing.Point(5, 3);
            this.txtPattern.Name = "txtPattern";
            this.txtPattern.Size = new System.Drawing.Size(635, 20);
            this.txtPattern.TabIndex = 4;
            this.txtPattern.TextChanged += new System.EventHandler(this.TxtPattern_TextChanged);
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(0, 516);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(892, 10);
            this.progressBar.TabIndex = 6;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(3, 164);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(50, 18);
            this.label12.TabIndex = 17;
            this.label12.Text = "Order By";
            // 
            // orderByText
            // 
            this.orderByText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.orderByText.Location = new System.Drawing.Point(75, 161);
            this.orderByText.Name = "orderByText";
            this.orderByText.Size = new System.Drawing.Size(110, 20);
            this.orderByText.TabIndex = 18;
            this.orderByText.Leave += new System.EventHandler(this.OrderByText_Leave);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 138);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 18);
            this.label2.TabIndex = 15;
            this.label2.Text = "Rows Limit";
            // 
            // rowsLimitText
            // 
            this.rowsLimitText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rowsLimitText.Location = new System.Drawing.Point(75, 135);
            this.rowsLimitText.Name = "rowsLimitText";
            this.rowsLimitText.Size = new System.Drawing.Size(110, 20);
            this.rowsLimitText.TabIndex = 16;
            this.rowsLimitText.TextChanged += new System.EventHandler(this.RowsLimitText_TextChanged);
            this.rowsLimitText.Leave += new System.EventHandler(this.RowsLimitText_Leave);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 18);
            this.label1.TabIndex = 3;
            this.label1.Text = "Servers";
            // 
            // trvMain
            // 
            this.trvMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trvMain.CheckBoxes = true;
            this.trvMain.Location = new System.Drawing.Point(0, 188);
            this.trvMain.Name = "trvMain";
            treeNode1.Name = "trvServers";
            treeNode1.Text = "Servers";
            treeNode2.Name = "trvTypes";
            treeNode2.Text = "Types";
            this.trvMain.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            this.trvMain.Size = new System.Drawing.Size(193, 214);
            this.trvMain.TabIndex = 2;
            this.trvMain.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.TrvMain_AfterCheck);
            // 
            // maxLinesStackText
            // 
            this.maxLinesStackText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.maxLinesStackText.Location = new System.Drawing.Point(75, 83);
            this.maxLinesStackText.Name = "maxLinesStackText";
            this.maxLinesStackText.Size = new System.Drawing.Size(110, 20);
            this.maxLinesStackText.TabIndex = 14;
            this.maxLinesStackText.TextChanged += new System.EventHandler(this.MaxLinesStackText_TextChanged);
            this.maxLinesStackText.Leave += new System.EventHandler(this.MaxLinesStackText_Leave);
            // 
            // serversText
            // 
            this.serversText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serversText.Location = new System.Drawing.Point(75, 5);
            this.serversText.Name = "serversText";
            this.serversText.Size = new System.Drawing.Size(110, 20);
            this.serversText.TabIndex = 4;
            this.serversText.TextChanged += new System.EventHandler(this.ServersText_TextChanged);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(3, 86);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 18);
            this.label6.TabIndex = 13;
            this.label6.Text = "Max Lines";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(3, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 18);
            this.label3.TabIndex = 7;
            this.label3.Text = "File Types";
            // 
            // logDirText
            // 
            this.logDirText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logDirText.Location = new System.Drawing.Point(75, 31);
            this.logDirText.Name = "logDirText";
            this.logDirText.Size = new System.Drawing.Size(110, 20);
            this.logDirText.TabIndex = 12;
            this.logDirText.TextChanged += new System.EventHandler(this.LogDirText_TextChanged);
            // 
            // fileNames
            // 
            this.fileNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileNames.Location = new System.Drawing.Point(75, 57);
            this.fileNames.Name = "fileNames";
            this.fileNames.Size = new System.Drawing.Size(110, 20);
            this.fileNames.TabIndex = 8;
            this.fileNames.TextChanged += new System.EventHandler(this.TypesText_TextChanged);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(3, 35);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 18);
            this.label5.TabIndex = 11;
            this.label5.Text = "Logs folder";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(3, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 18);
            this.label4.TabIndex = 9;
            this.label4.Text = "Max Threads";
            // 
            // maxThreadsText
            // 
            this.maxThreadsText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.maxThreadsText.Location = new System.Drawing.Point(75, 109);
            this.maxThreadsText.Name = "maxThreadsText";
            this.maxThreadsText.Size = new System.Drawing.Size(110, 20);
            this.maxThreadsText.TabIndex = 10;
            this.maxThreadsText.TextChanged += new System.EventHandler(this.MaxThreadsText_TextChanged);
            this.maxThreadsText.Leave += new System.EventHandler(this.MaxThreadsText_Leave);
            // 
            // descriptionText
            // 
            this.descriptionText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.descriptionText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.descriptionText.Location = new System.Drawing.Point(0, 0);
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.ReadOnly = true;
            this.descriptionText.Size = new System.Drawing.Size(415, 66);
            this.descriptionText.TabIndex = 0;
            this.descriptionText.Text = "";
            // 
            // StatusTextLable
            // 
            this.StatusTextLable.Name = "StatusTextLable";
            this.StatusTextLable.Size = new System.Drawing.Size(0, 17);
            // 
            // useRegex
            // 
            this.useRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.useRegex.Location = new System.Drawing.Point(646, 4);
            this.useRegex.Name = "useRegex";
            this.useRegex.Size = new System.Drawing.Size(72, 18);
            this.useRegex.TabIndex = 9;
            this.useRegex.Text = "Use Regex";
            this.useRegex.CheckStateChanged += new System.EventHandler(this.UseRegex_CheckedChanged);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(230, 7);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 18);
            this.label7.TabIndex = 24;
            this.label7.Text = "TraceName";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(5, 34);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(52, 18);
            this.label9.TabIndex = 23;
            this.label9.Text = "Date End";
            // 
            // traceNameFilter
            // 
            this.traceNameFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.traceNameFilter.Location = new System.Drawing.Point(400, 6);
            this.traceNameFilter.Name = "traceNameFilter";
            this.traceNameFilter.Size = new System.Drawing.Size(237, 20);
            this.traceNameFilter.TabIndex = 25;
            this.traceNameFilter.TextChanged += new System.EventHandler(this.TraceNameFilter_TextChanged);
            // 
            // dateEndFilter
            // 
            this.dateEndFilter.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            this.dateEndFilter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateEndFilter.Location = new System.Drawing.Point(65, 33);
            this.dateEndFilter.Name = "dateEndFilter";
            this.dateEndFilter.ShowCheckBox = true;
            this.dateEndFilter.ShowUpDown = true;
            this.dateEndFilter.Size = new System.Drawing.Size(159, 20);
            this.dateEndFilter.TabIndex = 21;
            this.dateEndFilter.TabStop = false;
            this.dateEndFilter.Text = "15.04.2020 18:05:38";
            this.dateEndFilter.Value = new System.DateTime(2020, 4, 15, 18, 5, 38, 688);
            // 
            // dateStartFilter
            // 
            this.dateStartFilter.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            this.dateStartFilter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateStartFilter.Location = new System.Drawing.Point(65, 6);
            this.dateStartFilter.Name = "dateStartFilter";
            this.dateStartFilter.ShowCheckBox = true;
            this.dateStartFilter.ShowUpDown = true;
            this.dateStartFilter.Size = new System.Drawing.Size(159, 20);
            this.dateStartFilter.TabIndex = 20;
            this.dateStartFilter.TabStop = false;
            this.dateStartFilter.Text = "15.04.2020 18:05:38";
            this.dateStartFilter.Value = new System.DateTime(2020, 4, 15, 18, 5, 38, 732);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(3, 7);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 18);
            this.label8.TabIndex = 22;
            this.label8.Text = "Date Start";
            // 
            // buttonFilter
            // 
            this.buttonFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFilter.Image = global::LogsReader.Properties.Resources.filter17;
            this.buttonFilter.Location = new System.Drawing.Point(643, 5);
            this.buttonFilter.Name = "buttonFilter";
            this.buttonFilter.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.buttonFilter.Size = new System.Drawing.Size(75, 23);
            this.buttonFilter.TabIndex = 28;
            this.buttonFilter.Text = "Filter [F7]";
            this.buttonFilter.TextAlignment = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonFilter.Click += new System.EventHandler(this.buttonFilter_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReset.Image = global::LogsReader.Properties.Resources.reset2;
            this.buttonReset.Location = new System.Drawing.Point(643, 31);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.buttonReset.Size = new System.Drawing.Size(75, 23);
            this.buttonReset.TabIndex = 29;
            this.buttonReset.Text = "Reset [F8]";
            this.buttonReset.TextAlignment = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(230, 34);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(33, 18);
            this.label11.TabIndex = 30;
            this.label11.Text = "Trace";
            // 
            // traceMessageFilter
            // 
            this.traceMessageFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.traceMessageFilter.Location = new System.Drawing.Point(400, 33);
            this.traceMessageFilter.Name = "traceMessageFilter";
            this.traceMessageFilter.Size = new System.Drawing.Size(237, 20);
            this.traceMessageFilter.TabIndex = 31;
            this.traceMessageFilter.TextChanged += new System.EventHandler(this.TraceMessageFilter_TextChanged);
            // 
            // groupBoxFilter
            // 
            this.groupBoxFilter.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
            this.groupBoxFilter.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxFilter.Controls.Add(this.traceMessageFilterComboBox);
            this.groupBoxFilter.Controls.Add(this.traceNameFilterComboBox);
            this.groupBoxFilter.Controls.Add(this.alreadyUseFilter);
            this.groupBoxFilter.Controls.Add(this.buttonExport);
            this.groupBoxFilter.Controls.Add(this.traceMessageFilter);
            this.groupBoxFilter.Controls.Add(this.label11);
            this.groupBoxFilter.Controls.Add(this.buttonReset);
            this.groupBoxFilter.Controls.Add(this.buttonFilter);
            this.groupBoxFilter.Controls.Add(this.label8);
            this.groupBoxFilter.Controls.Add(this.dateStartFilter);
            this.groupBoxFilter.Controls.Add(this.dateEndFilter);
            this.groupBoxFilter.Controls.Add(this.traceNameFilter);
            this.groupBoxFilter.Controls.Add(this.label9);
            this.groupBoxFilter.Controls.Add(this.label7);
            this.groupBoxFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxFilter.HeaderText = "";
            this.groupBoxFilter.Location = new System.Drawing.Point(0, 27);
            this.groupBoxFilter.MinimumSize = new System.Drawing.Size(850, 0);
            this.groupBoxFilter.Name = "groupBoxFilter";
            // 
            // 
            // 
            this.groupBoxFilter.RootElement.MinSize = new System.Drawing.Size(850, 0);
            this.groupBoxFilter.Size = new System.Drawing.Size(892, 61);
            this.groupBoxFilter.TabIndex = 28;
            this.groupBoxFilter.TabStop = false;
            ((Telerik.WinControls.UI.GroupBoxContent)(this.groupBoxFilter.GetChildAt(0).GetChildAt(0))).BorderHighlightThickness = 1;
            ((Telerik.WinControls.Primitives.BorderPrimitive)(this.groupBoxFilter.GetChildAt(0).GetChildAt(0).GetChildAt(1))).ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            // 
            // traceMessageFilterComboBox
            // 
            this.traceMessageFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.traceMessageFilterComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.traceMessageFilterComboBox.FormattingEnabled = true;
            this.traceMessageFilterComboBox.Items.AddRange(new object[] {
            "Contains",
            "Not Contains"});
            this.traceMessageFilterComboBox.Location = new System.Drawing.Point(299, 33);
            this.traceMessageFilterComboBox.MaxDropDownItems = 2;
            this.traceMessageFilterComboBox.Name = "traceMessageFilterComboBox";
            this.traceMessageFilterComboBox.Size = new System.Drawing.Size(95, 21);
            this.traceMessageFilterComboBox.Sorted = true;
            this.traceMessageFilterComboBox.TabIndex = 35;
            this.traceMessageFilterComboBox.SelectedIndexChanged += new System.EventHandler(this.traceMessageFilterComboBox_SelectedIndexChanged);
            this.traceMessageFilterComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ComboBox_KeyPress);
            // 
            // traceNameFilterComboBox
            // 
            this.traceNameFilterComboBox.BackColor = System.Drawing.SystemColors.Window;
            this.traceNameFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.traceNameFilterComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.traceNameFilterComboBox.FormattingEnabled = true;
            this.traceNameFilterComboBox.Items.AddRange(new object[] {
            "Contains",
            "Not Contains"});
            this.traceNameFilterComboBox.Location = new System.Drawing.Point(299, 6);
            this.traceNameFilterComboBox.MaxDropDownItems = 2;
            this.traceNameFilterComboBox.Name = "traceNameFilterComboBox";
            this.traceNameFilterComboBox.Size = new System.Drawing.Size(95, 21);
            this.traceNameFilterComboBox.Sorted = true;
            this.traceNameFilterComboBox.TabIndex = 34;
            this.traceNameFilterComboBox.SelectedIndexChanged += new System.EventHandler(this.traceNameFilterComboBox_SelectedIndexChanged);
            this.traceNameFilterComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ComboBox_KeyPress);
            // 
            // alreadyUseFilter
            // 
            this.alreadyUseFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.alreadyUseFilter.Location = new System.Drawing.Point(724, 33);
            this.alreadyUseFilter.Name = "alreadyUseFilter";
            this.alreadyUseFilter.Size = new System.Drawing.Size(146, 18);
            this.alreadyUseFilter.TabIndex = 33;
            this.alreadyUseFilter.Text = "Use filter when searching";
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExport.Image = global::LogsReader.Properties.Resources.save2;
            this.buttonExport.Location = new System.Drawing.Point(724, 5);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Padding = new System.Windows.Forms.Padding(0, 0, 3, 2);
            this.buttonExport.Size = new System.Drawing.Size(99, 23);
            this.buttonExport.TabIndex = 32;
            this.buttonExport.Text = "Export [Ctrl+S]";
            this.buttonExport.TextAlignment = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonExport.Click += new System.EventHandler(this.ButtonExport_Click);
            // 
            // filterPanel
            // 
            this.filterPanel.BackColor = System.Drawing.SystemColors.Control;
            this.filterPanel.Controls.Add(this.txtPattern);
            this.filterPanel.Controls.Add(this.useRegex);
            this.filterPanel.Controls.Add(this.btnSearch);
            this.filterPanel.Controls.Add(this.btnClear);
            this.filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterPanel.Location = new System.Drawing.Point(0, 0);
            this.filterPanel.MinimumSize = new System.Drawing.Size(850, 0);
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Padding = new System.Windows.Forms.Padding(2, 0, 1, 0);
            // 
            // 
            // 
            this.filterPanel.RootElement.MinSize = new System.Drawing.Size(850, 0);
            this.filterPanel.Size = new System.Drawing.Size(892, 27);
            this.filterPanel.TabIndex = 30;
            ((Telerik.WinControls.UI.RadPanelElement)(this.filterPanel.GetChildAt(0))).Padding = new System.Windows.Forms.Padding(2, 0, 1, 0);
            ((Telerik.WinControls.Primitives.BorderPrimitive)(this.filterPanel.GetChildAt(0).GetChildAt(1))).Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            // 
            // ParentRadSplitContainer
            // 
            this.ParentRadSplitContainer.Controls.Add(this.mainPanel);
            this.ParentRadSplitContainer.Controls.Add(this.splitPanel2);
            this.ParentRadSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ParentRadSplitContainer.Location = new System.Drawing.Point(0, 88);
            this.ParentRadSplitContainer.Name = "ParentRadSplitContainer";
            // 
            // 
            // 
            this.ParentRadSplitContainer.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.ParentRadSplitContainer.Size = new System.Drawing.Size(892, 428);
            this.ParentRadSplitContainer.TabIndex = 32;
            this.ParentRadSplitContainer.TabStop = false;
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.MainRadSplitContainer);
            this.mainPanel.Controls.Add(this.radStatusStrip);
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            // 
            // 
            // 
            this.mainPanel.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.mainPanel.Size = new System.Drawing.Size(614, 428);
            this.mainPanel.SizeInfo.AbsoluteSize = new System.Drawing.Size(614, 200);
            this.mainPanel.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(0.1790541F, 0F);
            this.mainPanel.SizeInfo.MinimumSize = new System.Drawing.Size(125, 0);
            this.mainPanel.SizeInfo.SizeMode = Telerik.WinControls.UI.Docking.SplitPanelSizeMode.Absolute;
            this.mainPanel.SizeInfo.SplitterCorrection = new System.Drawing.Size(546, 0);
            this.mainPanel.TabIndex = 0;
            this.mainPanel.TabStop = false;
            this.mainPanel.Text = "splitPanel1";
            // 
            // MainRadSplitContainer
            // 
            this.MainRadSplitContainer.Controls.Add(this.schemePanel);
            this.MainRadSplitContainer.Controls.Add(this.enumPanel);
            this.MainRadSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainRadSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainRadSplitContainer.Name = "MainRadSplitContainer";
            // 
            // 
            // 
            this.MainRadSplitContainer.RootElement.MinSize = new System.Drawing.Size(100, 25);
            this.MainRadSplitContainer.Size = new System.Drawing.Size(614, 402);
            this.MainRadSplitContainer.TabIndex = 0;
            this.MainRadSplitContainer.TabStop = false;
            // 
            // schemePanel
            // 
            this.schemePanel.Controls.Add(this.label12);
            this.schemePanel.Controls.Add(this.trvMain);
            this.schemePanel.Controls.Add(this.orderByText);
            this.schemePanel.Controls.Add(this.label1);
            this.schemePanel.Controls.Add(this.label2);
            this.schemePanel.Controls.Add(this.maxThreadsText);
            this.schemePanel.Controls.Add(this.rowsLimitText);
            this.schemePanel.Controls.Add(this.label4);
            this.schemePanel.Controls.Add(this.label5);
            this.schemePanel.Controls.Add(this.maxLinesStackText);
            this.schemePanel.Controls.Add(this.fileNames);
            this.schemePanel.Controls.Add(this.serversText);
            this.schemePanel.Controls.Add(this.logDirText);
            this.schemePanel.Controls.Add(this.label6);
            this.schemePanel.Controls.Add(this.label3);
            this.schemePanel.Location = new System.Drawing.Point(0, 0);
            this.schemePanel.Name = "schemePanel";
            // 
            // 
            // 
            this.schemePanel.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.schemePanel.Size = new System.Drawing.Size(195, 402);
            this.schemePanel.SizeInfo.AbsoluteSize = new System.Drawing.Size(195, 200);
            this.schemePanel.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(-0.1855241F, 0F);
            this.schemePanel.SizeInfo.MinimumSize = new System.Drawing.Size(25, 0);
            this.schemePanel.SizeInfo.SizeMode = Telerik.WinControls.UI.Docking.SplitPanelSizeMode.Absolute;
            this.schemePanel.SizeInfo.SplitterCorrection = new System.Drawing.Size(-122, 0);
            this.schemePanel.TabIndex = 0;
            this.schemePanel.TabStop = false;
            this.schemePanel.Text = "splitPanel3";
            ((Telerik.WinControls.Primitives.BorderPrimitive)(this.schemePanel.GetChildAt(0).GetChildAt(1))).BorderDrawMode = Telerik.WinControls.BorderDrawModes.RightOver;
            ((Telerik.WinControls.Primitives.BorderPrimitive)(this.schemePanel.GetChildAt(0).GetChildAt(1))).BorderDashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            ((Telerik.WinControls.Primitives.BorderPrimitive)(this.schemePanel.GetChildAt(0).GetChildAt(1))).Visibility = Telerik.WinControls.ElementVisibility.Visible;
            // 
            // enumPanel
            // 
            this.enumPanel.Controls.Add(this.dgvDescContainer);
            this.enumPanel.Location = new System.Drawing.Point(199, 0);
            this.enumPanel.Name = "enumPanel";
            // 
            // 
            // 
            this.enumPanel.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.enumPanel.Size = new System.Drawing.Size(415, 402);
            this.enumPanel.SizeInfo.AbsoluteSize = new System.Drawing.Size(489, 200);
            this.enumPanel.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(0.1855241F, 0F);
            this.enumPanel.SizeInfo.MinimumSize = new System.Drawing.Size(100, 0);
            this.enumPanel.SizeInfo.SizeMode = Telerik.WinControls.UI.Docking.SplitPanelSizeMode.Absolute;
            this.enumPanel.SizeInfo.SplitterCorrection = new System.Drawing.Size(122, 0);
            this.enumPanel.TabIndex = 1;
            this.enumPanel.TabStop = false;
            this.enumPanel.Text = "splitPanel4";
            ((Telerik.WinControls.Primitives.BorderPrimitive)(this.enumPanel.GetChildAt(0).GetChildAt(1))).Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            // 
            // dgvDescContainer
            // 
            this.dgvDescContainer.Controls.Add(this.dgvPanel);
            this.dgvDescContainer.Controls.Add(this.descriptionPanel);
            this.dgvDescContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDescContainer.Location = new System.Drawing.Point(0, 0);
            this.dgvDescContainer.Name = "dgvDescContainer";
            this.dgvDescContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // 
            // 
            this.dgvDescContainer.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.dgvDescContainer.Size = new System.Drawing.Size(415, 402);
            this.dgvDescContainer.TabIndex = 0;
            this.dgvDescContainer.TabStop = false;
            // 
            // dgvPanel
            // 
            this.dgvPanel.Controls.Add(this.dgvFiles);
            this.dgvPanel.Location = new System.Drawing.Point(0, 0);
            this.dgvPanel.Name = "dgvPanel";
            // 
            // 
            // 
            this.dgvPanel.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.dgvPanel.Size = new System.Drawing.Size(415, 332);
            this.dgvPanel.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(0F, 0.02786378F);
            this.dgvPanel.SizeInfo.SplitterCorrection = new System.Drawing.Size(0, 327);
            this.dgvPanel.TabIndex = 0;
            this.dgvPanel.TabStop = false;
            this.dgvPanel.Text = "splitPanel5";
            // 
            // descriptionPanel
            // 
            this.descriptionPanel.Controls.Add(this.descriptionText);
            this.descriptionPanel.Location = new System.Drawing.Point(0, 336);
            this.descriptionPanel.Name = "descriptionPanel";
            // 
            // 
            // 
            this.descriptionPanel.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.descriptionPanel.Size = new System.Drawing.Size(415, 66);
            this.descriptionPanel.SizeInfo.AbsoluteSize = new System.Drawing.Size(200, 66);
            this.descriptionPanel.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(0F, -0.4221106F);
            this.descriptionPanel.SizeInfo.MinimumSize = new System.Drawing.Size(0, 25);
            this.descriptionPanel.SizeInfo.SizeMode = Telerik.WinControls.UI.Docking.SplitPanelSizeMode.Absolute;
            this.descriptionPanel.SizeInfo.SplitterCorrection = new System.Drawing.Size(0, -327);
            this.descriptionPanel.TabIndex = 1;
            this.descriptionPanel.TabStop = false;
            this.descriptionPanel.Text = "splitPanel6";
            // 
            // radStatusStrip
            // 
            this.radStatusStrip.Location = new System.Drawing.Point(0, 402);
            this.radStatusStrip.Name = "radStatusStrip";
            this.radStatusStrip.Size = new System.Drawing.Size(614, 26);
            this.radStatusStrip.SizingGrip = false;
            this.radStatusStrip.TabIndex = 1;
            // 
            // splitPanel2
            // 
            this.splitPanel2.Controls.Add(this.notepad);
            this.splitPanel2.Location = new System.Drawing.Point(618, 0);
            this.splitPanel2.Name = "splitPanel2";
            // 
            // 
            // 
            this.splitPanel2.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel2.Size = new System.Drawing.Size(274, 428);
            this.splitPanel2.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(-0.315F, 0F);
            this.splitPanel2.SizeInfo.SplitterCorrection = new System.Drawing.Size(-564, 0);
            this.splitPanel2.TabIndex = 1;
            this.splitPanel2.TabStop = false;
            this.splitPanel2.Text = "splitPanel2";
            // 
            // notepad
            // 
            this.notepad.AllowUserCloseItems = false;
            this.notepad.DefaultEncoding = ((System.Text.Encoding)(resources.GetObject("notepad.DefaultEncoding")));
            this.notepad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.notepad.Highlights = false;
            this.notepad.Location = new System.Drawing.Point(0, 0);
            this.notepad.Name = "notepad";
            this.notepad.ReadOnly = true;
            this.notepad.SelectedIndex = -1;
            this.notepad.Size = new System.Drawing.Size(274, 428);
            this.notepad.SizingGrip = false;
            this.notepad.TabIndex = 0;
            this.notepad.TabsFont = this.Font;
            this.notepad.TabsForeColor = System.Drawing.Color.Green;
            this.notepad.TextFont = new System.Drawing.Font("Segoe UI", 10F);
            this.notepad.TextForeColor = System.Drawing.Color.Black;
            this.notepad.WordWrap = false;
            // 
            // LogsReaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ParentRadSplitContainer);
            this.Controls.Add(this.groupBoxFilter);
            this.Controls.Add(this.filterPanel);
            this.Controls.Add(this.progressBar);
            this.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.MinimumSize = new System.Drawing.Size(0, 25);
            this.Name = "LogsReaderForm";
            this.Size = new System.Drawing.Size(892, 526);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClear)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPattern)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.orderByText)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rowsLimitText)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxLinesStackText)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.serversText)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.logDirText)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileNames)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxThreadsText)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.useRegex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.traceNameFilter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateEndFilter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateStartFilter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonFilter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonReset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label11)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.traceMessageFilter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxFilter)).EndInit();
            this.groupBoxFilter.ResumeLayout(false);
            this.groupBoxFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.alreadyUseFilter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonExport)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.filterPanel)).EndInit();
            this.filterPanel.ResumeLayout(false);
            this.filterPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ParentRadSplitContainer)).EndInit();
            this.ParentRadSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainPanel)).EndInit();
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainRadSplitContainer)).EndInit();
            this.MainRadSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.schemePanel)).EndInit();
            this.schemePanel.ResumeLayout(false);
            this.schemePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.enumPanel)).EndInit();
            this.enumPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDescContainer)).EndInit();
            this.dgvDescContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPanel)).EndInit();
            this.dgvPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.descriptionPanel)).EndInit();
            this.descriptionPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radStatusStrip)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel2)).EndInit();
            this.splitPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridView dgvFiles;
        private System.Windows.Forms.ComboBox traceNameFilterComboBox;
        private System.Windows.Forms.ComboBox traceMessageFilterComboBox;
        private Utils.WinForm.CustomTreeView trvMain;
        private System.Windows.Forms.ToolStripStatusLabel StatusTextLable;
        private System.Windows.Forms.RichTextBox descriptionText;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date;
        private System.Windows.Forms.DataGridViewTextBoxColumn PrivateID;
        private System.Windows.Forms.DataGridViewTextBoxColumn IsMatched;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Server;
        private System.Windows.Forms.DataGridViewTextBoxColumn TraceName;
        private System.Windows.Forms.DataGridViewTextBoxColumn DateOfTrace;
        private System.Windows.Forms.DataGridViewTextBoxColumn File;

        private Telerik.WinControls.UI.RadButton btnSearch;
        private Telerik.WinControls.UI.RadButton btnClear;
        private Telerik.WinControls.UI.RadTextBox txtPattern;
        private Telerik.WinControls.UI.RadProgressBar progressBar;
        private Telerik.WinControls.UI.RadCheckBox useRegex;
        private Telerik.WinControls.UI.RadLabel label1;
        private Telerik.WinControls.UI.RadTextBox serversText;
        private Telerik.WinControls.UI.RadTextBox fileNames;
        private Telerik.WinControls.UI.RadLabel label3;
        private Telerik.WinControls.UI.RadTextBox maxThreadsText;
        private Telerik.WinControls.UI.RadLabel label4;
        private Telerik.WinControls.UI.RadTextBox logDirText;
        private Telerik.WinControls.UI.RadLabel label5;
        private Telerik.WinControls.UI.RadTextBox maxLinesStackText;
        private Telerik.WinControls.UI.RadLabel label6;
        private Telerik.WinControls.UI.RadLabel label7;
        private Telerik.WinControls.UI.RadLabel label9;
        private Telerik.WinControls.UI.RadTextBox traceNameFilter;
        private Telerik.WinControls.UI.RadDateTimePicker dateEndFilter;
        private Telerik.WinControls.UI.RadDateTimePicker dateStartFilter;
        private Telerik.WinControls.UI.RadLabel label8;
        private Telerik.WinControls.UI.RadButton buttonFilter;
        private Telerik.WinControls.UI.RadButton buttonReset;
        private Telerik.WinControls.UI.RadLabel label11;
        private Telerik.WinControls.UI.RadTextBox traceMessageFilter;
        private Telerik.WinControls.UI.RadGroupBox groupBoxFilter;
        private Telerik.WinControls.UI.RadPanel filterPanel;
        private Telerik.WinControls.UI.RadButton buttonExport;
        private Telerik.WinControls.UI.RadCheckBox alreadyUseFilter;
        private Telerik.WinControls.UI.RadLabel label2;
        private Telerik.WinControls.UI.RadTextBox rowsLimitText;
        private Telerik.WinControls.UI.RadLabel label12;
        private Telerik.WinControls.UI.RadTextBox orderByText;
        private Telerik.WinControls.UI.RadSplitContainer ParentRadSplitContainer;
        private Telerik.WinControls.UI.SplitPanel mainPanel;
        private Telerik.WinControls.UI.SplitPanel splitPanel2;
        private Telerik.WinControls.UI.RadSplitContainer MainRadSplitContainer;
        private Telerik.WinControls.UI.SplitPanel schemePanel;
        private Telerik.WinControls.UI.SplitPanel enumPanel;
        private Telerik.WinControls.UI.RadStatusStrip radStatusStrip;
        private Telerik.WinControls.UI.RadSplitContainer dgvDescContainer;
        private Telerik.WinControls.UI.SplitPanel dgvPanel;
        private Telerik.WinControls.UI.SplitPanel descriptionPanel;
        private NotepadControl notepad;
    }
}

