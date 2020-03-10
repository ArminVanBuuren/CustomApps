using System;
using System.Xml;
using FORIS.ScenarioProcessing.Base.Utilities.Functions;

namespace FORIS.ScenarioProcessing.Utilities.Functions
{
    public class getbalancequota : IDictionaryFunction
    {
        /// <summary>
        /// Вернуть оставшееся количество байт до конца месяца
        /// </summary>
        public String Invoke(params object[] args)
        {
			DateTime now = DateTime.Now;
            int year = now.Year;
            int month = now.Month;
            DateTime lastDayOfMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59);  // получаем последний день месяца
            int secinmonth = DateTime.DaysInMonth(year, month) * 24 * 3600; // получаем сколько секунд в месяце
            TimeSpan left = lastDayOfMonth.Subtract(now);  
            double remsecd = left.TotalSeconds;  // получаем остаток 
            int remsec = (int)Math.Round(remsecd);
            int byteonsec = 250 * (1024 * 1024) / secinmonth; /// сколько байт в сек
            int balancebyte = byteonsec * remsec; // квота.
            return balancebyte.ToString();
        }
    }
}