using System.Runtime.Serialization;

namespace WCFChat.Contracts.Chat.Entities
{
	[DataContract(Namespace = Namespace.EntitiesV001)]
	public enum ServerPrivelege
	{
		[EnumMember]
		Admin = 0,
		[EnumMember]
		User = 1
	}
}
