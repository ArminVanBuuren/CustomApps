using System.Collections.Generic;
using System.Xml;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace XPathTester
{
    public class DGVXPathResult
    {
        [DGVColumn(ColumnPosition.Last, "ID")]
        public int ID { get; set; }

        [DGVColumn(ColumnPosition.Last, "NodeType")]
        public string NodeType { get; set; }

        [DGVColumn(ColumnPosition.Last, "NodeName")]
        public string NodeName { get; set; }

        [DGVColumn(ColumnPosition.Last, "Value")]
        public string Value { get; set; }

        [DGVColumn(ColumnPosition.Last, "Node", false)]
        public XmlNode Node { get; set; }
    }

    internal class XPathCollection : List<DGVXPathResult>
    {
        public XPathCollection(IEnumerable<XPathResult> source)
        {
            foreach (var result in source)
            {
                Add(new DGVXPathResult()
                {
                    ID = result.ID,
                    NodeType = result.NodeType,
                    NodeName = result.NodeName,
                    Value = result.Value,
                    Node = result.Node
                });
            }
        }
    }
}
