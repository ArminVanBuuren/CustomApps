using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
        private bool _oldDateStartChecked = false;
        private bool _oldDateEndChecked = false;
        private bool _settingsLoaded = false;

        private readonly ToolStripStatusLabel _statusInfo;
        private readonly ToolStripStatusLabel _findedInfo;
        private readonly ToolStripStatusLabel _completedFilesStatus;
        private readonly ToolStripStatusLabel _totalFilesStatus;
        private readonly Editor _message;
        private readonly Editor _fullTrace;
        

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

        public Reader.LogsReader MainReader { get; private set; }


        public LogsReaderForm()
        {
            InitializeComponent();
            try
            {
                dgvFiles.AutoGenerateColumns = false;

                #region Set StatusStrip

                var statusStripItemsPaddingStart = new Padding(0, 2, 0, 2);
                var statusStripItemsPaddingMiddle = new Padding(-3, 2, 0, 2);
                var statusStripItemsPaddingEnd = new Padding(-3, 2, 1, 2);

                var autor = new ToolStripButton("?") {Font = new Font("Verdana", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0), Margin = new Padding(0, 0, 0, 2), ForeColor = Color.Blue };
                autor.Click += (sender, args) => { Utils.MessageShow(@"Hello! This is a universal application for searching and parsing applications logs, as well as convenient viewing.", ASSEMBLY.Company, false); };
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
                _statusInfo = new ToolStripStatusLabel("") {Font = this.Font, Margin = statusStripItemsPaddingStart};
                statusStrip.Items.Add(_statusInfo);

                #endregion

                var tooltipPrintXML = new ToolTip {InitialDelay = 100};
                tooltipPrintXML.SetToolTip(useRegex, Resources.LRSettings_UseRegexComment);
                tooltipPrintXML.SetToolTip(serversText, Resources.LRSettingsScheme_ServersComment);
                tooltipPrintXML.SetToolTip(fileNames, Resources.LRSettingsScheme_TypesComment);
                tooltipPrintXML.SetToolTip(maxThreadsText, Resources.LRSettingsScheme_MaxThreadsComment);
                tooltipPrintXML.SetToolTip(logDirText, Resources.LRSettingsScheme_LogsDirectoryComment);
                tooltipPrintXML.SetToolTip(maxLinesStackText, Resources.LRSettingsScheme_MaxTraceLinesComment);
                tooltipPrintXML.SetToolTip(dateTimePickerStart, Resources.Form_DateFilterComment);
                tooltipPrintXML.SetToolTip(dateTimePickerEnd, Resources.Form_DateFilterComment);
                tooltipPrintXML.SetToolTip(traceLikeText, Resources.Form_TraceNameLikeComment);
                tooltipPrintXML.SetToolTip(traceNotLikeText, Resources.Form_TraceNameNotLikeComment);
                tooltipPrintXML.SetToolTip(msgFilterText, Resources.Form_MessageFilterComment);

                dgvFiles.CellFormatting += DgvFiles_CellFormatting;

                var notepad = new NotepadControl();
                MainSplitContainer.Panel2.Controls.Add(notepad);
                _message = notepad.AddDocument("Message", string.Empty, Language.XML);
                _fullTrace = notepad.AddDocument("Full Trace", string.Empty);
                notepad.TabsFont = this.Font;
                notepad.TextFont = new Font("Segoe UI", 10F);
                notepad.Dock = DockStyle.Fill;
                notepad.SelectEditor(0);
                notepad.ReadOnly = true;

                dateTimePickerStart.ValueChanged += (sender, args) =>
                {
                    if (_oldDateStartChecked || !dateTimePickerStart.Checked)
                        return;
                    var today = DateTime.Now;
                    _oldDateStartChecked = true;
                    dateTimePickerStart.Value = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
                };
                dateTimePickerEnd.ValueChanged += (sender, args) =>
                {
                    if (_oldDateEndChecked || !dateTimePickerEnd.Checked)
                        return;
                    var today = DateTime.Now;
                    _oldDateEndChecked = true;
                    dateTimePickerEnd.Value = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);
                };
            }
            catch (Exception ex)
            {
                Utils.MessageShow(ex.ToString(), @"Initialization");
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
                traceLikeText.AssignValue(UserSettings.TraceLike, traceLikeText_TextChanged);
                traceNotLikeText.AssignValue(UserSettings.TraceNotLike, traceNotLikeText_TextChanged);
                msgFilterText.AssignValue(UserSettings.Message, msgFilterText_TextChanged);
                useRegex.Checked = UserSettings.UseRegex;

                serversText.Text = CurrentSettings.Servers;
                fileNames.Text = CurrentSettings.Types;
                maxThreadsText.AssignValue(CurrentSettings.MaxThreads, maxThreadsText_TextChanged);
                logDirText.AssignValue(CurrentSettings.LogsDirectory, logDirText_TextChanged);
                maxLinesStackText.AssignValue(CurrentSettings.MaxTraceLines, maxLinesStackText_TextChanged);
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
                ReportStatus(ex.Message, true);
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
                ReportStatus(ex.Message, true);
            }
        }

        public void LogsReaderKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.F5 when btnSearch.Enabled && !IsWorking:
                        ButtonStartStop_Click(this, EventArgs.Empty);
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
                    case Keys.Escape when btnSearch.Enabled && IsWorking:
                        ButtonStartStop_Click(this, EventArgs.Empty);
                        break;
                }
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, true);
            }
        }

        private async void ButtonStartStop_Click(object sender, EventArgs e)
        {
            if (!IsWorking)
            {
                var stop = new Stopwatch();
                try
                {
                    MainReader = new Reader.LogsReader(CurrentSettings, 
                        trvMain.Nodes["trvServers"].Nodes.Cast<TreeNode>().Where(x => x.Checked).Select(x => x.Text),
                        trvMain.Nodes["trvTypes"].Nodes.Cast<TreeNode>().Where(x => x.Checked).Select(x => x.Text),
                        txtPattern.Text, 
                        useRegex.Checked);
                    MainReader.OnProcessReport += ReportStatusOfProcess;

                    stop.Start();
                    IsWorking = true;
                    ChangeFormStatus();
                    ReportStatus(@"Working...", false);

                    await MainReader.StartAsync();

                    OverallResultList = new DataTemplateCollection(MainReader.ResultsOfSuccess);
                    OverallResultList.AddRange(MainReader.ResultsOfError.OrderBy(x => x.DateOfTrace));

                    if (await AssignResult(false))
                    {
                        ReportStatus($"Finished in {stop.Elapsed.ToReadableString()}", false);
                    }

                    stop.Stop();
                }
                catch (Exception ex)
                {
                    ReportStatus(ex.Message, true);
                }
                finally
                {
                    if (MainReader != null)
                    {
                        MainReader.OnProcessReport -= ReportStatusOfProcess;
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
                ReportStatus(@"Stopping...", false);
            }
        }

        void ReportStatusOfProcess(int countMatches, int percentOfProgeress, int filesCompleted, int totalFiles)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    _findedInfo.Text = countMatches.ToString();
                    progressBar.Value = percentOfProgeress;
                    _completedFilesStatus.Text = filesCompleted.ToString();
                    _totalFilesStatus.Text = totalFiles.ToString();
                }));
            }
            else
            {
                _findedInfo.Text = countMatches.ToString();
                progressBar.Value = percentOfProgeress;
                _completedFilesStatus.Text = filesCompleted.ToString();
                _totalFilesStatus.Text = totalFiles.ToString();
            }
        }

        private async void buttonFilter_Click(object sender, EventArgs e)
        {
            await AssignResult(true);
        }

        private async void buttonReset_Click(object sender, EventArgs e)
        {
            await AssignResult(false);
        }

        async Task<bool> AssignResult(bool applyFilter)
        {
            ClearDGV();
            ClearErrorStatus();

            if (OverallResultList == null)
                return false;

            IEnumerable<DataTemplate> result = new List<DataTemplate>(OverallResultList);

            if (!result.Any())
            {
                ReportStatus(@"No logs found", true);
                return false;
            }

            if (applyFilter)
            {
                if (dateTimePickerStart.Checked && dateTimePickerEnd.Checked && dateTimePickerStart.Value > dateTimePickerEnd.Value)
                {
                    ReportStatus(@"Date of end must be greater than date of start.", true);
                    return false;
                }

                if (dateTimePickerStart.Checked && dateTimePickerEnd.Checked)
                    result = result.Where(x => x.DateOfTrace != null && x.DateOfTrace.Value >= dateTimePickerStart.Value && x.DateOfTrace.Value <= dateTimePickerEnd.Value);
                else if (dateTimePickerStart.Checked)
                    result = result.Where(x => x.DateOfTrace != null && x.DateOfTrace.Value >= dateTimePickerStart.Value);
                else if (dateTimePickerEnd.Checked)
                    result = result.Where(x => x.DateOfTrace != null && x.DateOfTrace.Value <= dateTimePickerEnd.Value);


                var like = traceLikeText.Text.IsNullOrEmptyTrim()
                    ? new string[] { }
                    : traceLikeText.Text.Split(',').GroupBy(p => p.Trim(), StringComparer.CurrentCultureIgnoreCase).Where(x => !x.Key.IsNullOrEmptyTrim()).Select(x => x.Key);
                var notLike = traceNotLikeText.Text.IsNullOrEmptyTrim()
                    ? new string[] { }
                    : traceNotLikeText.Text.Split(',').GroupBy(p => p.Trim(), StringComparer.CurrentCultureIgnoreCase).Where(x => !x.Key.IsNullOrEmptyTrim()).Select(x => x.Key);
                if (like.Any() && notLike.Any())
                {
                    if (!like.Except(notLike).Any())
                    {
                        ReportStatus(@"Value of ""Trace Like"" can't be equal value of ""Trace Not Like""!", true);
                        return false;
                    }

                    result = result.Where(x => !x.Trace.IsNullOrEmptyTrim() && like.Any(p => x.Trace.StringContains(p)) && !notLike.Any(p => x.Trace.StringContains(p)));
                }
                else if (like.Any())
                    result = result.Where(x => !x.Trace.IsNullOrEmptyTrim() && like.Any(p => x.Trace.StringContains(p)));
                else if (notLike.Any())
                    result = result.Where(x => !x.Trace.IsNullOrEmptyTrim() && !notLike.Any(p => x.Trace.StringContains(p)));


                var msgFilter = msgFilterText.Text.IsNullOrEmptyTrim()
                    ? new string[] { }
                    : msgFilterText.Text.Split(',').GroupBy(p => p.Trim(), StringComparer.CurrentCultureIgnoreCase).Where(x => !x.Key.IsNullOrEmptyTrim()).Select(x => x.Key);
                if (msgFilter.Any())
                    result = result.Where(x => !x.Message.IsNullOrEmptyTrim() && msgFilter.Any(p => x.Message.StringContains(p)));

                if (!result.Any())
                {
                    ReportStatus(@"No filter results found", true);
                    return false;
                }
            }

            await dgvFiles.AssignCollectionAsync(result, null);

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
            _fullTrace.Enabled = !IsWorking;
            descriptionText.Enabled = !IsWorking;
            useRegex.Enabled = !IsWorking;
            serversText.Enabled = !IsWorking;
            fileNames.Enabled = !IsWorking;
            maxThreadsText.Enabled = !IsWorking;
            logDirText.Enabled = !IsWorking;
            maxLinesStackText.Enabled = !IsWorking;
            dateTimePickerStart.Enabled = !IsWorking;
            dateTimePickerEnd.Enabled = !IsWorking;
            traceNotLikeText.Enabled = !IsWorking;
            traceLikeText.Enabled = !IsWorking;
            msgFilterText.Enabled = !IsWorking;
            buttonFilter.Enabled = buttonReset.Enabled = OverallResultList != null && OverallResultList.Count > 0;
        }

        private void DgvFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                var row = ((DataGridView) sender).Rows[e.RowIndex];
                var tempaltePrivateID = TryGetPrivateID(row);
                if (tempaltePrivateID <= -1 || OverallResultList == null)
                    return;

                var template = OverallResultList[tempaltePrivateID];

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
                ReportStatus(ex.Message, true);
            }
        }

        private void dgvFiles_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgvFiles.CurrentRow == null || dgvFiles.SelectedRows.Count == 0)
                    return;

                var tempalteID = TryGetPrivateID(dgvFiles.SelectedRows[0]);

                if (tempalteID == -1 || OverallResultList == null)
                {
                    _message.Text = string.Empty;
                    _fullTrace.Text = string.Empty;
                    return;
                }

                var template = OverallResultList[tempalteID];
                var message = XML.RemoveUnallowable(template.Message, " ");

                descriptionText.Text = template.Description;
                _message.Text = message.IsXml(out var xmlDoc) ? xmlDoc.PrintXml() : message.Trim('\r', '\n');
                _fullTrace.Text = template.EntireTrace;
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, true);
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

        static int TryGetPrivateID(DataGridViewRow row)
        {
            if (row == null)
                return -1;

            foreach (DataGridViewCell cell in row.Cells)
            {
                if (!cell.OwningColumn.Name.Equals("PrivateID", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (cell.Value is int value)
                    return value;
            }

            return -1;
        }
    
        private void serversText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.Servers = serversText.Text;
            serversText.AssignValue(CurrentSettings.Servers, serversText_TextChanged);

            //заполняем список серверов из параметра
            trvMain.Nodes["trvServers"].Nodes.Clear();
            foreach (var s in CurrentSettings.Servers.Split(',').GroupBy(p => p.TrimWhiteSpaces(), StringComparer.CurrentCultureIgnoreCase).OrderBy(p => p.Key))
            {
                if (s.Key.IsNullOrEmptyTrim())
                    continue;
                trvMain.Nodes["trvServers"].Nodes.Add(s.Key.Trim().ToUpper());
            }

            trvMain.ExpandAll();

            ValidationCheck();
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void typesText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.Types = fileNames.Text;
            fileNames.AssignValue(CurrentSettings.Types, typesText_TextChanged);

            //заполняем список типов из параметра
            trvMain.Nodes["trvTypes"].Nodes.Clear();
            foreach (var s in CurrentSettings.Types.Split(',').GroupBy(p => p.TrimWhiteSpaces(), StringComparer.CurrentCultureIgnoreCase).OrderBy(p => p.Key))
            {
                if (s.Key.IsNullOrEmptyTrim())
                    continue;
                trvMain.Nodes["trvTypes"].Nodes.Add(s.Key.Trim().ToUpper());
            }

            trvMain.ExpandAll();

            ValidationCheck();
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void maxThreadsText_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(maxThreadsText.Text, out var res))
                MaxThreadsTextSave(res);
        }

        private void maxThreadsText_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(maxThreadsText.Text, out var res))
                res = -1;
            MaxThreadsTextSave(res);
        }

        void MaxThreadsTextSave(int res)
        {
            CurrentSettings.MaxThreads = res;
            maxThreadsText.AssignValue(CurrentSettings.MaxThreads, maxThreadsText_TextChanged);
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void logDirText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.LogsDirectory = logDirText.Text;
            logDirText.AssignValue(CurrentSettings.LogsDirectory, logDirText_TextChanged);
            ValidationCheck();
            OnSchemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void maxLinesStackText_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(maxLinesStackText.Text, out var res))
                MaxLinesStackTextSave(res);
        }

        private void maxLinesStackText_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(maxLinesStackText.Text, out var res))
                res = -1;

            MaxLinesStackTextSave(res);
        }

        void MaxLinesStackTextSave(int res)
        {
            CurrentSettings.MaxTraceLines = res;
            maxLinesStackText.AssignValue(CurrentSettings.MaxTraceLines, maxLinesStackText_TextChanged);
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

        private void traceLikeText_TextChanged(object sender, EventArgs e)
        {
            UserSettings.TraceLike = traceLikeText.Text;
        }

        private void traceNotLikeText_TextChanged(object sender, EventArgs e)
        {
            UserSettings.TraceNotLike = traceNotLikeText.Text;
        }

        private void msgFilterText_TextChanged(object sender, EventArgs e)
        {
            UserSettings.Message = msgFilterText.Text;
        }

        void ValidationCheck()
        {
            var isCorrectPath = IO.CHECK_PATH.IsMatch(logDirText.Text);
            logDirText.BackColor = isCorrectPath ? SystemColors.Window : Color.LightPink;

            var settIsCorrect = CurrentSettings.IsCorrect;
            if (settIsCorrect)
                ClearErrorStatus();

            btnSearch.Enabled = isCorrectPath
                                && settIsCorrect
                                && !txtPattern.Text.IsNullOrEmptyTrim()
                                && trvMain.Nodes["trvServers"].Nodes.Cast<TreeNode>().Any(x => x.Checked)
                                && trvMain.Nodes["trvTypes"].Nodes.Cast<TreeNode>().Any(x => x.Checked);

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

            progressBar.Value = 0;
            _completedFilesStatus.Text = @"0";
            _totalFilesStatus.Text = @"0";
            _findedInfo.Text = @"0";

            ReportStatus(string.Empty, false);

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
                _fullTrace.Text = string.Empty;
                buttonFilter.Enabled = buttonReset.Enabled = OverallResultList != null && OverallResultList.Count > 0;
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, true);
            }
        }

        private bool _isLastWasError = false;
        void ReportStatus(string message, bool isError)
        {
            _statusInfo.Text = message;
            _statusInfo.ForeColor = !isError ? Color.Black : Color.Red;
            _isLastWasError = isError;
        }

        void ClearErrorStatus()
        {
            if (!_isLastWasError)
                return;
            _statusInfo.Text = string.Empty;
            _statusInfo.ForeColor = Color.Black;
        }
    }
}