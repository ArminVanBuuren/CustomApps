using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Utils.UIControls.Main;
using WCFChat.Client.BasicControl;
using WCFChat.Client.ServiceReference1;

namespace WCFChat.Client.BasicControl
{
    class ChatWaiter
    {
        private InstanceContext context;
        public string TransactionId { get; }
        public ChatServiceClient Proxy { get; private set; }
        public User Initiator { get; }
        public Cloud JoinToCloud { get; }

        public ChatWaiter(InstanceContext context, User initiator, Cloud joinToCloud, string transactionId)
        {
            this.context = context;
            TransactionId = transactionId;
            Initiator = initiator;
            JoinToCloud = joinToCloud;

            Proxy = new ChatServiceClient(context);
            Proxy.Open();
            Proxy.InnerDuplexChannel.Faulted += new EventHandler(InnerDuplexChannel_Faulted);
            Proxy.InnerDuplexChannel.Opened += new EventHandler(InnerDuplexChannel_Opened);
            Proxy.InnerDuplexChannel.Closed += new EventHandler(InnerDuplexChannel_Closed);
            Proxy.Connect(initiator);
        }

        void OpenOrReopenConnection()
        {
            Proxy.Abort();
            Proxy = new ChatServiceClient(context);
            Proxy.Open();
            Proxy.InnerDuplexChannel.Faulted += new EventHandler(InnerDuplexChannel_Faulted);
            Proxy.InnerDuplexChannel.Opened += new EventHandler(InnerDuplexChannel_Opened);
            Proxy.InnerDuplexChannel.Closed += new EventHandler(InnerDuplexChannel_Closed);
            Proxy.Connect(Initiator);
        }

        private void HandleProxy()
        {
            if (Proxy != null)
            {
                switch (this.Proxy.State)
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
            HandleProxy();
        }

        void InnerDuplexChannel_Opened(object sender, EventArgs e)
        {
            HandleProxy();
        }

        void InnerDuplexChannel_Faulted(object sender, EventArgs e)
        {
            HandleProxy();
        }
    }

    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class MainWindowChatClient : IChatServiceCallback
    {
        protected object sync = new object();
        private InstanceContext context;
        private UIWindow mainWindow;
        public Dictionary<ChatServiceClient, WindowControl> Clouds { get; } = new Dictionary<ChatServiceClient, WindowControl>();

        public Dictionary<string, ChatWaiter> currentWaiterToCloud = new Dictionary<string, ChatWaiter>();

        public MainWindowChatClient(UIWindow window)
        {
            context = new InstanceContext(this);
            this.mainWindow = window;
        }

        public void JoinToCloud(User initiator, Cloud joinToCloud, string transactionId)
        {
            currentWaiterToCloud.Add(transactionId, new ChatWaiter(context, initiator, joinToCloud, transactionId));
        }

        public void ConnectResult(ServerResult result)
        {
            //ChatWaiter chat;
            //currentWaiterToCloud.TryGetValue()

            //switch (result)
            //{
            //    case ServerResult.AccessGranted:
            //    case ServerResult.SUCCESS:
            //        WindowControl control = new WindowControl(mainWindow, initiator, joinToCloud, transactionId);
            //        Clouds.Add(control);
            //        break;
            //    case ServerResult.CloudNotFound:
            //        break;
            //    case ServerResult.NameIsBusy:
            //        break;    
            //    case ServerResult.AccessDenied:
            //        break;
            //    case ServerResult.AwaitConfirmation:
            //    case ServerResult.YourRequestInProgress:
            //        return;
            //    case ServerResult.FAILURE:
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(result), result, null);
            //}
        }

        public void TransferHistory(User[] users, ChatMessage[] messages)
        {
	        throw new NotImplementedException();
        }

        public void IsWritingCallback(User client, bool isWriting)
        {
            //lock (sync)
            //{
            //    try
            //    {
            //        UserBindings userBind;
            //        if (!GetUserBinding(client, out userBind))
            //            return;

            //        SomeoneUserIsWriting(userBind, isWriting);

            //    }
            //    catch (Exception e)
            //    {

            //    }
            //}
        }

        protected void CurrentUserIsSaying(string msg)
        {
            //Message newMessage = new Message() {
            //                                       Sender = Initiator.User,
            //                                       Content = msg,
            //                                       Time = DateTime.Now
            //                                   };
            //SomeoneUserReceveMessage(newMessage);
            //proxy?.SayAsync(newMessage);
        }

        protected  void CurrentUserIsWriting(bool isWriting)
        {
            //proxy?.IsWritingAsync(Initiator.User, isWriting);
        }

        public void Receive(ChatMessage msg)
        {
            //lock (sync)
            //{
            //    try
            //    {
            //        UserBindings userBind;
            //        if (!GetUserBinding(msg.Sender, out userBind))
            //            return;

            //        SomeoneUserReceveMessage(msg);
            //    }
            //    catch (Exception e)
            //    {

            //    }
            //}
        }

        public void Terminate(Cloud cloud)
        {
            //proxy?.Abort();
        }

        public void TransferHistory(List<User> users, List<ChatMessage> messages)
        {
            //lock (sync)
            //{
            //    try
            //    {
            //        UpdateUserList(users);
            //        if (messages != null)
            //        {
            //            foreach (Message msg in messages)
            //            {
            //                SomeoneUserReceveMessage(msg);
            //            }
            //        }
            //    }
            //    catch (Exception e)
            //    {

            //    }
            //}
        }

        
    }
}
