using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using LogsReader.Properties;
using Utils;


namespace LogsReader
{
    public partial class MainForm : Form
    {
        private bool _isWorked = false;
        private bool _isStopPending = false;
        private Func<string, bool> _isMatch = null;
        Regex _maskRegex = null;
        public LogsReaderSettings Settings { get; private set; }
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
                Settings = LogsReaderSettings.Load();

                //заполняем список серверов из параметра
                foreach (var s in Settings.Servers.Split(',').GroupBy(p => p))
                {
                    trvMain.Nodes["trvServers"].Nodes.Add(s.Key.ToLower());
                }

                //заполняем список типов из параметра
                foreach (var s in Settings.Types.Split(','))
                {
                    trvMain.Nodes["trvTypes"].Nodes.Add(s.ToUpper());
                }

                _maskRegex = new Regex(Settings.TraceLinePattern[0].Value, RegexOptions.Compiled | RegexOptions.Singleline);
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
                        StatusTextLable.Text = @"The value for search pattern cannot be empty!";
                        return;
                    }

                    _isStopPending = false;
                    _isWorked = true;
                    ChangeFormStatus();

                    var kvpList = new List<FileLog>();
                    foreach (TreeNode serverNode in trvMain.Nodes["trvServers"].Nodes)
                    {
                        if (!serverNode.Checked)
                            continue;

                        foreach (TreeNode typeNode in trvMain.Nodes["trvTypes"].Nodes)
                        {
                            if (!typeNode.Checked)
                                continue;

                            var files = Directory.GetFiles($@"\\{serverNode.Text}\{Settings.LogsDirectory}", $@"*{typeNode.Text}*", SearchOption.AllDirectories);
                            foreach (var filePath in files)
                            {
                                kvpList.Add(new FileLog(serverNode.Text, filePath));
                            }
                        }
                    }

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
                StatusTextLable.Text = @"Stopping...";
            }
        }

        void ChangeFormStatus()
        {
            StatusTextLable.Text = _isWorked ? @"Working..." : @"Finished";
            btnSearch.Text = _isWorked ? @"Stop" : @"Search";
            btnClear.Enabled = !_isWorked;
            trvMain.Enabled = !_isWorked;
            txtPattern.Enabled = !_isWorked;
            dgvFiles.Enabled = !_isWorked;
            txtText.Enabled = !_isWorked;
            useRegex.Enabled = !_isWorked;
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

                            if (lastResult != null)
                            {
                                if (lastResult.IsMatched && !IsLineMatched(line, fileLog, out _))
                                {
                                    stackLines++;
                                    lastResult.Message = lastResult.Message + Environment.NewLine + line;
                                    continue;
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

            //var AllLines = File.ReadAllLines(path);
            //var results = Array.FindAll(AllLines, s => s.Contains(txtPattern.Text));

            //foreach (var str in results)
            //{
            //    var maskMatch = _maskRegex.Match(str);

            //    dgvFiles.Invoke(new Action(delegate ()
            //    {
            //        dgvFiles.Rows.Add(server, maskMatch.Groups[1].Value, Path.GetFileName(path), maskMatch.Groups[2].Value, maskMatch.Groups[3].Value);
            //    }));

            //}

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

        private void useRegex_CheckedChanged(object sender, EventArgs e)
        {
            if (useRegex.Checked)
            {
                var searchPattern = new Regex(txtPattern.Text, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                _isMatch = (input) => searchPattern.IsMatch(input);
            }
            else
            {
                _isMatch = (input) => input.IndexOf(txtPattern.Text, StringComparison.CurrentCultureIgnoreCase) != -1;
            }
        }
    }
}
