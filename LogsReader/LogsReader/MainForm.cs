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
    public sealed partial class MainForm : Form
    {
        private readonly object _syncRootFinded = new object();
        private int _finded = 0;
        private bool _isClosed = false;

        private readonly ToolStripStatusLabel _statusInfo;
        private readonly ToolStripStatusLabel _findedInfo;
        private readonly ToolStripStatusLabel _completedFilesStatus;
        private readonly ToolStripStatusLabel _totalFilesStatus;
        private readonly ToolStripStatusLabel _cpuUsage;
        private readonly ToolStripStatusLabel _threadsUsage;
        private readonly ToolStripStatusLabel _ramUsage;
        private readonly Editor _message;
        private readonly Editor _fullTrace;

        /// <summary>
        /// Статус выполнения поиска
        /// </summary>
        private bool IsWorking { get; set; } = false;

        /// <summary>
        /// Запрос на ожидание остановки выполнения поиска
        /// </summary>
        private bool IsStopPending { get; set; } = false;

        /// <summary>
        /// Функция на валидацию по критерию поиска в строке лога
        /// </summary>
        private Func<string, bool> IsMatchSearchPatternFunc { get; set; } = null;

        /// <summary>
        /// Все настройки
        /// </summary>
        private LRSettings Settings { get; set; }

        /// <summary>
        /// Текущая схема настроек
        /// </summary>
        private LRSettingsScheme CurrentSettings { get; set; }

        /// <summary>
        /// Основной обработчик для работы в асинхронном режиме
        /// </summary>
        private MTFuncResult<FileLog, List<DataTemplate>> MultiTaskingHandler { get; set; }

        private DataTemplateCollection ResultList { get; set; }

        /// <summary>
        /// Количество совпадений по критериям поиска
        /// </summary>
        private int Finded
        {
            get
            {
                lock (_syncRootFinded)
                    return _finded;
            }
            set
            {
                lock (_syncRootFinded)
                    _finded = value;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            try
            {
                #region Set StatusStrip

                var statusStripItemsPaddingStart = new Padding(0, 3, 0, 2);
                var statusStripItemsPaddingMiddle = new Padding(-3, 3, 0, 2);

                statusStrip.Items.Add(new ToolStripStatusLabel("CPU:") {Font = this.Font, Margin = new Padding(7, 3, 0, 2)});
                _cpuUsage = new ToolStripStatusLabel("    ") {Font = this.Font, Margin = new Padding(-7, 3, 0, 2)};
                statusStrip.Items.Add(_cpuUsage);

                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(new ToolStripStatusLabel("Threads:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                _threadsUsage = new ToolStripStatusLabel("  ") {Font = this.Font, Margin = statusStripItemsPaddingMiddle};
                statusStrip.Items.Add(_threadsUsage);

                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(new ToolStripStatusLabel("RAM:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                _ramUsage = new ToolStripStatusLabel("       ") {Font = this.Font, Margin = new Padding(-4, 3, 0, 2)};
                statusStrip.Items.Add(_ramUsage);

                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(new ToolStripStatusLabel("Files completed:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                _completedFilesStatus = new ToolStripStatusLabel("0") {Font = this.Font, Margin = statusStripItemsPaddingMiddle};
                statusStrip.Items.Add(_completedFilesStatus);
                statusStrip.Items.Add(new ToolStripStatusLabel("of") {Font = this.Font, Margin = statusStripItemsPaddingMiddle});
                _totalFilesStatus = new ToolStripStatusLabel("0") {Font = this.Font, Margin = statusStripItemsPaddingMiddle};
                statusStrip.Items.Add(_totalFilesStatus);

                statusStrip.Items.Add(new ToolStripSeparator());
                statusStrip.Items.Add(new ToolStripStatusLabel("Found") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                _findedInfo = new ToolStripStatusLabel("0") {Font = this.Font, Margin = statusStripItemsPaddingMiddle};
                statusStrip.Items.Add(_findedInfo);
                statusStrip.Items.Add(new ToolStripStatusLabel("matches") {Font = this.Font, Margin = statusStripItemsPaddingMiddle});

                statusStrip.Items.Add(new ToolStripSeparator());
                _statusInfo = new ToolStripStatusLabel("") {Font = this.Font, Margin = statusStripItemsPaddingStart};
                statusStrip.Items.Add(_statusInfo);

                var thread = new Thread(CalculateLocalResources) {IsBackground = true, Priority = ThreadPriority.Lowest};
                thread.Start();

                #endregion

                var tooltipPrintXML = new ToolTip {InitialDelay = 50};
                tooltipPrintXML.SetToolTip(useRegex, Resources.LRSettings_UseRegexComment);
                tooltipPrintXML.SetToolTip(serversText, Resources.LRSettingsScheme_ServersComment);
                tooltipPrintXML.SetToolTip(typesText, Resources.LRSettingsScheme_TypesComment);
                tooltipPrintXML.SetToolTip(maxThreadsText, Resources.LRSettingsScheme_MaxThreadsComment);
                tooltipPrintXML.SetToolTip(logDirText, Resources.LRSettingsScheme_LogsDirectoryComment);
                tooltipPrintXML.SetToolTip(maxLinesStackText, Resources.LRSettingsScheme_MaxTraceLinesComment);

                dgvFiles.CellFormatting += DgvFiles_CellFormatting;
                Closing += (s, e) =>
                {
                    _isClosed = true;
                    SaveSettings();
                };
                KeyPreview = true;
                KeyDown += MainForm_KeyDown;

                var notepad = new NotepadControl();
                splitContainer2.Panel2.Controls.Add(notepad);
                _message = notepad.AddDocument("Message", string.Empty, Language.XML);
                _fullTrace = notepad.AddDocument("Full Trace", string.Empty);
                notepad.TabsFont = this.Font;
                notepad.TextFont = new Font("Segoe UI", 10F);
                notepad.Dock = DockStyle.Fill;
                notepad.SelectEditor(0);
                notepad.ReadOnly = true;

                var oldDateStartChecked = false;
                var oldDateEndChecked = false;
                dateTimePickerStart.ValueChanged += (sender, args) =>
                {
                    if (oldDateStartChecked || !dateTimePickerStart.Checked)
                        return;
                    var today = DateTime.Now;
                    oldDateStartChecked = true;
                    dateTimePickerStart.Value = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
                };
                dateTimePickerEnd.ValueChanged += (sender, args) =>
                {
                    if (oldDateEndChecked || !dateTimePickerEnd.Checked)
                        return;
                    var today = DateTime.Now;
                    oldDateEndChecked = true;
                    dateTimePickerEnd.Value = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);
                };

                Settings = LRSettings.Deserialize();
                chooseScheme.DataSource = Settings.Schemes.Keys.ToList();
                txtPattern.AssignValue(Settings.PreviousSearch[0].Value, txtPattern_TextChanged);
                useRegex.Checked = Settings.UseRegex;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), @"Initialization", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Мониторинг локальных ресурсов
        /// </summary>
        void CalculateLocalResources()
        {
            try
            {
                Action checkAppCPU = null;
                var curProcName = SERVER.ObtainCurrentProcessName();
                if (curProcName != null)
                {
                    var appCPU = new PerformanceCounter("Process", "% Processor Time", curProcName, true);
                    appCPU.NextValue();
                    checkAppCPU = () =>
                    {
                        double.TryParse(appCPU.NextValue().ToString(), out var cpuResult);
                        _cpuUsage.Text = $"{(int) (cpuResult / Environment.ProcessorCount),3}%";
                    };
                }

                while (!_isClosed)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke(new MethodInvoker(delegate
                        {
                            checkAppCPU?.Invoke();
                            var currentProcess = Process.GetCurrentProcess();
                            _threadsUsage.Text = $"{currentProcess.Threads.Count,-2}";
                            _ramUsage.Text = $"{currentProcess.PrivateMemorySize64.ToFileSize(),-5}";
                        }));
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), @"System Resource Monitoring", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
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
                        IsMatchSearchPatternFunc = (input) => searchPattern.IsMatch(input);
                    }
                    else
                    {
                        IsMatchSearchPatternFunc = (input) => input.IndexOf(txtPattern.Text, StringComparison.OrdinalIgnoreCase) != -1;
                    }

                    if (dateTimePickerStart.Checked && dateTimePickerEnd.Checked && dateTimePickerStart.Value > dateTimePickerEnd.Value)
                    {
                        ReportStatus(@"Date of end must be greater than date of start.", true);
                        return;
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

                    // ThreadPriority.Lowest - необходим чтобы не залипал основной поток и не мешал другим процессам
                    MultiTaskingHandler = new MTFuncResult<FileLog, List<DataTemplate>>(ReadData, kvpList, CurrentSettings.MaxThreads <= 0 ? kvpList.Count : CurrentSettings.MaxThreads, ThreadPriority.Lowest);
                    new Action(CheckProgress).BeginInvoke(ProcessCompleted, null);
                    await MultiTaskingHandler.StartAsync();

                    var resultOfSuccess = MultiTaskingHandler.Result.CallBackList.Where(x => x.Result != null).SelectMany(x => x.Result);
                    if (dateTimePickerStart.Checked && dateTimePickerEnd.Checked)
                        resultOfSuccess = resultOfSuccess.Where(x => x.DateOfTrace != null && x.DateOfTrace.Value >= dateTimePickerStart.Value && x.DateOfTrace.Value <= dateTimePickerEnd.Value);
                    else if (dateTimePickerStart.Checked)
                        resultOfSuccess = resultOfSuccess.Where(x => x.DateOfTrace != null && x.DateOfTrace.Value >= dateTimePickerStart.Value);
                    else if (dateTimePickerEnd.Checked)
                        resultOfSuccess = resultOfSuccess.Where(x => x.DateOfTrace != null && x.DateOfTrace.Value <= dateTimePickerEnd.Value);

                    ResultList = new DataTemplateCollection(resultOfSuccess);
                    var resultOfError = MultiTaskingHandler.Result.CallBackList.Where(x => x.Error != null).Aggregate(new List<DataTemplate>(), (listErr, x) =>
                    {
                        listErr.Add(new DataTemplate(x.Source, x.Error));
                        return listErr;
                    });
                    ResultList.AddRange(resultOfError);

                    if (ResultList.Count <= 0)
                    {
                        ReportStatus(@"No logs found", true);
                        return;
                    }

                    await dgvFiles.AssignCollectionAsync(ResultList, null, true);

                    stop.Stop();
                    ReportStatus($"Finished in {stop.Elapsed.ToReadableString()}", false);
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
                }
            }
            else
            {
                IsStopPending = true;
                MultiTaskingHandler?.Stop();
                ReportStatus(@"Stopping...", false);
            }
        }

        List<FileLog> GetFileLogs(IEnumerable<TreeNode> servers, IEnumerable<TreeNode> traceTypes)
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
                    if(traceTypes.Any(x => fileLog.FileName.IndexOf(x.Text, StringComparison.CurrentCultureIgnoreCase) != -1))
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
                ClearForm();

            btnSearch.Text = IsWorking ? @"Stop" : @"Search [F5]";
            btnClear.Enabled = !IsWorking;
            trvMain.Enabled = !IsWorking;
            txtPattern.Enabled = !IsWorking;
            dgvFiles.Enabled = !IsWorking;
            _message.Enabled = !IsWorking;
            _fullTrace.Enabled = !IsWorking;
            useRegex.Enabled = !IsWorking;
            chooseScheme.Enabled = !IsWorking;
            serversText.Enabled = !IsWorking;
            typesText.Enabled = !IsWorking;
            maxThreadsText.Enabled = !IsWorking;
            logDirText.Enabled = !IsWorking;
            maxLinesStackText.Enabled = !IsWorking;
        }

        void CheckProgress()
        {
            try
            {
                var total = MultiTaskingHandler?.Source.Count.ToString();
                while (IsWorking && MultiTaskingHandler != null && !MultiTaskingHandler.IsCompleted)
                {
                    var completed = MultiTaskingHandler.Result.Count.ToString();
                    var progress = MultiTaskingHandler.PercentOfComplete;

                    if (InvokeRequired)
                    {
                        Invoke(new MethodInvoker(delegate
                        {
                            pgbThreads.Value = progress;
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
                        pgbThreads.Value = 100;
                        _findedInfo.Text = Finded.ToString();
                        if (MultiTaskingHandler != null)
                        {
                            _completedFilesStatus.Text = MultiTaskingHandler.Result.Count.ToString();
                            _totalFilesStatus.Text = MultiTaskingHandler.Source.Count().ToString();
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
            using (var inputStream = File.OpenRead(fileLog.FilePath))
            {
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

                        if (!IsMatchSearchPatternFunc.Invoke(line))
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
            }

            return listResult;
        }

        private void DgvFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                var row = ((DataGridView)sender).Rows[e.RowIndex];
                var tempalteID = TryGetPrivateID(row);
                if (tempalteID == -1 || ResultList == null)
                    return;

                var template = ResultList[tempalteID];

                if (template.IsMatched)
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.LightPink;
                    foreach (DataGridViewCell cell2 in row.Cells)
                    {
                        cell2.ToolTipText = "Doesn't match regex pattern for trace line";
                    }
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

                if (tempalteID == -1 || ResultList == null)
                {
                    _message.Text = string.Empty;
                    _fullTrace.Text = string.Empty;
                    return;
                }

                var template = ResultList[tempalteID];
                _message.Text = XML.RemoveUnallowable(template.Message);
                _message.PrintXml();
                _fullTrace.Text = template.EntireMessage;
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, true);
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

        private void chooseScheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (CurrentSettings != null)
                    CurrentSettings.CatchWaring -= ReportStatus;
                CurrentSettings = Settings.Schemes[chooseScheme.Text];
                CurrentSettings.CatchWaring += ReportStatus;

                serversText.Text = CurrentSettings.Servers;
                typesText.Text = CurrentSettings.Types;
                maxThreadsText.AssignValue(CurrentSettings.MaxThreads, maxThreadsText_TextChanged);
                logDirText.AssignValue(CurrentSettings.LogsDirectory, logDirText_TextChanged);
                maxLinesStackText.AssignValue(CurrentSettings.MaxTraceLines, maxLinesStackText_TextChanged);
                btnSearch.Enabled = CurrentSettings.IsCorrect;
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message, true);
            }
            finally
            {
                ValidationCheck();
            }
        }

        private void serversText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.Servers = serversText.Text;
            serversText.AssignValue(CurrentSettings.Servers, serversText_TextChanged);
            
            //заполняем список серверов из параметра
            trvMain.Nodes["trvServers"].Nodes.Clear();
            foreach (var s in CurrentSettings.Servers.Split(',').GroupBy(p => p.Trim(), StringComparer.InvariantCultureIgnoreCase).OrderBy(p => p.Key))
            {
                if(s.Key.IsNullOrEmptyTrim())
                    continue;
                trvMain.Nodes["trvServers"].Nodes.Add(s.Key.Trim().ToUpper());
            }
            trvMain.ExpandAll();

            ValidationCheck();
            SaveSettings();
        }

        private void typesText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.Types = typesText.Text;
            typesText.AssignValue(CurrentSettings.Types, typesText_TextChanged);

            //заполняем список типов из параметра
            trvMain.Nodes["trvTypes"].Nodes.Clear();
            foreach (var s in CurrentSettings.Types.Split(',').GroupBy(p => p.Trim(), StringComparer.InvariantCultureIgnoreCase).OrderBy(p => p.Key))
            {
                if(s.Key.IsNullOrEmptyTrim())
                    continue;
                trvMain.Nodes["trvTypes"].Nodes.Add(s.Key.Trim().ToUpper());
            }
            trvMain.ExpandAll();

            ValidationCheck();
            SaveSettings();
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
            SaveSettings();
        }

        private void logDirText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.LogsDirectory = logDirText.Text;
            logDirText.AssignValue(CurrentSettings.LogsDirectory, logDirText_TextChanged);
            ValidationCheck();
            SaveSettings();
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
            SaveSettings();
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

        void ReportStatus(string message, bool isError)
        {
            _statusInfo.Text = message;
            _statusInfo.ForeColor = !isError ? Color.Black : Color.Red;
        }

        private void txtPattern_TextChanged(object sender, EventArgs e)
        {
            Settings.PreviousSearch[0].Value = txtPattern.Text;
            ValidationCheck();
            SaveSettings();
        }

        private void useRegex_CheckedChanged(object sender, EventArgs e)
        {
            Settings.UseRegex = useRegex.Checked;
            SaveSettings();
        }

        void ValidationCheck()
        {
            var isCorrectPath = IO.CHECK_PATH.IsMatch(logDirText.Text);
            logDirText.BackColor = isCorrectPath ? SystemColors.Window : Color.LightPink;

            var settIsCorrect = CurrentSettings.IsCorrect;
            if(settIsCorrect)
                ReportStatus(string.Empty, false);

            btnSearch.Enabled = isCorrectPath
                                && settIsCorrect
                                && !txtPattern.Text.IsNullOrEmptyTrim() 
                                && trvMain.Nodes["trvServers"].Nodes.Cast<TreeNode>().Any(x => x.Checked) 
                                && trvMain.Nodes["trvTypes"].Nodes.Cast<TreeNode>().Any(x => x.Checked);
        }

        async void SaveSettings()
        {
            if (Settings != null)
                await LRSettings.SerializeAsync(Settings);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        void ClearForm()
        {
            dgvFiles.DataSource = null;
            dgvFiles.Refresh();
            ResultList?.Clear();
            ResultList = null;
            _message.Text = string.Empty;
            _fullTrace.Text = string.Empty;
            pgbThreads.Value = 0;
            _completedFilesStatus.Text = @"0";
            _totalFilesStatus.Text = @"0";
            Finded = 0;
            _findedInfo.Text = Finded.ToString();
            ReportStatus(string.Empty, false);
        }
    }
}