using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using TFSAssist.Control;
using Utils;
using Timer = System.Timers.Timer;

namespace TFSAssist
{

    public class LogFlowDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((double) value < 1800)
                return 1800;
            return (double) value - 28;
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
        private const string STR_START = "START";
        private const string STR_STOP = "STOP";
        private const string ERR_SECOND_PROC = " already started. Please check your notification area. To run second process, you can rename the executable file.";

        private const int timeoutMSECToShowToolTip = 2700;
        private const int timeoutToShowToolTip = 2700;

        private const int timerToCollectAndActivateUnUsedWindow = 120 * 1000;
        private int _openedWarningWindowCount = 0;
        private System.Windows.Forms.NotifyIcon notification;
        private Timer _timerActivateWindow;
        private Timer _timerGC;
        private WindowWarning warnWindow;
        private bool _thisIsLoaded = false; // фиксит ошибку при закрытии окна остановку таймера
        private DateTime? LastDeactivationDate = null;

        private TFSControl tfsControl { get; set; }

        public MainWindow()
        {
            Resources.Add("STR_START", STR_START);

            // Проверить запущен ли уже экземпляр приложения, если запущен то не запускать новый
            List<Process> processExists = Process.GetProcesses().Where(p => p.ProcessName == nameof(TFSAssist)).ToList();
            if (processExists.Count > 1)
            {
                warnWindow = new WindowWarning(Width, WarnSeverity.Warning.ToString("G"), $"{nameof(TFSAssist)}{ERR_SECOND_PROC}");
                warnWindow.Topmost = true;
                warnWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                warnWindow.Focus();
                warnWindow.ShowDialog();
                Process.GetCurrentProcess().Kill();  // принудительно грохаем текущий процесс
                return;
            }

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            InitializeComponent();

            Title = nameof(TFSAssist);
            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
            Activated += MainWindow_Activated;
            Deactivated += MainWindow_Deactivated;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _thisIsLoaded = true;
                //================Notification Bar==============================
                notification = new System.Windows.Forms.NotifyIcon
                               {
                                   BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info,
                                   Icon = Properties.Resources.Rick,
                                   Visible = true
                               };
                notification.BalloonTipClicked += ShowMyForm;
                notification.DoubleClick += ShowMyForm;

                //===========Initialize And Set Events===========
                TFSControl.NotifyUserAnError += NotifyUser; //static event
                tfsControl = TFSControl.GetControl();
                tfsControl.NotifyUserAnStatus += NotifyStatus;
                tfsControl.IsCompleted += TfsControl_IsCompleted;
                tfsControl.WriteLog += AddLog;

                //================Main Tab Default Bindings=======================
                DefaultBinding(MailAddress, TextBox.TextProperty, tfsControl.Settings.MailOption.Address);
                DefaultBinding(MailUserName, TextBox.TextProperty, tfsControl.Settings.MailOption.UserName);
                DefaultBinding(AuthorTimeout, TextBox.TextProperty, tfsControl.Settings.MailOption.AuthorizationTimeout);
                DefaultBinding(FilterFolder, TextBox.TextProperty, tfsControl.Settings.MailOption.SourceFolder);
                DefaultBinding(FilterFrom, TextBox.TextProperty, tfsControl.Settings.MailOption.FilterMailFrom);
                DefaultBinding(FilterSubject, TextBox.TextProperty, tfsControl.Settings.MailOption.FilterSubject);
                DefaultBinding(CreateBoot, ToggleButton.IsCheckedProperty, tfsControl.Settings.BootRun);
                DefaultBinding(IntervalTextBox, TextBox.TextProperty, tfsControl.Settings.Interval);

                AuthorTimeout.TextChanged += IntervalTextBox_TextChanged;
                AuthorTimeout.TextChanged += IntervalTextBox_OnLostFocus;
                IntervalTextBox.TextChanged += IntervalTextBox_TextChanged; // проверям на валидность интервал
                IntervalTextBox.LostFocus += IntervalTextBox_OnLostFocus; // проверям на валидность интервал, где число должно быть больше 1

                //================Main Tap Special Bindings=======================
                MailPassword.Password = tfsControl.Settings.MailOption.Password.Value; // по дефолту нельзя биндить классы паролей, т.к. его свойства защищенные
                MailPassword.PasswordChanged += MailPassword_OnPasswordChanged;
                tfsControl.Settings.MailOption.Password.PropertyChanged += MailPassword_PropertyChanged;
                ToolTipService.SetInitialShowDelay(MailPassword, timeoutToShowToolTip);

                TFSUserPassword.Password = tfsControl.Settings.TFSOption.TFSUserPassword.Value;
                TFSUserPassword.PasswordChanged += TFSUserPassword_PasswordChanged;
                tfsControl.Settings.TFSOption.TFSUserPassword.PropertyChanged += TFSUserPassword_PropertyChanged;
                ToolTipService.SetInitialShowDelay(TFSUserPassword, timeoutToShowToolTip);

                FilterStartDate.Text =
                        tfsControl.Settings.MailOption.StartDate.Value; // при зменении все равно вырезается время, остается только дата, по этому тоже биндим по особеному
                FilterStartDate.SelectedDateChanged += FilterStartDate_OnSelectedDateChanged;
                tfsControl.Settings.MailOption.StartDate.PropertyChanged += StartDate_PropertyChanged;
                ToolTipService.SetInitialShowDelay(FilterStartDate, timeoutToShowToolTip);

                //================Option Mail====================================
                DefaultBinding(MailExchangeUri, TextBox.TextProperty, tfsControl.Settings.MailOption.ExchangeUri);
                DefaultBinding(SetDebugLogging, ToggleButton.IsCheckedProperty, tfsControl.Settings.MailOption.DebugLogging);

                LableParceSubject.Content = nameof(tfsControl.Settings.MailOption.ParceSubject) + ":";
                RegexSubjectParce.Text = tfsControl.Settings.MailOption.ParceSubject[0].Value;
                RegexSubjectParce.TextChanged += RegexSubjectParce_OnTextChanged;
                ToolTipService.SetInitialShowDelay(RegexSubjectParce, timeoutToShowToolTip);

                LableParceBody.Content = nameof(tfsControl.Settings.MailOption.ParceBody) + ":";
                RegexBodyParce.Text = tfsControl.Settings.MailOption.ParceBody[0].Value;
                RegexBodyParce.TextChanged += RegexBodyParce_OnTextChanged;
                ToolTipService.SetInitialShowDelay(RegexBodyParce, timeoutToShowToolTip);

                //================Options TFS===================================
                DefaultBinding(TFSUri, TextBox.TextProperty, tfsControl.Settings.TFSOption.TFSUri);
                DefaultBinding(TFSUserName, TextBox.TextProperty, tfsControl.Settings.TFSOption.TFSUserName);
                Paragraph par = new Paragraph(new Run(tfsControl.Settings.TFSOption.GetDublicateTFS[0].Value)) {
                                                                                                                   LineHeight = 1
                                                                                                               };
                GetDublicateTFS.Document.Blocks.Add(par);
                GetDublicateTFS.TextChanged += GetDublicateTFS_OnTextChanged;
                ToolTipService.SetInitialShowDelay(GetDublicateTFS, timeoutToShowToolTip);


                //================Activate Button Start And Start Timer==============================
                ButtonStart.IsEnabled = true;

                _timerActivateWindow = new Timer {
                                                     Interval = timerToCollectAndActivateUnUsedWindow
                                                 };
                _timerActivateWindow.Elapsed += CheckWorking;
                _timerActivateWindow.AutoReset = false;
                _timerActivateWindow.Enabled = true;

                _timerGC = new Timer {
                                         Interval = 300 * 1000
                                     };
                _timerGC.Elapsed += _timerCollect;
                _timerGC.Start();
            }
            catch (Exception ex)
            {
                NotifyUser(WarnSeverity.Error, ex.Message, $"{ex.Message}{Environment.NewLine}{ex.StackTrace}", true);
                DisableWindow();
            }
            finally
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                ShowMyForm(this, EventArgs.Empty);
            }
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
            if (_timerActivateWindow != null)
            {
                _timerActivateWindow.Enabled = false;
                _timerActivateWindow.Stop();
                _timerActivateWindow.Dispose();
            }

            if (_timerGC != null)
            {
                _timerGC.Enabled = false;
                _timerGC.Stop();
                _timerGC.Dispose();
            }

            //Обязательно диспоузить, а то в окошке так и будет висеть
            notification?.Dispose();
            //обязательно диспоузить т.к. нужно результат сериализовать и остановить асинронный процесс
            tfsControl?.Dispose();
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

        /// <summary>
        /// Проверяет если долгое время не запущен то будет происходить постоянная активация окна приложения. Т.к. возможно возникла фатальная ошибка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckWorking(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_thisIsLoaded)
                return;

            if (((tfsControl != null && !tfsControl.InProgress) || _openedWarningWindowCount > 0) && LastDeactivationDate != null)
            {
                TimeSpan timeSpan = DateTime.Now - (DateTime) LastDeactivationDate;
                if (timeSpan.Days > 0 || (timeSpan.Days == 0 && timeSpan.TotalMilliseconds >= timerToCollectAndActivateUnUsedWindow))
                {
                    ShowMyForm(this, EventArgs.Empty);
                    _timerActivateWindow.Interval = timerToCollectAndActivateUnUsedWindow;
                }
                else
                {
                    _timerActivateWindow.Interval = timerToCollectAndActivateUnUsedWindow - timeSpan.TotalMilliseconds;
                }
            }
            else
            {
                _timerActivateWindow.Interval = timerToCollectAndActivateUnUsedWindow;
            }

            if (_timerActivateWindow != null)
                _timerActivateWindow.Enabled = true;
        }

        /// <summary>
        /// Удаляем не используемые объекты из памяти
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timerCollect(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_thisIsLoaded)
                return;

            GC.Collect();
        }

        #region Notify And Set Information On StatusBar

        /// <summary>
        /// Показать статус работы программы
        /// </summary>
        /// <param name="message"></param>
        void NotifyStatus(string message)
        {
            if (!_thisIsLoaded)
                return;

            Dispatcher?.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(() =>
                    StatusBarInfo.Content = message
                ));
        }

        /// <summary>
        /// При фатальныхошибках выводить сообщение в новом окне, записать подробный лог и активировать окно
        /// </summary>
        /// <param name="error"></param>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        /// <param name="lockProcess"></param>
        void NotifyUser(WarnSeverity error, string message, string stackTrace, bool lockProcess)
        {
            if (!_thisIsLoaded)
                return;

            Dispatcher?.Invoke(() =>
            {
                AddLog(stackTrace);

                // показывает уведомление при любых уведомлениях, если окно не активно или уже было одно уведомление
                DisplayNotify(error.ToString("G"), message);

                if (_openedWarningWindowCount >= 1 || !this.IsLoaded)
                    return;

                // Cразу активировать окно только при критических ошибках. При остальных уведомлениях появляется статус бар или по таймеру активируется окно если _openedWarningWindowCount > 0
                if (error == WarnSeverity.Error)
                    ShowMyForm(this, EventArgs.Empty);

                warnWindow = new WindowWarning(Width, error.ToString("G"), message);
                warnWindow.Loaded += Warn_Loaded;
                warnWindow.Closed += WarnWindow_Closed;
                warnWindow.Focus();
                warnWindow.Owner = this;

                if (lockProcess)
                    warnWindow.ShowDialog();
                else
                    warnWindow.Show();

            });
        }

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

        /// <summary>
        /// записать лог в отдельном окне
        /// </summary>
        /// <param name="stackTrace"></param>
        void AddLog(string stackTrace)
        {
            if (!_thisIsLoaded)
                return;

            if (!stackTrace.IsNullOrEmpty())
            {
                Dispatcher?.Invoke(() =>
                {
                    //LogTextBox.AppendText(stackTrace.Trim());
                    //LogTextBox.Document.Blocks.Add(new Paragraph(new Run(stackTrace.Trim())));

                    //Run _runDate = new Run(string.Format("[{0:G}]:", DateTime.Now));
                    //_runDate.Foreground = Brushes.Azure;
                    //_runDate.Background = Brushes.Chartreuse;
                    //Run _runLog = new Run(stackTrace.Trim());

                    Paragraph par = new Paragraph();
                    par.Inlines.Add(new Bold(new Run(string.Format("[{0:G}]:", DateTime.Now)))
                    {
                        Foreground = Brushes.Aqua,
                        Background = Brushes.Black
                    });
                    par.Inlines.Add(stackTrace.Trim());


                    par.LineHeight = 1;
                    LogTextBox.Document.Blocks.Add(par);
                });
            }
        }



        uint countNotWatchedNotifications = 0;

        /// <summary>
        /// Показать сообщение в уведомлениях Windows
        /// </summary>
        /// <param name="messageHeader"></param>
        /// <param name="messageDetails"></param>
        void DisplayNotify(string messageHeader, string messageDetails)
        {
            if (IsActive || messageHeader.IsNullOrEmpty() || messageDetails.IsNullOrEmpty() || countNotWatchedNotifications != 0)
                return;

            
            notification.Text = TFSControl.ApplicationName;
            notification.Visible = true;
            notification.BalloonTipTitle = messageHeader;
            notification.BalloonTipText = messageDetails;
            notification.ShowBalloonTip(100);
            countNotWatchedNotifications++;
        }



        void MainWindow_Activated(object sender, EventArgs e)
        {
            LastDeactivationDate = null;
            countNotWatchedNotifications = 0;

            if (!ShowInTaskbar)
                ShowInTaskbar = true;
        }


        void MainWindow_Deactivated(object sender, EventArgs e)
        {
            LastDeactivationDate = DateTime.Now;
            countNotWatchedNotifications = 0;

            if (WindowState == WindowState.Minimized)
                ShowInTaskbar = false;
        }

        #endregion

        /// <summary>
        /// запуск процесса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (!tfsControl.InProgress)
            {
                tfsControl.Start();
                DisableWindow();
                LogTextBox.Document.Blocks.Clear();
                StatusBarInfo.Content = string.Empty;
                MyProgeressBar.IsIndeterminate = true;
                ButtonStart.Content = STR_STOP;
            }
            else
            {
                tfsControl.Stop();
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

                MyProgeressBar.IsIndeterminate = false;
                MyProgeressBar.Visibility = Visibility.Collapsed;
                ProgressBarGrid.Children.Remove(MyProgeressBar);
                MyProgeressBar = new ProgressBar
                {
                    Margin = new Thickness(10)
                };
                ProgressBarGrid.Children.Add(MyProgeressBar);

                ButtonStart.Content = STR_START;
            });
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
            if (tfsControl == null)
                return;

            try
            {
                tfsControl.Settings.MailOption.Password.PropertyChanged -= MailPassword_PropertyChanged;
                tfsControl.Settings.MailOption.Password.Value = MailPassword.Password;
            }
            catch (Exception)
            {
                // null
            }
            finally
            {
                tfsControl.Settings.MailOption.Password.PropertyChanged += MailPassword_PropertyChanged;
            }
        }

        /// <summary>
        /// обновляем пароль из формы во внутрениие настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TFSUserPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (tfsControl == null)
                return;

            try
            {
                tfsControl.Settings.TFSOption.TFSUserPassword.PropertyChanged -= TFSUserPassword_PropertyChanged;
                tfsControl.Settings.TFSOption.TFSUserPassword.Value = TFSUserPassword.Password;
            }
            catch (Exception)
            {
                // null
            }
            finally
            {
                tfsControl.Settings.TFSOption.TFSUserPassword.PropertyChanged += TFSUserPassword_PropertyChanged;
            }
        }

        /// <summary>
        /// изменяем дату начала обработки из формы во внутрениие настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterStartDate_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tfsControl == null)
                return;

            try
            {
                tfsControl.Settings.MailOption.StartDate.PropertyChanged -= StartDate_PropertyChanged;
                tfsControl.Settings.MailOption.StartDate.Value = FilterStartDate.SelectedDate.ToString();
            }
            catch (Exception)
            {
                // null
            }
            finally
            {
                tfsControl.Settings.MailOption.StartDate.PropertyChanged += StartDate_PropertyChanged;
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
                                       MailPassword.Password = tfsControl.Settings.MailOption.Password.Value;
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
                                       TFSUserPassword.Password = tfsControl.Settings.TFSOption.TFSUserPassword.Value;
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
                                       FilterStartDate.Text = tfsControl.Settings.MailOption.StartDate.Value;
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

            Customs.GetOnlyNumberWithCaret(ref oldValue, ref caretIndex, 4);

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
            if (tfsControl != null)
                tfsControl.Settings.MailOption.ParceSubject[0].Value = RegexSubjectParce.Text;
        }

        private void RegexBodyParce_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (tfsControl != null)
                tfsControl.Settings.MailOption.ParceBody[0].Value = RegexBodyParce.Text;
        }

        private void GetDublicateTFS_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (tfsControl != null)
            {
                string richText = new TextRange(GetDublicateTFS.Document.ContentStart, GetDublicateTFS.Document.ContentEnd).Text.Trim();
                tfsControl.Settings.TFSOption.GetDublicateTFS[0].Value = richText;
            }
        }

        void ShowMyForm(object sender, EventArgs e)
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