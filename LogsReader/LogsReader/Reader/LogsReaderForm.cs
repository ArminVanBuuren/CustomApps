﻿using System;
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
using Utils;
using Utils.WinForm.DataGridViewHelper;
using Utils.WinForm.Notepad;

namespace LogsReader.Reader
{
    public sealed partial class LogsReaderForm : UserControl
    {
        private readonly Func<DateTime> _getStartDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        private readonly Func<DateTime> _getEndDate = () => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

        private bool _oldDateStartChecked = false;
        private bool _oldDateEndChecked = false;
        private bool _settingsLoaded = false;

        private readonly ToolStripStatusLabel _statusInfo;
        private readonly ToolStripStatusLabel _findedInfo;
        private readonly ToolStripStatusLabel _completedFilesStatus;
        private readonly ToolStripStatusLabel _totalFilesStatus;
        private readonly Editor _message;
        private readonly Editor _traceMessage;
        

        /// <summary>
        /// Сохранить изменения в конфиг
        /// </summary>
        public event EventHandler OnSchemeChanged;

        public ToolStripStatusLabel CPUUsage { get; }
        public ToolStripStatusLabel ThreadsUsage { get; }
        public ToolStripStatusLabel RAMUsage { get; }

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

        public LogsReaderForm()
        {
            InitializeComponent();
            try
            {
                dgvFiles.AutoGenerateColumns = false;
                dgvFiles.CellFormatting += DgvFiles_CellFormatting;

                #region Set StatusStrip

                var statusStripItemsPaddingStart = new Padding(0, 2, 0, 2);
                var statusStripItemsPaddingMiddle = new Padding(-3, 2, 0, 2);
                var statusStripItemsPaddingEnd = new Padding(-3, 2, 1, 2);

                var autor = new ToolStripButton("?") {Font = new Font("Verdana", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0), Margin = new Padding(0, 0, 0, 2), ForeColor = Color.Blue };
                autor.Click += (sender, args) => { Util.MessageShow(@"Hello! This is a universal application for searching and parsing applications logs, as well as convenient viewing.", $"© {ASSEMBLY.Company}", false); };
                statusStrip.Items.Add(autor);

                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(new ToolStripStatusLabel("CPU:") {Font = this.Font, Margin = statusStripItemsPaddingStart });
                CPUUsage = new ToolStripStatusLabel("    ") {Font = this.Font, Margin = new Padding(-7, 2, 1, 2)};
                statusStrip.Items.Add(CPUUsage);

                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(new ToolStripStatusLabel("Threads:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                ThreadsUsage = new ToolStripStatusLabel("  ") {Font = this.Font, Margin = statusStripItemsPaddingEnd};
                statusStrip.Items.Add(ThreadsUsage);

                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(new ToolStripStatusLabel("RAM:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                RAMUsage = new ToolStripStatusLabel("       ") {Font = this.Font, Margin = statusStripItemsPaddingEnd };
                statusStrip.Items.Add(RAMUsage);

                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(new ToolStripStatusLabel("Files completed:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                _completedFilesStatus = new ToolStripStatusLabel("0") {Font = this.Font, Margin = statusStripItemsPaddingMiddle };
                statusStrip.Items.Add(_completedFilesStatus);
                statusStrip.Items.Add(new ToolStripStatusLabel("of") {Font = this.Font, Margin = statusStripItemsPaddingMiddle });
                _totalFilesStatus = new ToolStripStatusLabel("0") {Font = this.Font, Margin = statusStripItemsPaddingEnd};
                statusStrip.Items.Add(_totalFilesStatus);

                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(new ToolStripStatusLabel("Overall found") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                _findedInfo = new ToolStripStatusLabel("0") {Font = this.Font, Margin = statusStripItemsPaddingMiddle };
                statusStrip.Items.Add(_findedInfo);
                statusStrip.Items.Add(new ToolStripStatusLabel("matches") {Font = this.Font, Margin = statusStripItemsPaddingEnd});

                statusStrip.Items.Add(new ToolStripSeparator());
                _statusInfo = new ToolStripStatusLabel("") {Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Margin = statusStripItemsPaddingStart};
                statusStrip.Items.Add(_statusInfo);

                #endregion

                var tooltipPrintXML = new ToolTip {InitialDelay = 100};
                tooltipPrintXML.SetToolTip(txtPattern, Resources.Form_SearchComment);
                tooltipPrintXML.SetToolTip(useRegex, Resources.LRSettings_UseRegexComment);
                tooltipPrintXML.SetToolTip(serversText, Resources.LRSettingsScheme_ServersComment);
                tooltipPrintXML.SetToolTip(fileNames, Resources.LRSettingsScheme_TypesComment);
                tooltipPrintXML.SetToolTip(maxThreadsText, Resources.LRSettingsScheme_MaxThreadsComment);
                tooltipPrintXML.SetToolTip(logDirText, Resources.LRSettingsScheme_LogsDirectoryComment);
                tooltipPrintXML.SetToolTip(maxLinesStackText, Resources.LRSettingsScheme_MaxTraceLinesComment);
                tooltipPrintXML.SetToolTip(dateStartFilter, Resources.Form_DateFilterComment);
                tooltipPrintXML.SetToolTip(dateEndFilter, Resources.Form_DateFilterComment);
                tooltipPrintXML.SetToolTip(traceNameLikeFilter, Resources.Form_TraceNameLikeComment);
                tooltipPrintXML.SetToolTip(traceNameNotLikeFilter, Resources.Form_TraceNameNotLikeComment);
                tooltipPrintXML.SetToolTip(traceMessageFilter, Resources.Form_MessageFilterComment);
                tooltipPrintXML.SetToolTip(alreadyUseFilter, Resources.Form_AlreadyUseFilterComment);
                tooltipPrintXML.SetToolTip(rowsLimitText, Resources.LRSettingsScheme_RowsLimitCommentComment);

                var notepad = new NotepadControl();
                MainSplitContainer.Panel2.Controls.Add(notepad);
                _message = notepad.AddDocument("Message", string.Empty, Language.XML);
                _traceMessage = notepad.AddDocument("Trace", string.Empty);
                notepad.TabsFont = this.Font;
                notepad.TextFont = new Font("Segoe UI", 10F);
                notepad.Dock = DockStyle.Fill;
                notepad.SelectEditor(0);
                notepad.ReadOnly = true;

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
                Util.MessageShow(ex.ToString(), @"Initialization");
            }
        }

        public void LoadForm(LRSettingsScheme scheme)
        {
            try
            {
                CurrentSettings = scheme;
                CurrentSettings.ReportStatus += ReportStatus;
                UserSettings = new UserSettings(CurrentSettings.Name);

                txtPattern.AssignValue(UserSettings.PreviousSearch, txtPattern_TextChanged);
                dateStartFilter.Checked = UserSettings.DateStartChecked;
                if(dateStartFilter.Checked)
                    dateStartFilter.Value = _getStartDate.Invoke();
                dateEndFilter.Checked = UserSettings.DateEndChecked;
                if (dateEndFilter.Checked)
                    dateEndFilter.Value = _getEndDate.Invoke();
                traceNameLikeFilter.AssignValue(UserSettings.TraceLike, traceNameLikeFilter_TextChanged);
                traceNameNotLikeFilter.AssignValue(UserSettings.TraceNotLike, traceNameNotLikeFilter_TextChanged);
                traceMessageFilter.AssignValue(UserSettings.Message, traceMessageFilter_TextChanged);
                useRegex.Checked = UserSettings.UseRegex;

                serversText.Text = CurrentSettings.Servers;
                logDirText.AssignValue(CurrentSettings.LogsDirectory, logDirText_TextChanged);
                fileNames.Text = CurrentSettings.Types;
                maxLinesStackText.AssignValue(CurrentSettings.MaxTraceLines, maxLinesStackText_TextChanged);
                maxThreadsText.AssignValue(CurrentSettings.MaxThreads, maxThreadsText_TextChanged);
                rowsLimitText.AssignValue(CurrentSettings.RowsLimit, rowsLimitText_TextChanged);

                btnSearch.Enabled = CurrentSettings.IsCorrect;
            }
            finally
            {
                ApplySettings();
                trvMain.Nodes["trvServers"].Checked = false;
                trvMain.Nodes["trvTypes"].Checked = false;
                CheckTreeViewNode(trvMain.Nodes["trvServers"], false);
                CheckTreeViewNode(trvMain.Nodes["trvTypes"], false);
                ClearForm();
                ValidationCheck();
            }
        }

        public void ApplySettings()
        {
            try
            {
                int i = 0;
                foreach (DataGridViewColumn column in dgvFiles.Columns)
                {
                    var valueStr = UserSettings.GetValue("COL" + i);
                    if (!valueStr.IsNullOrEmptyTrim() && int.TryParse(valueStr, out var value) && value > 1 && value < 1000)
                        dgvFiles.Columns[i].Width = value;

                    i++;
                }

                ParentSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(ParentSplitContainer), 25, 1000, ParentSplitContainer.SplitterDistance);
                MainSplitContainer.SplitterDistance = UserSettings.GetValue(nameof(MainSplitContainer), 25, 1000, MainSplitContainer.SplitterDistance);
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
            finally
            {
                _settingsLoaded = true;
            }
        }

        public void SaveInterfaceParams()
        {
            try
            {
                if (!_settingsLoaded)
                    return;

                int i = 0;
                foreach (DataGridViewColumn column in dgvFiles.Columns)
                {
                    UserSettings.SetValue("COL" + i, dgvFiles.Columns[i].Width);
                    i++;
                }

                UserSettings.SetValue(nameof(ParentSplitContainer), ParentSplitContainer.SplitterDistance);
                UserSettings.SetValue(nameof(MainSplitContainer), MainSplitContainer.SplitterDistance);
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
                        btnSearch_Click(this, EventArgs.Empty);
                        break;
                    case Keys.F6 when btnClear.Enabled:
                        ClearForm();
                        break;
                    case Keys.S when e.Control && buttonExport.Enabled:
                        buttonExport_Click(this, EventArgs.Empty);
                        break;
                    case Keys.F7 when buttonFilter.Enabled:
                        buttonFilter_Click(this, EventArgs.Empty);
                        break;
                    case Keys.F8 when buttonReset.Enabled:
                        buttonReset_Click(this, EventArgs.Empty);
                        break;
                    case Keys.Escape when btnSearch.Enabled && IsWorking:
                        btnSearch_Click(this, EventArgs.Empty);
                        break;
                }
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
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
                    ReportStatus(@"Working...", ReportStatusType.Success);

                    await MainReader.StartAsync();

                    OverallResultList = new DataTemplateCollection(CurrentSettings, MainReader.ResultsOfSuccess);
                    OverallResultList.AddRange(MainReader.ResultsOfError.OrderBy(x => x.DateOfTrace));

                    if (await AssignResult(filter))
                    {
                        ReportStatus($"Finished in {stop.Elapsed.ToReadableString()}", ReportStatusType.Success);
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
                    dgvFiles.Focus();
                }
            }
            else
            {
                MainReader?.Stop();
                ReportStatus(@"Stopping...", ReportStatusType.Success);
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

        private async void buttonExport_Click(object sender, EventArgs e)
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
                ReportStatus(@"Exporting...", ReportStatusType.Success);
                fileName = Path.GetFileName(desctination);

                int i = 0;
                Progress = 0;
                using (var writer = new StreamWriter(desctination, false, new UTF8Encoding(false)))
                {
                    await writer.WriteLineAsync(GetCSVRow(new[] {"ID", "Server", "File", "Trace name", "Date", "Description", "Message"}));
                    foreach (DataGridViewRow row in dgvFiles.Rows)
                    {
                        var privateID = (int) row.Cells["PrivateID"].Value;
                        var template = OverallResultList[privateID];
                        await writer.WriteLineAsync(GetCSVRow(new[] {template.ID.ToString(), template.Server, template.File, template.TraceName, template.Date, template.Description, $"\"{template.Message.Trim()}\""}));
                        writer.Flush();

                        Progress = (int)Math.Round((double)(100 * ++i) / dgvFiles.RowCount);
                    }
                    writer.Close();
                }

                ReportStatus($"Successfully exported to file \"{fileName}\"", ReportStatusType.Success);
            }
            catch (Exception ex)
            {
                ReportStatus($"Unable to save file \"{fileName}\". {ex.Message}", ReportStatusType.Error);
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
                traceNameLikeFilter.Text,
                traceNameNotLikeFilter.Text,
                traceMessageFilter.Text);
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
                ReportStatus(@"No logs found", ReportStatusType.Warning);
                return false;
            }

            if (filter != null)
            {
                result = filter.FilterCollection(result);

                if (!result.Any())
                {
                    ReportStatus(@"No filter results found", ReportStatusType.Warning);
                    return false;
                }
            }

            await dgvFiles.AssignCollectionAsync(result, null);

            buttonExport.Enabled = dgvFiles.RowCount > 0;
            return true;
        }

        void ChangeFormStatus()
        {
            if (IsWorking)
            {
                ParentSplitContainer.Cursor = Cursors.WaitCursor;
                ClearForm();
            }
            else
            {
                ParentSplitContainer.Cursor = Cursors.Default;
            }

            btnSearch.Text = IsWorking ? @"      Stop [Esc]" : @"      Search [F5]";
            btnClear.Enabled = !IsWorking;
            trvMain.Enabled = !IsWorking;
            txtPattern.Enabled = !IsWorking;
            dgvFiles.Enabled = !IsWorking;
            _message.Enabled = !IsWorking;
            _traceMessage.Enabled = !IsWorking;
            descriptionText.Enabled = !IsWorking;
            useRegex.Enabled = !IsWorking;
            serversText.Enabled = !IsWorking;
            fileNames.Enabled = !IsWorking;
            maxThreadsText.Enabled = !IsWorking;
            rowsLimitText.Enabled = !IsWorking;
            logDirText.Enabled = !IsWorking;
            maxLinesStackText.Enabled = !IsWorking;
            dateStartFilter.Enabled = !IsWorking;
            dateEndFilter.Enabled = !IsWorking;
            traceNameNotLikeFilter.Enabled = !IsWorking;
            traceNameLikeFilter.Enabled = !IsWorking;
            traceMessageFilter.Enabled = !IsWorking;
            alreadyUseFilter.Enabled = !IsWorking;
            buttonExport.Enabled = dgvFiles.RowCount > 0;
            buttonFilter.Enabled = buttonReset.Enabled = OverallResultList != null && OverallResultList.Count > 0;
        }

        private void DgvFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                var row = ((DataGridView) sender).Rows[e.RowIndex];
                var privateID = (int)row.Cells["PrivateID"].Value;
                if (privateID <= -1 || OverallResultList == null)
                    return;

                var template = OverallResultList[privateID];

                if (template.IsMatched)
                {
                    if (template.DateOfTrace == null)
                    {
                        row.DefaultCellStyle.BackColor = Color.Yellow;
                        foreach (DataGridViewCell cell2 in row.Cells)
                            cell2.ToolTipText = "Date value is incorrect";
                        return;
                    }

                    row.DefaultCellStyle.BackColor = Color.White;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.LightPink;
                    foreach (DataGridViewCell cell2 in row.Cells)
                        cell2.ToolTipText = "Doesn't match by \"TraceLinePattern\"";
                }
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private void dgvFiles_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgvFiles.CurrentRow == null || dgvFiles.SelectedRows.Count == 0)
                    return;

                var privateID = (int)dgvFiles.SelectedRows[0].Cells["PrivateID"].Value;

                if (privateID == -1 || OverallResultList == null)
                {
                    _message.Text = string.Empty;
                    _traceMessage.Text = string.Empty;
                    return;
                }

                var template = OverallResultList[privateID];
                var message = XML.RemoveUnallowable(template.Message, " ");

                descriptionText.Text = template.Description;
                _message.Text = message.IsXml(out var xmlDoc) ? xmlDoc.PrintXml() : message.TrimWhiteSpaces();
                _traceMessage.Text = template.TraceMessage;
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, ReportStatusType.Error);
            }
        }

        private void dgvFiles_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hti = dgvFiles.HitTest(e.X, e.Y);
                dgvFiles.ClearSelection();
                dgvFiles.Rows[hti.RowIndex].Selected = true;
            }
        }
    
        private void serversText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.Servers = serversText.Text;
            serversText.AssignValue(CurrentSettings.Servers, serversText_TextChanged);

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

        private void logDirText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.LogsDirectory = logDirText.Text;
            logDirText.AssignValue(CurrentSettings.LogsDirectory, logDirText_TextChanged);
            ValidationCheck();
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void typesText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.Types = fileNames.Text;
            fileNames.AssignValue(CurrentSettings.Types, typesText_TextChanged);

            //заполняем список типов из параметра
            trvMain.Nodes["trvTypes"].Nodes.Clear();
            foreach (var s in CurrentSettings.Types.Split(',').GroupBy(p => p.TrimWhiteSpaces(), StringComparer.InvariantCultureIgnoreCase).OrderBy(p => p.Key))
            {
                if (s.Key.IsNullOrEmptyTrim())
                    continue;
                trvMain.Nodes["trvTypes"].Nodes.Add(s.Key.Trim().ToUpper());
            }

            trvMain.ExpandAll();

            ValidationCheck();
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void maxLinesStackText_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(maxLinesStackText.Text, out var value))
                MaxLinesStackTextSave(value);
        }

        private void maxLinesStackText_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(maxLinesStackText.Text, out var value))
                value = -1;
            MaxLinesStackTextSave(value);
        }

        void MaxLinesStackTextSave(int value)
        {
            CurrentSettings.MaxTraceLines = value;
            maxLinesStackText.AssignValue(CurrentSettings.MaxTraceLines, maxLinesStackText_TextChanged);
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void maxThreadsText_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(maxThreadsText.Text, out var value))
                MaxThreadsTextSave(value);
        }

        private void maxThreadsText_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(maxThreadsText.Text, out var value))
                value = -1;
            MaxThreadsTextSave(value);
        }

        void MaxThreadsTextSave(int value)
        {
            CurrentSettings.MaxThreads = value;
            maxThreadsText.AssignValue(CurrentSettings.MaxThreads, maxThreadsText_TextChanged);
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void rowsLimitText_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(rowsLimitText.Text, out var value))
                RowsLimitTextSave(value);
        }

        private void rowsLimitText_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(rowsLimitText.Text, out var value))
                value = -1;
            RowsLimitTextSave(value);
        }

        void RowsLimitTextSave(int value)
        {
            CurrentSettings.RowsLimit = value;
            rowsLimitText.AssignValue(CurrentSettings.RowsLimit, rowsLimitText_TextChanged);
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void trvMain_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 0)
                CheckTreeViewNode(e.Node, e.Node.Checked);
            ValidationCheck();
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

        private void txtPattern_TextChanged(object sender, EventArgs e)
        {
            UserSettings.PreviousSearch = txtPattern.Text;
            ValidationCheck();
        }

        private void useRegex_CheckedChanged(object sender, EventArgs e)
        {
            UserSettings.UseRegex = useRegex.Checked;
        }

        private void traceNameLikeFilter_TextChanged(object sender, EventArgs e)
        {
            UserSettings.TraceLike = traceNameLikeFilter.Text;
        }

        private void traceNameNotLikeFilter_TextChanged(object sender, EventArgs e)
        {
            UserSettings.TraceNotLike = traceNameNotLikeFilter.Text;
        }

        private void traceMessageFilter_TextChanged(object sender, EventArgs e)
        {
            UserSettings.Message = traceMessageFilter.Text;
        }

        void ValidationCheck(bool clearStatus = true)
        {
            var isCorrectPath = IO.CHECK_PATH.IsMatch(logDirText.Text);
            logDirText.BackColor = isCorrectPath ? SystemColors.Window : Color.LightPink;

            var settIsCorrect = CurrentSettings.IsCorrect;
            if (settIsCorrect && clearStatus)
                ClearErrorStatus();

            btnSearch.Enabled = isCorrectPath
                                && settIsCorrect
                                && !txtPattern.Text.IsNullOrEmptyTrim()
                                && trvMain.Nodes["trvServers"].Nodes.Cast<TreeNode>().Any(x => x.Checked)
                                && trvMain.Nodes["trvTypes"].Nodes.Cast<TreeNode>().Any(x => x.Checked);

            buttonExport.Enabled = dgvFiles.RowCount > 0;
            buttonFilter.Enabled = buttonReset.Enabled = OverallResultList != null && OverallResultList.Count > 0;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        void ClearForm()
        {
            SaveInterfaceParams();

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
                _statusInfo.Text = message;
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
    }

    public enum ReportStatusType
    {
        Success = 0,
        Warning = 1,
        Error = 2
    }
}