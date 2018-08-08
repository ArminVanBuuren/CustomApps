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
        private User admin;

        public void Show(string transactionId, User sourceUser, AccessResult removeOrAcceptUser)
        {
            OnRemoveOrAccessUser = removeOrAcceptUser;
            TransactionID = transactionId;
            lock (sync)
            {
                admin = new User() {
                                                GUID = "ADMIN",
                                                Name = sourceUser.Name,
                                                CloudName = sourceUser.CloudName
                                            };
                acceptedUsers.Add("ADMIN", new UserBindings(admin, null, ".$my_localhost::", string.Empty));
                
                window = new MainWindow();
                window.Show();
                window.OnAccessOrRemoveUser += Window_OnAccessOrRemoveUser;
                window.OnUserWriting += AdminIsWriting;
                window.OnUserSay += AdminSay;
                window.Closing += Window_Closing;
                window.AddUser(admin, "localhost");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            lock (sync)
            {
                OnClose = true;
                Closing?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Close()
        {
            lock (sync)
            {
                if (!OnClose)
                    window?.Close();
            }
        }


        public void UnbindCurrentServerFromMain()
        {
            IsUnbinded = true;
            Unbind?.Invoke(this, EventArgs.Empty);
        }


        class UserBindings
        {
            public UserBindings(User user, IChatCallback callback, string address, string port)
            {
                SelfUser = user;
                CallBack = callback;
                Address = address;
                Port = port;
            }

            public User SelfUser { get; }
            public IChatCallback CallBack { get; set; }
            public string Address { get; }
            public string Port { get; }
        }

        public List<Message> Messages { get; } = new List<Message>();
        Dictionary<string, UserBindings> acceptedUsers = new Dictionary<string, UserBindings>(StringComparer.CurrentCulture);
        Dictionary<string, UserBindings> waitingAccess = new Dictionary<string, UserBindings>(StringComparer.CurrentCulture);
        
        public IChatCallback CurrentCallback => OperationContext.Current.GetCallbackChannel<IChatCallback>();
        public bool CurrentCallbackIsOpen => ((IChannel)CurrentCallback).State == CommunicationState.Opened;

        /// <summary>
        /// Main сервер запрашивает можно ли новому узеру присоединить к текущему облаку
        /// </summary>
        /// <param name="user"></param>
        /// <param name="address"></param>
        public void IncomingRequestForAccess(User user, string address)
        {
            lock (sync)
            {
                if (user == null || string.IsNullOrEmpty(user.GUID) || string.IsNullOrEmpty(user.Name))
                {
                    if (CurrentCallbackIsOpen)
                        CurrentCallback.ConnectResult(ServerResult.AccessDenied);
                    return;
                }

                string[] addr = address.Split(':');
                AddWaiter(user, null, addr[0], addr[1]);
            }
        }

        void AddWaiter(User newUser, IChatCallback callback, string address, string port)
        {
            waitingAccess.Add(newUser.GUID, new UserBindings(newUser, callback, address, port));
            window.Admin_AddUser(newUser, $"{address}:{port}");
        }

        void RemoveAcceptedUser(User user)
        {
            acceptedUsers.Remove(user.GUID);
            window.RemoveUser(user);
        }

        void UpdateUserList(UserBindings createdUser)
        {
            List<User> allUsers = acceptedUsers.Values.Where(p => p.CallBack != null).Select(p => p.SelfUser).ToList();

            if (((IChannel)createdUser.CallBack).State == CommunicationState.Opened)
                createdUser.CallBack.TransferHistory(allUsers, Messages);

            foreach (UserBindings existUser in acceptedUsers.Values)
            {
                if (createdUser == existUser)
                    continue;

                if (existUser.CallBack != null && ((IChannel)existUser.CallBack).State == CommunicationState.Opened)
                    existUser.CallBack.TransferHistory(allUsers, null);
            }
        }

        private bool Window_OnAccessOrRemoveUser(string guid, bool isRemove)
        {
            lock (sync)
            {
                UserBindings result;
                bool waiterIsExist = waitingAccess.TryGetValue(guid, out result);
                if (waiterIsExist)
                {
                    waitingAccess.Remove(result.SelfUser.GUID);
                    if (isRemove)
                    {
                        if (result.CallBack == null)
                        {
                            OnRemoveOrAccessUser?.BeginInvoke(ServerResult.AccessDenied, result.SelfUser, null, null);
                        }
                        else
                        {
                            if (((IChannel)result.CallBack).State == CommunicationState.Opened)
                                result.CallBack.ConnectResult(ServerResult.AccessDenied);
                        }
                    }
                    else
                    {
                        acceptedUsers.Add(result.SelfUser.GUID, result);
                        if (result.CallBack == null)
                        {
                            OnRemoveOrAccessUser?.BeginInvoke(ServerResult.AccessGranted, result.SelfUser, null, null);
                        }
                        else
                        {
                            if (((IChannel) result.CallBack).State == CommunicationState.Opened)
                            {
                                result.CallBack.ConnectResult(ServerResult.AccessGranted);
                                UpdateUserList(result);
                                window.ChangeUserStatusIsActive(result.SelfUser);
                            }
                            return true;
                        }
                    }
                }
                else
                {
                    if (isRemove)
                    {
                        bool acceptedIsExist = acceptedUsers.TryGetValue(guid, out result);
                        if (acceptedIsExist)
                        {
                            acceptedUsers.Remove(guid);
                            if (((IChannel) result.CallBack).State == CommunicationState.Opened)
                                result.CallBack.Terminate();
                        }
                    }
                }
                return false;
            }
        }



        void IChat.Connect(User newUser)
        {
            lock (sync)
            {
                try
                {
                    if (newUser == null || string.IsNullOrEmpty(newUser.GUID) || string.IsNullOrEmpty(newUser.Name))
                    {
                        if (CurrentCallbackIsOpen)
                            CurrentCallback.ConnectResult(ServerResult.AccessDenied);
                        return;
                    }

                    RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
                    UserBindings userForAccess;
                    bool result = acceptedUsers.TryGetValue(newUser.GUID, out userForAccess);
                    if (result)
                    {
                        if (userForAccess.Address == prop.Address)
                        {
                            if (userForAccess.CallBack == null)
                            {
                                userForAccess.CallBack = CurrentCallback;
                            }
                            else
                            {
                                if (((IChannel) userForAccess.CallBack).State == CommunicationState.Opened && userForAccess.CallBack != CurrentCallback)
                                {
                                    if (CurrentCallbackIsOpen)
                                        CurrentCallback.ConnectResult(ServerResult.AccessDenied);
                                    return;
                                }
                                else
                                {
                                    userForAccess.CallBack = CurrentCallback;
                                }
                            }

                            if (!userForAccess.SelfUser.Name.Equals(newUser.Name, StringComparison.CurrentCultureIgnoreCase))
                            {
                                userForAccess.SelfUser.Name = newUser.Name;
                                window.ChangeUserName(newUser);
                            }

                            if (CurrentCallbackIsOpen)
                            {
                                CurrentCallback.ConnectResult(ServerResult.SUCCESS);
                                UpdateUserList(userForAccess);
                                window.ChangeUserStatusIsActive(userForAccess.SelfUser);
                            }
                            return;
                        }
                        else 
                        {
                            if (CurrentCallbackIsOpen)
                                CurrentCallback.ConnectResult(ServerResult.AccessDenied);
                            return;
                        }
                    }

                    if (IsUnbinded)
                    {
                        if(CurrentCallbackIsOpen)
                            CurrentCallback.ConnectResult(ServerResult.AccessDenied);
                        return;
                    }


                    result = waitingAccess.TryGetValue(newUser.GUID, out userForAccess);
                    if (result)
                    {
                        if (CurrentCallbackIsOpen)
                            CurrentCallback.ConnectResult(ServerResult.YourRequestInProgress);
                        return;
                    }
                    else
                    {
                        bool uniqueName = acceptedUsers.Values.Any(p => p.SelfUser.Name.Equals(newUser.Name, StringComparison.CurrentCultureIgnoreCase));
                        if (uniqueName)
                        {
                            if (CurrentCallbackIsOpen)
                                CurrentCallback.ConnectResult(ServerResult.NameIsBusy);
                            return;
                        }
                        else
                        {
                            uniqueName = waitingAccess.Values.Any(p => p.SelfUser.Name.Equals(newUser.Name, StringComparison.CurrentCultureIgnoreCase));
                            if (uniqueName)
                            {
                                if (CurrentCallbackIsOpen)
                                    CurrentCallback.ConnectResult(ServerResult.NameIsBusy);
                                return;
                            }
                        }
                    }


                    AddWaiter(newUser, CurrentCallback, prop.Address, prop.Port.ToString());
                    if (CurrentCallbackIsOpen)
                        CurrentCallback.ConnectResult(ServerResult.AwaitConfirmation);
                }
                catch (Exception e)
                {
                    if (CurrentCallbackIsOpen)
                        CurrentCallback.ConnectResult(ServerResult.FAILURE);
                }
            }
        }



        void IChat.Disconnect(User user)
        {
            lock (sync)
            {
                try
                {
                    UserBindings userBind;
                    if(!GetUserBinding(user, out userBind))
                        return;

                    RemoveAcceptedUser(user);

                    List<User> allUsers = acceptedUsers.Values.Where(p => p.CallBack != null).Select(p => p.SelfUser).ToList();
                    foreach (UserBindings existUser in acceptedUsers.Values)
                    {
                        if (existUser.CallBack != null && ((IChannel)existUser.CallBack).State == CommunicationState.Opened)
                            existUser.CallBack.TransferHistory(allUsers, null);
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        void AdminIsWriting(bool isWriting)
        {
            lock (sync)
            {
                foreach (UserBindings existUser in acceptedUsers.Values)
                {
                    if (existUser.SelfUser == admin)
                        continue;

                    if (existUser.CallBack != null && ((IChannel)existUser.CallBack).State == CommunicationState.Opened)
                        existUser.CallBack.IsWritingCallback(admin, isWriting);
                }
            }
        }

        void IChat.IsWriting(User user, bool isWriting)
        {
            lock (sync)
            {
                try
                {
                    UserBindings userBind;
                    if (!GetUserBinding(user, out userBind))
                        return;

                    window.IncomingIsWriting(user, isWriting);

                    foreach (UserBindings existUser in acceptedUsers.Values)
                    {
                        if(existUser.SelfUser == user)
                            continue;

                        if (existUser.CallBack != null && ((IChannel)existUser.CallBack).State == CommunicationState.Opened)
                            existUser.CallBack.IsWritingCallback(user, isWriting);
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        void AdminSay(string msg)
        {
            lock (sync)
            {
                foreach (UserBindings existUser in acceptedUsers.Values)
                {
                    if (existUser.SelfUser == admin)
                        continue;

                    if (existUser.CallBack != null && ((IChannel) existUser.CallBack).State == CommunicationState.Opened)
                        existUser.CallBack.Receive(new Message() {
                                                                     Sender = admin,
                                                                     Content = msg,
                                                                     Time = DateTime.Now
                                                                 });
                }
            }
        }

        void IChat.Say(Message message)
        {
            lock (sync)
            {
                try
                {
                    UserBindings userBind;
                    if (!GetUserBinding(message.Sender, out userBind))
                        return;

                    window.IncomingReceve(message);

                    foreach (UserBindings existUser in acceptedUsers.Values)
                    {
                        if (existUser.SelfUser == message.Sender)
                            continue;

                        if (existUser.CallBack != null && ((IChannel)existUser.CallBack).State == CommunicationState.Opened)
                            existUser.CallBack.Receive(message);
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }


        bool GetUserBinding(User user, out UserBindings userBind)
        {
            userBind = null;
            if (user == null || string.IsNullOrEmpty(user.GUID) || string.IsNullOrEmpty(user.Name))
                return false;

            RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            UserBindings userForAccess;
            bool result = acceptedUsers.TryGetValue(user.GUID, out userForAccess);
            if (result)
            {
                if (userForAccess.Address == prop.Address)
                {
                    userBind = userForAccess;
                    return true;
                }
            }
            return false;
        }

    }
}
