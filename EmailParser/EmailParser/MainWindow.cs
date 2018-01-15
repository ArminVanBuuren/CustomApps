using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmailParser
{
    enum ProcessMode
    {
        Start = 0,
        Stop = 1
    }

    public partial class MainWindow : Form
    {
        OpenFileDialog _fileDialog;
        FolderBrowserDialog _folderDialog;
        List<EmailShell> _mailShells = new List<EmailShell>();

        /// <summary>
        /// true - процесс запущен; false - остановлен
        /// </summary>
        public bool StatusProcess { get; private set; } = false;

        public int Progress
        {
            get
            {
                if (StatusProcess)
                    return _mailShells.Aggregate(0, (current, file) => current + file.Progress) / _mailShells.Count;
                return 0;
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            _fileDialog = new OpenFileDialog {
                                                 Title = Properties.Resources.TITLE_TEXT,
                                                 Filter = Properties.Resources.FILE_FILTER,
                                                 InitialDirectory = @"C:\"
                                             };
            _folderDialog = new FolderBrowserDialog {
                                                        SelectedPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                                                    };

            progressBar.Maximum = 100;
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            try
            {
                Clean();
                if (radioButtonFolder.Checked)
                    OpenFolder();
                else
                    OpenFile();
            }
            catch (Exception ex)
            {
                AddStatusInfo(ex.Message, true);
            }
        }

        void OpenFolder()
        {
            _folderDialog.ShowDialog();
            if (!string.IsNullOrWhiteSpace(_folderDialog.SelectedPath))
            {
                textBoxPath.Text = _folderDialog.SelectedPath;
                GetFilesOnFolder();
                if (_mailShells.Count == 0)
                    AddStatusInfo(Properties.Resources.TEXT_ERR1, false);
            }
        }

        void OpenFile()
        {
            if (!string.IsNullOrEmpty(textBoxPath.Text))
            {
                if (File.Exists(textBoxPath.Text))
                    _fileDialog.InitialDirectory = Path.GetDirectoryName(textBoxPath.Text);
                else if (Directory.Exists(textBoxPath.Text))
                    _fileDialog.InitialDirectory = textBoxPath.Text;
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxPath.Text = _fileDialog.FileName;
            }
        }

        public void AddStatusInfo(string info, bool IsException)
        {
            if (string.IsNullOrEmpty(info))
                return;

            Invoke(new MethodInvoker(delegate
                                     {
                                         StatLable.ForeColor = IsException ? Color.Red : Color.DarkGreen;
                                         StatLable.Text = info;
                                     }));

        }





        private void RunButton_Click(object sender, EventArgs e)
        {
            if (StatusProcess)
                ProcessControl(ProcessMode.Stop);
            else
            {
                Clean();

                if (radioButtonFolder.Checked)
                {
                    GetFilesOnFolder();
                    if (_mailShells.Count == 0)
                    {
                        AddStatusInfo(Properties.Resources.TEXT_ERR1, true);
                        return;
                    }
                }
                else
                {
                    if (!File.Exists(textBoxPath.Text))
                    {
                        AddStatusInfo(Properties.Resources.TEXT_ERR2, true);
                        return;
                    }
                    _mailShells.Add(new EmailShell(textBoxPath.Text));
                }


                ProcessControl(ProcessMode.Start);

            }


        }

        void GetFilesOnFolder()
        {
            try
            {
                if (!Directory.Exists(textBoxPath.Text))
                    return;

                string[] files = Directory.GetFiles(textBoxPath.Text);
                foreach (string file in files)
                {
                    if (file.IndexOf(Properties.Resources.COMPLETED_TEXT, StringComparison.Ordinal) == file.Length - Properties.Resources.COMPLETED_TEXT.Length)
                        continue;
                    if (file.IndexOf(Properties.Resources.FAILED_TEXT, StringComparison.Ordinal) == file.Length - Properties.Resources.FAILED_TEXT.Length)
                        continue;

                    string[] splitted = file.Split('.');
                    if (splitted[splitted.Length - 1].Equals("txt", StringComparison.CurrentCultureIgnoreCase))
                        _mailShells.Add(new EmailShell(file));
                }
            }
            catch (Exception ex)
            {
                AddStatusInfo(ex.Message, true);
            }

        }





        IAsyncResult _myIasync;

        delegate string StartProcess();

        StartProcess _method;
        Thread _threadProgressBar;

        void ProcessControl(ProcessMode mode)
        {
            try
            {
                if (mode == ProcessMode.Start)
                {
                    Invoke(new MethodInvoker(delegate
                                             {
                                                 StatusProcess = true;
                                                 textBoxPath.Enabled = false;
                                                 OpenButton.Enabled = false;
                                                 radioButtonFolder.Enabled = false;
                                                 radioButtonFile.Enabled = false;
                                                 RunButton.Text = Properties.Resources.TEXT_STOP;
                                                 _threadProgressBar = new Thread(RefreshProgress);
                                                 _threadProgressBar.Start();
                                                 _method = StartPerforming;
                                                 _myIasync = _method.BeginInvoke(AsyncProcessCompleted, null);
                                             }));
                }
                else
                {
                    Invoke(new MethodInvoker(delegate
                                             {
                                                 StatusProcess = false;
                                                 textBoxPath.Enabled = true;
                                                 OpenButton.Enabled = true;
                                                 radioButtonFolder.Enabled = true;
                                                 radioButtonFile.Enabled = true;
                                                 RunButton.Text = Properties.Resources.TEXT_START;
                                                 AddStatusInfo(
                                                               string.Format("Успешные:{0} Нераспознанные:{1} Прочитано файлов:{2} ",
                                                                             _mailShells.Aggregate(0, (current, shell) => current + shell.StatValidMails),
                                                                             _mailShells.Aggregate(0, (current, shell) => current + shell.StatInvalidMails),
                                                                             _mailShells.Count(shell => shell.Progress > 0)),
                                                               false);
                                             }));

                }
            }
            catch (Exception ex)
            {
                AddStatusInfo(ex.Message, true);
            }
        }

        void AsyncProcessCompleted(IAsyncResult ares)
        {
            string callBackResultString = _method.EndInvoke(ares);
            if (!string.IsNullOrEmpty(callBackResultString))
                MessageBox.Show(callBackResultString);
            if (StatusProcess)
                ProcessControl(ProcessMode.Stop);
        }

        public string StartPerforming()
        {
            try
            {

                foreach (EmailShell file in _mailShells)
                {
                    if (!StatusProcess)
                        return null;

                    file.StartProcess(this);
                }

                return null;
            }
            catch (Exception ex)
            {
                return string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace);
            }
        }


        public void RefreshProgress()
        {
            while (progressBar.Value < 100)
            {
                try
                {
                    Thread.Sleep(5);
                    if (StatusProcess)
                        Invoke(new MethodInvoker(delegate
                                                 {
                                                     progressBar.Value = Progress;
                                                 }));
                    else
                    {
                        Invoke(new MethodInvoker(delegate
                                                 {
                                                     progressBar.Value = 100;
                                                 }));
                        break;
                    }
                }
                catch (ThreadAbortException ex)
                {
                    AddStatusInfo(ex.Message, true);
                }
                catch (Exception ex)
                {
                    AddStatusInfo(ex.Message, true);
                }
            }
        }


        void Clean()
        {
            progressBar.Value = 0;
            StatLable.Text = string.Empty;
            _mailShells.Clear();
        }

    }
}
