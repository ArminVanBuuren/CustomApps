namespace LogsReader.Config
{
	public interface ICustomFunction
	{
		string Invoke(string[] args);
	}

	public interface ICustomTraceParse
	{
		/// <summary>
		/// При попытке спарсить целевой фрагмент трейса
		/// </summary>
		TraceParseResult IsTraceMatch(string input);

		/// <summary>
		/// Спрашивает возможность дополнения строки к предыдущему успешно спарсенному трейсу.
		/// Вызывается только когда опциональные параметры StartWith или EndWith не установлены
		/// </summary>
		bool IsLineMatch(string input);
	}
}
