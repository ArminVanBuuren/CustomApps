using SPAMessageSaloon.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;
using Utils;
using Utils.WinForm;
using LogsReader;
using SPAFilter;
using XPathTester;

namespace SPAMessageSaloon
{
    public partial class MainForm : Form
    {
        private int _countOfLastProcess = 0;

        public bool IsClosed { get; private set; } = false;

        public ToolStripStatusLabel CPUUsage { get; }
        public ToolStripStatusLabel ThreadsUsage { get; }
        public ToolStripStatusLabel RAMUsage { get; }

        public MainForm()
        {
            InitializeComponent();
            IsMdiContainer = true;
            base.Text = $"SPA Message Saloon {this.GetAssemblyInfo().CurrentVersion}";

            try
            {
                var statusStripItemsPaddingStart = new Padding(0, 2, 0, 2);
                var statusStripItemsPaddingMiddle = new Padding(-3, 2, 0, 2);
                var statusStripItemsPaddingEnd = new Padding(-3, 2, 1, 2);

                var autor = new ToolStripButton("?")
                    {Font = new Font("Verdana", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0), Margin = new Padding(0, 0, 0, 2), ForeColor = Color.Blue};
                autor.Click += (sender, args) =>
                {
                    ReportMessage.Show(@"Hello! Thanks.",
                        MessageBoxIcon.Asterisk, $"© {this.GetAssemblyInfo().Company}");
                };
                statusStrip.Items.Add(autor);
                statusStrip.Items.Add(new ToolStripSeparator());

                statusStrip.Items.Add(new ToolStripStatusLabel("CPU:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                CPUUsage = new ToolStripStatusLabel("    ") {Font = this.Font, Margin = new Padding(-7, 2, 1, 2)};
                statusStrip.Items.Add(CPUUsage);
                statusStrip.Items.Add(new ToolStripSeparator());

                statusStrip.Items.Add(new ToolStripStatusLabel("Threads:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                ThreadsUsage = new ToolStripStatusLabel("  ") {Font = this.Font, Margin = statusStripItemsPaddingEnd};
                statusStrip.Items.Add(ThreadsUsage);
                statusStrip.Items.Add(new ToolStripSeparator());

                statusStrip.Items.Add(new ToolStripStatusLabel("RAM:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                RAMUsage = new ToolStripStatusLabel("       ") {Font = this.Font, Margin = statusStripItemsPaddingEnd};
                statusStrip.Items.Add(RAMUsage);
                statusStrip.Items.Add(new ToolStripSeparator());


                Closing += (sender, args) => { IsClosed = true; };
                Shown += (s, e) =>
                {
                    var thread = new Thread(CalculateLocalResources) {IsBackground = true, Priority = ThreadPriority.Lowest};
                    thread.Start();
                };
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex);
            }
            finally
            {
                CenterToScreen();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ToolStripManager.LoadSettings(this);
            InitializeToolbarsMenu();
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
                    ? (Action<int, int>)((processesCount, totalProgress) =>
                    {
                        if (processesCount == 0)
                        {
                            TaskbarManager.Instance.SetOverlayIcon(null, "");
                            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress); // TaskbarProgressBarState.Normal
                        }
                        else
                        {
                            SetTaskBarOverlay(processesCount);
                            TaskbarManager.Instance.SetProgressValue(totalProgress, processesCount * 100);
                        }
                    })
                    : null;

                void Monitoring()
                {
                    double percent = 0;
                    if (appCPU != null)
                        double.TryParse(appCPU.NextValue().ToString(), out percent);
                    var cpuUsage = $"{(int)(percent / Environment.ProcessorCount),3}%";
                    var currentProcess = Process.GetCurrentProcess();
                    var threadsUsage = $"{currentProcess.Threads.Count,-2}";
                    var ramUsage = $"{currentProcess.PrivateMemorySize64.ToFileSize(),-5}";

                    CPUUsage.Text = cpuUsage;
                    ThreadsUsage.Text = threadsUsage;
                    RAMUsage.Text = ramUsage;

                    int processCount = 0;
                    int totalProgress = 0;
                    foreach (var form in MdiChildren.OfType<ISPAMessageSaloonItems>())
                    {
                        processCount += form.ActiveProcessesCount;
                        totalProgress += form.ActiveTotalProgress;
                    }

                    if (_countOfLastProcess > 0 && processCount == 0)
                        FlashWindow.Flash(this);

                    taskBarNotify?.Invoke(processCount, totalProgress);
                    _countOfLastProcess = processCount;
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

        private void InitializeToolbarsMenu()
        {
            foreach (Control ctrl in toolStrip.Controls)
            {
                if (ctrl is ToolStrip strip && !(ctrl is MenuStrip))
                {
                    strip.AllowItemReorder = true;
                }
            }
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
                childForm.Close();
        }

        private void russianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            englishToolStripMenuItem.Checked = true;
            russianToolStripMenuItem.Checked = false;

            CultureInfo info = this.GetCultureInfo();
            if (info != null)
            {
                Thread.CurrentThread.CurrentUICulture = info;
                ApplyResources();
            }
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            englishToolStripMenuItem.Checked = false;
            russianToolStripMenuItem.Checked = true;

            CultureInfo info = this.GetCultureInfo();
            if (info != null)
            {
                Thread.CurrentThread.CurrentUICulture = info;
                ApplyResources();
            }
        }

        private CultureInfo GetCultureInfo()
        {
            if (this.russianToolStripMenuItem.Checked)
                return new CultureInfo("ru-RU");
            return new CultureInfo("en-US");
        }

        private void toolStripLogsReaderButton_Click(object sender, EventArgs e)
        {
            ShowMdiForm(() => new LogsReaderMainForm());
        }

        private void toolStripSpaFilterButton_Click(object sender, EventArgs e)
        {
            ShowMdiForm(() => new SPAFilterForm());
        }

        private void toolStripXPathButton_Click(object sender, EventArgs e)
        {
            ShowMdiForm(() => new XPathTesterForm());
        }

        private void ApplyResources()
        {
            
        }

        public T ShowMdiForm<T>(Func<T> formMaker, bool newInstance = false) where T : Form, ISPAMessageSaloonItems
        {
            try
            {
                if (!newInstance && ActivateMdiForm(out T form))
                    return form;

                form = formMaker.Invoke();
                if (form == null)
                    return null;

                form.MdiParent = this;
                form.Load += MDIManagerButton_Load;
                form.WindowState = FormWindowState.Maximized;
                form.Show();

                return form;
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex);
                return null;
            }
        }

        private bool ActivateMdiForm<T>(out T form) where T : Form
        {
            foreach (Form f in this.MdiChildren)
            {
                form = f as T;
                if (form != null)
                {
                    f.Activate();
                    return true;
                }
            }
            form = null;
            return false;
        }

        private void MDIManagerButton_Load(object sender, EventArgs e)
        {
            MDIManagerButton button = new MDIManagerButton(sender as Form) {BackColor = Color.White};
            button.Click += (S, E) => { ((MDIManagerButton) S).mdiForm.Activate(); };
            ((sender as Form)?.MdiParent as MainForm)?.statusStrip.Items.Add(button);
        }


    }
}
