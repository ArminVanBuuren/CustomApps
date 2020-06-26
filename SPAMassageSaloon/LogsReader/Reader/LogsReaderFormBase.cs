using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using LogsReader.Config;
using LogsReader.Properties;
using SPAMassageSaloon.Common;
using Utils;
using Utils.WinForm;
using Utils.WinForm.DataGridViewHelper;
using Utils.WinForm.Notepad;

namespace LogsReader.Reader
{
	public abstract partial class LogsReaderFormBase : UserControl, IUserForm
    {
	    private readonly Func<DateTime> _getStartDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        private readonly Func<DateTime> _getEndDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

        private bool _oldDateStartChecked;
        private bool _oldDateEndChecked;
        private bool _settingsLoaded;
        private bool _isWorking;

        private int _countMatches = 0;
        private int _filesCompleted = 0;
        private int _totalFiles = 0;

        private readonly ToolStripStatusLabel _statusInfo;
        private readonly ToolStripStatusLabel _findedInfo;
        private readonly ToolStripStatusLabel _completedFilesStatus;
        private readonly ToolStripStatusLabel _totalFilesStatus;
        private readonly ToolStripStatusLabel _filtersCompleted1;
        private readonly ToolStripStatusLabel _filtersCompleted2;
        private readonly ToolStripStatusLabel _overallFound1;
        private readonly ToolStripStatusLabel _overallFound2;

        protected ToolTip Tooltip { get; }

        protected Editor EditorMessage { get; }

        protected Editor EditorTraceMessage { get; }

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

        /// <summary>
        /// Юзерские настройки 
        /// </summary>
        public UserSettings UserSettings { get; }

        public int CountMatches
        {
	        get => _countMatches;
	        protected set
	        {
		        _countMatches = value;
		        _findedInfo.Text = _countMatches.ToString();
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
	        protected set
	        {
		        _filesCompleted = value;
		        _completedFilesStatus.Text = _filesCompleted.ToString();
	        }
        }

        public int TotalFiles
        {
	        get => _totalFiles;
	        protected set
	        {
		        _totalFiles = value;
		        _totalFilesStatus.Text = _totalFiles.ToString();
	        }
        }

        public abstract bool HasAnyResult { get; }

        protected LogsReaderFormBase(Encoding defaultEncoding, UserSettings userSettings)
        {
	        InitializeComponent();

	        UserSettings = userSettings;

            #region Initialize StripStatus

            Tooltip = new ToolTip { InitialDelay = 50 };

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

            _overallFound1 = new ToolStripStatusLabel { Font = base.Font, Margin = statusStripItemsPaddingStart };
            _findedInfo = new ToolStripStatusLabel("0") { Font = base.Font, Margin = statusStripItemsPaddingMiddle };
            _overallFound2 = new ToolStripStatusLabel { Font = base.Font, Margin = statusStripItemsPaddingEnd };
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(_overallFound1);
            statusStrip.Items.Add(_findedInfo);
            statusStrip.Items.Add(_overallFound2);

            statusStrip.Items.Add(new ToolStripSeparator());
            _statusInfo = new ToolStripStatusLabel("") { Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Margin = statusStripItemsPaddingStart };
            statusStrip.Items.Add(_statusInfo);

            #endregion

            try
            {
	            DgvData.AutoGenerateColumns = false;
	            DgvData.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
	            DgvData.CellFormatting += DgvDataOnCellFormatting;
	            DgvData.ColumnHeaderMouseClick += DgvDataOnColumnHeaderMouseClick;

                #region Initialize Controls

                EditorMessage = notepad.AddDocument(new BlankDocument {HeaderName = "Message", Language = Language.XML});
                EditorMessage.BackBrush = null;
                EditorMessage.BorderStyle = BorderStyle.FixedSingle;
                EditorMessage.Cursor = Cursors.IBeam;
                EditorMessage.DelayedEventsInterval = 1000;
                EditorMessage.DisabledColor = Color.FromArgb(100, 171, 171, 171);
                EditorMessage.IsReplaceMode = false;
                EditorMessage.SelectionColor = Color.FromArgb(50, 0, 0, 255);
                EditorMessage.LanguageChanged += Message_LanguageChanged;

                EditorTraceMessage = notepad.AddDocument(new BlankDocument {HeaderName = "Trace"});
                EditorTraceMessage.BackBrush = null;
                EditorTraceMessage.BorderStyle = BorderStyle.FixedSingle;
                EditorTraceMessage.Cursor = Cursors.IBeam;
                EditorTraceMessage.DelayedEventsInterval = 1000;
                EditorTraceMessage.DisabledColor = Color.FromArgb(100, 171, 171, 171);
                EditorTraceMessage.IsReplaceMode = false;
                EditorTraceMessage.SelectionColor = Color.FromArgb(50, 0, 0, 255);
                EditorTraceMessage.LanguageChanged += TraceMessage_LanguageChanged;

                notepad.SelectEditor(0);
                notepad.DefaultEncoding = defaultEncoding;
                notepad.WordWrapStateChanged += Notepad_WordWrapStateChanged;
                notepad.WordHighlightsStateChanged += Notepad_WordHighlightsStateChanged;

                DateStartFilter.ValueChanged += DateStartFilterOnValueChanged;
                DateEndFilter.ValueChanged += DateEndFilterOnValueChanged;

                #endregion

                #region Apply All Settings

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

                var langMessage = UserSettings.MessageLanguage;
                var langTrace = UserSettings.TraceLanguage;
                if (EditorMessage.Language != langMessage)
                    EditorMessage.ChangeLanguage(langMessage);
                if (EditorTraceMessage.Language != langTrace)
                    EditorTraceMessage.ChangeLanguage(langTrace);

                EditorMessage.WordWrap = UserSettings.MessageWordWrap;
                EditorMessage.Highlights = UserSettings.MessageHighlights;
                EditorTraceMessage.WordWrap = UserSettings.TraceWordWrap;
                EditorTraceMessage.Highlights = UserSettings.TraceHighlights;

                #endregion
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, Resources.Txt_Initialization);
            }
        }

        public void ApplyFormSettings()
        {
            try
            {
                if(UserSettings == null)
                    return;

                for (var i = 0; i < DgvData.Columns.Count; i++)
                {
                    var valueStr = UserSettings.GetValue("COL" + i);
                    if (!valueStr.IsNullOrEmptyTrim() && int.TryParse(valueStr, out var value) && value > 1 && value <= 1000)
                        DgvData.Columns[i].Width = value;
                }

                ParentSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(ParentSplitContainer), 25, 1000, ParentSplitContainer.SplitterDistance);
                MainSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(MainSplitContainer), 25, 1000, MainSplitContainer.SplitterDistance);
                EnumSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(EnumSplitContainer), 25, 1000, EnumSplitContainer.SplitterDistance);

                ParentSplitContainer.SplitterMoved += (sender, args) => { SaveData(); };
                MainSplitContainer.SplitterMoved += (sender, args) => { SaveData(); };
                EnumSplitContainer.SplitterMoved += (sender, args) => { SaveData(); };
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        public virtual void ApplySettings()
        {
            try
            {
                #region Change Language

                _filtersCompleted1.Text = Resources.Txt_LogsReaderForm_FilesCompleted_1;
                _filtersCompleted2.Text = Resources.Txt_LogsReaderForm_FilesCompleted_2;
                _overallFound1.Text = Resources.Txt_LogsReaderForm_OverallFound_1;
                _overallFound2.Text = Resources.Txt_LogsReaderForm_OverallFound_2;

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

                Tooltip.RemoveAll();
                Tooltip.SetToolTip(TbxPattern, Resources.Txt_Form_SearchComment);
                Tooltip.SetToolTip(ChbxUseRegex, Resources.Txt_LRSettings_UseRegexComment);
                Tooltip.SetToolTip(DateStartFilter, Resources.Txt_Form_DateFilterComment);
                Tooltip.SetToolTip(DateEndFilter, Resources.Txt_Form_DateFilterComment);
                Tooltip.SetToolTip(TbxTraceNameFilter, Resources.Txt_Form_TraceNameFilterComment);
                Tooltip.SetToolTip(TbxTraceMessageFilter, Resources.Txt_Form_TraceFilterComment);
                Tooltip.SetToolTip(ChbxAlreadyUseFilter, Resources.Txt_Form_AlreadyUseFilterComment);
                Tooltip.SetToolTip(btnExport, Resources.Txt_LogsReaderForm_ExportComment);

                ChbxUseRegex.Text = Resources.Txt_LogsReaderForm_UseRegex;
                BtnSearch.Text = IsWorking ? Resources.Txt_LogsReaderForm_Stop : Resources.Txt_LogsReaderForm_Search;
                btnClear.Text = Resources.Txt_LogsReaderForm_Clear;
                btnClear.Size = new Size(Convert.ToInt32(Resources.LogsReaderForm_btnClear_Width), btnClear.Height);
                btnFilter.Text = Resources.Txt_LogsReaderForm_Filter;
                btnFilter.Padding = new Padding(3, 0, Convert.ToInt32(Resources.LogsReaderForm_buttonFilter_rightPadding), 0);
                btnReset.Text = Resources.Txt_LogsReaderForm_Reset;
                btnReset.Padding = new Padding(2, 0, Convert.ToInt32(Resources.LogsReaderForm_buttonReset_rightPadding), 0);
                btnExport.Text = Resources.Txt_LogsReaderForm_Export;
                btnExport.Size = new Size(Convert.ToInt32(Resources.LogsReaderForm_buttonExport_Width), btnExport.Height);
                ChbxAlreadyUseFilter.Text = Resources.Txt_LogsReaderForm_UseFilterWhenSearching;
                ChbxAlreadyUseFilter.Padding = new Padding(0, 0, Convert.ToInt32(Resources.LogsReaderForm_alreadyUseFilter_rightPadding), 0);

                #endregion

                ReportStatus(string.Empty, ReportStatusType.Success);
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

        public virtual void SaveData()
        {
            try
            {
                if (!_settingsLoaded || UserSettings == null)
                    return;

                for (var i = 0; i < DgvData.Columns.Count; i++)
                {
                    UserSettings.SetValue("COL" + i, DgvData.Columns[i].Width);
                }

                UserSettings.SetValue(nameof(ParentSplitContainer), ParentSplitContainer.SplitterDistance);
                UserSettings.SetValue(nameof(MainSplitContainer), MainSplitContainer.SplitterDistance);
                UserSettings.SetValue(nameof(EnumSplitContainer), EnumSplitContainer.SplitterDistance);
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

                        var clipboardText = new StringBuilder(templateList.Sum(x => x.TraceMessage.Length) + (templateList.Count * 10) + 250);
                        foreach (var template in templateList)
                        {
                            clipboardText.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\r\n",
                                template.ID,
                                template.ParentReader.FilePath,
                                template.DateOfTrace,
                                template.TraceName,
                                template.Description,
                                notepad.SelectedIndex <= 0 ? template.Message.Trim() : template.TraceMessage.Trim());
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

        protected void ReportProcessStatus(int countMatches, int percentOfProgeress, int filesCompleted, int totalFiles)
        {
	        this.SafeInvoke(() =>
	        {
		        CountMatches = countMatches;
		        Progress = percentOfProgeress;
		        FilesCompleted = filesCompleted;
		        TotalFiles = totalFiles;
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

                BtnSearch.Enabled = btnClear.Enabled = btnExport.Enabled = btnFilter.Enabled = btnReset.Enabled = false;
                ReportStatus(Resources.Txt_LogsReaderForm_Exporting, ReportStatusType.Success);
                fileName = Path.GetFileName(desctination);

                var i = 0;
                Progress = 0;
                using (var writer = new StreamWriter(desctination, false, new UTF8Encoding(false)))
                {
                    await writer.WriteLineAsync(GetCSVRow(new[] {"ID", "File", "Date", "Trace name", "Description", notepad.SelectedIndex <= 0 ? "Message" : "Trace Message" }));
                    foreach (DataGridViewRow row in DgvData.Rows)
                    {
                        if(!TryGetTemplate(row, out var template))
                            continue;

                        await writer.WriteLineAsync(GetCSVRow(new[]
                        {
                            template.ID.ToString(),
                            template.ParentReader.FilePath,
                            template.DateOfTrace,
                            template.TraceName,
                            $"\"{template.Description}\"",
                            notepad.SelectedIndex <= 0 ? $"\"{template.Message.Trim()}\"" : $"\"{template.TraceMessage.Trim()}\""
                        }));
                        writer.Flush();

                        Progress = (int)Math.Round((double)(100 * ++i) / DgvData.RowCount);
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
                await AssignResult(GetFilter());
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
            await AssignResult(null);
        }

        private IEnumerable<DataTemplate> _currentDGVResult;

        protected async Task<bool> AssignResult(DataFilter filter)
        {
	        try
	        {
		        // сохраняем предыдущую позицию в гриде
		        var minIndex = -1;
		        var maxIndex = -1;
		        var countVisible = -1;
		        var firstVisible = -1;
		        if (DgvData.SelectedRows.Count > 0)
		        {
			        var selCol = DgvData.SelectedRows.OfType<DataGridViewRow>();
			        minIndex = selCol.First().Index;
			        maxIndex = selCol.Last().Index;
			        countVisible = DgvData.DisplayedRowCount(false);
			        firstVisible = DgvData.FirstDisplayedScrollingRowIndex;
		        }

		        ClearDGV();
		        ClearErrorStatus();

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

		        await DgvData.AssignCollectionAsync(_currentDGVResult, null);

		        btnExport.Enabled = DgvData.RowCount > 0;

                // возвращаем позицию в гриде
		        if (DgvData.RowCount > 0 && (minIndex > -1 || maxIndex > -1))
		        {
			        DgvData.ClearSelection();
			        if (DgvData.RowCount > maxIndex && maxIndex > -1)
			        {
				        EnsureVisibleRow(DgvData, maxIndex, firstVisible, countVisible);
			        }
			        else if (DgvData.RowCount > minIndex && minIndex > -1)
			        {
				        EnsureVisibleRow(DgvData, minIndex, firstVisible, countVisible);
			        }
			        else
			        {
				        EnsureVisibleRow(DgvData, DgvData.RowCount - 1, firstVisible, countVisible);
			        }
		        }

		        return true;
            }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
                return false;
	        }
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

		        notepad.Enabled = !IsWorking;
		        descriptionText.Enabled = !IsWorking;
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
		        btnFilter.Enabled = btnReset.Enabled = HasAnyResult;
            }
	        finally
	        {
		        if (IsWorking)
			        Focus();
		        else
			        DgvData.Focus();
            }
        }

        protected void DgvDataOnCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                var row = ((DataGridView) sender).Rows[e.RowIndex];
                if(!TryGetTemplate(row, out var template))
                    return;
                row.Cells["FileNamePartial"].ToolTipText = template.File;
                СolorizationDGV(row, template);
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private int _prevSortedColumn = -1;
        private bool _byDescending = true;

        private async void DgvDataOnColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
	        try
	        {
		        if(!HasAnyResult || _currentDGVResult == null)
                    return;

		        var byDescending = _prevSortedColumn != e.ColumnIndex || _prevSortedColumn == e.ColumnIndex && !_byDescending;
		        var source = _currentDGVResult;

                ClearDGV();
                ClearErrorStatus();

                DgvData.Columns[e.ColumnIndex].SortMode = DataGridViewColumnSortMode.Programmatic;

                var columnName = DgvData.Columns[e.ColumnIndex].HeaderText;
                var orderByOption = byDescending
	                ? new Dictionary<string, bool>() {{columnName, false}}
	                : new Dictionary<string, bool>() {{columnName, true}};
                if(!orderByOption.ContainsKey("File"))
	                orderByOption.Add("File", !byDescending);
                if (!orderByOption.ContainsKey("FoundLineID"))
                    orderByOption.Add("FoundLineID", !byDescending);

                _currentDGVResult = DataTemplateCollection.DoOrdering(source, orderByOption);

                await DgvData.AssignCollectionAsync(_currentDGVResult, null);

                _prevSortedColumn = e.ColumnIndex;
                _byDescending = byDescending;

	        }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        }
        }

        protected virtual void СolorizationDGV(DataGridViewRow row, DataTemplate template)
        {
	        if (template.IsMatched)
	        {
		        if (template.Date == null)
		        {
			        row.DefaultCellStyle.BackColor = Color.Yellow;
			        foreach (DataGridViewCell cell2 in row.Cells)
				        cell2.ToolTipText = Resources.Txt_LogsReaderForm_DateValueIsIncorrect;
			        return;
		        }

		        row.DefaultCellStyle.BackColor = Color.White;
	        }
	        else
	        {
		        row.DefaultCellStyle.BackColor = Color.LightPink;
		        foreach (DataGridViewCell cell2 in row.Cells)
			        cell2.ToolTipText = Resources.Txt_LogsReaderForm_DoesntMatchByPattern;
	        }
        }

        private void DgvData_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                EditorMessage.Text = string.Empty;
                EditorTraceMessage.Text = string.Empty;

                if (DgvData.CurrentRow == null || DgvData.SelectedRows.Count == 0)
                    return;

                if(!TryGetTemplate(DgvData.SelectedRows[0], out var template))
                    return;

                var foundByTransactionValue = string.Empty;
                if (!template.TransactionValue.IsNullOrEmptyTrim())
	                foundByTransactionValue = $" | Found by Trn = \"{template.TransactionValue}\"";

                descriptionText.Text = $"FoundLineID = {template.FoundLineID}{foundByTransactionValue}\r\n{template.Description}";

                string messageString;
                if (EditorMessage.Language == Language.XML || EditorMessage.Language == Language.HTML)
                {
	                var messageXML = XML.RemoveUnallowable(template.Message, " ");
	                messageString = messageXML.IsXml(out var xmlDoc) ? xmlDoc.PrintXml() : messageXML.TrimWhiteSpaces();
                }
                else
                {
	                messageString = template.Message.TrimWhiteSpaces();
                }

                EditorMessage.Text = messageString;
                EditorMessage.DelayedEventsInterval = 10;

                EditorTraceMessage.Text = template.TraceMessage;
                EditorTraceMessage.DelayedEventsInterval = 10;
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
            catch (Exception)
            {
                // ignored
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

        private void Message_LanguageChanged(object sender, EventArgs e)
        {
            var prev = UserSettings.MessageLanguage;
            UserSettings.MessageLanguage = EditorMessage.Language;
            if((prev == Language.HTML || prev == Language.XML) && (EditorMessage.Language == Language.XML || EditorMessage.Language == Language.HTML))
                return;
            DgvData_SelectionChanged(DgvData, EventArgs.Empty);
        }

        private void TraceMessage_LanguageChanged(object sender, EventArgs e)
        {
            UserSettings.TraceLanguage = EditorTraceMessage.Language;
        }

        private void Notepad_WordWrapStateChanged(object sender, EventArgs e)
        {
            if (!(sender is Editor editor))
                return;

            if (editor == EditorMessage)
                UserSettings.MessageWordWrap = editor.WordWrap;
            else if (editor == EditorTraceMessage)
                UserSettings.TraceWordWrap = editor.WordWrap;
        }

        private void Notepad_WordHighlightsStateChanged(object sender, EventArgs e)
        {
            if (!(sender is Editor editor))
                return;

            if (editor == EditorMessage)
                UserSettings.MessageHighlights = editor.Highlights;
            else if (editor == EditorTraceMessage)
                UserSettings.TraceHighlights = editor.Highlights;
        }

        protected virtual void ValidationCheck(bool clearStatus)
        {
	        btnExport.Enabled = DgvData.RowCount > 0;
	        btnFilter.Enabled = btnReset.Enabled = HasAnyResult;
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

                ClearDGV();

                ReportProcessStatus(0, 0, 0, 0);

                _completedFilesStatus.Text = @"0";
                _totalFilesStatus.Text = @"0";
                _findedInfo.Text = @"0";

                ReportStatus(string.Empty, ReportStatusType.Success);

                STREAM.GarbageCollect();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        protected void ClearDGV()
        {
            try
            {
	            _prevSortedColumn = -1;
	            _currentDGVResult = null;
	            _byDescending = true;

                DgvData.DataSource = null;
                DgvData.Rows.Clear();
                DgvData.Refresh();

                descriptionText.Text = string.Empty;
                if (EditorMessage != null)
                    EditorMessage.Text = string.Empty;
                if (EditorTraceMessage != null)
                    EditorTraceMessage.Text = string.Empty;

                btnExport.Enabled = false;
                btnFilter.Enabled = btnReset.Enabled = HasAnyResult;
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private bool _isLastWasError;

        protected void ReportStatus(string message, ReportStatusType type)
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
    }
}