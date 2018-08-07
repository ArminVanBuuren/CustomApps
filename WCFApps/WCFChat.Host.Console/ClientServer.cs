using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Timers;
using WCFChat.Service;
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

    [ServiceBehavior(Namespace = "http://localhost/services/chat",
        InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ClientServer : IChat
    {
        object syncObj = new object();
        Dictionary<CloudArgs, CloudBinding> clouds = new Dictionary<CloudArgs, CloudBinding>();
        public IChatCallback CurrentCallback => OperationContext.Current.GetCallbackChannel<IChatCallback>();
        public bool CurrentIsOpened => ((IChannel)CurrentCallback).State == CommunicationState.Opened;

        public ClientServer()
        {
            Timer _timer = new Timer {
                                         Interval = 30 * 1000
                                     };
            _timer.Elapsed += (sender, args) =>
                              {
                                  lock (syncObj)
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
                              };
            _timer.Start();
        }

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
                        cloudBinding.CloudUserCollection.Add(new CloudUser(newUser, CurrentCallback, existCloud));
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
                            if (((IChannel) existUser.CallBack).State == CommunicationState.Opened)
                                existUser.CallBack.Terminate();
                            cloudUsers.Remove(existUser);
                        }

                        cloudUsers.Add(new CloudUser(newUser, CurrentCallback, existCloud));
                    }

                    List<User> usersInCloud = cloudUsers.Select(c => c.User).ToList();
                    foreach (CloudUser cloudUser in cloudBinding.CloudUserCollection)
                    {
                        if (((IChannel) cloudUser.CallBack).State == CommunicationState.Opened)
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
