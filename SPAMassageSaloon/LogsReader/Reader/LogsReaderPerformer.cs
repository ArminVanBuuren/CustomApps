﻿using System;
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
	public delegate void ReportProcessStatusHandler(int countMatches, int percentOfProgeress, int filesCompleted, int totalFiles);

    public class LogsReaderPerformer : LogsReaderFiles
    {
        private readonly object _syncRootMatches = new object();
        private int _countMatches;

        private readonly object _syncRootResult = new object();
        private readonly SortedDictionary<DataTemplate, DataTemplate> _result = new SortedDictionary<DataTemplate, DataTemplate>(new DataTemplatesDuplicateComparer());

        private MTActionResult<TraceReader> _multiTaskingHandler;

        public event ReportProcessStatusHandler OnProcessReport;

        /// <summary>
        /// Количество совпадений по критериям поиска
        /// </summary>
        public int CountMatches
        {
            get
            {
                lock (_syncRootMatches)
                    return _countMatches;
            }
            internal set
            {
                lock (_syncRootMatches)
                    _countMatches = value;
            }
        }

        public IEnumerable<DataTemplate> ResultsOfSuccess => _result.Values;

        public IEnumerable<DataTemplate> ResultsOfError { get; private set; } = null;

        public DataFilter Filter { get; }

        public LogsReaderPerformer(
	        LRSettingsScheme settings,
	        string findMessage,
	        bool useRegex,
	        IEnumerable<string> servers,
	        IEnumerable<string> fileTypes,
	        IReadOnlyDictionary<string, bool> folders, 
	        DataFilter filter) 
	        :base(settings, findMessage, useRegex, servers, fileTypes, folders)
        {
	        Filter = filter;
        }

        public override async Task StartAsync()
        {
	        if (IsStopPending)
		        return;

	        if (TraceReaders == null)
		        throw new Exception(Resources.Txt_LogsReaderPerformer_FilesNotInitialized);
	        if (TraceReaders.Count <= 0)
		        throw new Exception(Resources.Txt_LogsReaderPerformer_NoFilesLogsFound);

	        // ThreadPriority.Lowest - необходим чтобы не залипал основной поток и не мешал другим процессам
            var readers = TraceReaders.OrderByDescending(x => x.File.LastWriteTime).ThenByDescending(x => x.File.CreationTime).ToList();
            var maxThreads = MaxThreads <= 0 ? TraceReaders.Count : MaxThreads;
	        _multiTaskingHandler = new MTActionResult<TraceReader>(
		        ReadData,
		        readers, 
		        maxThreads, 
		        ThreadPriority.Lowest);

	        new Action(CheckProgress).BeginInvoke(ProcessCompleted, null);
	        await _multiTaskingHandler.StartAsync();

	        ResultsOfError = _multiTaskingHandler?.Result.CallBackList
		        .Where(x => x.Error != null)
		        .Aggregate(new List<DataTemplate>(), (listErr, x) =>
	        {
		        listErr.Add(new DataTemplate(x.Source, -1, string.Empty, null, x.Error));
		        return listErr;
	        });
        }

        public void ReadData(TraceReader fileLog)
        {
            try
            {
	            if (IsStopPending)
		            return;

                fileLog.OnFound += AddResult;

                // FileShare должен быть ReadWrite. Иначе, если файл используется другим процессом то доступ к чтению файла будет запрещен.
                using (var inputStream = new FileStream(fileLog.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
	                if (IsStopPending)
		                return;

                    using (var inputReader = new StreamReader(inputStream, Encoding))
	                {
		                string line;
		                while ((line = inputReader.ReadLine()) != null && !IsStopPending)
		                {
			                fileLog.ReadLine(line);
		                }
	                }
                }
            }
            finally
            {
                try { fileLog.Commit(); }
                catch
                {
                    // ignored
                }

                fileLog.OnFound -= AddResult;
            }
        }

        protected void AddResult(DataTemplate item)
        {
            if (Filter != null && !Filter.IsAllowed(item))
                return;

            if (item.Error == null)
                ++CountMatches;

            lock (_syncRootResult)
            {
	            if (!_result.ContainsKey(item))
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
		            
	            }
            }
        }

        public override void Stop()
        {
	        base.Stop();
            _multiTaskingHandler?.Stop();
        }

        public override void Reset()
        {
	        _multiTaskingHandler?.Stop();
	        _multiTaskingHandler = null;
	        CountMatches = 0;
	        _result.Clear();
	        ResultsOfError = null;
            base.Reset();
        }

        void CheckProgress()
        {
            try
            {
                var total = _multiTaskingHandler.Source.Count;
                while (_multiTaskingHandler != null && !_multiTaskingHandler.IsCompleted && !IsDisposed)
                {
                    OnProcessReport?.Invoke(CountMatches, _multiTaskingHandler.PercentOfComplete, _multiTaskingHandler.Result.Count, total);
                    Thread.Sleep(10);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        void ProcessCompleted(IAsyncResult asyncResult)
        {
            try
            {
                OnProcessReport?.Invoke(CountMatches, 100, _multiTaskingHandler.Result.Count, _multiTaskingHandler.Source.Count());
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public override void Dispose()
        {
	        Reset();
	        base.Dispose();
        }
    }
}
