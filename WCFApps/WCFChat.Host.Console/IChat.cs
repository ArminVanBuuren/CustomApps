using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace WCFChat.Host.Console
{
    [ServiceContract(CallbackContract = typeof(IChatCallback), SessionMode = SessionMode.Required)]
    public interface IChat
    {
        [OperationContract(IsInitiating = true)]
        bool Login(User user);

        [OperationContract(IsOneWay = true)]
        void Say(Message message);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Logoff(WCFChatClient client);
    }

    public interface IChatCallback
    {
        [OperationContract(IsOneWay = false)]
        DateTime Refresh(WCFChatClient[] clients, bool isGetEarlyMessage);

        [OperationContract(IsOneWay = false)]
        List<Message> GetAllContentHistory();

        [OperationContract(IsOneWay = true)]
        void RefreshContentHistory(Message[] messages);

        [OperationContract(IsOneWay = false)]
        DateTime Receive(Message msg);

        [OperationContract(IsOneWay = true)]
        void IsWritingCallback(WCFChatClient client);

        [OperationContract(IsOneWay = true)]
        void Terminate();
    }
}
