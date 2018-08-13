using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using WCFChat.Service;
using System.Timers;
using Message = WCFChat.Service.Message;

namespace WCFChat.Host.Console
{
    class CloudUser
    {
        public CloudUser(User user, IChatCallback callBack, CloudArgs cloudBind)
        {
            User = user;
            CallBack = callBack;
            CloudBind = cloudBind;
        }
        public User User { get; }
        public IChatCallback CallBack { get; }
        public CloudArgs CloudBind { get; }
    }

    class CloudBinding
    {
        public CloudBinding(CloudArgs cloud)
        {
            Cloud = cloud;
        }
        public CloudArgs Cloud { get; }
        public List<Message> Messages { get; } = new List<Message>();
        public List<CloudUser> CloudUserCollection { get; } = new List<CloudUser>();
    }

    class CloudArgs
    {
        public CloudArgs(Cloud cloud, IMainCallback authorCallBack, bool localServer):this(cloud, authorCallBack)
        {
            IsLocalServer = localServer;
        }
        public CloudArgs(Cloud cloud, IMainCallback authorCallBack)
        {
            CloudConfig = cloud;
            AuthorCallBack = authorCallBack;
        }
        public Cloud CloudConfig { get; }
        public IMainCallback AuthorCallBack { get; }
        public bool IsAvailable { get; set; } = true;
        public bool IsLocalServer { get; } = false;
    }

    class CloudCollection : Dictionary<string, CloudArgs>
    {
        public CloudCollection() : base(StringComparer.CurrentCultureIgnoreCase)
        {
            
        }
        public CloudArgs GetCloud(string transactionID)
        {
            CloudArgs result = this[transactionID];
            if (result == null || !result.IsAvailable)
                return null;

            return result;
        }

        public CloudArgs GetCloud(User user)
        {
            return Values.FirstOrDefault(exs => exs.CloudConfig.Name.Equals(user.CloudName, StringComparison.CurrentCultureIgnoreCase));
        }

        public CloudArgs GetCloudArgs(string transactionID)
        {
            CloudArgs result = this[transactionID];
            if (result == null || !result.IsAvailable)
                return null;

            return result;
        }

        public bool IsExist(Cloud cloud)
        {
            return Values.Any(exs => exs.CloudConfig.Name.Equals(cloud.Name, StringComparison.CurrentCultureIgnoreCase));
        }

    }


    [ServiceBehavior(Namespace = "http://localhost/services", 
        InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class MainServer : IMainContract, IChat
    {
        public MainServer()
        {
            
        }


        object syncObj = new object();

        CloudCollection Clouds { get; } = new CloudCollection();
        Dictionary<User, IMainCallback> waitForAccessToCloud = new Dictionary<User, IMainCallback>();
        public IMainCallback Main_CurrentCallback => OperationContext.Current.GetCallbackChannel<IMainCallback>();
        public bool Main_CurrentCallbackIsOpen => ((IChannel) Main_CurrentCallback).State == CommunicationState.Opened;
        ServiceHost host = null;
        Dictionary<string, KeyValuePair<Cloud, IMainCallback>> cloudInMainServer = new Dictionary<string, KeyValuePair<Cloud, IMainCallback>>();


        public void CreateCloud(Cloud cloud, string transactionID)
        {
            lock (syncObj)
            {
                try
                {
                    if(string.IsNullOrEmpty(cloud.Name))
                        if (Main_CurrentCallbackIsOpen)
                            Main_CurrentCallback.CreateCloudResult(CloudResult.FAILURE, transactionID);

                    bool exist = Clouds.IsExist(cloud);
                    if (!exist)
                    {
                        bool isOpenned = true;
                        if (string.IsNullOrEmpty(cloud.Address))
                        {
                            try
                            {
                                if (host == null)
                                {
                                    MainServer chatServer = new MainServer(Clouds);
                                    host = new ServiceHost(chatServer);
                                    host.Open();
                                }
                                cloud.Address = host.BaseAddresses.ToString();
                                Clouds.Add(transactionID, new CloudArgs(cloud, Main_CurrentCallback, true));
                            }
                            catch (Exception e)
                            {
                                isOpenned = false;
                            }
                        }
                        else
                        {
                            Clouds.Add(transactionID, new CloudArgs(cloud, Main_CurrentCallback));
                        }

                        if (isOpenned)
                        {
                            if (Main_CurrentCallbackIsOpen)
                                Main_CurrentCallback.CreateCloudResult(CloudResult.SUCCESS, transactionID);
                        }
                        else
                        {
                            if (Main_CurrentCallbackIsOpen)
                                Main_CurrentCallback.CreateCloudResult(CloudResult.FAILURE, transactionID);
                        }
                    }
                    else
                    {
                        if (Main_CurrentCallbackIsOpen)
                            Main_CurrentCallback.CreateCloudResult(CloudResult.CloudIsBusy, transactionID);
                    }
                }
                catch (Exception e)
                {
                    if (Main_CurrentCallbackIsOpen)
                        Main_CurrentCallback.CreateCloudResult(CloudResult.FAILURE, transactionID);
                }
            }
        }

        public void Unbind(string transactionID)
        {
            lock (syncObj)
            {
                try
                {
                    CloudArgs exist = Clouds.GetCloud(transactionID);
                    if (exist != null)
                    {
                        if (exist.IsLocalServer)
                            exist.IsAvailable = false;
                        else
                            Clouds.Remove(transactionID);
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

        public void GetCloud(User user)
        {
            lock (syncObj)
            {
                try
                {
                    if (waitForAccessToCloud.ContainsKey(user))
                    {
                        if (Main_CurrentCallbackIsOpen)
                            Main_CurrentCallback.GetCloudResult(ServerResult.YourRequestInProgress, null);
                        return;
                    }

                    RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty) OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
                    string address = $"{prop.Address}:{prop.Port}";

                    string[] cloudTrnIDs = Clouds.Keys.ToArray();
                    for (int i = 0; i < cloudTrnIDs.Length; i++)
                    {
                        string transactionID = cloudTrnIDs[i];
                        CloudArgs exist = Clouds.GetCloudArgs(transactionID);
                        if (exist != null && exist.CloudConfig.Name.Equals(user.CloudName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            IMainCallback cloudCreator = exist.AuthorCallBack;
                            if (((IChannel) cloudCreator).State == CommunicationState.Opened)
                            {
                                cloudCreator.RequestForAccess(user, address);
                                waitForAccessToCloud.Add(user, Main_CurrentCallback);
                                return;
                            }
                            else
                            {
                                Clouds.Remove(transactionID);
                                if (Main_CurrentCallbackIsOpen)
                                    Main_CurrentCallback.GetCloudResult(ServerResult.CloudNotFound, null);
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (Main_CurrentCallbackIsOpen)
                        Main_CurrentCallback.GetCloudResult(ServerResult.FAILURE, null);
                }
            }
        }

        public void RemoveOrAccessUser(ServerResult result, User user)
        {
            lock (syncObj)
            {
                try
                {
                    if (!waitForAccessToCloud.ContainsKey(user))
                        return;

                    IMainCallback callBackWaiter = waitForAccessToCloud[user];
                    if (result == ServerResult.AccessGranted || result == ServerResult.SUCCESS)
                    {
                        CloudArgs cloudRes = Clouds.GetCloud(user);
                        if (((IChannel) callBackWaiter).State == CommunicationState.Opened)
                            callBackWaiter.GetCloudResult(result, cloudRes.CloudConfig);
                    }
                    else
                    {
                        if (((IChannel) callBackWaiter).State == CommunicationState.Opened)
                            callBackWaiter.GetCloudResult(result, null);
                    }
                    waitForAccessToCloud.Remove(user);

                }
                catch (Exception ex)
                {
                    // ignored
                }
            }
        }



        Dictionary<CloudArgs, CloudBinding> clouds = new Dictionary<CloudArgs, CloudBinding>();
        public IChatCallback Chat_CurrentCallback => OperationContext.Current.GetCallbackChannel<IChatCallback>();
        public bool Chat_CurrentCallbackIsOpen => ((IChannel)Chat_CurrentCallback).State == CommunicationState.Opened;
        private CloudCollection MainClouds;
        MainServer(CloudCollection Clouds)
        {
            MainClouds = Clouds;

            Timer _timer = new Timer
            {
                Interval = 120 * 1000
            };
            _timer.Elapsed += (sender, args) =>
            {
                lock (syncObj)
                {
                    try
                    {
                        foreach (var cloud in clouds)
                        {
                            cloud.Value.CloudUserCollection.RemoveAll((a) => ((IChannel) a.CallBack).State != CommunicationState.Opened);
                            foreach (CloudUser user in cloud.Value.CloudUserCollection)
                            {
                                user.CallBack.TransferHistory(cloud.Value.CloudUserCollection.Select(p => p.User).ToList(), null);
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            };
            _timer.AutoReset = true;
            _timer.Start();
        }

        public void Connect(User newUser)
        {
            lock (syncObj)
            {
                try
                {
                    if (newUser == null || string.IsNullOrEmpty(newUser.GUID) || string.IsNullOrEmpty(newUser.Name))
                    {
                        if (Chat_CurrentCallbackIsOpen)
                            Chat_CurrentCallback.ConnectResult(ServerResult.AccessDenied);
                        return;
                    }

                    CloudArgs existCloud = MainClouds.GetCloud(newUser);
                    if (existCloud == null)
                    {
                        if(Chat_CurrentCallbackIsOpen)
                            Chat_CurrentCallback.ConnectResult(ServerResult.CloudNotFound);
                        return;
                    }
                    if (!existCloud.IsAvailable) // если облако содано на локальном сервере и недоступно для новых пользователей
                    {
                        if (Chat_CurrentCallbackIsOpen)
                            Chat_CurrentCallback.ConnectResult(ServerResult.AccessDenied);
                        return;
                    }

                    CloudBinding cloudBinding;
                    List<CloudUser> cloudUsers;
                    bool existInLocalServer = clouds.TryGetValue(existCloud, out cloudBinding);

                    CloudUser createdUser;
                    if (!existInLocalServer)
                    {
                        cloudBinding = new CloudBinding(existCloud);
                        createdUser = new CloudUser(newUser, Chat_CurrentCallback, existCloud);
                        cloudBinding.CloudUserCollection.Add(createdUser);
                        cloudUsers = cloudBinding.CloudUserCollection;
                        clouds.Add(existCloud, cloudBinding);
                    }
                    else
                    {
                        cloudUsers = cloudBinding.CloudUserCollection;

                        //todo: Нужно сделать проверку по GUID т.к. могут коннектится с одиинаковыми гуидами но с дургими никами
                        //CloudUser existUserGuid = cloudUsers.FirstOrDefault(p => p.User.GUID.Equals(newUser.GUID)); 

                        CloudUser existUserName = cloudUsers.FirstOrDefault(p => p.User.Name.Equals(newUser.Name, StringComparison.CurrentCultureIgnoreCase));
                        // Если в системе уже есть другой юзер с тем же именем
                        if (existUserName != null && existUserName.User.GUID != newUser.GUID)
                        {
                            if (Chat_CurrentCallbackIsOpen)
                                Chat_CurrentCallback.ConnectResult(ServerResult.NameIsBusy);
                            return;
                        }
                        else if (existUserName != null)
                        {
                            // грохаем юзера который уже есть в чате но с другого приложения, т.е. грохаем его на старом приложении
                            if (((IChannel) existUserName.CallBack).State == CommunicationState.Opened)
                                existUserName.CallBack.Terminate(existUserName.CloudBind.CloudConfig);
                            cloudUsers.Remove(existUserName);
                        }

                        createdUser = new CloudUser(newUser, Chat_CurrentCallback, existCloud);
                        cloudUsers.Add(createdUser);
                    }

                    List<User> usersInCloud = cloudUsers.Select(c => c.User).ToList();

                    // отправляем новому бзеру всех кто в облаке и все сообщения
                    if (((IChannel) createdUser.CallBack).State == CommunicationState.Opened)
                        createdUser.CallBack.TransferHistory(usersInCloud, cloudBinding.Messages);

                    // отправляем всем юзерам обновленный список юзеров в облаке
                    foreach (CloudUser cloudUser in cloudBinding.CloudUserCollection)
                    {
                        if(createdUser == cloudUser)
                            continue;

                        if (((IChannel) cloudUser.CallBack).State == CommunicationState.Opened)
                            cloudUser.CallBack.TransferHistory(usersInCloud, null);
                    }

                    if (Chat_CurrentCallbackIsOpen)
                        Chat_CurrentCallback.ConnectResult(ServerResult.SUCCESS);
                }
                catch (Exception e)
                {
                    if (Chat_CurrentCallbackIsOpen)
                        Chat_CurrentCallback.ConnectResult(ServerResult.FAILURE);
                }
            }
        }

        public void Disconnect(User user)
        {
            lock (syncObj)
            {
                try
                {
                    CloudBinding cloudBinding;
                    if (!GetCloudUsers(user, out cloudBinding))
                        return;


                    cloudBinding.CloudUserCollection.RemoveAll((u) => u.User.GUID == user.GUID);
                    List<User> usersInCloud = cloudBinding.CloudUserCollection.Select(s => s.User).ToList();
                    foreach (var cloudUser in cloudBinding.CloudUserCollection)
                    {
                        if (((IChannel)cloudUser.CallBack).State == CommunicationState.Opened)
                            cloudUser.CallBack.TransferHistory(usersInCloud, null);
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        public void IsWriting(User user, bool isWriting)
        {
            lock (syncObj)
            {
                try
                {
                    CloudBinding cloudBinding;
                    if (!GetCloudUsers(user, out cloudBinding))
                        return;


                    bool userInGroup = cloudBinding.CloudUserCollection.Any(p => p.User.GUID == user.GUID);
                    if (!userInGroup)
                        return;

                    foreach (CloudUser cloudUser in cloudBinding.CloudUserCollection)
                    {
                        if (((IChannel)cloudUser.CallBack).State == CommunicationState.Opened)
                            cloudUser.CallBack.IsWritingCallback(user, isWriting);
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        public void Say(Message message)
        {
            lock (syncObj)
            {
                try
                {
                    CloudBinding cloudBinding;
                    if (!GetCloudUsers(message.Sender, out cloudBinding))
                        return;


                    bool userInGroup = cloudBinding.CloudUserCollection.Any(p => p.User.GUID == message.Sender.GUID);
                    if (!userInGroup)
                        return;


                    foreach (CloudUser cloudUser in cloudBinding.CloudUserCollection)
                    {
                        if (((IChannel)cloudUser.CallBack).State == CommunicationState.Opened)
                            cloudUser.CallBack.Receive(message);
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        bool GetCloudUsers(User user, out CloudBinding cloudBinding)
        {
            cloudBinding = null;
            if (user == null)
                return false;
            CloudArgs existCloud = MainClouds.GetCloud(user);
            if (existCloud == null)
                return false;
            if (!existCloud.IsLocalServer)
                return false;


            bool isExist = clouds.TryGetValue(existCloud, out cloudBinding);
            if (!isExist)
                return false;
            return true;
        }
    }
}