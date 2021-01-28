using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFChat.Contracts
{
	public static class Namespace
	{
		/// <summary>
		/// Базовое пространство имен.
		/// </summary>
		private const string _businessProcess = "http://schemas.sitels.ru/Foris.OrderManagement.Workflow2";

		/// <summary>
		/// Пространство имен для контрактов.
		/// </summary>
		public const string ContractsV001 = _businessProcess + ".Contracts/v001";

		/// <summary>
		/// Пространство имен для сообщений.
		/// </summary>
		public const string MessagesV001 = _businessProcess + ".Messages/v001";

		/// <summary>
		/// Пространство имен для исключений.
		/// </summary>
		public const string FaultsV001 = _businessProcess + ".Faults/v001";

		/// <summary>
		/// Пространство имен для сервисов.
		/// </summary>
		public const string ServicesV001 = _businessProcess + ".Services/v001";

		/// <summary>
		/// Пространство имен для констант.
		/// </summary>
		public const string ConstV001 = _businessProcess + ".Const/v001";

		/// <summary>
		/// Пространство имен для контрактов.
		/// </summary>
		public const string ContractsV002 = _businessProcess + ".Contracts/v002";

		/// <summary>
		/// Пространство имен для сообщений.
		/// </summary>
		public const string MessagesV002 = _businessProcess + ".Messages/v002";

		/// <summary>
		/// Пространство имен для исключений.
		/// </summary>
		public const string FaultsV002 = _businessProcess + ".Faults/v002";

		/// <summary>
		/// Пространство имен для сервисов.
		/// </summary>
		public const string ServicesV002 = _businessProcess + ".Services/v002";

		/// <summary>
		/// Пространство имен для констант.
		/// </summary>
		public const string ConstV002 = _businessProcess + ".Const/v002";
	}
}
