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
using WCFChat.Client.CS;

namespace WCFChat.Client
{
    /// <summary>
    /// Interaction logic for AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow
    {
        private MainWindow mainWindow;
        public event EventHandler CheckAuthorization;
        GeneratedUser currentUser = null;
        private CS.ChatClient proxy = null;

        public AuthorizationWindow()
        {
            Title = "Authorization";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            Topmost = true;
            Progress.IsIndeterminate = true;
            InitializeComponent();

            Task.Run(() => Initialize()).ContinueWith(SuccessfulConnected, TaskScheduler.FromCurrentSynchronizationContext());
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

        void Initialize()
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
                InstanceContext context = new InstanceContext(this);
                proxy = new CS.ChatClient(context);
                string servicePath = proxy.Endpoint.ListenUri.AbsolutePath;
                string serviceListenPort = proxy.Endpoint.Address.Uri.Port.ToString();
                proxy.Endpoint.Address = new EndpointAddress(@"http://localhost:" + serviceListenPort + servicePath);
                proxy.Open();
                proxy.InnerDuplexChannel.Faulted += new EventHandler(InnerDuplexChannel_Faulted);
                proxy.InnerDuplexChannel.Opened += new EventHandler(InnerDuplexChannel_Opened);
                proxy.InnerDuplexChannel.Closed += new EventHandler(InnerDuplexChannel_Closed);
            }
            catch (Exception e)
            {
                WindowInfo authorizationError = GetWindowNotificationTemplate("Server Error!", e.Message);
                authorizationError.ShowDialog();
                Process.GetCurrentProcess().Kill();
                return;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CheckAuthorization?.Invoke(new KeyValuePair<string, string>(UserName.Text, Password.Text), e);


            if (currentUser == null)
            {

                KeyValuePair<string, string> userData = (KeyValuePair<string, string>) sender;
                currentUser = new GeneratedUser(new User() {
                                                               Name = userData.Key,
                                                               Password = userData.Value,
                                                               GUID = Guid.NewGuid().ToString("D"),
                                                               Time = DateTime.Now
                                                           });
                TryToConnect(currentUser);

            }
            else
            {

                KeyValuePair<string, string> userData = (KeyValuePair<string, string>) sender;
                if (currentUser.MyUser.Name.Equals(userData.Key, StringComparison.CurrentCultureIgnoreCase) && currentUser.MyUser.Password.Equals(userData.Value))
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
                                                                  if (antecedent.Result)
                                                                  {
                                                                      HandleProxy();
                                                                  }
                                                                  else
                                                                  {
                                                                      Dispatcher?.Invoke(() => InfoWarningMessage(string.Format("Username \"{0}\" already exist. Please choose another.", generedUser.MyUser.Name)));
                                                                      LoggningFailed();
                                                                  }
                                                              });
        }

        public void SuccessfulConnected(Task antecedent)
        {
            Dispatcher.Invoke(() =>
                              {
                                  Authorization.Visibility = Visibility.Visible;
                                  ButtonOK.Visibility = Visibility.Visible;
                                  Progress.IsIndeterminate = false;
                              });
        }

        public void WaitWhenLoggning()
        {
            Progress.IsIndeterminate = true;
            UserName.IsEnabled = false;
            Password.IsEnabled = false;
            ButtonOK.IsEnabled = false;
        }

        public void LoggningFailed()
        {
            Dispatcher.Invoke(() =>
                              {
                                  Progress.IsIndeterminate = false;
                                  UserName.IsEnabled = true;
                                  Password.IsEnabled = true;
                                  ButtonOK.IsEnabled = true;
                              });
        }

        public void InfoWarningMessage(string msg)
        {
            ErrorMessage.Visibility = Visibility.Visible;
            ErrorMessage.Text = msg;
            ErrorMessage.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFCB0000");
        }

        private void UserName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ErrorMessage.Visibility == Visibility.Visible)
                ErrorMessage.Visibility = Visibility.Collapsed;
        }

        private void Password_OnTextChanged(object sender, TextChangedEventArgs e)
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
                        this.Visibility = Visibility.Collapsed;
                        mainWindow = new MainWindow(currentUser, proxy);
                        mainWindow.Show();
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
