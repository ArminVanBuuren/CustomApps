using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XPathEvaluator
{
    class XPathResults
    {
        public int ID { get; set; }
        public string NodeType { get; set; }
        public string NodeName { get; set; }
        public string Value { get; set; }
        public XmlNode Node { get; set; }
    }
    class XpathCollection : List<XPathResults>
    {

        public string MaxWidthId
        {
            get
            {
                string word = "ID";
                int maxLength = word.Length;
                foreach (XPathResults res in this)
                {
                    if (res.ID.ToString().Length > maxLength)
                    {
                        word = res.ID.ToString();
                        maxLength = word.Length;
                    }
                }
                return word;
            }
        }
        public string MaxWidthNodeType
        {
            get
            {
                string word = "NodeType";
                int maxLength = word.Length;
                foreach (XPathResults res in this)
                {
                    if (res.NodeType.Length > maxLength)
                    {
                        word = res.NodeType;
                        maxLength = word.Length;
                    }
                }
                return word;
            }
        }
        public string MaxWidthNodeName
        {
            get
            {
                string word = "NodeName";
                int maxLength = word.Length;
                foreach (XPathResults res in this)
                {
                    if (res.NodeName.Length > maxLength)
                    {
                        word = res.NodeName;
                        maxLength = word.Length;
                    }
                }
                return word;
            }
        }
    }
}
