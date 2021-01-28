namespace WCFChat.Contracts.Chat
{
	public static class Namespace
	{
		/// <summary>
		/// Базовое пространство имен.
		/// </summary>
		private const string _businessProcess = "http://localhost/Chat/";

		internal const string ServicesV001 = _businessProcess + "Services/v001";

		internal const string EntitiesV001 = _businessProcess + "Entities/v001";

		internal const string ConstantsV001 = _businessProcess + "Constants/v001";
	}
}
