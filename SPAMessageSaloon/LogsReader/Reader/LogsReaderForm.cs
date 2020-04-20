using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using LogsReader.Config;
using LogsReader.Properties;
using SPAMessageSaloon.Common;
using Utils;
using Utils.WinForm;
using Utils.WinForm.DataGridViewHelper;
using Utils.WinForm.Notepad;

namespace LogsReader.Reader
{
    public sealed partial class LogsReaderForm : UserControl, IUserForm
    {
        private readonly Func<DateTime> _getStartDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        private readonly Func<DateTime> _getEndDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

        private bool _oldDateStartChecked = false;
        private bool _oldDateEndChecked = false;
        private bool _settingsLoaded = false;

        readonly TreeNode treeNode3 = new TreeNode("Servers");
        readonly TreeNode treeNode4 = new TreeNode("Types");
        private readonly ToolStripStatusLabel _statusInfo;
        private readonly ToolStripStatusLabel _findedInfo;
        private readonly ToolStripStatusLabel _completedFilesStatus;
        private readonly ToolStripStatusLabel _totalFilesStatus;
        private readonly ToolStripStatusLabel _filtersCompleted1;
        private readonly ToolStripStatusLabel _filtersCompleted2;
        private readonly ToolStripStatusLabel _overallFound1;
        private readonly ToolStripStatusLabel _overallFound2;
        private readonly ToolTip _tooltip;
        private readonly Editor _message;
        private readonly Editor _traceMessage;


        /// <summary>
        /// Сохранить изменения в конфиг
        /// </summary>
        public event EventHandler OnSchemeChanged;

        /// <summary>
        /// Статус выполнения поиска
        /// </summary>
        public bool IsWorking { get; private set; } = false;

        /// <summary>
        /// Юзерские настройки 
        /// </summary>
        public UserSettings UserSettings { get; private set; }

        /// <summary>
        /// Текущая схема настроек
        /// </summary>
        public LRSettingsScheme CurrentSettings { get; private set; }

        public DataTemplateCollection OverallResultList { get; private set; }

        public LogsReaderPerformer MainReader { get; private set; }

        public int Progress
        {
            get => IsWorking ? progressBar.Value : 100;
            private set => progressBar.Value = value;
        }

        public LogsReaderForm(LRSettingsScheme scheme)
        {
            InitializeComponent();

            try
            {
                CurrentSettings = scheme;
                CurrentSettings.ReportStatus += ReportStatus;
                UserSettings = new UserSettings(CurrentSettings.Name);

                treeNode3.Name = "trvServers";
                treeNode4.Name = "trvTypes";
                trvMain.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {treeNode3, treeNode4});

                dgvFiles.AutoGenerateColumns = false;
                dgvFiles.CellFormatting += DgvFiles_CellFormatting;
                orderByText.GotFocus += OrderByText_GotFocus;

                #region Set StatusStrip

                var statusStripItemsPaddingStart = new Padding(0, 2, 0, 2);
                var statusStripItemsPaddingMiddle = new Padding(-3, 2, 0, 2);
                var statusStripItemsPaddingEnd = new Padding(-3, 2, 1, 2);

                _filtersCompleted1 = new ToolStripStatusLabel() {Font = this.Font, Margin = statusStripItemsPaddingStart};
                _completedFilesStatus = new ToolStripStatusLabel("0") {Font = this.Font, Margin = statusStripItemsPaddingMiddle};
                _filtersCompleted2 = new ToolStripStatusLabel() {Font = this.Font, Margin = statusStripItemsPaddingMiddle};
                _totalFilesStatus = new ToolStripStatusLabel("0") {Font = this.Font, Margin = statusStripItemsPaddingEnd};
                statusStrip.Items.Add(_filtersCompleted1);
                statusStrip.Items.Add(_completedFilesStatus);
                statusStrip.Items.Add(_filtersCompleted2);
                statusStrip.Items.Add(_totalFilesStatus);

                _overallFound1 = new ToolStripStatusLabel() {Font = this.Font, Margin = statusStripItemsPaddingStart};
                _findedInfo = new ToolStripStatusLabel("0") {Font = this.Font, Margin = statusStripItemsPaddingMiddle};
                _overallFound2 = new ToolStripStatusLabel() {Font = this.Font, Margin = statusStripItemsPaddingEnd};
                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(_overallFound1);
                statusStrip.Items.Add(_findedInfo);
                statusStrip.Items.Add(_overallFound2);

                statusStrip.Items.Add(new ToolStripSeparator());
                _statusInfo = new ToolStripStatusLabel("") {Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Margin = statusStripItemsPaddingStart};
                statusStrip.Items.Add(_statusInfo);

                #endregion

                _tooltip = new ToolTip {InitialDelay = 50};


                _message = notepad.AddDocument(new BlankDocument() {HeaderName = "Message", Language = Language.XML});
                _message.BackBrush = null;
                _message.BorderStyle = BorderStyle.FixedSingle;
                _message.Cursor = Cursors.IBeam;
                _message.DelayedEventsInterval = 1000;
                _message.DisabledColor = Color.FromArgb(100, 171, 171, 171);
                _message.IsReplaceMode = false;
                _message.SelectionColor = Color.FromArgb(50, 0, 0, 255);
                _message.LanguageChanged += Message_LanguageChanged;

                _traceMessage = notepad.AddDocument(new BlankDocument() {HeaderName = "Trace"});
                _traceMessage.BackBrush = null;
                _traceMessage.BorderStyle = BorderStyle.FixedSingle;
                _traceMessage.Cursor = Cursors.IBeam;
                _traceMessage.DelayedEventsInterval = 1000;
                _traceMessage.DisabledColor = Color.FromArgb(100, 171, 171, 171);
                _traceMessage.IsReplaceMode = false;
                _traceMessage.SelectionColor = Color.FromArgb(50, 0, 0, 255);
                _traceMessage.LanguageChanged += TraceMessage_LanguageChanged;

                notepad.SelectEditor(0);
                notepad.WordWrapStateChanged += Notepad_WordWrapStateChanged;
                notepad.WordHighlightsStateChanged += Notepad_WordHighlightsStateChanged;

                dateStartFilter.ValueChanged += (sender, args) =>
                {
                    if (UserSettings != null)
                        UserSettings.DateStartChecked = dateStartFilter.Checked;

                    if (_oldDateStartChecked || !dateStartFilter.Checked)
                        return;
                    _oldDateStartChecked = true;
                    dateStartFilter.Value = _getStartDate.Invoke();
                };
                dateEndFilter.ValueChanged += (sender, args) =>
                {
                    if (UserSettings != null)
                        UserSettings.DateEndChecked = dateEndFilter.Checked;

                    if (_oldDateEndChecked || !dateEndFilter.Checked)
                        return;
                    _oldDateEndChecked = true;
                    dateEndFilter.Value = _getEndDate.Invoke();
                };
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, Resources.Txt_Initialization);
            }
            finally
            {
                ClearForm(false);
                ValidationCheck();
            }
        }

        public void ApplyFormSettings()
        {
            try
            {
                for (var i = 0; i < dgvFiles.Columns.Count; i++)
                {
                    var valueStr = UserSettings.GetValue("COL" + i);
                    if (!valueStr.IsNullOrEmptyTrim() && int.TryParse(valueStr, out var value) && value > 1 && value <= 1000)
                        dgvFiles.Columns[i].Width = value;
                }

                ParentSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(ParentSplitContainer), 25, 1000, ParentSplitContainer.SplitterDistance);
                MainSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(MainSplitContainer), 25, 1000, MainSplitContainer.SplitterDistance);
                EnumSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(EnumSplitContainer), 25, 1000, EnumSplitContainer.SplitterDistance);
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
            finally
            {
                trvMain.Nodes["trvServers"].Checked = false;
                trvMain.Nodes["trvTypes"].Checked = false;
                CheckTreeViewNode(trvMain.Nodes["trvServers"], false);
                CheckTreeViewNode(trvMain.Nodes["trvTypes"], false);
            }
        }

        public void ApplySettings()
        {
            try
            {
                treeNode3.Text = Resources.Txt_LogsReaderForm_Servers2;
                treeNode4.Text = Resources.Txt_LogsReaderForm_Types;

                _filtersCompleted1.Text = Resources.Txt_LogsReaderForm_FilesCompleted_1;
                _filtersCompleted2.Text = Resources.Txt_LogsReaderForm_FilesCompleted_2;
                _overallFound1.Text = Resources.Txt_LogsReaderForm_OverallFound_1;
                _overallFound2.Text = Resources.Txt_LogsReaderForm_OverallFound_2;

                traceNameFilterComboBox.Items.Clear();
                traceNameFilterComboBox.Items.Add(Resources.Txt_LogsReaderForm_Contains);
                traceNameFilterComboBox.Items.Add(Resources.Txt_LogsReaderForm_NotContains);
                traceNameFilterComboBox.AssignValue(UserSettings.TraceNameFilterContains ? Resources.Txt_LogsReaderForm_Contains : Resources.Txt_LogsReaderForm_NotContains, traceNameFilterComboBox_SelectedIndexChanged);

                traceMessageFilterComboBox.Items.Clear();
                traceMessageFilterComboBox.Items.Add(Resources.Txt_LogsReaderForm_Contains);
                traceMessageFilterComboBox.Items.Add(Resources.Txt_LogsReaderForm_NotContains);
                traceMessageFilterComboBox.AssignValue(UserSettings.TraceMessageFilterContains ? Resources.Txt_LogsReaderForm_Contains : Resources.Txt_LogsReaderForm_NotContains, traceMessageFilterComboBox_SelectedIndexChanged);

                _tooltip.RemoveAll();
                _tooltip.SetToolTip(txtPattern, Resources.Txt_Form_SearchComment);
                _tooltip.SetToolTip(useRegex, Resources.Txt_LRSettings_UseRegexComment);
                _tooltip.SetToolTip(serversText, Resources.Txt_LRSettingsScheme_Servers);
                _tooltip.SetToolTip(fileTypes, Resources.Txt_LRSettingsScheme_Types);
                _tooltip.SetToolTip(maxThreadsText, Resources.Txt_LRSettingsScheme_MaxThreads);
                _tooltip.SetToolTip(logFolderText, Resources.Txt_LRSettingsScheme_LogsDirectory);
                _tooltip.SetToolTip(maxLinesStackText, Resources.Txt_LRSettingsScheme_MaxTraceLines);
                _tooltip.SetToolTip(dateStartFilter, Resources.Txt_Form_DateFilterComment);
                _tooltip.SetToolTip(dateEndFilter, Resources.Txt_Form_DateFilterComment);
                _tooltip.SetToolTip(traceNameFilter, Resources.Txt_Form_TraceNameFilterComment);
                _tooltip.SetToolTip(traceMessageFilter, Resources.Txt_Form_TraceFilterComment);
                _tooltip.SetToolTip(alreadyUseFilter, Resources.Txt_Form_AlreadyUseFilterComment);
                _tooltip.SetToolTip(rowsLimitText, Resources.Txt_LRSettingsScheme_RowsLimit);
                _tooltip.SetToolTip(orderByText, Resources.Txt_LRSettingsScheme_OrderBy);
                _tooltip.SetToolTip(trvMain, Resources.Txt_Form_trvMainComment);

                
                label12.Text = Resources.Txt_LogsReaderForm_OrderBy;
                label2.Text = Resources.Txt_LogsReaderForm_RowsLimit;
                label1.Text = Resources.Txt_LogsReaderForm_Servers;
                label6.Text = Resources.Txt_LogsReaderForm_MaxLines;
                label3.Text = Resources.Txt_LogsReaderForm_FilteTypes;
                label5.Text = Resources.Txt_LogsReaderForm_LogsFolder;
                useRegex.Text = Resources.Txt_LogsReaderForm_UseRegex;

                btnSearch.Text = Resources.Txt_LogsReaderForm_Search;
                btnClear.Text = Resources.Txt_LogsReaderForm_Clear;
                btnClear.Size = new Size(Convert.ToInt32(Resources.LogsReaderForm_btnClear_Width), btnClear.Height);
                buttonFilter.Text = Resources.Txt_LogsReaderForm_Filter;
                buttonFilter.Padding = new Padding(3,0, Convert.ToInt32(Resources.LogsReaderForm_buttonFilter_rightPadding), 0);
                buttonReset.Text = Resources.Txt_LogsReaderForm_Reset;
                buttonReset.Padding = new Padding(2, 0, Convert.ToInt32(Resources.LogsReaderForm_buttonReset_rightPadding), 0);
                buttonExport.Text = Resources.Txt_LogsReaderForm_Export;
                buttonExport.Size = new Size(Convert.ToInt32(Resources.LogsReaderForm_buttonExport_Width), buttonExport.Height);
                alreadyUseFilter.Text = Resources.Txt_LogsReaderForm_UseFilterWhenSearching;
                alreadyUseFilter.Padding = new Padding(0,0, Convert.ToInt32(Resources.LogsReaderForm_alreadyUseFilter_rightPadding), 0);

                txtPattern.AssignValue(UserSettings.PreviousSearch, TxtPattern_TextChanged);
                useRegex.Checked = UserSettings.UseRegex;
                dateStartFilter.Checked = UserSettings.DateStartChecked;
                if (dateStartFilter.Checked)
                    dateStartFilter.Value = _getStartDate.Invoke();
                dateEndFilter.Checked = UserSettings.DateEndChecked;
                if (dateEndFilter.Checked)
                    dateEndFilter.Value = _getEndDate.Invoke();
                traceNameFilter.AssignValue(UserSettings.TraceNameFilter, TraceNameFilter_TextChanged);
                traceMessageFilter.AssignValue(UserSettings.TraceMessageFilter, TraceMessageFilter_TextChanged);

                var langMessage = UserSettings.MessageLanguage;
                var langTrace = UserSettings.TraceLanguage;
                if (_message.Language != langMessage)
                    _message.ChangeLanguage(langMessage);
                if (_traceMessage.Language != langTrace)
                    _traceMessage.ChangeLanguage(langTrace);

                _message.WordWrap = UserSettings.MessageWordWrap;
                _message.Highlights = UserSettings.MessageHighlights;
                _traceMessage.WordWrap = UserSettings.TraceWordWrap;
                _traceMessage.Highlights = UserSettings.TraceHighlights;

                serversText.Text = CurrentSettings.Servers;
                notepad.DefaultEncoding = CurrentSettings.Encoding;
                logFolderText.AssignValue(CurrentSettings.LogsFolder, LogDirText_TextChanged);
                fileTypes.Text = CurrentSettings.FileTypes;
                maxLinesStackText.AssignValue(CurrentSettings.MaxLines, MaxLinesStackText_TextChanged);
                maxThreadsText.AssignValue(CurrentSettings.MaxThreads, MaxThreadsText_TextChanged);
                rowsLimitText.AssignValue(CurrentSettings.RowsLimit, RowsLimitText_TextChanged);
                orderByText.Text = CurrentSettings.OrderBy;
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
            finally
            {
                ReportStatus(string.Empty, ReportStatusType.Success);
                ValidationCheck();
                _settingsLoaded = true;
            }
        }

        public void SaveData()
        {
            try
            {
                if (!_settingsLoaded)
                    return;

                for (var i = 0; i < dgvFiles.Columns.Count; i++)
                {
                    UserSettings.SetValue("COL" + i, dgvFiles.Columns[i].Width);
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

        public void LogsReaderKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.F5 when btnSearch.Enabled && !IsWorking:
                        BtnSearch_Click(this, EventArgs.Empty);
                        break;
                    case Keys.Escape when btnSearch.Enabled && IsWorking:
                        BtnSearch_Click(this, EventArgs.Empty);
                        break;
                    case Keys.F6 when btnClear.Enabled:
                        ClearForm();
                        break;
                    case Keys.F7 when buttonFilter.Enabled:
                        buttonFilter_Click(this, EventArgs.Empty);
                        break;
                    case Keys.F8 when buttonReset.Enabled:
                        buttonReset_Click(this, EventArgs.Empty);
                        break;
                    case Keys.S when e.Control && buttonExport.Enabled:
                        ButtonExport_Click(this, EventArgs.Empty);
                        break;
                }
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            if (!IsWorking)
            {
                var stop = new Stopwatch();
                try
                {
                    var filter = alreadyUseFilter.Checked ? GetFilter() : null;

                    MainReader = new LogsReaderPerformer(CurrentSettings, 
                        trvMain.Nodes["trvServers"].Nodes.Cast<TreeNode>().Where(x => x.Checked).Select(x => x.Text),
                        trvMain.Nodes["trvTypes"].Nodes.Cast<TreeNode>().Where(x => x.Checked).Select(x => x.Text),
                        txtPattern.Text, 
                        useRegex.Checked,
                        filter);
                    MainReader.OnProcessReport += ReportProcessStatus;

                    stop.Start();
                    IsWorking = true;
                    ChangeFormStatus();
                    ReportStatus(Resources.Txt_LogsReaderForm_Working, ReportStatusType.Success);

                    await MainReader.StartAsync();

                    OverallResultList = new DataTemplateCollection(CurrentSettings, MainReader.ResultsOfSuccess);
                    OverallResultList.AddRange(MainReader.ResultsOfError.OrderBy(x => x.Date));

                    if (await AssignResult(filter))
                    {
                        ReportStatus(string.Format(Resources.Txt_LogsReaderForm_FinishedIn, stop.Elapsed.ToReadableString()), ReportStatusType.Success);
                    }

                    stop.Stop();
                }
                catch (Exception ex)
                {
                    ReportStatus(ex.Message, ReportStatusType.Error);
                }
                finally
                {
                    if (MainReader != null)
                    {
                        MainReader.OnProcessReport -= ReportProcessStatus;
                        MainReader.Dispose();
                        MainReader = null;
                    }

                    IsWorking = false;
                    ChangeFormStatus();
                    if (stop.IsRunning)
                        stop.Stop();
                }
            }
            else
            {
                MainReader?.Stop();
                ReportStatus(Resources.Txt_LogsReaderForm_Stopping, ReportStatusType.Success);
            }
        }

        void ReportProcessStatus(int countMatches, int percentOfProgeress, int filesCompleted, int totalFiles)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    _findedInfo.Text = countMatches.ToString();
                    Progress = percentOfProgeress;
                    _completedFilesStatus.Text = filesCompleted.ToString();
                    _totalFilesStatus.Text = totalFiles.ToString();
                }));
            }
            else
            {
                _findedInfo.Text = countMatches.ToString();
                Progress = percentOfProgeress;
                _completedFilesStatus.Text = filesCompleted.ToString();
                _totalFilesStatus.Text = totalFiles.ToString();
            }
        }

        private async void ButtonExport_Click(object sender, EventArgs e)
        {
            if (IsWorking)
                return;

            string fileName = string.Empty;
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

                btnSearch.Enabled = btnClear.Enabled = buttonExport.Enabled = buttonFilter.Enabled = buttonReset.Enabled = false;
                ReportStatus(Resources.Txt_LogsReaderForm_Exporting, ReportStatusType.Success);
                fileName = Path.GetFileName(desctination);

                int i = 0;
                Progress = 0;
                using (var writer = new StreamWriter(desctination, false, new UTF8Encoding(false)))
                {
                    await writer.WriteLineAsync(GetCSVRow(new[] {"ID", "Server", "File", "Trace name", "Date", "Description", "Message"}));
                    foreach (DataGridViewRow row in dgvFiles.Rows)
                    {
                        var privateID = (int) (row.Cells["PrivateID"]?.Value ?? -1);
                        if(privateID <= -1)
                            continue;
                        var template = OverallResultList[privateID];
                        if(template == null)
                            continue;

                        await writer.WriteLineAsync(GetCSVRow(new[] {template.ID.ToString(), template.Server, template.File, template.TraceName, template.DateOfTrace, template.Description, $"\"{template.Message.Trim()}\""}));
                        writer.Flush();

                        Progress = (int)Math.Round((double)(100 * ++i) / dgvFiles.RowCount);
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
                dgvFiles.Focus();
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
                else if (param.StartsWith("\"") && param.EndsWith("\"") && param.IndexOf("\"", 1, param.Length - 2, StringComparison.Ordinal) != -1)
                {
                    var test = param.Substring(1, param.Length - 2).Replace("\"", "\"\"");
                    builder.Append($"\"{test}\";");
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

        private async void buttonFilter_Click(object sender, EventArgs e)
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

        DataFilter GetFilter()
        {
            return new DataFilter(dateStartFilter.Checked ? dateStartFilter.Value : DateTime.MinValue,
                dateEndFilter.Checked ? dateEndFilter.Value : DateTime.MaxValue,
                traceNameFilter.Text,
                traceNameFilterComboBox.Text.Like(Resources.Txt_LogsReaderForm_Contains),
                traceMessageFilter.Text,
                traceMessageFilterComboBox.Text.Like(Resources.Txt_LogsReaderForm_Contains));
        }

        private async void buttonReset_Click(object sender, EventArgs e)
        {
            await AssignResult(null);
        }

        async Task<bool> AssignResult(DataFilter filter)
        {
            ClearDGV();
            ClearErrorStatus();

            if (OverallResultList == null)
                return false;

            IEnumerable<DataTemplate> result = new List<DataTemplate>(OverallResultList);

            if (!result.Any())
            {
                ReportStatus(Resources.Txt_LogsReaderForm_NoLogsFound, ReportStatusType.Warning);
                return false;
            }

            if (filter != null)
            {
                result = filter.FilterCollection(result);

                if (!result.Any())
                {
                    ReportStatus(Resources.Txt_LogsReaderForm_NoFilterResultsFound, ReportStatusType.Warning);
                    return false;
                }
            }

            await dgvFiles.AssignCollectionAsync(result, null);

            buttonExport.Enabled = dgvFiles.RowCount > 0;
            return true;
        }

        void ChangeFormStatus()
        {
            btnSearch.Text = IsWorking ? Resources.Txt_LogsReaderForm_Stop : Resources.Txt_LogsReaderForm_Search;
            btnClear.Enabled = !IsWorking;
            trvMain.Enabled = !IsWorking;
            txtPattern.Enabled = !IsWorking;

            foreach (var dgvChild in dgvFiles.Controls.OfType<Control>()) // решает баг с задисейбленным скролл баром DataGridView
                dgvChild.Enabled = !IsWorking;
            dgvFiles.Enabled = !IsWorking;

            notepad.Enabled = !IsWorking;
            descriptionText.Enabled = !IsWorking;
            useRegex.Enabled = !IsWorking;
            serversText.Enabled = !IsWorking;
            fileTypes.Enabled = !IsWorking;
            maxThreadsText.Enabled = !IsWorking;
            rowsLimitText.Enabled = !IsWorking;
            logFolderText.Enabled = !IsWorking;
            maxLinesStackText.Enabled = !IsWorking;
            dateStartFilter.Enabled = !IsWorking;
            dateEndFilter.Enabled = !IsWorking;
            traceNameFilterComboBox.Enabled = !IsWorking;
            traceNameFilter.Enabled = !IsWorking;
            traceMessageFilterComboBox.Enabled = !IsWorking;
            traceMessageFilter.Enabled = !IsWorking;
            alreadyUseFilter.Enabled = !IsWorking;
            orderByText.Enabled = !IsWorking;
            buttonExport.Enabled = dgvFiles.RowCount > 0;
            buttonFilter.Enabled = buttonReset.Enabled = OverallResultList != null && OverallResultList.Count > 0;

            if (IsWorking)
            {
                ParentSplitContainer.Cursor = Cursors.WaitCursor;
                ClearForm();
                this.Focus();
            }
            else
            {
                //foreach (var pb in ParentSplitContainer.Controls.OfType<Control>())
                //    pb.Enabled = true;

                ParentSplitContainer.Cursor = Cursors.Default;
                dgvFiles.Focus();
            }
        }

        private void DgvFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                var row = ((DataGridView) sender).Rows[e.RowIndex];
                var privateID = (int)(row.Cells["PrivateID"]?.Value ?? -1);
                if (privateID <= -1 || OverallResultList == null)
                    return;

                var template = OverallResultList[privateID];
                if (template == null)
                    return;

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
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private void DgvFiles_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                _message.Text = string.Empty;
                _traceMessage.Text = string.Empty;

                if (dgvFiles.CurrentRow == null || dgvFiles.SelectedRows.Count == 0 || OverallResultList == null)
                    return;

                var privateID = (int) (dgvFiles.SelectedRows[0].Cells["PrivateID"]?.Value ?? -1);
                if (privateID <= -1)
                    return;

                var template = OverallResultList?[privateID];
                if (template == null)
                    return;

                descriptionText.Text = template.Description;

                if (_message.Language == Language.XML || _message.Language == Language.HTML)
                {
                    var messageXML = XML.RemoveUnallowable(template.Message, " ");
                    _message.Text = messageXML.IsXml(out var xmlDoc) ? xmlDoc.PrintXml() : messageXML.TrimWhiteSpaces();
                }
                else
                {
                    _message.Text = template.Message.TrimWhiteSpaces();
                }
                _message.DelayedEventsInterval = 10;

                _traceMessage.Text = template.TraceMessage;
                _traceMessage.DelayedEventsInterval = 10;
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
            finally
            {
                dgvFiles.Focus();
            }
        }

        private void DgvFiles_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hti = dgvFiles.HitTest(e.X, e.Y);
                dgvFiles.ClearSelection();
                dgvFiles.Rows[hti.RowIndex].Selected = true;
            }
        }
    
        private void ServersText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.Servers = serversText.Text;
            serversText.AssignValue(CurrentSettings.Servers, ServersText_TextChanged);

            //заполняем список серверов из параметра
            trvMain.Nodes["trvServers"].Nodes.Clear();
            foreach (var s in CurrentSettings.Servers.Split(',').GroupBy(p => p.TrimWhiteSpaces(), StringComparer.InvariantCultureIgnoreCase).OrderBy(p => p.Key))
            {
                if (s.Key.IsNullOrEmptyTrim())
                    continue;
                trvMain.Nodes["trvServers"].Nodes.Add(s.Key.Trim().ToUpper());
            }

            trvMain.ExpandAll();

            ValidationCheck();
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void LogDirText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.LogsFolder = logFolderText.Text;
            logFolderText.AssignValue(CurrentSettings.LogsFolder, LogDirText_TextChanged);
            ValidationCheck();
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TypesText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.FileTypes = fileTypes.Text;
            fileTypes.AssignValue(CurrentSettings.FileTypes, TypesText_TextChanged);

            //заполняем список типов из параметра
            trvMain.Nodes["trvTypes"].Nodes.Clear();
            foreach (var s in CurrentSettings.FileTypes.Split(',').GroupBy(p => p.TrimWhiteSpaces(), StringComparer.InvariantCultureIgnoreCase).OrderBy(p => p.Key))
            {
                if (s.Key.IsNullOrEmptyTrim())
                    continue;
                trvMain.Nodes["trvTypes"].Nodes.Add(s.Key.Trim().ToUpper());
            }

            trvMain.ExpandAll();

            ValidationCheck();
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TrvMain_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 0)
                CheckTreeViewNode(e.Node, e.Node.Checked);
            ValidationCheck();
        }

        private void MaxLinesStackText_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(maxLinesStackText.Text, out var value))
                MaxLinesStackTextSave(value);
        }

        private void MaxLinesStackText_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(maxLinesStackText.Text, out var value))
                value = -1;
            MaxLinesStackTextSave(value);
        }

        void MaxLinesStackTextSave(int value)
        {
            CurrentSettings.MaxLines = value;
            maxLinesStackText.AssignValue(CurrentSettings.MaxLines, MaxLinesStackText_TextChanged);
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MaxThreadsText_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(maxThreadsText.Text, out var value))
                MaxThreadsTextSave(value);
        }

        private void MaxThreadsText_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(maxThreadsText.Text, out var value))
                value = -1;
            MaxThreadsTextSave(value);
        }

        void MaxThreadsTextSave(int value)
        {
            CurrentSettings.MaxThreads = value;
            maxThreadsText.AssignValue(CurrentSettings.MaxThreads, MaxThreadsText_TextChanged);
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RowsLimitText_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(rowsLimitText.Text, out var value))
                RowsLimitTextSave(value);
        }

        private void RowsLimitText_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(rowsLimitText.Text, out var value))
                value = -1;
            RowsLimitTextSave(value);
        }

        void RowsLimitTextSave(int value)
        {
            CurrentSettings.RowsLimit = value;
            rowsLimitText.AssignValue(CurrentSettings.RowsLimit, RowsLimitText_TextChanged);
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OrderByText_Leave(object sender, EventArgs e)
        {
            CurrentSettings.OrderBy = orderByText.Text;
            ValidationCheck(CurrentSettings.OrderBy.Equals(orderByText.Text, StringComparison.InvariantCultureIgnoreCase));
            orderByText.Text = CurrentSettings.OrderBy;
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OrderByText_GotFocus(object sender, EventArgs e)
        {
            LRSettingsScheme.CheckOrderByItem(orderByText.Text);
            ValidationCheck(CurrentSettings.OrderBy.Equals(orderByText.Text, StringComparison.InvariantCultureIgnoreCase));
        }

        private static void CheckTreeViewNode(TreeNode node, bool isChecked)
        {
            foreach (TreeNode item in node.Nodes)
            {
                item.Checked = isChecked;
                if (item.Nodes.Count > 0)
                    CheckTreeViewNode(item, isChecked);
            }
        }

        private void TxtPattern_TextChanged(object sender, EventArgs e)
        {
            UserSettings.PreviousSearch = txtPattern.Text;
            ValidationCheck();
        }

        private void UseRegex_CheckedChanged(object sender, EventArgs e)
        {
            UserSettings.UseRegex = useRegex.Checked;
        }

        private void ComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void TraceNameFilter_TextChanged(object sender, EventArgs e)
        {
            UserSettings.TraceNameFilter = traceNameFilter.Text;
        }

        private void traceNameFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UserSettings.TraceNameFilterContains = traceNameFilterComboBox.Text.Like(Resources.Txt_LogsReaderForm_Contains);
        }

        private void TraceMessageFilter_TextChanged(object sender, EventArgs e)
        {
            UserSettings.TraceMessageFilter = traceMessageFilter.Text;
        }

        private void traceMessageFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UserSettings.TraceMessageFilterContains = traceMessageFilterComboBox.Text.Like(Resources.Txt_LogsReaderForm_Contains);
        }

        private void Message_LanguageChanged(object sender, EventArgs e)
        {
            var prev = UserSettings.MessageLanguage;
            UserSettings.MessageLanguage = _message.Language;
            if((prev == Language.HTML || prev == Language.XML) && (_message.Language == Language.XML || _message.Language == Language.HTML))
                return;
            DgvFiles_SelectionChanged(this, EventArgs.Empty);
        }

        private void TraceMessage_LanguageChanged(object sender, EventArgs e)
        {
            UserSettings.TraceLanguage = _traceMessage.Language;
        }

        private void Notepad_WordWrapStateChanged(object sender, EventArgs e)
        {
            if (!(sender is Editor editor))
                return;

            if (editor == _message)
                UserSettings.MessageWordWrap = editor.WordWrap;
            else if (editor == _traceMessage)
                UserSettings.TraceWordWrap = editor.WordWrap;
        }

        private void Notepad_WordHighlightsStateChanged(object sender, EventArgs e)
        {
            if (!(sender is Editor editor))
                return;

            if (editor == _message)
                UserSettings.MessageHighlights = editor.Highlights;
            else if (editor == _traceMessage)
                UserSettings.TraceHighlights = editor.Highlights;
        }

        void ValidationCheck(bool clearStatus = true)
        {
            var isCorrectPath = IO.CHECK_PATH.IsMatch(logFolderText.Text);
            logFolderText.BackColor = isCorrectPath ? SystemColors.Window : Color.LightPink;

            var settIsCorrect = CurrentSettings.IsCorrect;
            if (settIsCorrect && clearStatus)
                ClearErrorStatus();

            btnSearch.Enabled = isCorrectPath
                                && settIsCorrect
                                && !txtPattern.Text.IsNullOrEmpty()
                                && trvMain.Nodes["trvServers"].Nodes.Cast<TreeNode>().Any(x => x.Checked)
                                && trvMain.Nodes["trvTypes"].Nodes.Cast<TreeNode>().Any(x => x.Checked);

            buttonExport.Enabled = dgvFiles.RowCount > 0;
            buttonFilter.Enabled = buttonReset.Enabled = OverallResultList != null && OverallResultList.Count > 0;
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        void ClearForm(bool saveData = true)
        {
            if (saveData)
                SaveData();

            OverallResultList?.Clear();
            OverallResultList = null;

            ClearDGV();

            Progress = 0;
            _completedFilesStatus.Text = @"0";
            _totalFilesStatus.Text = @"0";
            _findedInfo.Text = @"0";

            ReportStatus(string.Empty, ReportStatusType.Success);

            STREAM.GarbageCollect();
        }

        void ClearDGV()
        {
            try
            {
                dgvFiles.DataSource = null;
                dgvFiles.Rows.Clear();
                dgvFiles.Refresh();
                descriptionText.Text = string.Empty;
                _message.Text = string.Empty;
                _traceMessage.Text = string.Empty;
                buttonExport.Enabled = false;
                buttonFilter.Enabled = buttonReset.Enabled = OverallResultList != null && OverallResultList.Count > 0;
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private bool _isLastWasError = false;

        void ReportStatus(string message, ReportStatusType type)
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

        void ClearErrorStatus()
        {
            if (!_isLastWasError)
                return;
            _statusInfo.BackColor = SystemColors.Control;
            _statusInfo.ForeColor = Color.Black;
            _statusInfo.Text = string.Empty;
        }

        public override string ToString()
        {
            return CurrentSettings?.ToString();
        }
    }

    public enum ReportStatusType
    {
        Success = 0,
        Warning = 1,
        Error = 2
    }
}