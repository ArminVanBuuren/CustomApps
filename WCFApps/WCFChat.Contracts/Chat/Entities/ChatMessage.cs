using System;
using System.Runtime.Serialization;
using WCFChat.Contracts.Main.Entities;

namespace WCFChat.Contracts.Chat.Entities
{
	[DataContract(Namespace = Namespace.EntitiesV001)]
	public class ChatMessage
	{
		[DataMember(IsRequired = true)]
		public User Sender { get; set; }

		[DataMember]
		public string Content { get; set; }

		[DataMember]
		public DateTime Time { get; set; }
	}
}
