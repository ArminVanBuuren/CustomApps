using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
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
using WCFChat.Client.ServiceReference1;

namespace WCFChat.Client
{
    /// <summary>
    /// Interaction logic for StartApp.xaml
    /// </summary>
    public partial class StartApp
    {
        private bool isClosing = false;
        private MainWindow mainWin;
        private MainContractClient mainProxy = null;
        public StartApp()
        {
            InitializeComponent();

            mainWin = new MainWindow();
            mainWin.Closing += Main_Closing;

            this.Closing += StartApp_Closing;
            Task.Run(() => ConnectToMainServer()).ContinueWith(ConnectionCompleted, TaskScheduler.FromCurrentSynchronizationContext());
        }

        bool ConnectToMainServer()
        {
            try
            {
                OpenOrReopenConnection();
                return true;
            }
            catch (Exception)
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
            InstanceContext context = new InstanceContext(mainWin);
            mainProxy = new MainContractClient(context);
            mainProxy.Open();
            mainProxy.InnerDuplexChannel.Faulted += new EventHandler(InnerDuplexChannel_Faulted);
            mainProxy.InnerDuplexChannel.Opened += new EventHandler(InnerDuplexChannel_Opened);
            mainProxy.InnerDuplexChannel.Closed += new EventHandler(InnerDuplexChannel_Closed);
            mainWin.UpdateProxy(mainProxy);
        }

        public void ConnectionCompleted(Task<bool> antecedent)
        {
            Dispatcher.Invoke(() =>
            {
                if (antecedent.Result)
                {
                    mainWin.Show();
                    this.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ErrorMessage.Visibility = Visibility.Visible;
                    WorkingProgressBar.Visibility = Visibility.Collapsed;
                    Title = "Error!";
                    ErrorMessage.Text = "Failed when connect to Mainserver";
                }
            });
        }

        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Close();
        }

        private void StartApp_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
            mainProxy?.Abort();
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
                        if (!isClosing)
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
