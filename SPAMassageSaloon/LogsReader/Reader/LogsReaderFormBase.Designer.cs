
using System.ComponentModel;
using System.Windows.Forms;
using SPAMassageSaloon.Common;
using Utils.WinForm;
using Utils.WinForm.Notepad;

namespace LogsReader.Reader
{
    partial class LogsReaderFormBase
	{
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogsReaderFormBase));
			this.DgvData = new SPAMassageSaloon.Common.CustomDataGridView();
			this.SchemeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.PrivateID = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.IsMatched = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Server = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.TraceName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DateOfTrace = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.File = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.BtnSearch = new System.Windows.Forms.Button();
			this.btnClear = new System.Windows.Forms.Button();
			this.TbxPattern = new System.Windows.Forms.TextBox();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.EnumSplitContainer = new System.Windows.Forms.SplitContainer();
			this.descriptionText = new System.Windows.Forms.RichTextBox();
			this.ChbxUseRegex = new System.Windows.Forms.CheckBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.TbxTraceNameFilter = new System.Windows.Forms.TextBox();
			this.DateEndFilter = new System.Windows.Forms.DateTimePicker();
			this.DateStartFilter = new System.Windows.Forms.DateTimePicker();
			this.label8 = new System.Windows.Forms.Label();
			this.btnFilter = new System.Windows.Forms.Button();
			this.btnReset = new System.Windows.Forms.Button();
			this.label11 = new System.Windows.Forms.Label();
			this.TbxTraceMessageFilter = new System.Windows.Forms.TextBox();
			this.filterPanel = new System.Windows.Forms.Panel();
			this.CobxTraceMessageFilter = new System.Windows.Forms.ComboBox();
			this.CobxTraceNameFilter = new System.Windows.Forms.ComboBox();
			this.ChbxAlreadyUseFilter = new System.Windows.Forms.CheckBox();
			this.btnExport = new System.Windows.Forms.Button();
			this.searchPanel = new System.Windows.Forms.Panel();
			this.ParentSplitContainer = new System.Windows.Forms.SplitContainer();
			this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
			this.notepad = new Utils.WinForm.Notepad.NotepadControl();
			((System.ComponentModel.ISupportInitialize)(this.DgvData)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EnumSplitContainer)).BeginInit();
			this.EnumSplitContainer.Panel1.SuspendLayout();
			this.EnumSplitContainer.Panel2.SuspendLayout();
			this.EnumSplitContainer.SuspendLayout();
			this.filterPanel.SuspendLayout();
			this.searchPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentSplitContainer)).BeginInit();
			this.ParentSplitContainer.Panel1.SuspendLayout();
			this.ParentSplitContainer.Panel2.SuspendLayout();
			this.ParentSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// dgvFiles
			// 
			this.DgvData.AllowUserToAddRows = false;
			this.DgvData.AllowUserToDeleteRows = false;
			this.DgvData.AllowUserToResizeRows = false;
			this.DgvData.BackgroundColor = System.Drawing.SystemColors.Window;
			this.DgvData.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.DgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.DgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SchemeName,
            this.PrivateID,
            this.IsMatched,
            this.ID,
            this.Server,
            this.TraceName,
            this.DateOfTrace,
            this.File});
			this.DgvData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DgvData.GridColor = System.Drawing.SystemColors.ControlLight;
			this.DgvData.Location = new System.Drawing.Point(0, 0);
			this.DgvData.Name = "dgvFiles";
			this.DgvData.ReadOnly = true;
			this.DgvData.RowHeadersVisible = false;
			this.DgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.DgvData.Size = new System.Drawing.Size(459, 410);
			this.DgvData.TabIndex = 20;
			this.DgvData.SelectionChanged += new System.EventHandler(this.DgvData_SelectionChanged);
			this.DgvData.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DgvData_MouseDown);
			// 
			// SchemeName
			// 
			this.SchemeName.DataPropertyName = "SchemeName";
			this.SchemeName.HeaderText = "SchemeName";
			this.SchemeName.MinimumWidth = 5;
			this.SchemeName.Name = "SchemeName";
			this.SchemeName.ReadOnly = true;
			this.SchemeName.Visible = false;
			this.SchemeName.Width = 5;
			this.SchemeName.SortMode = DataGridViewColumnSortMode.Programmatic;
			// 
			// PrivateID
			// 
			this.PrivateID.DataPropertyName = "PrivateID";
			this.PrivateID.HeaderText = "PrivateID";
			this.PrivateID.MinimumWidth = 5;
			this.PrivateID.Name = "PrivateID";
			this.PrivateID.ReadOnly = true;
			this.PrivateID.Visible = false;
			this.PrivateID.Width = 5;
			this.PrivateID.SortMode = DataGridViewColumnSortMode.Programmatic;
			// 
			// IsMatched
			// 
			this.IsMatched.DataPropertyName = "IsMatched";
			this.IsMatched.HeaderText = "IsMatched";
			this.IsMatched.MinimumWidth = 5;
			this.IsMatched.Name = "IsMatched";
			this.IsMatched.ReadOnly = true;
			this.IsMatched.Visible = false;
			this.IsMatched.Width = 5;
			this.IsMatched.SortMode = DataGridViewColumnSortMode.Programmatic;
			// 
			// ID
			// 
			this.ID.DataPropertyName = "ID";
			this.ID.HeaderText = "ID";
			this.ID.MinimumWidth = 35;
			this.ID.Name = "ID";
			this.ID.ReadOnly = true;
			this.ID.Width = 35;
			this.ID.SortMode = DataGridViewColumnSortMode.Programmatic;
			// 
			// Server
			// 
			this.Server.DataPropertyName = "Server";
			this.Server.HeaderText = "Server";
			this.Server.MinimumWidth = 48;
			this.Server.Name = "Server";
			this.Server.ReadOnly = true;
			this.Server.Width = 50;
			this.Server.SortMode = DataGridViewColumnSortMode.Programmatic;
			// 
			// TraceName
			// 
			this.TraceName.DataPropertyName = "TraceName";
			this.TraceName.HeaderText = "TraceName";
			this.TraceName.MinimumWidth = 75;
			this.TraceName.Name = "TraceName";
			this.TraceName.ReadOnly = true;
			this.TraceName.Width = 80;
			this.TraceName.SortMode = DataGridViewColumnSortMode.Programmatic;
			// 
			// DateOfTrace
			// 
			this.DateOfTrace.DataPropertyName = "DateOfTrace";
			this.DateOfTrace.HeaderText = "Date";
			this.DateOfTrace.MinimumWidth = 40;
			this.DateOfTrace.Name = "DateOfTrace";
			this.DateOfTrace.ReadOnly = true;
			this.DateOfTrace.Width = 140;
			this.DateOfTrace.SortMode = DataGridViewColumnSortMode.Programmatic;
			// 
			// File
			// 
			this.File.DataPropertyName = "FileNamePartial";
			this.File.HeaderText = "File";
			this.File.MinimumWidth = 40;
			this.File.Name = "FileNamePartial";
			this.File.ReadOnly = true;
			this.File.Width = 1000;
			this.File.SortMode = DataGridViewColumnSortMode.Programmatic;
			// 
			// BTNSearch
			// 
			this.BtnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BtnSearch.BackColor = System.Drawing.SystemColors.Control;
			this.BtnSearch.Image = global::LogsReader.Properties.Resources.find;
			this.BtnSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.BtnSearch.Location = new System.Drawing.Point(943, 2);
			this.BtnSearch.Name = "BTNSearch";
			this.BtnSearch.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
			this.BtnSearch.Size = new System.Drawing.Size(90, 24);
			this.BtnSearch.TabIndex = 3;
			this.BtnSearch.Text = global::LogsReader.Properties.Resources.Txt_LogsReaderForm_Search;
			this.BtnSearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.BtnSearch.UseVisualStyleBackColor = true;
			this.BtnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
			// 
			// btnClear
			// 
			this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClear.Image = global::LogsReader.Properties.Resources.clear1;
			this.btnClear.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnClear.Location = new System.Drawing.Point(1039, 2);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(90, 24);
			this.btnClear.TabIndex = 4;
			this.btnClear.Text = "Clear [F6]";
			this.btnClear.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
			// 
			// txtPattern
			// 
			this.TbxPattern.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TbxPattern.Location = new System.Drawing.Point(8, 3);
			this.TbxPattern.Name = "txtPattern";
			this.TbxPattern.Size = new System.Drawing.Size(840, 23);
			this.TbxPattern.TabIndex = 1;
			this.TbxPattern.TextChanged += new System.EventHandler(this.TxtPatternOnTextChanged);
			// 
			// progressBar
			// 
			this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.progressBar.Location = new System.Drawing.Point(0, 584);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(1152, 10);
			this.progressBar.TabIndex = 6;
			// 
			// statusStrip
			// 
			this.statusStrip.GripMargin = new System.Windows.Forms.Padding(1);
			this.statusStrip.Location = new System.Drawing.Point(0, 470);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(680, 22);
			this.statusStrip.SizingGrip = false;
			this.statusStrip.TabIndex = 8;
			this.statusStrip.Text = "statusStrip1";
			// 
			// EnumSplitContainer
			// 
			this.EnumSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.EnumSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EnumSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.EnumSplitContainer.Location = new System.Drawing.Point(0, 0);
			this.EnumSplitContainer.Name = "EnumSplitContainer";
			this.EnumSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// EnumSplitContainer.Panel1
			// 
			this.EnumSplitContainer.Panel1.Controls.Add(this.DgvData);
			// 
			// EnumSplitContainer.Panel2
			// 
			this.EnumSplitContainer.Panel2.Controls.Add(this.descriptionText);
			this.EnumSplitContainer.Size = new System.Drawing.Size(463, 470);
			this.EnumSplitContainer.SplitterDistance = 414;
			this.EnumSplitContainer.TabIndex = 2;
			// 
			// descriptionText
			// 
			this.descriptionText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.descriptionText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.descriptionText.Location = new System.Drawing.Point(0, 0);
			this.descriptionText.Name = "descriptionText";
			this.descriptionText.ReadOnly = true;
			this.descriptionText.Size = new System.Drawing.Size(459, 48);
			this.descriptionText.TabIndex = 21;
			this.descriptionText.Text = "";
			// 
			// useRegex
			// 
			this.ChbxUseRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ChbxUseRegex.AutoSize = true;
			this.ChbxUseRegex.Location = new System.Drawing.Point(858, 6);
			this.ChbxUseRegex.Name = "useRegex";
			this.ChbxUseRegex.Size = new System.Drawing.Size(79, 19);
			this.ChbxUseRegex.TabIndex = 2;
			this.ChbxUseRegex.Text = global::LogsReader.Properties.Resources.Txt_LogsReaderForm_UseRegex;
			this.ChbxUseRegex.UseVisualStyleBackColor = true;
			this.ChbxUseRegex.CheckedChanged += new System.EventHandler(this.ChbxUseRegex_CheckedChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(225, 11);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(67, 15);
			this.label7.TabIndex = 24;
			this.label7.Text = "TraceName";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(4, 37);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(54, 15);
			this.label9.TabIndex = 23;
			this.label9.Text = "Date End";
			// 
			// traceNameFilter
			// 
			this.TbxTraceNameFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TbxTraceNameFilter.Location = new System.Drawing.Point(403, 7);
			this.TbxTraceNameFilter.Name = "traceNameFilter";
			this.TbxTraceNameFilter.Size = new System.Drawing.Size(444, 23);
			this.TbxTraceNameFilter.TabIndex = 9;
			this.TbxTraceNameFilter.TextChanged += new System.EventHandler(this.TbxTraceNameFilterOnTextChanged);
			// 
			// dateEndFilter
			// 
			this.DateEndFilter.Checked = false;
			this.DateEndFilter.CustomFormat = "dd.MM.yyyy HH:mm:ss";
			this.DateEndFilter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.DateEndFilter.Location = new System.Drawing.Point(68, 34);
			this.DateEndFilter.Name = "dateEndFilter";
			this.DateEndFilter.ShowCheckBox = true;
			this.DateEndFilter.ShowUpDown = true;
			this.DateEndFilter.Size = new System.Drawing.Size(151, 23);
			this.DateEndFilter.TabIndex = 6;
			// 
			// dateStartFilter
			// 
			this.DateStartFilter.Checked = false;
			this.DateStartFilter.CustomFormat = "dd.MM.yyyy HH:mm:ss";
			this.DateStartFilter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.DateStartFilter.Location = new System.Drawing.Point(68, 7);
			this.DateStartFilter.Name = "dateStartFilter";
			this.DateStartFilter.ShowCheckBox = true;
			this.DateStartFilter.ShowUpDown = true;
			this.DateStartFilter.Size = new System.Drawing.Size(151, 23);
			this.DateStartFilter.TabIndex = 5;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(4, 11);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(58, 15);
			this.label8.TabIndex = 22;
			this.label8.Text = "Date Start";
			// 
			// buttonFilter
			// 
			this.btnFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFilter.Image = global::LogsReader.Properties.Resources.filter;
			this.btnFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnFilter.Location = new System.Drawing.Point(853, 7);
			this.btnFilter.Name = "buttonFilter";
			this.btnFilter.Padding = new System.Windows.Forms.Padding(5, 0, 7, 0);
			this.btnFilter.Size = new System.Drawing.Size(100, 23);
			this.btnFilter.TabIndex = 11;
			this.btnFilter.Text = "Filter [F7]";
			this.btnFilter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btnFilter.UseVisualStyleBackColor = true;
			this.btnFilter.Click += new System.EventHandler(this.BtnFilter_Click);
			// 
			// buttonReset
			// 
			this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnReset.Image = global::LogsReader.Properties.Resources.reset2;
			this.btnReset.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnReset.Location = new System.Drawing.Point(853, 34);
			this.btnReset.Name = "buttonReset";
			this.btnReset.Padding = new System.Windows.Forms.Padding(2, 0, 7, 0);
			this.btnReset.Size = new System.Drawing.Size(100, 23);
			this.btnReset.TabIndex = 12;
			this.btnReset.Text = global::LogsReader.Properties.Resources.Txt_LogsReaderForm_Reset;
			this.btnReset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btnReset.UseVisualStyleBackColor = true;
			this.btnReset.Click += new System.EventHandler(this.BtnReset_Click);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(225, 37);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(35, 15);
			this.label11.TabIndex = 30;
			this.label11.Text = "Trace";
			// 
			// traceMessageFilter
			// 
			this.TbxTraceMessageFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TbxTraceMessageFilter.Location = new System.Drawing.Point(403, 34);
			this.TbxTraceMessageFilter.Name = "traceMessageFilter";
			this.TbxTraceMessageFilter.Size = new System.Drawing.Size(444, 23);
			this.TbxTraceMessageFilter.TabIndex = 10;
			this.TbxTraceMessageFilter.TextChanged += new System.EventHandler(this.TbxTraceMessageFilterOnTextChanged);
			// 
			// filterPanel
			// 
			this.filterPanel.BackColor = System.Drawing.SystemColors.Control;
			this.filterPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.filterPanel.Controls.Add(this.CobxTraceMessageFilter);
			this.filterPanel.Controls.Add(this.CobxTraceNameFilter);
			this.filterPanel.Controls.Add(this.ChbxAlreadyUseFilter);
			this.filterPanel.Controls.Add(this.btnExport);
			this.filterPanel.Controls.Add(this.TbxTraceMessageFilter);
			this.filterPanel.Controls.Add(this.label11);
			this.filterPanel.Controls.Add(this.btnReset);
			this.filterPanel.Controls.Add(this.btnFilter);
			this.filterPanel.Controls.Add(this.label8);
			this.filterPanel.Controls.Add(this.DateStartFilter);
			this.filterPanel.Controls.Add(this.DateEndFilter);
			this.filterPanel.Controls.Add(this.TbxTraceNameFilter);
			this.filterPanel.Controls.Add(this.label9);
			this.filterPanel.Controls.Add(this.label7);
			this.filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.filterPanel.Location = new System.Drawing.Point(0, 28);
			this.filterPanel.MinimumSize = new System.Drawing.Size(850, 2);
			this.filterPanel.Name = "filterPanel";
			this.filterPanel.Size = new System.Drawing.Size(1152, 64);
			this.filterPanel.TabIndex = 28;
			// 
			// traceMessageFilterComboBox
			// 
			this.CobxTraceMessageFilter.BackColor = System.Drawing.SystemColors.Window;
			this.CobxTraceMessageFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CobxTraceMessageFilter.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.CobxTraceMessageFilter.FormattingEnabled = true;
			this.CobxTraceMessageFilter.Location = new System.Drawing.Point(298, 35);
			this.CobxTraceMessageFilter.MaxDropDownItems = 2;
			this.CobxTraceMessageFilter.Name = "traceMessageFilterComboBox";
			this.CobxTraceMessageFilter.Size = new System.Drawing.Size(102, 21);
			this.CobxTraceMessageFilter.TabIndex = 8;
			this.CobxTraceMessageFilter.SelectedIndexChanged += new System.EventHandler(this.CobxTraceMessageFilter_SelectedIndexChanged);
			this.CobxTraceMessageFilter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ComboBox_KeyPress);
			// 
			// traceNameFilterComboBox
			// 
			this.CobxTraceNameFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CobxTraceNameFilter.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.CobxTraceNameFilter.FormattingEnabled = true;
			this.CobxTraceNameFilter.Location = new System.Drawing.Point(298, 8);
			this.CobxTraceNameFilter.MaxDropDownItems = 2;
			this.CobxTraceNameFilter.Name = "traceNameFilterComboBox";
			this.CobxTraceNameFilter.Size = new System.Drawing.Size(102, 21);
			this.CobxTraceNameFilter.TabIndex = 7;
			this.CobxTraceNameFilter.SelectedIndexChanged += new System.EventHandler(this.CobxTraceNameFilter_SelectedIndexChanged);
			this.CobxTraceNameFilter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ComboBox_KeyPress);
			// 
			// alreadyUseFilter
			// 
			this.ChbxAlreadyUseFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ChbxAlreadyUseFilter.AutoSize = true;
			this.ChbxAlreadyUseFilter.Font = new System.Drawing.Font("Segoe UI", 8.5F);
			this.ChbxAlreadyUseFilter.Location = new System.Drawing.Point(962, 37);
			this.ChbxAlreadyUseFilter.Name = "alreadyUseFilter";
			this.ChbxAlreadyUseFilter.Size = new System.Drawing.Size(158, 19);
			this.ChbxAlreadyUseFilter.TabIndex = 14;
			this.ChbxAlreadyUseFilter.Text = "Use filter when searching";
			this.ChbxAlreadyUseFilter.UseVisualStyleBackColor = true;
			this.ChbxAlreadyUseFilter.CheckedChanged += new System.EventHandler(this.ChbxAlreadyUseFilter_CheckedChanged);
			// 
			// buttonExport
			// 
			this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExport.Image = global::LogsReader.Properties.Resources.save2;
			this.btnExport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnExport.Location = new System.Drawing.Point(959, 7);
			this.btnExport.Name = "buttonExport";
			this.btnExport.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.btnExport.Size = new System.Drawing.Size(110, 23);
			this.btnExport.TabIndex = 13;
			this.btnExport.Text = "Export [Ctrl+S]";
			this.btnExport.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btnExport.UseVisualStyleBackColor = true;
			this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);
			// 
			// searchPanel
			// 
			this.searchPanel.BackColor = System.Drawing.SystemColors.Control;
			this.searchPanel.Controls.Add(this.TbxPattern);
			this.searchPanel.Controls.Add(this.ChbxUseRegex);
			this.searchPanel.Controls.Add(this.BtnSearch);
			this.searchPanel.Controls.Add(this.btnClear);
			this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.searchPanel.Location = new System.Drawing.Point(0, 0);
			this.searchPanel.MinimumSize = new System.Drawing.Size(850, 0);
			this.searchPanel.Name = "searchPanel";
			this.searchPanel.Size = new System.Drawing.Size(1152, 28);
			this.searchPanel.TabIndex = 30;
			// 
			// ParentSplitContainer
			// 
			this.ParentSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ParentSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParentSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.ParentSplitContainer.Location = new System.Drawing.Point(0, 92);
			this.ParentSplitContainer.Name = "ParentSplitContainer";
			// 
			// ParentSplitContainer.Panel1
			// 
			this.ParentSplitContainer.Panel1.Controls.Add(this.MainSplitContainer);
			this.ParentSplitContainer.Panel1.Controls.Add(this.statusStrip);
			this.ParentSplitContainer.Panel1MinSize = 250;
			// 
			// ParentSplitContainer.Panel2
			// 
			this.ParentSplitContainer.Panel2.Controls.Add(this.notepad);
			this.ParentSplitContainer.Size = new System.Drawing.Size(1152, 492);
			this.ParentSplitContainer.SplitterDistance = 680;
			this.ParentSplitContainer.TabIndex = 32;
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Panel1MinSize = 60;
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.EnumSplitContainer);
			this.MainSplitContainer.Size = new System.Drawing.Size(680, 470);
			this.MainSplitContainer.SplitterDistance = 213;
			this.MainSplitContainer.TabIndex = 0;
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
			this.notepad.Size = new System.Drawing.Size(464, 488);
			this.notepad.SizingGrip = false;
			this.notepad.TabIndex = 0;
			this.notepad.TabsFont = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.notepad.TabsForeColor = System.Drawing.Color.Green;
			this.notepad.TextFont = new System.Drawing.Font("Segoe UI", 10F);
			this.notepad.TextForeColor = System.Drawing.Color.Black;
			this.notepad.WordWrap = true;
			// 
			// LogsReaderFormBase
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ParentSplitContainer);
			this.Controls.Add(this.filterPanel);
			this.Controls.Add(this.searchPanel);
			this.Controls.Add(this.progressBar);
			this.Font = new System.Drawing.Font("Segoe UI", 8.5F);
			this.MinimumSize = new System.Drawing.Size(0, 25);
			this.Name = "LogsReaderFormBase";
			this.Size = new System.Drawing.Size(1152, 594);
			((System.ComponentModel.ISupportInitialize)(this.DgvData)).EndInit();
			this.EnumSplitContainer.Panel1.ResumeLayout(false);
			this.EnumSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.EnumSplitContainer)).EndInit();
			this.EnumSplitContainer.ResumeLayout(false);
			this.filterPanel.ResumeLayout(false);
			this.filterPanel.PerformLayout();
			this.searchPanel.ResumeLayout(false);
			this.searchPanel.PerformLayout();
			this.ParentSplitContainer.Panel1.ResumeLayout(false);
			this.ParentSplitContainer.Panel1.PerformLayout();
			this.ParentSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ParentSplitContainer)).EndInit();
			this.ParentSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion
        private Button btnClear;
        private ProgressBar progressBar;
        private StatusStrip statusStrip;
        private Label label7;
        private Label label9;
        private Label label8;
        private Button btnFilter;
        private Button btnReset;
        private Label label11;
        private Panel filterPanel;
        private SplitContainer EnumSplitContainer;
        private RichTextBox descriptionText;
        private Panel searchPanel;
        private Button btnExport;
        private DataGridViewTextBoxColumn SchemeName;
        private DataGridViewTextBoxColumn PrivateID;
        private DataGridViewTextBoxColumn IsMatched;
        private DataGridViewTextBoxColumn ID;
        private DataGridViewTextBoxColumn Server;
        private DataGridViewTextBoxColumn TraceName;
        private DataGridViewTextBoxColumn DateOfTrace;
        private DataGridViewTextBoxColumn File;
        private SplitContainer ParentSplitContainer;
        private NotepadControl notepad;

        protected CustomDataGridView DgvData;
        protected SplitContainer MainSplitContainer;

        internal Button BtnSearch;
        internal TextBox TbxPattern;
        internal CheckBox ChbxUseRegex;
        internal DateTimePicker DateStartFilter;
        internal DateTimePicker DateEndFilter;
        internal ComboBox CobxTraceNameFilter;
        internal ComboBox CobxTraceMessageFilter;
        internal TextBox TbxTraceNameFilter;
        internal TextBox TbxTraceMessageFilter;
        internal CheckBox ChbxAlreadyUseFilter;
	}
}

