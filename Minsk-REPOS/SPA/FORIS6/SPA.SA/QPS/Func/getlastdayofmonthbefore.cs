using System;
using System.Xml;
using FORIS.ScenarioProcessing.Base.Utilities.Functions;

namespace FORIS.ScenarioProcessing.Utilities.Functions
{
    public class getlastdayofmonthbefore : IDictionaryFunction
    {
        /// <summary>
        /// Вернуть последний день текущего месяца
        /// </summary>
        public String Invoke(XmlDocument request, params object[] args)
        {
            //DateTime date = DateTime.Now;
            // DateTime lastDayOfMonth = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59);
			
			DateTime dateMinusMonth = DateTime.Today.AddMonths(-1);
			DateTime lastDayMonthBefore = new DateTime(dateMinusMonth.Year, dateMinusMonth.Month, DateTime.DaysInMonth(dateMinusMonth.Year, dateMinusMonth.Month));
            //return lastDayOfMonth.ToString("yyyy.MM.dd HH:mm:ss");
			return lastDayMonthBefore.ToString("yyyy-MM-dd");
        }
    }
}