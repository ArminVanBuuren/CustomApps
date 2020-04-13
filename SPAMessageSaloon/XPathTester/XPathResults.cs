using System.Collections;
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

        public XmlNode Node { get; set; }
    }

    public class XPathCollection : IEnumerable<DGVXPathResult>
    {
        private readonly Dictionary<int, DGVXPathResult> _values = new Dictionary<int, DGVXPathResult>();

        public XPathCollection(IEnumerable<DGVXPathResult> source)
        {
            foreach (var item in source)
                _values.Add(item.ID, item);
        }

        public XPathCollection(IEnumerable<XPathResult> source)
        {
            foreach (var result in source)
            {
                _values.Add(result.ID, new DGVXPathResult()
                {
                    ID = result.ID,
                    NodeType = result.NodeType,
                    NodeName = result.NodeName,
                    Value = result.Value,
                    Node = result.Node
                });
            }
        }

        public DGVXPathResult this[int id] => _values[id];

        public IEnumerator<DGVXPathResult> GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }
    }
}
