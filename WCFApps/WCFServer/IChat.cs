using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace WCFChat.Service
{
    [ServiceContract(CallbackContract = typeof(IChatCallback), SessionMode = SessionMode.Required)]
    public interface IChat
    {
        [OperationContract(IsInitiating = true)]
        bool Login(User user);

        [OperationContract(IsOneWay = true)]
        void Say(Message message);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Logoff(Client client);
    }

    public interface IChatCallback
    {
        // IsOneWay - означает что сервис не будет ждать выполнение запроса на клиенте
        // также если IsOneWay=true то возвращать значения нельзя, только void
        [OperationContract(IsOneWay = false)]
        DateTime RefreshClientsAndGetEarlyDataMessage(List<Client> clients, bool isGetEarlyMessage);

        [OperationContract(IsOneWay = false)]
        List<Message> GetAllContentHistory();

        [OperationContract(IsOneWay = true)]
        void RefreshContentHistory(List<Message> messages);

        [OperationContract(IsOneWay = true)]
        void Receive(Message msg);

        [OperationContract(IsOneWay = true)]
        void IsWritingCallback(Client client);

        [OperationContract(IsOneWay = true)]
        void Terminate();
    }
}
