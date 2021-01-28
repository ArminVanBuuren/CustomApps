using System;
using System.Runtime.Serialization;

namespace WCFChat.Contracts.Entities
{
	/// <summary>
	/// Представляет абстрактный контакт.
	/// </summary>
	[DataContract(Namespace = Namespace.ContractsV001)]
	[KnownType(typeof(EdmContact))]
	public abstract class ContactBase
	{
		/// <summary>
		/// Возвращает или задает Guid контакта.
		/// </summary>
		[DataMember]
		public Guid? Guid { get; set; }

		/// <summary>
		/// Возвращает код типа контакта.
		/// </summary>
		public abstract string ContactTypeCode { get; }
	}
}
