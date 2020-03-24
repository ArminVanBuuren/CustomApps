using LogsReader.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Utils;

namespace LogsReader
{
    public partial class MainForm : Form
    {
        private LogsReaderForm _current;
        /// <summary>
        /// Настройки схем
        /// </summary>
        public LRSettings AllSettings { get; }

        public Dictionary<TabPage, LogsReaderForm> AllForms { get; }

        public LogsReaderForm CurrentForm
        {
            get
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(() =>
                    {
                        if (MainTabControl.SelectedTab != null && AllForms.TryGetValue(MainTabControl.SelectedTab, out var current))
                            _current = current;
                    }));
                }
                else
                {
                    if (MainTabControl.SelectedTab != null && AllForms.TryGetValue(MainTabControl.SelectedTab, out var current))
                        _current = current;
                }

                return _current;
            }
        }

        public bool IsClosed { get; private set; } = false;

        public MainForm()
        {
            InitializeComponent();

            try
            {
                KeyPreview = true;
                KeyDown += MainForm_KeyDown;
                Closing += (s, e) =>
                {
                    IsClosed = true;
                    SaveSettings();
                    SaveInterfaceParams();
                };

                AllSettings = LRSettings.Deserialize();
                AllForms = new Dictionary<TabPage, LogsReaderForm>(AllSettings.SchemeList.Length);

                foreach (var scheme in AllSettings.SchemeList)
                {
                    try
                    {
                        var logsReader = new LogsReaderForm();
                        logsReader.LoadLogsReader(scheme);
                        logsReader.Dock = DockStyle.Fill;
                        logsReader.OnSaveScheme += SaveSettings;

                        var page = new TabPage
                        {
                            Text = scheme.Name,
                            UseVisualStyleBackColor = false,
                            Font = new Font("Segoe UI", 8.5F),
                            Margin = new Padding(0),
                            Padding = new Padding(0)
                        };
                        page.Controls.Add(logsReader);

                        AddPage(page);
                        AllForms.Add(page, logsReader);
                    }
                    catch (Exception ex)
                    {
                        Program.MessageShow($"Failed to load schema \"{scheme.Name}\"\r\n{ex.ToString()}", @"Load scheme");
                    }
                }

                var thread = new Thread(CalculateLocalResources) { IsBackground = true, Priority = ThreadPriority.Lowest };
                thread.Start();
            }
            catch (Exception ex)
            {
                Program.MessageShow(ex.ToString(), @"Initialization");
            }
        }

        //async void LoadFormAsync()
        //{
        //    await Task.Factory.StartNew(LoadForm);
        //}

        //void LoadForm()
        //{
        //    try
        //    {
                
        //    }
        //    catch (Exception ex)
        //    {
        //        Program.MessageShow(ex.ToString(), @"Load config");
        //    }
        //}

        void AddPage(TabPage page)
        {
            if (InvokeRequired)
                BeginInvoke(new MethodInvoker(() => MainTabControl.TabPages.Add(page)));
            else
                MainTabControl.TabPages.Add(page);
        }

        void SelectTab(int index)
        {
            if (InvokeRequired)
                BeginInvoke(new MethodInvoker(() =>
                {
                    if (MainTabControl.TabCount > 0 && MainTabControl.TabCount >= index + 1)
                        MainTabControl.SelectedIndex = index;
                }));
            else
            {
                if (MainTabControl.TabCount > 0 && MainTabControl.TabCount >= index + 1)
                    MainTabControl.SelectedIndex = index;
            }
        }


        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            CurrentForm?.LogsReaderKeyDown(sender, e);
        }

        /// <summary>
        /// Мониторинг локальных ресурсов
        /// </summary>
        void CalculateLocalResources()
        {
            try
            {
                var curProcName = SERVER.ObtainCurrentProcessName();
                PerformanceCounter appCPU = null;
                if (curProcName != null)
                {
                    appCPU = new PerformanceCounter("Process", "% Processor Time", curProcName, true);
                    appCPU.NextValue();
                }

                while (!IsClosed)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke(new MethodInvoker(delegate
                        {
                            if(AllForms == null || AllForms.Count == 0)
                                return;

                            double percent = 0;
                            if (appCPU != null)
                                double.TryParse(appCPU.NextValue().ToString(), out percent);
                            var cpuUsage = $"{(int)(percent / Environment.ProcessorCount),3}%";
                            var currentProcess = Process.GetCurrentProcess();
                            var threadsUsage = $"{currentProcess.Threads.Count,-2}";
                            var ramUsage = $"{currentProcess.PrivateMemorySize64.ToFileSize(),-5}";

                            foreach (var form in AllForms.Values)
                            {
                                form.CPUUsage.Text = cpuUsage;
                                form.ThreadsUsage.Text = threadsUsage;
                                form.RAMUsage.Text = ramUsage;
                            }
                        }));
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Program.MessageShow(ex.ToString(), @"System Resource Monitoring");
            }
        }

        public void ApplySettings()
        {
            try
            {
                if (!IsMdiChild)
                {
                    try
                    {
                        WindowState = Settings.Default.FormState;
                        if (WindowState != FormWindowState.Maximized && (Settings.Default.FormSize.Height < 100 || Settings.Default.FormSize.Width < 100 ||
                                                                         Settings.Default.FormLocation.X < 0 || Settings.Default.FormLocation.Y < 0))
                            WindowState = FormWindowState.Maximized;
                    }
                    catch
                    {
                        // ignored
                    }
                }


                //int i = 0;
                //foreach (DataGridViewColumn column in dgvFiles.Columns)
                //{
                //    dgvFiles.Columns[i].Width = Settings.Default["COL" + i] != null ? (int)Settings.Default["COL" + i] : 80;
                //    i++;
                //}

                //_settingsLoaded = true;
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private void SaveInterfaceParams()
        {
            try
            {
                //int i = 0;
                //foreach (DataGridViewColumn column in dgvFiles.Columns)
                //{
                //    Settings.Default["COL" + i] = dgvFiles.Columns[i].Width;
                //    i++;
                //}

                Settings.Default.FormLocation = Location;
                Settings.Default.FormSize = Size;
                Settings.Default.FormState = WindowState;

                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        async void SaveSettings(object sender, EventArgs args)
        {
            if (AllSettings != null)
                await LRSettings.SerializeAsync(AllSettings);
        }

        void SaveSettings()
        {
            if (AllSettings != null)
                LRSettings.Serialize(AllSettings);
        }
    }
}