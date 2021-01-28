using System.Runtime.Serialization;

namespace WCFChat.Contracts.Chat.Entities
{
	[DataContract(Namespace = Namespace.EntitiesV001)]
	public enum ServerResult
	{
		[EnumMember]
		SUCCESS = 0,
		[EnumMember]
		FAILURE = 1,
		[EnumMember]
		CloudNotFound = 2,
		[EnumMember]
		NameIsBusy = 4,
		[EnumMember]
		AwaitConfirmation = 8,
		[EnumMember]
		AccessDenied = 16,
		[EnumMember]
		AccessGranted = 32,
		[EnumMember]
		YourRequestInProgress = 64
	}
}
