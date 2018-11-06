using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace TFSAssist.Control.DataBase.Datas
{
    [Serializable]
    public class ItemExecuted
    {
        [XmlElement("DataMail")]
        public List<DataMail> MailParcedItems { get; set; } = new List<DataMail>();


        [XmlAttribute]
        public string TFSID { get; set; }

        /// <summary>
        /// Основной метод для замены динамические параметров из шаблона на спец функций или спарсенные параметры из запросы
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal string ReplaceParcedValues(string source)
        {
            string _result = source;

            //https://doremifaso.ca/archives/unicode/latin1.html - таблицы со спец символами html
            // реплейсим спец символы xml в обычный текст
            while (_result.IndexOf("&amp;", StringComparison.Ordinal) != -1)
            {
                _result = _result.Replace(@"&amp;", @"&");
            }
            _result = _result.Replace(@"\r", "\r").Replace(@"\n", "\n").Replace(@"&lt;", @"<").Replace(@"&gt;", @">").Replace(@"&quot;", "\"").Replace(@"&apos;", @"'").Replace("&#xD;","\r").Replace("&#xA;", "\n");

            // реплейсим динамические параметры на спарсенные параметры из запроса, по ситаксису %ParceBody_TITLE%
            foreach (DataMail mailParceItem in MailParcedItems)
            {
                _result = new Regex(string.Format(@"%\s*{0}\s*%", mailParceItem.Name), RegexOptions.IgnoreCase).Replace(_result, mailParceItem.Value);
            }

            // реплейсим динамические параметры на результат функций, по ситаксису %now%
            _result = Utils.GetCustomFuncResult(_result);

            return _result;
        }

        public override string ToString()
        {
            return MailParcedItems.Aggregate(string.Empty, (current, data) => current + (data + ";\r\n")).Trim();
        }
    }



    [Serializable]
    public class DataMail
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Value { get; set; }

        public override string ToString ()
        {
            return string.Format("[{0}]={1}", Name, Value);
        }
    }
}
