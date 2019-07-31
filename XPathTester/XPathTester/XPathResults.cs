using System.Collections.Generic;
using Utils;

namespace XPathTester
{
    class XPathCollection : List<XPathResult>
    {
        public XPathCollection(IEnumerable<XPathResult> source): base(source)
        {
            MaxWidthId = "ID";
            MaxWidthNodeType = "NodeType";
            MaxWidthNodeName = "NodeName";
            CalcColumnsName();
        }

        public new void Add(XPathResult item)
        {
            if (item.ID.ToString().Length > MaxWidthId.Length)
                MaxWidthId = item.ID.ToString();

            if (item.NodeType.Length > MaxWidthNodeType.Length)
                MaxWidthNodeType = item.NodeType;

            if (item.NodeName.Length > MaxWidthNodeName.Length)
                MaxWidthNodeName = item.NodeName;

            base.Add(item);
        }

        public new void AddRange(IEnumerable<XPathResult> items)
        {
            base.AddRange(items);
            CalcColumnsName();
        }

        void CalcColumnsName()
        {
            foreach (var xpathResult in this)
            {
                if (xpathResult.ID.ToString().Length > MaxWidthId.Length)
                    MaxWidthId = xpathResult.ID.ToString();

                if (xpathResult.NodeType.Length > MaxWidthNodeType.Length)
                    MaxWidthNodeType = xpathResult.NodeType;

                if (xpathResult.NodeName.Length > MaxWidthNodeName.Length)
                    MaxWidthNodeName = xpathResult.NodeName;
            }
        }

        public void ModifyValueToNodeName()
        {
            MaxWidthNodeType = "NodeType";

            foreach (var xpathResult in this)
            {
                xpathResult.Value = xpathResult.NodeName;

                xpathResult.NodeName = "Empty";

                xpathResult.NodeType = xpathResult.NodeName.GetType().Name;
                if (xpathResult.NodeType.Length > MaxWidthNodeType.Length)
                    MaxWidthNodeType = xpathResult.NodeType;
            }
        }

        public string MaxWidthId { get; private set; }

        public string MaxWidthNodeType { get; private set; }

        public string MaxWidthNodeName { get; private set; }
    }
}
