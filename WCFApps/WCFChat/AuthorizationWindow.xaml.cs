using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using UIControls.Utils;
using WCFChat.Client.ServiceReference1;

namespace WCFChat.Client
{
    /// <summary>
    /// Interaction logic for AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow
    {
        private MainWindow mainWindow;
        GeneratedUser currentUser = null;
        private ChatClient proxy = null;
        //IChat proxy2 = null;

        public AuthorizationWindow():base(true, false)
        {
            InitializeComponent();
            this.Closing += AuthorizationWindow_Closing;
            mainWindow = new MainWindow();
            WorkingProgressBar.IsIndeterminate = true;

            Task.Run(() => Initialize()).ContinueWith(SuccessfulConnected, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void AuthorizationWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindow.Close();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Close();
        }

        WindowInfo GetWindowNotificationTemplate(string title, string message)
        {
            WindowInfo windowError = new WindowInfo(Width, message)
                                     {
                                         Topmost = true,
                                         Title = "Authorization Error!",
                                         WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                                     };
            windowError.Focus();
            return windowError;
        }

        bool Initialize()
        {
            try
            {
                if (File.Exists(MainWindow.AccountStorePath))
                {
                    using (Stream stream = new FileStream(MainWindow.AccountStorePath, FileMode.Open, FileAccess.Read))
                    {
                        currentUser = new BinaryFormatter().Deserialize(stream) as GeneratedUser;
                    }
                }
            }
            catch (Exception)
            {
                File.Delete(MainWindow.AccountStorePath);
            }

            
            try
            {
                
                InstanceContext context = new InstanceContext(mainWindow);
                proxy = new ChatClient(context);
                proxy.Open();
                proxy.InnerDuplexChannel.Faulted += new EventHandler(InnerDuplexChannel_Faulted);
                proxy.InnerDuplexChannel.Opened += new EventHandler(InnerDuplexChannel_Opened);
                proxy.InnerDuplexChannel.Closed += new EventHandler(InnerDuplexChannel_Closed);

                //DuplexChannelFactory<IChat> factory1 = new DuplexChannelFactory<IChat>(context, new WSDualHttpBinding(), new EndpointAddress(new Uri("http://localhost:8040/WPFHost/")));
                //IChat channel = factory1.CreateChannel();

                //channel.Login(new User());

                //InstanceContext context = new InstanceContext(mainWindow);
                //DuplexChannelFactory<IChat> factory = new DuplexChannelFactory<IChat>(context, new WSDualHttpBinding(), @"http://localhost:8040/WPFHost/");
                //proxy2 = factory.CreateChannel();
                //proxy2.Login(new User());

                //proxy = new ChatClient(context);
                //string servicePath = proxy.Endpoint.ListenUri.AbsolutePath;
                //string serviceListenPort = proxy.Endpoint.Address.Uri.Port.ToString();
                //proxy.Endpoint.Address = new EndpointAddress(@"http://localhost:" + serviceListenPort + servicePath);
                //proxy.Open();
                //proxy.InnerDuplexChannel.Faulted += new EventHandler(InnerDuplexChannel_Faulted);
                //proxy.InnerDuplexChannel.Opened += new EventHandler(InnerDuplexChannel_Opened);
                //proxy.InnerDuplexChannel.Closed += new EventHandler(InnerDuplexChannel_Closed);
                return true;
            }
            catch (Exception e)
            {
                ((ICommunicationObject) proxy)?.Abort();

                InfoWarningMessage(e.InnerException != null ? string.Format("Exception when connect to Server!\r\n{0}", e.InnerException.Message) : string.Format("Exception when connect to Server!\r\n{0}", e.Message));
                return false;
            }
        }

        public void SuccessfulConnected(Task<bool> antecedent)
        {
            Dispatcher.Invoke(() =>
                              {
                                  ButtonOK.Visibility = Visibility.Visible;

                                  WorkingProgressBar.Visibility = Visibility.Collapsed;
                                  NotWorkingProgressBar.Visibility = Visibility.Visible;

                                  if (antecedent.Result)
                                  {
                                      Authorization.Visibility = Visibility.Visible;
                                      ButtonOK.Click += ButtonOkOnClickConnect;
                                  }
                                  else
                                      ButtonOK.Click += ButtonOkOnClickClose;
                              });
        }

        private void ButtonOkOnClickClose(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }

        private void ButtonOkOnClickConnect(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
            {
                GeneratedUser newGenUser = new GeneratedUser(new User() {
                                                               Name = UserName.Text,
                                                               Password = Password.Password,
                                                               GUID = Guid.NewGuid().ToString("D"),
                                                               Time = DateTime.Now
                                                           });
                TryToConnect(newGenUser);

            }
            else
            {
                if (currentUser.MyUser.Name.Equals(UserName.Text, StringComparison.CurrentCultureIgnoreCase) && currentUser.MyUser.Password.Equals(Password.Password))
                {
                    TryToConnect(currentUser);
                }
                else
                {
                    InfoWarningMessage("Username or password is incorrect. Please try again.");
                }

            }
        }

        void TryToConnect(GeneratedUser generedUser)
        {
            WaitWhenLoggning();
            proxy.LoginAsync(generedUser.MyUser).ContinueWith((antecedent) =>
                                                              {
                                                                  LoggingCompleted();
                                                                  if (antecedent.Result)
                                                                  {
                                                                      currentUser = generedUser;
                                                                      ShowMainWindow();
                                                                  }
                                                                  else
                                                                  {
                                                                      Dispatcher?.Invoke(() => InfoWarningMessage(string.Format("Username \"{0}\" already exist. Please choose another.", generedUser.MyUser.Name)));   
                                                                  }
                                                              });
        }

        void ShowMainWindow()
        {
            Dispatcher?.Invoke(() =>
                               {
                                   this.Visibility = Visibility.Collapsed;
                                   mainWindow.Show(currentUser, proxy);
                                   mainWindow.Closing += MainWindow_Closing;
                                   this.Closing -= AuthorizationWindow_Closing;
                               });
        }

        public void WaitWhenLoggning()
        {
            WorkingProgressBar.Visibility = Visibility.Visible;
            NotWorkingProgressBar.Visibility = Visibility.Collapsed;
            UserName.IsEnabled = false;
            Password.IsEnabled = false;
            ButtonOK.IsEnabled = false;
        }

        public void LoggingCompleted()
        {
            Dispatcher.Invoke(() =>
                              {
                                  WorkingProgressBar.Visibility = Visibility.Collapsed;
                                  NotWorkingProgressBar.Visibility = Visibility.Visible;
                                  UserName.IsEnabled = true;
                                  Password.IsEnabled = true;
                                  ButtonOK.IsEnabled = true;
                              });
        }

        public void InfoWarningMessage(string msg)
        {
            Dispatcher?.Invoke(() =>
                               {
                                   ErrorMessage.Visibility = Visibility.Visible;
                                   ErrorMessage.Text = msg;
                                   ErrorMessage.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFFF7070");
                               });
        }

        private void UserName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ErrorMessage.Visibility == Visibility.Visible)
                ErrorMessage.Visibility = Visibility.Collapsed;
        }

        private void Password_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ErrorMessage.Visibility == Visibility.Visible)
                ErrorMessage.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// This is the most method I like, it helps us alot
        /// We may can't know when a connection is lost in 
        /// of network failure or service stopped.
        /// And also to maintain performance client doesnt know
        /// that the connection will be lost when hitting the 
        /// disconnect button, but when a session is terminated
        /// this method will be called, and it will handle everything.
        /// </summary>
        private void HandleProxy()
        {
            if (proxy != null)
            {
                switch (this.proxy.State)
                {
                    case CommunicationState.Closed:
                        proxy = null;
                        mainWindow?.Close();
                        break;
                    case CommunicationState.Closing:
                        break;
                    case CommunicationState.Created:
                        break;
                    case CommunicationState.Faulted:
                        proxy.Abort();
                        proxy = null;
                        mainWindow?.Close();
                        break;
                    case CommunicationState.Opened:
                        ShowMainWindow();
                        break;
                    case CommunicationState.Opening:
                        break;
                    default:
                        break;
                }
            }
        }



        //When the communication object turns to fault state it will
        //require another thread to invoke a fault event
        private delegate void FaultedInvoker();
        void InnerDuplexChannel_Closed(object sender, EventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FaultedInvoker(HandleProxy));
                return;
            }
            HandleProxy();
        }

        void InnerDuplexChannel_Opened(object sender, EventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FaultedInvoker(HandleProxy));
                return;
            }
            HandleProxy();
        }

        void InnerDuplexChannel_Faulted(object sender, EventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FaultedInvoker(HandleProxy));
                return;
            }
            HandleProxy();
        }

    }
}
