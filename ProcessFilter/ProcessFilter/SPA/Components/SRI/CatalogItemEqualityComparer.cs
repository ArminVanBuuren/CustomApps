using System;
using System.Collections.Generic;
using System.Xml;

namespace SPAFilter.SPA.Components.SRI
{
    class CatalogItemEqualityComparer : IEqualityComparer<XmlNode>
    {
        public bool Equals(XmlNode x, XmlNode y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            var xName = x.Attributes?["name"]?.Value;
            var yName = y.Attributes?["name"]?.Value;

            if (xName == null || yName == null)
                return false;

            return xName.Equals(yName, StringComparison.CurrentCultureIgnoreCase);
        }

        public int GetHashCode(XmlNode obj)
        {
            var name = obj.Attributes?["name"]?.Value;
            return name == null ? obj.GetHashCode() : name.ToLower().GetHashCode();
        }
    }
}
