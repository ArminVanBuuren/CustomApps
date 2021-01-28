using System.ServiceModel;
using WCFChat.Contracts.Chat.Entities;
using WCFChat.Contracts.Main.Entities;

namespace WCFChat.Contracts.Chat
{
	// [KnownType(typeof(User))] - в случа апкаста чтобы свойство Password из наследника передавалось в запрос то нужен этот аттрибут иначе серализация не сработает
    // Конструктор должен быть всегда без параметров иначе сериализатор не сможет создать экземпляр
    [ServiceContract(Namespace = Namespace.ServicesV001, CallbackContract = typeof(IChatServiceCallback), SessionMode = SessionMode.Required)]
    public interface IChatService
    {
        [OperationContract(IsOneWay = true, IsInitiating = true)]
        void Connect(User user);

        [OperationContract(IsOneWay = true)]
        void Say(ChatMessage message);

        [OperationContract(IsOneWay = true)]
        void IsWriting(User user, bool isWriting);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Disconnect(User user);
    }
}
