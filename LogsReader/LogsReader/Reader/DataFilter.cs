using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace LogsReader.Reader
{
    public class DataFilter
    {
        private readonly Func<DataTemplate, bool> _checkStartDate;
        private readonly Func<DataTemplate, bool> _checkEndDate;
        private readonly Func<DataTemplate, bool> _checkTraceNameLike;
        private readonly Func<DataTemplate, bool> _checkTraceNameNotLike;
        private readonly Func<DataTemplate, bool> _checkTraceMessage;

        DateTime StartDate { get; }
        DateTime EndDate { get; }
        List<string> TraceNameLikeList { get; }
        List<string> TraceNameNotLikeList { get; }
        List<string> TraceMessageList { get; }

        public DataFilter(DateTime startDate, DateTime endDate, string traceNameLike, string traceNameNotLike, string traceMessage)
        {
            if (startDate > endDate)
                throw new Exception(@"Date of end must be greater than date of start.");

            #region фильтр по дате начала
            StartDate = startDate;
            if (StartDate > DateTime.MinValue)
                _checkStartDate = (input) => input.DateOfTrace != null && input.DateOfTrace.Value >= StartDate;
            else
                _checkStartDate = (input) => true;
            #endregion


            #region фильтр по дате конца
            EndDate = endDate;
            if (EndDate < DateTime.MaxValue)
                _checkEndDate = (input) => input.DateOfTrace != null && input.DateOfTrace.Value <= EndDate;
            else
                _checkEndDate = (input) => true;
            #endregion
            

            #region фильтр по полю Trace
            TraceNameLikeList = traceNameLike.IsNullOrEmptyTrim()
                ? new List<string>()
                : traceNameLike.Split(',').GroupBy(p => p.Trim(), StringComparer.InvariantCultureIgnoreCase).Where(x => !x.Key.IsNullOrEmptyTrim()).Select(x => x.Key).ToList();

            TraceNameNotLikeList = traceNameNotLike.IsNullOrEmptyTrim()
                ? new List<string>()
                : traceNameNotLike.Split(',').GroupBy(p => p.Trim(), StringComparer.InvariantCultureIgnoreCase).Where(x => !x.Key.IsNullOrEmptyTrim()).Select(x => x.Key).ToList();

            if (TraceNameLikeList.Count > 0 && TraceNameNotLikeList.Count > 0 && !TraceNameLikeList.Except(TraceNameNotLikeList).Any())
                throw new Exception(@"Items in ""Trace-name Like"" cannot contain items in ""Trace-name NOT Like""! Please remove the same.");

            if (TraceNameLikeList.Any())
                _checkTraceNameLike = (input) => !input.TraceName.IsNullOrEmptyTrim() && TraceNameLikeList.Any(p => input.TraceName.StringContains(p));
            else
                _checkTraceNameLike = (input) => true;

            if (TraceNameNotLikeList.Any())
                _checkTraceNameNotLike = (input) => !input.TraceName.IsNullOrEmptyTrim() && !TraceNameNotLikeList.Any(p => input.TraceName.StringContains(p));
            else
                _checkTraceNameNotLike = (input) => true;
            #endregion


            #region фильтр по полю Message
            TraceMessageList = traceMessage.IsNullOrEmptyTrim()
                ? new List<string>()
                : traceMessage.Split(',').GroupBy(p => p.Trim(), StringComparer.InvariantCultureIgnoreCase).Where(x => !x.Key.IsNullOrEmptyTrim()).Select(x => x.Key).ToList();
            if (TraceMessageList.Any())
                _checkTraceMessage = (input) => !input.TraceMessage.IsNullOrEmptyTrim() && TraceMessageList.Any(p => input.TraceMessage.StringContains(p));
            else
                _checkTraceMessage = (input) => true;
            #endregion
        }

        public IEnumerable<DataTemplate> FilterCollection(IEnumerable<DataTemplate> input)
        {
            return input.Where(x => _checkStartDate(x) && _checkEndDate(x) && _checkTraceNameLike(x) && _checkTraceNameNotLike(x) && _checkTraceMessage(x));
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
            return _checkStartDate(input) && _checkEndDate(input) && _checkTraceNameLike(input) && _checkTraceNameNotLike(input) && _checkTraceMessage(input);
        }
    }
}
