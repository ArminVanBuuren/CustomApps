using System.ServiceModel;
using WCFChat.Contracts.Chat.Entities;
using WCFChat.Contracts.Main.Entities;
using WCFChat.Contracts.Main.Entities.Contacts;

namespace WCFChat.Contracts.Main
{
	[ServiceContract(Namespace = Namespace.ServicesV001, CallbackContract = typeof(IMainServiceCallback), SessionMode = SessionMode.Allowed)]
    public interface IMainService
    {
        /// <summary>
        /// Создать облако
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cloud"></param>
        /// <param name="transactionID"></param>
        [OperationContract(IsOneWay = true)]
        void CreateCloud(Cloud cloud, string transactionID);

        /// <summary>
        /// Стать самостоятельным сервером и отвязаться от сервера
        /// </summary>
        /// <param name="transactionID"></param>
        [OperationContract(IsOneWay = true)]
        void Unbind(string transactionID);

        /// <summary>
        /// Получить облако
        /// </summary>
        /// <param name="user"></param>
        [OperationContract(IsOneWay = true)]
        void GetCloud(User user, string transactionID);

        [OperationContract(IsOneWay = true)]
        void RemoveOrAccessUser(ServerResult result, User user);

        [OperationContract(IsOneWay = true)]
        void Test(ContactBase baseContact);
    }

}