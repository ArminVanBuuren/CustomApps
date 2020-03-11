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
            var lastTraceLines = new Stack<string>(10);
            var listResult = new List<DataTemplate>();
            using (var inputStream = File.OpenRead(fileLog.FilePath))
            {
                using (var inputReader = new StreamReader(inputStream))
                {
                    var isLastWasSuccess = false;
                    string line;
                    while ((line = inputReader.ReadLine()) != null && !_isStopPending)
                    {
                        if (isLastWasSuccess)
                        {
                            if (!_maskRegex.IsMatch(line))
                            {
                                listResult.Last().Message += Environment.NewLine + line;
                                continue;
                            }
                            else
                            {
                                isLastWasSuccess = false;
                            }
                        }
                        
                        if (line.IndexOf(txtPattern.Text, StringComparison.CurrentCultureIgnoreCase) == -1)
                        {
                            lastTraceLines.Push(line);
                            if (lastTraceLines.Count >= 10)
                                lastTraceLines.Pop();
                            continue;
                        }

                        if (!IsAppendMatched(line, fileLog, listResult))
                        {
                            var isMatched = false;
                            var builder = new StringBuilder();
                            var lastline = line;
                            while (lastTraceLines.Count > 0)
                            {
                                lastline = lastTraceLines.Pop() + Environment.NewLine + lastline;

                                if (IsAppendMatched(lastline, fileLog, listResult))
                                {
                                    isMatched = true;
                                    break;
                                }

                                if (!isMatched)
                                {

                                }
                            }
                        }

                        lastTraceLines.Clear();
                        isLastWasSuccess = true;
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

        bool IsAppendMatched(string line, FileLog fileLog, ICollection<DataTemplate> listResult)
        {
            var maskMatch = _maskRegex.Match(line);
            if (maskMatch.Success)
            {
                listResult.Add(new DataTemplate
                {
                    Server = fileLog.Server,
                    Date = maskMatch.Groups["Date"].Value,
                    FileName = fileLog.FileName,
                    TraceType = maskMatch.Groups["TraceType"].Value,
                    Message = maskMatch.Groups["Message"].Value
                });
                return true;
            }

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
