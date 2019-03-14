using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using TFSAssist.Control;
using UIControls.MainControl;
using Utils;
using Utils.AppUpdater;
using Timer = System.Timers.Timer;

namespace TFSAssist
{
    public class LogFlowDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((double) value < 2000)
                return 2000;
            return (double) value - 25;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <inheritdoc cref="" />
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //ApplicationUpdater _updater = new ApplicationUpdater();

        private const string STR_START = "START";
        private const string STR_STOP = "STOP";
        private readonly string ERR_SECOND_PROC = $"{nameof(TFSAssist)} already started. Please check your notification area. To run second process, you can rename the executable file.";

        private const int timeoutMSECToShowToolTip = 2000;
        private const int timeoutToShowToolTip = 2000;

        private const int _intervalForActivateUnUsedWindow = 600 * 1000;
        private int _openedWarningWindowCount = 0;
        
        private Timer _timerActivateUnUsedWindow;
        private Timer _timerGC;
        private WindowWarning warnWindow;
        private bool _thisIsLoaded = false; // фиксит ошибку при закрытии окна остановку таймера
        private DateTime? LastDeactivationDate = null;
        public TFSControl TfsControl { get; private set; }
        private BottomNotification _bottomNotification;
        static MainWindow()
        {
            System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
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
                _thisIsLoaded = true;
                //================Notification Bar==============================
                _bottomNotification = new BottomNotification(this, TFSControl.ApplicationName);
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
                ToolTipService.SetInitialShowDelay(MailPassword, timeoutToShowToolTip);

                TFSUserPassword.Password = TfsControl.Settings.TFSOption.TFSUserPassword.Value;
                TFSUserPassword.PasswordChanged += TFSUserPassword_PasswordChanged;
                TfsControl.Settings.TFSOption.TFSUserPassword.PropertyChanged += TFSUserPassword_PropertyChanged;
                ToolTipService.SetInitialShowDelay(TFSUserPassword, timeoutToShowToolTip);

                FilterStartDate.Text =
                        TfsControl.Settings.MailOption.StartDate.Value; // при зменении все равно вырезается время, остается только дата, по этому тоже биндим по особеному
                FilterStartDate.SelectedDateChanged += FilterStartDate_OnSelectedDateChanged;
                TfsControl.Settings.MailOption.StartDate.PropertyChanged += StartDate_PropertyChanged;
                ToolTipService.SetInitialShowDelay(FilterStartDate, timeoutToShowToolTip);

                //================Option Mail====================================
                DefaultBinding(MailExchangeUri, TextBox.TextProperty, TfsControl.Settings.MailOption.ExchangeUri);
                //DefaultBinding(SetDebugLogging, ToggleButton.IsCheckedProperty, tfsControl.Settings.MailOption.DebugLogging);

                LableParceSubject.Content = $"Regex pattern for parsing subject of mail ({nameof(TfsControl.Settings.MailOption.ParceSubject)}__*):";
                RegexSubjectParce.Text = TfsControl.Settings.MailOption.ParceSubject[0].Value;
                RegexSubjectParce.TextChanged += RegexSubjectParce_OnTextChanged;
                ToolTipService.SetInitialShowDelay(RegexSubjectParce, timeoutToShowToolTip);

                LableParceBody.Content = $"Regex pattern for parsing body of mail ({nameof(TfsControl.Settings.MailOption.ParceBody)}__*):";
                RegexBodyParce.Text = TfsControl.Settings.MailOption.ParceBody[0].Value;
                RegexBodyParce.TextChanged += RegexBodyParce_OnTextChanged;
                ToolTipService.SetInitialShowDelay(RegexBodyParce, timeoutToShowToolTip);

                //================Options TFS===================================
                DefaultBinding(TFSUri, TextBox.TextProperty, TfsControl.Settings.TFSOption.TFSUri);
                DefaultBinding(TFSUserName, TextBox.TextProperty, TfsControl.Settings.TFSOption.TFSUserName);
                Paragraph par = new Paragraph(new Run(TfsControl.Settings.TFSOption.GetDublicateTFS[0].Value)) {
                                                                                                                   LineHeight = 1
                                                                                                               };
                GetDublicateTFS.Document.Blocks.Add(par);
                GetDublicateTFS.TextChanged += GetDublicateTFS_OnTextChanged;
                ToolTipService.SetInitialShowDelay(GetDublicateTFS, timeoutToShowToolTip);


                //================Activate Button Start And Start Timer==============================
                ButtonStart.IsEnabled = true;

                _timerActivateUnUsedWindow = new Timer {
                                                     Interval = _intervalForActivateUnUsedWindow
                                                 };
                _timerActivateUnUsedWindow.Elapsed += CheckWorking;
                _timerActivateUnUsedWindow.AutoReset = false;
                _timerActivateUnUsedWindow.Enabled = true;

                _timerGC = new Timer {
                                         Interval = 900 * 1000
                                     };
                _timerGC.Elapsed += GarbageCollect;
                _timerGC.Start();
            }
            catch (Exception ex)
            {
                Informing(WarnSeverity.Error, DateTime.Now,  ex.Message, $"{ex.Message}\r\n{ex.StackTrace}", true);
                DisableWindow();
            }
            finally
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                ShowMyForm(this, EventArgs.Empty);
            }
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
            ToolTipService.SetInitialShowDelay(target, timeoutMSECToShowToolTip);
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

        void MainWindow_Activated(object sender, EventArgs e)
        {
            LastDeactivationDate = null;
            _bottomNotification.Clear();

            if (!ShowInTaskbar)
                ShowInTaskbar = true;
        }

        void MainWindow_Deactivated(object sender, EventArgs e)
        {
            LastDeactivationDate = DateTime.Now;
            _bottomNotification.Clear();

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
            //удаляем таймер
            if (_timerActivateUnUsedWindow != null)
            {
                _timerActivateUnUsedWindow.Enabled = false;
                _timerActivateUnUsedWindow.Stop();
                _timerActivateUnUsedWindow.Dispose();
            }

            if (_timerGC != null)
            {
                _timerGC.Enabled = false;
                _timerGC.Stop();
                _timerGC.Dispose();
            }

            //Обязательно диспоузить, а то в окошке так и будет висеть
            _bottomNotification?.Dispose();
            //обязательно диспоузить т.к. нужно результат сериализовать и остановить асинронный процесс
            TfsControl?.Dispose();
        }

        #region Timers for checking unused main window and timer for garbage callect

        /// <summary>
        /// Проверяет если долгое время не запущен то будет происходить постоянная активация окна приложения. Т.к. возможно возникла фатальная ошибка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckWorking(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_thisIsLoaded)
                return;

            if (((TfsControl != null && !TfsControl.InProgress) || _openedWarningWindowCount > 0) && LastDeactivationDate != null)
            {
                TimeSpan timeSpan = DateTime.Now - (DateTime) LastDeactivationDate;
                if (timeSpan.Days > 0 || (timeSpan.Days == 0 && timeSpan.TotalMilliseconds >= _intervalForActivateUnUsedWindow))
                {
                    ShowMyForm(this, EventArgs.Empty);
                    _timerActivateUnUsedWindow.Interval = _intervalForActivateUnUsedWindow;
                }
                else
                {
                    _timerActivateUnUsedWindow.Interval = _intervalForActivateUnUsedWindow - timeSpan.TotalMilliseconds;
                }
            }
            else
            {
                _timerActivateUnUsedWindow.Interval = _intervalForActivateUnUsedWindow;
            }

            if (_timerActivateUnUsedWindow != null)
                _timerActivateUnUsedWindow.Enabled = true;
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

            GC.Collect();
        }

        #endregion

        #region WarningWindow, Logging, NotifyBar, StatusBar

        private void Warn_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                _openedWarningWindowCount++;
                IsBlured = true;
            });
        }

        private void WarnWindow_Closed(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                _openedWarningWindowCount--;
                ShowMyForm(this, EventArgs.Empty);
                IsBlured = false;
            });
        }

        private void Informing(WarnSeverity severity, DateTime dateLog, string message, string stackMessage, bool lockProcess)
        {
            if (!_thisIsLoaded)
                return;

            switch (severity)
            {
                case WarnSeverity.Status:
                    Dispatcher?.BeginInvoke(DispatcherPriority.Normal, new Action(() => StatusBarInfo.Text = message));
                    WriteLog(WarnSeverity.Normal, dateLog, message, stackMessage);
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
                        _bottomNotification.DisplayNotify(severity.ToString("G"), message);

                        if (_openedWarningWindowCount >= 1 || !this.IsLoaded)
                            return;

                        // Cразу активировать окно только при критических ошибках. При остальных уведомлениях появляется статус бар или по таймеру активируется окно если _openedWarningWindowCount > 0
                        if (severity == WarnSeverity.Error)
                            ShowMyForm(this, EventArgs.Empty);

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

                    WriteLog(severity, dateLog, message, stackMessage);
                    break;

                default:
                    WriteLog(severity, dateLog, message, stackMessage);
                    break;
            }
        }

        private void ButtonClearLog_OnClick(object sender, RoutedEventArgs e)
        {
            LogTextBox.Document.Blocks.Clear();
        }

        /// <summary>
        /// записать лог в отдельном окне
        /// </summary>
        /// <param name="stackTrace"></param>
        void WriteLog(WarnSeverity severity, DateTime dateLog, string message, string stackMessage = null)
        {
            if (message.IsNullOrEmpty() && stackMessage.IsNullOrEmpty())
                return;

            Dispatcher?.Invoke(() =>
            {
                if (!(SetDebugLogging?.IsChecked == true && severity == WarnSeverity.Debug || severity == WarnSeverity.Normal || severity == WarnSeverity.Error || severity == WarnSeverity.Status))
                    return;

                Paragraph par = new Paragraph();
                par.Inlines.Add(new Bold(new Run(string.Format("[{0:G}]:", dateLog)))
                {
                    Foreground = Brushes.Aqua,
                    Background = Brushes.Black
                });

                if (!stackMessage.IsNullOrEmpty())
                {
                    Highlighter.Traces(par, stackMessage);
                }
                else
                {
                    Highlighter.Traces(par, message);
                }

                par.LineHeight = 0.1;
                LogTextBox.Document.Blocks.Add(par);
            });
        }

        #endregion

        private void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (!TfsControl.InProgress)
            {
                TfsControl.Start();
                DisableWindow();
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
            });
        }

        private double _isPbBlured = 35;
        private double _isPbNormal = 4;
        void ProgressBarBlurEffect(UIElement element, bool isBlur)
        {
            BlurEffect effect = element.Effect as BlurEffect;
            
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

        public void ShowMyForm(object sender, EventArgs e)
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



    }
}