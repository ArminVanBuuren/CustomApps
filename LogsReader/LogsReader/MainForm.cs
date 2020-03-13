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
using Utils;
using Utils.WinForm.DataGridViewHelper;
using Exception = System.Exception;

namespace LogsReader
{
    public partial class MainForm : Form
    {
        private readonly object _syncRootFinded = new object();
        private int _finded = 0;
        private bool _isWorked = false;
        private bool _isStopPending = false;
        private Func<string, bool> _isMatch = null;

        private readonly LRSettings _allSettings;
        public LRSettingsScheme CurrentSettings { get; private set; }

        public int Finded
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

        private MTFuncResult<FileLog, List<DataTemplate>> _multiTasking;

        private readonly ToolStripStatusLabel _statusInfo = new ToolStripStatusLabel();
        private readonly ToolStripStatusLabel _findedInfo = new ToolStripStatusLabel();

        public MainForm()
        {
            InitializeComponent();

            statusStrip1.Items.Add(new ToolStripSeparator());
            statusStrip1.Items.Add(new ToolStripStatusLabel("Finded:"));
            statusStrip1.Items.Add(_findedInfo);
            statusStrip1.Items.Add(new ToolStripSeparator());
            statusStrip1.Items.Add(_statusInfo);
            

            dgvFiles.CellFormatting += DgvFiles_CellFormatting;

            FCTB.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);";
            FCTB.ClearStylesBuffer();
            FCTB.Range.ClearStyle(StyleIndex.All);
            FCTB.Language = Language.XML;
            FCTB.OnSyntaxHighlight(new TextChangedEventArgs(FCTB.Range));

            try
            {
                _allSettings = LRSettings.Deserialize();
                chooseScheme.DataSource = _allSettings.Schemes.Keys.ToList();
                txtPattern.AssignValue(_allSettings.PreviousSearch[0].Value, txtPattern_TextChanged);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Closing += (s, e) => { SaveSettings(); };
        }

        private void DgvFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
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
                    cell2.ToolTipText = "Not matched by TraceLinePattern";
                }
            }
        }

        private async void ButtonStartStop_Click(object sender, EventArgs e)
        {
            if (!_isWorked)
            {
                try
                {
                    if (useRegex.Checked)
                    {
                        if (!REGEX.Verify(txtPattern.Text))
                        {
                            SetStatus(@"Regular expression for search pattern is invalid! Please check.", true);
                            return;
                        }

                        var searchPattern = new Regex(txtPattern.Text, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        _isMatch = (input) => searchPattern.IsMatch(input);
                    }
                    else
                    {
                        _isMatch = (input) => input.IndexOf(txtPattern.Text, StringComparison.CurrentCultureIgnoreCase) != -1;
                    }

                    _isStopPending = false;
                    _isWorked = true;
                    ChangeFormStatus();
                    SetStatus(@"Working...", false);


                    var kvpList = await Task<List<FileLog>>.Factory.StartNew(() => GetFileLogs(
                        trvMain.Nodes["trvServers"].Nodes.Cast<TreeNode>().Where(x => x.Checked),
                        trvMain.Nodes["trvTypes"].Nodes.Cast<TreeNode>().Where(x => x.Checked)));

                    if (kvpList.Count <= 0)
                    {
                        SetStatus(@"No logs files found", true);
                        return;
                    }

                    _multiTasking = new MTFuncResult<FileLog, List<DataTemplate>>(ReadData, kvpList, CurrentSettings.MaxThreads <= 0 ? kvpList.Count : CurrentSettings.MaxThreads);
                    new Action(CheckProgress).BeginInvoke(IsCompleted, null);
                    await _multiTasking.StartAsync();

                    var i = 0;
                    var result = _multiTasking.Result.Values.SelectMany(x => x.Result).OrderBy(p => p.Date).ToList();
                    foreach (var v in result)
                        v.ID = ++i;

                    if (result.Count <= 0)
                    {
                        SetStatus(@"No logs found", true);
                        return;
                    }

                    await dgvFiles.AssignCollectionAsync(result, null, true);

                    SetStatus(@"Finished", false);
                }
                catch (Exception ex)
                {
                    SetStatus(ex.Message, true);
                }
                finally
                {
                    _isWorked = false;
                    ChangeFormStatus();
                }
            }
            else
            {
                _isStopPending = true;
                _multiTasking?.Stop();
                SetStatus(@"Stopping...", false);
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
            if (_isWorked)
            {
                ClearForm();
            }

            btnSearch.Text = _isWorked ? @"Stop" : @"Search";
            btnClear.Enabled = !_isWorked;
            trvMain.Enabled = !_isWorked;
            txtPattern.Enabled = !_isWorked;
            dgvFiles.Enabled = !_isWorked;
            FCTB.Enabled = !_isWorked;
            FCTBFullsStackTrace.Enabled = !_isWorked;
            useRegex.Enabled = !_isWorked;
            chooseScheme.Enabled = !_isWorked;
            serversText.Enabled = !_isWorked;
            typesText.Enabled = !_isWorked;
            maxThreadsText.Enabled = !_isWorked;
            logDirText.Enabled = !_isWorked;
            maxLinesStackText.Enabled = !_isWorked;
            tracePatternText.Enabled = !_isWorked;
        }

        void CheckProgress()
        {
            try
            {
                var total = _multiTasking.Source.Count().ToString();
                while (_isWorked && _multiTasking != null && !_multiTasking.IsCompleted)
                {
                    var completed = _multiTasking.Result.Values.Count().ToString();
                    var progress = _multiTasking.PercentOfComplete;

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

        void IsCompleted(IAsyncResult asyncResult)
        {
            try
            {
                Invoke(new MethodInvoker(delegate
                {
                    pgbThreads.Value = 100;
                    if (_multiTasking != null)
                    {
                        completedFilesStatus.Text = _multiTasking.Result.Values.Count().ToString();
                        totalFilesStatus.Text = _multiTasking.Source.Count().ToString();  
                    }
                    else
                    {
                        _findedInfo.Text = Finded.ToString();
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
                    while ((line = inputReader.ReadLine()) != null && !_isStopPending)
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

                        if (!_isMatch.Invoke(line))
                        {
                            beforeTraceLines.Enqueue(line);
                            if (beforeTraceLines.Count > CurrentSettings.MaxTraceLines)
                                beforeTraceLines.Dequeue();
                            continue;
                        }
                        else
                        {
                            Finded++;
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

        static bool GetCellItemSelectedRows(DataGridView grid, out List<string> result, string name = "Message")
        {
            result = new List<string>();
            if (grid.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in grid.SelectedRows)
                {
                    if (TryGetCellValue(row, name, out var cellValue))
                        result.Add(cellValue.ToString());
                }

                result = result.Distinct().ToList();
            }
            else
            {
                return false;
            }
            return result.Count > 0;
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
                    CurrentSettings.CatchWaring -= SetStatus;
                CurrentSettings = _allSettings.Schemes[chooseScheme.Text];
                CurrentSettings.CatchWaring += SetStatus;

                serversText.Text = CurrentSettings.Servers;
                typesText.Text = CurrentSettings.Types;
                maxThreadsText.AssignValue(CurrentSettings.MaxThreads, maxThreadsText_TextChanged);
                logDirText.AssignValue(CurrentSettings.LogsDirectory, logDirText_TextChanged);
                maxLinesStackText.AssignValue(CurrentSettings.MaxTraceLines, maxLinesStackText_TextChanged);
                tracePatternText.AssignValue(CurrentSettings.TraceLinePattern[0].Value, tracePatternText_TextChanged);
                btnSearch.Enabled = CurrentSettings.IsCorrect;
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
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
            {
                MaxThreadsTextSave(res);
            }
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
            {
                MaxLinesStackTextSave(res);
            }
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
            SaveSettings(); ;
        }

        private void tracePatternText_TextChanged(object sender, EventArgs e)
        {
            CurrentSettings.TraceLinePattern = new XmlNode[] { new XmlDocument().CreateCDataSection(tracePatternText.Text) };
            tracePatternText.AssignValue(CurrentSettings.TraceLinePattern[0].Value, tracePatternText_TextChanged);
            ValidationCheck();
            SaveSettings();
        }

        private void trvMain_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 0)
                CheckTreeViewNode(e.Node, e.Node.Checked);
            ValidationCheck();
        }

        private void CheckTreeViewNode(TreeNode node, bool isChecked)
        {
            foreach (TreeNode item in node.Nodes)
            {
                item.Checked = isChecked;

                if (item.Nodes.Count > 0)
                {
                    CheckTreeViewNode(item, isChecked);
                }
            }
        }

        void SetStatus(string message, bool isError)
        {
            _statusInfo.Text = message;
            _statusInfo.ForeColor = !isError ? Color.Black : Color.Red;
        }

        private void txtPattern_TextChanged(object sender, EventArgs e)
        {
            _allSettings.PreviousSearch[0].Value = txtPattern.Text;
            ValidationCheck();
            SaveSettings();
        }

        void ValidationCheck()
        {
            btnSearch.Enabled = CurrentSettings.IsCorrect && !txtPattern.Text.IsNullOrEmptyTrim() && trvMain.Nodes["trvServers"].Nodes.Cast<TreeNode>().Any(x => x.Checked) && trvMain.Nodes["trvTypes"].Nodes.Cast<TreeNode>().Any(x => x.Checked);
        }

        async void SaveSettings()
        {
            if (_allSettings != null)
                await LRSettings.SerializeAsync(_allSettings);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        void ClearForm()
        {
            dgvFiles.DataSource = null;
            dgvFiles.Refresh();
            FCTB.Text = "";
            FCTBFullsStackTrace.Text = "";
            pgbThreads.Value = 0;
            completedFilesStatus.Text = @"0";
            totalFilesStatus.Text = @"0";
            Finded = 0;
            _findedInfo.Text = Finded.ToString();
            SetStatus("", false);
        }
    }
}