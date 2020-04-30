using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;
using Utils;
using Utils.AppUpdater;
using Utils.Handles;
using Utils.UIControls.Main;
using Utils.WinForm;
using SPAMassageSaloon.Common;
using SPAMassageSaloon.Properties;
using LogsReader;
using SPAFilter;
using XPathTester;
using FontStyle = System.Drawing.FontStyle;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace SPAMassageSaloon
{
    public enum NationalLanguage
    {
        English = 0,
        Russian = 1
    }


    public partial class MainForm : Form
    {
        internal static readonly string AppName = "SPA Massage Saloon";
        internal static readonly string FormName;
        internal static readonly string BuildTime;

        private AboutWindow _about;
        private NationalLanguage _language;
        private int _countOfLastProcess = 0;
        private int _countOfXPathTester = 0;

        public ToolStripStatusLabel _cpuUsage;
        public ToolStripStatusLabel _threadsUsage;
        public ToolStripStatusLabel _ramUsage;

        public bool FormOnClosing { get; private set; } = false;

        internal int CountOfDefaultStatusItems { get; private set; } = 0;

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

                    foreach (var form in MdiChildren.OfType<ISaloonForm>())
                    {
                        try
                        {
                            form.ApplySettings();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    
                    _about?.ApplySettings();

                    languageToolStripMenuItem.Text = Resources.Txt_Language;
                    aboutToolStripMenuItem.Text = Resources.Txt_About;   
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }
        }

        private string LastUpdatePackage
        {
            get => GetRegeditValue(nameof(LastUpdatePackage));
            set => SetRegeditValue(nameof(LastUpdatePackage), value);
        }

        static MainForm()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            BuildTime = $"Build time: {currentAssembly.GetLinkerTime():dd.MM.yyyy HH:mm:ss}";
            FormName = $"{AppName} {currentAssembly.GetAssemblyInfo().CurrentVersion}";
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                this.Visible = false;
                this.IsMdiContainer = true;
                base.Text = FormName;

                try
                {
                    ToolStripManager.LoadSettings(this);
                    InitializeToolbarsMenu();

                    if (!IsMdiChild)
                    {
                        WindowState = Settings.Default.FormState;
                        if (WindowState != FormWindowState.Maximized &&
                            (Settings.Default.FormSize.Height < 100 ||
                             Settings.Default.FormSize.Width < 100 ||
                             Settings.Default.FormLocation.X < 0 ||
                             Settings.Default.FormLocation.Y < 0))
                        {
                            WindowState = FormWindowState.Maximized;
                        }
                        else if (Settings.Default.FormSize.Height > 300 && Settings.Default.FormSize.Width > 300)
                            Size = Settings.Default.FormSize;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                if (!NationalLanguage.TryParse(GetRegeditValue(nameof(Language)), out NationalLanguage lang))
                    lang = NationalLanguage.English;
                switch (lang)
                {
                    case NationalLanguage.Russian:
                        russianToolStripMenuItem_Click(this, null);
                        break;
                    default:
                        englishToolStripMenuItem_Click(this, null);
                        break;
                }

                try
                {
                    AppUpdater = new ApplicationUpdater(Assembly.GetExecutingAssembly(), 2);
                    AppUpdater.OnUpdate += AppUpdater_OnUpdate;
                    AppUpdater.OnSuccessfulUpdated += AppUpdater_OnSuccessfulUpdated;
                    AppUpdater.Start();
                }
                catch (Exception ex)
                {
                    // ignored
                }

                var statusStripItemsPaddingStart = new Padding(0, 2, 0, 2);
                var statusStripItemsPaddingEnd = new Padding(-3, 2, 1, 2);

                var autor = new ToolStripButton("?")
                    {Font = new Font("Verdana", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0), Margin = new Padding(0, 0, 0, 2), ForeColor = Color.Blue};
                autor.Click += (s, args) =>
                {
                    Presenter.ShowOwner();
                    STREAM.GarbageCollect();
                };
                statusStrip.Items.Add(autor);
                statusStrip.Items.Add(new ToolStripSeparator());

                statusStrip.Items.Add(new ToolStripStatusLabel("CPU:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                _cpuUsage = new ToolStripStatusLabel("    ") {Font = this.Font, Margin = new Padding(-7, 2, 1, 2)};
                statusStrip.Items.Add(_cpuUsage);
                statusStrip.Items.Add(new ToolStripSeparator());

                statusStrip.Items.Add(new ToolStripStatusLabel("Threads:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                _threadsUsage = new ToolStripStatusLabel("  ") {Font = this.Font, Margin = statusStripItemsPaddingEnd};
                statusStrip.Items.Add(_threadsUsage);
                statusStrip.Items.Add(new ToolStripSeparator());

                statusStrip.Items.Add(new ToolStripStatusLabel("RAM:") {Font = this.Font, Margin = statusStripItemsPaddingStart});
                _ramUsage = new ToolStripStatusLabel("       ") {Font = this.Font, Margin = statusStripItemsPaddingEnd};
                statusStrip.Items.Add(_ramUsage);
                statusStrip.Items.Add(new ToolStripSeparator());
                CountOfDefaultStatusItems = statusStrip.Items.Count;
                statusStrip.ItemRemoved += (o, args) => { if(statusStrip?.Items.Count == CountOfDefaultStatusItems) base.Text = FormName; };

                Closing += (o, args) =>
                {
                    try
                    {
                        Settings.Default.FormLocation = Location;
                        Settings.Default.FormSize = Size;
                        Settings.Default.FormState = WindowState;
                        Settings.Default.Save();
                        using (var reg = new RegeditControl(this.GetAssemblyInfo().ApplicationName))
                        {
                            reg.Remove("UpdateAlreadyShown");
                            reg.Remove("LastUpdatePackage");
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                };
                Closing += (o, args) => { FormOnClosing = true; };
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex);
            }
            finally
            {
                CenterToScreen();
                this.Visible = true;

                var monitoring = new Thread(CalculateLocalResources) { IsBackground = true, Priority = ThreadPriority.Lowest };
                monitoring.Start();
            }
        }

        private void AppUpdater_OnSuccessfulUpdated(object sender, ApplicationUpdaterArgs args)
        {
            try
            {
                if (args.Control == null)
                    return;

                var separator = $"\r\n{new string('-', 61)}\r\n";
                var description = separator.TrimStart()
                                  + string.Join(separator, args.Control.Select(x => $"{x.RemoteFile.Location} Version = {x.RemoteFile.VersionString}\r\nDescription = {x.RemoteFile.Description}")).Trim()
                                  + separator.TrimEnd();

                ReportMessage.Show(string.Format(Resources.Txt_Updated, AppName, this.GetAssemblyInfo().CurrentVersion, BuildTime, description).Trim(), MessageBoxIcon.Information, AppName);
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private void AppUpdater_OnUpdate(object sender, ApplicationUpdaterArgs args)
        {
            try
            {
                SaveData();
                AppUpdater.DoUpdate(args.Control);
            }
            catch (Exception ex)
            {
                AppUpdater.Refresh();
            }
        }

        public void SaveData()
        {
            try
            {
                foreach (var form in MdiChildren.OfType<ISaloonForm>())
                {
                    try
                    {
                        form.SaveData();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            catch (Exception e)
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
                    ? (Action<int, int>) ((processesCount, totalProgress) =>
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
                    var cpuUsage = $"{(int) (percent / Environment.ProcessorCount),3}%";
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
                        if (form is Form child && child.IsDisposed)
                            continue;
                        processCount += form.ActiveProcessesCount;
                        totalProgress += form.ActiveTotalProgress;
                    }

                    if (_countOfLastProcess > 0 && processCount == 0)
                        FlashWindow.Flash(this);

                    taskBarNotify?.Invoke(processCount, totalProgress);
                    _countOfLastProcess = processCount;
                }

                while (!FormOnClosing)
                {
                    try { this.SafeInvoke(Monitoring); }
                    catch (InvalidOperationException) { }
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
            ShowMdiForm(() =>
            {
                var tester = new XPathTesterForm();
                tester.Load += (o, args) => _countOfXPathTester++;
                tester.Closed += (o, args) => _countOfXPathTester--;
                return tester;
            }, _countOfXPathTester < 6);
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
            try
            {
                var mdiForm = sender as Form;
                var button = new MDIManagerButton(mdiForm) { BackColor = Color.White };
                var mainForm = mdiForm?.MdiParent as MainForm;
                var status = mainForm?.statusStrip;
                if (status == null)
                    return;

                mainForm.Text = $"{MainForm.FormName}  [{mdiForm.Text.Trim()}]";
                button.Activated += (o, args) =>
                {
                    try
                    {
                        var mdiButton = (MDIManagerButton)o;
                        mainForm.Text = $"{MainForm.FormName}  [{mdiButton.mdiForm.Text.Trim()}]";
                    }
                    catch (Exception ex)
                    {
                        // ignored
                    }
                };
                button.Click += (o, args) =>
                {
                    try
                    {
                        var mdiButton = (MDIManagerButton)o;
                        mdiButton.mdiForm.Activate();
                    }
                    catch (Exception ex)
                    {
                        // ignored
                    }
                };

                switch (sender)
                {
                    case LogsReaderMainForm _:
                        status.Items.Insert(mainForm.CountOfDefaultStatusItems, button);
                        break;
                    case SPAFilterForm _:
                        status.Items.Insert(status.Items.Count > mainForm.CountOfDefaultStatusItems
                                            && status.Items[mainForm.CountOfDefaultStatusItems] is MDIManagerButton mdiButton
                                            && mdiButton.mdiForm is LogsReaderMainForm
                            ? mainForm.CountOfDefaultStatusItems + 1
                            : mainForm.CountOfDefaultStatusItems, button);
                        break;
                    default:
                        status.Items.Add(button);
                        break;
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private void toolButtonAbout_ButtonClick(object sender, EventArgs e)
        {
            toolButtonAbout.ShowDropDown();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_about != null)
                {
                    _about.Focus();
                    _about.Activate();
                }
                else
                {
                    _about = new AboutWindow
                    {
                        //Topmost = true,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    _about.Focus();
                    _about.Show();
                    //about.ShowDialog();
                    _about.Closed += About_Closed;
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private void About_Closed(object sender, EventArgs e)
        {
            try
            {
                _about.Closed -= About_Closed;
                _about = null;

            }
            catch (Exception ex)
            {
                // ignored
            }
            finally
            {
                STREAM.GarbageCollect();
            }
        }

        string GetRegeditValue(string name)
        {
            try
            {
                using (var reg = new RegeditControl(this.GetAssemblyInfo().ApplicationName))
                    return (string)reg[name];
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        void SetRegeditValue(string name, object value)
        {
            try
            {
                using (var reg = new RegeditControl(this.GetAssemblyInfo().ApplicationName))
                    reg[name] = value;
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
