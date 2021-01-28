using System.Runtime.Serialization;

namespace WCFChat.Contracts.Main.Entities
{
	[DataContract(Namespace = Namespace.EntitiesV001)]
	public enum CloudResult
	{
		[EnumMember]
		SUCCESS = 0,

		[EnumMember]
		FAILURE = 1,

		[EnumMember]
		CloudNotFound = 2,

		[EnumMember]
		CloudIsBusy = 3
	}
}
