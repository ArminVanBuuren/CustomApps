using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using FORIS.ScenarioProcessing.Base.Utilities.Functions;

namespace FORIS.ScenarioProcessing.Utilities.Functions
{
    public class getlastdayofmonth : IDictionaryFunction
    {
        /// <summary>
        /// Вернуть последний день текущего месяца
        /// </summary>
        public String Invoke(XmlDocument request, params object[] args)
        {
            DateTime date = DateTime.Now;
            DateTime lastDayOfMonth = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59);
            return lastDayOfMonth.ToString("yyyy-MM-dd");
        }
    }
}