namespace LogsReader.Config
{
	public interface ICustomFunction
	{
		string Invoke(string[] args);
	}

	public interface ICustomTraceParse
	{
		/// <summary>
		///     Парсинг целевого фрагмента трейса
		/// </summary>
		TraceParseResult IsTraceMatch(string input);

		/// <summary>
		///     Запрашивает возможность дополнения строк к предыдущему успешно спарсенному трейсу.
		///     Вызывается только когда опциональные параметры StartWith или EndWith не установлены.
		/// </summary>
		bool IsLineMatch(string input);
	}
}