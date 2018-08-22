using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using WCFChat.Service;
using System.Timers;
using UIControls.MainControl;
using WCFChat.Client.BasicControl;
using Message = WCFChat.Service.Message;

namespace WCFChat.Client.BasicControl
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class MainWindowChatServer : WCFChat.Service.IChat
    {
        protected object sync = new object();
        private AccessResult OnRemoveOrAccessUser;
        public IChatCallback CurrentCallback => OperationContext.Current.GetCallbackChannel<IChatCallback>();
        public bool CurrentCallbackIsOpen => ((IChannel)CurrentCallback).State == CommunicationState.Opened;

        protected Dictionary<string, WindowControl> Clouds { get; } = new Dictionary<string, WindowControl>();
        private UIWindow mainWindow;

        public MainWindowChatServer(UIWindow mainWindow, AccessResult removeOrAcceptUser)
        {
            OnRemoveOrAccessUser = removeOrAcceptUser;
            this.mainWindow = mainWindow;
        }

        public void CreateCloud(User initiator, Cloud cloud, string transactionId)
        {
            Clouds.Add(cloud.Name, new WindowControl(mainWindow, initiator, cloud, transactionId));



            //Timer _timer = new Timer
            //{
            //    Interval = 120 * 1000
            //};
            //_timer.Elapsed += (sender, args) =>
            //{
            //    lock (sync)
            //    {
            //        try
            //        {
            //            List<string> forRemove = (from userBindItem in AllUsers.Values where userBindItem.CallBack != null && ((IChannel) userBindItem.CallBack).State != CommunicationState.Opened select userBindItem.GUID.Value).ToList();

            //            foreach (string removeUser in forRemove)
            //            {
            //                RemoveUser(removeUser);
            //            }

            //            List<User> users = AllUsers.Values.Where(p => p.CallBack != null || p.Status == UserStatus.Admin).Select(p => p.User).ToList();
            //            foreach (UserBindings existUser in AllUsers.Values)
            //            {
            //                if (existUser.CallBack != null && ((IChannel)existUser.CallBack).State == CommunicationState.Opened)
            //                    existUser.CallBack.TransferHistory(users, null);
            //            }
            //        }
            //        catch (Exception e)
            //        {

            //        }
            //    }
            //};
            //_timer.AutoReset = true;
            //_timer.Start();

        }

        /// <summary>
        /// Main сервер запрашивает можно ли новому узеру присоединить к текущему облаку
        /// </summary>
        /// <param name="newUser"></param>
        /// <param name="address"></param>
        public void IncomingRequestForAccess(User newUser, string address)
        {
            lock (sync)
            {
                if (newUser == null || string.IsNullOrEmpty(newUser.GUID) || string.IsNullOrEmpty(newUser.Name) || IsUnbinded)
                {
                    if (CurrentCallbackIsOpen)
                        CurrentCallback.ConnectResult(ServerResult.AccessDenied);
                    return;
                }

                if (!IsUniqueNames(newUser))
                    return;

                string[] addr = address.Split(':');
                AddWaiter(newUser, null, addr[0], addr[1]);
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
                        if (((IChannel) userBind.CallBack).State == CommunicationState.Opened)
                        {
                            if (userBind.Status == UserStatus.Waiter)
                            {
                                userBind.CallBack.ConnectResult(ServerResult.AccessDenied);
                            }
                            else
                            {
                                userBind.CallBack.Terminate(CurrentCloud);
                            }
                        }
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

                                    if (!userForAccess.Name.Value.Equals(newUser.Name, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        ChangeUserName(userForAccess, newUser);
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


                    if (!IsUniqueNames(newUser))
                        return;

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

        bool IsUniqueNames(User newUser)
        {
            bool currentNameAlreadyExist = AllUsers.Values.Any(p => p.User.Name.Equals(newUser.Name, StringComparison.CurrentCultureIgnoreCase));
            if (currentNameAlreadyExist)
            {
                if (CurrentCallbackIsOpen)
                    CurrentCallback.ConnectResult(ServerResult.NameIsBusy);
                return false;
            }
            return true;
        }

        void UpdateUserList(UserBindings createdUser)
        {
            List<User> allUsers = AllUsers.Values.Where(p => p.CallBack != null || p.Status == UserStatus.Admin).Select(p => p.User).ToList();

            if (((IChannel)createdUser.CallBack).State == CommunicationState.Opened)
                createdUser.CallBack.TransferHistory(allUsers, Messages());

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

                    List<User> allUsers = AllUsers.Values.Where(p => p.CallBack != null || p.Status == UserStatus.Admin).Select(p => p.User).ToList();
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
                        existUser.CallBack.IsWritingCallback(Initiator.User, isWriting);
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

                    SomeoneUserIsWriting(userBind, isWriting);

                    foreach (UserBindings existUser in AllUsers.Values)
                    {
                        if(existUser.User.GUID == user.GUID)
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
                                                     Sender = Initiator.User,
                                                     Content = msg,
                                                     Time = DateTime.Now
                                                 };

                SomeoneUserReceveMessage(adminSay);

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
                        if (existUser.User.GUID == message.Sender.GUID)
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


        protected override bool GetUserBinding(User user, out UserBindings userBind)
        {
            userBind = null;
            if (user == null || string.IsNullOrEmpty(user.GUID) || string.IsNullOrEmpty(user.Name))
                return false;

            RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty) OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            UserBindings userForAccess;
            bool result = AllUsers.TryGetValue(user.GUID, out userForAccess);
            if (result)
            {
                // если коллбек инициатора запроса и коллбек который мы сохранили отличаются, то реджектим
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
