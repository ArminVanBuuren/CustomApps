using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Script.Control;
using XPackage;

namespace Script
{
    public partial class MainWindow : Form
    {
        private string configPath;
        public static string LocalPath => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        ScriptTemplate st = null;
        public bool InProgress { get; private set; }
        AbortableBackgroundWorker asyncPerforming = new AbortableBackgroundWorker();

        enum ProcessStatus
        {
            None = 0,
            Processing = 1,
            Completed = 2,
            Aborted = 3,
            Exception = 4
        }

        public MainWindow()
        {
            InitializeComponent();
            asyncPerforming.DoWork += AsyncPerforming_DoWork;
            asyncPerforming.RunWorkerCompleted += AsyncPerforming_RunWorkerCompleted;

            try
            {
                configPath = Path.Combine(LocalPath, "Script.Config.sxml");
                if (!File.Exists(configPath))
                {
                    using (StreamWriter writer = new StreamWriter(configPath, false, Functions.Enc))
                    {
                        //writer.Write(Properties.Resources.Script_Config);
                        writer.Write(ScriptTemplate.GetExampleOfConfig());
                        writer.Close();
                    }
                }
                SXML_Config.Text = configPath.LoadFileByPath();
            }
            catch (Exception ex)
            {
                ChangeStatusBar(ProcessStatus.Exception, ex.Message);
            }
        }

        private void Open_Click(object sender, EventArgs e)
        {
            try
            {
                ChangeStatusBar();
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
                    openFileDialog.Filter = @"Value(*.sxml) | *.sxml";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        configPath = openFileDialog.FileName;
                        SXML_Config.Text = configPath.LoadFileByPath();
                    }
                }
            }
            catch (Exception ex)
            {
                ChangeStatusBar(ProcessStatus.Exception, ex.Message);
            }
        }

        private void Start_Click(object sender, EventArgs e)
        {
            try
            {
                if (!InProgress)
                {
                    ChangeStatusBar(ProcessStatus.Processing);
                    asyncPerforming.RunWorkerAsync();
                }
                else
                {
                    ChangeStatusBar(ProcessStatus.Aborted);
                    asyncPerforming.Abort();
                }
            }
            catch (Exception ex)
            {
                ChangeStatusBar(ProcessStatus.Exception, ex.Message);
            }
        }

        private int asyncExceptions = 0;
        private void AsyncPerforming_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                InProgress = true;
                Invoke(new Action(() => buttonStartStop.Text = @"Stop"));
                string sourceControl = string.Empty;
                Invoke(new Action(() => sourceControl = SXML_Config.Text.SaveStreamToFile(configPath)));

                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(sourceControl);
                st = new ScriptTemplate(xdoc);
                InProgress = false;
            }
            catch (Exception ex)
            {
                asyncExceptions++;
                ChangeStatusBar(ProcessStatus.Exception, ex.Message);
            }
        }

        private void AsyncPerforming_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (asyncExceptions == 0)
                ChangeStatusBar(ProcessStatus.Completed);
            asyncExceptions = 0;
            InProgress = false;
            Invoke(new Action(() => buttonStartStop.Text = @"Start"));
        }


        delegate void SetStatusMessage(ProcessStatus stat, string message);
        void ChangeStatusBar(ProcessStatus stat = ProcessStatus.None, string message = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new SetStatusMessage(ChangeStatusBar), new object[] { stat, message });
                return;
            }

            StatusBarLable.Text = (stat == ProcessStatus.None) ? string.Empty : stat.ToString("G");
            StatusBarDesc.Text = message ?? string.Empty;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            asyncPerforming.Abort();
            SXML_Config.Text.SaveStreamToFile(configPath);
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
