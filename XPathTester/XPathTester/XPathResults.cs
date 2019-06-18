using System;
using System.Collections.Generic;
using System.Xml;
using Utils;

namespace XPathTester
{
    class XPathCollection : List<XPathResult>
    {
        public XPathCollection()
        {
            MaxWidthId = "ID";
            MaxWidthNodeType = "NodeType";
            MaxWidthNodeName = "NodeName";
        }

        public static explicit operator XPathCollection(XPathResultCollection args)
        {
            if (args == null)
                return null;

            var result = new XPathCollection();

            result.AddRange(args);
            result.CalcColumnsName();

            return result;
        }

        public void CalcColumnsName()
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

        public void ChangeNodeType()
        {
            foreach (var xpathResult in this)
            {
                xpathResult.NodeName = "Empty";

                xpathResult.NodeType = xpathResult.NodeName.GetType().Name;
                if (xpathResult.NodeType.Length > MaxWidthNodeType.Length)
                    MaxWidthNodeType = xpathResult.NodeType;

                xpathResult.Value = xpathResult.NodeName;
            }
        }

        public string MaxWidthId { get; private set; }

        public string MaxWidthNodeType { get; private set; }

        public string MaxWidthNodeName { get; private set; }
    }
}
