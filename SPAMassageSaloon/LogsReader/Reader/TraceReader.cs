using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Utils;

namespace LogsReader.Reader
{
    public delegate void FoundDataTemplate(DataTemplate item);

    public abstract class TraceReader : LogsReaderPerformerBase
    {
	    private bool _searchByTransaction;
        private string _trn = null;

        private Func<string, bool> _IsMatchedFunc;

	    /// <summary>
        /// Сохраненные данный которые не спарсились по основному поиску и по транзакциям
        /// </summary>
        protected Queue<string> PastTraceLines { get; }

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

        protected TraceReader(LogsReaderPerformerBase control, string server, string filePath, string originalFolder) : base(control)
        {
	        PastTraceLines = new Queue<string>(MaxTraceLines);

            Server = server;
            FilePath = filePath;
            File = new FileInfo(filePath);

            OriginalFolder = originalFolder;
            FileNamePartial = IO.GetPartialPath(FilePath, OriginalFolder);

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
                    AddResult(new DataTemplate(this, Found.FoundLineID, Found.TraceMessage, _trn, ex));
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

	                result = new DataTemplate(this, failed?.FoundLineID ?? Lines, input, _trn, ex);
	                return false;
                }

                if (match.Success && match.Value.Length == traceMessage.Length)
                {
                    var current = new DataTemplate(
	                    this,
                        failed?.FoundLineID ?? Lines,
	                    traceParsePattern.GetParsingResult(match),
	                    traceMessage,
                        _trn);

                    if (SearchByTransaction && TransactionPatterns != null && TransactionPatterns.Length > 0)
                    {
	                    try
	                    {
		                    foreach (var transactionParsePattern in TransactionPatterns)
		                    {
			                    var trnMatch = transactionParsePattern.RegexItem.Match(traceMessage);
			                    if (trnMatch.Success)
			                    {
				                    var trnValue = transactionParsePattern.GetParsingResult(trnMatch).Trn;
				                    AddTransactionValue(trnValue);

				                    if (PastTraceLines.Count > 0)
				                    {
                                        // создаем внутренний ридер, для считывания предыдущих записей для поиска текущей транзакции
                                        var innerReader = GetTraceReader((Server, FilePath, OriginalFolder));
					                    innerReader.SearchByTransaction = false; // отменить повторную внутреннюю проверку по транзакциям предыдущих записей
					                    innerReader._trn = trnValue;
					                    innerReader.Lines = (failed?.FoundLineID ?? Lines) - PastTraceLines.Count - 1; // возвращаемся обратно к первой сохраненной строке
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
            foreach (var traceParsePattern in TraceParsePatterns.Select(x => x.RegexItem))
            {
                Match match;
                try
                {
                    match = traceParsePattern.Match(traceMessage);
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