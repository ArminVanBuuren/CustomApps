using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using FORIS.ScenarioProcessing.Base.Utilities.Functions;

namespace FORIS.ScenarioProcessing.Utilities.Functions
{
    public class ff_count_string : IDictionaryFunction
    {
        /// <summary>
        /// Формирование поля FF_COUNT из HLA Bug-567810. 
        /// </summary>
        /// <param name="args">Выполняется без дополнительных параметров. Пример: ifstringformat_scp()</param>
        /// <returns>Сфомированная строка, если ничего не найдено то пустая строка.</returns>
        public String Invoke(XmlDocument request, params object[] args)
        {
            XmlNodeList list = request.SelectNodes("ExtendedRequestMessage/AdditionalData/RegisteredList/FavoritNumberList/*");
            if(list == null || list.Count == 0) list = request.SelectNodes("ExtendedRequestMessage/AdditionalData/EndList/RFS_FP_SERVICE_FAVORITE_NUMBER");
            if (list != null && list.Count > 0)
            {
                Dictionary<string, int> codeList = new Dictionary<string, int>();
                foreach (XmlNode node in list)
                {
                    XmlNode code = node.SelectSingleNode("FavoriteNumberTypeCode");
                    if (code == null) continue;

                    if (codeList.ContainsKey(code.InnerText))
                        codeList[code.InnerText]++;
                    else
                        codeList.Add(code.InnerText, 1);
                }

                return string.Join("|", codeList.Select(p => p.Key + "_" + p.Value));
            }
            return String.Empty;
        }
    }
}
