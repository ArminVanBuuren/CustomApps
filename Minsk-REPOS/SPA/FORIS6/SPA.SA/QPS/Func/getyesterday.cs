using System;
using System.Xml;
using FORIS.ScenarioProcessing.Base.Utilities.Functions;

namespace FORIS.ScenarioProcessing.Utilities.Functions
{
    public class getyesterday : IDictionaryFunction
    {
        /// <summary>
        /// Вернуть последний день текущего месяца
        /// </summary>
        public String Invoke(XmlDocument request, params object[] args)
        {
            return DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        }
    }
}