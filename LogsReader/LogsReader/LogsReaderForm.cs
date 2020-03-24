using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogsReader.Properties;
using Utils;
using Utils.WinForm.DataGridViewHelper;
using Utils.WinForm.Notepad;

namespace LogsReader
{
    public delegate void ReportStatusHandler(string message, bool isError);

    public sealed partial class LogsReaderForm : UserControl
    {
        private readonly object _syncRootFinded = new object();
        private int _finded = 0;
        private bool _oldDateStartChecked = false;
        private bool _oldDateEndChecked = false;

        private readonly ToolStripStatusLabel _statusInfo;
        private readonly ToolStripStatusLabel _findedInfo;
        private readonly ToolStripStatusLabel _completedFilesStatus;
        private readonly ToolStripStatusLabel _totalFilesStatus;
        private readonly Editor _message;
        private readonly Editor _fullTrace;
        private Func<string, bool> _isMatchSearchPatternFunc = null;
        private MTFuncResult<FileLog, List<DataTemplate>> _multiTaskingHandler = null;

        /// <summary>
        /// Сохранить изменения в конфиг
        /// </summary>
        public event EventHandler OnSaveScheme;

        public ToolStripStatusLabel CPUUsage { get; }
        public ToolStripStatusLabel ThreadsUsage { get; }
        public ToolStripStatusLabel RAMUsage { get; }

        /// <summary>
        /// Статус выполнения поиска
        /// </summary>
        public bool IsWorking { get; private set; } = false;

        /// <summary>
        /// Запрос на ожидание остановки выполнения поиска
        /// </summary>
        public bool IsStopPending { get; private set; } = false;

        /// <summary>
        /// Юзерские настройки 
        /// </summary>
        public UserSettings UserSettings { get; private set; }

        /// <summary>
        /// Текущая схема настроек
        /// </summary>
        public LRSettingsScheme CurrentSettings { get; private set; }


        public DataTemplateCollection OverallResultList { get; private set; }

        /// <summary>
        /// Количество совпадений по критериям поиска
        /// </summary>
        public int Finded
        {
            get
            {
                lock (_syncRootFinded)
                    return _finded;
            }
            private set
            {
                lock (_syncRootFinded)
                    _finded = value;
            }
        }

        public LogsReaderForm()
        {
            InitializeComponent();
            dgvFiles.AutoGenerateColumns = false;

            try
            {
                #region Set StatusStrip

                var statusStripItemsPaddingStart = new Padding(0, 3, 0, 2);
                var statusStripItemsPaddingMiddle = new Padding(-3, 3, 0, 2);

                statusStrip.Items.Add(new ToolStripStatusLabel("CPU:") {Font = this.Font, Margin = new Padding(7, 3, 0, 2)});
                CPUUsage = new ToolStripStatusLabel("    ") {Font = this.Font, Margin = new Padding(-7, 3, 0, 2)};
                statusStrip.Items.Add(CPUUsage);

                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(new ToolStripStatusLabel("Threads:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                ThreadsUsage = new ToolStripStatusLabel("  ") {Font = this.Font, Margin = statusStripItemsPaddingMiddle};
                statusStrip.Items.Add(ThreadsUsage);

                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(new ToolStripStatusLabel("RAM:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                RAMUsage = new ToolStripStatusLabel("       ") {Font = this.Font, Margin = new Padding(-4, 3, 0, 2)};
                statusStrip.Items.Add(RAMUsage);

                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(new ToolStripStatusLabel("Files completed:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                _completedFilesStatus = new ToolStripStatusLabel("0") {Font = this.Font, Margin = statusStripItemsPaddingMiddle};
                statusStrip.Items.Add(_completedFilesStatus);
                statusStrip.Items.Add(new ToolStripStatusLabel("of") {Font = this.Font, Margin = statusStripItemsPaddingMiddle});
                _totalFilesStatus = new ToolStripStatusLabel("0") {Font = this.Font, Margin = statusStripItemsPaddingMiddle};
                statusStrip.Items.Add(_totalFilesStatus);

                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(new ToolStripStatusLabel("Overall found") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                _findedInfo = new ToolStripStatusLabel("0") {Font = this.Font, Margin = statusStripItemsPaddingMiddle};
                statusStrip.Items.Add(_findedInfo);
                statusStrip.Items.Add(new ToolStripStatusLabel("matches") {Font = this.Font, Margin = statusStripItemsPaddingMiddle});

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
                splitContainer2.Panel2.Controls.Add(notepad);
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
                Program.MessageShow(ex.ToString(), @"Initialization");
            }
        }

        public void LoadLogsReader(LRSettingsScheme scheme)
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
                trvMain.Nodes["trvServers"].Checked = false;
                trvMain.Nodes["trvTypes"].Checked = false;
                CheckTreeViewNode(trvMain.Nodes["trvServers"], false);
                CheckTreeViewNode(trvMain.Nodes["trvTypes"], false);
                ClearForm();
                ValidationCheck();
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
                    if (useRegex.Checked)
                    {
                        if (!REGEX.Verify(txtPattern.Text))
                        {
                            ReportStatus(@"Regular expression for search pattern is incorrect! Please check.", true);
                            return;
                        }

                        var searchPattern = new Regex(txtPattern.Text, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        _isMatchSearchPatternFunc = (input) => searchPattern.IsMatch(input);
                    }
                    else
                    {
                        _isMatchSearchPatternFunc = (input) => input.IndexOf(txtPattern.Text, StringComparison.OrdinalIgnoreCase) != -1;
                    }

                    stop.Start();
                    IsStopPending = false;
                    IsWorking = true;
                    ChangeFormStatus();
                    ReportStatus(@"Working...", false);

                    var kvpList = await Task<List<FileLog>>.Factory.StartNew(() => GetFileLogs(
                        trvMain.Nodes["trvServers"].Nodes.Cast<TreeNode>().Where(x => x.Checked),
                        trvMain.Nodes["trvTypes"].Nodes.Cast<TreeNode>().Where(x => x.Checked)));

                    if (kvpList.Count <= 0)
                    {
                        ReportStatus(@"No files logs found", true);
                        return;
                    }

                    var maxThreads = CurrentSettings.MaxThreads <= 0 ? kvpList.Count : CurrentSettings.MaxThreads;
                    // ThreadPriority.Lowest - необходим чтобы не залипал основной поток и не мешал другим процессам
                    _multiTaskingHandler = new MTFuncResult<FileLog, List<DataTemplate>>(ReadData, kvpList, maxThreads, ThreadPriority.Lowest);
                    new Action(CheckProgress).BeginInvoke(ProcessCompleted, null);
                    await _multiTaskingHandler.StartAsync();


                    OverallResultList = new DataTemplateCollection(_multiTaskingHandler.Result.CallBackList.Where(x => x.Result != null).SelectMany(x => x.Result));
                    var resultOfError = _multiTaskingHandler.Result.CallBackList.Where(x => x.Error != null).Aggregate(new List<DataTemplate>(), (listErr, x) =>
                    {
                        listErr.Add(new DataTemplate(x.Source, x.Error));
                        return listErr;
                    });
                    OverallResultList.AddRange(resultOfError.OrderByDescending(x => x.DateOfTrace));

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
                    IsWorking = false;
                    ChangeFormStatus();
                    if (stop.IsRunning)
                        stop.Stop();
                    dgvFiles.Focus();
                }
            }
            else
            {
                IsStopPending = true;
                _multiTaskingHandler?.Stop();
                ReportStatus(@"Stopping...", false);
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
            ReportStatus(string.Empty, false);

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

        List<FileLog> GetFileLogs(IEnumerable<TreeNode> servers, IEnumerable<TreeNode> traces)
        {
            var dirMatch = IO.CHECK_PATH.Match(CurrentSettings.LogsDirectory);
            var logsDirFormat = @"\\{0}\" + $"{dirMatch.Groups["DISC"]}${dirMatch.Groups["FULL"]}";
            var kvpList = new List<FileLog>();

            foreach (var serverNode in servers)
            {
                var serverDir = string.Format(logsDirFormat, serverNode.Text);
                if (!Directory.Exists(serverDir))
                    continue;

                var files = Directory.GetFiles(serverDir, "*", SearchOption.AllDirectories);
                foreach (var fileLog in files.Select(x => new FileLog(serverNode.Text, x)))
                {
                    if (traces.Any(x => fileLog.FileName.IndexOf(x.Text, StringComparison.CurrentCultureIgnoreCase) != -1))
                        kvpList.Add(fileLog);
                }
            }

            //var stop = new Stopwatch();
            //stop.Start();
            //var getCountLines = new MTFuncResult<FileLog, long>((input) => IO.CountLinesReader(input.FilePath), kvpList, kvpList.Count, ThreadPriority.Lowest);
            //getCountLines.Start();
            //var lines = getCountLines.Result.Values.Select(x => x.Result).Sum(x => x);
            //stop.Stop();

            return kvpList;
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

        void CheckProgress()
        {
            try
            {
                var total = _multiTaskingHandler?.Source.Count.ToString();
                while (IsWorking && _multiTaskingHandler != null && !_multiTaskingHandler.IsCompleted)
                {
                    var completed = _multiTaskingHandler.Result.Count.ToString();
                    var progress = _multiTaskingHandler.PercentOfComplete;

                    if (InvokeRequired)
                    {
                        Invoke(new MethodInvoker(delegate
                        {
                            progressBar.Value = progress;
                            _completedFilesStatus.Text = completed;
                            _totalFilesStatus.Text = total;
                            _findedInfo.Text = Finded.ToString();
                        }));
                    }

                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        void ProcessCompleted(IAsyncResult asyncResult)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        progressBar.Value = 100;
                        _findedInfo.Text = Finded.ToString();
                        if (_multiTaskingHandler != null)
                        {
                            _completedFilesStatus.Text = _multiTaskingHandler.Result.Count.ToString();
                            _totalFilesStatus.Text = _multiTaskingHandler.Source.Count().ToString();
                        }
                        else
                        {
                            _completedFilesStatus.Text = @"0";
                            _totalFilesStatus.Text = @"0";
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private List<DataTemplate> ReadData(FileLog fileLog)
        {
            var beforeTraceLines = new Queue<string>(CurrentSettings.MaxTraceLines);
            var listResult = new List<DataTemplate>();

            // FileShare должен быть ReadWrite. Иначе, если файл используется другим процессом то доступ к чтению файла будет запрещен.
            using (var inputStream = new FileStream(fileLog.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var inputReader = new StreamReader(inputStream, Encoding.GetEncoding("windows-1251")))
            {
                var stackLines = 0;
                string line;
                DataTemplate lastResult = null;
                while ((line = inputReader.ReadLine()) != null && !IsStopPending)
                {
                    if (lastResult != null)
                    {
                        // если стек лога превышает допустимый размер, то лог больше не дополняется
                        if (stackLines >= CurrentSettings.MaxTraceLines)
                        {
                            if (!lastResult.IsMatched)
                                listResult.Add(lastResult);
                            stackLines = 0;
                            lastResult = null;
                        }
                        else
                        {
                            if (lastResult.IsMatched)
                            {
                                // Eсли строка не совпадает с паттерном строки, то текущая строка лога относится к предыдущему успешно спарсеному.
                                // Иначе строка относится к другому логу и завершается дополнение
                                if (CurrentSettings.IsMatch(line) == null)
                                {
                                    stackLines++;
                                    lastResult.AppendMessageAfter(Environment.NewLine + line);
                                    continue;
                                }
                                else
                                {
                                    stackLines = 0;
                                    lastResult = null;
                                }
                            }
                            else if (!lastResult.IsMatched)
                            {
                                // Если предыдущий фрагмент лога не спарсился удачано, то выполняются новые попытки спарсить лог
                                stackLines++;
                                var appendFailedLog = lastResult.Message + Environment.NewLine + line;
                                if (CurrentSettings.IsLineMatch(appendFailedLog, fileLog, out var afterSuccessResult))
                                {
                                    // Паттерн успешно сработал и тепмлейт заменяется. И дальше продолжается проврерка на дополнение строк
                                    listResult.Add(afterSuccessResult);
                                    lastResult = afterSuccessResult;
                                }
                                else
                                {
                                    // Паттерн не сработал успешно, но лог дополняется для следующей попытки
                                    lastResult.AppendMessageAfter(appendFailedLog);
                                }
                            }
                        }
                    }

                    if (!_isMatchSearchPatternFunc.Invoke(line))
                    {
                        beforeTraceLines.Enqueue(line);
                        if (beforeTraceLines.Count > CurrentSettings.MaxTraceLines)
                            beforeTraceLines.Dequeue();
                        continue;
                    }
                    else
                    {
                        ++Finded;
                        stackLines = 1;
                    }

                    if (CurrentSettings.IsLineMatch(line, fileLog, out lastResult))
                    {
                        listResult.Add(lastResult);
                    }
                    else
                    {
                        // Попытки спарсить текущую строку вместе с сохраненными предыдущими строками лога
                        var reverceBeforeTraceLines = new Queue<string>(beforeTraceLines.Reverse());
                        while (stackLines < CurrentSettings.MaxTraceLines && reverceBeforeTraceLines.Count > 0)
                        {
                            stackLines++;
                            lastResult.AppendMessageBefore(reverceBeforeTraceLines.Dequeue() + Environment.NewLine);

                            if (CurrentSettings.IsLineMatch(lastResult.Message, fileLog, out var beforeResult))
                            {
                                lastResult = beforeResult;
                                listResult.Add(lastResult);
                                break;
                            }
                        }
                    }

                    beforeTraceLines.Clear();
                }

                if (lastResult != null && !lastResult.IsMatched)
                {
                    listResult.Add(lastResult);
                }
            }

            return listResult;
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
                _message.Text = message.IsXml(out var xmlDoc) ? xmlDoc.PrintXml() : message;
                _fullTrace.Text = template.EntireMessage;
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
            OnSaveScheme?.Invoke(this, EventArgs.Empty);
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
            OnSaveScheme?.Invoke(this, EventArgs.Empty);
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
            OnSaveScheme?.Invoke(this, EventArgs.Empty);
        }

        private void logDirText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.LogsDirectory = logDirText.Text;
            logDirText.AssignValue(CurrentSettings.LogsDirectory, logDirText_TextChanged);
            ValidationCheck();
            OnSaveScheme?.Invoke(this, EventArgs.Empty);
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
            OnSaveScheme?.Invoke(this, EventArgs.Empty);
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
                ReportStatus(string.Empty, false);

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
            OverallResultList?.Clear();
            OverallResultList = null;

            ClearDGV();

            progressBar.Value = 0;
            _completedFilesStatus.Text = @"0";
            _totalFilesStatus.Text = @"0";

            Finded = 0;
            _findedInfo.Text = Finded.ToString();

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

        void ReportStatus(string message, bool isError)
        {
            _statusInfo.Text = message;
            _statusInfo.ForeColor = !isError ? Color.Black : Color.Red;
        }
    }

    public class MyTreeView : TreeView
    {
        /// <summary>
        /// Правит баг когда ячейка выбрана, но визуально не обновляется 
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            // Suppress WM_LBUTTONDBLCLK
            if (m.Msg == 0x203)
            {
                m.Result = IntPtr.Zero;
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}