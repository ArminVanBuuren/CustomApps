using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using UIControls.MainControl;
using WCFChat.Client.BasicControl;
using WCFChat.Client.ServiceReference1;
using WCFChat.Service;

namespace WCFChat.Client.BasicControl
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class MainWindowChatClient : WindowControl, Service.IChatCallback
    {
        private ChatClient proxy;

        public MainWindowChatClient(UIWindow window) : base(window)
        {
            
        }

        public override void JoinToCloud(User initiator, Cloud jointoCloud)
        {
            OpenOrReopenConnection();
            base.JoinToCloud(initiator, jointoCloud);
        }

        void OpenOrReopenConnection()
        {
            proxy?.Abort();
            InstanceContext context = new InstanceContext(this);
            proxy = new ChatClient(context);
            proxy.Open();
            proxy.InnerDuplexChannel.Faulted += new EventHandler(InnerDuplexChannel_Faulted);
            proxy.InnerDuplexChannel.Opened += new EventHandler(InnerDuplexChannel_Opened);
            proxy.InnerDuplexChannel.Closed += new EventHandler(InnerDuplexChannel_Closed);
            proxy.Connect(Initiator.User);
        }

        public void ConnectResult(ServerResult result)
        {
            switch (result)
            {
                case ServerResult.AccessGranted:
                case ServerResult.SUCCESS:
                    break;
                case ServerResult.CloudNotFound:
                    break;
                case ServerResult.NameIsBusy:
                    break;    
                case ServerResult.AccessDenied:
                    break;
                case ServerResult.AwaitConfirmation:
                case ServerResult.YourRequestInProgress:
                    return;
                case ServerResult.FAILURE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

        public void IsWritingCallback(User client, bool isWriting)
        {
            lock (sync)
            {
                try
                {
                    UserBindings userBind;
                    if (!GetUserBinding(client, out userBind))
                        return;

                    SomeoneUserIsWriting(userBind, isWriting);

                }
                catch (Exception e)
                {

                }
            }
        }

        protected override void CurrentUserIsSaying(string msg)
        {
            Message newMessage = new Message() {
                                                   Sender = Initiator.User,
                                                   Content = msg,
                                                   Time = DateTime.Now
                                               };
            SomeoneUserReceveMessage(newMessage);
            proxy?.SayAsync(newMessage);
        }

        protected override void CurrentUserIsWriting(bool isWriting)
        {
            proxy?.IsWritingAsync(Initiator.User, isWriting);
        }

        public void Receive(Message msg)
        {
            lock (sync)
            {
                try
                {
                    UserBindings userBind;
                    if (!GetUserBinding(msg.Sender, out userBind))
                        return;

                    SomeoneUserReceveMessage(msg);
                }
                catch (Exception e)
                {

                }
            }
        }

        public void Terminate(Cloud cloud)
        {
            proxy?.Abort();
        }

        public void TransferHistory(List<User> users, List<Message> messages)
        {
            lock (sync)
            {
                try
                {
                    UpdateUserList(users);
                    if (messages != null)
                    {
                        foreach (Message msg in messages)
                        {
                            SomeoneUserReceveMessage(msg);
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }
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

        private delegate void FaultedInvoker();
        void InnerDuplexChannel_Closed(object sender, EventArgs e)
        {
            //if (!window.Dispatcher.CheckAccess())
            //{
            //    window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FaultedInvoker(HandleProxy));
            //    return;
            //}
            HandleProxy();
        }

        void InnerDuplexChannel_Opened(object sender, EventArgs e)
        {
            //if (!window.Dispatcher.CheckAccess())
            //{
            //    window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FaultedInvoker(HandleProxy));
            //    return;
            //}
            HandleProxy();
        }

        void InnerDuplexChannel_Faulted(object sender, EventArgs e)
        {
            //if (!window.Dispatcher.CheckAccess())
            //{
            //    window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FaultedInvoker(HandleProxy));
            //    return;
            //}
            HandleProxy();
        }
    }
}
