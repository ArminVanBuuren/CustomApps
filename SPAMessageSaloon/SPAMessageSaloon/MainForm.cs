using SPAMessageSaloon.Common;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;
using Utils;
using Utils.AppUpdater;
using Utils.AppUpdater.Updater;
using Utils.Handles;
using Utils.UIControls.Main;
using Utils.WinForm;
using LogsReader;
using SPAFilter;
using XPathTester;

namespace SPAMessageSaloon
{
    public enum NationalLanguage
    {
        English = 0,
        Russian = 1
    }


    public partial class MainForm : Form
    {
        private NationalLanguage _language;
        private int _countOfLastProcess = 0;

        public ToolStripStatusLabel _cpuUsage;
        public ToolStripStatusLabel _threadsUsage;
        public ToolStripStatusLabel _ramUsage;

        ApplicationUpdater AppUpdater { get; set; }

        public NationalLanguage Language
        {
            get => _language;
            private set
            {
                try
                {
                    _language = value;
                    SetRegeditValue(nameof(Language), value);

                    var culture = value == NationalLanguage.Russian ? new CultureInfo("ru-RU") : new CultureInfo("en-US");


                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = culture;

                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                    Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy HH:mm:ss.fff";
                    Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongDatePattern = "dd.MM.yyyy HH:mm:ss.fff";

                    CultureInfo.DefaultThreadCurrentCulture.DateTimeFormat = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat;
                    CultureInfo.DefaultThreadCurrentUICulture.DateTimeFormat = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat;
                    

                    languageToolStripMenuItem.Text = Properties.Resources.Txt_Language;
                    aboutToolStripMenuItem.Text = Properties.Resources.Txt_About;

                    foreach (var form in MdiChildren.OfType<ISaloonForm>())
                        form.ApplySettings();
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }
        }

        private string LastUpdatePackage
        {
            get => GetRegeditValue(nameof(LastUpdatePackage))?.ToString();
            set => SetRegeditValue(nameof(LastUpdatePackage), value);
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    AppUpdater = new ApplicationUpdater(Assembly.GetExecutingAssembly(), LastUpdatePackage, 900);
                    AppUpdater.OnFetch += AppUpdater_OnFetch;
                    AppUpdater.OnUpdate += AppUpdater_OnUpdate;
                    AppUpdater.OnProcessingError += AppUpdater_OnProcessingError;
                    AppUpdater.Start();
                    AppUpdater.CheckUpdates();
                }
                catch (Exception ex)
                {
                    // ignored
                }

                IsMdiContainer = true;
                base.Text = $"SPA Message Saloon {this.GetAssemblyInfo().CurrentVersion}";

                ToolStripManager.LoadSettings(this);
                InitializeToolbarsMenu();

                if (!NationalLanguage.TryParse(GetRegeditValue(nameof(Language))?.ToString(), out NationalLanguage lang))
                    lang = NationalLanguage.English;
                switch (lang)
                {
                    case NationalLanguage.Russian:
                        russianToolStripMenuItem_Click(this, null); break;
                    default:
                        englishToolStripMenuItem_Click(this, null); break;
                }

                var statusStripItemsPaddingStart = new Padding(0, 2, 0, 2);
                var statusStripItemsPaddingEnd = new Padding(-3, 2, 1, 2);

                var autor = new ToolStripButton("?") { Font = new Font("Verdana", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0), Margin = new Padding(0, 0, 0, 2), ForeColor = Color.Blue };
                autor.Click += (s, args) => { Presenter.ShowOwner(); STREAM.GarbageCollect(); };
                statusStrip.Items.Add(autor);
                statusStrip.Items.Add(new ToolStripSeparator());

                statusStrip.Items.Add(new ToolStripStatusLabel("CPU:") { Font = this.Font, Margin = statusStripItemsPaddingStart });
                _cpuUsage = new ToolStripStatusLabel("    ") { Font = this.Font, Margin = new Padding(-7, 2, 1, 2) };
                statusStrip.Items.Add(_cpuUsage);
                statusStrip.Items.Add(new ToolStripSeparator());

                statusStrip.Items.Add(new ToolStripStatusLabel("Threads:") { Font = this.Font, Margin = statusStripItemsPaddingStart });
                _threadsUsage = new ToolStripStatusLabel("  ") { Font = this.Font, Margin = statusStripItemsPaddingEnd };
                statusStrip.Items.Add(_threadsUsage);
                statusStrip.Items.Add(new ToolStripSeparator());

                statusStrip.Items.Add(new ToolStripStatusLabel("RAM:") { Font = this.Font, Margin = statusStripItemsPaddingStart });
                _ramUsage = new ToolStripStatusLabel("       ") { Font = this.Font, Margin = statusStripItemsPaddingEnd };
                statusStrip.Items.Add(_ramUsage);
                statusStrip.Items.Add(new ToolStripSeparator());


                Shown += (s, args) =>
                {
                    var thread = new Thread(CalculateLocalResources) { IsBackground = true, Priority = ThreadPriority.Lowest };
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

        #region Application Update

        private static void AppUpdater_OnFetch(object sender, ApplicationFetchingArgs args)
        {
            if (args == null)
                return;

            if (args.Control == null)
                args.Result = UpdateBuildResult.Cancel;
        }

        private void AppUpdater_OnUpdate(object sender, ApplicationUpdatingArgs args)
        {
            try
            {
                if (args?.Control?.ProjectBuildPack == null)
                {
                    AppUpdater.Refresh();
                    return;
                }

                var updater = args.Control;
                updater.SecondsMoveDelay = 1;
                updater.SecondsRunDelay = 5;

                if (updater.ProjectBuildPack.NeedRestartApplication)
                    SaveData(updater);

                AppUpdater.DoUpdate(updater);
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private static void AppUpdater_OnProcessingError(object sender, ApplicationUpdatingArgs args)
        {

        }

        #endregion

        public void SaveData(IUpdater updater)
        {
            try
            {
                LastUpdatePackage = updater?.ProjectBuildPack.Name;
                foreach (var form in MdiChildren.OfType<ISaloonForm>())
                    form.SaveData();
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        /// <summary>
        /// Мониторинг системных ресурсов
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
                    if (this.IsDisposed || _cpuUsage.IsDisposed || _threadsUsage.IsDisposed || _ramUsage.IsDisposed)
                        return;

                    double percent = 0;
                    if (appCPU != null)
                        double.TryParse(appCPU.NextValue().ToString(), out percent);
                    var cpuUsage = $"{(int)(percent / Environment.ProcessorCount),3}%";
                    var currentProcess = Process.GetCurrentProcess();
                    var threadsUsage = $"{currentProcess.Threads.Count,-2}";
                    var ramUsage = $"{currentProcess.PrivateMemorySize64.ToFileSize(),-5}";

                    _cpuUsage.Text = cpuUsage;
                    _threadsUsage.Text = threadsUsage;
                    _ramUsage.Text = ramUsage;

                    int processCount = 0;
                    int totalProgress = 0;
                    foreach (var form in MdiChildren.OfType<ISaloonForm>())
                    {
                        if(form is Form child && child.IsDisposed)
                            continue;
                        processCount += form.ActiveProcessesCount;
                        totalProgress += form.ActiveTotalProgress;
                    }

                    if (_countOfLastProcess > 0 && processCount == 0)
                        FlashWindow.Flash(this);

                    taskBarNotify?.Invoke(processCount, totalProgress);
                    _countOfLastProcess = processCount;
                }

                while (!this.IsDisposed)
                {
                    this.SafeInvoke(Monitoring);
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, Properties.Resources.Txt_SystemResourceMonitoring);
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
                if (ctrl is ToolStrip strip && !(ctrl is MenuStrip))
                    strip.AllowItemReorder = true;
        }

        private void russianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            englishToolStripMenuItem.Checked = false;
            russianToolStripMenuItem.Checked = true;
            Language = NationalLanguage.Russian;
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            englishToolStripMenuItem.Checked = true;
            russianToolStripMenuItem.Checked = false;
            Language = NationalLanguage.English;
        }

        private void toolStripLogsReaderButton_Click(object sender, EventArgs e)
        {
            ShowMdiForm(() => new LogsReaderMainForm());
        }

        private void toolStripSpaFilterButton_Click(object sender, EventArgs e)
        {
            ShowMdiForm(SPAFilterForm.GetControl);
        }

        private void toolStripXPathButton_Click(object sender, EventArgs e)
        {
            ShowMdiForm(() => new XPathTesterForm());
        }

        public T ShowMdiForm<T>(Func<T> formMaker, bool newInstance = false) where T : Form, ISaloonForm
        {
            try
            {
                if (!newInstance && ActivateMdiForm(out T form))
                    return form;

                form = formMaker.Invoke();
                if (form == null)
                    return null;

                form.MdiParent = this;
                //  form.WindowState = FormWindowState.Maximized;

                form.TopLevel = false;
                form.ControlBox = false;
                form.Dock = DockStyle.Fill;
                form.FormBorderStyle = FormBorderStyle.None;

                form.Load += MDIManagerButton_Load;
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
            foreach (Form f in MdiChildren)
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

        private static void MDIManagerButton_Load(object sender, EventArgs e)
        {
            MDIManagerButton button = new MDIManagerButton(sender as Form) {BackColor = Color.White};
            button.Click += (S, E) => { ((MDIManagerButton) S).mdiForm.Activate(); };
            ((sender as Form)?.MdiParent as MainForm)?.statusStrip.Items.Add(button);
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
            toolStripSplitButton1.ShowDropDown();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        object GetRegeditValue(string name)
        {
            using (var reg = new RegeditControl(this.GetAssemblyInfo().ApplicationName))
                return (string)reg[name];
        }

        void SetRegeditValue(string name, object value)
        {
            using (var reg = new RegeditControl(this.GetAssemblyInfo().ApplicationName))
                reg[name] = value;
        }
    }
}
