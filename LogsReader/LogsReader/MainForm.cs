using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LogsReader.Properties;
using Utils;
using Utils.WinForm.DataGridViewHelper;
using Exception = System.Exception;

namespace LogsReader
{
    public partial class MainForm : Form
    {
        private readonly object _syncRootFinded = new object();
        private int _finded = 0;
        
        private readonly ToolStripStatusLabel _statusInfo = new ToolStripStatusLabel();
        private readonly ToolStripStatusLabel _findedInfo = new ToolStripStatusLabel("0");

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

            statusStrip1.Items.Add(new ToolStripSeparator());
            statusStrip1.Items.Add(new ToolStripStatusLabel("Finded:"));
            statusStrip1.Items.Add(_findedInfo);
            statusStrip1.Items.Add(new ToolStripSeparator());
            statusStrip1.Items.Add(_statusInfo);

            var tooltipPrintXML = new ToolTip{ InitialDelay = 50 };
            tooltipPrintXML.SetToolTip(serversText, Resources.LRSettingsScheme_ServersComment);
            tooltipPrintXML.SetToolTip(typesText, Resources.LRSettingsScheme_TypesComment);
            tooltipPrintXML.SetToolTip(maxThreadsText, Resources.LRSettingsScheme_MaxThreadsComment);
            tooltipPrintXML.SetToolTip(logDirText, Resources.LRSettingsScheme_LogsDirectoryComment);
            tooltipPrintXML.SetToolTip(maxLinesStackText, Resources.LRSettingsScheme_MaxTraceLinesComment);
            tooltipPrintXML.SetToolTip(traceLinePatternText, Resources.LRSettingsScheme_TraceLinePatternComment);

            dgvFiles.CellFormatting += DgvFiles_CellFormatting;
            Closing += (s, e) => { SaveSettings(); };
            KeyPreview = true;
            KeyDown += MainForm_KeyDown;

            FCTB.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);";
            FCTB.ClearStylesBuffer();
            FCTB.Range.ClearStyle(StyleIndex.All);
            FCTB.Language = Language.XML;
            FCTB.OnSyntaxHighlight(new TextChangedEventArgs(FCTB.Range));

            FCTBFullsStackTrace.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);";
            FCTBFullsStackTrace.ClearStylesBuffer();
            FCTBFullsStackTrace.Range.ClearStyle(StyleIndex.All);
            FCTBFullsStackTrace.Language = Language.XML;
            FCTBFullsStackTrace.OnSyntaxHighlight(new TextChangedEventArgs(FCTB.Range));

            try
            {
                Settings = LRSettings.Deserialize();
                chooseScheme.DataSource = Settings.Schemes.Keys.ToList();
                txtPattern.AssignValue(Settings.PreviousSearch[0].Value, txtPattern_TextChanged);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.F5 when btnSearch.Enabled:
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

        private static void DgvFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var row = ((DataGridView)sender).Rows[e.RowIndex];
            TryGetCellValue(row, "IsMatched", out var cell);
            if (cell is bool cell0 && cell0)
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

        private async void ButtonStartStop_Click(object sender, EventArgs e)
        {
            if (!IsWorking)
            {
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

                    var result = MultiTaskingHandler.Result.Values.Where(x => x.Result != null).SelectMany(x => x.Result).OrderBy(p => p.Date).ThenBy(p => p.FileName).ToList();
                    var resultError = MultiTaskingHandler.Result.Values.Where(x => x.Error != null).Aggregate(new List<DataTemplate>(), (listErr, x) =>
                    {
                        listErr.Add(new DataTemplate()
                        {
                            Server = x.Source.Server,
                            Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff"),
                            FileName = x.Source.FileName,
                            TraceType = x.Error.GetType().ToString(),
                            Message = x.Error.ToString()
                        });
                        return listErr;
                    });
                    result.AddRange(resultError);

                    var i = 0;
                    foreach (var v in result)
                        v.ID = ++i;

                    if (result.Count <= 0)
                    {
                        ReportStatus(@"No logs found", true);
                        return;
                    }

                    await dgvFiles.AssignCollectionAsync(result, null, true);

                    ReportStatus(@"Finished", false);
                }
                catch (Exception ex)
                {
                    ReportStatus(ex.Message, true);
                }
                finally
                {
                    IsWorking = false;
                    ChangeFormStatus();
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
            var kvpList = new List<FileLog>();
            foreach (var serverNode in servers)
            {
                foreach (var typeNode in traceTypes)
                {
                    var files = Directory.GetFiles($@"\\{serverNode.Text}\{CurrentSettings.LogsDirectory}", $@"*{typeNode.Text}*", SearchOption.AllDirectories);
                    foreach (var filePath in files)
                    {
                        kvpList.Add(new FileLog(serverNode.Text, filePath));
                    }
                }
            }

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
            FCTB.Enabled = !IsWorking;
            FCTBFullsStackTrace.Enabled = !IsWorking;
            useRegex.Enabled = !IsWorking;
            chooseScheme.Enabled = !IsWorking;
            serversText.Enabled = !IsWorking;
            typesText.Enabled = !IsWorking;
            maxThreadsText.Enabled = !IsWorking;
            logDirText.Enabled = !IsWorking;
            maxLinesStackText.Enabled = !IsWorking;
            traceLinePatternText.Enabled = !IsWorking;
        }

        void CheckProgress()
        {
            try
            {
                var total = MultiTaskingHandler?.Source.Count().ToString();
                while (IsWorking && MultiTaskingHandler != null && !MultiTaskingHandler.IsCompleted)
                {
                    var completed = MultiTaskingHandler.Result.Values.Count().ToString();
                    var progress = MultiTaskingHandler.PercentOfComplete;

                    Invoke(new MethodInvoker(delegate
                    {
                        pgbThreads.Value = progress;
                        completedFilesStatus.Text = completed;
                        totalFilesStatus.Text = total;
                        _findedInfo.Text = Finded.ToString();
                    }));
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
                Invoke(new MethodInvoker(delegate
                {
                    pgbThreads.Value = 100;
                    _findedInfo.Text = Finded.ToString();
                    if (MultiTaskingHandler != null)
                    {
                        completedFilesStatus.Text = MultiTaskingHandler.Result.Values.Count().ToString();
                        totalFilesStatus.Text = MultiTaskingHandler.Source.Count().ToString();  
                    }
                    else
                    {
                        completedFilesStatus.Text = @"0";
                        totalFilesStatus.Text = @"0";
                    }
                }));
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        class FileLog
        {
            public FileLog(string server, string filePath)
            {
                Server = server;
                FilePath = filePath;
            }
            public string Server { get; }
            public string FileName => Path.GetFileName(FilePath);
            public string FilePath { get; }
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
                                    if (!CurrentSettings.TraceLinePatternRegex.IsMatch(line))
                                    {
                                        stackLines++;
                                        lastResult.Message = lastResult.Message + Environment.NewLine + line;
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
                                    if (IsLineMatched(appendFailedLog, fileLog, out var afterSuccessResult))
                                    {
                                        // Паттерн успешно сработал и тепмлейт заменяется. И дальше продолжается проврерка на дополнение строк
                                        listResult.Add(afterSuccessResult);
                                        lastResult = afterSuccessResult;
                                    }
                                    else
                                    {
                                        // Паттерн не сработал успешно, но лог дополняется для следующей попытки
                                        lastResult.Message = appendFailedLog;
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

                        if (IsLineMatched(line, fileLog, out lastResult))
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
                                lastResult.Message = reverceBeforeTraceLines.Dequeue() + Environment.NewLine + lastResult.Message;

                                if (IsLineMatched(lastResult.Message, fileLog, out var beforeResult))
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

        bool IsLineMatched(string message, FileLog fileLog, out DataTemplate result)
        {
            var maskMatch = CurrentSettings.TraceLinePatternRegex.Match(message);
            if (maskMatch.Success)
            {
                result = new DataTemplate
                {
                    IsMatched = true,
                    Server = fileLog.Server,
                    Date = maskMatch.Groups["Date"].Value,
                    FileName = fileLog.FileName,
                    TraceType = maskMatch.Groups["TraceType"].Value,
                    Message = maskMatch.Groups["Message"].Value,
                    EntireMessage = message
                };
                return true;
            }

            result = new DataTemplate
            {
                IsMatched = false,
                Server = fileLog.Server,
                FileName = fileLog.FileName,
                Message = message
            };
            return false;
        }

        private void dgvFiles_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvFiles.CurrentRow == null || dgvFiles.SelectedRows.Count == 0)
                return;

            if (TryGetCellValue(dgvFiles.SelectedRows[0], "Message", out var message))
            {
                FCTB.Text = message.ToString();
                FCTB.ClearUndo();
            }

            if (TryGetCellValue(dgvFiles.SelectedRows[0], "EntireMessage", out var entireMessage))
            {
                FCTBFullsStackTrace.Text = entireMessage.ToString();
                FCTBFullsStackTrace.ClearUndo();
            }
        }

        static bool TryGetCellValue(DataGridViewRow row, string cellName, out object result)
        {
            result = null;

            if (row == null)
                return false;

            foreach (DataGridViewCell cell in row.Cells)
            {
                if (!cell.OwningColumn.Name.Equals(cellName, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                result = cell.Value;
                return true;
            }

            return false;
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
                traceLinePatternText.AssignValue(CurrentSettings.TraceLinePattern[0].Value, tracePatternText_TextChanged);
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
            foreach (var s in CurrentSettings.Servers.Split(',').GroupBy(p => p, StringComparer.InvariantCultureIgnoreCase))
            {
                if(s.Key.IsNullOrEmptyTrim())
                    continue;
                trvMain.Nodes["trvServers"].Nodes.Add(s.Key.ToLower());
            }
            trvMain.ExpandAll();

            SaveSettings();
        }

        private void typesText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.Types = typesText.Text;
            typesText.AssignValue(CurrentSettings.Types, typesText_TextChanged);

            //заполняем список типов из параметра
            trvMain.Nodes["trvTypes"].Nodes.Clear();
            foreach (var s in CurrentSettings.Types.Split(',').GroupBy(p => p, StringComparer.InvariantCultureIgnoreCase))
            {
                if(s.Key.IsNullOrEmptyTrim())
                    continue;
                trvMain.Nodes["trvTypes"].Nodes.Add(s.Key.ToUpper());
            }
            trvMain.ExpandAll();

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

        private void tracePatternText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.TraceLinePattern = new XmlNode[] { new XmlDocument().CreateCDataSection(traceLinePatternText.Text) };
            traceLinePatternText.AssignValue(CurrentSettings.TraceLinePattern[0].Value, tracePatternText_TextChanged);
            ValidationCheck();
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

        void ValidationCheck()
        {
            btnSearch.Enabled = CurrentSettings.IsCorrect && !txtPattern.Text.IsNullOrEmptyTrim() && trvMain.Nodes["trvServers"].Nodes.Cast<TreeNode>().Any(x => x.Checked) && trvMain.Nodes["trvTypes"].Nodes.Cast<TreeNode>().Any(x => x.Checked);
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
            FCTB.Text = string.Empty;
            FCTBFullsStackTrace.Text = string.Empty;
            pgbThreads.Value = 0;
            completedFilesStatus.Text = @"0";
            totalFilesStatus.Text = @"0";
            Finded = 0;
            _findedInfo.Text = Finded.ToString();
            ReportStatus(string.Empty, false);
        }
    }
}