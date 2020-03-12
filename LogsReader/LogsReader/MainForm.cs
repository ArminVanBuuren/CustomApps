using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace LogsReader
{
    public partial class MainForm : Form
    {
        private bool _isWorked = false;
        private bool _isStopPending = false;
        private Func<string, bool> _isMatch = null;
        Regex _maskRegex = null;
        public LRSettingsScheme CurrentSettings { get; private set; }
        private MTFuncResult<FileLog, List<DataTemplate>> _multiTasking;

        public MainForm()
        {
            InitializeComponent();
            ReadAllSettings();
        }

        void ReadAllSettings()
        {
            try
            {
                CurrentSettings = LRSettings.Deserialize();

                //заполняем список серверов из параметра
                foreach (var s in CurrentSettings.Servers.Split(',').GroupBy(p => p))
                {
                    trvMain.Nodes["trvServers"].Nodes.Add(s.Key.ToLower());
                }

                //заполняем список типов из параметра
                foreach (var s in CurrentSettings.Types.Split(','))
                {
                    trvMain.Nodes["trvTypes"].Nodes.Add(s.ToUpper());
                }

                _maskRegex = new Regex(CurrentSettings.TraceLinePattern[0].Value, RegexOptions.Compiled | RegexOptions.Singleline);

                var groups = _maskRegex.GetGroupNames();
                if (groups.All(x => x != "Date"))
                    MessageBox.Show("Not found group '?<Date>' in TraceLinePattern");
                if(groups.All(x => x != "TraceType"))
                    MessageBox.Show("Not found group '?<TraceType>' in TraceLinePattern");
                if (groups.All(x => x != "Message"))
                    MessageBox.Show("Not found group '?<Message>' in TraceLinePattern");
            }
            catch (ConfigurationException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void ButtonStartStop_Click(object sender, EventArgs e)
        {
            if (!_isWorked)
            {
                try
                {
                    if (txtPattern.Text.IsNullOrEmptyTrim())
                    {
                        SetStatus(@"The value for search pattern cannot be empty!", true);
                        return;
                    }

                    if (useRegex.Checked)
                    {
                        if (!VerifyRegEx(txtPattern.Text))
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

                    var kvpList = await Task<List<FileLog>>.Factory.StartNew(() => GetFileLogs(
                            trvMain.Nodes["trvServers"].Nodes.Cast<TreeNode>().Where(x => x.Checked),
                            trvMain.Nodes["trvTypes"].Nodes.Cast<TreeNode>().Where(x => x.Checked)));

                    _multiTasking = new MTFuncResult<FileLog, List<DataTemplate>>(ReadData, kvpList, kvpList.Count);
                    new Action(CheckProgress).BeginInvoke(IsCompleted, null);
                    await _multiTasking.StartAsync();

                    _isWorked = false;
                    ChangeFormStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                _isStopPending = true;
                _multiTasking?.Stop();
                SetStatus(@"Stopping...");
            }
        }

        List<FileLog> GetFileLogs(IEnumerable<TreeNode> servers, IEnumerable<TreeNode> traceTypes)
        {
            var kvpList = new List<FileLog>();
            foreach (TreeNode serverNode in servers)
            {
                foreach (TreeNode typeNode in traceTypes)
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
            SetStatus(_isWorked ? @"Working..." : @"Finished");
            btnSearch.Text = _isWorked ? @"Stop" : @"Search";
            btnClear.Enabled = !_isWorked;
            trvMain.Enabled = !_isWorked;
            txtPattern.Enabled = !_isWorked;
            dgvFiles.Enabled = !_isWorked;
            txtText.Enabled = !_isWorked;
            useRegex.Enabled = !_isWorked;
        }

        void SetStatus(string status, bool isError = false)
        {
            StatusTextLable.Text = status;
            StatusTextLable.ForeColor = !isError ? Color.Black : Color.Red;
        }

        public static bool VerifyRegEx(string testPattern)
        {
            if ((testPattern != null) && (testPattern.Trim().Length > 0))
            {
                try
                {
                    Regex.Match("", testPattern);
                    return true;
                }
                catch (ArgumentException)
                {
                    return false; // BAD PATTERN: Syntax error
                }
            }
            else
            {
                return false; //BAD PATTERN: Pattern is null or blank
            }
        }

        void CheckProgress()
        {
            while (_isWorked && _multiTasking != null && !_multiTasking.IsCompleted)
            {
                var progress = _multiTasking.PercentOfComplete;

                Invoke(new MethodInvoker(delegate
                {
                    pgbThreads.Value = progress;
                }));
                Thread.Sleep(10);
            }
        }

        void IsCompleted(IAsyncResult asyncResult)
        {
            Invoke(new MethodInvoker(delegate
            {
                pgbThreads.Value = 100;
            }));
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
            const int maxLinesStack = 10;
            var beforeTraceLines = new Stack<string>(maxLinesStack);
            var listResult = new List<DataTemplate>();
            using (var inputStream = File.OpenRead(fileLog.FilePath))
            {
                using (var inputReader = new StreamReader(inputStream))
                {
                    int stackLines = 0;
                    string line;
                    DataTemplate lastResult = null;
                    while ((line = inputReader.ReadLine()) != null && !_isStopPending)
                    {
                        if (lastResult != null)
                        {
                            if (stackLines <= maxLinesStack)
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
                                    if (!IsLineMatched(line, fileLog, out _))
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
                                else if (!lastResult.IsMatched && IsLineMatched(lastResult.Message + Environment.NewLine + line, fileLog, out var afterResult))
                                {
                                    listResult.Add(afterResult);
                                    lastResult = afterResult;
                                }
                            }
                        }

                        if (!_isMatch.Invoke(line))
                        {
                            beforeTraceLines.Push(line);
                            if (beforeTraceLines.Count >= maxLinesStack)
                                beforeTraceLines.Pop();
                            continue;
                        }
                        else
                        {
                            stackLines = 0;
                        }

                        if (IsLineMatched(line, fileLog, out lastResult))
                        {
                            listResult.Add(lastResult);
                        }
                        else
                        {
                            lastResult.Message = line;
                            while (stackLines <= maxLinesStack && beforeTraceLines.Count > 0)
                            {
                                stackLines++;
                                lastResult.Message = beforeTraceLines.Pop() + Environment.NewLine + lastResult.Message;

                                if (IsLineMatched(lastResult.Message, fileLog, out var beforeResult))
                                {
                                    lastResult = beforeResult;
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

        bool IsLineMatched(string line, FileLog fileLog, out DataTemplate result)
        {
            var maskMatch = _maskRegex.Match(line);
            if (maskMatch.Success)
            {
                result = new DataTemplate
                {
                    IsMatched = true,
                    Server = fileLog.Server,
                    Date = maskMatch.Groups["Date"].Value,
                    FileName = fileLog.FileName,
                    TraceType = maskMatch.Groups["TraceType"].Value,
                    Message = maskMatch.Groups["Message"].Value
                };
                return true;
            }

            result = new DataTemplate
            {
                IsMatched = false,
                Server = fileLog.Server,
                FileName = fileLog.FileName
            };
            return false;
        }

        private void trvMain_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                if (trvMain.Nodes["trvServers"].Checked)
                {
                    foreach (TreeNode tn in trvMain.Nodes["trvServers"].Nodes)
                    {
                        tn.Checked = true;
                    }
                }
                else
                {
                    foreach (TreeNode tn in trvMain.Nodes["trvServers"].Nodes)
                    {
                        tn.Checked = false;
                    }
                }
                if (trvMain.Nodes["trvTypes"].Checked)
                {
                    foreach (TreeNode tn in trvMain.Nodes["trvTypes"].Nodes)
                    {
                        tn.Checked = true;
                    }
                }
                else
                {
                    foreach (TreeNode tn in trvMain.Nodes["trvTypes"].Nodes)
                    {
                        tn.Checked = false;
                    }
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            dgvFiles.Rows.Clear();
            dgvFiles.Refresh();
            txtText.Text = "";
            StatusTextLable.Text = "";
            pgbThreads.Value = 0;
        }

        private void dgvFiles_SelectionChanged(object sender, EventArgs e)
        {
            txtText.Text = dgvFiles.CurrentRow.Cells[4].Value.ToString();
        }
    }
}
