using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using Utils;
using Utils.Crypto;
using WCFChat.Client.CS;

namespace WCFChat.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : CS.IChatCallback, IDisposable
    {
        internal static string AccountStorePath { get; }
        private CS.ChatClient proxy = null;
        private GeneratedUser localUser = null;
        private WindowInfo authorizationWindow;
        static MainWindow()
        {
            AccountStorePath = Customs.AccountFilePath + ".dat";
        }

        WindowInfo GetWindowNotificationTemplate(string title, string message)
        {
            WindowInfo windowError = new WindowInfo(Width, message) {
                                                                        Topmost = true,
                                                                        Title = "Authorization Error!",
                                                                        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                                                                    };
            windowError.Focus();
            return windowError;
        }

        public MainWindow()
        {
            GeneratedUser possibleUser = null;
            
            try
            {
                if (File.Exists(AccountStorePath))
                {
                    using (Stream stream = new FileStream(AccountStorePath, FileMode.Open, FileAccess.Read))
                    {
                        possibleUser = new BinaryFormatter().Deserialize(stream) as GeneratedUser;
                    }
                }
            }
            catch (Exception)
            {
                File.Delete(AccountStorePath);
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

            authorizationWindow = new WindowInfo
                                    {
                                        Title = "Authorization",
                                        Topmost = true,
                                        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                                    };
            authorizationWindow.Focus();

            if (possibleUser == null)
            {
                authorizationWindow.CheckAuthorization += (sender, args) =>
                                              {
                                                  KeyValuePair<string, string> userData = (KeyValuePair<string, string>) sender;
                                                  possibleUser = new GeneratedUser(new User() {
                                                                                                  Name = userData.Key,
                                                                                                  Password = userData.Value,
                                                                                                  GUID = Guid.NewGuid().ToString("D"),
                                                                                                  Time = DateTime.Now
                                                                                              });
                                                  TryToConnect(possibleUser);
                                              };
            }
            else
            {
                authorizationWindow.CheckAuthorization += (sender, args) =>
                                              {
                                                  KeyValuePair<string, string> userData = (KeyValuePair<string, string>) sender;
                                                  if (possibleUser.MyUser.Name.Equals(userData.Key, StringComparison.CurrentCultureIgnoreCase) && possibleUser.MyUser.Password.Equals(userData.Value))
                                                  {
                                                      TryToConnect(possibleUser);
                                                  }
                                                  else
                                                  {
                                                      authorizationWindow.InfoWarningMessage("Username or password is incorrect. Please try again.");
                                                  }
                                              };
            }
            authorizationWindow.Closed += (sender, args) =>
                              {
                                  if (localUser == null)
                                      this.Close();
                              };
            authorizationWindow.ShowDialog();



            InitializeComponent();
        }

        void TryToConnect(GeneratedUser generedUser)
        {
            authorizationWindow.WaitWhenConnect();
            proxy.LoginAsync(generedUser.MyUser).ContinueWith((antecedent) =>
                                                {
                                                    if (antecedent.Result)
                                                    {
                                                        localUser = generedUser;
                                                        HandleProxy();
                                                    }
                                                    else
                                                    {
                                                        authorizationWindow.Dispatcher?.Invoke(() => authorizationWindow.InfoWarningMessage(string.Format("Username \"{0}\" already exist. Choose another.", generedUser.MyUser.Name)));
                                                    }
                                                });
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

                        break;
                    case CommunicationState.Closing:
                        break;
                    case CommunicationState.Created:
                        break;
                    case CommunicationState.Faulted:
                        proxy.Abort();
                        proxy = null;

                        break;
                    case CommunicationState.Opened:
                        if (authorizationWindow.IsEnabled)
                        {
                            authorizationWindow.Dispatcher?.Invoke(authorizationWindow.Close);
                        }
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


        public Message[] GetAllContentHistory()
        {
            
            throw new NotImplementedException();
        }

        public void IsWritingCallback(CS.Client client)
        {
            throw new NotImplementedException();
        }

        public void Receive(Message msg)
        {
            throw new NotImplementedException();
        }

        public DateTime RefreshClientsAndGetEarlyDataMessage(CS.Client[] clients, bool isGetEarlyMessage)
        {
            throw new NotImplementedException();
        }

        public void RefreshContentHistory(Message[] messages)
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}