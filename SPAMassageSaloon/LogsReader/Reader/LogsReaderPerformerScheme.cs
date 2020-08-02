using System;
using System.Collections.Generic;
using System.IO;
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
		private SortedDictionary<DataTemplate, DataTemplate> _result = new SortedDictionary<DataTemplate, DataTemplate>(new DataTemplatesDuplicateComparer());
		private List<MTActionResult<TraceReader>> _multiTaskingHandlerList = new List<MTActionResult<TraceReader>>();

		private List<List<TraceReader>> TraceReadersOrdered { get; } = new List<List<TraceReader>>();

		public int TotalCount => TraceReaders != null && TraceReaders.Count > 0 ? TraceReaders.Count : 0;

		public int ResultCount => _multiTaskingHandlerList.Sum(x => x.Result.Count);

		public int PercentOfComplete => TotalCount > 0 ? (ResultCount * 100) / TotalCount : 0;

		public IEnumerable<DataTemplate> ResultsOfSuccess => _result.Values;

		public List<DataTemplate> ResultsOfError { get; private set; } = new List<DataTemplate>();

		public DataFilter Filter { get; }

		public bool IsCompleted { get; private set; } = false;

		public LogsReaderPerformerScheme(
			LRSettingsScheme settings,
			string findMessage,
			bool useRegex,
			IReadOnlyDictionary<string, int> servers,
			IReadOnlyDictionary<string, int> fileTypes,
			IReadOnlyDictionary<string, bool> folders,
			DataFilter filter)
			: base(settings, findMessage, useRegex, servers, fileTypes, folders)
		{
			Filter = filter;
		}

		public override async Task GetTargetFilesAsync()
		{
			await base.GetTargetFilesAsync();

			if (TraceReaders == null)
				throw new Exception(Resources.Txt_LogsReaderPerformer_FilesNotInitialized);

			if (TraceReaders.Count <= 0)
				throw new Exception(Resources.Txt_LogsReaderPerformer_NoFilesLogsFound);

			foreach (var traceReaders in TraceReaders.Values.GroupBy(x => x.Priority).OrderBy(x => x.Key).ToList())
			{
				var readersOrders = traceReaders
					.OrderByDescending(x => x.File.LastWriteTime.Date)
					.ThenByDescending(x => x.File.LastWriteTime.Hour)
					.ThenByDescending(x => x.File.Length)
					.ToList();

				TraceReadersOrdered.Add(readersOrders);
			}
		}

		public override async Task StartAsync()
		{
			ClearPreviousProcess();

			if(TraceReadersOrdered.Count == 0 || IsStopPending)
				return;

			try
			{
				foreach (var traceReaders in TraceReadersOrdered)
				{
					if (IsStopPending)
						break;

					// ThreadPriority.Lowest - необходим чтобы не залипал основной поток и не мешал другим процессам
					var maxThreads = MaxThreads <= 0 ? traceReaders.Count : MaxThreads;
					var multiTaskingHandler = new MTActionResult<TraceReader>(
						ReadData,
						traceReaders,
						maxThreads,
						ThreadPriority.Lowest);

					_multiTaskingHandlerList.Add(multiTaskingHandler);

					await multiTaskingHandler.StartAsync();

					var errors = multiTaskingHandler.Result.CallBackList
						.Where(x => x.Error != null)
						.Aggregate(new List<DataTemplate>(), (listErr, x) =>
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
				STREAM.GarbageCollect();
				IsCompleted = true;
			}
		}

		void ClearPreviousProcess()
		{
			IsCompleted = false;
			_multiTaskingHandlerList = new List<MTActionResult<TraceReader>>();
			_result = new SortedDictionary<DataTemplate, DataTemplate>(new DataTemplatesDuplicateComparer());
			ResultsOfError = new List<DataTemplate>();
		}

		public void ReadData(TraceReader traceReader)
		{
			try
			{
				if (IsStopPending)
				{
					traceReader.Status = TraceReaderStatus.Cancelled;
					return;
				}

				if (!File.Exists(traceReader.FilePath))
				{
					traceReader.Status = TraceReaderStatus.Finished;
					return;
				}

				traceReader.OnFound += AddResult;
				traceReader.ThreadId = Task.CurrentId?.ToString();

				// FileShare должен быть ReadWrite. Иначе, если файл используется другим процессом то доступ к чтению файла будет запрещен.
				using (var inputStream = new FileStream(traceReader.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					if (IsStopPending)
					{
						traceReader.Status = TraceReaderStatus.Cancelled;
						return;
					}

					using (var streamReader = new StreamReader(inputStream, Encoding))
					{
						traceReader.Status = TraceReaderStatus.Processing;
						string line;
						while ((line = streamReader.ReadLine()) != null)
						{
							traceReader.ReadLine(line);

							if (IsStopPending)
							{
								traceReader.Status = TraceReaderStatus.Cancelled;
								return;
							}
						}
					}
				}

				traceReader.Status = TraceReaderStatus.Finished;
			}
			catch (Exception)
			{
				traceReader.Status = TraceReaderStatus.Failed;
				throw;
			}
			finally
			{
				try
				{
					traceReader.Commit();
					traceReader.Clear();
				}
				catch
				{
					// ignored
				}

				traceReader.OnFound -= AddResult;
			}
		}

		/// <summary>
		/// Добавляем найденный трейс
		/// </summary>
		/// <param name="item"></param>
		protected void AddResult(DataTemplate item)
		{
			if (Filter != null && !Filter.IsAllowed(item))
				return;

			lock (_syncRootResult)
			{
				if (!_result.TryGetValue(item, out var existingItem))
				{
					var dateCurrent = item.Date ?? DateTime.MinValue;
					if (_result.Count >= RowsLimit)
					{
						var latest = _result.First().Key;
						if (latest.Date == null || latest.Date > dateCurrent)
							return;

						_result.Remove(latest);
						_result.Add(item, item);
					}
					else
					{
						_result.Add(item, item);
					}
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

		public override void Stop()
		{
			base.Stop();
			if (_multiTaskingHandlerList.Count > 0)
				foreach (var tasks in _multiTaskingHandlerList)
					tasks.Stop();
		}

		public override void Reset()
		{
			Stop();
			_result.Clear();
			ResultsOfError = null;
			base.Reset();
		}

		public override void Dispose()
		{
			if (TraceReaders != null)
				foreach (var reader in TraceReaders.Values)
					reader.Dispose();

			Reset();
			base.Dispose();
		}
	}
}