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
    class MainWindowChatServer : WindowControl, WCFChat.Service.IChat
    {
        public string TransactionID { get; private set; }
        private AccessResult OnRemoveOrAccessUser;
        private User admin;

        public MainWindowChatServer():base(true)
        {
            
        }

        public void Show(string transactionId, User initiator, AccessResult removeOrAcceptUser)
        {
            TransactionID = transactionId;
            OnRemoveOrAccessUser = removeOrAcceptUser;
            admin = new User()
                    {
                        GUID = UserBindings.GUID_ADMIN,
                        Name = initiator.Name,
                        CloudName = initiator.CloudName
                    };
            UserBindings userBind = new UserBindings(admin);
            AddUser(userBind, ".$my_localhost::");
        }


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

        protected override void AccessOrRemoveUser(UserBindings userBind, bool isRemove)
        {
            lock (sync)
            {
                if (isRemove)
                {
                    if (userBind.CallBack == null)
                    {
                        OnRemoveOrAccessUser?.BeginInvoke(ServerResult.AccessDenied, userBind.User, null, null);
                    }
                    else
                    {
                        if (((IChannel)userBind.CallBack).State == CommunicationState.Opened)
                            userBind.CallBack.ConnectResult(ServerResult.AccessDenied);
                    }
                    base.AccessOrRemoveUser(userBind, true);
                }
                else
                {
                    if (userBind.CallBack == null)
                    {
                        OnRemoveOrAccessUser?.BeginInvoke(ServerResult.AccessGranted, userBind.User, null, null);
                    }
                    else
                    {
                        if (((IChannel) userBind.CallBack).State == CommunicationState.Opened)
                        {
                            userBind.CallBack.ConnectResult(ServerResult.AccessGranted);
                            UpdateUserList(userBind);
                            ChangeUserStatusIsActive(userBind);
                        }
                    }
                }
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

                    RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty) OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
                    UserBindings userForAccess;
                    bool result = AllUsers.TryGetValue(newUser.GUID, out userForAccess);
                    if (result)
                    {
                        switch (userForAccess.Status)
                        {
                            case UserStatus.User:
                                if (userForAccess.Address.Value == prop.Address)
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

                                    if (!userForAccess.User.Name.Equals(newUser.Name, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        userForAccess.User.Name = newUser.Name;
                                        ChangeUserName(newUser);
                                    }

                                    if (CurrentCallbackIsOpen)
                                    {
                                        CurrentCallback.ConnectResult(ServerResult.SUCCESS);
                                        UpdateUserList(userForAccess);
                                        ChangeUserStatusIsActive(userForAccess);
                                    }
                                    return;
                                }
                                else
                                {
                                    if (CurrentCallbackIsOpen)
                                        CurrentCallback.ConnectResult(ServerResult.AccessDenied);
                                    return;
                                }
                            case UserStatus.Waiter:
                                if (CurrentCallbackIsOpen)
                                    CurrentCallback.ConnectResult(ServerResult.YourRequestInProgress);
                                return;
                            default:
                                return;
                        }
                    }


                    if (IsUnbinded)
                    {
                        if (CurrentCallbackIsOpen)
                            CurrentCallback.ConnectResult(ServerResult.AccessDenied);
                        return;
                    }


                    bool uniqueName = AllUsers.Values.Any(p => p.User.Name.Equals(newUser.Name, StringComparison.CurrentCultureIgnoreCase));
                    if (uniqueName)
                    {
                        if (CurrentCallbackIsOpen)
                            CurrentCallback.ConnectResult(ServerResult.NameIsBusy);
                        return;
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

        void UpdateUserList(UserBindings createdUser)
        {
            List<User> allUsers = AllUsers.Values.Where(p => p.CallBack != null || p.User.GUID == UserBindings.GUID_ADMIN).Select(p => p.User).ToList();

            if (((IChannel)createdUser.CallBack).State == CommunicationState.Opened)
                createdUser.CallBack.TransferHistory(allUsers, Messages);

            foreach (UserBindings existUser in AllUsers.Values)
            {
                if (createdUser == existUser)
                    continue;

                if (existUser.CallBack != null && ((IChannel)existUser.CallBack).State == CommunicationState.Opened)
                    existUser.CallBack.TransferHistory(allUsers, null);
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

                    RemoveUser(user);

                    List<User> allUsers = AllUsers.Values.Where(p => p.CallBack != null || p.User.GUID == UserBindings.GUID_ADMIN).Select(p => p.User).ToList();
                    foreach (UserBindings existUser in AllUsers.Values)
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

        protected override void CurrentUserIsWriting(bool isWriting)
        {
            lock (sync)
            {
                foreach (UserBindings existUser in AllUsers.Values)
                {
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

                    SomeoneUserIsWriting(user, isWriting);

                    foreach (UserBindings existUser in AllUsers.Values)
                    {
                        if(existUser.User == user)
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

        protected override void CurrentUserIsSaying(string msg)
        {
            lock (sync)
            {
                Message adminSay = new Message() {
                                                     Sender = admin,
                                                     Content = msg,
                                                     Time = DateTime.Now
                                                 };
                foreach (UserBindings existUser in AllUsers.Values)
                {
                    if (existUser.CallBack != null && ((IChannel) existUser.CallBack).State == CommunicationState.Opened)
                        existUser.CallBack.Receive(adminSay);
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

                    SomeoneUserReceveMessage(message);

                    foreach (UserBindings existUser in AllUsers.Values)
                    {
                        if (existUser.User == message.Sender)
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

            RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty) OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            UserBindings userForAccess;
            bool result = AllUsers.TryGetValue(user.GUID, out userForAccess);
            if (result)
            {
                if (userForAccess.CallBack != CurrentCallback)
                    return false;

                if (userForAccess.Address.Value == prop.Address)
                {
                    userBind = userForAccess;
                    return true;
                }
            }
            return false;
        }

    }
}
