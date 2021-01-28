using System.ServiceModel;
using WCFChat.Contracts.Chat.Entities;
using WCFChat.Contracts.Main.Entities;

namespace WCFChat.Contracts.Main
{
	public interface IMainServiceCallback
	{
		/// <summary>
		/// Результат создания облака на основном сервере, через который можно подключаться к серверу чата
		/// </summary>
		/// <param name="result"></param>
		/// <param name="transactionID"></param>
		[OperationContract(IsOneWay = true)]
		void CreateCloudResult(CloudResult result, string transactionID);

		/// <summary>
		/// Получить адрес облака к которому хочет подконнектиться юзер
		/// </summary>
		/// <param name="user"></param>
		/// <param name="address"></param>
		[OperationContract(IsOneWay = true)]
		void RequestForAccess(User user, string address);

		/// <summary>
		/// Вернукть результат рецепиенту который запросил войти в облако
		/// </summary>
		/// <param name="result"></param>
		/// <param name="cloud"></param>
		[OperationContract(IsOneWay = true)]
		void GetCloudResult(ServerResult result, Cloud cloud, string transactionID);
	}
}
