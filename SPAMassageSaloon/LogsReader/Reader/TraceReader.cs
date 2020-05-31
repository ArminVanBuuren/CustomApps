using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Utils;

namespace LogsReader.Reader
{
    public delegate void FoundDataTemplate(DataTemplate item);

    public abstract class TraceReader : LogsReaderControl
    {
	    private readonly string _originalFolder = null;

	    private bool _searchByTransaction;
        private string _trn = null;

        private Func<string, bool> _IsMatchedFunc;

        protected Queue<string> PastTraceLines { get; }

	    protected DataTemplate Found { get; set; }

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

					    PastTraceLines.Enqueue(input);
					    if (PastTraceLines.Count > MaxTraceLines)
						    PastTraceLines.Dequeue();

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

					    PastTraceLines.Enqueue(input);
					    if (PastTraceLines.Count > MaxTraceLines)
						    PastTraceLines.Dequeue();

					    return false;
				    };
                }
		    }
	    }

	    public event FoundDataTemplate OnFound;

        public string Server { get; }

        public string FileNamePartial { get; }

        public string FilePath { get; }

        public FileInfo File { get; }

        public long Lines { get; protected set; } = 0;

        protected TraceReader(LogsReaderControl control, string server, string filePath, string originalFolder) : base(control)
        {
	        PastTraceLines = new Queue<string>(MaxTraceLines);

            Server = server;
            FilePath = filePath;
            File = new FileInfo(filePath);

            _originalFolder = originalFolder;
            FileNamePartial = IO.GetPartialPath(FilePath, _originalFolder);

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
                        AddResult(result);
                    else
                        AddResult(Found);
                }
            }
            catch (Exception ex)
            {
                if (!Found.IsMatched)
                    AddResult(new DataTemplate(this, Lines, Found.TraceMessage, _trn, ex));
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
            foreach (var pattern in TraceParsePatterns)
            {
                Match match;
                try
                {
	                match = pattern.RegexItem.Match(traceMessage);
                }
                catch (Exception ex)
                {
	                if (throwException)
		                throw;

	                result = new DataTemplate(this, failed?.FoundLineID ?? Lines, input, _trn, ex);
	                return false;
                }

                if (match.Success && match.Value.Length == traceMessage.Length)
                {
                    var current = new DataTemplate(
	                    this,
                        failed?.FoundLineID ?? Lines,
                        match.GetValueByReplacement(pattern.ID),
                        match.GetValueByReplacement(pattern.Date, (value, format) =>
                        {
	                        if (TIME.TryParseAnyDateExtract(value, format, DateTimeStyles.AllowWhiteSpaces, out var customDateParseResult))
		                        return customDateParseResult.ToString(OutputDateFormat);
	                        return value;
                        }),
                        match.GetValueByReplacement(pattern.TraceName),
                        match.GetValueByReplacement(pattern.Description),
                        match.GetValueByReplacement(pattern.Message),
                        traceMessage,
                        _trn);

                    if (SearchByTransaction && TransactionPatterns != null && TransactionPatterns.Length > 0)
                    {
	                    try
	                    {
		                    foreach (var trnPattern in TransactionPatterns)
		                    {
			                    var trnMatch = trnPattern.RegexItem.Match(traceMessage);
			                    if (trnMatch.Success)
			                    {
				                    var trnValue = trnMatch.GetValueByReplacement(trnPattern.Trn);
				                    AddTransactionValue(trnValue);

				                    if (PastTraceLines.Count > 0)
				                    {
					                    var innerReader = GetTraceReader((Server, FilePath, _originalFolder));
					                    innerReader.SearchByTransaction = false;
					                    innerReader._trn = trnValue;
					                    innerReader.Lines = Lines - PastTraceLines.Count - 1;
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

					                    try { innerReader.Commit(); }
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
	                    }
	                    catch (Exception ex)
	                    {
		                    // ignored
	                    }
                    }

                    result = current;
                    return true;
                }
            }

            result = new DataTemplate(this, Lines, traceMessage, _trn);
            return false;
        }

        protected Match IsLineMatch(string input)
        {
            // замена '\r' чинит баг с некорректным парсингом
            var traceMessage = input.Replace("\r", string.Empty);
            foreach (var regexPatt in TraceParsePatterns.Select(x => x.RegexItem))
            {
                Match match;
                try
                {
                    match = regexPatt.Match(traceMessage);
                }
                catch (Exception)
                {
                    return null;
                }
                
                if (match.Success && match.Value.Length == traceMessage.Length)
                    return match;
            }

            return null;
        }

        public override bool Equals(object obj)
        {
	        var isEqual = false;
	        if (obj is TraceReader input)
		        isEqual = FilePath == input.FilePath && Server == input.Server && _originalFolder == input._originalFolder && base.Equals(input);
	        return isEqual;
        }

        public override int GetHashCode()
        {
	        // только файл и базовый хэш настроек
            var hash = File.GetHashCode() + base.GetHashCode();
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