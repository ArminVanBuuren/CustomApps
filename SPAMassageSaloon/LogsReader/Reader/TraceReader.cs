using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Utils;

namespace LogsReader.Reader
{
	public enum TraceReaderSearchType
	{
		ByCustomFunctions = 0,
		ByRegexPatterns = 1,
		ByBoth = 3
	}

	public delegate void FoundDataTemplate(DataTemplate item);

    public abstract class TraceReader : LogsReaderPerformerBase
    {
	    private bool _searchByTransaction;
        private string _trn = null;

        private Func<string, bool> _IsMatchedFunc;

	    /// <summary>
        /// Сохраненные данный которые не спарсились по основному поиску и по транзакциям
        /// </summary>
        protected Queue<string> PastTraceLines { get; set; }

	    /// <summary>
        /// Прошлый успешный результат, который возможно будет дополняться
        /// </summary>
	    protected DataTemplate Found { get; set; }

        /// <summary>
        /// Текущая транзакция, используется для DataTempalte 
        /// </summary>
	    protected string CurrentTransactionValue => _trn;

	    /// <summary>
	    /// Разрешить поиск по транзакциям
	    /// </summary>
	    protected bool SearchByTransaction
	    {
		    get => _searchByTransaction;
		    set
		    {
			    _searchByTransaction = value;
			    if (_searchByTransaction)
			    {
				    _IsMatchedFunc = (input) =>
				    {
					    _trn = null;
					    if (IsMatch.Invoke(input) || IsMatchByTransactions.Invoke(input, out _trn))
						    return true;

					    return false;
                    };
			    }
			    else
			    {
				    _trn = null;
                    _IsMatchedFunc = (input) =>
				    {
					    if (IsMatch.Invoke(input))
						    return true;

					    return false;
				    };
                }
		    }
	    }

        /// <summary>
        /// Генерирует событие при найденном трейсе
        /// </summary>
	    public event FoundDataTemplate OnFound;

        public string Server { get; }

        /// <summary>
        /// Отсекается часть пути из первичных настроек (OriginalFolder) от основного пути к файлу (FilePath)
        /// Пример - отсекается "\\LOCALHOST\C$\TEST" от "\\LOCALHOST\C$\TEST\soapcon.log" - получается soapcon.log
        /// </summary>
        public string FileNamePartial { get; }

        /// <summary>
        /// Содержит в себе полный путь к файлу, включая сервер "\\LOCALHOST\C$\TEST\soapcon.log"
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Содержит в себе исходную настройку пути к директории "C:\TEST"
        /// </summary>
        public string OriginalFolder { get; }

        public FileInfo File { get; }

        public long Lines { get; protected set; } = 0;

        protected void AddLine(string input)
		{
			PastTraceLines.Enqueue(input);
			if (PastTraceLines.Count > MaxTraceLines)
				PastTraceLines.Dequeue();

			Lines++;
		}

        public TraceReaderSearchType SearchType { get; }

		protected TraceReader(LogsReaderPerformerBase control, string server, string filePath, string originalFolder) : base(control)
		{
			PastTraceLines = new Queue<string>(MaxTraceLines);

			Server = server;
			FilePath = filePath;
			File = new FileInfo(filePath);

			OriginalFolder = originalFolder;
			FileNamePartial = IO.GetPartialPath(FilePath, OriginalFolder);

			if (TraceParsePatterns != null && TraceParsePatterns.Length > 0 && TraceParseCustomFunction != null)
				SearchType = TraceReaderSearchType.ByBoth;
			else if (TraceParsePatterns != null && TraceParsePatterns.Length > 0)
				SearchType = TraceReaderSearchType.ByRegexPatterns;
			else if (TraceParseCustomFunction != null)
				SearchType = TraceReaderSearchType.ByCustomFunctions;

			SearchByTransaction = TransactionPatterns != null && TransactionPatterns.Length > 0;
		}

		public abstract void ReadLine(string line);

        protected bool IsMatched(string input)
        {
	        return _IsMatchedFunc.Invoke(input);
        }

        public void Commit()
        {
            if (Found == null)
                return;

            try
            {
                if (!Found.IsMatched)
                {
                    if (IsTraceMatch(Found.TraceMessage, out var result, Found, true))
                    {
	                    AddResult(result);
                    }
                    else
                    {
	                    TransactionsSearch(Found.TraceMessage, Found);
						AddResult(Found);
                    }
                }
            }
            catch (Exception ex)
            {
	            if (!Found.IsMatched)
	            {
		            Found = new DataTemplate(this, Found.FoundLineID, Found.TraceMessage, _trn, ex);
		            TransactionsSearch(Found.TraceMessage, Found);
					AddResult(Found);
	            }
	            throw;
            }
            finally
            {
                Found = null;
            }
        }

        protected void AddResult(DataTemplate item)
        {
            OnFound?.Invoke(item);
        }

        protected bool IsTraceMatch(string input, out DataTemplate result, DataTemplate failed = null, bool throwException = false)
        {
            // замена '\r' чинит баг с некорректным парсингом
            var traceMessage = input.Replace("\r", string.Empty);
            var length = traceMessage.Split('\n').Length;
			var foundLineId = Lines - length + 1;

            switch (SearchType)
            {
	            case TraceReaderSearchType.ByRegexPatterns:
		            if (IsTraceMatchByRegexPatterns(traceMessage, foundLineId, out result, throwException))
			            return true;
		            break;
				case TraceReaderSearchType.ByCustomFunctions:
					if (IsTraceMatchByCustomFunction(traceMessage, foundLineId, out result))
						return true;
					break;
				case TraceReaderSearchType.ByBoth:
					if (IsTraceMatchByRegexPatterns(traceMessage, foundLineId, out result, throwException))
						return true;
					if (IsTraceMatchByCustomFunction(traceMessage, foundLineId, out result))
						return true;
					break;
				default:
					throw new NotSupportedException();
            }

            result = new DataTemplate(this, foundLineId, traceMessage, _trn);
            return false;
        }

        bool IsTraceMatchByCustomFunction(string traceMessage, long foundLineId, out DataTemplate result)
        {
			var traceParceResult = TraceParseCustomFunction.Value.Item1.Invoke(traceMessage);
			if (traceParceResult != null)
			{
				var current = new DataTemplate(
					this,
					foundLineId,
					traceParceResult,
					traceMessage,
					_trn);

				TransactionsSearch(traceMessage, current);

				result = current;
				return true;
			}

			result = null;
	        return false;
        }

		bool IsTraceMatchByRegexPatterns(string traceMessage, long foundLineId, out DataTemplate result, bool throwException)
        {
	        foreach (var traceParsePattern in TraceParsePatterns)
	        {
		        Match match;
		        try
		        {
			        match = traceParsePattern.RegexItem.Match(traceMessage);
		        }
		        catch (Exception ex)
		        {
			        if (throwException)
				        throw;

			        result = new DataTemplate(this, foundLineId, traceMessage, _trn, ex);
			        return false;
		        }

		        if (!match.Success || match.Value.Length != traceMessage.Length)
			        continue;

		        var current = new DataTemplate(
			        this,
			        foundLineId,
			        traceParsePattern.GetParsingResult(match),
			        traceMessage,
			        _trn);

		        TransactionsSearch(traceMessage, current);

		        result = current;
		        return true;
	        }

	        result = null;
	        return false;
        }

        /// <summary>
		/// поиск по транзакциям
		/// </summary>
		/// <param name="traceMessage">Успешно спарсенный трейс</param>
		/// <param name="current">Успешно созданный темплейт</param>
		void TransactionsSearch(string traceMessage, DataTemplate current)
        {
	        if (!SearchByTransaction || TransactionPatterns == null || TransactionPatterns.Length <= 0)
		        return;

	        try
            {
	            foreach (var transactionParsePattern in TransactionPatterns)
	            {
		            // пытаемся из успешного результата найти транзакцию
		            var trnMatch = transactionParsePattern.RegexItem.Match(traceMessage);
		            if (!trnMatch.Success)
			            continue;

		            // Текущая транзакция. Подставляется из группировок regex replace mode
		            var trnValue = transactionParsePattern.GetParsingResult(trnMatch).Trn;
		            if (!AddTransactionValue(trnValue)) // добавляем новую транзакцию в общую коллекцию спарсенных транзакций
			            break; // если транзакция найдена, но по результатам replace mode значение пустое, или транзакция уже была в списках, то завершаем поиск транзакций

		            // если сохранились предыдущие строки, то ищем текущую транзакцию в предыдущих строках
		            if (PastTraceLines.Count > 0)
		            {
			            // создаем внутренний ридер, для считывания предыдущих записей для поиска текущей транзакции
			            var innerReader = GetTraceReader((Server, FilePath, OriginalFolder));
			            innerReader.SearchByTransaction = false; // отменить повторную внутреннюю проверку по транзакциям предыдущих записей
			            innerReader._trn = trnValue;
			            innerReader.Lines = Lines - PastTraceLines.Count - current.CountOfLines; // возвращаемся обратно к первой сохраненной строке
			            innerReader.ResetMatchFunc(Regex.Escape(trnValue), true);

			            void OnPastFound(DataTemplate pastItem)
			            {
				            if (pastItem.IsMatched && !pastItem.Equals(current))
				            {
					            AddResult(pastItem);
				            }
			            }

			            innerReader.OnFound += OnPastFound;
			            foreach (var pastLine in PastTraceLines)
			            {
				            innerReader.ReadLine(pastLine);
			            }

			            try
			            {
				            innerReader.Commit();
			            }
			            catch (Exception)
			            {
				            // ignored
			            }

			            innerReader.OnFound -= OnPastFound;

			            // очищаем предыдущие данные, т.к. трейс успешно был спарсен в текущем контексте
			            // и также в дочернем вызове был произведен поиск по транзакциям
			            PastTraceLines.Clear();
		            }

		            break;
	            }
            }
            catch (Exception)
            {
	            // ignored
            }
        }

        protected bool IsLineMatch(string input)
        {
	        // замена '\r' чинит баг с некорректным парсингом
	        var traceMessage = input.Replace("\r", string.Empty);

			switch (SearchType)
			{
				case TraceReaderSearchType.ByRegexPatterns:
					return IsLineMatchByRegexPatterns(traceMessage);
				case TraceReaderSearchType.ByCustomFunctions:
					return IsLineMatchByCustomFunction(traceMessage);
				case TraceReaderSearchType.ByBoth:
					if (IsLineMatchByRegexPatterns(traceMessage))
						return true;
					if (IsLineMatchByCustomFunction(traceMessage))
						return true;
					break;
				default:
					throw new NotSupportedException();
			}

			return false;
        }

        protected bool IsLineMatchByCustomFunction(string traceMessage)
        {
	        return TraceParseCustomFunction.Value.Item2.Invoke(traceMessage);
        }

        protected bool IsLineMatchByRegexPatterns(string traceMessage)
        {
	        foreach (var traceParsePattern in TraceParsePatterns.Select(x => x.RegexItem))
	        {
		        Match match;
		        try
		        {
			        match = traceParsePattern.Match(traceMessage);
		        }
		        catch (Exception)
		        {
			        return false;
		        }

		        if (match.Success && match.Value.Length == traceMessage.Length)
			        return true;
	        }

			return false;
        }

		public override bool Equals(object obj)
        {
	        var isEqual = false;
	        if (obj is TraceReader input)
		        isEqual = FilePath == input.FilePath && base.Equals(input);
	        return isEqual;
        }

        /// <summary>
        /// хэш только полного пути к файлу и базовый хэш
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
	        var hash = FilePath.GetHashCode() + base.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return FilePath;
        }

        public override void Dispose()
        {
	        PastTraceLines.Clear();
	        base.Dispose();
        }
    }
}