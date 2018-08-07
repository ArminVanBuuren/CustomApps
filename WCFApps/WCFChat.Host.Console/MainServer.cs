using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using WCFChat.Service;
using Message = WCFChat.Service.Message;

namespace WCFChat.Host.Console
{
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

    [ServiceBehavior(Namespace = "http://localhost/services/server", 
        InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class MainServer : IMainContract, IChat
    {
        object syncServer = new object();
        object syncOperation = new object();
        internal static CloudCollection Clouds { get; } = new CloudCollection();
        Dictionary<User, IMainCallback> waitForAccessToCloud = new Dictionary<User, IMainCallback>();

        public IMainCallback Main_CurrentCallback => OperationContext.Current.GetCallbackChannel<IMainCallback>();

        public bool Main_CurrentCallbackIsOpened => ((IChannel) Main_CurrentCallback).State == CommunicationState.Opened;


        ServiceHost host = null;
        Dictionary<string, KeyValuePair<Cloud, IMainCallback>> cloudInMainServer = new Dictionary<string, KeyValuePair<Cloud, IMainCallback>>();


        public void CreateCloud(User user, Cloud cloud, string transactionID)
        {
            lock (syncServer)
            {
                try
                {
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
                                    host = new ServiceHost(typeof(Host.Console.ClientServer));
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
                            if (Main_CurrentCallbackIsOpened)
                                Main_CurrentCallback.CreateCloudResult(CloudResult.SUCCESS, transactionID);
                        }
                        else
                        {
                            if (Main_CurrentCallbackIsOpened)
                                Main_CurrentCallback.CreateCloudResult(CloudResult.FAILURE, transactionID);
                        }
                    }
                    else
                    {
                        if (Main_CurrentCallbackIsOpened)
                            Main_CurrentCallback.CreateCloudResult(CloudResult.CloudIsBusy, transactionID);
                    }
                }
                catch (Exception e)
                {
                    if (Main_CurrentCallbackIsOpened)
                        Main_CurrentCallback.CreateCloudResult(CloudResult.FAILURE, transactionID);
                }
            }
        }

        public void Unbind(string transactionID)
        {
            lock (syncServer)
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

                        if (Main_CurrentCallbackIsOpened)
                            Main_CurrentCallback.UnbindResult(CloudResult.SUCCESS, transactionID);
                    }
                    else if (Main_CurrentCallbackIsOpened)
                    {
                        Main_CurrentCallback.UnbindResult(CloudResult.NotFound, transactionID);
                    }
                }
                catch (Exception ex)
                {
                    if (Main_CurrentCallbackIsOpened)
                        Main_CurrentCallback.UnbindResult(CloudResult.FAILURE, null);
                }
            }
        }

        public void GetCloud(User user)
        {
            lock (syncServer)
            {
                try
                {
                    if (waitForAccessToCloud.ContainsKey(user))
                    {
                        if (Main_CurrentCallbackIsOpened)
                            Main_CurrentCallback.GetCloudResult(CloudResult.YourRequestInProgress, null);
                        return;
                    }

                    RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty) OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];

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
                                cloudCreator.RequestForAccess(user, $"{prop.Address}:{prop.Port}");
                                waitForAccessToCloud.Add(user, Main_CurrentCallback);
                                return;
                            }
                            else
                            {
                                Clouds.Remove(transactionID);
                                if (Main_CurrentCallbackIsOpened)
                                    Main_CurrentCallback.GetCloudResult(CloudResult.CloudNotFound, null);
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (Main_CurrentCallbackIsOpened)
                        Main_CurrentCallback.GetCloudResult(CloudResult.FAILURE, null);
                }
            }
        }

        public void RequestForAccessResult(CloudResult result, User user)
        {
            lock (syncServer)
            {
                try
                {
                    if (!waitForAccessToCloud.ContainsKey(user))
                        return;

                    IMainCallback callBackWaiter = waitForAccessToCloud[user];
                    if (result == CloudResult.SUCCESS)
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


        object syncObj = new object();
        Dictionary<CloudArgs, CloudBinding> clouds = new Dictionary<CloudArgs, CloudBinding>();
        public IChatCallback Chat_CurrentCallback => OperationContext.Current.GetCallbackChannel<IChatCallback>();
        public bool Chat_CurrentCallbackIsOpened => ((IChannel)Chat_CurrentCallback).State == CommunicationState.Opened;
        public ServerResult Connect(User newUser)
        {
            lock (syncObj)
            {
                try
                {
                    CloudArgs existCloud = MainServer.Clouds.GetCloud(newUser);
                    if (existCloud == null)
                        return ServerResult.CloudNotFound;
                    if (!existCloud.IsAvailable) // если облако содано на локальном сервере и недоступно для новых пользователей
                        return ServerResult.AccessDenied;

                    CloudBinding cloudBinding;
                    List<CloudUser> cloudUsers;
                    bool existInLocalServer = clouds.TryGetValue(existCloud, out cloudBinding);
                    if (!existInLocalServer)
                    {
                        cloudBinding = new CloudBinding(existCloud);
                        cloudBinding.CloudUserCollection.Add(new CloudUser(newUser, Chat_CurrentCallback, existCloud));
                        cloudUsers = cloudBinding.CloudUserCollection;
                        clouds.Add(existCloud, cloudBinding);
                    }
                    else
                    {
                        cloudUsers = cloudBinding.CloudUserCollection;
                        CloudUser existUser = cloudUsers.FirstOrDefault(p => p.User.Name.Equals(newUser.Name, StringComparison.CurrentCultureIgnoreCase));
                        // Если в системе уже есть другой юзер с тем же именем
                        if (existUser != null && existUser.User.GUID != newUser.GUID)
                            return ServerResult.NameIsBusy;
                        else if (existUser != null)
                        {
                            // грохаем юзера который уже есть в чате но с другого приложения, т.е. грохаем его на старом приложении
                            if (((IChannel)existUser.CallBack).State == CommunicationState.Opened)
                                existUser.CallBack.Terminate();
                            cloudUsers.Remove(existUser);
                        }

                        cloudUsers.Add(new CloudUser(newUser, Chat_CurrentCallback, existCloud));
                    }

                    List<User> usersInCloud = cloudUsers.Select(c => c.User).ToList();
                    foreach (CloudUser cloudUser in cloudBinding.CloudUserCollection)
                    {
                        if (((IChannel)cloudUser.CallBack).State == CommunicationState.Opened)
                            cloudUser.CallBack.TransferHistory(usersInCloud, cloudBinding.Messages);
                    }

                    return ServerResult.SUCCESS;
                }
                catch (Exception e)
                {
                    return ServerResult.FAILURE;
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
                            cloudUser.CallBack.IsWritingCallback(user);
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
            CloudArgs existCloud = MainServer.Clouds.GetCloud(user);
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