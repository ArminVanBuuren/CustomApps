using System.Runtime.Serialization;

namespace WCFChat.Contracts.Entities
{
	/// <summary>
	/// Представляет контакт "ЭДО".
	/// </summary>
	[DataContract(Namespace = Namespace.ContractsV001)]
	public sealed class EdmContact : ContactBase
	{
		/// <summary>
		/// Возвращает или задает адрес FnsId.
		/// </summary>
		[DataMember]
		public string FnsId { get; set; }

		/// <summary>
		/// Возвращает код типа контакта.
		/// </summary>
		public override string ContactTypeCode => "Edo";
	}
}
