using System.Runtime.Serialization;

namespace WCFChat.Contracts.Main.Entities.Contacts
{
	/// <summary>
	/// Представляет контакт "ЭДО".
	/// </summary>
	[DataContract(Namespace = Chat.Namespace.EntitiesV001)]
	public sealed class EdmContact : ContactBase
	{
		/// <summary>
		/// Возвращает или задает адрес FnsId.
		/// </summary>
		[DataMember]
		public string FnsId { get; set; }

		[DataMember]
		public string TestTest { get; set; }

		/// <summary>
		/// Возвращает код типа контакта.
		/// </summary>
		[DataMember]
		public override string ContactTypeCode => "Edo";
	}
}
