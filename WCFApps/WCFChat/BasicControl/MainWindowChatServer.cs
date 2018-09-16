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
    public class MainWindowChatServer : WCFChat.Service.IChat
    {
        protected object sync = new object();
        private AccessResult OnRemoveOrAccessUser;
        public IChatCallback CurrentCallback => OperationContext.Current.GetCallbackChannel<IChatCallback>();
        public bool CurrentCallbackIsOpen => ((IChannel)CurrentCallback).State == CommunicationState.Opened;

        internal Dictionary<string, WindowControl> Clouds { get; } = new Dictionary<string, WindowControl>(StringComparer.CurrentCultureIgnoreCase);
        private UIWindow mainWindow;

        public MainWindowChatServer(UIWindow mainWindow, AccessResult removeOrAcceptUser)
        {
            OnRemoveOrAccessUser = removeOrAcceptUser;
            this.mainWindow = mainWindow;
        }

        public bool CreateCloud(User initiator, Cloud cloud, string transactionId)
        {
            lock (sync)
            {
                if (Clouds.ContainsKey(cloud.Name))
                    return false;

                WindowControl control = new WindowControl(mainWindow, initiator, cloud, transactionId);
                control.AdminChangedUserList += AccessOrRemoveUser;
                Clouds.Add(cloud.Name, control);
            }
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
            return true;
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
                
                if (newUser == null || string.IsNullOrEmpty(newUser.GUID) || string.IsNullOrEmpty(newUser.Name))
                {
                    if (CurrentCallbackIsOpen)
                        CurrentCallback.ConnectResult(ServerResult.AccessDenied);
                    return;
                }

                WindowControl control;
                if (!Clouds.TryGetValue(newUser.Name, out control) || control.IsUnbinded)
                {
                    if (CurrentCallbackIsOpen)
                        CurrentCallback.ConnectResult(ServerResult.AccessDenied);
                    return;
                }


                if (!control.IsUniqueNames(newUser))
                    return;

                string[] addr = address.Split(':');
                control.AddWaiter(newUser, null, addr[0], addr[1]);
            }
        }

        internal void AccessOrRemoveUser(UserBindings userBind, WindowControl control, UserChanges changes)
        {
            lock (sync)
            {
                if (changes == UserChanges.Remove)
                {
                    if (userBind.CallBack == null)
                    {
                        OnRemoveOrAccessUser?.BeginInvoke(ServerResult.AccessDenied, userBind.User, null, null);
                    }
                    else
                    {
                        if (((IChannel)userBind.CallBack).State == CommunicationState.Opened)
                        {
                            if (userBind.Status == UserStatus.Waiter)
                            {
                                userBind.CallBack.ConnectResult(ServerResult.AccessDenied);
                            }
                            else
                            {
                                userBind.CallBack.Terminate(control.CurrentCloud);
                            }
                        }
                    }
                }
                else
                {
                    if (userBind.CallBack == null)
                    {
                        OnRemoveOrAccessUser?.BeginInvoke(ServerResult.AccessGranted, userBind.User, null, null);
                    }
                    else
                    {
                        if (((IChannel)userBind.CallBack).State == CommunicationState.Opened)
                        {
                            userBind.CallBack.ConnectResult(ServerResult.AccessGranted);
                            UpdateUserList(userBind, control);
                            control.ChangeUserStatusIsActive(userBind);
                        }
                    }
                }
            }
        }

        public void RemoveCloud(Cloud cloud)
        {
            WindowControl control;
            if (!Clouds.TryGetValue(cloud.Name, out control))
            {
                if (CurrentCallbackIsOpen)
                    CurrentCallback.ConnectResult(ServerResult.CloudNotFound);
                return;
            }

            foreach (UserBindings userBind in control.AllUsers.Values)
            {
                userBind.CallBack.Terminate(cloud);
            }

            Clouds.Remove(cloud.Name);
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

                    WindowControl control;
                    if (!Clouds.TryGetValue(newUser.Name, out control))
                    {
                        if (CurrentCallbackIsOpen)
                            CurrentCallback.ConnectResult(ServerResult.CloudNotFound);
                        return;
                    }

                    UserBindings userForAccess;
                    bool result = control.AllUsers.TryGetValue(newUser.GUID, out userForAccess);
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
                                        control.ChangeUserName(userForAccess, newUser);
                                    }

                                    if (CurrentCallbackIsOpen)
                                    {
                                        CurrentCallback.ConnectResult(ServerResult.SUCCESS);
                                        UpdateUserList(userForAccess, control);
                                        control.ChangeUserStatusIsActive(userForAccess);
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


                    if (control.IsUnbinded)
                    {
                        if (CurrentCallbackIsOpen)
                            CurrentCallback.ConnectResult(ServerResult.AccessDenied);
                        return;
                    }


                    if (!control.IsUniqueNames(newUser))
                        return;

                    control.AddWaiter(newUser, CurrentCallback, prop.Address, prop.Port.ToString());
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

        

        void UpdateUserList(UserBindings createdUser, WindowControl control)
        {
            List<User> allUsers = control.AllUsers.Values.Where(p => p.CallBack != null || p.Status == UserStatus.Admin).Select(p => p.User).ToList();

            if (((IChannel)createdUser.CallBack).State == CommunicationState.Opened)
                createdUser.CallBack.TransferHistory(allUsers, control.Messages());

            foreach (UserBindings existUser in control.AllUsers.Values)
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
                    WindowControl control;
                    UserBindings userBind;
                    if(!GetUserBinding(user, out userBind, out control))
                        return;

                    control.RemoveUser(userBind);

                    List<User> allUsers = control.AllUsers.Values.Where(p => p.CallBack != null || p.Status == UserStatus.Admin).Select(p => p.User).ToList();
                    foreach (UserBindings existUser in control.AllUsers.Values)
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

        

        void IChat.IsWriting(User user, bool isWriting)
        {
            lock (sync)
            {
                try
                {
                    WindowControl control;
                    UserBindings userBind;
                    if (!GetUserBinding(user, out userBind, out control))
                        return;

                    control.SomeoneUserIsWriting(userBind, isWriting);

                    foreach (UserBindings existUser in control.AllUsers.Values)
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

        

        void IChat.Say(Message message)
        {
            lock (sync)
            {
                try
                {
                    WindowControl control;
                    UserBindings userBind;
                    if (!GetUserBinding(message.Sender, out userBind, out control))
                        return;

                    control.SomeoneUserReceveMessage(message);

                    foreach (UserBindings existUser in control.AllUsers.Values)
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


        internal bool GetUserBinding(User user, out UserBindings userBind, out WindowControl control)
        {
            userBind = null;
            control = null;
            if (user == null || string.IsNullOrEmpty(user.GUID) || string.IsNullOrEmpty(user.Name))
                return false;

            if (!Clouds.TryGetValue(user.CloudName, out control))
                return false;

            
            UserBindings userForAccess;
            bool result = control.AllUsers.TryGetValue(user.GUID, out userForAccess);
            if (result)
            {
                RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];

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
