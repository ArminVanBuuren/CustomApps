using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogsReader.Config;
using LogsReader.Properties;
using Utils;

namespace LogsReader.Reader
{
	public delegate void ReportProcessStatusHandler(int countMatches, int countErrorMatches, int percentOfProgeress, int filesCompleted, int totalFiles);

	public class LogsReaderPerformerScheme : LogsReaderPerformerFiles
	{
		private readonly object _syncRootResult = new object();

		private Dictionary<DataTemplate, DataTemplate> _result = new Dictionary<DataTemplate, DataTemplate>(new DataTemplatesDuplicateComparer());

		private List<MTActionResult<TraceReader>> _multiTaskingHandlerList = new List<MTActionResult<TraceReader>>();

		private List<List<TraceReader>> TraceReadersOrdered { get; } = new List<List<TraceReader>>();

		public int TotalCount => TraceReaders != null && TraceReaders.Count > 0 ? TraceReaders.Count : 0;

		public int ResultCount => _multiTaskingHandlerList.Sum(x => x.Result.Count);

		public int PercentOfComplete => TotalCount > 0 ? ResultCount * 100 / TotalCount : 0;

		public IEnumerable<DataTemplate> ResultsOfSuccess => _result.Values;

		public List<DataTemplate> ResultsOfError { get; private set; } = new List<DataTemplate>();

		public bool IsCompleted { get; private set; }

		public LogsReaderPerformerScheme(LRSettingsScheme settings,
		                                 string findMessage,
		                                 bool useRegex,
		                                 IReadOnlyDictionary<string, int> servers,
		                                 IReadOnlyDictionary<string, int> fileTypes,
		                                 IReadOnlyDictionary<string, bool> folders,
		                                 DataFilter filter,
		                                 GetUserCredential getUserCredential)
			: base(settings, findMessage, useRegex, servers, fileTypes, folders, filter, getUserCredential)
		{
		}

		public override async Task GetTargetFilesAsync()
		{
			await base.GetTargetFilesAsync();
			if (TraceReaders == null)
				throw new Exception(Resources.Txt_LogsReaderPerformer_FilesNotInitialized);
			if (TraceReaders.Count <= 0)
				throw new Exception(Resources.Txt_LogsReaderPerformer_NoFilesLogsFound);

			TraceReadersOrdered.Clear();
			var id = 0;

			foreach (var traceReaders in TraceReaders.Values.GroupBy(x => x.Priority).OrderBy(x => x.Key).ToList())
			{
				var readersOrders = traceReaders.OrderByDescending(x => x.File.LastWriteTime.Date)
				                                .ThenByDescending(x => x.File.LastWriteTime.Hour)
				                                .ThenBy(x => x.File.LastWriteTime.Minute)
				                                .ThenByDescending(x => x.File.Length)
				                                .ToList();
				foreach (var reader in readersOrders)
					reader.ID = ++id;
				TraceReadersOrdered.Add(readersOrders);
			}
		}

		public async Task StartAsync()
		{
			ClearPreviousProcess();
			if (TraceReadersOrdered.Count == 0 || IsStopPending)
				return;

			try
			{
				foreach (var traceReaders in TraceReadersOrdered)
				{
					if (IsStopPending || HasOutOfMemoryException)
						break;

					// ThreadPriority.Lowest - необходим чтобы не залипал основной поток и не мешал другим процессам
					var maxThreads = MaxThreads <= 0 ? traceReaders.Count : MaxThreads;
					var multiTaskingHandler = new MTActionResult<TraceReader>(ReadData, traceReaders, maxThreads, ThreadPriority.Lowest);
					_multiTaskingHandlerList.Add(multiTaskingHandler);
					
					await multiTaskingHandler.StartAsync();
					var errors = multiTaskingHandler.Result.CallBackList.Where(x => x.Error != null)
					                                .Aggregate(new List<DataTemplate>(),
					                                           (listErr, x) =>
					                                           {
						                                           listErr.Add(new DataTemplate(x.Source, -1, string.Empty, null, x.Error));
						                                           return listErr;
					                                           });
					ResultsOfError.AddRange(errors);
				}
			}
			finally
			{
				TraceReadersOrdered.Clear();
				
				if (HasOutOfMemoryException)
					ClearInternal();
				
				IsCompleted = true;
				
				if (HasOutOfMemoryException)
					throw new OutOfMemoryException("Too large items found.");
			}
		}

		public void ReadData(TraceReader traceReader)
		{
			try
			{
				traceReader.OnFound += AddResult;
				traceReader.Start();
			}
			finally
			{
				traceReader.OnFound -= AddResult;
			}
		}

		/// <summary>
		///     Добавляем найденный трейс
		/// </summary>
		/// <param name="item"></param>
		protected void AddResult(DataTemplate item)
		{
			lock (_syncRootResult)
			{
				if (!_result.TryGetValue(item, out var existingItem))
				{
					_result.Add(item, item);
				}
				else
				{
					// Если существующий трейс корректно спарсен, а новый не получилось спарсить, то оставляем в коллекции сущесвующий коректный
					if (existingItem.IsMatched && !item.IsMatched)
						return;

					// Если найден еще один темплейт в той же строке и том же файле.
					// То перазаписываем, возможно это тот же темплейт только более дополненный.
					_result[item] = item;
				}
			}
		}

		public override void Abort()
		{
			IsStopPending = true;
			if (_multiTaskingHandlerList != null)
				foreach (var tasks in _multiTaskingHandlerList)
					tasks.Stop();
			base.Abort();
		}

		public override void Clear()
		{
			ClearInternal();
			HasOutOfMemoryException = false;
		}

		/// <summary>
		///     Останавливаем и очищаем. Но оставляем ошибку с памятью
		/// </summary>
		private void ClearInternal()
		{
			if (_multiTaskingHandlerList != null)
				foreach (var tasks in _multiTaskingHandlerList)
					tasks.Stop();
			base.Clear();
			ClearPreviousProcess();
			STREAM.GarbageCollectAsync();
		}

		/// <summary>
		///     Очищаем все статусы процессинга
		/// </summary>
		private void ClearPreviousProcess()
		{
			IsCompleted = false;
			IsStopPending = false;
			_multiTaskingHandlerList?.Clear();
			_multiTaskingHandlerList = new List<MTActionResult<TraceReader>>();
			_result?.Clear();
			_result = new Dictionary<DataTemplate, DataTemplate>(new DataTemplatesDuplicateComparer());
			ResultsOfError?.Clear();
			ResultsOfError = new List<DataTemplate>();
		}

		public override void Dispose()
		{
			Clear();
			Reset();
		}
	}
}