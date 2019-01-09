using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Utils.XPathHelper
{
    public class XPathResult
    {
        public int ID { get; set; }
        public string NodeType { get; set; }
        public string NodeName { get; set; }
        public string Value { get; set; }
        public XmlNode Node { get; set; }
    }

    public class XPathResultCollection : List<XPathResult>
    {

    }
}
