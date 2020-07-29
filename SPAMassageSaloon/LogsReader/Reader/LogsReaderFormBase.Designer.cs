
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
			this.DgvData = new SPAMassageSaloon.Common.CustomDataGridView();
			this.Prompt = new LogsReader.TextAndImageColumn();
			this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Server = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.TraceName = new LogsReader.TextAndImageColumn();
			this.DateOfTrace = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ElapsedSec = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.SchemeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.PrivateID = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.IsSuccess = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.IsFiltered = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.File = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
			this.SuspendLayout();
			// 
			// DgvData
			// 
			this.DgvData.AllowUserToAddRows = false;
			this.DgvData.AllowUserToDeleteRows = false;
			this.DgvData.AllowUserToResizeRows = false;
			this.DgvData.BackgroundColor = System.Drawing.SystemColors.Window;
			this.DgvData.BorderStyle = System.Windows.Forms.BorderStyle.None;
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.DgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
			this.DgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.DgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.Prompt,
			this.ID,
            this.Server,
            this.TraceName,
            this.DateOfTrace,
            this.ElapsedSec,
            this.SchemeName,
            this.PrivateID,
            this.IsSuccess,
            this.IsFiltered,
            this.File});
			dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.DgvData.DefaultCellStyle = dataGridViewCellStyle5;
			this.DgvData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DgvData.GridColor = System.Drawing.SystemColors.ControlLight;
			this.DgvData.Location = new System.Drawing.Point(0, 0);
			this.DgvData.Name = "DgvData";
			this.DgvData.ReadOnly = true;
			dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.DgvData.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
			this.DgvData.RowHeadersVisible = false;
			this.DgvData.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.DgvData.RowTemplate.Height = 18;
			this.DgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.DgvData.Size = new System.Drawing.Size(537, 466);
			this.DgvData.TabIndex = 20;
			this.DgvData.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvData_CellDoubleClick);
			this.DgvData.SelectionChanged += new System.EventHandler(this.DgvData_SelectionChanged);
			this.DgvData.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DgvData_MouseDown);
			// 
			// Prompt
			// 
			this.Prompt.Image = null;
			this.Prompt.MinimumWidth = 23;
			this.Prompt.Name = "Prompt";
			this.Prompt.ReadOnly = true;
			this.Prompt.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.Prompt.Width = 23;
			// 
			// ID
			// 
			this.ID.MinimumWidth = 25;
			this.ID.Name = "ID";
			this.ID.ReadOnly = true;
			this.ID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.ID.Width = 30;
			// 
			// Server
			// 
			this.Server.MinimumWidth = 45;
			this.Server.Name = "Server";
			this.Server.ReadOnly = true;
			this.Server.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.Server.Width = 50;
			// 
			// TraceName
			//
			this.TraceName.Image = null;
			this.TraceName.MinimumWidth = 70;
			this.TraceName.Name = "TraceName";
			this.TraceName.ReadOnly = true;
			this.TraceName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.TraceName.Width = 80;
			// 
			// DateOfTrace
			// 
			this.DateOfTrace.MinimumWidth = 37;
			this.DateOfTrace.Name = "DateOfTrace";
			this.DateOfTrace.ReadOnly = true;
			this.DateOfTrace.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.DateOfTrace.Width = 140;
			// 
			// ElapsedSec
			// 
			this.ElapsedSec.MinimumWidth = 46;
			this.ElapsedSec.Name = "ElapsedSec";
			this.ElapsedSec.ReadOnly = true;
			this.ElapsedSec.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.ElapsedSec.Width = 46;
			// 
			// SchemeName
			// 
			this.SchemeName.Name = "SchemeName";
			this.SchemeName.ReadOnly = true;
			this.SchemeName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.SchemeName.Visible = false;
			this.SchemeName.Width = 5;
			// 
			// PrivateID
			// 
			this.PrivateID.Name = "PrivateID";
			this.PrivateID.ReadOnly = true;
			this.PrivateID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.PrivateID.Visible = false;
			this.PrivateID.Width = 5;
			// 
			// IsSuccess
			// 
			this.IsSuccess.Name = "IsSuccess";
			this.IsSuccess.ReadOnly = true;
			this.IsSuccess.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.IsSuccess.Visible = false;
			this.IsSuccess.Width = 5;
			// 
			// IsFiltered
			// 
			this.IsFiltered.Name = "IsFiltered";
			this.IsFiltered.ReadOnly = true;
			this.IsFiltered.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.IsFiltered.Visible = false;
			this.IsFiltered.Width = 5;
			// 
			// File
			// 
			this.File.MinimumWidth = 300;
			this.File.Name = "File";
			this.File.ReadOnly = true;
			this.File.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.File.Width = 1000;
			// 
			// BtnSearch
			// 
			this.BtnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BtnSearch.BackColor = System.Drawing.SystemColors.Control;
			this.BtnSearch.Image = global::LogsReader.Properties.Resources.find;
			this.BtnSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.BtnSearch.Location = new System.Drawing.Point(943, 2);
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
			this.btnClear.Location = new System.Drawing.Point(1039, 2);
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
			this.TbxPattern.Size = new System.Drawing.Size(840, 20);
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
			// ChbxUseRegex
			// 
			this.ChbxUseRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ChbxUseRegex.AutoSize = true;
			this.ChbxUseRegex.Location = new System.Drawing.Point(858, 6);
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
			this.label7.Location = new System.Drawing.Point(225, 11);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(63, 13);
			this.label7.TabIndex = 24;
			this.label7.Text = "TraceName";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(4, 37);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(52, 13);
			this.label9.TabIndex = 23;
			this.label9.Text = "Date End";
			// 
			// TbxTraceNameFilter
			// 
			this.TbxTraceNameFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TbxTraceNameFilter.Location = new System.Drawing.Point(403, 7);
			this.TbxTraceNameFilter.Name = "TbxTraceNameFilter";
			this.TbxTraceNameFilter.Size = new System.Drawing.Size(444, 20);
			this.TbxTraceNameFilter.TabIndex = 9;
			this.TbxTraceNameFilter.TextChanged += new System.EventHandler(this.TbxTraceNameFilterOnTextChanged);
			// 
			// DateEndFilter
			// 
			this.DateEndFilter.Checked = false;
			this.DateEndFilter.CustomFormat = "dd.MM.yyyy HH:mm:ss";
			this.DateEndFilter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.DateEndFilter.Location = new System.Drawing.Point(68, 34);
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
			this.DateStartFilter.Location = new System.Drawing.Point(68, 7);
			this.DateStartFilter.Name = "DateStartFilter";
			this.DateStartFilter.ShowCheckBox = true;
			this.DateStartFilter.ShowUpDown = true;
			this.DateStartFilter.Size = new System.Drawing.Size(151, 20);
			this.DateStartFilter.TabIndex = 5;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(4, 11);
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
			this.btnFilter.Location = new System.Drawing.Point(853, 7);
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
			this.btnReset.Location = new System.Drawing.Point(962, 7);
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
			this.label11.Location = new System.Drawing.Point(225, 37);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(54, 13);
			this.label11.TabIndex = 30;
			this.label11.Text = "Full Trace";
			// 
			// TbxTraceMessageFilter
			// 
			this.TbxTraceMessageFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TbxTraceMessageFilter.Location = new System.Drawing.Point(403, 34);
			this.TbxTraceMessageFilter.Name = "TbxTraceMessageFilter";
			this.TbxTraceMessageFilter.Size = new System.Drawing.Size(444, 20);
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
			this.filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.filterPanel.Location = new System.Drawing.Point(0, 28);
			this.filterPanel.MinimumSize = new System.Drawing.Size(850, 2);
			this.filterPanel.Name = "filterPanel";
			this.filterPanel.Size = new System.Drawing.Size(1152, 64);
			this.filterPanel.TabIndex = 28;
			// 
			// buttonHighlightOff
			// 
			this.buttonHighlightOff.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonHighlightOff.BackColor = System.Drawing.Color.Gray;
			this.buttonHighlightOff.Image = global::LogsReader.Properties.Resources.filtered;
			this.buttonHighlightOff.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.buttonHighlightOff.Location = new System.Drawing.Point(906, 33);
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
			this.buttonHighlightOn.Location = new System.Drawing.Point(853, 33);
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
			this.CobxTraceMessageFilter.Location = new System.Drawing.Point(298, 35);
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
			this.CobxTraceNameFilter.Location = new System.Drawing.Point(298, 8);
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
			this.ChbxAlreadyUseFilter.Location = new System.Drawing.Point(962, 37);
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
			this.btnExport.Location = new System.Drawing.Point(1068, 7);
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
			this.ParentSplitContainer.Panel2.Controls.Add(this.tabControlViewer);
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
			this.MainSplitContainer.Size = new System.Drawing.Size(680, 470);
			this.MainSplitContainer.SplitterDistance = 135;
			this.MainSplitContainer.TabIndex = 0;
			// 
			// CustomPanel
			// 
			this.CustomPanel.BackColor = System.Drawing.SystemColors.Control;
			this.CustomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomPanel.Location = new System.Drawing.Point(0, 80);
			this.CustomPanel.Name = "CustomPanel";
			this.CustomPanel.Size = new System.Drawing.Size(131, 386);
			this.CustomPanel.TabIndex = 1;
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
			this.tabControlViewer.Size = new System.Drawing.Size(464, 488);
			this.tabControlViewer.TabIndex = 0;
			// 
			// LogsReaderFormBase
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ParentSplitContainer);
			this.Controls.Add(this.filterPanel);
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
        private TextAndImageColumn Prompt;
        private DataGridViewTextBoxColumn ID;
        private DataGridViewTextBoxColumn Server;
        private TextAndImageColumn TraceName;
        private DataGridViewTextBoxColumn DateOfTrace;
        private DataGridViewTextBoxColumn ElapsedSec;
        private DataGridViewTextBoxColumn SchemeName;
        private DataGridViewTextBoxColumn PrivateID;
        private DataGridViewTextBoxColumn IsSuccess;
        private DataGridViewTextBoxColumn IsFiltered;
		private DataGridViewTextBoxColumn File;
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
	}
}

