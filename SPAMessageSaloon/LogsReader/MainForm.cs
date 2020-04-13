using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using LogsReader.Reader;
using Microsoft.WindowsAPICodePack.Taskbar;
using SPAMessageSaloon.Common;
using Utils;
using Utils.WinForm;

namespace LogsReader
{
    public partial class MainForm : Form
    {
        private int _countOfLastProcess = 0;
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
                base.Text = $"Logs Reader {ASSEMBLY.CurrentVersion}";
                KeyPreview = true;
                KeyDown += MainForm_KeyDown;
                Closing += (s, e) =>
                {
                    IsClosed = true;
                    if (AllSettings != null)
                        LRSettings.Serialize(AllSettings);
                    if (AllForms != null)
                        foreach (var form in AllForms.Values)
                            form.SaveInterfaceParams();
                    SaveInterfaceParams();
                };
                Shown += (s, e) =>
                {
                    if (AllForms == null)
                        return;
                    foreach (var form in AllForms)
                        form.Value.ApplySettings();

                    var thread = new Thread(CalculateLocalResources) { IsBackground = true, Priority = ThreadPriority.Lowest };
                    thread.Start();
                };


                MainTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
                MainTabControl.DrawItem += MainTabControl_DrawItem;

                AllSettings = LRSettings.Deserialize();
                AllForms = new Dictionary<TabPage, LogsReaderForm>(AllSettings.SchemeList.Length);

                foreach (var scheme in AllSettings.SchemeList)
                {
                    try
                    {
                        var logsReader = new LogsReaderForm();
                        logsReader.LoadForm(scheme);
                        logsReader.Dock = DockStyle.Fill;
                        logsReader.OnSchemeChanged += SaveSchemas;

                        var page = new TabPage
                        {
                            Text = scheme.Name,
                            UseVisualStyleBackColor = false,
                            Font = new Font("Segoe UI", 8.5F),
                            Margin = new Padding(0),
                            Padding = new Padding(0)
                        };
                        page.Controls.Add(logsReader);

                        MainTabControl.TabPages.Add(page);
                        AllForms.Add(page, logsReader);
                    }
                    catch (Exception ex)
                    {
                        ReportMessage.Show($"Failed to load schema \"{scheme.Name}\"\r\n{ex}", MessageBoxIcon.Error, @"Load scheme");
                    }
                }
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, @"Initialization");
            }
        }

        private void MainTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = MainTabControl.TabPages[e.Index];
            if (page == MainTabControl.SelectedTab)
            {
                RenderTabPage(page, e, Color.FromArgb(0, 200, 205), Color.White);
            }
            else
            {
                RenderTabPage(page, e, Color.DimGray, Color.White);
            }
        }

        void RenderTabPage(TabPage page, DrawItemEventArgs e, Color fore, Color text)
        {
            e.Graphics.FillRectangle(new SolidBrush(fore), e.Bounds);

            Rectangle paddedBounds = e.Bounds;
            int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
            paddedBounds.Offset(1, yOffset);
            TextRenderer.DrawText(e.Graphics, page.Text, e.Font, paddedBounds, text);
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

                var taskBarNotify = TaskbarManager.IsPlatformSupported
                    ? (Action<int, int>) ((inProgress, progressItems) =>
                    {
                        if (inProgress == 0)
                        {
                            TaskbarManager.Instance.SetOverlayIcon(null, "");
                            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress); // TaskbarProgressBarState.Normal
                        }
                        else
                        {
                            SetTaskBarOverlay(inProgress);
                            TaskbarManager.Instance.SetProgressValue(progressItems, inProgress * 100);
                        }
                    })
                    : null;

                void Monitoring()
                {
                    if (AllForms == null || AllForms.Count == 0)
                        return;

                    double percent = 0;
                    if (appCPU != null)
                        double.TryParse(appCPU.NextValue().ToString(), out percent);
                    var cpuUsage = $"{(int) (percent / Environment.ProcessorCount),3}%";
                    var currentProcess = Process.GetCurrentProcess();
                    var threadsUsage = $"{currentProcess.Threads.Count,-2}";
                    var ramUsage = $"{currentProcess.PrivateMemorySize64.ToFileSize(),-5}";

                    int inProgress = 0;
                    int progressItems = 0;
                    foreach (var form in AllForms.Values)
                    {
                        form.CPUUsage.Text = cpuUsage;
                        form.ThreadsUsage.Text = threadsUsage;
                        form.RAMUsage.Text = ramUsage;

                        if ((form.Progress > 0 && form.Progress < 100) || (form.Progress == 0 && form.IsWorking))
                        {
                            inProgress++;
                            progressItems += form.Progress;
                        }
                    }

                    if (_countOfLastProcess > 0 && inProgress == 0)
                        FlashWindow.Flash(this);

                    taskBarNotify?.Invoke(inProgress, progressItems);
                    _countOfLastProcess = inProgress;
                }

                while (!IsClosed)
                {
                    if (InvokeRequired)
                        Invoke(new MethodInvoker(Monitoring));
                    else
                        Monitoring();

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, @"System Resource Monitoring");

                void Clear()
                {
                    if (AllForms == null || AllForms.Count == 0)
                        return;
                    foreach (var form in AllForms.Values)
                    {
                        form.CPUUsage.Text = string.Empty;
                        form.ThreadsUsage.Text = string.Empty;
                        form.RAMUsage.Text = string.Empty;
                    }
                }

                if (InvokeRequired)
                    Invoke(new MethodInvoker(Clear));
                else
                    Clear();
            }
        }

        private static void SetTaskBarOverlay(int countItem)
        {
            var bmp = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillEllipse(Brushes.Red, new Rectangle(new Point(4, 4), new Size(27, 27)));
                g.DrawString(countItem.ToString(), new Font("Sans serif", 16, GraphicsUnit.Point), Brushes.White, new Rectangle(new Point(countItem <= 9 ? 8 : 1, 5), bmp.Size));
            }
            TaskbarManager.Instance.SetOverlayIcon(Icon.FromHandle(bmp.GetHicon()), "");
        }

        public void ApplySettings()
        {
            try
            {
                if (!IsMdiChild)
                {
                    WindowState = Settings.Default.FormState;
                    if (WindowState != FormWindowState.Maximized &&
                        (Settings.Default.FormSize.Height < 100 ||
                         Settings.Default.FormSize.Width < 100 ||
                         Settings.Default.FormLocation.X < 0 ||
                         Settings.Default.FormLocation.Y < 0))
                    { WindowState = FormWindowState.Maximized; return; }
                    else if (Settings.Default.FormSize.Height > 300 && Settings.Default.FormSize.Width > 300)
                        Size = Settings.Default.FormSize;

                    CenterToScreen();
                    //Location = Settings.Default.FormLocation;
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        async void SaveSchemas(object sender, EventArgs args)
        {
            if (AllSettings != null)
                await LRSettings.SerializeAsync(AllSettings);
        }

        void SaveInterfaceParams()
        {
            try
            {
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
    }
}