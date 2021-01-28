using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using WCFChat.Contracts.Chat.Entities;
using WCFChat.Contracts.Main.Entities;

namespace WCFChat.Contracts.Chat
{
	public interface IChatServiceCallback
	{
		[OperationContract(IsOneWay = true)]
		void ConnectResult(ServerResult result);

		[OperationContract(IsOneWay = false)]
		void TransferHistory(List<User> users, List<ChatMessage> messages);

		[OperationContract(IsOneWay = false)]
		void Receive(ChatMessage msg);

		[OperationContract(IsOneWay = true)]
		void IsWritingCallback(User client, bool isWriting);

		[OperationContract(IsOneWay = true)]
		void Terminate(Cloud cloud);
	}
}
