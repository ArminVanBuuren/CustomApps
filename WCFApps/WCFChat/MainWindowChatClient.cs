using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WCFChat.Client.ServiceReference1;
using WCFChat.Service;

namespace WCFChat.Client
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class MainWindowChatClient : WindowControl, Service.IChatCallback
    {
        private ChatClient proxy;
        private Cloud cloud;
        private User currentUser;

        public override void JoinToCloud(User initiator, Cloud cloud)
        {
            this.cloud = cloud;
            this.currentUser = initiator;
            OpenOrReopenConnection();
            base.JoinToCloud(initiator, cloud);
        }

        void OpenOrReopenConnection()
        {
            proxy?.Abort();
            InstanceContext context = new InstanceContext(this);
            proxy = new ChatClient(context);
            proxy.Open();
            //proxy.InnerDuplexChannel.Faulted += new EventHandler(InnerDuplexChannel_Faulted);
            //proxy.InnerDuplexChannel.Opened += new EventHandler(InnerDuplexChannel_Opened);
            //proxy.InnerDuplexChannel.Closed += new EventHandler(InnerDuplexChannel_Closed);
        }
        public void ConnectResult(ServerResult result)
        {
            switch (result)
            {
                case ServerResult.SUCCESS:
                    break;
                case ServerResult.FAILURE:
                    break;
                case ServerResult.CloudNotFound:
                    break;
                case ServerResult.NameIsBusy:
                    break;
                case ServerResult.AwaitConfirmation:
                    break;
                case ServerResult.AccessDenied:
                    break;
                case ServerResult.AccessGranted:
                    break;
                case ServerResult.YourRequestInProgress:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }
        public void IsWritingCallback(User client, bool isWriting)
        {
            throw new NotImplementedException();
        }

        public void Receive(Message msg)
        {
            throw new NotImplementedException();
        }

        public void SetPrivilege(User user, ServerPrivelege privelege)
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }
        public void TransferHistory(List<User> users, List<Message> messages)
        {
            throw new NotImplementedException();
        }

        private void HandleProxy()
        {
            if (proxy != null)
            {
                switch (this.proxy.State)
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


        //private delegate void FaultedInvoker();
        //void InnerDuplexChannel_Closed(object sender, EventArgs e)
        //{
        //    if (!window.Dispatcher.CheckAccess())
        //    {
        //        window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FaultedInvoker(HandleProxy));
        //        return;
        //    }
        //    HandleProxy();
        //}

        //void InnerDuplexChannel_Opened(object sender, EventArgs e)
        //{
        //    if (!window.Dispatcher.CheckAccess())
        //    {
        //        window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FaultedInvoker(HandleProxy));
        //        return;
        //    }
        //    HandleProxy();
        //}

        //void InnerDuplexChannel_Faulted(object sender, EventArgs e)
        //{
        //    if (!window.Dispatcher.CheckAccess())
        //    {
        //        window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FaultedInvoker(HandleProxy));
        //        return;
        //    }
        //    HandleProxy();
        //}


    }
}
