using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFChat.Host.Console
{
    [DataContract]
    public enum ServerResult
    {
        [EnumMember]
        SUCCESS = 0,
        [EnumMember]
        FAILURE = 1,
        [EnumMember]
        NameIsBusy = 1,
        [EnumMember]
        AwaitConfirmation = 2,
        [EnumMember]
        AccessDenied = 3,
        [EnumMember]
        AccessGranted = 4
    }

    [DataContract]
    public enum ServerPrivelege
    {
        [EnumMember]
        Admin = 0,
        [EnumMember]
        User = 1
    }

    [DataContract]
    public class Message
    {
        [DataMember]
        public User Sender { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public DateTime Time { get; set; }
    }

    // [KnownType(typeof(User))] - в случа апкаста чтобы свойство Password из наследника передавалось в запрос то нужен этот аттрибут иначе серализация не сработает
    // Конструктор должен быть всегда без параметров иначе сериализатор не сможет создать экземпляр
    [ServiceContract(CallbackContract = typeof(IChatCallback), SessionMode = SessionMode.Required)]
    public interface IChat
    {
        [OperationContract(IsOneWay = false, IsInitiating = true)]
        ServerResult Connect(User user);

        [OperationContract(IsOneWay = true)]
        void Say(Message message);

        [OperationContract(IsOneWay = true)]
        void IsWriting(User client, bool isWriting);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Disconnect(User client);
    }

    public interface IChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void SetPrivilege(User user, ServerPrivelege privelege);

        [OperationContract(IsOneWay = false)]
        void TransferHistory(List<User> users, List<Message> messages);

        [OperationContract(IsOneWay = false)]
        void Receive(Message msg);

        [OperationContract(IsOneWay = true)]
        void IsWritingCallback(User client);

        [OperationContract(IsOneWay = true)]
        void Terminate();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ClientServer : IChat
    {
        object syncObj = new object();
        Dictionary<Cloud, IChatCallback> clouds = new Dictionary<Cloud, IChatCallback>();
        Dictionary<User, KeyValuePair<CloudArgs, IChatCallback>> users = new Dictionary<User, KeyValuePair<CloudArgs, IChatCallback>>();
        public IChatCallback CurrentCallback => OperationContext.Current.GetCallbackChannel<IChatCallback>();
        public bool CurrentIsOpened => ((IChannel)CurrentCallback).State == CommunicationState.Opened;

        public ServerResult Connect(User newUser)
        {
            lock (syncObj)
            {
                CloudArgs existCloud = MainServer.Clouds.GetCloud(newUser);
                if(!existCloud.IsAvailable)
                    return ServerResult.AccessDenied;

                // Если в системе уже есть другой юзер с тем же именем
                User existUserName = users.Keys.FirstOrDefault(p => p.Name.Equals(newUser.Name, StringComparison.CurrentCultureIgnoreCase));
                if (existUserName != null && newUser.GUID != existUserName.GUID)
                    return ServerResult.NameIsBusy;

                User existGUID = users.Keys.FirstOrDefault(p => p.GUID.Equals(newUser.GUID)
                                                        && p.CloudName.Equals(existCloud.CloudConfig.Name, StringComparison.CurrentCultureIgnoreCase));

                if (existGUID != null)
                {
                    // грохаем юзера который уже есть в чате но с другого приложения, т.е. грохаем его на старом приложении
                    users[existGUID].Value.Terminate();
                    users.Remove(existGUID);
                }

                users.Add(newUser, new KeyValuePair<CloudArgs, IChatCallback>(existCloud, CurrentCallback));

                foreach (KeyValuePair<User, KeyValuePair<CloudArgs, IChatCallback>> cloudUser in users.Where(p => p.Value.Key.CloudConfig.Name.Equals(existCloud.CloudConfig.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    if (((IChannel) cloudUser.Value.Value).State == CommunicationState.Opened)
                    {
                        cloudUser.Value.Value.TransferHistory(users.Keys.ToList(), null);
                    }
                }

                return ServerResult.SUCCESS;
            }
        }

        public void Disconnect(User client)
        {
            throw new NotImplementedException();
        }

        public void IsWriting(User client, bool isWriting)
        {
            throw new NotImplementedException();
        }

        public void Say(Message message)
        {
            throw new NotImplementedException();
        }
    }


    //public class Cloud1
    //{
    //    public Dictionary<User, IChatCallback> AwaitsConfirmaton { get; }
    //    public Dictionary<User, IChatCallback> Users { get; }
    //    public List<WCFChatClient> Clients { get; }
    //    public string Name { get; }

    //    public Cloud(string name)
    //    {
    //        Name = name;
    //        Users = new Dictionary<User, IChatCallback>();
    //        AwaitsConfirmaton = new Dictionary<User, IChatCallback>();
    //        Clients = new List<WCFChatClient>();
    //    }

    //    public UserConfirmed Add(User newUser, IChatCallback callBack)
    //    {
    //        UserConfirmed result = new UserConfirmed();
    //        if (Users.Count == 0)
    //        {
    //            AddNewUser(newUser, callBack);
    //            result.Result = ServerResult.YouAreAdmin;
    //            return result;
    //        }

    //        User exist = Users.Keys.First(p => p.Name.Equals(newUser.Name, StringComparison.CurrentCultureIgnoreCase));
    //        if (exist != null && exist.GUID != newUser.GUID)
    //        {
    //            result.Result = ServerResult.UserNameBusy;
    //            return result;
    //        }


    //        if (exist != null)
    //        {
    //            Users[exist].Terminate();
    //            Users.Remove(exist);
    //            Clients.Remove(Clients.First(p => p.GUID.Equals(exist.GUID)));
    //        }

    //        AddNewUser(newUser, callBack);
    //        result.Result = ServerResult.AwaitConfirmation;
    //        return result;
    //    }

    //    void AddNewUser(User newUser, IChatCallback callBack)
    //    {
    //        Users.Add(newUser, callBack);
    //        Clients.Add(new WCFChatClient()
    //                    {
    //                        GUID = newUser.GUID,
    //                        Name = newUser.Name,
    //                        Time = newUser.Time
    //                    });
    //    }
    //}
}
