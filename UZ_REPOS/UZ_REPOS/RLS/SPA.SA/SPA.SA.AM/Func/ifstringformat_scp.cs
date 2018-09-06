using System;
using System.Xml;
using FORIS.ScenarioProcessing.Base.Utilities.Functions;

namespace FORIS.ScenarioProcessing.Utilities.Functions
{
    public class ifstringformat_scp : IDictionaryFunction
    {
        /// <summary>
        /// Выбор по условию из HLA Bug-524675. Некорректное формирование кода группы ЗГП
        /// </summary>
        /// <param name="args">параметры. Пример: ifstringformat_scp(conditionValue, postValue)</param>
        /// <returns>conditionValue, если не пустой. Иначе postValue</returns>
        public String Invoke(XmlDocument request, params object[] args)
        {
            string conditionValue = args[0].ToString();
            string postValue = args[1].ToString();

            return conditionValue != ""
                       ? conditionValue
                       : postValue;
        }
    }
}
