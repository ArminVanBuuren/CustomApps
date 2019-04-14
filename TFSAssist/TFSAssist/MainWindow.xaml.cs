using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using System.Windows.Threading;
using TFSAssist.Control;
using TFSAssist.Remoter;
using TLSharp.Core.MTProto.Crypto;
using Utils;
using Utils.AppUpdater;
using Utils.AppUpdater.Updater;
using Utils.Handles;
using Binding = System.Windows.Data.Binding;
using ProgressBar = System.Windows.Controls.ProgressBar;
using TextBox = System.Windows.Controls.TextBox;
using Timer = System.Timers.Timer;

namespace TFSAssist
{
    public class LogFlowDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return (double) 0;
            if ((double) value < 2000)
                return 2000;
            return (double) value - 25;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string STR_START = "START";
        private const string STR_STOP = "STOP";
        private readonly string ERR_SECOND_PROC = $"{nameof(TFSAssist)} already started. Please check your notification area. To run second process, you can rename the executable file.";

        // 15 минут на проверку обновлений. Потому что если долго не будет интернета, в логах будут одни ошибки.
        private const int _intervalCheckUpdatesSec = 900;
        private const int _timeoutMSECToShowToolTip = 2000;
        private const int _timeoutToShowToolTip = 2000;
        // каждые 15 минут проверять работает ли приложение, если нет то сообщать это пользователю, т.к. может быть ошибка которую он не знает.
        private const double _intervalForActivateUnUsedWindow = 900 * 1000;
        // каждый час очистка памяти и очиста логов
        private const double _intervalGCCollectAndClearTraces = 3600 * 1000;
        // количество дней на хранение логов
        private const int _daysToSaveLogs = 10;
        object syncTraces = new object();

        public Thread MainThread { get; private set; }
        private int _openedWarningWindowCount = 0;
        private Timer _timerOnActivateUnUsingWindow;
        private Timer _timerOnGC;
        private WindowWarning warnWindow;
        private bool _thisIsLoaded = false; // фиксит ошибку при закрытии окна остановку таймера
        private DateTime? _lastDeactivationDate = null;

        public TFSControl TfsControl { get; private set; }
        public BottomNotification BottomNotification { get; private set; }
        public ApplicationUpdater AppUpdater { get; private set; }
        public List<TraceHighlighter> Traces { get; private set; } = new List<TraceHighlighter>();
        public RemoteControl RemControl { get; private set; }
        public string CliendID { get; private set; }

        public string CurrentPackUpdaterName
        {
            get
            {
                string _currentVal;
                using (RegeditControl regControl = new RegeditControl(ASSEMBLY.ApplicationName))
                {
                    _currentVal = regControl[nameof(BuildPackInfo)]?.ToString();
                }

                return _currentVal;
            }
            private set
            {
                using (RegeditControl regControl = new RegeditControl(ASSEMBLY.ApplicationName))
                {
                    regControl[nameof(BuildPackInfo)] = value;
                }
            }
        }

        static MainWindow()
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            //Thread.CurrentThread.CurrentCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            //обязательно устанавливаем необходимый формат даты под культуру "ru-RU"
            System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
            System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongDatePattern = "dd.MM.yyyy HH:mm:ss";
        }

        public MainWindow()
        {
            // Проверить запущен ли уже экземпляр приложения, если запущен то не запускать новый
            List<Process> processExists = Process.GetProcesses().Where(p => p.ProcessName == nameof(TFSAssist)).ToList();
            if (processExists.Count > 1)
            {
                warnWindow = new WindowWarning(Width, WarnSeverity.Warning.ToString("G"), ERR_SECOND_PROC)
                {
                    Topmost = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                warnWindow.Focus();
                warnWindow.ShowDialog();
                Process.GetCurrentProcess().Kill();  // принудительно грохаем текущий процесс
                return;
            }

            Resources.Add("STR_START", STR_START);

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            InitializeComponent();

            Title = "TFS Assist";
            Loaded += MainWindow_Loaded;
            Activated += MainWindow_Activated;
            Deactivated += MainWindow_Deactivated;
            Unloaded += MainWindow_Unloaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MainThread = Thread.CurrentThread;

                using (RegeditControl regControl = new RegeditControl(ASSEMBLY.ApplicationName))
                {
                    string clinetID = regControl["CliendID"]?.ToString();
                    if (clinetID.IsNullOrEmptyTrim())
                    {
                        clinetID = STRING.RandomNumbers(5);
                        regControl["CliendID"] = clinetID;
                    }

                    CliendID = clinetID;
                }

                _thisIsLoaded = true;
                //================Notification Bar==============================
                BottomNotification = new BottomNotification(this, TFSControl.ApplicationName);
                //===========Initialize And Set Events===========
                LogPerformer log = new LogPerformer();
                log.WriteLog += Informing;
                TfsControl = TFSControl.GetControl(log);
                TfsControl.IsCompleted += TfsControl_IsCompleted;

                //================Main Tab Default Bindings=======================
                DefaultBinding(MailAddress, TextBox.TextProperty, TfsControl.Settings.MailOption.Address);
                DefaultBinding(MailUserName, TextBox.TextProperty, TfsControl.Settings.MailOption.UserName);
                DefaultBinding(AuthorTimeout, TextBox.TextProperty, TfsControl.Settings.MailOption.AuthorizationTimeout);
                DefaultBinding(FilterFolder, TextBox.TextProperty, TfsControl.Settings.MailOption.SourceFolder);
                DefaultBinding(FilterFrom, TextBox.TextProperty, TfsControl.Settings.MailOption.FilterMailFrom);
                DefaultBinding(FilterSubject, TextBox.TextProperty, TfsControl.Settings.MailOption.FilterSubject);
                DefaultBinding(CreateBoot, ToggleButton.IsCheckedProperty, TfsControl.Settings.BootRun);
                DefaultBinding(IntervalTextBox, TextBox.TextProperty, TfsControl.Settings.Interval);

                AuthorTimeout.TextChanged += IntervalTextBox_TextChanged;
                AuthorTimeout.TextChanged += IntervalTextBox_OnLostFocus;
                IntervalTextBox.TextChanged += IntervalTextBox_TextChanged; // проверям на валидность интервал
                IntervalTextBox.LostFocus += IntervalTextBox_OnLostFocus; // проверям на валидность интервал, где число должно быть больше 1

                //================Main Tap Special Bindings=======================
                MailPassword.Password = TfsControl.Settings.MailOption.Password.Value; // по дефолту нельзя биндить классы паролей, т.к. его свойства защищенные
                MailPassword.PasswordChanged += MailPassword_OnPasswordChanged;
                TfsControl.Settings.MailOption.Password.PropertyChanged += MailPassword_PropertyChanged;
                ToolTipService.SetInitialShowDelay(MailPassword, _timeoutToShowToolTip);

                TFSUserPassword.Password = TfsControl.Settings.TFSOption.TFSUserPassword.Value;
                TFSUserPassword.PasswordChanged += TFSUserPassword_PasswordChanged;
                TfsControl.Settings.TFSOption.TFSUserPassword.PropertyChanged += TFSUserPassword_PropertyChanged;
                ToolTipService.SetInitialShowDelay(TFSUserPassword, _timeoutToShowToolTip);

                FilterStartDate.Text = TfsControl.Settings.MailOption.StartDate.Value; // при изменении все равно вырезается время, остается только дата, по этому тоже биндим по особеному
                FilterStartDate.SelectedDateChanged += FilterStartDate_OnSelectedDateChanged;
                TfsControl.Settings.MailOption.StartDate.PropertyChanged += StartDate_PropertyChanged;
                ToolTipService.SetInitialShowDelay(FilterStartDate, _timeoutToShowToolTip);

                //================Option Mail====================================
                DefaultBinding(MailExchangeUri, TextBox.TextProperty, TfsControl.Settings.MailOption.ExchangeUri);
                //DefaultBinding(SetDebugLogging, ToggleButton.IsCheckedProperty, tfsControl.Settings.MailOption.DebugLogging);

                LableParceSubject.Content = $"Regex pattern for parsing subject of mail ({nameof(TfsControl.Settings.MailOption.ParceSubject)}__*):";
                RegexSubjectParce.Text = TfsControl.Settings.MailOption.ParceSubject[0].Value;
                RegexSubjectParce.TextChanged += RegexSubjectParce_OnTextChanged;
                ToolTipService.SetInitialShowDelay(RegexSubjectParce, _timeoutToShowToolTip);

                LableParceBody.Content = $"Regex pattern for parsing body of mail ({nameof(TfsControl.Settings.MailOption.ParceBody)}__*):";
                RegexBodyParce.Text = TfsControl.Settings.MailOption.ParceBody[0].Value;
                RegexBodyParce.TextChanged += RegexBodyParce_OnTextChanged;
                ToolTipService.SetInitialShowDelay(RegexBodyParce, _timeoutToShowToolTip);

                //================Options TFS===================================
                DefaultBinding(TFSUri, TextBox.TextProperty, TfsControl.Settings.TFSOption.TFSUri);
                DefaultBinding(TFSUserName, TextBox.TextProperty, TfsControl.Settings.TFSOption.TFSUserName);
                Paragraph par = new Paragraph(new Run(TfsControl.Settings.TFSOption.GetDublicateTFS[0].Value))
                {
                    LineHeight = 1
                };
                GetDublicateTFS.Document.Blocks.Clear();
                GetDublicateTFS.Document.Blocks.Add(par);
                GetDublicateTFS.TextChanged += GetDublicateTFS_OnTextChanged;
                ToolTipService.SetInitialShowDelay(GetDublicateTFS, _timeoutToShowToolTip);

                // 1
                PullWindowSaver();
                // 2
                InitializeAppUpdater();
                // 3
                InitializeTimers();
                // 4 Инициализация телеграма должна быть последней, т.к. он ссылается на Updater, если необходимо проверить обновления
                Task.Run(InitializeRemControl);

                //================ Activate button Start if all correct ==============================
                ButtonStart.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Informing(WarnSeverity.Error, DateTime.Now, ex.Message, $"{ex.Message}\r\n{ex.StackTrace}", true);
                DisableWindow();
            }
            finally
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                //ShowMyForm(this, EventArgs.Empty);
            }
        }

        //void TestLogging()
        //{
        //    new Action(TestLogging).BeginInvoke(null, null);
        //    uint index = 0;
        //    while (true)
        //    {
        //        WriteLog(WarnSeverity.Normal, DateTime.Now, $"Trace=[{index++}]");
        //        System.Threading.Thread.Sleep(1);
        //        if(index > 1000)
        //            System.Threading.Thread.Sleep(5000);
        //    }
        //}

        void MainWindow_Activated(object sender, EventArgs e)
        {
            _lastDeactivationDate = null;
            BottomNotification?.Clear();

            if (!ShowInTaskbar)
                ShowInTaskbar = true;
        }

        void MainWindow_Deactivated(object sender, EventArgs e)
        {
            _lastDeactivationDate = DateTime.Now;
            BottomNotification?.Clear();

            Dispatcher?.Invoke(() =>
            {
                if (WindowState == WindowState.Minimized)
                    ShowInTaskbar = false;
            });
        }

        /// <summary>
        /// удаляем все экземпляры
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _thisIsLoaded = false;

            //Обязательно диспоузить, а то в окошке так и будет висеть
            BottomNotification?.Dispose();
            BottomNotification = null;
            //обязательно диспоузить т.к. нужно результат сериализовать и остановить асинронный процесс
            TfsControl?.Dispose();
            //обязательно очистить все временные файлы и дисконнектимся от телеграма
            RemControl?.Dispose();

            //удаляем таймер
            if (_timerOnActivateUnUsingWindow != null)
            {
                _timerOnActivateUnUsingWindow.Enabled = false;
                _timerOnActivateUnUsingWindow.Stop();
                _timerOnActivateUnUsingWindow.Dispose();
            }

            if (_timerOnGC != null)
            {
                _timerOnGC.Enabled = false;
                _timerOnGC.Stop();
                _timerOnGC.Dispose();
            }

            //if (_tlControl != null)
            //{
            //    try
            //    {
            //        Task task = _tlControl.EndTransaction();
            //        task.Wait();
            //    }
            //    catch (Exception)
            //    {

            //    }
            //}
        }

        /// <summary>
        /// Биндим все необходимые свойства с параметрами в Windows форме. Чтобы значения синхронизировались, если изменить свойство в INotifyPropertyChanged
        /// или изменить свойство DependencyProperty в объекте Window формы. 
        /// Это тоже самое как сделать два эвента с измененными свойствами на примере: MailPassword_OnPasswordChanged, MailPassword_PropertyChanged
        /// </summary>
        /// <param name="target"></param>
        /// <param name="dp"></param>
        /// <param name="notify"></param>
        public void DefaultBinding(DependencyObject target, DependencyProperty dp, INotifyPropertyChanged notify)
        {
            ToolTipService.SetInitialShowDelay(target, _timeoutMSECToShowToolTip);
            Binding myBinding = new Binding
            {
                Source = notify,
                // Value свойства класса SettingsValue<T>
                Path = new PropertyPath("Value"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(target, dp, myBinding);
        }

        #region Pull previous saver data of WindowSaver or restart application

        void PullWindowSaver()
        {
            WindowSaver lastUpdate = WindowSaver.Deserialize();
            if (lastUpdate == null)
                return;

            using (lastUpdate)
            {
                try
                {
                    WindowState = lastUpdate.WindowState;
                    ShowInTaskbar = lastUpdate.ShowInTaskbar;

                    if (lastUpdate.Traces != null)
                    {
                        lock (syncTraces)
                        {
                            Traces = lastUpdate.Traces;

                            foreach (TraceHighlighter trace in Traces)
                            {
                                trace.Refresh();
                                LogTextBox.Document.Blocks.Add(trace.GetParagraph());
                            }
                        }
                    }

                    if (lastUpdate.Updater != null)
                    {
                        string updatePackName = lastUpdate.Updater.ProjectBuildPack.Name;
                        CurrentPackUpdaterName = updatePackName;
                        WriteLog(WarnSeverity.Normal, DateTime.Now, string.Format(UPDATE_ON_COMPLETE, updatePackName));
                    }

                    if (lastUpdate.TfsInProgress)
                    {
                        StartStopTfsControl(false);
                    }
                }
                catch (Exception ex)
                {
                    Informing(WarnSeverity.Error, DateTime.Now, ex.Message, $"{ex.Message}\r\n{ex.StackTrace}", true);
                }
            }
        }

        void DoRestartApplication(bool wasInProgress = false)
        {
            // сначала останавливаем рабочий процесс
            if (TfsControl != null && TfsControl.InProgress)
            {
                // после полной остановки wasInProgress будет true
                TfsControl.Stop();
                return;
            }

            WindowSaver saver = null;
            try
            {
                if (_readyUpdateEntity != null)
                {
                    WriteLog(WarnSeverity.Normal, DateTime.Now, UPDATE_ON_START);
                    saver = PrepareToApplicationRestart(wasInProgress, _readyUpdateEntity);
                    AppUpdater.DoUpdate(_readyUpdateEntity); // метод DoUpdate сам убъет текущий процесс, т.к. приложение на этой стадии должно быть перезагружено
                    return;
                }

                if (_restartApplication)
                {
                    WriteLog(WarnSeverity.Normal, DateTime.Now, REMOTE_ON_RESTART);
                    saver = PrepareToApplicationRestart(wasInProgress);
                    CMD.StartApplication(ASSEMBLY.ApplicationPath, 5);
                    Process.GetCurrentProcess().Kill();
                }
            }
            catch (Exception ex)
            {
                if (_readyUpdateEntity != null)
                {
                    WriteLog(WarnSeverity.Error, DateTime.Now, string.Format(UPDATE_ON_EXCEPTION, _readyUpdateEntity?.ProjectBuildPack?.Name, ex.ToString()));

                    if (_readyUpdateEntity.Status != UploaderStatus.Disposed)
                        _readyUpdateEntity.Dispose();

                    _readyUpdateEntity = null;
                    _traceHighUpdateStatus = null;

                    AppUpdater.Refresh();
                }

                _restartApplication = false;

                saver?.Dispose();

                if (BottomNotification == null)
                    Dispatcher?.Invoke(() => BottomNotification = new BottomNotification(this, TFSControl.ApplicationName));

                if (wasInProgress)
                    StartStopTfsControlSafety(false);

                if (RemControl == null || !RemControl.IsEnabled)
                    Task.Run(InitializeRemControl);
            }
        }


        WindowSaver PrepareToApplicationRestart(bool wasInProgress, IUpdater readyEntity = null)
        {
            WindowSaver saver = null;
            Dispatcher?.Invoke(() =>
            {
                if (readyEntity != null)
                    saver = new WindowSaver(readyEntity, WindowState, ShowInTaskbar, Traces, wasInProgress);
                else
                    saver = new WindowSaver(WindowState, ShowInTaskbar, Traces, wasInProgress);
            });

            saver?.Serialize();

            BottomNotification?.Dispose();
            BottomNotification = null;

            if (!wasInProgress)
                TfsControl?.SaveSettings();

            RemControl?.Dispose();

            return saver;
        }

        #endregion

        #region Updater


        const string UPDATE_TRACE_FORMAT = "Downloaded updates [{0}] of [{1}]";
        const string UPDATE_ON_START = "Update processing begins";
        const string UPDATE_ON_COMPLETE = "Update successfully installed. Package=[{0}]";
        const string UPDATE_ON_EXCEPTION = "Update error. Package=[{0}]\r\n{1}";
        private TraceHighlighter _traceHighUpdateStatus;
        private IUpdater _readyUpdateEntity;
        object sync = new object();

        void InitializeAppUpdater()
        {
            AppUpdater = new ApplicationUpdater(Assembly.GetExecutingAssembly(), CurrentPackUpdaterName, _intervalCheckUpdatesSec);
            AppUpdater.OnFetch += AppUpdater_OnFetch;
            AppUpdater.OnUpdate += AppUpdater_OnUpdate;
            AppUpdater.OnProcessingError += AppUpdater_OnProcessingError;
            AppUpdater.Start();
            AppUpdater.CheckUpdates();
        }

        private void AppUpdater_OnFetch(object sender, ApplicationFetchingArgs args)
        {
            if (args == null)
                return;

            if (args.Control == null)
            {
                args.Result = UpdateBuildResult.Cancel;
                return;
            }

            Dispatcher?.Invoke(() =>
            {
                IUpdater updater = args.Control;
                updater.DownloadProgressChanged += Updater_DownloadProgressChanged;
                _traceHighUpdateStatus = WriteLog(WarnSeverity.Normal, DateTime.Now, string.Format(UPDATE_TRACE_FORMAT, updater.UploadedString, updater.TotalString));
                //_runsTraceUpdate = parControl.Inlines.Where(x => x is Run && !x.Name.IsNullOrEmptyTrim()).ToDictionary(p => p.Name, r => (Run) r);
            });
        }

        private void Updater_DownloadProgressChanged(object sender, EventArgs empty)
        {
            if (_traceHighUpdateStatus == null)
                return;

            // нужно в диспатчере т.к. мы обновляем трейс TraceHighlighter, в котором контрольные объекты UI
            Dispatcher?.Invoke(() =>
            {
                try
                {
                    IUpdater updater = (IUpdater)sender;
                    _traceHighUpdateStatus.Refresh(string.Format(UPDATE_TRACE_FORMAT, updater.UploadedString, updater.TotalString));
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }

        private void AppUpdater_OnUpdate(object sender, ApplicationUpdatingArgs args)
        {
            if (args?.Control == null)
            {
                AppUpdater.Refresh();
                return;
            }

            if (args.Control.ProjectBuildPack == null)
            {
                AppUpdater.Refresh();
                return;
            }

            try
            {
                if (args.Control.ProjectBuildPack.NeedRestartApplication)
                {
                    _readyUpdateEntity = args.Control;
                    DoRestartApplication();
                }
                else
                {
                    string updatePackName = args.Control.ProjectBuildPack.Name;
                    CurrentPackUpdaterName = updatePackName;

                    WriteLog(WarnSeverity.Normal, DateTime.Now, UPDATE_ON_START);
                    AppUpdater.DoUpdate(args.Control);
                    WriteLog(WarnSeverity.Normal, DateTime.Now, string.Format(UPDATE_ON_COMPLETE, updatePackName));
                }
            }
            catch (Exception ex)
            {
                WriteLog(WarnSeverity.Error, DateTime.Now, string.Format(UPDATE_ON_EXCEPTION, args.Control?.ProjectBuildPack?.Name, ex.ToString()));
            }
        }

        private void AppUpdater_OnProcessingError(object sender, ApplicationUpdatingArgs args)
        {
            if (args?.Error != null)
                WriteLog(WarnSeverity.Error, DateTime.Now, args.Error.ToString());
        }

        #endregion

        #region Timers for checking unused main window and timer for garbage callect

        void InitializeTimers()
        {
            _timerOnActivateUnUsingWindow = new Timer
            {
                Interval = _intervalForActivateUnUsedWindow
            };
            _timerOnActivateUnUsingWindow.Elapsed += CheckWorking;
            _timerOnActivateUnUsingWindow.AutoReset = false;
            _timerOnActivateUnUsingWindow.Enabled = true;

            _timerOnGC = new Timer
            {
                Interval = _intervalGCCollectAndClearTraces
            };
            _timerOnGC.Elapsed += GarbageCollect;
            _timerOnGC.AutoReset = false;
            _timerOnGC.Enabled = true;
        }

        /// <summary>
        /// Проверяет если долгое время не запущен то будет происходить постоянная активация окна приложения. Т.к. возможно возникла фатальная ошибка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckWorking(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_thisIsLoaded)
                return;

            if (((TfsControl != null && !TfsControl.InProgress) || _openedWarningWindowCount > 0) && _lastDeactivationDate != null)
            {
                TimeSpan timeSpan = DateTime.Now - (DateTime) _lastDeactivationDate;
                if (timeSpan.Days > 0 || (timeSpan.Days == 0 && timeSpan.TotalMilliseconds >= _intervalForActivateUnUsedWindow))
                {
                    ActivateWindow(this, EventArgs.Empty);
                    _timerOnActivateUnUsingWindow.Interval = _intervalForActivateUnUsedWindow;
                }
                else
                {
                    _timerOnActivateUnUsingWindow.Interval = _intervalForActivateUnUsedWindow - timeSpan.TotalMilliseconds;
                }
            }
            else
            {
                _timerOnActivateUnUsingWindow.Interval = _intervalForActivateUnUsedWindow;
            }

            if (_timerOnActivateUnUsingWindow != null && !_timerOnActivateUnUsingWindow.Enabled)
                _timerOnActivateUnUsingWindow.Enabled = true;
        }


        /// <summary>
        /// Удаляем не используемые объекты из памяти
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GarbageCollect(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_thisIsLoaded)
                return;

            try
            {
                lock (syncTraces)
                {
                    // удаляем логи если прошло больше 10 дней
                    List<TraceHighlighter> oldTraces = Traces.Where(p => DateTime.Now.Subtract(p.DateOfTrace).Days > _daysToSaveLogs).ToList();
                    foreach (TraceHighlighter trace in oldTraces)
                    {
                        Traces.Remove(trace);
                        Dispatcher?.BeginInvoke(DispatcherPriority.Normal, new Action(() => LogTextBox.Document.Blocks.Remove(trace.GetParagraph())));
                    }
                }

                GC.Collect();
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                if (_timerOnGC != null && !_timerOnGC.Enabled)
                    _timerOnGC.Enabled = true;
            }
        }

        #endregion

        #region Telegram

        // Обязательно добавить Unmanaged (т.е. библиотеки которые нельзя добавить в проект Visual Studio.) библиотеки в проект.
        // При запуске приложения, он их создаст в папке:
        // C:\Users\User\Local Settings\Temp\Costura\Temp\XXXXX\32\Microsoft.WITDataStore32.dll и C:\Users\User\Local Settings\Temp\Costura\Temp\XXXXX\64\Microsoft.WITDataStore64.dll
        // 
        //
        //Для 32-битного приложения
        //<ItemGroup Condition = "'$(Platform)' == 'x86'" >
        //<EmbeddedResource Include="..\ForeignAssembly\Microsoft.WITDataStore32.dll">
        //    <Link>costura32\Microsoft.WITDataStore32.dll</Link>
        //</EmbeddedResource>
        //<EmbeddedResource Include = "..\ForeignAssembly\Microsoft.Exchange.WebServices.dll" >
        //    <Link>costura32\Microsoft.Exchange.WebServices.dll</Link>
        //</EmbeddedResource>
        //</ItemGroup>
        // и для 64-битного приложения
        //<ItemGroup Condition = "'$(Platform)' == 'x64'" >
        //<EmbeddedResource Include="..\ForeignAssembly\Microsoft.WITDataStore64.dll">
        //    <Link>costura64\Microsoft.WITDataStore64.dll</Link>
        //</EmbeddedResource>
        //<EmbeddedResource Include = "..\ForeignAssembly\Microsoft.Exchange.WebServices.dll" >
        //    <Link>costura64\Microsoft.Exchange.WebServices.dll</Link>
        //</EmbeddedResource>
        //</ItemGroup>
        //
        //Также добавить Unmanagment библиотеки в файл FobyWaves.xml
        //<Costura CreateTemporaryAssemblies='false' DisableCleanup='false' DisableCompression='true' IncludeDebugSymbols='false'>
        //<Unmanaged32Assemblies>
        //    Microsoft.WITDataStore32
        //    Microsoft.Exchange.WebServices
        //</Unmanaged32Assemblies>
        //<Unmanaged64Assemblies>
        //    Microsoft.WITDataStore64
        //    Microsoft.Exchange.WebServices
        //</Unmanaged64Assemblies>
        //</Costura>


        const string REMOTE_ON_RESTART = "Restart application";
        bool _restartApplication = false; // всегда должно быть false, а то приложение будет все время перегружаться при остановки процесса TFScontrol

        async Task InitializeRemControl()
        {
            try
            {
                RemControl = new RemoteControl(MainThread, CliendID, ClientHandle);
                if (await RemControl.Initialize())
                    await RemControl.Run();

                try
                {
                    RemControl?.SendMessage($"{nameof(RemoteControl)} process is terminated. IsEnabled=[{RemControl?.IsEnabled}]");
                }
                catch (Exception)
                {
                    // igonred
                }

                //WriteLog(WarnSeverity.Error, DateTime.Now, $"{nameof(RemoteControl)} IsEnabled=[{RemControl.IsEnabled}]");
            }
            catch (Exception ex)
            {
                try
                {
                    RemControl?.SendMessage($"{nameof(RemoteControl)} process is terminated.\r\n{ex.ToString()}");
                }
                catch (Exception)
                {
                    // igonred
                }

                WriteLog(WarnSeverity.Error, DateTime.Now, ex.ToString());
            }
            finally
            {
                RemControl?.Dispose();
            }
        }

        string ClientHandle(string command)
        {
            switch (command.ToUpper())
            {
                case "LOG":
                    string textLogs = string.Empty;
                    lock (syncTraces)
                    {
                        Dispatcher?.Invoke(() => textLogs = new TextRange(LogTextBox.Document.ContentStart, LogTextBox.Document.ContentEnd).Text);
                    }

                    return textLogs;

                case "UPDATE":
                    AppUpdater?.CheckUpdates();
                    break;

                case "RESTART":
                    _restartApplication = true;
                    DoRestartApplication();
                    break;

                case "TFSCOMM":
                    StartStopTfsControlSafety(false);
                    break;

                case "TFSETT":
                    return TfsControl?.Settings?.GetString();

                case "WINSTATE":
                    var state = Dispatcher?.Invoke(() => WindowState);
                    switch (state)
                    {
                        case WindowState.Minimized:
                            ActivateWindow(this, EventArgs.Empty);
                            break;
                        case WindowState.Maximized:
                        case WindowState.Normal:
                            DeactivateWindow();
                            break;
                    }

                    break;
            }

            return null;
        }

        #endregion

        #region WarningWindow, Logging, NotifyBar, StatusBar

        private void Warn_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                _openedWarningWindowCount++;
                this.Blur();
            });
        }

        private void WarnWindow_Closed(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                _openedWarningWindowCount--;
                ActivateWindow(this, EventArgs.Empty);
                this.UnBlur();
            });
        }

        private void Informing(WarnSeverity severity, DateTime dateLog, string message, string detailMessage, bool lockProcess)
        {
            if (!_thisIsLoaded)
                return;

            switch (severity)
            {
                case WarnSeverity.Status:
                    Dispatcher?.BeginInvoke(DispatcherPriority.Normal, new Action(() => StatusBarInfo.Text = message));
                    WriteLog(WarnSeverity.Normal, dateLog, message, detailMessage);
                    break;
                case WarnSeverity.StatusRegular:
                    Dispatcher?.BeginInvoke(DispatcherPriority.Normal, new Action(() => StatusBarInfo.Text = message));
                    break;

                case WarnSeverity.Error:
                case WarnSeverity.Warning:
                case WarnSeverity.Attention:
                    Dispatcher?.Invoke(() =>
                    {
                        // показывает уведомление при любых уведомлениях, если окно не активно или уже было одно уведомление
                        BottomNotification?.DisplayNotify(severity.ToString("G"), message);

                        if (_openedWarningWindowCount >= 1 || !IsLoaded)
                            return;

                        // Cразу активировать окно только при критических ошибках. При остальных уведомлениях появляется статус бар или по таймеру активируется окно если _openedWarningWindowCount > 0
                        if (severity == WarnSeverity.Error)
                            ActivateWindow(this, EventArgs.Empty);

                        warnWindow = new WindowWarning(Width, severity.ToString("G"), message);
                        warnWindow.Loaded += Warn_Loaded;
                        warnWindow.Closed += WarnWindow_Closed;
                        warnWindow.Focus();
                        warnWindow.Owner = this;

                        if (lockProcess)
                            warnWindow.ShowDialog();
                        else
                            warnWindow.Show();
                    });

                    WriteLog(severity, dateLog, message, detailMessage);
                    break;

                default:
                    WriteLog(severity, dateLog, message, detailMessage);
                    break;
            }
        }

        private void ButtonClearLog_OnClick(object sender, RoutedEventArgs e)
        {
            lock (syncTraces)
            {
                Traces.Clear();
                LogTextBox.Document.Blocks.Clear();
            }
        }

        //void SimpleWriteLog(WarnSeverity severity, string message)
        //{
        //    WriteLog(severity, DateTime.Now, message);
        //}

        /// <summary>
        /// записать лог в отдельном окне
        /// </summary>
        TraceHighlighter WriteLog(WarnSeverity severity, DateTime dateLog, string message, string detailMessage = null)
        {
            if (!_thisIsLoaded || (message.IsNullOrEmpty() && detailMessage.IsNullOrEmpty()))
                return null;

            TraceHighlighter trace = null;

            Dispatcher?.Invoke(() =>
            {
                if (!(SetDebugLogging?.IsChecked == true && severity == WarnSeverity.Debug || severity == WarnSeverity.Normal || severity == WarnSeverity.Error || severity == WarnSeverity.Status))
                    return;

                trace = new TraceHighlighter(dateLog, detailMessage.IsNullOrEmptyTrim() ? message : detailMessage);
                
                lock (syncTraces)
                {
                    Traces.Add(trace);
                    LogTextBox.Document.Blocks.Add(trace.GetParagraph());
                }
            });

            return trace;
        }

        

        #endregion

        private void ButtonStartStop_OnClick(object sender, RoutedEventArgs e)
        {
            StartStopTfsControl();
        }

        void StartStopTfsControlSafety(bool clearLog = true)
        {
            Dispatcher?.Invoke(() => StartStopTfsControl(clearLog));
        }

        void StartStopTfsControl(bool clearLog = true)
        {
            if (TfsControl == null)
                return;

            if (!TfsControl.InProgress)
            {
                TfsControl.SaveSettings();
                TfsControl.Start();
                DisableWindow();
                if (clearLog)
                    ButtonClearLog_OnClick(this, null);
                StatusBarInfo.Text = string.Empty;

                MyProgeressBar.IsIndeterminate = true;
                ProgressBarBlurEffect(MyProgeressBar, false);

                ButtonStart.Content = STR_STOP;
            }
            else
            {
                TfsControl.Stop();
            }
        }

        /// <summary>
        /// процесс завершился
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TfsControl_IsCompleted(object sender, EventArgs e)
        {
            if (!_thisIsLoaded)
                return;

            Dispatcher?.Invoke(() =>
            {
                EnableWindow();

                ProgressBarBlurEffect(MyProgeressBar, true);

                ButtonStart.Content = STR_START;

                // передаем true т.к. если пак с апдейтом был скачан, то updater точно остановил процесс в tfs и значит в TFSASssist процесс после апдейта нужно заново запустить.
                // либо из вне пришла команда на рестарт приложения
                DoRestartApplication(true);
            });
        }

        private double _isPbBlured = 35;
        private double _isPbNormal = 4;
        void ProgressBarBlurEffect(UIElement element, bool isBlur)
        {
            BlurEffect effect = element.Effect as BlurEffect;
            if(effect == null)
                return;

            if (isBlur)
            {
                effect.Changed += Effect_Changed;
                effect.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation(_isPbBlured, TimeSpan.FromSeconds(0.5)));
            }
            else
            {
                effect.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation(_isPbNormal, TimeSpan.FromSeconds(0.5)));
            }
        }

        private void Effect_Changed(object sender, EventArgs e)
        {
            if (((BlurEffect)MyProgeressBar.Effect).Radius == _isPbBlured)
            {
                ProgressBarGrid.Children.Remove(MyProgeressBar);
                MyProgeressBar = new ProgressBar
                {
                    Effect = new BlurEffect()
                    {
                        Radius = 15
                    },
                    BorderThickness = new Thickness(0)
                };
                ProgressBarGrid.Children.Add(MyProgeressBar);
            }
        }

        void DisableWindow()
        {
            MailOptions.IsEnabled = false;
            GridTFSOption.IsEnabled = false;
            IntervalTextBox.IsEnabled = false;
            CreateBoot.IsEnabled = false;
        }

        void EnableWindow()
        {
            MailOptions.IsEnabled = true;
            GridTFSOption.IsEnabled = true;
            IntervalTextBox.IsEnabled = true;
            CreateBoot.IsEnabled = true;
        }

        /// <summary>
        /// обновляем пароль из формы во внутрениие настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MailPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (TfsControl == null)
                return;

            try
            {
                TfsControl.Settings.MailOption.Password.PropertyChanged -= MailPassword_PropertyChanged;
                TfsControl.Settings.MailOption.Password.Value = MailPassword.Password;
            }
            catch (Exception)
            {
                // null
            }
            finally
            {
                TfsControl.Settings.MailOption.Password.PropertyChanged += MailPassword_PropertyChanged;
            }
        }

        /// <summary>
        /// обновляем пароль из формы во внутрениие настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TFSUserPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (TfsControl == null)
                return;

            try
            {
                TfsControl.Settings.TFSOption.TFSUserPassword.PropertyChanged -= TFSUserPassword_PropertyChanged;
                TfsControl.Settings.TFSOption.TFSUserPassword.Value = TFSUserPassword.Password;
            }
            catch (Exception)
            {
                // null
            }
            finally
            {
                TfsControl.Settings.TFSOption.TFSUserPassword.PropertyChanged += TFSUserPassword_PropertyChanged;
            }
        }

        /// <summary>
        /// изменяем дату начала обработки из формы во внутрениие настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterStartDate_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TfsControl == null)
                return;

            try
            {
                TfsControl.Settings.MailOption.StartDate.PropertyChanged -= StartDate_PropertyChanged;
                TfsControl.Settings.MailOption.StartDate.Value = FilterStartDate.SelectedDate.ToString();
            }
            catch (Exception)
            {
                // null
            }
            finally
            {
                TfsControl.Settings.MailOption.StartDate.PropertyChanged += StartDate_PropertyChanged;
            }
        }

        /// <summary>
        /// обновлем пароль из внутренних настроек в форму
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MailPassword_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_thisIsLoaded)
                return;

            Dispatcher?.Invoke(() =>
                               {
                                   try
                                   {
                                       MailPassword.PasswordChanged -= MailPassword_OnPasswordChanged;
                                       MailPassword.Password = TfsControl.Settings.MailOption.Password.Value;
                                   }
                                   catch (Exception)
                                   {
                                       // null
                                   }
                                   finally
                                   {
                                       MailPassword.PasswordChanged += MailPassword_OnPasswordChanged;
                                   }
                               });
        }

        /// <summary>
        /// обновлем пароль из внутренних настроек в форму
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TFSUserPassword_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_thisIsLoaded)
                return;

            Dispatcher?.Invoke(() =>
                               {
                                   try
                                   {
                                       TFSUserPassword.PasswordChanged -= TFSUserPassword_PasswordChanged;
                                       TFSUserPassword.Password = TfsControl.Settings.TFSOption.TFSUserPassword.Value;
                                   }
                                   catch (Exception)
                                   {
                                       // null
                                   }
                                   finally
                                   {
                                       TFSUserPassword.PasswordChanged += TFSUserPassword_PasswordChanged;
                                   }
                               });
        }

        /// <summary>
        /// изменяем дату начала обработки из внутренних настроек в форму
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartDate_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_thisIsLoaded)
                return;

            Dispatcher?.Invoke(() =>
                               {
                                   try
                                   {
                                       FilterStartDate.SelectedDateChanged -= FilterStartDate_OnSelectedDateChanged;
                                       FilterStartDate.Text = TfsControl.Settings.MailOption.StartDate.Value;
                                   }
                                   catch (Exception)
                                   {
                                       // null
                                   }
                                   finally
                                   {
                                       FilterStartDate.SelectedDateChanged += FilterStartDate_OnSelectedDateChanged;
                                   }
                               });
        }

        /// <summary>
        /// Проверяем корректность ввода данных интервала
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox AnyIntervalTextBox = (TextBox) sender;

            int caretIndex = AnyIntervalTextBox.CaretIndex;
            string oldValue = AnyIntervalTextBox.Text;

            INT.GetOnlyNumberWithCaret(ref oldValue, ref caretIndex, 4);

            AnyIntervalTextBox.Text = oldValue;
            AnyIntervalTextBox.CaretIndex = caretIndex;
        }

        private void IntervalTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox AnyIntervalTextBox = (TextBox)sender;

            int interval;
            if (!int.TryParse(AnyIntervalTextBox.Text, out interval))
                interval = 10;
            if (interval < 1)
                interval = 1;
            if (interval > 7200)
                interval = 7200;

            AnyIntervalTextBox.Text = interval.ToString();
        }

        private void RegexSubjectParce_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TfsControl != null)
                TfsControl.Settings.MailOption.ParceSubject[0].Value = RegexSubjectParce.Text;
        }

        private void RegexBodyParce_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TfsControl != null)
                TfsControl.Settings.MailOption.ParceBody[0].Value = RegexBodyParce.Text;
        }

        private void GetDublicateTFS_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TfsControl != null)
            {
                string richText = new TextRange(GetDublicateTFS.Document.ContentStart, GetDublicateTFS.Document.ContentEnd).Text.Trim();
                TfsControl.Settings.TFSOption.GetDublicateTFS[0].Value = richText;
            }
        }

        public void ActivateWindow(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                Activate();

                if (WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;

                //IInputElement focusedElement = FocusManager.GetFocusedElement(this);
                //if (!focusedElement.Focusable)

                if (warnWindow != null && warnWindow.IsLoaded && (!warnWindow.IsActive || !warnWindow.IsFocused))
                    warnWindow.Activate();
                else if (!IsActive || !IsFocused)
                    Activate();
            });
        }

        void DeactivateWindow()
        {
            Dispatcher?.Invoke(() =>
            {
                if (WindowState != WindowState.Minimized)
                    WindowState = WindowState.Minimized;

                MainWindow_Deactivated(this, EventArgs.Empty);
            });
        }

    }
}