using System;
using System.Collections.Generic;
using System.Linq;
using LogsReader.Properties;
using Utils;

namespace LogsReader.Reader
{
	public class DataFilter
	{
		private readonly Func<DataTemplate, bool> _checkStartDate;
		private readonly Func<DataTemplate, bool> _checkEndDate;
		private readonly Func<DataTemplate, bool> _checkTraceNameFilter;
		private readonly Func<DataTemplate, bool> _checkTraceMessageFilter;

		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public List<string> TraceNameFilterList { get; }
		public List<string> TraceMessageFilterList { get; }

		public DataFilter(DateTime startDate,
		                  DateTime endDate,
		                  string traceNameFilter,
		                  bool traceNameContains,
		                  string traceMessageFilter,
		                  bool traceMessageContains)
		{
			if (startDate > endDate)
				throw new Exception(Resources.Txt_DataFilter_ErrDate);

			// фильтр по дате начала
			StartDate = startDate;
			if (StartDate > DateTime.MinValue)
				_checkStartDate = input => input.Date != null && input.Date.Value >= StartDate;
			else
				_checkStartDate = input => true;

			// фильтр по дате конца
			EndDate = endDate;
			if (EndDate < DateTime.MaxValue)
				_checkEndDate = input => input.Date != null && input.Date.Value <= EndDate;
			else
				_checkEndDate = input => true;

			// фильтр по полю TraceName
			TraceNameFilterList = traceNameFilter.IsNullOrWhiteSpace()
				? new List<string>()
				: traceNameFilter.Split(',')
				                 .GroupBy(p => p.Trim(), StringComparer.InvariantCultureIgnoreCase)
				                 .Where(x => !x.Key.IsNullOrWhiteSpace())
				                 .Select(x => x.Key)
				                 .ToList();

			if (TraceNameFilterList.Any())
				if (traceNameContains)
					_checkTraceNameFilter = input => !input.TraceName.IsNullOrWhiteSpace() && TraceNameFilterList.Any(p => input.TraceName.StringContains(p));
				else
					_checkTraceNameFilter = input => !input.TraceName.IsNullOrWhiteSpace() && !TraceNameFilterList.Any(p => input.TraceName.StringContains(p));
			else
				_checkTraceNameFilter = input => true;

			// фильтр по полю TraceMessage
			TraceMessageFilterList = traceMessageFilter.IsNullOrWhiteSpace()
				? new List<string>()
				: traceMessageFilter.Split(',')
				                    .GroupBy(p => p.Trim(), StringComparer.InvariantCultureIgnoreCase)
				                    .Where(x => !x.Key.IsNullOrWhiteSpace())
				                    .Select(x => x.Key)
				                    .ToList();

			if (TraceMessageFilterList.Any())
				if (traceMessageContains)
					_checkTraceMessageFilter = input
						=> !input.TraceMessage.IsNullOrWhiteSpace() && TraceMessageFilterList.Any(p => input.TraceMessage.StringContains(p));
				else
					_checkTraceMessageFilter = input
						=> !input.TraceMessage.IsNullOrWhiteSpace() && !TraceMessageFilterList.Any(p => input.TraceMessage.StringContains(p));
			else
				_checkTraceMessageFilter = input => true;
		}

		public List<DataTemplate> FilterCollection(List<DataTemplate> input)
			=> input.Where(x => _checkStartDate(x) && _checkEndDate(x) && _checkTraceNameFilter(x) && _checkTraceMessageFilter(x)).ToList();

		/// <summary>
		///     Проверям по фильтру. Если был задан дополнительный поиск по фильтру
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public bool IsAllowed(DataTemplate input)
			=> _checkStartDate(input) && _checkEndDate(input) && _checkTraceNameFilter(input) && _checkTraceMessageFilter(input);
	}
}