using System.Collections;
using System.Collections.Generic;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace XPathTester
{
    public class DGVXPathResult
    {
	    public XPathNavigatorResult Navigator { get; }

        [DGVColumn(ColumnPosition.Last, "ID")] 
	    public int ID => Navigator.ID;

	    [DGVColumn(ColumnPosition.Last, "NodeType")]
	    public string NodeType => Navigator.NodeType;

	    [DGVColumn(ColumnPosition.Last, "Name")]
	    public string Name => Navigator.NodeName;

	    [DGVColumn(ColumnPosition.Last, "Value")]
	    public string Value => Navigator.Value?.Trim().Replace("\r", "").Replace("\n", "");

	    public DGVXPathResult(XPathNavigatorResult navigator) => Navigator = navigator;
    }

    public class XPathCollection : IEnumerable<DGVXPathResult>
    {
        private readonly Dictionary<int, DGVXPathResult> _values = new Dictionary<int, DGVXPathResult>();

        public XPathCollection(IEnumerable<DGVXPathResult> source)
        {
            foreach (var item in source)
                _values.Add(item.ID, item);
        }

        public XPathCollection(IEnumerable<XPathNavigatorResult> source)
        {
	        foreach (var result in source)
		        _values.Add(result.ID, new DGVXPathResult(result));
        }

        public DGVXPathResult this[int id] => _values.TryGetValue(id, out var result) ? result : null;

        public IEnumerator<DGVXPathResult> GetEnumerator() => _values.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _values.Values.GetEnumerator();
    }
}
