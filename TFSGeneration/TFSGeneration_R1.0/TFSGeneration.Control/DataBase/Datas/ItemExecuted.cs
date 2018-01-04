﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TFSGeneration.Control.DataBase.Datas
{
    [Serializable]
    public class ItemExecuted
    {
        [XmlElement("DataMail")]
        public List<DataMail> MailParcedItems { get; set; } = new List<DataMail>();


        [XmlAttribute]
        public string TFSID { get; set; }

        /// <summary>
        /// Основной метод для замены спец функций из текста значения
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal string ReplaceParcedValues(string source)
        {
            string _result = source;

            foreach (DataMail mailParceItem in MailParcedItems)
            {
                _result = new Regex(string.Format(@"%\s*{0}\s*%", mailParceItem.Name), RegexOptions.IgnoreCase).Replace(_result, mailParceItem.Value);
            }

            _result = Utils.GetCustomFuncResult(_result);
            _result = _result.Replace(@"\r\n", Environment.NewLine).Replace(@"\r", "\r").Replace(@"\n", "\n");


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
