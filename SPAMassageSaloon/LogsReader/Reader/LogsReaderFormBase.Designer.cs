
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
			this.dgvFiles = new SPAMassageSaloon.Common.CustomDataGridView();
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
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.EnumSplitContainer = new System.Windows.Forms.SplitContainer();
			this.descriptionText = new System.Windows.Forms.RichTextBox();
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
			this.filterPanel = new System.Windows.Forms.Panel();
			this.traceMessageFilterComboBox = new System.Windows.Forms.ComboBox();
			this.traceNameFilterComboBox = new System.Windows.Forms.ComboBox();
			this.alreadyUseFilter = new System.Windows.Forms.CheckBox();
			this.buttonExport = new System.Windows.Forms.Button();
			this.searchPanel = new System.Windows.Forms.Panel();
			this.ParentSplitContainer = new System.Windows.Forms.SplitContainer();
			this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
			this.notepad = new Utils.WinForm.Notepad.NotepadControl();
			((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).BeginInit();
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
			this.dgvFiles.AllowUserToAddRows = false;
			this.dgvFiles.AllowUserToDeleteRows = false;
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
			this.dgvFiles.Name = "dgvFiles";
			this.dgvFiles.ReadOnly = true;
			this.dgvFiles.RowHeadersVisible = false;
			this.dgvFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvFiles.Size = new System.Drawing.Size(459, 410);
			this.dgvFiles.TabIndex = 20;
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
			this.btnSearch.Location = new System.Drawing.Point(943, 2);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
			this.btnSearch.Size = new System.Drawing.Size(90, 24);
			this.btnSearch.TabIndex = 3;
			this.btnSearch.Text = global::LogsReader.Properties.Resources.Txt_LogsReaderForm_Search;
			this.btnSearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btnSearch.UseVisualStyleBackColor = true;
			this.btnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
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
			this.txtPattern.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtPattern.Location = new System.Drawing.Point(8, 3);
			this.txtPattern.Name = "txtPattern";
			this.txtPattern.Size = new System.Drawing.Size(840, 23);
			this.txtPattern.TabIndex = 1;
			this.txtPattern.TextChanged += new System.EventHandler(this.TxtPattern_TextChanged);
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
			this.EnumSplitContainer.Panel1.Controls.Add(this.dgvFiles);
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
			this.useRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.useRegex.AutoSize = true;
			this.useRegex.Location = new System.Drawing.Point(858, 6);
			this.useRegex.Name = "useRegex";
			this.useRegex.Size = new System.Drawing.Size(79, 19);
			this.useRegex.TabIndex = 2;
			this.useRegex.Text = global::LogsReader.Properties.Resources.Txt_LogsReaderForm_UseRegex;
			this.useRegex.UseVisualStyleBackColor = true;
			this.useRegex.CheckedChanged += new System.EventHandler(this.UseRegex_CheckedChanged);
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
			this.traceNameFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.traceNameFilter.Location = new System.Drawing.Point(403, 7);
			this.traceNameFilter.Name = "traceNameFilter";
			this.traceNameFilter.Size = new System.Drawing.Size(444, 23);
			this.traceNameFilter.TabIndex = 9;
			this.traceNameFilter.TextChanged += new System.EventHandler(this.TraceNameFilter_TextChanged);
			// 
			// dateEndFilter
			// 
			this.dateEndFilter.Checked = false;
			this.dateEndFilter.CustomFormat = "dd.MM.yyyy HH:mm:ss";
			this.dateEndFilter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dateEndFilter.Location = new System.Drawing.Point(68, 34);
			this.dateEndFilter.Name = "dateEndFilter";
			this.dateEndFilter.ShowCheckBox = true;
			this.dateEndFilter.ShowUpDown = true;
			this.dateEndFilter.Size = new System.Drawing.Size(151, 23);
			this.dateEndFilter.TabIndex = 6;
			// 
			// dateStartFilter
			// 
			this.dateStartFilter.Checked = false;
			this.dateStartFilter.CustomFormat = "dd.MM.yyyy HH:mm:ss";
			this.dateStartFilter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dateStartFilter.Location = new System.Drawing.Point(68, 7);
			this.dateStartFilter.Name = "dateStartFilter";
			this.dateStartFilter.ShowCheckBox = true;
			this.dateStartFilter.ShowUpDown = true;
			this.dateStartFilter.Size = new System.Drawing.Size(151, 23);
			this.dateStartFilter.TabIndex = 5;
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
			this.buttonFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonFilter.Image = global::LogsReader.Properties.Resources.filter;
			this.buttonFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonFilter.Location = new System.Drawing.Point(853, 7);
			this.buttonFilter.Name = "buttonFilter";
			this.buttonFilter.Padding = new System.Windows.Forms.Padding(5, 0, 7, 0);
			this.buttonFilter.Size = new System.Drawing.Size(100, 23);
			this.buttonFilter.TabIndex = 11;
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
			this.buttonReset.Location = new System.Drawing.Point(853, 34);
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.Padding = new System.Windows.Forms.Padding(2, 0, 7, 0);
			this.buttonReset.Size = new System.Drawing.Size(100, 23);
			this.buttonReset.TabIndex = 12;
			this.buttonReset.Text = global::LogsReader.Properties.Resources.Txt_LogsReaderForm_Reset;
			this.buttonReset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonReset.UseVisualStyleBackColor = true;
			this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
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
			this.traceMessageFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.traceMessageFilter.Location = new System.Drawing.Point(403, 34);
			this.traceMessageFilter.Name = "traceMessageFilter";
			this.traceMessageFilter.Size = new System.Drawing.Size(444, 23);
			this.traceMessageFilter.TabIndex = 10;
			this.traceMessageFilter.TextChanged += new System.EventHandler(this.TraceMessageFilter_TextChanged);
			// 
			// filterPanel
			// 
			this.filterPanel.BackColor = System.Drawing.SystemColors.Control;
			this.filterPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.filterPanel.Controls.Add(this.traceMessageFilterComboBox);
			this.filterPanel.Controls.Add(this.traceNameFilterComboBox);
			this.filterPanel.Controls.Add(this.alreadyUseFilter);
			this.filterPanel.Controls.Add(this.buttonExport);
			this.filterPanel.Controls.Add(this.traceMessageFilter);
			this.filterPanel.Controls.Add(this.label11);
			this.filterPanel.Controls.Add(this.buttonReset);
			this.filterPanel.Controls.Add(this.buttonFilter);
			this.filterPanel.Controls.Add(this.label8);
			this.filterPanel.Controls.Add(this.dateStartFilter);
			this.filterPanel.Controls.Add(this.dateEndFilter);
			this.filterPanel.Controls.Add(this.traceNameFilter);
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
			this.traceMessageFilterComboBox.BackColor = System.Drawing.SystemColors.Window;
			this.traceMessageFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.traceMessageFilterComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.traceMessageFilterComboBox.FormattingEnabled = true;
			this.traceMessageFilterComboBox.Location = new System.Drawing.Point(298, 35);
			this.traceMessageFilterComboBox.MaxDropDownItems = 2;
			this.traceMessageFilterComboBox.Name = "traceMessageFilterComboBox";
			this.traceMessageFilterComboBox.Size = new System.Drawing.Size(102, 21);
			this.traceMessageFilterComboBox.TabIndex = 8;
			this.traceMessageFilterComboBox.SelectedIndexChanged += new System.EventHandler(this.traceMessageFilterComboBox_SelectedIndexChanged);
			this.traceMessageFilterComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ComboBox_KeyPress);
			// 
			// traceNameFilterComboBox
			// 
			this.traceNameFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.traceNameFilterComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.traceNameFilterComboBox.FormattingEnabled = true;
			this.traceNameFilterComboBox.Location = new System.Drawing.Point(298, 8);
			this.traceNameFilterComboBox.MaxDropDownItems = 2;
			this.traceNameFilterComboBox.Name = "traceNameFilterComboBox";
			this.traceNameFilterComboBox.Size = new System.Drawing.Size(102, 21);
			this.traceNameFilterComboBox.TabIndex = 7;
			this.traceNameFilterComboBox.SelectedIndexChanged += new System.EventHandler(this.traceNameFilterComboBox_SelectedIndexChanged);
			this.traceNameFilterComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ComboBox_KeyPress);
			// 
			// alreadyUseFilter
			// 
			this.alreadyUseFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.alreadyUseFilter.AutoSize = true;
			this.alreadyUseFilter.Font = new System.Drawing.Font("Segoe UI", 8.5F);
			this.alreadyUseFilter.Location = new System.Drawing.Point(962, 37);
			this.alreadyUseFilter.Name = "alreadyUseFilter";
			this.alreadyUseFilter.Size = new System.Drawing.Size(158, 19);
			this.alreadyUseFilter.TabIndex = 14;
			this.alreadyUseFilter.Text = "Use filter when searching";
			this.alreadyUseFilter.UseVisualStyleBackColor = true;
			// 
			// buttonExport
			// 
			this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonExport.Image = global::LogsReader.Properties.Resources.save2;
			this.buttonExport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonExport.Location = new System.Drawing.Point(959, 7);
			this.buttonExport.Name = "buttonExport";
			this.buttonExport.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.buttonExport.Size = new System.Drawing.Size(110, 23);
			this.buttonExport.TabIndex = 13;
			this.buttonExport.Text = "Export [Ctrl+S]";
			this.buttonExport.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonExport.UseVisualStyleBackColor = true;
			this.buttonExport.Click += new System.EventHandler(this.ButtonExport_Click);
			// 
			// searchPanel
			// 
			this.searchPanel.BackColor = System.Drawing.SystemColors.Control;
			this.searchPanel.Controls.Add(this.txtPattern);
			this.searchPanel.Controls.Add(this.useRegex);
			this.searchPanel.Controls.Add(this.btnSearch);
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
			((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).EndInit();
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
        private CustomDataGridView dgvFiles;
        private Button btnClear;
        private ProgressBar progressBar;
        private StatusStrip statusStrip;
        private Label label7;
        private Label label9;
        private TextBox traceNameFilter;
        private DateTimePicker dateEndFilter;
        private DateTimePicker dateStartFilter;
        private Label label8;
        private Button buttonFilter;
        private Button buttonReset;
        private Label label11;
        private TextBox traceMessageFilter;
        private Panel filterPanel;
        private SplitContainer EnumSplitContainer;
        private RichTextBox descriptionText;
        private Panel searchPanel;
        private Button buttonExport;
        private DataGridViewTextBoxColumn Date;
        private ComboBox traceNameFilterComboBox;
        private ComboBox traceMessageFilterComboBox;
        private DataGridViewTextBoxColumn PrivateID;
        private DataGridViewTextBoxColumn IsMatched;
        private DataGridViewTextBoxColumn ID;
        private DataGridViewTextBoxColumn Server;
        private DataGridViewTextBoxColumn TraceName;
        private DataGridViewTextBoxColumn DateOfTrace;
        private DataGridViewTextBoxColumn File;
        private SplitContainer ParentSplitContainer;
        private NotepadControl notepad;

        protected Button btnSearch;
		protected CheckBox useRegex;
		protected TextBox txtPattern;
		protected CheckBox alreadyUseFilter;
        protected SplitContainer MainSplitContainer;
	}
}

