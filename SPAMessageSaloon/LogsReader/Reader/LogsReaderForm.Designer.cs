
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
            this.dgvFiles = new System.Windows.Forms.DataGridView();
            this.PrivateID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsMatched = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Server = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TraceName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DateOfTrace = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.File = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.txtPattern = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.label12 = new System.Windows.Forms.Label();
            this.orderByText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.rowsLimitText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.trvMain = new Utils.WinForm.CustomTreeView();
            this.maxLinesStackText = new System.Windows.Forms.TextBox();
            this.serversText = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.logDirText = new System.Windows.Forms.TextBox();
            this.fileNames = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.maxThreadsText = new System.Windows.Forms.TextBox();
            this.descriptionText = new System.Windows.Forms.RichTextBox();
            this.StatusTextLable = new System.Windows.Forms.ToolStripStatusLabel();
            this.useRegex = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.traceNameFilter = new System.Windows.Forms.TextBox();
            this.dateEndFilter = new System.Windows.Forms.DateTimePicker();
            this.dateStartFilter = new System.Windows.Forms.DateTimePicker();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonFilter = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.traceMessageFilter = new System.Windows.Forms.TextBox();
            this.groupBoxFilter = new System.Windows.Forms.GroupBox();
            this.traceMessageFilterComboBox = new System.Windows.Forms.ComboBox();
            this.traceNameFilterComboBox = new System.Windows.Forms.ComboBox();
            this.alreadyUseFilter = new System.Windows.Forms.CheckBox();
            this.buttonExport = new System.Windows.Forms.Button();
            this.filterPanel = new System.Windows.Forms.Panel();
            this.ParentRadSplitContainer = new Telerik.WinControls.UI.RadSplitContainer();
            this.splitPanel1 = new Telerik.WinControls.UI.SplitPanel();
            this.MainRadSplitContainer = new Telerik.WinControls.UI.RadSplitContainer();
            this.splitPanel3 = new Telerik.WinControls.UI.SplitPanel();
            this.splitPanel4 = new Telerik.WinControls.UI.SplitPanel();
            this.EnumRadSplitContainer = new Telerik.WinControls.UI.RadSplitContainer();
            this.splitPanel5 = new Telerik.WinControls.UI.SplitPanel();
            this.splitPanel6 = new Telerik.WinControls.UI.SplitPanel();
            this.radStatusStrip = new Telerik.WinControls.UI.RadStatusStrip();
            this.splitPanel2 = new Telerik.WinControls.UI.SplitPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).BeginInit();
            this.groupBoxFilter.SuspendLayout();
            this.filterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ParentRadSplitContainer)).BeginInit();
            this.ParentRadSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel1)).BeginInit();
            this.splitPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainRadSplitContainer)).BeginInit();
            this.MainRadSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel3)).BeginInit();
            this.splitPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel4)).BeginInit();
            this.splitPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EnumRadSplitContainer)).BeginInit();
            this.EnumRadSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel5)).BeginInit();
            this.splitPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel6)).BeginInit();
            this.splitPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radStatusStrip)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel2)).BeginInit();
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
            this.dgvFiles.Size = new System.Drawing.Size(421, 535);
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
            this.btnSearch.BackColor = System.Drawing.SystemColors.Control;
            this.btnSearch.Image = global::LogsReader.Properties.Resources.find;
            this.btnSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSearch.Location = new System.Drawing.Point(1065, 2);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
            this.btnSearch.Size = new System.Drawing.Size(90, 24);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "Search [F5]";
            this.btnSearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Image = global::LogsReader.Properties.Resources.clear1;
            this.btnClear.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClear.Location = new System.Drawing.Point(1161, 2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(87, 24);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear [F6]";
            this.btnClear.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // txtPattern
            // 
            this.txtPattern.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPattern.Location = new System.Drawing.Point(3, 3);
            this.txtPattern.Name = "txtPattern";
            this.txtPattern.Size = new System.Drawing.Size(965, 23);
            this.txtPattern.TabIndex = 4;
            this.txtPattern.TextChanged += new System.EventHandler(this.TxtPattern_TextChanged);
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(0, 805);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1251, 10);
            this.progressBar.TabIndex = 6;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 164);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(55, 13);
            this.label12.TabIndex = 17;
            this.label12.Text = "Order By:";
            // 
            // orderByText
            // 
            this.orderByText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.orderByText.Location = new System.Drawing.Point(84, 161);
            this.orderByText.Name = "orderByText";
            this.orderByText.Size = new System.Drawing.Size(129, 23);
            this.orderByText.TabIndex = 18;
            this.orderByText.Leave += new System.EventHandler(this.OrderByText_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 138);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Rows Limit:";
            // 
            // rowsLimitText
            // 
            this.rowsLimitText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rowsLimitText.Location = new System.Drawing.Point(84, 135);
            this.rowsLimitText.Name = "rowsLimitText";
            this.rowsLimitText.Size = new System.Drawing.Size(129, 23);
            this.rowsLimitText.TabIndex = 16;
            this.rowsLimitText.TextChanged += new System.EventHandler(this.RowsLimitText_TextChanged);
            this.rowsLimitText.Leave += new System.EventHandler(this.RowsLimitText_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Servers:";
            // 
            // trvMain
            // 
            this.trvMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trvMain.CheckBoxes = true;
            this.trvMain.Location = new System.Drawing.Point(0, 190);
            this.trvMain.Name = "trvMain";
            treeNode1.Name = "trvServers";
            treeNode1.Text = "Servers";
            treeNode2.Name = "trvTypes";
            treeNode2.Text = "Types";
            this.trvMain.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            this.trvMain.Size = new System.Drawing.Size(221, 494);
            this.trvMain.TabIndex = 2;
            this.trvMain.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.TrvMain_AfterCheck);
            // 
            // maxLinesStackText
            // 
            this.maxLinesStackText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.maxLinesStackText.Location = new System.Drawing.Point(84, 83);
            this.maxLinesStackText.Name = "maxLinesStackText";
            this.maxLinesStackText.Size = new System.Drawing.Size(129, 23);
            this.maxLinesStackText.TabIndex = 14;
            this.maxLinesStackText.TextChanged += new System.EventHandler(this.MaxLinesStackText_TextChanged);
            this.maxLinesStackText.Leave += new System.EventHandler(this.MaxLinesStackText_Leave);
            // 
            // serversText
            // 
            this.serversText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serversText.Location = new System.Drawing.Point(84, 5);
            this.serversText.Name = "serversText";
            this.serversText.Size = new System.Drawing.Size(129, 23);
            this.serversText.TabIndex = 4;
            this.serversText.TextChanged += new System.EventHandler(this.ServersText_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 86);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Max Lines:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "File Types:";
            // 
            // logDirText
            // 
            this.logDirText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logDirText.Location = new System.Drawing.Point(84, 31);
            this.logDirText.Name = "logDirText";
            this.logDirText.Size = new System.Drawing.Size(129, 23);
            this.logDirText.TabIndex = 12;
            this.logDirText.TextChanged += new System.EventHandler(this.LogDirText_TextChanged);
            // 
            // fileNames
            // 
            this.fileNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileNames.Location = new System.Drawing.Point(84, 57);
            this.fileNames.Name = "fileNames";
            this.fileNames.Size = new System.Drawing.Size(129, 23);
            this.fileNames.TabIndex = 8;
            this.fileNames.TextChanged += new System.EventHandler(this.TypesText_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 35);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Logs folder:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Max Threads:";
            // 
            // maxThreadsText
            // 
            this.maxThreadsText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.maxThreadsText.Location = new System.Drawing.Point(84, 109);
            this.maxThreadsText.Name = "maxThreadsText";
            this.maxThreadsText.Size = new System.Drawing.Size(129, 23);
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
            this.descriptionText.Size = new System.Drawing.Size(421, 145);
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
            this.useRegex.AutoSize = true;
            this.useRegex.Location = new System.Drawing.Point(976, 6);
            this.useRegex.Name = "useRegex";
            this.useRegex.Size = new System.Drawing.Size(79, 19);
            this.useRegex.TabIndex = 9;
            this.useRegex.Text = "Use Regex";
            this.useRegex.UseVisualStyleBackColor = true;
            this.useRegex.CheckedChanged += new System.EventHandler(this.UseRegex_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(236, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 15);
            this.label7.TabIndex = 24;
            this.label7.Text = "TraceName";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 48);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(54, 15);
            this.label9.TabIndex = 23;
            this.label9.Text = "Date End";
            // 
            // traceNameFilter
            // 
            this.traceNameFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.traceNameFilter.Location = new System.Drawing.Point(407, 15);
            this.traceNameFilter.Name = "traceNameFilter";
            this.traceNameFilter.Size = new System.Drawing.Size(559, 23);
            this.traceNameFilter.TabIndex = 25;
            this.traceNameFilter.TextChanged += new System.EventHandler(this.TraceNameFilter_TextChanged);
            // 
            // dateEndFilter
            // 
            this.dateEndFilter.Checked = false;
            this.dateEndFilter.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            this.dateEndFilter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateEndFilter.Location = new System.Drawing.Point(71, 44);
            this.dateEndFilter.Name = "dateEndFilter";
            this.dateEndFilter.ShowCheckBox = true;
            this.dateEndFilter.ShowUpDown = true;
            this.dateEndFilter.Size = new System.Drawing.Size(159, 23);
            this.dateEndFilter.TabIndex = 21;
            // 
            // dateStartFilter
            // 
            this.dateStartFilter.Checked = false;
            this.dateStartFilter.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            this.dateStartFilter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateStartFilter.Location = new System.Drawing.Point(71, 15);
            this.dateStartFilter.Name = "dateStartFilter";
            this.dateStartFilter.ShowCheckBox = true;
            this.dateStartFilter.ShowUpDown = true;
            this.dateStartFilter.Size = new System.Drawing.Size(159, 23);
            this.dateStartFilter.TabIndex = 20;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 18);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(58, 15);
            this.label8.TabIndex = 22;
            this.label8.Text = "Date Start";
            // 
            // buttonFilter
            // 
            this.buttonFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFilter.Image = global::LogsReader.Properties.Resources.filter17;
            this.buttonFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonFilter.Location = new System.Drawing.Point(972, 15);
            this.buttonFilter.Name = "buttonFilter";
            this.buttonFilter.Padding = new System.Windows.Forms.Padding(3, 0, 2, 0);
            this.buttonFilter.Size = new System.Drawing.Size(85, 23);
            this.buttonFilter.TabIndex = 28;
            this.buttonFilter.Text = "Filter [F7]";
            this.buttonFilter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonFilter.UseVisualStyleBackColor = true;
            this.buttonFilter.Click += new System.EventHandler(this.buttonFilter_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReset.Image = global::LogsReader.Properties.Resources.reset2;
            this.buttonReset.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonReset.Location = new System.Drawing.Point(972, 44);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(85, 23);
            this.buttonReset.TabIndex = 29;
            this.buttonReset.Text = "Reset [F8]";
            this.buttonReset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(236, 48);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 15);
            this.label11.TabIndex = 30;
            this.label11.Text = "Trace";
            // 
            // traceMessageFilter
            // 
            this.traceMessageFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.traceMessageFilter.Location = new System.Drawing.Point(407, 44);
            this.traceMessageFilter.Name = "traceMessageFilter";
            this.traceMessageFilter.Size = new System.Drawing.Size(559, 23);
            this.traceMessageFilter.TabIndex = 31;
            this.traceMessageFilter.TextChanged += new System.EventHandler(this.TraceMessageFilter_TextChanged);
            // 
            // groupBoxFilter
            // 
            this.groupBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.groupBoxFilter.Location = new System.Drawing.Point(2, 20);
            this.groupBoxFilter.Name = "groupBoxFilter";
            this.groupBoxFilter.Size = new System.Drawing.Size(1246, 75);
            this.groupBoxFilter.TabIndex = 28;
            this.groupBoxFilter.TabStop = false;
            // 
            // traceMessageFilterComboBox
            // 
            this.traceMessageFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.traceMessageFilterComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.traceMessageFilterComboBox.FormattingEnabled = true;
            this.traceMessageFilterComboBox.Items.AddRange(new object[] {
            "Contains",
            "Not Contains"});
            this.traceMessageFilterComboBox.Location = new System.Drawing.Point(306, 45);
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
            this.traceNameFilterComboBox.Location = new System.Drawing.Point(306, 16);
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
            this.alreadyUseFilter.AutoSize = true;
            this.alreadyUseFilter.Location = new System.Drawing.Point(1065, 48);
            this.alreadyUseFilter.Name = "alreadyUseFilter";
            this.alreadyUseFilter.Size = new System.Drawing.Size(158, 19);
            this.alreadyUseFilter.TabIndex = 33;
            this.alreadyUseFilter.Text = "Use filter when searching";
            this.alreadyUseFilter.UseVisualStyleBackColor = true;
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExport.Image = global::LogsReader.Properties.Resources.save2;
            this.buttonExport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonExport.Location = new System.Drawing.Point(1063, 15);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.buttonExport.Size = new System.Drawing.Size(112, 23);
            this.buttonExport.TabIndex = 32;
            this.buttonExport.Text = "Export [Ctrl+S]";
            this.buttonExport.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.ButtonExport_Click);
            // 
            // filterPanel
            // 
            this.filterPanel.BackColor = System.Drawing.SystemColors.Control;
            this.filterPanel.Controls.Add(this.txtPattern);
            this.filterPanel.Controls.Add(this.useRegex);
            this.filterPanel.Controls.Add(this.btnSearch);
            this.filterPanel.Controls.Add(this.btnClear);
            this.filterPanel.Controls.Add(this.groupBoxFilter);
            this.filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterPanel.Location = new System.Drawing.Point(0, 0);
            this.filterPanel.MinimumSize = new System.Drawing.Size(871, 95);
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Size = new System.Drawing.Size(1251, 95);
            this.filterPanel.TabIndex = 30;
            // 
            // ParentRadSplitContainer
            // 
            this.ParentRadSplitContainer.Controls.Add(this.splitPanel1);
            this.ParentRadSplitContainer.Controls.Add(this.splitPanel2);
            this.ParentRadSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ParentRadSplitContainer.Location = new System.Drawing.Point(0, 95);
            this.ParentRadSplitContainer.Name = "ParentRadSplitContainer";
            // 
            // 
            // 
            this.ParentRadSplitContainer.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.ParentRadSplitContainer.Size = new System.Drawing.Size(1251, 710);
            this.ParentRadSplitContainer.TabIndex = 32;
            this.ParentRadSplitContainer.TabStop = false;
            // 
            // splitPanel1
            // 
            this.splitPanel1.Controls.Add(this.MainRadSplitContainer);
            this.splitPanel1.Controls.Add(this.radStatusStrip);
            this.splitPanel1.Location = new System.Drawing.Point(0, 0);
            this.splitPanel1.Name = "splitPanel1";
            // 
            // 
            // 
            this.splitPanel1.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel1.Size = new System.Drawing.Size(648, 710);
            this.splitPanel1.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(0.01995164F, 0F);
            this.splitPanel1.SizeInfo.SplitterCorrection = new System.Drawing.Size(17, 0);
            this.splitPanel1.TabIndex = 0;
            this.splitPanel1.TabStop = false;
            this.splitPanel1.Text = "splitPanel1";
            // 
            // MainRadSplitContainer
            // 
            this.MainRadSplitContainer.Controls.Add(this.splitPanel3);
            this.MainRadSplitContainer.Controls.Add(this.splitPanel4);
            this.MainRadSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainRadSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainRadSplitContainer.Name = "MainRadSplitContainer";
            // 
            // 
            // 
            this.MainRadSplitContainer.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.MainRadSplitContainer.Size = new System.Drawing.Size(648, 684);
            this.MainRadSplitContainer.TabIndex = 0;
            this.MainRadSplitContainer.TabStop = false;
            // 
            // splitPanel3
            // 
            this.splitPanel3.Controls.Add(this.label12);
            this.splitPanel3.Controls.Add(this.trvMain);
            this.splitPanel3.Controls.Add(this.orderByText);
            this.splitPanel3.Controls.Add(this.label1);
            this.splitPanel3.Controls.Add(this.label2);
            this.splitPanel3.Controls.Add(this.maxThreadsText);
            this.splitPanel3.Controls.Add(this.rowsLimitText);
            this.splitPanel3.Controls.Add(this.label4);
            this.splitPanel3.Controls.Add(this.label5);
            this.splitPanel3.Controls.Add(this.maxLinesStackText);
            this.splitPanel3.Controls.Add(this.fileNames);
            this.splitPanel3.Controls.Add(this.serversText);
            this.splitPanel3.Controls.Add(this.logDirText);
            this.splitPanel3.Controls.Add(this.label6);
            this.splitPanel3.Controls.Add(this.label3);
            this.splitPanel3.Location = new System.Drawing.Point(0, 0);
            this.splitPanel3.Name = "splitPanel3";
            // 
            // 
            // 
            this.splitPanel3.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel3.Size = new System.Drawing.Size(223, 684);
            this.splitPanel3.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(-0.1541176F, 0F);
            this.splitPanel3.SizeInfo.SplitterCorrection = new System.Drawing.Size(-67, 0);
            this.splitPanel3.TabIndex = 0;
            this.splitPanel3.TabStop = false;
            this.splitPanel3.Text = "splitPanel3";
            // 
            // splitPanel4
            // 
            this.splitPanel4.Controls.Add(this.EnumRadSplitContainer);
            this.splitPanel4.Location = new System.Drawing.Point(227, 0);
            this.splitPanel4.Name = "splitPanel4";
            // 
            // 
            // 
            this.splitPanel4.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel4.Size = new System.Drawing.Size(421, 684);
            this.splitPanel4.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(0.1541176F, 0F);
            this.splitPanel4.SizeInfo.SplitterCorrection = new System.Drawing.Size(67, 0);
            this.splitPanel4.TabIndex = 1;
            this.splitPanel4.TabStop = false;
            this.splitPanel4.Text = "splitPanel4";
            // 
            // EnumRadSplitContainer
            // 
            this.EnumRadSplitContainer.Controls.Add(this.splitPanel5);
            this.EnumRadSplitContainer.Controls.Add(this.splitPanel6);
            this.EnumRadSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EnumRadSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.EnumRadSplitContainer.Name = "EnumRadSplitContainer";
            this.EnumRadSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // 
            // 
            this.EnumRadSplitContainer.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.EnumRadSplitContainer.Size = new System.Drawing.Size(421, 684);
            this.EnumRadSplitContainer.TabIndex = 0;
            this.EnumRadSplitContainer.TabStop = false;
            // 
            // splitPanel5
            // 
            this.splitPanel5.Controls.Add(this.dgvFiles);
            this.splitPanel5.Location = new System.Drawing.Point(0, 0);
            this.splitPanel5.Name = "splitPanel5";
            // 
            // 
            // 
            this.splitPanel5.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel5.Size = new System.Drawing.Size(421, 535);
            this.splitPanel5.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(0F, 0.2861272F);
            this.splitPanel5.SizeInfo.SplitterCorrection = new System.Drawing.Size(0, 99);
            this.splitPanel5.TabIndex = 0;
            this.splitPanel5.TabStop = false;
            this.splitPanel5.Text = "splitPanel5";
            // 
            // splitPanel6
            // 
            this.splitPanel6.Controls.Add(this.descriptionText);
            this.splitPanel6.Location = new System.Drawing.Point(0, 539);
            this.splitPanel6.Name = "splitPanel6";
            // 
            // 
            // 
            this.splitPanel6.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel6.Size = new System.Drawing.Size(421, 145);
            this.splitPanel6.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(0F, -0.2861272F);
            this.splitPanel6.SizeInfo.SplitterCorrection = new System.Drawing.Size(0, -99);
            this.splitPanel6.TabIndex = 1;
            this.splitPanel6.TabStop = false;
            this.splitPanel6.Text = "splitPanel6";
            // 
            // radStatusStrip
            // 
            this.radStatusStrip.Location = new System.Drawing.Point(0, 684);
            this.radStatusStrip.Name = "radStatusStrip";
            this.radStatusStrip.Size = new System.Drawing.Size(648, 26);
            this.radStatusStrip.SizingGrip = false;
            this.radStatusStrip.TabIndex = 1;
            // 
            // splitPanel2
            // 
            this.splitPanel2.Location = new System.Drawing.Point(652, 0);
            this.splitPanel2.Name = "splitPanel2";
            // 
            // 
            // 
            this.splitPanel2.RootElement.MinSize = new System.Drawing.Size(25, 25);
            this.splitPanel2.Size = new System.Drawing.Size(599, 710);
            this.splitPanel2.SizeInfo.AutoSizeScale = new System.Drawing.SizeF(-0.01995164F, 0F);
            this.splitPanel2.SizeInfo.SplitterCorrection = new System.Drawing.Size(-35, 0);
            this.splitPanel2.TabIndex = 1;
            this.splitPanel2.TabStop = false;
            this.splitPanel2.Text = "splitPanel2";
            // 
            // LogsReaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ParentRadSplitContainer);
            this.Controls.Add(this.filterPanel);
            this.Controls.Add(this.progressBar);
            this.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.MinimumSize = new System.Drawing.Size(0, 25);
            this.Name = "LogsReaderForm";
            this.Size = new System.Drawing.Size(1251, 815);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).EndInit();
            this.groupBoxFilter.ResumeLayout(false);
            this.groupBoxFilter.PerformLayout();
            this.filterPanel.ResumeLayout(false);
            this.filterPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ParentRadSplitContainer)).EndInit();
            this.ParentRadSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel1)).EndInit();
            this.splitPanel1.ResumeLayout(false);
            this.splitPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainRadSplitContainer)).EndInit();
            this.MainRadSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel3)).EndInit();
            this.splitPanel3.ResumeLayout(false);
            this.splitPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel4)).EndInit();
            this.splitPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.EnumRadSplitContainer)).EndInit();
            this.EnumRadSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel5)).EndInit();
            this.splitPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel6)).EndInit();
            this.splitPanel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radStatusStrip)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridView dgvFiles;
        private Utils.WinForm.CustomTreeView trvMain;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TextBox txtPattern;
        private System.Windows.Forms.ProgressBar progressBar;
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
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox traceNameFilter;
        private System.Windows.Forms.DateTimePicker dateEndFilter;
        private System.Windows.Forms.DateTimePicker dateStartFilter;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button buttonFilter;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox traceMessageFilter;
        private System.Windows.Forms.GroupBox groupBoxFilter;
        private System.Windows.Forms.RichTextBox descriptionText;
        private System.Windows.Forms.Panel filterPanel;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.CheckBox alreadyUseFilter;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox rowsLimitText;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox orderByText;
        private System.Windows.Forms.ComboBox traceNameFilterComboBox;
        private System.Windows.Forms.ComboBox traceMessageFilterComboBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn PrivateID;
        private System.Windows.Forms.DataGridViewTextBoxColumn IsMatched;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Server;
        private System.Windows.Forms.DataGridViewTextBoxColumn TraceName;
        private System.Windows.Forms.DataGridViewTextBoxColumn DateOfTrace;
        private System.Windows.Forms.DataGridViewTextBoxColumn File;
        private Telerik.WinControls.UI.RadSplitContainer ParentRadSplitContainer;
        private Telerik.WinControls.UI.SplitPanel splitPanel1;
        private Telerik.WinControls.UI.SplitPanel splitPanel2;
        private Telerik.WinControls.UI.RadSplitContainer MainRadSplitContainer;
        private Telerik.WinControls.UI.SplitPanel splitPanel3;
        private Telerik.WinControls.UI.SplitPanel splitPanel4;
        private Telerik.WinControls.UI.RadStatusStrip radStatusStrip;
        private Telerik.WinControls.UI.RadSplitContainer EnumRadSplitContainer;
        private Telerik.WinControls.UI.SplitPanel splitPanel5;
        private Telerik.WinControls.UI.SplitPanel splitPanel6;
    }
}

