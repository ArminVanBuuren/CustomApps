
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
			this.DgvData = new SPAMassageSaloon.Common.CustomDataGridView();
			this.PromptColumn = new LogsReader.TextAndImageColumn();
			this.IDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ServerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.TraceNameColumn = new LogsReader.TextAndImageColumn();
			this.DateOfTraceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ElapsedSecColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.SchemeNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.PrivateIDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.IsSuccessColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.IsFilteredColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.FileColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
			this.DgvFileProcessStatus = new System.Windows.Forms.DataGridView();
			this.ProcessingSelectColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.ProcessingImageColumn = new System.Windows.Forms.DataGridViewImageColumn();
			this.ProcessingFlowColumn = new System.Windows.Forms.DataGridViewButtonColumn();
			this.ProcessingThreadId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ProcessingCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ProcessingSchemeNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ProcessingFileColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.textAndImageColumn1 = new LogsReader.TextAndImageColumn();
			this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.textAndImageColumn2 = new LogsReader.TextAndImageColumn();
			this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
			((System.ComponentModel.ISupportInitialize)(this.DgvFileProcessStatus)).BeginInit();
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
			dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.DgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
			this.DgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.DgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PromptColumn,
            this.IDColumn,
            this.ServerColumn,
            this.TraceNameColumn,
            this.DateOfTraceColumn,
            this.ElapsedSecColumn,
            this.SchemeNameColumn,
            this.PrivateIDColumn,
            this.IsSuccessColumn,
            this.IsFilteredColumn,
            this.FileColumn});
			dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.DgvData.DefaultCellStyle = dataGridViewCellStyle8;
			this.DgvData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DgvData.GridColor = System.Drawing.SystemColors.ControlLight;
			this.DgvData.Location = new System.Drawing.Point(0, 0);
			this.DgvData.Name = "DgvData";
			this.DgvData.ReadOnly = true;
			dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.DgvData.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
			this.DgvData.RowHeadersVisible = false;
			this.DgvData.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.DgvData.RowTemplate.Height = 18;
			this.DgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.DgvData.Size = new System.Drawing.Size(537, 462);
			this.DgvData.TabIndex = 20;
			this.DgvData.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvData_CellDoubleClick);
			this.DgvData.SelectionChanged += new System.EventHandler(this.DgvData_SelectionChanged);
			this.DgvData.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DgvData_MouseDown);
			// 
			// PromptColumn
			// 
			this.PromptColumn.Image = null;
			this.PromptColumn.MinimumWidth = 23;
			this.PromptColumn.Name = "PromptColumn";
			this.PromptColumn.ReadOnly = true;
			this.PromptColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.PromptColumn.Width = 23;
			// 
			// IDColumn
			// 
			this.IDColumn.MinimumWidth = 25;
			this.IDColumn.Name = "IDColumn";
			this.IDColumn.ReadOnly = true;
			this.IDColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.IDColumn.Width = 30;
			// 
			// ServerColumn
			// 
			this.ServerColumn.MinimumWidth = 45;
			this.ServerColumn.Name = "ServerColumn";
			this.ServerColumn.ReadOnly = true;
			this.ServerColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.ServerColumn.Width = 50;
			// 
			// TraceNameColumn
			// 
			this.TraceNameColumn.Image = null;
			this.TraceNameColumn.MinimumWidth = 70;
			this.TraceNameColumn.Name = "TraceNameColumn";
			this.TraceNameColumn.ReadOnly = true;
			this.TraceNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.TraceNameColumn.Width = 80;
			// 
			// DateOfTraceColumn
			// 
			this.DateOfTraceColumn.MinimumWidth = 37;
			this.DateOfTraceColumn.Name = "DateOfTraceColumn";
			this.DateOfTraceColumn.ReadOnly = true;
			this.DateOfTraceColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.DateOfTraceColumn.Width = 140;
			// 
			// ElapsedSecColumn
			// 
			this.ElapsedSecColumn.MinimumWidth = 46;
			this.ElapsedSecColumn.Name = "ElapsedSecColumn";
			this.ElapsedSecColumn.ReadOnly = true;
			this.ElapsedSecColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.ElapsedSecColumn.Width = 46;
			// 
			// SchemeNameColumn
			// 
			this.SchemeNameColumn.Name = "SchemeNameColumn";
			this.SchemeNameColumn.ReadOnly = true;
			this.SchemeNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.SchemeNameColumn.Visible = false;
			this.SchemeNameColumn.Width = 5;
			// 
			// PrivateIDColumn
			// 
			this.PrivateIDColumn.Name = "PrivateIDColumn";
			this.PrivateIDColumn.ReadOnly = true;
			this.PrivateIDColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.PrivateIDColumn.Visible = false;
			this.PrivateIDColumn.Width = 5;
			// 
			// IsSuccessColumn
			// 
			this.IsSuccessColumn.Name = "IsSuccessColumn";
			this.IsSuccessColumn.ReadOnly = true;
			this.IsSuccessColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.IsSuccessColumn.Visible = false;
			this.IsSuccessColumn.Width = 5;
			// 
			// IsFilteredColumn
			// 
			this.IsFilteredColumn.Name = "IsFilteredColumn";
			this.IsFilteredColumn.ReadOnly = true;
			this.IsFilteredColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.IsFilteredColumn.Visible = false;
			this.IsFilteredColumn.Width = 5;
			// 
			// FileColumn
			// 
			this.FileColumn.MinimumWidth = 300;
			this.FileColumn.Name = "FileColumn";
			this.FileColumn.ReadOnly = true;
			this.FileColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.FileColumn.Width = 1000;
			// 
			// BtnSearch
			// 
			this.BtnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BtnSearch.BackColor = System.Drawing.SystemColors.Control;
			this.BtnSearch.Image = global::LogsReader.Properties.Resources.find;
			this.BtnSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.BtnSearch.Location = new System.Drawing.Point(795, 3);
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
			this.btnClear.Location = new System.Drawing.Point(891, 3);
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
			this.TbxPattern.Size = new System.Drawing.Size(699, 20);
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
			this.statusStrip.Location = new System.Drawing.Point(0, 466);
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
			this.ChbxUseRegex.Location = new System.Drawing.Point(713, 7);
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
			this.filterPanel.Location = new System.Drawing.Point(0, 28);
			this.filterPanel.MaximumSize = new System.Drawing.Size(5014, 59);
			this.filterPanel.MinimumSize = new System.Drawing.Size(850, 2);
			this.filterPanel.Name = "filterPanel";
			this.filterPanel.Size = new System.Drawing.Size(1010, 59);
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
			this.searchPanel.Size = new System.Drawing.Size(1010, 28);
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
			this.ParentSplitContainer.Size = new System.Drawing.Size(1152, 488);
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
			this.MainSplitContainer.Size = new System.Drawing.Size(680, 466);
			this.MainSplitContainer.SplitterDistance = 135;
			this.MainSplitContainer.TabIndex = 0;
			// 
			// CustomPanel
			// 
			this.CustomPanel.BackColor = System.Drawing.SystemColors.Control;
			this.CustomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomPanel.Location = new System.Drawing.Point(0, 80);
			this.CustomPanel.Name = "CustomPanel";
			this.CustomPanel.Size = new System.Drawing.Size(131, 382);
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
			this.tabControlViewer.Size = new System.Drawing.Size(464, 484);
			this.tabControlViewer.TabIndex = 0;
			// 
			// buttonNextBlock
			// 
			this.buttonNextBlock.BackColor = System.Drawing.Color.LightGray;
			this.buttonNextBlock.Dock = System.Windows.Forms.DockStyle.Right;
			this.buttonNextBlock.Location = new System.Drawing.Point(1127, 0);
			this.buttonNextBlock.MaximumSize = new System.Drawing.Size(21, 2000);
			this.buttonNextBlock.MinimumSize = new System.Drawing.Size(21, 25);
			this.buttonNextBlock.Name = "buttonNextBlock";
			this.buttonNextBlock.Size = new System.Drawing.Size(21, 88);
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
			this.splitContainerTop.Panel1.Controls.Add(this.searchPanel);
			// 
			// splitContainerTop.Panel2
			// 
			this.splitContainerTop.Panel2.Controls.Add(this.DgvFileProcessStatus);
			this.splitContainerTop.Panel2MinSize = 0;
			this.splitContainerTop.Size = new System.Drawing.Size(1127, 88);
			this.splitContainerTop.SplitterDistance = 1010;
			this.splitContainerTop.TabIndex = 36;
			// 
			// DgvFileProcessStatus
			// 
			this.DgvFileProcessStatus.AllowUserToAddRows = false;
			this.DgvFileProcessStatus.AllowUserToDeleteRows = false;
			this.DgvFileProcessStatus.AllowUserToResizeRows = false;
			this.DgvFileProcessStatus.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.DgvFileProcessStatus.ColumnHeadersVisible = false;
			this.DgvFileProcessStatus.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ProcessingSelectColumn,
            this.ProcessingImageColumn,
            this.ProcessingFlowColumn,
            this.ProcessingThreadId,
            this.ProcessingCount,
            this.ProcessingSchemeNameColumn,
            this.ProcessingFileColumn});
			this.DgvFileProcessStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DgvFileProcessStatus.Location = new System.Drawing.Point(0, 0);
			this.DgvFileProcessStatus.Name = "DgvFileProcessStatus";
			this.DgvFileProcessStatus.ReadOnly = true;
			this.DgvFileProcessStatus.RowHeadersVisible = false;
			this.DgvFileProcessStatus.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.DgvFileProcessStatus.Size = new System.Drawing.Size(113, 88);
			this.DgvFileProcessStatus.TabIndex = 50;
			// 
			// ProcessingSelectColumn
			// 
			this.ProcessingSelectColumn.HeaderText = "#";
			this.ProcessingSelectColumn.MinimumWidth = 25;
			this.ProcessingSelectColumn.Name = "ProcessingSelectColumn";
			this.ProcessingSelectColumn.ReadOnly = true;
			this.ProcessingSelectColumn.Width = 25;
			// 
			// ProcessingImageColumn
			// 
			this.ProcessingImageColumn.HeaderText = "Status";
			this.ProcessingImageColumn.MinimumWidth = 45;
			this.ProcessingImageColumn.Name = "ProcessingImageColumn";
			this.ProcessingImageColumn.ReadOnly = true;
			this.ProcessingImageColumn.Width = 45;
			// 
			// ProcessingFlowColumn
			// 
			this.ProcessingFlowColumn.HeaderText = "Flow";
			this.ProcessingFlowColumn.MinimumWidth = 45;
			this.ProcessingFlowColumn.Name = "ProcessingFlowColumn";
			this.ProcessingFlowColumn.ReadOnly = true;
			this.ProcessingFlowColumn.Width = 45;
			// 
			// ProcessingThreadId
			// 
			this.ProcessingThreadId.HeaderText = "ProcessingThreadId";
			this.ProcessingThreadId.Name = "ProcessingThreadId";
			this.ProcessingThreadId.ReadOnly = true;
			// 
			// ProcessingCount
			// 
			this.ProcessingCount.HeaderText = "ProcessingCount";
			this.ProcessingCount.Name = "ProcessingCount";
			this.ProcessingCount.ReadOnly = true;
			// 
			// ProcessingSchemeNameColumn
			// 
			this.ProcessingSchemeNameColumn.HeaderText = "SchemeName";
			this.ProcessingSchemeNameColumn.Name = "ProcessingSchemeNameColumn";
			this.ProcessingSchemeNameColumn.ReadOnly = true;
			this.ProcessingSchemeNameColumn.Visible = false;
			// 
			// ProcessingFileColumn
			// 
			this.ProcessingFileColumn.HeaderText = "File";
			this.ProcessingFileColumn.Name = "ProcessingFileColumn";
			this.ProcessingFileColumn.ReadOnly = true;
			this.ProcessingFileColumn.Width = 1000;
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.HeaderText = "ProcessingThreadId";
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.HeaderText = "ProcessingCount";
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			// 
			// dataGridViewTextBoxColumn3
			// 
			this.dataGridViewTextBoxColumn3.HeaderText = "SchemeName";
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			this.dataGridViewTextBoxColumn3.Visible = false;
			// 
			// dataGridViewTextBoxColumn4
			// 
			this.dataGridViewTextBoxColumn4.HeaderText = "File";
			this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			this.dataGridViewTextBoxColumn4.Width = 1000;
			// 
			// textAndImageColumn1
			// 
			this.textAndImageColumn1.Image = null;
			this.textAndImageColumn1.MinimumWidth = 23;
			this.textAndImageColumn1.Name = "textAndImageColumn1";
			this.textAndImageColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.textAndImageColumn1.Width = 23;
			// 
			// dataGridViewTextBoxColumn5
			// 
			this.dataGridViewTextBoxColumn5.MinimumWidth = 25;
			this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
			this.dataGridViewTextBoxColumn5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.dataGridViewTextBoxColumn5.Width = 30;
			// 
			// dataGridViewTextBoxColumn6
			// 
			this.dataGridViewTextBoxColumn6.MinimumWidth = 45;
			this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
			this.dataGridViewTextBoxColumn6.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.dataGridViewTextBoxColumn6.Width = 50;
			// 
			// textAndImageColumn2
			// 
			this.textAndImageColumn2.Image = null;
			this.textAndImageColumn2.MinimumWidth = 70;
			this.textAndImageColumn2.Name = "textAndImageColumn2";
			this.textAndImageColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.textAndImageColumn2.Width = 80;
			// 
			// dataGridViewTextBoxColumn7
			// 
			this.dataGridViewTextBoxColumn7.MinimumWidth = 37;
			this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
			this.dataGridViewTextBoxColumn7.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.dataGridViewTextBoxColumn7.Width = 140;
			// 
			// dataGridViewTextBoxColumn8
			// 
			this.dataGridViewTextBoxColumn8.MinimumWidth = 46;
			this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
			this.dataGridViewTextBoxColumn8.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.dataGridViewTextBoxColumn8.Width = 46;
			// 
			// dataGridViewTextBoxColumn9
			// 
			this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
			this.dataGridViewTextBoxColumn9.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.dataGridViewTextBoxColumn9.Visible = false;
			this.dataGridViewTextBoxColumn9.Width = 5;
			// 
			// dataGridViewTextBoxColumn10
			// 
			this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
			this.dataGridViewTextBoxColumn10.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.dataGridViewTextBoxColumn10.Visible = false;
			this.dataGridViewTextBoxColumn10.Width = 5;
			// 
			// dataGridViewTextBoxColumn11
			// 
			this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
			this.dataGridViewTextBoxColumn11.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.dataGridViewTextBoxColumn11.Visible = false;
			this.dataGridViewTextBoxColumn11.Width = 5;
			// 
			// dataGridViewTextBoxColumn12
			// 
			this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
			this.dataGridViewTextBoxColumn12.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.dataGridViewTextBoxColumn12.Visible = false;
			this.dataGridViewTextBoxColumn12.Width = 5;
			// 
			// dataGridViewTextBoxColumn13
			// 
			this.dataGridViewTextBoxColumn13.MinimumWidth = 300;
			this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
			this.dataGridViewTextBoxColumn13.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.dataGridViewTextBoxColumn13.Width = 1000;
			// 
			// splitContainer1
			// 
			this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainerTop);
			this.splitContainer1.Panel1.Controls.Add(this.buttonNextBlock);
			this.splitContainer1.Panel1MinSize = 92;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ParentSplitContainer);
			this.splitContainer1.Panel2MinSize = 50;
			this.splitContainer1.Size = new System.Drawing.Size(1152, 584);
			this.splitContainer1.SplitterDistance = 92;
			this.splitContainer1.TabIndex = 37;
			// 
			// LogsReaderFormBase
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
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
			((System.ComponentModel.ISupportInitialize)(this.DgvFileProcessStatus)).EndInit();
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
        private TextAndImageColumn PromptColumn;
        private DataGridViewTextBoxColumn IDColumn;
        private DataGridViewTextBoxColumn ServerColumn;
        private TextAndImageColumn TraceNameColumn;
        private DataGridViewTextBoxColumn DateOfTraceColumn;
        private DataGridViewTextBoxColumn ElapsedSecColumn;
        private DataGridViewTextBoxColumn SchemeNameColumn;
        private DataGridViewTextBoxColumn PrivateIDColumn;
        private DataGridViewTextBoxColumn IsSuccessColumn;
        private DataGridViewTextBoxColumn IsFilteredColumn;
		private DataGridViewTextBoxColumn FileColumn;
        private CustomTabControl tabControlViewer;

		protected CustomDataGridView DgvData;
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
		protected DataGridView DgvFileProcessStatus;
		private DataGridViewCheckBoxColumn ProcessingSelectColumn;
		private DataGridViewImageColumn ProcessingImageColumn;
		private DataGridViewButtonColumn ProcessingFlowColumn;
		private DataGridViewTextBoxColumn ProcessingThreadId;
		private DataGridViewTextBoxColumn ProcessingCount;
		private DataGridViewTextBoxColumn ProcessingSchemeNameColumn;
		private DataGridViewTextBoxColumn ProcessingFileColumn;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
		private TextAndImageColumn textAndImageColumn1;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
		private TextAndImageColumn textAndImageColumn2;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
		private SplitContainer splitContainer1;
	}
}

