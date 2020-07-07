namespace LogsReader.Config
{
	public class TraceParseResult
	{
		/// <summary>
		/// Логический результат, отвечает только за подсветку в основном гриде. По дефолту всегда true.
		/// </summary>
		public bool IsSuccess { get; set; } = true;

		public string ID { get; set; }
		public string Date { get; set; }
		public string TraceName { get; set; }
		public string Description { get; set; }
		public string Message { get; set; }
	}
}