using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public DataFilter(DateTime startDate, DateTime endDate, string traceNameFilter, bool traceNameContains, string traceMessageFilter, bool traceMessageContains)
        {
            if (startDate > endDate)
                throw new Exception(Properties.Resources.Txt_DataFilter_ErrDate);

            #region фильтр по дате начала

            StartDate = startDate;
            if (StartDate > DateTime.MinValue)
                _checkStartDate = (input) => input.Date != null && input.Date.Value >= StartDate;
            else
                _checkStartDate = (input) => true;

            #endregion


            #region фильтр по дате конца

            EndDate = endDate;
            if (EndDate < DateTime.MaxValue)
                _checkEndDate = (input) => input.Date != null && input.Date.Value <= EndDate;
            else
                _checkEndDate = (input) => true;

            #endregion


            #region фильтр по полю Trace

            TraceNameFilterList = traceNameFilter.IsNullOrEmptyTrim()
                ? new List<string>()
                : traceNameFilter.Split(',').GroupBy(p => p.Trim(), StringComparer.InvariantCultureIgnoreCase).Where(x => !x.Key.IsNullOrEmptyTrim()).Select(x => x.Key).ToList();

            if (TraceNameFilterList.Any())
                if (traceNameContains)
                    _checkTraceNameFilter = (input) => !input.TraceName.IsNullOrEmptyTrim() && TraceNameFilterList.Any(p => input.TraceName.StringContains(p));
                else
                    _checkTraceNameFilter = (input) => !input.TraceName.IsNullOrEmptyTrim() && !TraceNameFilterList.Any(p => input.TraceName.StringContains(p));
            else
                _checkTraceNameFilter = (input) => true;

            #endregion


            #region фильтр по полю Message

            TraceMessageFilterList = traceMessageFilter.IsNullOrEmptyTrim()
                ? new List<string>()
                : traceMessageFilter.Split(',').GroupBy(p => p.Trim(), StringComparer.InvariantCultureIgnoreCase).Where(x => !x.Key.IsNullOrEmptyTrim()).Select(x => x.Key)
                    .ToList();
            if (TraceMessageFilterList.Any())
                if (traceMessageContains)
                    _checkTraceMessageFilter = (input) => !input.TraceMessage.IsNullOrEmptyTrim() && TraceMessageFilterList.Any(p => input.TraceMessage.StringContains(p));
                else
                    _checkTraceMessageFilter = (input) => !input.TraceMessage.IsNullOrEmptyTrim() && !TraceMessageFilterList.Any(p => input.TraceMessage.StringContains(p));
            else
                _checkTraceMessageFilter = (input) => true;

            #endregion
        }

        public IEnumerable<DataTemplate> FilterCollection(IEnumerable<DataTemplate> input)
        {
            return input.Where(x => _checkStartDate(x) && _checkEndDate(x) && _checkTraceNameFilter(x) && _checkTraceMessageFilter(x));
        }

        /// <summary>
        /// Корявенько работает, т.к. дата lastwrite иногда не совпадает с датой записи из файле
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool IsAllowed(FileInfo file)
        {
            //if (Filter != null && !Filter.IsAllowed(fileLog.File))
            //    continue;

            // если фильтр даты начала больше даты последней записи в файл, то пропускаем
            if (DateTime.Compare(StartDate, file.LastWriteTime) > 0)
                return false;

            // если фильтр даты конца меньше даты создания файла и даты последней записи, то пропускаем
            if (DateTime.Compare(EndDate, file.CreationTime) < 0 && DateTime.Compare(EndDate, file.LastWriteTime) < 0)
                return false;

            return true;
        }

        public bool IsAllowed(DataTemplate input)
        {
            return _checkStartDate(input) && _checkEndDate(input) && _checkTraceNameFilter(input) && _checkTraceMessageFilter(input);
        }
    }
}
