using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
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
using Utils;
using WCFChat.Client.ServiceReference1;
using WCFChat.Client.ServiceReference2;


namespace WCFChat.Client
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public partial class AuthorizationWindow : IMainContractCallback
    {
        private object sync = new object();
        private MainContractClient proxyMainServer = null;

        private MainContractClient MainProxy
        {
            get
            {
                try
                {
                    if (proxyMainServer == null)
                        return null;

                    if (proxyMainServer.State != CommunicationState.Opened)
                        proxyMainServer.Open();
                }
                catch (Exception e)
                {
                    Informing(e.Message);
                }
                return null;
            }
        }
        internal static string AccountStorePath { get; }
        internal static string RegeditKey { get; }

        static AuthorizationWindow()
        {
            AccountStorePath = Customs.AccountFilePath + ".dat";
            RegeditKey = Customs.GetOrSetRegedit(Customs.ApplicationName, "This application create WCF chat client-server or only client to Main foreign server.");
        }
        public AuthorizationWindow():base(true, false)
        {
            InitializeComponent();
            this.Closing += AuthorizationWindow_Closing;
            WorkingProgressBar.IsIndeterminate = true;
            Task.Run(() => ConnectToMainServer()).ContinueWith(ConnectionCompleted, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void AuthorizationWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            innerChatServer?.Close();
        }

        private void MainWindow_Closing(object sender, EventArgs e)
        {
            this.Close();
        }

 

        bool ConnectToMainServer()
        {
            //try
            //{
            //    if (File.Exists(MainWindow.AccountStorePath))
            //    {
            //        using (Stream stream = new FileStream(MainWindow.AccountStorePath, FileMode.Open, FileAccess.Read))
            //        {
            //            currentUser = new BinaryFormatter().Deserialize(stream) as GeneratedUser;
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //    File.Delete(MainWindow.AccountStorePath);
            //}


            try
            {
                InstanceContext context = new InstanceContext(this);
                proxyMainServer = new MainContractClient(context);
                proxyMainServer.Open();
                proxyMainServer.InnerDuplexChannel.Faulted += new EventHandler(InnerDuplexChannel_Faulted);
                proxyMainServer.InnerDuplexChannel.Opened += new EventHandler(InnerDuplexChannel_Opened);
                proxyMainServer.InnerDuplexChannel.Closed += new EventHandler(InnerDuplexChannel_Closed);
                return true;
            }
            catch (Exception e)
            {
                ((ICommunicationObject) proxyMainServer)?.Abort();
                proxyMainServer = null;
                //InfoWarningMessage(e.InnerException != null ? string.Format("Exception when connect to Server!\r\n{0}", e.InnerException.Message) : string.Format("Exception when connect to Server!\r\n{0}", e.Message));
                return false;
            }
        }

        public void ConnectionCompleted(Task<bool> antecedent)
        {
            Dispatcher.Invoke(() =>
                              {
                                  AuthorizationBlock.Visibility = Visibility.Visible;
                                  WorkingProgressBar.Visibility = Visibility.Collapsed;
                                  NotWorkingProgressBar.Visibility = Visibility.Visible;

                                  if (!antecedent.Result)
                                      Informing("Failed when connect to main server.");

                              });
        }


        private void UserName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ErrorMessage.Visibility == Visibility.Visible)
                ErrorMessage.Visibility = Visibility.Collapsed;
        }
        private void CloudAddress_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ErrorMessage.Visibility == Visibility.Visible)
                ErrorMessage.Visibility = Visibility.Collapsed;
        }



        void IMainContractCallback.RequestForAccess(ServiceReference1.User user, string address)
        {
            throw new NotImplementedException();
        }




        private void Create_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsTransactionOpen())
                return;

            
            if (MainProxy != null)
            {
                IsEnabledWindow = false;
                IAsyncResult asyncResult = new Func<string, string, string>(TryCreateCloud).BeginInvoke(NickName.Text, CloudAddress.Text, new AsyncCallback(TryCreateCloudCallBack), null);
            }
        }

        private ServiceHost host;
        private ServiceReference1.Cloud currentCloud;
        private ServiceReference1.User currentUser;
        private string innerTransactionId;
        private MainWindowChatServer innerChatServer;

        string TryCreateCloud(string nickName, string cloudName)
        {
            try
            {
                innerTransactionId = Guid.NewGuid().ToString("D");
                currentUser = new ServiceReference1.User {
                                                             Name = nickName,
                                                             CloudName = cloudName,
                                                             GUID = RegeditKey
                                                         };

                if (host == null)
                {
                    innerChatServer = new MainWindowChatServer();
                    innerChatServer.Closing += MainWindow_Closing;
                    innerChatServer.Unbind += MainChatServer_Unbind;
                    host = new ServiceHost(innerChatServer);
                    host.Open();
                }
                else if (host.State != CommunicationState.Opened)
                {
                    host.Open();
                }

                currentCloud = new ServiceReference1.Cloud {
                                                               Name = cloudName,
                                                               Address = host.Description.Endpoints[0].Address.ToString()
                                                           };

                MainProxy.CreateCloudAsync(currentUser, currentCloud, innerTransactionId);
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        void TryCreateCloudCallBack(IAsyncResult asyncResult)
        {
            AsyncResult ar = asyncResult as AsyncResult;
            Func<string, string, string> caller = (Func<string, string, string>)ar.AsyncDelegate;
            string sumResult = caller.EndInvoke(asyncResult);
            if (!sumResult.IsNullOrEmpty())
            {
                IsEnabledWindow = true;
                innerTransactionId = string.Empty;
                Informing(sumResult);
            }
        }

        void IMainContractCallback.CreateCloudResult(CloudResult result, string transactionID)
        {
            try
            {
                if (!IsEnabledWindow)
                {
                    IsEnabledWindow = true;
                    if (innerChatServer == null)
                    {
                        Informing($"Incoming:CreateCloudResult. Internal Error! There was no request to create a local server.");
                    }
                    if (transactionID.Equals(innerTransactionId))
                    {
                        switch (result)
                        {
                            case CloudResult.SUCCESS:
                                Dispatcher?.Invoke(() =>
                                                  {
                                                      this.Visibility = Visibility.Collapsed;
                                                      innerChatServer.Show(innerTransactionId);
                                                  });
                                break;
                            case CloudResult.FAILURE:
                                Informing($"Incoming:CreateCloudResult. MainServer-error when create cloud!");
                                break;
                            case CloudResult.CloudIsBusy:
                                Informing($"Incoming:CreateCloudResult. This cloud is busy!");
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        Informing($"Incoming:CreateCloudResult. MainServer transactionID Not Equals current transactionID! Result:{result:F}");
                    }
                }
                else
                {
                    Informing($"Incoming:CreateCloudResult. Request was not expected from MainServer! Result:{result:F}");
                }
            }
            catch (Exception e)
            {
                Informing(e.Message);
            }
        }

       
        private void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsTransactionOpen())
                return;

            if (MainProxy != null)
            {
                IsEnabledWindow = false;
                currentUser = new ServiceReference1.User {
                                                             Name = NickName.Text,
                                                             CloudName = CloudAddress.Text,
                                                             GUID = RegeditKey
                                                         };
                MainProxy.GetCloudAsync(currentUser);
            }
        }

        private MainWindowChatClient innerChatClient;
        void IMainContractCallback.GetCloudResult(CloudResult result, Cloud cloud)
        {
            if (!IsEnabledWindow)
            {
                IsEnabledWindow = true;
                switch (result)
                {
                    case CloudResult.CloudNotFound:
                        Informing($"Incoming:GetCloudResult. Cloud not found on MainServer!");
                        break;
                    case CloudResult.FAILURE:
                        Informing($"Incoming:GetCloudResult. MainServer-error when create cloud!");
                        break;
                    case CloudResult.SUCCESS:
                        innerChatClient = new MainWindowChatClient(cloud);
                        
                        break;
                    default:
                        Informing($"Incoming:GetCloudResult. Result:{result}!");
                        break;
                }
            }
        }

        private void MainChatServer_Unbind(object sender, EventArgs e)
        {
            if (!IsTransactionOpen())
                return;

            if (MainProxy != null && sender is MainWindowChatServer)
            {
                IsEnabledWindow = false;
                MainProxy.UnbindAsync(((MainWindowChatServer)sender).TransactionID);
            }
        }

        void IMainContractCallback.UnbindResult(CloudResult result, string transactionID)
        {
            try
            {
                if (!IsEnabledWindow)
                {
                    IsEnabledWindow = true;
                    if (innerChatServer != null && transactionID.Equals(innerChatServer.TransactionID))
                    {
                        innerChatServer.IsUnbinded = true;
                    }
                    else
                    {
                        Informing($"Incoming:UnbindResult. Internal Error! Local server Not Created.");
                    }
                }
                else
                {
                    Informing($"Incoming:UnbindResult. Request was not expected from MainServer! Result:{result:F}");
                }
            }
            catch (Exception e)
            {
                Informing(e.Message);
            }
        }



        bool IsTransactionOpen()
        {
            if (!IsEnabledWindow)
            {
                Informing($"Wait! Another process doesn't complete.");
                return false;
            }
            return true;
        }

        private bool _isEnabledWindow = true;

        private bool IsEnabledWindow
        {
            get => _isEnabledWindow;
            set
            {
                lock (sync)
                    _isEnabledWindow = value;
                if (value)
                {
                    Dispatcher?.Invoke(() =>
                                       {
                                           AuthorizationBlock.IsEnabled = true;
                                           WorkingProgressBar.Visibility = Visibility.Collapsed;
                                           NotWorkingProgressBar.Visibility = Visibility.Visible;
                                       });
                }
                else
                {
                    Dispatcher?.Invoke(() =>
                                       {
                                           AuthorizationBlock.IsEnabled = false;
                                           WorkingProgressBar.Visibility = Visibility.Visible;
                                           NotWorkingProgressBar.Visibility = Visibility.Collapsed;
                                       });
                }
            }
        }





        public void Informing(string msg, bool isError = true)
        {
            Dispatcher?.Invoke(() =>
                               {
                                   if (this.Visibility == Visibility.Visible)
                                   {
                                       IsEnabledWindow = true;
                                       ErrorMessage.Visibility = Visibility.Visible;
                                       ErrorMessage.Text = msg;
                                       ErrorMessage.Foreground = isError ? (Brush) new BrushConverter().ConvertFrom("#FFFF7070") : (Brush) new BrushConverter().ConvertFrom("#FF00FF0C");
                                   }
                                   else
                                   {
                                       GetWindowNotificationTemplate(isError ? "Error" : "Warning", msg).ShowDialog();
                                   }
                               });
        }

        WindowInfo GetWindowNotificationTemplate(string title, string message)
        {
            WindowInfo windowError = new WindowInfo(Width, message)
                                     {
                                         Topmost = true,
                                         Title = title,
                                         WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                                     };
            windowError.Focus();
            return windowError;
        }

        //private void ButtonOkOnClickConnect(object sender, RoutedEventArgs e)
        //{
        //    if (currentUser == null)
        //    {
        //        GeneratedUser newGenUser = new GeneratedUser(new User() {
        //                                                       Name = UserName.Text,
        //                                                       Password = Password.Password,
        //                                                       GUID = Guid.NewGuid().ToString("D"),
        //                                                       Time = DateTime.Now
        //                                                   });
        //        TryToConnect(newGenUser);
        //    }
        //    else
        //    {
        //        if (currentUser.MyUser.Name.Equals(UserName.Text, StringComparison.CurrentCultureIgnoreCase) && currentUser.MyUser.Password.Equals(Password.Password))
        //        {
        //            TryToConnect(currentUser);
        //        }
        //        else
        //        {
        //            InfoWarningMessage("Username or password is incorrect. Please try again.");
        //        }

        //    }
        //}

        //void TryToConnect(GeneratedUser generedUser)
        //{
        //    WaitWhenLoggning();
        //    proxy.LoginAsync(generedUser.MyUser).ContinueWith((antecedent) =>
        //                                                      {
        //                                                          LoggingCompleted();
        //                                                          if (antecedent.Result)
        //                                                          {
        //                                                              currentUser = generedUser;
        //                                                              ShowMainWindow();
        //                                                          }
        //                                                          else
        //                                                          {
        //                                                              Dispatcher?.Invoke(() => InfoWarningMessage(string.Format("Username \"{0}\" already exist. Please choose another.", generedUser.MyUser.Name)));   
        //                                                          }
        //                                                      });
        //}

        //void ShowMainWindow()
        //{
        //    Dispatcher?.Invoke(() =>
        //                       {
        //                           this.Visibility = Visibility.Collapsed;
        //                           mainWindow.Show(currentUser, proxy);
        //                           mainWindow.Closing += MainWindow_Closing;
        //                           this.Closing -= AuthorizationWindow_Closing;
        //                       });
        //}

        //public void WaitWhenLoggning()
        //{
        //    WorkingProgressBar.Visibility = Visibility.Visible;
        //    NotWorkingProgressBar.Visibility = Visibility.Collapsed;
        //    UserName.IsEnabled = false;
        //    Password.IsEnabled = false;
        //    ButtonOK.IsEnabled = false;
        //}

        //public void LoggingCompleted()
        //{
        //    Dispatcher.Invoke(() =>
        //                      {
        //                          WorkingProgressBar.Visibility = Visibility.Collapsed;
        //                          NotWorkingProgressBar.Visibility = Visibility.Visible;
        //                          UserName.IsEnabled = true;
        //                          Password.IsEnabled = true;
        //                          ButtonOK.IsEnabled = true;
        //                      });
        //}






        private void HandleProxy()
        {
            if (MainProxy != null)
            {
                switch (this.MainProxy.State)
                {
                    case CommunicationState.Closed:
                        break;
                    case CommunicationState.Closing:
                        break;
                    case CommunicationState.Created:
                        break;
                    case CommunicationState.Faulted:
                        break;
                    case CommunicationState.Opened:
                        break;
                    case CommunicationState.Opening:
                        break;
                    default:
                        break;
                }
            }
        }


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
