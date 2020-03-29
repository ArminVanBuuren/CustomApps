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
        private readonly Func<DataTemplate, bool> _checkLike;
        private readonly Func<DataTemplate, bool> _checkNotLike;
        private readonly Func<DataTemplate, bool> _checkMessage;

        DateTime StartDate { get; }
        DateTime EndDate { get; }
        List<string> Like { get; }
        List<string> NotLike { get; }
        List<string> Message { get; }

        public DataFilter(DateTime startDate, DateTime endDate, string traceLike, string traceNotLike, string message)
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
            Like = traceLike.IsNullOrEmptyTrim()
                ? new List<string>()
                : traceLike.Split(',').GroupBy(p => p.Trim(), StringComparer.CurrentCultureIgnoreCase).Where(x => !x.Key.IsNullOrEmptyTrim()).Select(x => x.Key).ToList();
            NotLike = traceNotLike.IsNullOrEmptyTrim()
                ? new List<string>()
                : traceNotLike.Split(',').GroupBy(p => p.Trim(), StringComparer.CurrentCultureIgnoreCase).Where(x => !x.Key.IsNullOrEmptyTrim()).Select(x => x.Key).ToList();

            if (Like.Count > 0 && NotLike.Count > 0 && !Like.Except(NotLike).Any())
                throw new Exception(@"Items in  ""Trace Like"" can't be equal items in ""Trace Not Like""! Please remove the same items.");

            if (Like.Any())
                _checkLike = (input) => !input.Trace.IsNullOrEmptyTrim() && Like.Any(p => input.Trace.StringContains(p));
            else
                _checkLike = (input) => true;

            if (NotLike.Any())
                _checkNotLike = (input) => !input.Trace.IsNullOrEmptyTrim() && !NotLike.Any(p => input.Trace.StringContains(p));
            else
                _checkNotLike = (input) => true;
            #endregion


            #region фильтр по полю Message
            Message = message.IsNullOrEmptyTrim()
                ? new List<string>()
                : message.Split(',').GroupBy(p => p.Trim(), StringComparer.CurrentCultureIgnoreCase).Where(x => !x.Key.IsNullOrEmptyTrim()).Select(x => x.Key).ToList();
            if (Message.Any())
                _checkMessage = (input) => !input.EntireTrace.IsNullOrEmptyTrim() && Message.Any(p => input.EntireTrace.StringContains(p));
            else
                _checkMessage = (input) => true;
            #endregion
        }

        public IEnumerable<DataTemplate> FilterCollection(IEnumerable<DataTemplate> input)
        {
            return input.Where(x => _checkStartDate(x) && _checkEndDate(x) && _checkLike(x) && _checkNotLike(x) && _checkMessage(x));
        }

        /// <summary>
        /// Корявенько работает, т.к. дата lastwrite не совпадает с датой записи из файла
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
            return _checkStartDate(input) && _checkEndDate(input) && _checkLike(input) && _checkNotLike(input) && _checkMessage(input);
        }
    }
}
