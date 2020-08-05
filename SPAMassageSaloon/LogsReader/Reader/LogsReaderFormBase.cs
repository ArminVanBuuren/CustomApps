using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using SPAMassageSaloon.Common;
using Utils;
using Utils.WinForm;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader.Reader
{
	public abstract partial class LogsReaderFormBase : UserControl, IUserForm
	{
		private readonly Func<DateTime> _getStartDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        private readonly Func<DateTime> _getEndDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

        private readonly object syncRootStatus = new object();

		private bool _settingIsApplied;
        private bool _oldDateStartChecked;
        private bool _oldDateEndChecked;
        private bool _settingsLoaded;
        private bool _isWorking;

        private int _countMatches;
        private int _countErrorMatches;
        private int _filesCompleted;
        private int _totalFiles;

        private IEnumerable<DataTemplate> _currentDGVResult;
        private DataGridViewColumn _oldSortedColumn;
        private bool _byAscending = true;

        private Color _formBackColor;
        private Color _formForeColor;

        private readonly ToolStripStatusLabel _statusInfo;
        private readonly ToolStripStatusLabel _findedInfo;
        private readonly ToolStripStatusLabel _completedFilesStatus;
        private readonly ToolStripStatusLabel _totalFilesStatus;
        private readonly ToolStripStatusLabel _filtersCompleted1;
        private readonly ToolStripStatusLabel _filtersCompleted2;
        private readonly ToolStripStatusLabel _overallFound;
        private readonly ToolStripStatusLabel _errorFound;
        private readonly ToolStripStatusLabel _errorFoundValue;

        private TransactionsMarkingType _currentTransactionsMarkingType = TransactionsMarkingType.None;
        private TransactionsMarkingType _prevTransactionsMarkingType = TransactionsMarkingType.None;

        protected static Image Img_Failed { get; private set; }

        protected static Image Img_Filtered { get; private set; }

        protected static Image Img_Selected { get; private set; }

        protected static Image Img_Failed_Filtered { get; private set; }

        /// <summary>
        /// Юзерские настройки 
        /// </summary>
        protected UserSettings UserSettings { get; }

        protected Stopwatch TimeWatcher { get; set; } = new Stopwatch();

        protected ToolTip Tooltip { get; }

        protected bool ButtonHighlightEnabled
        {
	        get => buttonHighlightOn.Enabled;
	        set => buttonHighlightOn.Enabled = buttonHighlightOff.Enabled = value;
        }

        protected abstract TransactionsMarkingType DefaultTransactionsMarkingType { get; }

        protected TransactionsMarkingType CurrentTransactionsMarkingType
        {
	        get => _currentTransactionsMarkingType;
	        set
	        {
		        _currentTransactionsMarkingType = value;
		        _prevTransactionsMarkingType = value == TransactionsMarkingType.None ? DefaultTransactionsMarkingType : value;
                try
		        {
			        checkBoxShowTrns.CheckedChanged -= checkBoxShowTrns_CheckedChanged;
			        checkBoxShowTrns.Checked = value != TransactionsMarkingType.None;
                    checkBoxShowTrns_CheckedChanged(this, EventArgs.Empty);
		        }
		        catch (Exception ex)
		        {
					ReportStatus(ex.Message, ReportStatusType.Error);
				}
		        finally
		        {
			        checkBoxShowTrns.CheckedChanged += checkBoxShowTrns_CheckedChanged;
                }
	        }
        }

        /// <summary>
        /// При загрузке ридеров
        /// </summary>
        public event EventHandler OnUploadReaders;

		/// <summary>
		/// Поиск логов начался или завершился
		/// </summary>
		public event EventHandler OnProcessStatusChanged;

        /// <summary>
        /// При смене языка
        /// </summary>
        public event EventHandler OnAppliedSettings;

        /// <summary>
        /// Статус выполнения поиска
        /// </summary>
        public bool IsWorking
        {
	        get => _isWorking;
	        protected set
	        {
		        _isWorking = value;
		        ChangeFormStatus();
	        }
        }

        public Encoding DefaultEncoding { get; }

        public Color FormBackColor
        {
	        get => _formBackColor;
	        set => _formBackColor = UserSettings.BackColor = value;
        }

        public Color FormForeColor
        {
	        get => _formForeColor;
	        set => _formForeColor = UserSettings.ForeColor = value;
        }

        public int CountMatches
        {
	        get => _countMatches;
	        private set
	        {
		        _countMatches = value;
		        _findedInfo.Text = _countMatches.ToString();
	        }
        }

        public int CountErrorMatches
        {
	        get => _countErrorMatches;
	        private set
	        {
		        _countErrorMatches = value;
		        _errorFoundValue.Text = _countErrorMatches.ToString();
	        }
        }

        public int Progress
        {
	        get => IsWorking ? progressBar.Value : 100;
	        protected set => progressBar.Value = value;
        }

        public int FilesCompleted
        {
	        get => _filesCompleted;
	        private set
	        {
		        _filesCompleted = value;
		        _completedFilesStatus.Text = _filesCompleted.ToString();
	        }
        }

        public int TotalFiles
        {
	        get => _totalFiles;
	        private set
	        {
		        _totalFiles = value;
		        _totalFilesStatus.Text = _totalFiles.ToString();
	        }
        }

        public int CustomPanelMinSize
        {
	        get => MainSplitContainer.Panel1MinSize;
            set => MainSplitContainer.Panel1MinSize = value;
        }

		/// <summary>
		/// Если выполнили фильтр (не подсвечивая)
		/// </summary>
        protected bool IsDgvDataFiltered { get; private set; } = false;

		public abstract bool HasAnyResult { get; }

        public TraceItemView MainViewer { get; }

        static LogsReaderFormBase()
        {
	        Image GetImage(IEnumerable<Image> images)
	        {
		        var bmp = new Bitmap(images.Sum(x => x.Width), images.Max(x => x.Height));
		        using (var g = Graphics.FromImage(bmp))
		        {
			        var x = 0;
			        foreach (var img in images)
			        {
				        g.DrawImage(img, x, 0, img.Width, img.Height);
				        x += img.Width;
			        }
		        }

		        return bmp;
	        }

	        Img_Failed = Resources.Error1;
	        Img_Filtered = Resources.filtered;
	        Img_Selected = Resources.trn;
	        Img_Failed_Filtered = GetImage(new[] { Resources.Error1, Resources.filtered });
        }

        protected LogsReaderFormBase(Encoding defaultEncoding, UserSettings userSettings)
        {
	        InitializeComponent();
	        buttonNextBlock_Click(null, EventArgs.Empty);

            DefaultEncoding = defaultEncoding;
            UserSettings = userSettings;

            _formBackColor = UserSettings.BackColor;
            _formForeColor = UserSettings.ForeColor;

            base.Font = LogsReaderMainForm.DefFont;
	        ChbxAlreadyUseFilter.Font = LogsReaderMainForm.DefFont;
	        tabControlViewer.Font = LogsReaderMainForm.DgvDataFont;

            #region Initialize StripStatus

            Tooltip = new ToolTip
            {
	            AutoPopDelay = 15000,
				InitialDelay = 100,
				ReshowDelay = 500
			};

            var statusStripItemsPaddingStart = new Padding(0, 2, 0, 2);
            var statusStripItemsPaddingMiddle = new Padding(-3, 2, 0, 2);
            var statusStripItemsPaddingEnd = new Padding(-3, 2, 1, 2);

            _filtersCompleted1 = new ToolStripStatusLabel { Font = base.Font, Margin = statusStripItemsPaddingStart };
            _completedFilesStatus = new ToolStripStatusLabel("0") { Font = base.Font, Margin = statusStripItemsPaddingMiddle };
            _filtersCompleted2 = new ToolStripStatusLabel { Font = base.Font, Margin = statusStripItemsPaddingMiddle };
            _totalFilesStatus = new ToolStripStatusLabel("0") { Font = base.Font, Margin = statusStripItemsPaddingEnd };
            statusStrip.Items.Add(_filtersCompleted1);
            statusStrip.Items.Add(_completedFilesStatus);
            statusStrip.Items.Add(_filtersCompleted2);
            statusStrip.Items.Add(_totalFilesStatus);

            _overallFound = new ToolStripStatusLabel { Font = base.Font, Margin = statusStripItemsPaddingStart };
            _findedInfo = new ToolStripStatusLabel("0") { Font = base.Font, Margin = statusStripItemsPaddingMiddle };
            _errorFound = new ToolStripStatusLabel { Font = base.Font, Margin = statusStripItemsPaddingEnd };
            _errorFoundValue = new ToolStripStatusLabel("0") { Font = base.Font, Margin = statusStripItemsPaddingMiddle };
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(_overallFound);
            statusStrip.Items.Add(_findedInfo);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(_errorFound);
            statusStrip.Items.Add(_errorFoundValue);

            statusStrip.Items.Add(new ToolStripSeparator());
            _statusInfo = new ToolStripStatusLabel("") { Font = new Font(LogsReaderMainForm.MainFontFamily, 8.5F, FontStyle.Bold), Margin = statusStripItemsPaddingStart };
            statusStrip.Items.Add(_statusInfo);

            #endregion

            try
            {
                #region Initialize DgvData

                DgvData.AutoGenerateColumns = false;
                DgvData.TabStop = false;
                DgvData.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
	            DgvData.DefaultCellStyle.Font = LogsReaderMainForm.DgvDataFont;
	            DgvData.Font = LogsReaderMainForm.DgvDataFont;
	            DgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
	            DgvData.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
	            foreach (DataGridViewColumn c in DgvData.Columns)
		            c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
	            DgvData.CellDoubleClick += DgvData_CellDoubleClick;
	            DgvData.SelectionChanged += DgvData_SelectionChanged;
	            DgvData.MouseDown += DgvData_MouseDown;
				DgvData.CellFormatting += DataGridViewOnCellFormatting;
                DgvData.ColumnHeaderMouseClick += DgvDataOnColumnHeaderMouseClick;
                DgvData.DataBindingComplete += (sender, args) => DgvData.ClearSelection();
                DgvData.ColumnWidthChanged += (sender, args) =>
                {
	                if (DgvDataPromptColumn.Width > DgvDataPromptColumn.MinimumWidth)
		                DgvDataPromptColumn.Width = DgvDataPromptColumn.MinimumWidth;
                };

                DgvDataPromptColumn.DataPropertyName = DgvDataPromptColumn.Name = nameof(DataTemplate.Tmp.Prompt);
                DgvDataPromptColumn.HeaderText = DataTemplate.HeaderPrompt;

                DgvDataIDColumn.DataPropertyName = DgvDataIDColumn.Name = DgvDataIDColumn.HeaderText = nameof(DataTemplate.Tmp.ID);

                DgvDataServerColumn.DataPropertyName = DgvDataServerColumn.Name = DgvDataServerColumn.HeaderText = nameof(DataTemplate.Tmp.Server);

                DgvDataTraceNameColumn.DataPropertyName = DgvDataTraceNameColumn.Name = nameof(DataTemplate.Tmp.TraceName);
                DgvDataTraceNameColumn.HeaderText = nameof(DataTemplate.Tmp.TraceName);

                DgvDataDateOfTraceColumn.DataPropertyName = DgvDataDateOfTraceColumn.Name = nameof(DataTemplate.Tmp.DateString);
                DgvDataDateOfTraceColumn.HeaderText = nameof(DataTemplate.Tmp.Date);

                DgvDataElapsedSecColumn.DataPropertyName = DgvDataElapsedSecColumn.Name = nameof(DataTemplate.Tmp.ElapsedSecString);
                DgvDataElapsedSecColumn.HeaderText = nameof(DataTemplate.Tmp.ElapsedSec);

                DgvDataSchemeNameColumn.DataPropertyName = DgvDataSchemeNameColumn.Name = nameof(DataTemplate.Tmp.SchemeName);// not visible

                DgvDataPrivateIDColumn.DataPropertyName = DgvDataPrivateIDColumn.Name = nameof(DataTemplate.Tmp.PrivateID); // not visible

                DgvDataIsSuccessColumn.DataPropertyName = DgvDataIsSuccessColumn.Name = nameof(DataTemplate.Tmp.IsSuccess); // not visible

                DgvDataIsFilteredColumn.DataPropertyName = DgvDataIsFilteredColumn.Name = nameof(DataTemplate.Tmp.IsFiltered); // not visible

                DgvDataFileColumn.DataPropertyName = DgvDataFileColumn.Name = nameof(DataTemplate.Tmp.FileNamePartial);
                DgvDataFileColumn.HeaderText = nameof(DataTemplate.Tmp.File);

	            label7.Text = nameof(DataTemplate.Tmp.TraceName);
                label11.Text = DataTemplate.HeaderTraceMessage;

                #endregion

                #region Initialize DgvProcessing

                DgvReader.AutoGenerateColumns = false;
                DgvReader.TabStop = false;
                DgvReader.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
                DgvReader.DefaultCellStyle.Font = LogsReaderMainForm.DgvReaderFont;
                DgvReader.Font = LogsReaderMainForm.DgvReaderFont;
                DgvReader.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                DgvReader.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                foreach (DataGridViewColumn c in DgvReader.Columns)
	                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                DgvReaderProcessColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
				DgvReaderAbortColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
				DgvReader.ColumnHeaderMouseClick += (sender, args) => { RefreshAllRows(DgvReader, DgvReaderRefreshRow); };
				DgvReader.ColumnHeaderMouseDoubleClick += (sender, args) => { RefreshAllRows(DgvReader, DgvReaderRefreshRow); };
				DgvReader.DataBindingComplete += (sender, args) => DgvReader.ClearSelection();
				DgvReader.CellContentClick += (sender, args) =>
				{
					if (args.RowIndex < 0)
						return;

					var row = DgvReader.Rows[args.RowIndex];

					if (args.ColumnIndex == DgvReaderAbortColumn.Index
					    && row.Cells[DgvReaderAbortColumn.Name] is DgvDisableButtonCell cellAbort
					    && cellAbort.Enabled
					    && TryGetReader(DgvReader.Rows[args.RowIndex], out var reader1))
					{
						if (reader1.Status == TraceReaderStatus.Waiting || reader1.Status == TraceReaderStatus.Processing || reader1.Status == TraceReaderStatus.OnPause)
							reader1.Status = TraceReaderStatus.Aborted;
						RefreshAllRows(DgvReader, DgvReaderRefreshRow);
						return;
					}

					if (args.ColumnIndex == DgvReaderProcessColumn.Index
					    && row.Cells[DgvReaderProcessColumn.Name] is DgvDisableButtonCell cellPauseResume
					    && cellPauseResume.Enabled
					    && TryGetReader(DgvReader.Rows[args.RowIndex], out var reader2))
					{
						switch (reader2.Status)
						{
							case TraceReaderStatus.OnPause:
								reader2.Status = reader2.ThreadId.IsNullOrEmpty() ? TraceReaderStatus.Waiting : TraceReaderStatus.Processing;
								break;
							case TraceReaderStatus.Waiting:
							case TraceReaderStatus.Processing:
								reader2.Status = TraceReaderStatus.OnPause;
								break;
						}
						RefreshAllRows(DgvReader, DgvReaderRefreshRow);
						return;
					}

				};

                #endregion

                #region Apply All Settings

                DateStartFilter.ValueChanged += DateStartFilterOnValueChanged;
                DateEndFilter.ValueChanged += DateEndFilterOnValueChanged;

                TbxPattern.AssignValue(UserSettings.PreviousSearch, TxtPatternOnTextChanged);
                ChbxUseRegex.Checked = UserSettings.UseRegex;
                DateStartFilter.Checked = UserSettings.DateStartChecked;
                if (DateStartFilter.Checked)
                    DateStartFilter.Value = _getStartDate.Invoke();
                DateEndFilter.Checked = UserSettings.DateEndChecked;
                if (DateEndFilter.Checked)
                    DateEndFilter.Value = _getEndDate.Invoke();
                TbxTraceNameFilter.AssignValue(UserSettings.TraceNameFilter, TbxTraceNameFilterOnTextChanged);
                TbxTraceMessageFilter.AssignValue(UserSettings.TraceMessageFilter, TbxTraceMessageFilterOnTextChanged);

                MainViewer = new TraceItemView(defaultEncoding, userSettings, true);
                tabControlViewer.DrawMode = TabDrawMode.Normal;
                tabControlViewer.BackColor = Color.White;
                AddViewer(MainViewer, null);

                #endregion
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, Resources.Txt_Initialization);
            }
        }

        void AddViewer(TraceItemView traceViewer, DataTemplate template)
        {
	        var tabPage = new CustomTabPage(traceViewer)
            {
	            UseVisualStyleBackColor = true,
		        ForeColor = Color.Black,
		        Margin = new Padding(0),
		        Padding = new Padding(0),
		        BorderStyle = BorderStyle.None,
		        CanClose = !traceViewer.IsMain
            };

	        if (!traceViewer.IsMain && MainViewer != null)
		        traceViewer.SplitterDistance = MainViewer.SplitterDistance;
            traceViewer.Dock = DockStyle.Fill;
	        traceViewer.ChangeTemplate(template, checkBoxShowTrns.Checked, out var _);

	        tabControlViewer.TabPages.Add(tabPage);
        }

        public void ApplyFormSettings()
        {
	        try
	        {
		        if (UserSettings == null)
			        return;

		        for (var i = 0; i < DgvData.Columns.Count; i++)
		        {
			        var valueStr = UserSettings.GetValue("COL" + i);
			        if (!valueStr.IsNullOrWhiteSpace() && int.TryParse(valueStr, out var value) && value > 1 && value <= 1000)
				        DgvData.Columns[i].Width = value;
		        }

		        ParentSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(ParentSplitContainer), 25, 2000, ParentSplitContainer.SplitterDistance);
		        MainSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(MainSplitContainer), 25, 2000, MainSplitContainer.SplitterDistance);
		        MainViewer.SplitterDistance = UserSettings.GetValue(nameof(MainViewer), 25, 2000, MainViewer.SplitterDistance);

		        ParentSplitContainer.SplitterMoved += (sender, args) => { SaveData(); };
		        MainSplitContainer.SplitterMoved += (sender, args) => { SaveData(); };
		        MainViewer.SplitterMoved += (sender, args) => { SaveData(); };

		        _settingIsApplied = true;
            }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        }
        }

        #region Change Language

		public virtual void ApplySettings()
        {
            try
            {
	            _filtersCompleted1.Text = Resources.Txt_LogsReaderForm_FilesCompleted_1;
                _filtersCompleted2.Text = Resources.Txt_LogsReaderForm_FilesCompleted_2;
                _overallFound.Text = Resources.Txt_LogsReaderForm_OverallFound;
                _errorFound.Text = Resources.Txt_LogsReaderForm_Error;

                CobxTraceNameFilter.Items.Clear();
                CobxTraceNameFilter.Items.Add(Resources.Txt_LogsReaderForm_Contains);
                CobxTraceNameFilter.Items.Add(Resources.Txt_LogsReaderForm_NotContains);
                if (UserSettings != null)
                    CobxTraceNameFilter.AssignValue(UserSettings.TraceNameFilterContains ? Resources.Txt_LogsReaderForm_Contains : Resources.Txt_LogsReaderForm_NotContains,
                        CobxTraceNameFilter_SelectedIndexChanged);

                CobxTraceMessageFilter.Items.Clear();
                CobxTraceMessageFilter.Items.Add(Resources.Txt_LogsReaderForm_Contains);
                CobxTraceMessageFilter.Items.Add(Resources.Txt_LogsReaderForm_NotContains);
                if (UserSettings != null)
                    CobxTraceMessageFilter.AssignValue(UserSettings.TraceMessageFilterContains ? Resources.Txt_LogsReaderForm_Contains : Resources.Txt_LogsReaderForm_NotContains,
                        CobxTraceMessageFilter_SelectedIndexChanged);

                ApplyTooltip();

                ChbxUseRegex.Text = Resources.Txt_LogsReaderForm_UseRegex;
                BtnSearch.Text = IsWorking ? Resources.Txt_LogsReaderForm_Stop : Resources.Txt_LogsReaderForm_Search;
                btnClear.Text = Resources.Txt_LogsReaderForm_Clear;
                btnClear.Size = new Size(Convert.ToInt32(Resources.LogsReaderForm_btnClear_Width), btnClear.Height);
                btnFilter.Text = Resources.Txt_LogsReaderForm_Filter;
                btnReset.Text = Resources.Txt_LogsReaderForm_Reset;
                ChbxAlreadyUseFilter.Text = Resources.Txt_LogsReaderForm_UseFilterWhenSearching;

                DgvReaderSchemeNameColumn.HeaderText = Resources.TxtReader_DgvScheme;
                DgvReaderStatusColumn.HeaderText = Resources.TxtReader_DgvStatus;
                DgvReaderProcessColumn.HeaderText = Resources.TxtReader_DgvProcess;
                DgvReaderAbortColumn.HeaderText = Resources.TxtReader_DgvFlow;
                DgvReaderThreadIdColumn.HeaderText = Resources.TxtReader_DgvThreadID;
                DgvReaderCountMatchesColumn.HeaderText = Resources.TxtReader_DgvMatches;
                DgvReaderCountErrorMatchesColumn.HeaderText = Resources.TxtReader_DgvErrors;
                DgvReaderFileColumn.HeaderText = Resources.TxtReader_DgvFile;
                DgvReaderFileCreationTimeColumn.HeaderText = Resources.TxtReader_DgvCreationTime;
                DgvReaderFileLastWriteTimeColumn.HeaderText = Resources.TxtReader_DgvLastWrite;
                DgvReaderFileSizeColumn.HeaderText = Resources.TxtReader_DgvSize;

                DgvReaderStatusColumn.MinimumWidth = int.Parse(Resources.TxtReader_DgvStatusMinWidth);
                DgvReaderProcessColumn.MinimumWidth = int.Parse(Resources.TxtReader_DgvProcessMinWidth);
                DgvReaderAbortColumn.MinimumWidth = int.Parse(Resources.TxtReader_DgvAbortMinWidth);
                DgvReaderCountMatchesColumn.MinimumWidth = int.Parse(Resources.TxtReader_DgvCountMatchesMinWidth);
                DgvReaderCountErrorMatchesColumn.MinimumWidth = int.Parse(Resources.TxtReader_DgvCountErrorMatchesMinWidth);
                DgvReaderFileLastWriteTimeColumn.MinimumWidth = int.Parse(Resources.TxtReader_DgvCountLastWriteTimeMinWidth);
            }
            catch (Exception ex)
            {
	            ReportStatus(ex.Message, ReportStatusType.Error);
            }
            finally
            {
                _settingsLoaded = true;
                OnAppliedSettings?.Invoke(this, EventArgs.Empty);
            }
        }

        protected virtual void ApplyTooltip()
        {
	        Tooltip.RemoveAll();

	        if (LRSettings.DisableHintTooltip)
		        return;

	        Tooltip.SetToolTip(TbxPattern, Resources.Txt_Form_SearchComment);
	        Tooltip.SetToolTip(ChbxUseRegex, Resources.Txt_LRSettings_UseRegexComment);
	        Tooltip.SetToolTip(DateStartFilter, Resources.Txt_Form_DateFilterComment);
	        Tooltip.SetToolTip(DateEndFilter, Resources.Txt_Form_DateFilterComment);
	        Tooltip.SetToolTip(TbxTraceNameFilter, Resources.Txt_Form_TraceNameFilterComment);
	        Tooltip.SetToolTip(TbxTraceMessageFilter, Resources.Txt_Form_TraceFilterComment);
	        Tooltip.SetToolTip(ChbxAlreadyUseFilter, Resources.Txt_Form_AlreadyUseFilterComment);
	        Tooltip.SetToolTip(btnExport, Resources.Txt_LogsReaderForm_ExportComment);
	        Tooltip.SetToolTip(buttonErrPrev, Resources.Txt_LogsReaderForm_PrevErrButt);
	        Tooltip.SetToolTip(buttonErrNext, Resources.Txt_LogsReaderForm_NextErrButt);
	        Tooltip.SetToolTip(buttonFilteredPrev, Resources.Txt_LogsReaderForm_PrevFilteredButt);
	        Tooltip.SetToolTip(buttonFilteredNext, Resources.Txt_LogsReaderForm_NextFilteredButt);
	        Tooltip.SetToolTip(checkBoxShowTrns, Resources.Txt_Forms_ShowTransactions);
	        Tooltip.SetToolTip(buttonHighlightOn, Resources.Txt_LogsReaderForm_HighlightTxt);
	        Tooltip.SetToolTip(buttonHighlightOff, Resources.Txt_LogsReaderForm_HighlightTxtOff);
        }

        #endregion

		public virtual void SaveData()
        {
            try
            {
                if (!_settingsLoaded || UserSettings == null || !_settingIsApplied)
                    return;

                for (var i = 0; i < DgvData.Columns.Count; i++)
                {
                    UserSettings.SetValue("COL" + i, DgvData.Columns[i].Width);
                }

                UserSettings.SetValue(nameof(ParentSplitContainer), ParentSplitContainer.SplitterDistance);
                UserSettings.SetValue(nameof(MainSplitContainer), MainSplitContainer.SplitterDistance);
                UserSettings.SetValue(nameof(MainViewer), MainViewer.SplitterDistance);
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        public virtual void LogsReaderKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
	                case Keys.F1 when ButtonHighlightEnabled:
		                buttonHighlight_Click(this, EventArgs.Empty);
		                break;
	                case Keys.F2 when ButtonHighlightEnabled:
		                buttonHighlightOff_Click(this, EventArgs.Empty);
		                break;
                    case Keys.F3 when e.Shift:
		                buttonErrorPrev_Click(this, EventArgs.Empty);
		                break;
	                case Keys.F3:
		                buttonErrorNext_Click(this, EventArgs.Empty);
		                break;
	                case Keys.F4 when e.Shift:
		                buttonFilteredPrev_Click(this, EventArgs.Empty);
		                break;
	                case Keys.F4:
		                buttonFilteredNext_Click(this, EventArgs.Empty);
		                break;
                    case Keys.F5 when BtnSearch.Enabled && !IsWorking:
                        BtnSearch_Click(this, EventArgs.Empty);
                        break;
                    case Keys.Escape when BtnSearch.Enabled && IsWorking:
                        BtnSearch_Click(this, EventArgs.Empty);
                        break;
                    case Keys.F6 when btnClear.Enabled:
	                    BtnClear_Click(this, EventArgs.Empty);
                        break;
                    case Keys.F7 when btnFilter.Enabled:
                        BtnFilter_Click(this, EventArgs.Empty);
                        break;
	                case Keys.F8 when btnReset.Enabled:
		                BtnReset_Click(this, EventArgs.Empty);
		                break;
	                case Keys.S when e.Control && btnExport.Enabled:
                        BtnExport_Click(this, EventArgs.Empty);
                        break;
                    case Keys.C when e.Control && DgvData.SelectedRows.Count > 0:
                        var templateList = new List<DataTemplate>();
                        foreach (DataGridViewRow row in DgvData.SelectedRows)
                        {
                            if (TryGetTemplate(row, out var template))
                                templateList.Insert(0, template);
                        }

                        var clipboardText = new StringBuilder();
                        if (MainViewer.CurrentEditor == MainViewer.EditorMessage)
                        {
	                        foreach (var template in templateList)
	                        {
		                        clipboardText.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\r\n",
			                        template.ID,
			                        template.ParentReader.FilePath,
			                        template.DateString,
			                        template.TraceName,
			                        template.ElapsedSecString,
			                        template.Description,
			                        template.Message.Trim());
                            }
                        }
                        else
                        {
	                        foreach (var template in templateList)
		                        clipboardText.Append($"{template.ParentReader.FilePath}\t{template.TraceMessage.Trim()}\r\n");
                        }

                        Clipboard.SetText(clipboardText.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        internal virtual void BtnSearch_Click(object sender, EventArgs e)
        {
	        if (TbxPattern.Text.IsNullOrEmpty())
                throw new Exception(Resources.Txt_LogsReaderForm_SearchPatternIsNull);
        }

        protected async Task UploadReaders(IEnumerable<TraceReader> readers)
		{
			try
			{
				await DgvReader.AssignCollectionAsync(readers.OrderBy(x => x.SchemeName).ThenBy(x => x.ID), null, true);
				RefreshAllRows(DgvReader, DgvReaderRefreshRow);
				DgvReader.ColumnHeadersVisible = true;
				//DgvReader.Sort(DgvReaderThreadIdColumn, ListSortDirection.Descending);
				OnUploadReaders?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				ReportStatus(ex.Message, ReportStatusType.Error);
			}
		}

		protected void ReportProcessStatus(IEnumerable<TraceReader> readers)
		{
			if (!IsWorking)
				return;

			var isChanged = false;

			this.SafeInvoke(() =>
			{
				if (!IsWorking)
					return;
				try
				{
					var countMatches = 0;
					var countErrorMatches = 0;
					var totalFiles = 0;
					var filesCompleted = 0;
					var progress = 0;

					lock (syncRootStatus)
					{
						ReportStatus(string.Format(Resources.Txt_LogsReaderForm_Working, $" ({TimeWatcher.Elapsed.ToReadableString()})"), ReportStatusType.Success);

						countMatches = readers.Sum(x => x.CountMatches);
						countErrorMatches = readers.Sum(x => x.CountErrors);
						totalFiles = readers.Count();
						filesCompleted = readers.Count(x => x.Status != TraceReaderStatus.Waiting && x.Status != TraceReaderStatus.Processing && x.Status != TraceReaderStatus.OnPause && !x.ThreadId.IsNullOrWhiteSpace());
						progress = 0;
						if (filesCompleted > 0 && TotalFiles > 0)
							progress = (filesCompleted * 100) / TotalFiles;

						if (CountMatches == countMatches && CountErrorMatches == countErrorMatches
						                                 && TotalFiles == totalFiles && FilesCompleted == filesCompleted && Progress == progress)
							return;
					}

					CountMatches = countMatches;
					CountErrorMatches = countErrorMatches;
					TotalFiles = totalFiles;
					FilesCompleted = filesCompleted;
					Progress = progress;
					isChanged = true;
				}
				finally
				{
					RefreshAllRows(DgvReader, DgvReaderRefreshRow);
				}
			});

			if (isChanged)
				OnProcessStatusChanged?.Invoke(this, EventArgs.Empty);
		}

		void ClearStatus()
        {
	        this.SafeInvoke(() =>
	        {
		        CountMatches = 0;
		        CountErrorMatches = 0;
		        Progress = 0;
		        FilesCompleted = 0;
		        TotalFiles = 0;
		        ReportStatus(string.Empty, ReportStatusType.Success);
            });

	        OnProcessStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private async void BtnExport_Click(object sender, EventArgs e)
        {
            if (IsWorking)
                return;

            var fileName = string.Empty;
            try
            { 
                string desctination;
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = @"CSV files (*.csv)|*.csv";
                    if (sfd.ShowDialog() != DialogResult.OK)
                        return;
                    desctination = sfd.FileName;
                }

                BtnSearch.Enabled = btnClear.Enabled = btnExport.Enabled = btnFilter.Enabled = ButtonHighlightEnabled = btnReset.Enabled = false;
                ReportStatus(Resources.Txt_LogsReaderForm_Exporting, ReportStatusType.Success);
                fileName = Path.GetFileName(desctination);

                var i = 0;
                Progress = 0;
                using (var writer = new StreamWriter(desctination, false, new UTF8Encoding(false)))
                {
	                if (MainViewer.CurrentEditor == MainViewer.EditorMessage)
	                {
		                await writer.WriteLineAsync(GetCSVRow(new[]
		                {
			                nameof(DataTemplate.Tmp.ID),
			                nameof(DataTemplate.Tmp.File),
			                nameof(DataTemplate.Tmp.Date),
			                nameof(DataTemplate.Tmp.TraceName),
			                DataTemplate.HeaderDescription,
			                DataTemplate.HeaderMessage
                        }));

                        foreach (DataGridViewRow row in DgvData.Rows)
		                {
			                if (!TryGetTemplate(row, out var template))
				                continue;

			                await writer.WriteLineAsync(
				                GetCSVRow(new[]
				                {
					                template.ID.ToString(),
					                template.ParentReader.FilePath,
					                template.DateString,
					                template.TraceName,
					                $"\"{template.Description}\"",
					                $"\"{template.Message.Trim()}\""
				                }));
			                writer.Flush();

			                Progress = (int) Math.Round((double) (100 * ++i) / DgvData.RowCount);
		                }
	                }
	                else
	                {
		                await writer.WriteLineAsync(GetCSVRow(new[]
		                {
			                nameof(DataTemplate.Tmp.File), 
			                DataTemplate.HeaderTraceMessage
		                }));

                        foreach (DataGridViewRow row in DgvData.Rows)
		                {
			                if (!TryGetTemplate(row, out var template))
				                continue;

			                await writer.WriteLineAsync(
				                GetCSVRow(new[]
				                {
					                template.ParentReader.FilePath,
					                $"\"{template.TraceMessage.Trim()}\""
				                }));
			                writer.Flush();

			                Progress = (int) Math.Round((double) (100 * ++i) / DgvData.RowCount);
		                }
	                }

	                writer.Close();
                }

                ReportStatus(string.Format(Resources.Txt_LogsReaderForm_SuccessExport, fileName), ReportStatusType.Success);
            }
            catch (Exception ex)
            {
                ReportStatus(string.Format(Resources.Txt_LogsReaderForm_ErrExport, fileName, ex.Message), ReportStatusType.Error);
            }
            finally
            {
                Progress = 100;
                btnClear.Enabled = true;
                ValidationCheck(false);
                DgvData.Focus();
            }
        }

        static string GetCSVRow(IReadOnlyCollection<string> @params)
        {
            var builder = new StringBuilder(@params.Count);
            foreach (var param in @params)
            {
                if (param.IsNullOrEmpty())
                {
                    builder.Append(";");
                }
                else if (param.StartsWith("\"") && param.EndsWith("\""))
                {
                    if (param.IndexOf("\"", 1, param.Length - 2, StringComparison.Ordinal) != -1)
                    {
                        var test = param.Substring(1, param.Length - 2).Replace("\"", "\"\"");
                        builder.Append($"\"{test}\";");
                    }
                    else
                    {
                        builder.Append($"{param};");
                    }
                }
                else if (param.Contains("\""))
                {
                    builder.Append($"\"{param.Replace("\"", "\"\"")}\";");
                }
                else
                {
                    builder.Append($"{param};");
                }
            }

            return builder.ToString();
        }

        private async void BtnFilter_Click(object sender, EventArgs e)
        {
            try
            {
	            await AssignResult(GetFilter(), false);
	            IsDgvDataFiltered = true;
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        protected DataFilter GetFilter()
        {
            return new DataFilter(DateStartFilter.Checked ? DateStartFilter.Value : DateTime.MinValue,
                DateEndFilter.Checked ? DateEndFilter.Value : DateTime.MaxValue,
                TbxTraceNameFilter.Text,
                CobxTraceNameFilter.Text.Like(Resources.Txt_LogsReaderForm_Contains),
                TbxTraceMessageFilter.Text,
                CobxTraceMessageFilter.Text.Like(Resources.Txt_LogsReaderForm_Contains));
        }

        private async void BtnReset_Click(object sender, EventArgs e)
        {
	        try
	        {
		        await AssignResult(null, false);
		        IsDgvDataFiltered = false;
			}
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private void buttonHighlight_Click(object sender, EventArgs e)
        {
	        if (DgvData.RowCount == 0)
		        return;

	        DataGridViewRow selected = null;
            if(DgvData.SelectedRows.Count > 0)
				selected = DgvData.SelectedRows[0];
            var selectedCellIndex = DgvData.CurrentCell?.ColumnIndex ?? -1;

            try
            {
	            var filter = GetFilter();
	            if (filter == null)
	            {
		            foreach (var row in DgvData.Rows.OfType<DataGridViewRow>())
			            row.Cells[DgvDataIsFilteredColumn.Name].Value = false;
		            return;
	            }


	            var count = 0;
	            foreach (var row in DgvData.Rows.OfType<DataGridViewRow>())
	            {
		            var isFiltered = TryGetTemplate(row, out var template) && filter.IsAllowed(template);
		            row.Cells[DgvDataIsFilteredColumn.Name].Value = isFiltered;
		            if (isFiltered)
			            count++;
	            }


	            if (count == 0)
		            ReportStatus(Resources.Txt_LogsReaderForm_NoFilterResultsFound, ReportStatusType.Warning);
	            else
		            ClearErrorStatus();

	            DgvData.Refresh();
			}
            catch (Exception ex)
            {
	            ReportStatus(ex.Message, ReportStatusType.Error);
            }
            finally
            {
	            SelectRow(selected, selectedCellIndex);
            }
        }

        private void buttonHighlightOff_Click(object sender, EventArgs e)
        {
	        if (DgvData.RowCount == 0)
		        return;

	        DataGridViewRow selected = null;
	        if (DgvData.SelectedRows.Count > 0)
		        selected = DgvData.SelectedRows[0];
	        var selectedCellIndex = DgvData.CurrentCell?.ColumnIndex ?? -1;

			try
	        {
		        ClearErrorStatus();

                foreach (var row in DgvData.Rows.OfType<DataGridViewRow>())
	                row.Cells[DgvDataIsFilteredColumn.Name].Value = false;

                DgvData.Refresh();
			}
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
            }
            finally
            {
	            SelectRow(selected, selectedCellIndex);
			}
        }

        protected async Task<bool> AssignResult(DataFilter filter, bool isNewData)
        {
	        try
	        {
		        // сохраняем предыдущую позицию в гриде
				var minIndex = -1;
		        var maxIndex = -1;
		        var countVisible = -1;
		        var firstVisibleIndex = -1;
		        if (DgvData.SelectedRows.Count > 0)
		        {
			        var selCol = DgvData.SelectedRows.OfType<DataGridViewRow>();
			        minIndex = selCol.First().Index;
			        maxIndex = selCol.Last().Index;
			        countVisible = DgvData.DisplayedRowCount(false);
			        firstVisibleIndex = DgvData.FirstDisplayedScrollingRowIndex;
		        }

		        _currentDGVResult = GetResultTemplates();

		        if (_currentDGVResult == null || !_currentDGVResult.Any())
		        {
			        ReportStatus(Resources.Txt_LogsReaderForm_NoLogsFound, ReportStatusType.Warning);
			        return false;
		        }

                if (filter != null)
		        {
			        _currentDGVResult = filter.FilterCollection(_currentDGVResult);

			        if (!_currentDGVResult.Any())
			        {
				        ReportStatus(Resources.Txt_LogsReaderForm_NoFilterResultsFound, ReportStatusType.Warning);
				        return false;
			        }
		        }

                var isSelected = await AssignCurrentDgvResult(isNewData);

                // возвращаем позицию в гриде
				if (DgvData.RowCount > 0 && (minIndex > -1 || maxIndex > -1) && !isSelected)
		        {
			        if (DgvData.RowCount > maxIndex && maxIndex > -1)
					{
						EnsureVisibleRow(DgvData, maxIndex, firstVisibleIndex, countVisible);
					}
					else if (DgvData.RowCount > minIndex && minIndex > -1)
					{
						EnsureVisibleRow(DgvData, minIndex, firstVisibleIndex, countVisible);
					}
					else
					{
						EnsureVisibleRow(DgvData, DgvData.RowCount - 1, firstVisibleIndex, countVisible);
					}
				}

		        return true;
            }
	        catch (Exception ex)
	        {
		        DgvData.Focus();
		        ReportStatus(ex.Message, ReportStatusType.Error);
                return false;
	        }
        }

        void SelectRow(DataGridViewRow row, int cellIndex = -1)
        {
	        if (row == null)
		        return;

	        DgvData.ClearSelection();
	        row.Selected = true;
	        if (cellIndex > -1 && row.Cells[cellIndex].Visible)
		        DgvData.CurrentCell = row.Cells[cellIndex];
	        else
		        DgvData.CurrentCell = row.Cells.OfType<DataGridViewCell>().FirstOrDefault(x => x.Visible);
        }

        static void EnsureVisibleRow(DataGridView view, int rowToShow, int firstVisible, int countVisible)
        {
	        if (rowToShow < 0 || rowToShow >= view.RowCount)
		        return;

	        view.Rows[rowToShow].Selected = true;
	        if (firstVisible <= -1 || countVisible <= -1)
	        {
		        view.FirstDisplayedScrollingRowIndex = rowToShow;
                return;
	        }

	        if (rowToShow < firstVisible)
	        {
		        view.FirstDisplayedScrollingRowIndex = rowToShow;
	        }
	        else if (rowToShow >= firstVisible + countVisible)
	        {
		        view.FirstDisplayedScrollingRowIndex = rowToShow - countVisible + 1;
	        }
	        else
	        {
		        view.FirstDisplayedScrollingRowIndex = firstVisible;
	        }
        }

        protected abstract IEnumerable<DataTemplate> GetResultTemplates();

        protected virtual void ChangeFormStatus()
        {
	        try
	        {
		        BtnSearch.Text = IsWorking ? Resources.Txt_LogsReaderForm_Stop : Resources.Txt_LogsReaderForm_Search;
		        btnClear.Enabled = !IsWorking;
		        TbxPattern.Enabled = !IsWorking;

		        foreach (var dgvChild in DgvData.Controls.OfType<Control>()) // решает баг с задисейбленным скролл баром DataGridView
			        dgvChild.Enabled = !IsWorking;
		        DgvData.Enabled = !IsWorking;

		        tabControlViewer.Enabled = !IsWorking;
		        ChbxUseRegex.Enabled = !IsWorking;
		        DateStartFilter.Enabled = !IsWorking;
		        DateEndFilter.Enabled = !IsWorking;
		        CobxTraceNameFilter.Enabled = !IsWorking;
		        TbxTraceNameFilter.Enabled = !IsWorking;
		        CobxTraceMessageFilter.Enabled = !IsWorking;
		        TbxTraceMessageFilter.Enabled = !IsWorking;
		        ChbxAlreadyUseFilter.Enabled = !IsWorking;

		        if (IsWorking)
		        {
			        ParentSplitContainer.Cursor = Cursors.WaitCursor;
			        ClearForm(true);
		        }
		        else
		        {
			        ParentSplitContainer.Cursor = Cursors.Default;
		        }

		        btnExport.Enabled = DgvData.RowCount > 0;
		        btnFilter.Enabled = ButtonHighlightEnabled = btnReset.Enabled = HasAnyResult;
            }
	        finally
	        {
		        if (IsWorking)
			        Focus();
		        else
			        DgvData.Focus();
            }
        }

        private void DataGridViewOnCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
	        if (e.RowIndex < 0 || e.ColumnIndex < 0) 
		        return;

	        try
	        {
		        var dgv = (DataGridView) sender;
		        var row = dgv.Rows[e.RowIndex];

		        if (dgv == DgvData)
			        DgvDataRefreshRow(row, null, null);
		        else if (dgv == DgvReader)
			        DgvReaderRefreshRow(row, null, null);
	        }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        }
        }

        /// <summary>
        /// Чтобы обновлялись не все строки, а только те что показаны. Т.к. для обновления свойтсва Image тратиться много времени
        /// </summary>
        protected void RefreshVisibleRows(CustomDataGridView dgv, Action<DataGridViewRow, int?, int?> refreshRow)
        {
	        try
	        {
		        if (dgv == null || dgv.RowCount == 0)
			        return;

		        var countVisible = dgv.DisplayedRowCount(false);
		        var firstVisibleRowIndex = dgv.FirstDisplayedScrollingRowIndex;

		        var firstIndex = Math.Max(0, firstVisibleRowIndex - 50);
		        var lastIndex = Math.Min(dgv.RowCount - 1, firstVisibleRowIndex + countVisible + 50);

		        for (var i = firstIndex; i <= lastIndex; i++)
			        refreshRow(dgv.Rows[i], countVisible, firstVisibleRowIndex);
	        }
	        catch (Exception ex)
	        {
				ReportStatus(ex.Message, ReportStatusType.Error);
			}
        }

        /// <summary>
        /// Вызывается после присвоения значений в DatagridView, чтобы отрисовать все строки
        /// </summary>
        protected static void RefreshAllRows(CustomDataGridView dgv, Action<DataGridViewRow, int?, int?> refreshRow)
        {
	        if (dgv == null || dgv.RowCount == 0)
		        return;

	        var countVisible = dgv.DisplayedRowCount(false);
	        var firstVisibleRowIndex = dgv.FirstDisplayedScrollingRowIndex;

            foreach (var row in dgv.Rows.OfType<DataGridViewRow>())
	            refreshRow(row, countVisible, firstVisibleRowIndex);
        }

        protected void DgvDataRefreshRow(DataGridViewRow row, int? countVisible, int? firstVisibleRowIndex)
        {
	        if (!TryGetTemplate(row, out var template))
		        return;

	        try
	        {
		        if (template.IsSuccess)
		        {
			        if (template.Date == null)
				        foreach (DataGridViewCell cell2 in row.Cells)
					        if (cell2.ToolTipText != Resources.Txt_LogsReaderForm_DateValueIsIncorrect)
						        cell2.ToolTipText = Resources.Txt_LogsReaderForm_DateValueIsIncorrect;

			        if (!Equals(row.DefaultCellStyle.Font, LogsReaderMainForm.DgvDataFont))
				        row.DefaultCellStyle.Font = LogsReaderMainForm.DgvDataFont;
                }
		        else
		        {
			        if (!template.IsMatched)
				        foreach (DataGridViewCell cell2 in row.Cells)
					        if (cell2.ToolTipText != Resources.Txt_LogsReaderForm_DoesntMatchByPattern)
						        cell2.ToolTipText = Resources.Txt_LogsReaderForm_DoesntMatchByPattern;

			        var cellTraceName = row.Cells[DgvDataTraceNameColumn.Name];
			        if (!Equals(cellTraceName.Style.Font, LogsReaderMainForm.ErrFont))
						cellTraceName.Style.Font = LogsReaderMainForm.ErrFont;
                }

		        if ((row.Cells[DgvDataFileColumn.Name] is DataGridViewCell cellFile) && cellFile.ToolTipText != template.File)
			        cellFile.ToolTipText = template.File;

		        // меняем свойтсво Image только для строк которые показаны пользователю
                // Т.к. для обновления свойтсва Image тратиться много времени
                if (countVisible == null && firstVisibleRowIndex == null
                    || countVisible != null && firstVisibleRowIndex != null && row.Index >= firstVisibleRowIndex - 5 && row.Index <= firstVisibleRowIndex + countVisible + 5)
                {
	                if (!(row.Cells[DgvDataPromptColumn.Name] is DgvTextAndImageCell imgCellPrompt))
		                return;

	                if (!(row.Cells[DgvDataTraceNameColumn.Name] is DgvTextAndImageCell imgCellTraceName))
		                return;

	                var isfiltered = false;
	                if (row.Cells[DgvDataIsFilteredColumn.Name] is DataGridViewCell isfilteredCell)
		                isfiltered = bool.Parse(isfilteredCell.Value.ToString());

	                if (!template.IsSuccess)
	                {
		                imgCellTraceName.Image = isfiltered ? Img_Failed_Filtered : Img_Failed;
	                }
	                else
	                {
		                imgCellTraceName.Image = isfiltered ? Img_Filtered : null;
	                }

	                imgCellPrompt.Image = template.IsSelected && DgvDataPromptColumn.Visible ? Img_Selected : null;
                }
	        }
	        catch (Exception ex)
	        {
				ReportStatus(ex.Message, ReportStatusType.Error);
			}
	        finally
	        {
		        if (template.IsSelected && (CurrentTransactionsMarkingType == TransactionsMarkingType.Color || CurrentTransactionsMarkingType == TransactionsMarkingType.Both))
		        {
			        if (row.DefaultCellStyle.BackColor != LogsReaderMainForm.TRN_COLOR_BACK)
				        row.DefaultCellStyle.BackColor = LogsReaderMainForm.TRN_COLOR_BACK;

			        if (row.DefaultCellStyle.ForeColor != LogsReaderMainForm.TRN_COLOR_FORE)
				        row.DefaultCellStyle.ForeColor = LogsReaderMainForm.TRN_COLOR_FORE;
		        }
		        else
		        {
			        ColorizationDGV(row, template);
		        }
	        }
        }

        protected virtual void ColorizationDGV(DataGridViewRow row, DataTemplate template)
        {
	        var color = row.Index.IsParity() ? Color.White : Color.FromArgb(245, 245, 245);
	        if (row.DefaultCellStyle.BackColor != color)
		        row.DefaultCellStyle.BackColor = color;
        }

        protected void DgvReaderRefreshRow(DataGridViewRow row, int? countVisible, int? firstVisibleRowIndex)
        {
	        if (!TryGetReader(row, out var reader))
		        return;

	        DataGridViewRow selected = null;
	        if (DgvReader.SelectedRows.Count > 0)
		        selected = DgvReader.SelectedRows[0];
	        if (selected == null)
		        selected = DgvReader.CurrentRow;

	        try
	        {
		        var cellPauseResume = row.Cells[DgvReaderProcessColumn.Name] as DgvDisableButtonCell;

		        void CanPause(bool isAlowed)
		        {
			        if (cellPauseResume == null)
				        return;

			        if (isAlowed)
			        {
				        if (!cellPauseResume.Enabled)
					        cellPauseResume.Enabled = true;
			        }
			        else
			        {
				        if (cellPauseResume.Enabled)
					        cellPauseResume.Enabled = false;
			        }

			        if (cellPauseResume.Value == null || cellPauseResume.Value.ToString() != Resources.TxtReader_BtnPause)
				        cellPauseResume.Value = Resources.TxtReader_BtnPause;
		        }

		        void AllowedResume()
		        {
			        if (cellPauseResume == null)
				        return;

			        if (!cellPauseResume.Enabled)
				        cellPauseResume.Enabled = true;
			        if (cellPauseResume.Value == null || cellPauseResume.Value.ToString() != Resources.TxtReader_BtnResume)
				        cellPauseResume.Value = Resources.TxtReader_BtnResume;
		        }

		        if (row.Cells[DgvReaderStatusColumn.Name] is DgvTextAndImageCell cellImage)
		        {

			        switch (reader.Status)
			        {
				        case TraceReaderStatus.Waiting:
					        cellImage.Image = Resources.waiting;
					        if (row.DefaultCellStyle.BackColor != Color.LightGray)
						        row.DefaultCellStyle.BackColor = Color.LightGray;
					        if (cellImage.Value == null || cellImage.Value.ToString() != Resources.TxtReader_StatusWaiting)
						        cellImage.Value = Resources.TxtReader_StatusWaiting;
					        CanPause(true);
					        break;
				        case TraceReaderStatus.Processing:
					        cellImage.Image = Resources.processing;
					        if (row.DefaultCellStyle.BackColor != Color.White)
						        row.DefaultCellStyle.BackColor = Color.White;
					        if (cellImage.Value == null || cellImage.Value.ToString() != Resources.TxtReader_StatusProcessing)
						        cellImage.Value = Resources.TxtReader_StatusProcessing;
					        CanPause(true);
					        break;
				        case TraceReaderStatus.OnPause:
					        cellImage.Image = Resources.onPause;
					        if (row.DefaultCellStyle.BackColor != LogsReaderMainForm.READER_COLOR_BACK_ONPAUSE)
						        row.DefaultCellStyle.BackColor = LogsReaderMainForm.READER_COLOR_BACK_ONPAUSE;
					        if (cellImage.Value == null || cellImage.Value.ToString() != Resources.TxtReader_StatusOnPause)
						        cellImage.Value = Resources.TxtReader_StatusOnPause;
					        AllowedResume();
					        break;
				        case TraceReaderStatus.Aborted:
					        cellImage.Image = Resources.aborted;
					        if (row.DefaultCellStyle.BackColor != LogsReaderMainForm.READER_COLOR_BACK_ERROR)
						        row.DefaultCellStyle.BackColor = LogsReaderMainForm.READER_COLOR_BACK_ERROR;
					        if (cellImage.Value == null || cellImage.Value.ToString() != Resources.TxtReader_StatusAborted)
						        cellImage.Value = Resources.TxtReader_StatusAborted;
					        CanPause(false);
					        break;
				        case TraceReaderStatus.Failed:
					        cellImage.Image = Resources.failed;
					        if (row.DefaultCellStyle.BackColor != LogsReaderMainForm.READER_COLOR_BACK_ERROR)
						        row.DefaultCellStyle.BackColor = LogsReaderMainForm.READER_COLOR_BACK_ERROR;
					        if (cellImage.Value == null || cellImage.Value.ToString() != Resources.TxtReader_StatusFailed)
						        cellImage.Value = Resources.TxtReader_StatusFailed;
					        CanPause(false);
					        break;
				        case TraceReaderStatus.Finished:
					        cellImage.Image = Resources.finished;
					        if (row.DefaultCellStyle.BackColor != LogsReaderMainForm.READER_COLOR_BACK_SUCCESS)
						        row.DefaultCellStyle.BackColor = LogsReaderMainForm.READER_COLOR_BACK_SUCCESS;
					        if (cellImage.Value == null || cellImage.Value.ToString() != Resources.TxtReader_StatusFinished)
						        cellImage.Value = Resources.TxtReader_StatusFinished;
					        CanPause(false);
					        break;
			        }
		        }

		        if (row.Cells[DgvReaderAbortColumn.Name] is DgvDisableButtonCell cellAbort)
		        {
			        if (cellAbort.Value == null || cellAbort.Value.ToString() != Resources.TxtReader_BtnAbort)
				        cellAbort.Value = Resources.TxtReader_BtnAbort;

			        if (cellAbort.Enabled &&
			            (reader.Status == TraceReaderStatus.Aborted || reader.Status == TraceReaderStatus.Failed || reader.Status == TraceReaderStatus.Finished))
				        cellAbort.Enabled = false;
			        else if (!cellAbort.Enabled && (reader.Status == TraceReaderStatus.Waiting || reader.Status == TraceReaderStatus.Processing ||
			                                        reader.Status == TraceReaderStatus.OnPause))
				        cellAbort.Enabled = true;
		        }

		        var cellThreadId = row.Cells[DgvReaderThreadIdColumn.Name];
		        if (cellThreadId.Value == null || cellThreadId.Value.ToString() != reader.ThreadId)
			        cellThreadId.Value = reader.ThreadId;

		        var cellMatches = row.Cells[DgvReaderCountMatchesColumn.Name];
				if(cellMatches.Value == null || cellMatches.Value.ToString() != reader.CountMatches.ToString())
					cellMatches.Value = reader.CountMatches;

				var cellErrors = row.Cells[DgvReaderCountErrorMatchesColumn.Name];
				if (cellErrors.Value == null || cellErrors.Value.ToString() != reader.CountErrors.ToString())
					cellErrors.Value = reader.CountErrors;
	        }
	        catch (Exception ex)
	        {
				ReportStatus(ex.Message, ReportStatusType.Error);
			}
	        finally
	        {
		        if (selected != null)
			        selected.Selected = true;
	        }
        }

        //DgvData.RowPostPaint += DgvData_RowPostPaint;
        //private void DgvData_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        //{
        // if (e.RowIndex < 0)
        //  return;

        // try
        // {
        //  var row = ((DataGridView)sender).Rows[e.RowIndex];
        //  if (!TryGetTemplate(row, out var template))
        //   return;
        //        //Color.Red

        //        //((DataGridView)sender).GridColor 
        //        using (Pen pen = new Pen(Color.Red))
        //        {
        //         int penWidth = 1;

        //         pen.Width = penWidth;

        //         int x = e.RowBounds.Left + (penWidth / 2);
        //         int y = e.RowBounds.Top + (penWidth / 2);
        //         int width = e.RowBounds.Width - penWidth;
        //         int height = e.RowBounds.Height - penWidth;

        //         e.Graphics.DrawRectangle(pen, x, y, width, height);
        //        }
        //    }
        // catch (Exception ex)
        // {
        //  ReportStatus(ex.Message, ReportStatusType.Error);
        // }
        //}

        //void SetBorderRowColor(Color color, DataGridViewCellPaintingEventArgs e)
        //{
        //    e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);
        //    using (var p = new Pen(color, 2))
        //    {
        //        var rect = e.CellBounds;
        //        rect.Y = rect.Top + 1;
        //        rect.Height -= 2;
        //        e.Graphics.DrawRectangle(p, rect);
        //    }

        //    e.Handled = true;
        //}

        private void buttonErrorPrev_Click(object sender, EventArgs e)
        {
	        SearchPrev(x=> !bool.Parse(x.Cells[DgvDataIsSuccessColumn.Name].Value?.ToString()));
        }

        private void buttonFilteredPrev_Click(object sender, EventArgs e)
        {
	        SearchPrev(x => bool.Parse(x.Cells[DgvDataIsFilteredColumn.Name].Value?.ToString()));
        }

        void SearchPrev(Func<DataGridViewRow, bool> condition)
        {
	        try
	        {
		        if (DgvData.SelectedRows.Count == 0 && DgvData.CurrentRow == null)
			        return;

		        var countVisible = DgvData.DisplayedRowCount(false);
		        var firstVisible = DgvData.FirstDisplayedScrollingRowIndex;

		        var selected = DgvData.SelectedRows.Count > 0 ? DgvData.SelectedRows[0] : DgvData.CurrentRow;
		        if (selected == null)
			        return;

                foreach (var row in DgvData.Rows.OfType<DataGridViewRow>()
			        .Where(x => x.Index < selected.Index)
			        .Where(condition)
			        .Reverse())
		        {
			        DgvData.FirstDisplayedScrollingRowIndex = row.Index >= firstVisible && row.Index < firstVisible + countVisible
				        ? firstVisible
				        : row.Index;

			        SelectRow(row, DgvData.CurrentCell?.ColumnIndex ?? -1);
			        return;
		        }
	        }
	        catch (Exception ex)
	        {
				ReportStatus(ex.Message, ReportStatusType.Error);
			}
        }

        private void buttonErrorNext_Click(object sender, EventArgs e)
        {
	        SearchNext(x => !bool.Parse(x.Cells[DgvDataIsSuccessColumn.Name].Value?.ToString()));
        }

        private void buttonFilteredNext_Click(object sender, EventArgs e)
        {
	        SearchNext(x => bool.Parse(x.Cells[DgvDataIsFilteredColumn.Name].Value?.ToString()));
        }

        void SearchNext(Func<DataGridViewRow, bool> condition)
        {
	        try
	        {
		        if (DgvData.SelectedRows.Count == 0 && DgvData.CurrentRow == null)
			        return;

		        var countVisible = DgvData.DisplayedRowCount(false);
		        var firstVisible = DgvData.FirstDisplayedScrollingRowIndex;

		        var selected = DgvData.SelectedRows.Count > 0 ? DgvData.SelectedRows[0] : DgvData.CurrentRow;
                if(selected == null)
                    return;

                foreach (var row in DgvData.Rows.OfType<DataGridViewRow>()
			        .Where(x => x.Index > selected.Index)
			        .Where(condition))
		        {
			        DgvData.FirstDisplayedScrollingRowIndex = row.Index >= firstVisible && row.Index < firstVisible + countVisible
				        ? firstVisible
				        : row.Index - countVisible >= 0 ? row.Index - countVisible : row.Index;

					SelectRow(row, DgvData.CurrentCell?.ColumnIndex ?? -1);
					return;
		        }
	        }
	        catch (Exception ex)
	        {
				ReportStatus(ex.Message, ReportStatusType.Error);
			}
        }

        private async void DgvDataOnColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
	        try
	        {
		        if (!HasAnyResult || _currentDGVResult == null)
			        return;

		        var oldSortedColumn = _oldSortedColumn; // поле очищается в методе ClearDGV
		        var newColumn = DgvData.Columns[e.ColumnIndex];

		        var columnName = DgvData.Columns[e.ColumnIndex].HeaderText;
		        if (columnName == DgvDataPromptColumn.HeaderText)
			        return;

                var byAscending = _oldSortedColumn?.Index != e.ColumnIndex || _oldSortedColumn?.Index == e.ColumnIndex && !_byAscending;
		        var source = _currentDGVResult;

		        var orderByOption = byAscending
			        ? new Dictionary<string, bool> {{columnName, false}}
			        : new Dictionary<string, bool> {{columnName, true}};

		        if (!orderByOption.ContainsKey(nameof(DataTemplate.Tmp.File)))
			        orderByOption.Add(nameof(DataTemplate.Tmp.File), !byAscending);
		        if (!orderByOption.ContainsKey(nameof(DataTemplate.Tmp.FoundLineID)))
			        orderByOption.Add(nameof(DataTemplate.Tmp.FoundLineID), !byAscending);

		        _currentDGVResult = DataTemplateCollection.DoOrdering(source, orderByOption);

		        await AssignCurrentDgvResult(false);

		        if (oldSortedColumn != null)
			        oldSortedColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
		        if (newColumn != null)
			        newColumn.HeaderCell.SortGlyphDirection = byAscending ? SortOrder.Ascending : SortOrder.Descending;

		        _oldSortedColumn = newColumn;
		        _byAscending = byAscending;
	        }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        }
        }

        async Task<bool> AssignCurrentDgvResult(bool isNewData)
		{
			var selectedSchemeName = string.Empty;
			var selectedPrivateID = -1;
			var selectedRangeToFirstVisible = -1;
			if (!isNewData && DgvData.SelectedRows.Count > 0 && int.TryParse(DgvData.SelectedRows[0].Cells[DgvDataPrivateIDColumn.Name].Value?.ToString(), out var privateID2))
			{
				selectedSchemeName = DgvData.SelectedRows[0].Cells[DgvDataSchemeNameColumn.Name].Value?.ToString();
				selectedPrivateID = privateID2;
				selectedRangeToFirstVisible = DgvData.SelectedRows[0].Index - DgvData.FirstDisplayedScrollingRowIndex;
			}

			DgvData.ClearSelection();
			DgvData.DataSource = null;
			DgvData.Rows.Clear();
			DgvData.Refresh();
			ClearErrorStatus();

			await DgvData.AssignCollectionAsync(_currentDGVResult, null);
	        DgvDataPromptColumn.Visible = CurrentTransactionsMarkingType == TransactionsMarkingType.Both || CurrentTransactionsMarkingType == TransactionsMarkingType.Prompt;
	        RefreshAllRows(DgvData, DgvDataRefreshRow);
	        btnExport.Enabled = DgvData.RowCount > 0;

	        if (selectedPrivateID > -1 && !selectedSchemeName.IsNullOrWhiteSpace())
	        {
				// возвращяем к предыдущей выбранной строке (происходит при фильтре и сортировке)
				// если новые данные (нового поиска) то не возвращает
		        foreach (var row in DgvData.Rows.OfType<DataGridViewRow>())
		        {
			        if (!Equals(row.Cells[DgvDataPrivateIDColumn.Name].Value, selectedPrivateID) ||
			            !Equals(row.Cells[DgvDataSchemeNameColumn.Name].Value, selectedSchemeName)) 
				        continue;

			        var firstVisible = row.Index - selectedRangeToFirstVisible;
			        if (firstVisible > -1 && DgvData.RowCount > firstVisible)
				        DgvData.FirstDisplayedScrollingRowIndex = firstVisible;
			        SelectRow(row);
			        return true;
		        }
	        }

	        return false;
		}

        private void DgvData_SelectionChanged(object sender, EventArgs e)
        {
	        try
	        {
		        if (DgvData.SelectedRows.Count == 0 || !TryGetTemplate(DgvData.SelectedRows[0], out var template))
			        return;

		        MainViewer.ChangeTemplate(template, checkBoxShowTrns.Checked, out var noChanged);

		        if (checkBoxShowTrns.Checked && !noChanged)
			        RefreshVisibleRows(DgvData, DgvDataRefreshRow);

		        if (MainViewer.Parent is TabPage page)
			        tabControlViewer.SelectTab(page);
	        }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        }
	        finally
	        {
		        DgvData.Focus();
	        }
        }

        private void DgvData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
	        try
	        {
		        if (DgvData.SelectedRows.Count == 0 || !TryGetTemplate(DgvData.SelectedRows[0], out var template))
			        return;

                AddViewer(new TraceItemView(DefaultEncoding, UserSettings, false), template);
	        }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        }
	        finally
	        {
		        DgvData.Focus();
	        }
        }

        internal abstract bool TryGetTemplate(DataGridViewRow row, out DataTemplate template);

        internal abstract bool TryGetReader(DataGridViewRow row, out TraceReader reader);

        private void DgvData_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button != MouseButtons.Right)
                    return;
                var hti = DgvData.HitTest(e.X, e.Y);
                if (hti.RowIndex == -1)
                    return;
                DgvData.ClearSelection();
                DgvData.Rows[hti.RowIndex].Selected = true;
            }
            catch (Exception ex)
            {
				ReportStatus(ex.Message, ReportStatusType.Error);
			}
        }

        internal virtual void TxtPatternOnTextChanged(object sender, EventArgs e)
        {
            UserSettings.PreviousSearch = ((TextBox)sender).Text;
            ValidationCheck(true);
        }

        internal virtual void ChbxUseRegex_CheckedChanged(object sender, EventArgs e)
        {
            UserSettings.UseRegex = ((CheckBox)sender).Checked;
        }

        internal virtual void DateStartFilterOnValueChanged(object sender, EventArgs e)
        {
	        if (UserSettings != null)
		        UserSettings.DateStartChecked = DateStartFilter.Checked;

	        if (_oldDateStartChecked || !DateStartFilter.Checked)
		        return;

	        _oldDateStartChecked = true;
	        DateStartFilter.Value = _getStartDate.Invoke();
        }

        internal virtual void DateEndFilterOnValueChanged(object sender, EventArgs e)
        {
	        if (UserSettings != null)
		        UserSettings.DateEndChecked = DateEndFilter.Checked;

	        if (_oldDateEndChecked || !DateEndFilter.Checked)
		        return;

	        _oldDateEndChecked = true;
	        DateEndFilter.Value = _getEndDate.Invoke();
        }

        private void checkBoxShowTrns_CheckedChanged(object sender, EventArgs e)
        {
	        try
	        {
		        if (tabControlViewer.TabCount > 0)
			        foreach (var page in tabControlViewer.TabPages.OfType<CustomTabPage>().ToList())
				        page.View.RefreshDescription(checkBoxShowTrns.Checked, out var _);

		        if (checkBoxShowTrns.Checked)
		        {
			        _currentTransactionsMarkingType = _prevTransactionsMarkingType == TransactionsMarkingType.None ? DefaultTransactionsMarkingType : _prevTransactionsMarkingType;
		        }
		        else
		        {
			        _prevTransactionsMarkingType = _currentTransactionsMarkingType;
			        _currentTransactionsMarkingType = TransactionsMarkingType.None;
		        }

		        DgvDataPromptColumn.Visible = _currentTransactionsMarkingType == TransactionsMarkingType.Both || _currentTransactionsMarkingType == TransactionsMarkingType.Prompt;

		        CheckBoxTransactionsMarkingTypeChanged(_currentTransactionsMarkingType);

		        RefreshAllRows(DgvData, DgvDataRefreshRow);
		        DgvData.Focus();
	        }
	        catch (Exception ex)
	        {
				ReportStatus(ex.Message, ReportStatusType.Error);
			}
        }

        protected abstract void CheckBoxTransactionsMarkingTypeChanged(TransactionsMarkingType newType);

        private void ComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        internal virtual void CobxTraceNameFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
	        UserSettings.TraceNameFilterContains = ((ComboBox)sender).Text.Like(Resources.Txt_LogsReaderForm_Contains);
        }

        internal virtual void TbxTraceNameFilterOnTextChanged(object sender, EventArgs e)
        {
            UserSettings.TraceNameFilter = ((TextBox)sender).Text;
        }

        internal virtual void CobxTraceMessageFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
	        UserSettings.TraceMessageFilterContains = ((ComboBox)sender).Text.Like(Resources.Txt_LogsReaderForm_Contains);
        }

        internal virtual void TbxTraceMessageFilterOnTextChanged(object sender, EventArgs e)
        {
            UserSettings.TraceMessageFilter = ((TextBox)sender).Text;
        }

        internal virtual void ChbxAlreadyUseFilter_CheckedChanged(object sender, EventArgs e)
        {

        }

        protected virtual void ValidationCheck(bool clearStatus)
        {
	        btnExport.Enabled = DgvData.RowCount > 0;
	        btnFilter.Enabled = ButtonHighlightEnabled = btnReset.Enabled = HasAnyResult;
        }

        protected virtual void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm(true);
        }

        protected virtual void ClearForm(bool saveData)
        {
	        try
	        {
		        if (saveData)
			        SaveData();

		        Clear();

		        ClearStatus();

		        CountMatches = 0;
		        CountErrorMatches = 0;
				FilesCompleted = 0;
		        TotalFiles = 0;

		        IsDgvDataFiltered = false;
	        }
	        catch (Exception ex)
	        {
				ReportStatus(ex.Message, ReportStatusType.Error);
			}
	        finally
	        {
		        STREAM.GarbageCollect();
			}
        }

        internal void SelectTransactions()
        {
	        if (checkBoxShowTrns.Checked)
		        MainViewer.SelectTransactions();

	        RefreshVisibleRows(DgvData, DgvDataRefreshRow);
	        DgvData.Focus();
        }

        internal void DeselectTransactions()
        {
	        foreach (var page in tabControlViewer.TabPages.OfType<CustomTabPage>().ToList())
		        page.View.DeselectTransactions();

	        RefreshVisibleRows(DgvData, DgvDataRefreshRow);
	        DgvData.Focus();
        }

        protected void Clear()
        {
	        try
	        {
		        _oldSortedColumn = null;
		        _currentDGVResult = null;
		        _byAscending = true;

		        DgvData.DataSource = null;
		        DgvData.Rows.Clear();
		        DgvData.Refresh();

				MainViewer.Clear();
		        foreach (var page in tabControlViewer.TabPages.OfType<CustomTabPage>().ToList())
		        {
			        if(page == MainViewer.Parent)
                        continue;

			        page.View.Clear();
			        tabControlViewer.TabPages.Remove(page);
		        }

                btnExport.Enabled = false;
		        btnFilter.Enabled = ButtonHighlightEnabled = btnReset.Enabled = HasAnyResult;
	        }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        }
        }

        private bool _isLastWasError;

        protected void ReportStatus(string message, ReportStatusType type)
        {
	        this.SafeInvoke(() =>
	        {
		        if (!message.IsNullOrEmpty())
		        {
			        _statusInfo.BackColor = type == ReportStatusType.Error ? Color.Red : type == ReportStatusType.Warning ? Color.Yellow : Color.Green;
			        _statusInfo.ForeColor = type == ReportStatusType.Warning ? Color.Black : Color.White;
			        _statusInfo.Text = message.Replace("\r", "").Replace("\n", " ");
		        }
		        else
		        {
			        _statusInfo.BackColor = SystemColors.Control;
			        _statusInfo.ForeColor = Color.Black;
			        _statusInfo.Text = string.Empty;
		        }
		        _isLastWasError = type == ReportStatusType.Error || type == ReportStatusType.Warning;
			});
        }

        protected void ClearErrorStatus()
        {
            if (!_isLastWasError)
                return;
            _statusInfo.BackColor = SystemColors.Control;
            _statusInfo.ForeColor = Color.Black;
            _statusInfo.Text = string.Empty;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
	        UserSettings?.Dispose();

	        if (disposing)
		        components?.Dispose();
	        base.Dispose(disposing);
        }

        private int prevReadersDistance = -1;
        private bool shownFilterPanel;
        private void buttonNextBlock_Click(object sender, EventArgs e)
        {
	        try
	        {
		        shownFilterPanel = !shownFilterPanel;

		        splitContainerTop.Panel1Collapsed = !shownFilterPanel;
		        splitContainerTop.Panel2Collapsed = shownFilterPanel;

		        if (shownFilterPanel)
		        {
			        if (sender != null)
				        prevReadersDistance = splitContainerMainFilter.SplitterDistance;
			        splitContainerMainFilter.SplitterDistance = 65;
			        buttonNextBlock.Text = @">";
		        }
		        else
		        {
			        buttonNextBlock.Text = @"<";
			        RefreshAllRows(DgvReader, DgvReaderRefreshRow); // надо обновить при первой загрузке, иначе не прорисовываются
			        splitContainerMainFilter.SplitterDistance = prevReadersDistance > 0 ? prevReadersDistance : splitContainerMainFilter.Height / 3;
				}
			}
	        catch (Exception ex)
	        {
				ReportStatus(ex.Message, ReportStatusType.Error);
			}
        }

        protected virtual void CustomPanel_Resize(object sender, EventArgs e)
        {

        }
	}
}