using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Timers;

namespace WCFChat.Service
{
    [DataContract]
    public enum ServerResult
    {
        [EnumMember]
        SUCCESS = 0,
        [EnumMember]
        FAILURE = 1,
        [EnumMember]
        CloudNotFound = 2,
        [EnumMember]
        NameIsBusy = 3,
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
    [ServiceContract(Namespace = "http://localhost/services/chat",
        CallbackContract = typeof(IChatCallback), SessionMode = SessionMode.Required)]
    public interface IChat
    {
        [OperationContract(IsOneWay = false, IsInitiating = true)]
        ServerResult Connect(User user);

        [OperationContract(IsOneWay = true)]
        void Say(Message message);

        [OperationContract(IsOneWay = true)]
        void IsWriting(User user, bool isWriting);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Disconnect(User user);
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


}
