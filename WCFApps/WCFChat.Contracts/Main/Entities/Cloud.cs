using System.Runtime.Serialization;

namespace WCFChat.Contracts.Main.Entities
{
	[DataContract(Namespace = Namespace.EntitiesV001)]
	public class Cloud
	{
		[DataMember(IsRequired = true)]
		public string Name { get; set; }

		[DataMember]
		public string Address { get; set; }
	}
}
