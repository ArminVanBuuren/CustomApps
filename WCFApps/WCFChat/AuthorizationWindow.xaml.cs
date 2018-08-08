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
using WCFChat.Service;

namespace WCFChat.Client
{
    public delegate void AccessResult(ServerResult result, User user);
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public partial class AuthorizationWindow : IMainContractCallback
    {
        private object sync = new object();
        private MainContractClient mainProxy = null;

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
            try
            {
                OpenOrReopenConnection();
                return true;
            }
            catch (Exception e)
            {
                ((ICommunicationObject)mainProxy)?.Abort();
                mainProxy = null;
                //InfoWarningMessage(e.InnerException != null ? string.Format("Exception when connect to Server!\r\n{0}", e.InnerException.Message) : string.Format("Exception when connect to Server!\r\n{0}", e.Message));
                return false;
            }
        }

        void OpenOrReopenConnection()
        {
            mainProxy?.Abort();
            InstanceContext context = new InstanceContext(this);
            mainProxy = new MainContractClient(context);
            mainProxy.Open();
            mainProxy.InnerDuplexChannel.Faulted += new EventHandler(InnerDuplexChannel_Faulted);
            mainProxy.InnerDuplexChannel.Opened += new EventHandler(InnerDuplexChannel_Opened);
            mainProxy.InnerDuplexChannel.Closed += new EventHandler(InnerDuplexChannel_Closed);
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



        private void Create_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsTransactionOpen())
                return;

            
            if (mainProxy != null)
            {
                IsEnabledWindow = false;
                IAsyncResult asyncResult = new Func<string, string, string>(TryCreateCloud).BeginInvoke(NickName.Text, CloudAddress.Text, new AsyncCallback(TryCreateCloudCallBack), null);
            }
        }

        private ServiceHost host;
        private Cloud currentCloud;
        private User currentUser;
        private string innerTransactionId;
        private MainWindowChatServer innerChatServer;

        
        void IMainContractCallback.RequestForAccess(User user, string address)
        {
            if (innerChatServer != null)
            {
                innerChatServer.IncomingRequestForAccess(user, address);
                
            }
        }

        void RequestForAccessResult(ServerResult result, User user)
        {
            if (mainProxy != null)
            {
                mainProxy.RemoveOrAccessUser(result, user);
            }
        }

        string TryCreateCloud(string nickName, string cloudName)
        {
            try
            {
                innerTransactionId = Guid.NewGuid().ToString("D");
                currentUser = new User {
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

                currentCloud = new Cloud {
                                             Name = cloudName,
                                             Address = host.Description.Endpoints[0].Address.ToString()
                                         };

                mainProxy.CreateCloudAsync(currentUser, currentCloud, innerTransactionId);
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
                                                      innerChatServer.Show(innerTransactionId, currentUser, RequestForAccessResult);
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

            if (mainProxy != null)
            {
                IsEnabledWindow = false;
                currentUser = new User {
                                                             Name = NickName.Text,
                                                             CloudName = CloudAddress.Text,
                                                             GUID = RegeditKey
                                                         };
                mainProxy.GetCloudAsync(currentUser);
            }
        }

        private MainWindowChatClient innerChatClient;
        void IMainContractCallback.GetCloudResult(ServerResult result, Cloud cloud)
        {
            if (!IsEnabledWindow)
            {
                IsEnabledWindow = true;
                switch (result)
                {
                    case ServerResult.CloudNotFound:
                        Informing($"Incoming:GetCloudResult. Cloud not found on MainServer!");
                        break;
                    case ServerResult.FAILURE:
                        Informing($"Incoming:GetCloudResult. MainServer-error when create cloud!");
                        break;
                    case ServerResult.AccessGranted:
                    case ServerResult.SUCCESS:
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

            if (mainProxy != null && sender is MainWindowChatServer)
            {
                IsEnabledWindow = false;
                mainProxy.UnbindAsync(((MainWindowChatServer)sender).TransactionID);
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
                                       WindowInfo windowError = new WindowInfo(isError ? "Error" : "Warning", msg);
                                       windowError.Focus();
                                       windowError.ShowDialog();
                                   }
                               });
        }

        private void HandleProxy()
        {
            if (mainProxy != null)
            {
                switch (this.mainProxy.State)
                {
                    case CommunicationState.Closed:
                        break;
                    case CommunicationState.Closing:
                        break;
                    case CommunicationState.Created:
                        break;
                    case CommunicationState.Faulted:
                        OpenOrReopenConnection();
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
