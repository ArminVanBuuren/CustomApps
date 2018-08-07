using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using WCFChat.Service;
using Message = WCFChat.Service.Message;

namespace WCFChat.Client
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class MainWindowChatServer : WCFChat.Service.IChat
    {
        object sync = new object();
        public event EventHandler Closing;
        public event EventHandler Unbind;
        public string TransactionID { get; private set; }
        public bool OnClose { get; private set; } = false;
        public bool IsUnbinded { get; set; } = false;
        private MainWindow window;
        private AccessResult OnRemoveOrAccessUser;
        
        public void Show(string transactionId, AccessResult removeOrAcceptUser)
        {
            OnRemoveOrAccessUser = removeOrAcceptUser;
            TransactionID = transactionId;
            window = new MainWindow();
            window.Show();
            window.OnAccessOrRemoveUser += Window_OnAccessOrRemoveUser;
            window.Closing += Window_Closing;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OnClose = true;
            Closing?.Invoke(this, EventArgs.Empty);
        }

        public void Close()
        {
            if (!OnClose)
                window?.Close();
        }

        private void Window_OnAccessOrRemoveUser(string name, string address, bool isRemove)
        {
            lock (sync)
            {
                User user;
                bool result = waitingAccess.TryGetValue(address, out user);
                if (result)
                {
                    waitingAccess.Remove(address);
                    if (isRemove)
                    {
                        OnRemoveOrAccessUser.BeginInvoke(ServerResult.AccessDenied, user, null, null);
                    }
                    else
                    {
                        OnRemoveOrAccessUser.BeginInvoke(ServerResult.AccessGranted, user, null, null);
                    }
                }
                else
                {
                    
                }
            }
        }



        public void IncomingRequestForAccess(User user, string address)
        {
            lock (sync)
            {
                waitingAccess.Add(address, user);
                window.AdminAddUser(user.Name, address);
            }
        }

        public void UnbindCurrentServerFromMain()
        {
            IsUnbinded = true;
            Unbind?.Invoke(this, EventArgs.Empty);
        }


        class UserBindings
        {
            public UserBindings(User user, IChatCallback callback, string address)
            {
                SelfUser = user;
                CallBack = callback;
                Address = address;
            }

            public User SelfUser { get; }
            public IChatCallback CallBack { get; }
            public string Address { get; }
        }

        public List<Message> Messages { get; } = new List<Message>();
        Dictionary<string, UserBindings> acceptedUsers = new Dictionary<string, UserBindings>();
        Dictionary<string, UserBindings> waitingAccess = new Dictionary<string, UserBindings>(StringComparer.CurrentCultureIgnoreCase);
        public IChatCallback CurrentCallback => OperationContext.Current.GetCallbackChannel<IChatCallback>();

        public Service.ServerResult Connect(User newUser)
        {
            lock (sync)
            {
                try
                {
                    if (IsUnbinded)
                        return ServerResult.AccessDenied;

                    RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
                    string address = $"{prop.Address}:{prop.Port}";

                    UserBindings waitUser;
                    waitingAccess.TryGetValue(address, out waitUser);
                    if (waitUser != null)
                        return ServerResult.YourRequestInProgress;

                    UserBindings userAccess;
                    bool result = acceptedUsers.TryGetValue(newUser.Name, out userAccess);

                    if (result)
                    {
                        if (userAccess.SelfUser.GUID == newUser.GUID)
                        {
                            if (((IChannel) userAccess.CallBack).State == CommunicationState.Opened)
                            {
                                userAccess.CallBack.Terminate();
                                acceptedUsers.Remove(newUser.Name);
                            }
                        }
                        else
                        {
                            return ServerResult.NameIsBusy;
                        }
                    }

                    waitingAccess.Add(address, new UserBindings(newUser, CurrentCallback, address));
                    return ServerResult.AwaitConfirmation;
                }
                catch (Exception e)
                {
                    return ServerResult.FAILURE;
                }
            }
        }

        public void Disconnect(User user)
        {
            lock (sync)
            {
                try
                {

                }
                catch (Exception e)
                {

                }
            }
        }

        public void IsWriting(User user, bool isWriting)
        {
            lock (sync)
            {
                try
                {

                }
                catch (Exception e)
                {

                }
            }
        }

        public void Say(Message message)
        {
            lock (sync)
            {
                try
                {

                }
                catch (Exception e)
                {

                }
            }
        }
    }
}
