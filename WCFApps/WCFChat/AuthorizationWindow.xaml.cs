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
using WCFChat.Client.BasicControl;
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
        internal static string RegeditKey => Guid.NewGuid().ToString("D");

        static AuthorizationWindow()
        {
            AccountStorePath = Customs.AccountFilePath + ".dat";
            //RegeditKey = Customs.GetOrSetRegedit(Customs.ApplicationName, "This application create WCF chat client-server or only client to Main foreign server.");
        }

        public AuthorizationWindow() : base(true, false)
        {
            InitializeComponent();
            this.Closing += AuthorizationWindow_Closing;
            WorkingProgressBar.IsIndeterminate = true;
            Task.Run(() => ConnectToMainServer()).ContinueWith(ConnectionCompleted, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void AuthorizationWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainServerControl?.Close();
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
                ((ICommunicationObject) mainProxy)?.Abort();
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


        private ServiceHost host;
        private Cloud currentCloud;
        private User currentUser;
        private string innerTransactionId;
        private WindowControl mainServerControl;
        private WindowControl mainClientControl;

        private void Create_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsTransactionOpen())
                return;

            IsEnabledWindow = false;

            if (mainServerControl == null)
            {
                mainServerControl = new MainWindowChatServer();
                mainServerControl.Closing += MainWindow_Closing;
                mainServerControl.Unbind += MainChatServer_Unbind;
            }

            IAsyncResult asyncResult = new Func<string, string, bool, string>(TryCreateCloud).BeginInvoke(NickName.Text, CloudAddress.Text, mainProxy == null, new AsyncCallback(TryCreateCloudCallBack), null);
        }

        private void MainChatServer_Unbind(object sender, EventArgs e)
        {
            if (mainProxy != null && sender is MainWindowChatServer)
            {
                mainProxy.UnbindAsync(((MainWindowChatServer) sender).TransactionID);
            }
        }

        string TryCreateCloud(string nickName, string cloudName, bool withoutMainServer)
        {
            try
            {
                if (withoutMainServer)
                {
                    OpenLocalHost(nickName, null);
                    CreateLocalCloud();
                }
                else
                {
                    OpenLocalHost(nickName, cloudName);
                    innerTransactionId = Guid.NewGuid().ToString("D");
                    mainProxy.CreateCloudAsync(currentUser, currentCloud, innerTransactionId);
                }

                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        void OpenLocalHost(string nickName, string cloudName)
        {
            if (host == null)
            {
                host = new ServiceHost(mainServerControl);
                host.Open();
            }
            else if (host.State != CommunicationState.Opened)
            {
                host.Open();
            }

            currentUser = new User {
                                       Name = nickName,
                                       CloudName = cloudName,
                                       GUID = RegeditKey
                                   };

            currentCloud = new Cloud {
                                         Name = cloudName,
                                         Address = host.Description.Endpoints[0].Address.ToString()
                                     };
        }

        void TryCreateCloudCallBack(IAsyncResult asyncResult)
        {
            AsyncResult ar = asyncResult as AsyncResult;
            Func<string, string, bool, string> caller = (Func<string, string, bool, string>) ar.AsyncDelegate;
            string exceptionResult = caller.EndInvoke(asyncResult);

            if (exceptionResult.IsNullOrEmpty())
                return;

            IsEnabledWindow = true;
            innerTransactionId = string.Empty;
            Informing(exceptionResult);
        }

        void IMainContractCallback.CreateCloudResult(CloudResult result, string transactionID)
        {
            if (IsEnabledWindow)
            {
                Informing($"Incoming:CreateCloudResult. Request is not expected from MainServer! Result:{result:F}");
                return;
            }
            try
            {
                if (mainServerControl == null)
                {
                    Informing($"Internal Error when create Cloud '{currentCloud?.Name}'. There is no reference exist for create local client-server.");
                }

                if (transactionID.Equals(innerTransactionId))
                {
                    switch (result)
                    {
                        case CloudResult.SUCCESS:
                            CreateLocalCloud();
                            break;
                        case CloudResult.CloudIsBusy:
                            Informing($"Cloud '{currentCloud?.Name}' is busy! Choose another name.");
                            break;
                        case CloudResult.CloudNotFound:
                        case CloudResult.FAILURE:
                            Informing($"Unknown MainServer-Error when create Cloud '{currentCloud?.Name}'.");
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Informing($"Unexpected error when create Cloud '{currentCloud?.Name}'. Incoming TransactionID from MainServer Not Equals current TransactionID! Result:{result:F}");
                }
            }
            catch (Exception e)
            {
                Informing(e.Message);
            }
            finally
            {
                IsEnabledWindow = true;
            }
        }

        void CreateLocalCloud()
        {
            Dispatcher?.Invoke(() =>
                               {
                                   this.Visibility = Visibility.Collapsed;
                                   mainServerControl.CreateCloud(currentUser, currentCloud, innerTransactionId, RequestForAccessResult);
                               });
        }

        void IMainContractCallback.RequestForAccess(User user, string address)
        {
            if (mainServerControl != null)
            {
                mainServerControl.IncomingRequestForAccess(user, address);
            }
            else
            {
                RequestForAccessResult(ServerResult.CloudNotFound, user);
            }
        }

        void RequestForAccessResult(ServerResult result, User user)
        {
            mainProxy?.RemoveOrAccessUser(result, user);
        }



        private void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsTransactionOpen())
                return;

            IsEnabledWindow = false;

            currentUser = new User {
                                       Name = NickName.Text,
                                       CloudName = CloudAddress.Text,
                                       GUID = RegeditKey
                                   };

            if (mainClientControl == null)
            {
                mainClientControl = new MainWindowChatServer();
                mainClientControl.Closing += MainWindow_Closing;
            }

            if (mainProxy != null)
            {

                mainProxy.GetCloudAsync(currentUser);
            }
            else
            {
                currentCloud = new Cloud {
                                             Name = null,
                                             Address = CloudAddress.Text
                                         };

                Task.Run(() =>
                         {
                             try
                             {
                                 mainClientControl.JoinToCloud(currentUser, currentCloud);
                                 return string.Empty;
                             }
                             catch (Exception ex)
                             {
                                 return ex.Message;
                             }
                         }).ContinueWith((antecedent) =>
                                         {
                                             if (antecedent.Result.IsNullOrEmpty())
                                             {
                                                 this.Visibility = Visibility.Collapsed;
                                             }
                                             else
                                             {
                                                 Informing(antecedent.Result);
                                             }
                                         }, TaskScheduler.FromCurrentSynchronizationContext());

            }
        }

        void IMainContractCallback.GetCloudResult(ServerResult result, Cloud cloud)
        {
            if (IsEnabledWindow)
            {
                Informing($"Incoming:GetCloudResult. Request is not expected from MainServer! Result:{result:F}");
                return;
            }
            try
            {
                switch (result)
                {
                    case ServerResult.AccessGranted:
                    case ServerResult.SUCCESS:
                        mainClientControl.JoinToCloud(currentUser, cloud);
                        break;
                    case ServerResult.CloudNotFound:
                        Informing($"Incoming:GetCloudResult. Cloud '{currentUser?.CloudName}' not found on MainServer!");
                        break;
                    case ServerResult.NameIsBusy:
                        Informing($"Incoming:GetCloudResult. Nick name '{currentUser?.Name}' in Cloud '{currentUser?.CloudName}' is busy! Try again.");
                        break;
                    case ServerResult.AccessDenied:
                        Informing($"Incoming:GetCloudResult. Access is denied for access to the Cloud '{currentUser?.CloudName}'.");
                        break;
                    case ServerResult.AwaitConfirmation:
                    case ServerResult.YourRequestInProgress:
                        Informing($"Incoming:GetCloudResult. Your request for access to the Cloud '{currentUser?.CloudName}' in progress.");
                        return;
                    case ServerResult.FAILURE:
                        Informing($"Incoming:GetCloudResult. MainServer-error when get Cloud '{currentUser?.CloudName}'!");
                        break;
                    default:
                        Informing($"Incoming:GetCloudResult. Result:{result}!");
                        break;
                }
                IsEnabledWindow = true;
            }
            catch (Exception ex)
            {
                Informing(ex.Message);
                IsEnabledWindow = true;
            }
        }

        bool IsTransactionOpen()
        {
            if (!IsEnabledWindow)
            {
                Informing($"Await! Another process doesn't complete.");
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