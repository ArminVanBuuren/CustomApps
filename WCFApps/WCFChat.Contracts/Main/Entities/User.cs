using System.Runtime.Serialization;

namespace WCFChat.Contracts.Main.Entities
{
	[DataContract(Namespace = Namespace.EntitiesV001)]
	public class User
	{
		[DataMember(IsRequired = true)]
		public string GUID { get; set; }

		[DataMember(IsRequired = true)]
		public string Name { get; set; }

		[DataMember(IsRequired = true)]
		public string CloudName { get; set; }
	}
}
