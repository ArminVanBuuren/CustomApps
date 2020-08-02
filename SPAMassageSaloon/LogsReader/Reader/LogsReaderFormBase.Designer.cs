﻿
using System.ComponentModel;
using System.Windows.Forms;
using SPAMassageSaloon.Common;
using Utils.WinForm;

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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			this.DgvData = new SPAMassageSaloon.Common.CustomDataGridView();
			this.DgvDataPromptColumn = new LogsReader.DgvTextAndImageColumn();
			this.DgvDataIDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DgvDataServerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DgvDataTraceNameColumn = new LogsReader.DgvTextAndImageColumn();
			this.DgvDataDateOfTraceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DgvDataElapsedSecColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DgvDataSchemeNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DgvDataPrivateIDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DgvDataIsSuccessColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DgvDataIsFilteredColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DgvDataFileColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.BtnSearch = new System.Windows.Forms.Button();
			this.btnClear = new System.Windows.Forms.Button();
			this.TbxPattern = new System.Windows.Forms.TextBox();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
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
			this.buttonHighlightOff = new System.Windows.Forms.Button();
			this.buttonHighlightOn = new System.Windows.Forms.Button();
			this.CobxTraceMessageFilter = new System.Windows.Forms.ComboBox();
			this.CobxTraceNameFilter = new System.Windows.Forms.ComboBox();
			this.ChbxAlreadyUseFilter = new System.Windows.Forms.CheckBox();
			this.btnExport = new System.Windows.Forms.Button();
			this.labelError = new System.Windows.Forms.Label();
			this.buttonErrNext = new System.Windows.Forms.Button();
			this.buttonErrPrev = new System.Windows.Forms.Button();
			this.searchPanel = new System.Windows.Forms.Panel();
			this.ParentSplitContainer = new System.Windows.Forms.SplitContainer();
			this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
			this.CustomPanel = new System.Windows.Forms.Panel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonFilteredPrev = new System.Windows.Forms.Button();
			this.buttonFilteredNext = new System.Windows.Forms.Button();
			this.panel3 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.checkBoxShowTrns = new System.Windows.Forms.CheckBox();
			this.tabControlViewer = new LogsReader.Reader.CustomTabControl();
			this.buttonNextBlock = new System.Windows.Forms.Button();
			this.splitContainerTop = new System.Windows.Forms.SplitContainer();
			this.DgvReader = new SPAMassageSaloon.Common.CustomDataGridView();
			this.DgvReaderSelectColumn = new LogsReader.DgvCheckBoxColumn();
			this.DgvReaderImageColumn = new LogsReader.DgvTextAndImageColumn();
			this.DgvReaderFlowColumn = new System.Windows.Forms.DataGridViewButtonColumn();
			this.DgvReaderSchemeNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DgvReaderPrivateIDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DgvReaderThreadIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DgvReaderCountMatchesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DgvReaderCountErrorMatchesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DgvReaderFileColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.DgvData)).BeginInit();
			this.filterPanel.SuspendLayout();
			this.searchPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentSplitContainer)).BeginInit();
			this.ParentSplitContainer.Panel1.SuspendLayout();
			this.ParentSplitContainer.Panel2.SuspendLayout();
			this.ParentSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel4.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).BeginInit();
			this.splitContainerTop.Panel1.SuspendLayout();
			this.splitContainerTop.Panel2.SuspendLayout();
			this.splitContainerTop.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DgvReader)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// DgvData
			// 
			this.DgvData.AllowUserToAddRows = false;
			this.DgvData.AllowUserToDeleteRows = false;
			this.DgvData.AllowUserToResizeRows = false;
			this.DgvData.BackgroundColor = System.Drawing.SystemColors.Window;
			this.DgvData.BorderStyle = System.Windows.Forms.BorderStyle.None;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.DgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.DgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.DgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DgvDataPromptColumn,
            this.DgvDataIDColumn,
            this.DgvDataServerColumn,
            this.DgvDataTraceNameColumn,
            this.DgvDataDateOfTraceColumn,
            this.DgvDataElapsedSecColumn,
            this.DgvDataSchemeNameColumn,
            this.DgvDataPrivateIDColumn,
            this.DgvDataIsSuccessColumn,
            this.DgvDataIsFilteredColumn,
            this.DgvDataFileColumn});
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.DgvData.DefaultCellStyle = dataGridViewCellStyle2;
			this.DgvData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DgvData.GridColor = System.Drawing.SystemColors.ControlLight;
			this.DgvData.Location = new System.Drawing.Point(0, 0);
			this.DgvData.Name = "DgvData";
			this.DgvData.ReadOnly = true;
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.DgvData.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.DgvData.RowHeadersVisible = false;
			this.DgvData.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.DgvData.RowTemplate.Height = 18;
			this.DgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.DgvData.Size = new System.Drawing.Size(537, 461);
			this.DgvData.TabIndex = 20;
			// 
			// DgvDataPromptColumn
			// 
			this.DgvDataPromptColumn.Image = null;
			this.DgvDataPromptColumn.MinimumWidth = 23;
			this.DgvDataPromptColumn.Name = "DgvDataPromptColumn";
			this.DgvDataPromptColumn.ReadOnly = true;
			this.DgvDataPromptColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.DgvDataPromptColumn.Width = 23;
			// 
			// DgvDataIDColumn
			// 
			this.DgvDataIDColumn.MinimumWidth = 25;
			this.DgvDataIDColumn.Name = "DgvDataIDColumn";
			this.DgvDataIDColumn.ReadOnly = true;
			this.DgvDataIDColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.DgvDataIDColumn.Width = 30;
			// 
			// DgvDataServerColumn
			// 
			this.DgvDataServerColumn.MinimumWidth = 45;
			this.DgvDataServerColumn.Name = "DgvDataServerColumn";
			this.DgvDataServerColumn.ReadOnly = true;
			this.DgvDataServerColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.DgvDataServerColumn.Width = 50;
			// 
			// DgvDataTraceNameColumn
			// 
			this.DgvDataTraceNameColumn.Image = null;
			this.DgvDataTraceNameColumn.MinimumWidth = 70;
			this.DgvDataTraceNameColumn.Name = "DgvDataTraceNameColumn";
			this.DgvDataTraceNameColumn.ReadOnly = true;
			this.DgvDataTraceNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.DgvDataTraceNameColumn.Width = 80;
			// 
			// DgvDataDateOfTraceColumn
			// 
			this.DgvDataDateOfTraceColumn.MinimumWidth = 37;
			this.DgvDataDateOfTraceColumn.Name = "DgvDataDateOfTraceColumn";
			this.DgvDataDateOfTraceColumn.ReadOnly = true;
			this.DgvDataDateOfTraceColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.DgvDataDateOfTraceColumn.Width = 140;
			// 
			// DgvDataElapsedSecColumn
			// 
			this.DgvDataElapsedSecColumn.MinimumWidth = 46;
			this.DgvDataElapsedSecColumn.Name = "DgvDataElapsedSecColumn";
			this.DgvDataElapsedSecColumn.ReadOnly = true;
			this.DgvDataElapsedSecColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.DgvDataElapsedSecColumn.Width = 46;
			// 
			// DgvDataSchemeNameColumn
			// 
			this.DgvDataSchemeNameColumn.Name = "DgvDataSchemeNameColumn";
			this.DgvDataSchemeNameColumn.ReadOnly = true;
			this.DgvDataSchemeNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.DgvDataSchemeNameColumn.Visible = false;
			this.DgvDataSchemeNameColumn.Width = 5;
			// 
			// DgvDataPrivateIDColumn
			// 
			this.DgvDataPrivateIDColumn.Name = "DgvDataPrivateIDColumn";
			this.DgvDataPrivateIDColumn.ReadOnly = true;
			this.DgvDataPrivateIDColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.DgvDataPrivateIDColumn.Visible = false;
			this.DgvDataPrivateIDColumn.Width = 5;
			// 
			// DgvDataIsSuccessColumn
			// 
			this.DgvDataIsSuccessColumn.Name = "DgvDataIsSuccessColumn";
			this.DgvDataIsSuccessColumn.ReadOnly = true;
			this.DgvDataIsSuccessColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.DgvDataIsSuccessColumn.Visible = false;
			this.DgvDataIsSuccessColumn.Width = 5;
			// 
			// DgvDataIsFilteredColumn
			// 
			this.DgvDataIsFilteredColumn.Name = "DgvDataIsFilteredColumn";
			this.DgvDataIsFilteredColumn.ReadOnly = true;
			this.DgvDataIsFilteredColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.DgvDataIsFilteredColumn.Visible = false;
			this.DgvDataIsFilteredColumn.Width = 5;
			// 
			// DgvDataFileColumn
			// 
			this.DgvDataFileColumn.MinimumWidth = 300;
			this.DgvDataFileColumn.Name = "DgvDataFileColumn";
			this.DgvDataFileColumn.ReadOnly = true;
			this.DgvDataFileColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.DgvDataFileColumn.Width = 1000;
			// 
			// BtnSearch
			// 
			this.BtnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BtnSearch.BackColor = System.Drawing.SystemColors.Control;
			this.BtnSearch.Image = global::LogsReader.Properties.Resources.find;
			this.BtnSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.BtnSearch.Location = new System.Drawing.Point(937, 3);
			this.BtnSearch.Name = "BtnSearch";
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
			this.btnClear.Location = new System.Drawing.Point(1033, 3);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(90, 24);
			this.btnClear.TabIndex = 4;
			this.btnClear.Text = global::LogsReader.Properties.Resources.Txt_LogsReaderForm_Clear;
			this.btnClear.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
			// 
			// TbxPattern
			// 
			this.TbxPattern.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TbxPattern.Location = new System.Drawing.Point(8, 3);
			this.TbxPattern.Name = "TbxPattern";
			this.TbxPattern.Size = new System.Drawing.Size(841, 20);
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
			this.statusStrip.Location = new System.Drawing.Point(0, 465);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(680, 22);
			this.statusStrip.SizingGrip = false;
			this.statusStrip.TabIndex = 8;
			this.statusStrip.Text = "statusStrip1";
			// 
			// ChbxUseRegex
			// 
			this.ChbxUseRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ChbxUseRegex.AutoSize = true;
			this.ChbxUseRegex.Location = new System.Drawing.Point(855, 7);
			this.ChbxUseRegex.Name = "ChbxUseRegex";
			this.ChbxUseRegex.Size = new System.Drawing.Size(79, 17);
			this.ChbxUseRegex.TabIndex = 2;
			this.ChbxUseRegex.Text = global::LogsReader.Properties.Resources.Txt_LogsReaderForm_UseRegex;
			this.ChbxUseRegex.UseVisualStyleBackColor = true;
			this.ChbxUseRegex.CheckedChanged += new System.EventHandler(this.ChbxUseRegex_CheckedChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(225, 9);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(63, 13);
			this.label7.TabIndex = 24;
			this.label7.Text = "TraceName";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(4, 35);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(52, 13);
			this.label9.TabIndex = 23;
			this.label9.Text = "Date End";
			// 
			// TbxTraceNameFilter
			// 
			this.TbxTraceNameFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TbxTraceNameFilter.Location = new System.Drawing.Point(403, 5);
			this.TbxTraceNameFilter.Name = "TbxTraceNameFilter";
			this.TbxTraceNameFilter.Size = new System.Drawing.Size(329, 20);
			this.TbxTraceNameFilter.TabIndex = 9;
			this.TbxTraceNameFilter.TextChanged += new System.EventHandler(this.TbxTraceNameFilterOnTextChanged);
			// 
			// DateEndFilter
			// 
			this.DateEndFilter.Checked = false;
			this.DateEndFilter.CustomFormat = "dd.MM.yyyy HH:mm:ss";
			this.DateEndFilter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.DateEndFilter.Location = new System.Drawing.Point(68, 32);
			this.DateEndFilter.Name = "DateEndFilter";
			this.DateEndFilter.ShowCheckBox = true;
			this.DateEndFilter.ShowUpDown = true;
			this.DateEndFilter.Size = new System.Drawing.Size(151, 20);
			this.DateEndFilter.TabIndex = 6;
			// 
			// DateStartFilter
			// 
			this.DateStartFilter.Checked = false;
			this.DateStartFilter.CustomFormat = "dd.MM.yyyy HH:mm:ss";
			this.DateStartFilter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.DateStartFilter.Location = new System.Drawing.Point(68, 5);
			this.DateStartFilter.Name = "DateStartFilter";
			this.DateStartFilter.ShowCheckBox = true;
			this.DateStartFilter.ShowUpDown = true;
			this.DateStartFilter.Size = new System.Drawing.Size(151, 20);
			this.DateStartFilter.TabIndex = 5;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(4, 9);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(55, 13);
			this.label8.TabIndex = 22;
			this.label8.Text = "Date Start";
			// 
			// btnFilter
			// 
			this.btnFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFilter.Image = global::LogsReader.Properties.Resources.filter;
			this.btnFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnFilter.Location = new System.Drawing.Point(738, 3);
			this.btnFilter.Name = "btnFilter";
			this.btnFilter.Size = new System.Drawing.Size(100, 23);
			this.btnFilter.TabIndex = 11;
			this.btnFilter.Text = "          Filter [F7]";
			this.btnFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnFilter.UseVisualStyleBackColor = true;
			this.btnFilter.Click += new System.EventHandler(this.BtnFilter_Click);
			// 
			// btnReset
			// 
			this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnReset.Image = global::LogsReader.Properties.Resources.reset2;
			this.btnReset.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnReset.Location = new System.Drawing.Point(847, 3);
			this.btnReset.Name = "btnReset";
			this.btnReset.Size = new System.Drawing.Size(100, 23);
			this.btnReset.TabIndex = 12;
			this.btnReset.Text = "        Reset [F10]";
			this.btnReset.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnReset.UseVisualStyleBackColor = true;
			this.btnReset.Click += new System.EventHandler(this.BtnReset_Click);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(225, 35);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(54, 13);
			this.label11.TabIndex = 30;
			this.label11.Text = "Full Trace";
			// 
			// TbxTraceMessageFilter
			// 
			this.TbxTraceMessageFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TbxTraceMessageFilter.Location = new System.Drawing.Point(403, 32);
			this.TbxTraceMessageFilter.Name = "TbxTraceMessageFilter";
			this.TbxTraceMessageFilter.Size = new System.Drawing.Size(329, 20);
			this.TbxTraceMessageFilter.TabIndex = 10;
			this.TbxTraceMessageFilter.TextChanged += new System.EventHandler(this.TbxTraceMessageFilterOnTextChanged);
			// 
			// filterPanel
			// 
			this.filterPanel.BackColor = System.Drawing.SystemColors.Control;
			this.filterPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.filterPanel.Controls.Add(this.buttonHighlightOff);
			this.filterPanel.Controls.Add(this.buttonHighlightOn);
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
			this.filterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.filterPanel.Location = new System.Drawing.Point(0, 0);
			this.filterPanel.MaximumSize = new System.Drawing.Size(5014, 61);
			this.filterPanel.MinimumSize = new System.Drawing.Size(850, 2);
			this.filterPanel.Name = "filterPanel";
			this.filterPanel.Size = new System.Drawing.Size(1010, 61);
			this.filterPanel.TabIndex = 28;
			// 
			// buttonHighlightOff
			// 
			this.buttonHighlightOff.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonHighlightOff.BackColor = System.Drawing.Color.Gray;
			this.buttonHighlightOff.Image = global::LogsReader.Properties.Resources.filtered;
			this.buttonHighlightOff.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.buttonHighlightOff.Location = new System.Drawing.Point(791, 30);
			this.buttonHighlightOff.Name = "buttonHighlightOff";
			this.buttonHighlightOff.Padding = new System.Windows.Forms.Padding(11, 0, 0, 1);
			this.buttonHighlightOff.Size = new System.Drawing.Size(47, 24);
			this.buttonHighlightOff.TabIndex = 34;
			this.buttonHighlightOff.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonHighlightOff.UseVisualStyleBackColor = false;
			this.buttonHighlightOff.Click += new System.EventHandler(this.buttonHighlightOff_Click);
			// 
			// buttonHighlightOn
			// 
			this.buttonHighlightOn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonHighlightOn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
			this.buttonHighlightOn.Image = global::LogsReader.Properties.Resources.filtered;
			this.buttonHighlightOn.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.buttonHighlightOn.Location = new System.Drawing.Point(738, 30);
			this.buttonHighlightOn.Name = "buttonHighlightOn";
			this.buttonHighlightOn.Padding = new System.Windows.Forms.Padding(11, 0, 0, 1);
			this.buttonHighlightOn.Size = new System.Drawing.Size(47, 24);
			this.buttonHighlightOn.TabIndex = 33;
			this.buttonHighlightOn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonHighlightOn.UseVisualStyleBackColor = false;
			this.buttonHighlightOn.Click += new System.EventHandler(this.buttonHighlight_Click);
			// 
			// CobxTraceMessageFilter
			// 
			this.CobxTraceMessageFilter.BackColor = System.Drawing.SystemColors.Window;
			this.CobxTraceMessageFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CobxTraceMessageFilter.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.CobxTraceMessageFilter.FormattingEnabled = true;
			this.CobxTraceMessageFilter.Location = new System.Drawing.Point(298, 33);
			this.CobxTraceMessageFilter.MaxDropDownItems = 2;
			this.CobxTraceMessageFilter.Name = "CobxTraceMessageFilter";
			this.CobxTraceMessageFilter.Size = new System.Drawing.Size(102, 21);
			this.CobxTraceMessageFilter.TabIndex = 8;
			this.CobxTraceMessageFilter.SelectedIndexChanged += new System.EventHandler(this.CobxTraceMessageFilter_SelectedIndexChanged);
			this.CobxTraceMessageFilter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ComboBox_KeyPress);
			// 
			// CobxTraceNameFilter
			// 
			this.CobxTraceNameFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CobxTraceNameFilter.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.CobxTraceNameFilter.FormattingEnabled = true;
			this.CobxTraceNameFilter.Location = new System.Drawing.Point(298, 6);
			this.CobxTraceNameFilter.MaxDropDownItems = 2;
			this.CobxTraceNameFilter.Name = "CobxTraceNameFilter";
			this.CobxTraceNameFilter.Size = new System.Drawing.Size(102, 21);
			this.CobxTraceNameFilter.TabIndex = 7;
			this.CobxTraceNameFilter.SelectedIndexChanged += new System.EventHandler(this.CobxTraceNameFilter_SelectedIndexChanged);
			this.CobxTraceNameFilter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ComboBox_KeyPress);
			// 
			// ChbxAlreadyUseFilter
			// 
			this.ChbxAlreadyUseFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ChbxAlreadyUseFilter.Location = new System.Drawing.Point(847, 34);
			this.ChbxAlreadyUseFilter.Name = "ChbxAlreadyUseFilter";
			this.ChbxAlreadyUseFilter.Size = new System.Drawing.Size(179, 19);
			this.ChbxAlreadyUseFilter.TabIndex = 14;
			this.ChbxAlreadyUseFilter.Text = "UseFilterWhenSearching";
			this.ChbxAlreadyUseFilter.UseVisualStyleBackColor = true;
			this.ChbxAlreadyUseFilter.CheckedChanged += new System.EventHandler(this.ChbxAlreadyUseFilter_CheckedChanged);
			// 
			// btnExport
			// 
			this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExport.Image = global::LogsReader.Properties.Resources.save2;
			this.btnExport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnExport.Location = new System.Drawing.Point(953, 3);
			this.btnExport.Name = "btnExport";
			this.btnExport.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.btnExport.Size = new System.Drawing.Size(27, 23);
			this.btnExport.TabIndex = 13;
			this.btnExport.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btnExport.UseVisualStyleBackColor = true;
			this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);
			// 
			// labelError
			// 
			this.labelError.AutoSize = true;
			this.labelError.Image = global::LogsReader.Properties.Resources.Error1;
			this.labelError.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelError.Location = new System.Drawing.Point(36, 5);
			this.labelError.Name = "labelError";
			this.labelError.Size = new System.Drawing.Size(47, 13);
			this.labelError.TabIndex = 31;
			this.labelError.Text = "      Error";
			// 
			// buttonErrNext
			// 
			this.buttonErrNext.BackColor = System.Drawing.Color.White;
			this.buttonErrNext.FlatAppearance.BorderSize = 0;
			this.buttonErrNext.Image = global::LogsReader.Properties.Resources.next_arrow;
			this.buttonErrNext.Location = new System.Drawing.Point(93, 0);
			this.buttonErrNext.Name = "buttonErrNext";
			this.buttonErrNext.Size = new System.Drawing.Size(24, 24);
			this.buttonErrNext.TabIndex = 1;
			this.buttonErrNext.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonErrNext.UseVisualStyleBackColor = false;
			this.buttonErrNext.Click += new System.EventHandler(this.buttonErrorNext_Click);
			// 
			// buttonErrPrev
			// 
			this.buttonErrPrev.BackColor = System.Drawing.Color.White;
			this.buttonErrPrev.FlatAppearance.BorderSize = 0;
			this.buttonErrPrev.Image = global::LogsReader.Properties.Resources.prev_arrow;
			this.buttonErrPrev.Location = new System.Drawing.Point(7, 0);
			this.buttonErrPrev.Name = "buttonErrPrev";
			this.buttonErrPrev.Size = new System.Drawing.Size(24, 24);
			this.buttonErrPrev.TabIndex = 0;
			this.buttonErrPrev.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonErrPrev.UseVisualStyleBackColor = false;
			this.buttonErrPrev.Click += new System.EventHandler(this.buttonErrorPrev_Click);
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
			this.ParentSplitContainer.Location = new System.Drawing.Point(0, 0);
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
			this.ParentSplitContainer.Panel2.Controls.Add(this.tabControlViewer);
			this.ParentSplitContainer.Size = new System.Drawing.Size(1152, 487);
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
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.BackColor = System.Drawing.Color.White;
			this.MainSplitContainer.Panel1.Controls.Add(this.CustomPanel);
			this.MainSplitContainer.Panel1.Controls.Add(this.panel1);
			this.MainSplitContainer.Panel1MinSize = 132;
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.DgvData);
			this.MainSplitContainer.Size = new System.Drawing.Size(680, 465);
			this.MainSplitContainer.SplitterDistance = 135;
			this.MainSplitContainer.TabIndex = 0;
			// 
			// CustomPanel
			// 
			this.CustomPanel.BackColor = System.Drawing.SystemColors.Control;
			this.CustomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomPanel.Location = new System.Drawing.Point(0, 80);
			this.CustomPanel.Name = "CustomPanel";
			this.CustomPanel.Size = new System.Drawing.Size(131, 381);
			this.CustomPanel.TabIndex = 1;
			this.CustomPanel.Resize += new System.EventHandler(this.CustomPanel_Resize);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.WhiteSmoke;
			this.panel1.Controls.Add(this.panel4);
			this.panel1.Controls.Add(this.panel3);
			this.panel1.Controls.Add(this.panel2);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(131, 80);
			this.panel1.TabIndex = 2;
			// 
			// panel4
			// 
			this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel4.Controls.Add(this.label1);
			this.panel4.Controls.Add(this.buttonFilteredPrev);
			this.panel4.Controls.Add(this.buttonFilteredNext);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel4.Location = new System.Drawing.Point(0, 26);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(131, 26);
			this.panel4.TabIndex = 35;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Image = global::LogsReader.Properties.Resources.filtered;
			this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label1.Location = new System.Drawing.Point(29, 5);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(59, 13);
			this.label1.TabIndex = 31;
			this.label1.Text = "      Filtered";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// buttonFilteredPrev
			// 
			this.buttonFilteredPrev.BackColor = System.Drawing.Color.White;
			this.buttonFilteredPrev.FlatAppearance.BorderSize = 0;
			this.buttonFilteredPrev.Image = global::LogsReader.Properties.Resources.prev_arrow;
			this.buttonFilteredPrev.Location = new System.Drawing.Point(4, 0);
			this.buttonFilteredPrev.Name = "buttonFilteredPrev";
			this.buttonFilteredPrev.Size = new System.Drawing.Size(24, 24);
			this.buttonFilteredPrev.TabIndex = 0;
			this.buttonFilteredPrev.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonFilteredPrev.UseVisualStyleBackColor = false;
			this.buttonFilteredPrev.Click += new System.EventHandler(this.buttonFilteredPrev_Click);
			// 
			// buttonFilteredNext
			// 
			this.buttonFilteredNext.BackColor = System.Drawing.Color.White;
			this.buttonFilteredNext.FlatAppearance.BorderSize = 0;
			this.buttonFilteredNext.Image = global::LogsReader.Properties.Resources.next_arrow;
			this.buttonFilteredNext.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonFilteredNext.Location = new System.Drawing.Point(96, 0);
			this.buttonFilteredNext.Name = "buttonFilteredNext";
			this.buttonFilteredNext.Size = new System.Drawing.Size(24, 24);
			this.buttonFilteredNext.TabIndex = 1;
			this.buttonFilteredNext.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonFilteredNext.UseVisualStyleBackColor = false;
			this.buttonFilteredNext.Click += new System.EventHandler(this.buttonFilteredNext_Click);
			// 
			// panel3
			// 
			this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel3.Controls.Add(this.labelError);
			this.panel3.Controls.Add(this.buttonErrPrev);
			this.panel3.Controls.Add(this.buttonErrNext);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel3.Location = new System.Drawing.Point(0, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(131, 26);
			this.panel3.TabIndex = 34;
			// 
			// panel2
			// 
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel2.Controls.Add(this.checkBoxShowTrns);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 52);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(131, 28);
			this.panel2.TabIndex = 33;
			// 
			// checkBoxShowTrns
			// 
			this.checkBoxShowTrns.AutoSize = true;
			this.checkBoxShowTrns.Location = new System.Drawing.Point(4, 4);
			this.checkBoxShowTrns.Name = "checkBoxShowTrns";
			this.checkBoxShowTrns.Size = new System.Drawing.Size(113, 17);
			this.checkBoxShowTrns.TabIndex = 32;
			this.checkBoxShowTrns.Text = "Show transactions";
			this.checkBoxShowTrns.UseVisualStyleBackColor = true;
			this.checkBoxShowTrns.CheckedChanged += new System.EventHandler(this.checkBoxShowTrns_CheckedChanged);
			// 
			// tabControlViewer
			// 
			this.tabControlViewer.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabControlViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlViewer.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.tabControlViewer.Location = new System.Drawing.Point(0, 0);
			this.tabControlViewer.Name = "tabControlViewer";
			this.tabControlViewer.SelectedIndex = 0;
			this.tabControlViewer.Size = new System.Drawing.Size(464, 483);
			this.tabControlViewer.TabIndex = 0;
			// 
			// buttonNextBlock
			// 
			this.buttonNextBlock.BackColor = System.Drawing.Color.White;
			this.buttonNextBlock.Dock = System.Windows.Forms.DockStyle.Right;
			this.buttonNextBlock.Location = new System.Drawing.Point(1127, 0);
			this.buttonNextBlock.MaximumSize = new System.Drawing.Size(21, 2000);
			this.buttonNextBlock.MinimumSize = new System.Drawing.Size(21, 25);
			this.buttonNextBlock.Name = "buttonNextBlock";
			this.buttonNextBlock.Size = new System.Drawing.Size(21, 61);
			this.buttonNextBlock.TabIndex = 35;
			this.buttonNextBlock.Text = ">";
			this.buttonNextBlock.UseVisualStyleBackColor = false;
			this.buttonNextBlock.Click += new System.EventHandler(this.buttonNextBlock_Click);
			// 
			// splitContainerTop
			// 
			this.splitContainerTop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerTop.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainerTop.Location = new System.Drawing.Point(0, 0);
			this.splitContainerTop.Name = "splitContainerTop";
			// 
			// splitContainerTop.Panel1
			// 
			this.splitContainerTop.Panel1.Controls.Add(this.filterPanel);
			// 
			// splitContainerTop.Panel2
			// 
			this.splitContainerTop.Panel2.Controls.Add(this.DgvReader);
			this.splitContainerTop.Panel2MinSize = 0;
			this.splitContainerTop.Size = new System.Drawing.Size(1127, 61);
			this.splitContainerTop.SplitterDistance = 1010;
			this.splitContainerTop.TabIndex = 36;
			// 
			// DgvReader
			// 
			this.DgvReader.AllowUserToAddRows = false;
			this.DgvReader.AllowUserToDeleteRows = false;
			this.DgvReader.AllowUserToResizeRows = false;
			this.DgvReader.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.DgvReader.ColumnHeadersVisible = false;
			this.DgvReader.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DgvReaderSelectColumn,
            this.DgvReaderImageColumn,
            this.DgvReaderFlowColumn,
            this.DgvReaderSchemeNameColumn,
            this.DgvReaderPrivateIDColumn,
            this.DgvReaderThreadIdColumn,
            this.DgvReaderCountMatchesColumn,
            this.DgvReaderCountErrorMatchesColumn,
            this.DgvReaderFileColumn});
			this.DgvReader.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DgvReader.Location = new System.Drawing.Point(0, 0);
			this.DgvReader.MultiSelect = false;
			this.DgvReader.Name = "DgvReader";
			this.DgvReader.ReadOnly = true;
			this.DgvReader.RowHeadersVisible = false;
			this.DgvReader.RowTemplate.Height = 18;
			this.DgvReader.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.DgvReader.Size = new System.Drawing.Size(113, 61);
			this.DgvReader.TabIndex = 50;
			// 
			// DgvReaderSelectColumn
			// 
			this.DgvReaderSelectColumn.Checked = false;
			this.DgvReaderSelectColumn.HeaderText = "";
			this.DgvReaderSelectColumn.MinimumWidth = 25;
			this.DgvReaderSelectColumn.Name = "DgvReaderSelectColumn";
			this.DgvReaderSelectColumn.ReadOnly = true;
			this.DgvReaderSelectColumn.Width = 25;
			// 
			// DgvReaderImageColumn
			// 
			this.DgvReaderImageColumn.HeaderText = "Status";
			this.DgvReaderImageColumn.Image = null;
			this.DgvReaderImageColumn.MinimumWidth = 80;
			this.DgvReaderImageColumn.Name = "DgvReaderImageColumn";
			this.DgvReaderImageColumn.ReadOnly = true;
			this.DgvReaderImageColumn.Width = 80;
			// 
			// DgvReaderFlowColumn
			// 
			this.DgvReaderFlowColumn.HeaderText = "Flow";
			this.DgvReaderFlowColumn.MinimumWidth = 37;
			this.DgvReaderFlowColumn.Name = "DgvReaderFlowColumn";
			this.DgvReaderFlowColumn.ReadOnly = true;
			this.DgvReaderFlowColumn.Width = 37;
			// 
			// DgvReaderSchemeNameColumn
			// 
			this.DgvReaderSchemeNameColumn.HeaderText = "SchemeName";
			this.DgvReaderSchemeNameColumn.Name = "DgvReaderSchemeNameColumn";
			this.DgvReaderSchemeNameColumn.ReadOnly = true;
			this.DgvReaderSchemeNameColumn.Visible = false;
			// 
			// DgvReaderPrivateIDColumn
			// 
			this.DgvReaderPrivateIDColumn.HeaderText = "PrivateID";
			this.DgvReaderPrivateIDColumn.Name = "DgvReaderPrivateIDColumn";
			this.DgvReaderPrivateIDColumn.ReadOnly = true;
			this.DgvReaderPrivateIDColumn.Visible = false;
			// 
			// DgvReaderThreadIdColumn
			// 
			this.DgvReaderThreadIdColumn.HeaderText = "ThreadId";
			this.DgvReaderThreadIdColumn.MinimumWidth = 60;
			this.DgvReaderThreadIdColumn.Name = "DgvReaderThreadIdColumn";
			this.DgvReaderThreadIdColumn.ReadOnly = true;
			this.DgvReaderThreadIdColumn.Width = 60;
			// 
			// DgvReaderCountMatchesColumn
			// 
			this.DgvReaderCountMatchesColumn.HeaderText = "Matches";
			this.DgvReaderCountMatchesColumn.MinimumWidth = 58;
			this.DgvReaderCountMatchesColumn.Name = "DgvReaderCountMatchesColumn";
			this.DgvReaderCountMatchesColumn.ReadOnly = true;
			this.DgvReaderCountMatchesColumn.Width = 58;
			// 
			// DgvReaderCountErrorMatchesColumn
			// 
			this.DgvReaderCountErrorMatchesColumn.HeaderText = "Errors";
			this.DgvReaderCountErrorMatchesColumn.MinimumWidth = 45;
			this.DgvReaderCountErrorMatchesColumn.Name = "DgvReaderCountErrorMatchesColumn";
			this.DgvReaderCountErrorMatchesColumn.ReadOnly = true;
			this.DgvReaderCountErrorMatchesColumn.Width = 45;
			// 
			// DgvReaderFileColumn
			// 
			this.DgvReaderFileColumn.HeaderText = "File";
			this.DgvReaderFileColumn.MinimumWidth = 60;
			this.DgvReaderFileColumn.Name = "DgvReaderFileColumn";
			this.DgvReaderFileColumn.ReadOnly = true;
			this.DgvReaderFileColumn.Width = 60;
			// 
			// splitContainer1
			// 
			this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Location = new System.Drawing.Point(0, 28);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainerTop);
			this.splitContainer1.Panel1.Controls.Add(this.buttonNextBlock);
			this.splitContainer1.Panel1MinSize = 65;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ParentSplitContainer);
			this.splitContainer1.Panel2MinSize = 50;
			this.splitContainer1.Size = new System.Drawing.Size(1152, 556);
			this.splitContainer1.SplitterDistance = 65;
			this.splitContainer1.TabIndex = 37;
			// 
			// LogsReaderFormBase
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.searchPanel);
			this.Controls.Add(this.progressBar);
			this.MinimumSize = new System.Drawing.Size(0, 25);
			this.Name = "LogsReaderFormBase";
			this.Size = new System.Drawing.Size(1152, 594);
			((System.ComponentModel.ISupportInitialize)(this.DgvData)).EndInit();
			this.filterPanel.ResumeLayout(false);
			this.filterPanel.PerformLayout();
			this.searchPanel.ResumeLayout(false);
			this.searchPanel.PerformLayout();
			this.ParentSplitContainer.Panel1.ResumeLayout(false);
			this.ParentSplitContainer.Panel1.PerformLayout();
			this.ParentSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ParentSplitContainer)).EndInit();
			this.ParentSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.splitContainerTop.Panel1.ResumeLayout(false);
			this.splitContainerTop.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).EndInit();
			this.splitContainerTop.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DgvReader)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
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
        private SplitContainer ParentSplitContainer;
        private SplitContainer MainSplitContainer;
        private Panel searchPanel;
        private Button btnExport;

        protected CustomDataGridView DgvData;
		private DgvTextAndImageColumn DgvDataPromptColumn;
        private DataGridViewTextBoxColumn DgvDataIDColumn;
        private DataGridViewTextBoxColumn DgvDataServerColumn;
        private DgvTextAndImageColumn DgvDataTraceNameColumn;
        private DataGridViewTextBoxColumn DgvDataDateOfTraceColumn;
        private DataGridViewTextBoxColumn DgvDataElapsedSecColumn;
        protected DataGridViewTextBoxColumn DgvDataSchemeNameColumn;
        protected DataGridViewTextBoxColumn DgvDataPrivateIDColumn;
        private DataGridViewTextBoxColumn DgvDataIsSuccessColumn;
        private DataGridViewTextBoxColumn DgvDataIsFilteredColumn;
		private DataGridViewTextBoxColumn DgvDataFileColumn;
        private CustomTabControl tabControlViewer;

        protected Panel CustomPanel;

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
		private Button buttonErrPrev;
		private Button buttonErrNext;
		private Label labelError;
		private Panel panel1;
		private CheckBox checkBoxShowTrns;
		private Panel panel2;
		private Panel panel3;
		private Button buttonHighlightOn;
		private Button buttonHighlightOff;
		private Panel panel4;
		private Label label1;
		private Button buttonFilteredPrev;
		private Button buttonFilteredNext;
		private Button buttonNextBlock;
		private SplitContainer splitContainerTop;
		private SplitContainer splitContainer1;

		protected CustomDataGridView DgvReader;
		private DgvCheckBoxColumn DgvReaderSelectColumn;
		private DgvTextAndImageColumn DgvReaderImageColumn;
		private DataGridViewButtonColumn DgvReaderFlowColumn;
		protected DataGridViewTextBoxColumn DgvReaderSchemeNameColumn;
		protected DataGridViewTextBoxColumn DgvReaderPrivateIDColumn;
		private DataGridViewTextBoxColumn DgvReaderThreadIdColumn;
		private DataGridViewTextBoxColumn DgvReaderCountMatchesColumn;
		private DataGridViewTextBoxColumn DgvReaderCountErrorMatchesColumn;
		private DataGridViewTextBoxColumn DgvReaderFileColumn;
	}
}

