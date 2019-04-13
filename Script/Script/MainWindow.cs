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
using System.Xml.Serialization;
using FastColoredTextBoxNS;
using Script.ColoredStyle;
using Script.Control;
using Script.DataGridViewCustom;
using Script.Properties;
using XPackage;

namespace Script
{
    enum ProcessStatus
    {
        None = 0,
        Processing = 1,
        Completed = 2,
        Aborted = 3,
        Exception = 4
    }

    public partial class MainWindow : Form
    {
        public static string ApplicationName { get; }
        public static string AppConfigPath { get; }
        public static string LocalPath => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        static MainWindow()
        {
            ApplicationName = Assembly.GetEntryAssembly().GetName().Name;
            AppConfigPath = Path.Combine(LocalPath, ApplicationName + ".xml");
        }

        string _configPath = string.Empty;
        ScriptTemplate st = null;
        AbortableBackgroundWorker asyncPerforming = new AbortableBackgroundWorker();
        ScriptSettings AppSettings;
        public bool InProgress { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            ProcessGrid.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(ProcessGrid, true, null);
            DataGridViewTextButtonColumn GridColumnPath = new DataGridViewCustom.DataGridViewTextButtonColumn {
                                                                                                                  AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                                                                                                                  ValueType = typeof(string),
                                                                                                                  HeaderText = @"Path",
                                                                                                                  ButtonClickHandler = GridColumnPath_ButtonClick
                                                                                                              };
            ProcessGrid.Columns.Add(GridColumnPath);

            if (File.Exists(AppConfigPath))
                AppSettings = DeserializeSettings(AppConfigPath);
            if (AppSettings == null)
                AppSettings = new ScriptSettings();

            uint i = 0;
            foreach (ConfigurationProcess proc in AppSettings.ProcessList)
            {
                ProcessGrid.Rows.Add(new object[] {
                                               i, proc.ConfiguraionName, proc.Description, "None", "Start", proc.Path
                                           });
                i++;
            }

            //grid.Rows[rowIndex].Cells[columnIndex].Value = value;



            ConfigStyle colored = new ConfigStyle(SXML_Config);
            asyncPerforming.DoWork += AsyncPerforming_DoWork;
            asyncPerforming.RunWorkerCompleted += AsyncPerforming_RunWorkerCompleted;


            try
            {
                _configPath = Path.Combine(LocalPath, "Script.Config.sxml");
                if (!File.Exists(_configPath))
                {
                    using (StreamWriter writer = new StreamWriter(_configPath, false, Functions.Enc))
                    {
                        //writer.Write(Properties.Resources.Script_Config);
                        writer.Write(ScriptTemplate.GetExampleOfConfig());
                        writer.Close();
                    }
                }
                SXML_Config.Text = _configPath.LoadFileByPath();
            }
            catch (Exception ex)
            {
                ChangeStatusBar(ProcessStatus.Exception, ex.Message);
            }
        }

        void LoadProcess()
        {

        }

        void GridColumnPath_ButtonClick(object sender, EventArgs arg)
        {
            ProcessGrid.EndEdit();
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
                openFileDialog.Filter = @"Value(*.sxml) | *.sxml";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _configPath = openFileDialog.FileName;
                    SXML_Config.Text = _configPath.LoadFileByPath();
                }
            }
            ProcessGrid.BeginEdit(false);
        }


        private void grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
            MessageBox.Show(this, @"Invalid value: " + ProcessGrid[e.ColumnIndex, e.RowIndex].EditedFormattedValue, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                        _configPath = openFileDialog.FileName;
                        SXML_Config.Text = _configPath.LoadFileByPath();
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
                //Invoke(new Action(() => buttonStartStop.Text = @"Stop"));
                string sourceControl = string.Empty;
                Invoke(new Action(() => sourceControl = SXML_Config.Text.SaveStreamToFile(_configPath)));

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
            //Invoke(new Action(() => buttonStartStop.Text = @"Start"));
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

        #region Serialize And Deserialize Settings

        static ScriptSettings DeserializeSettings(string settPath)
        {
            if (!File.Exists(settPath))
                return null;

            try
            {
                using (FileStream stream = new FileStream(settPath, FileMode.Open, FileAccess.Read))
                {
                    return new XmlSerializer(typeof(ScriptSettings)).Deserialize(stream) as ScriptSettings;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return null;
        }

        void SerializeSettings()
        {
            try
            {
                using (FileStream stream = new FileStream(Path.Combine(LocalPath, ApplicationName + ".xml"), FileMode.Create, FileAccess.ReadWrite))
                {
                    new XmlSerializer(typeof(ScriptSettings)).Serialize(stream, AppSettings);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            asyncPerforming.Abort();
            //SXML_Config.Text.SaveStreamToFile(_configPath);
            SerializeSettings();




            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void ProcessGrid_Click(object sender, EventArgs e)
        {

        }

        private void ProcessGrid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {

        }

        private void ProcessGrid_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {

        }
    }
}
