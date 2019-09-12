using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using FORIS.ScenarioProcessing.Base.Utilities.Functions;

namespace FORIS.ScenarioProcessing.Utilities.Functions
{
    public class func_adjust_utcoffset : IDictionaryFunction
    {
        /// <summary>
        /// 1234486 корректировать даты ChangeDateTime, ChargingDateTime  
        /// </summary>
        /// <param name="args">func_adjust_utcoffset(Исходная дата, смещение времени абонента)</param>
        /// <returns>Сформированная строка, если ничего не найдено то пустая строка.</returns>
        public String Invoke(XmlDocument request, params object[] args)
        {
            if (args[0] == null || args[0].ToString() == string.Empty)
                return string.Empty;

            string dateStr = args[0].ToString();
            int customerOffset = Convert.ToInt32(args[1].ToString());
            int serverOffset = (int)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes;

            DateTime date = DateTime.ParseExact(dateStr, "MM/dd/yyyy HH:mm:ss", new DateTimeFormatInfo());
            date = date.AddMinutes(serverOffset - customerOffset);

            return date.ToString("MM/dd/yyyy HH:mm:ss");
        }
    }
}
