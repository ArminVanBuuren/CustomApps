using System.Collections.Generic;
using System.Xml;
using Utils;

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

            return xName.Like(yName);
        }

        public int GetHashCode(XmlNode obj)
        {
	        unchecked
            {
	            var hash = 12;
	            var name = obj.Attributes?["name"]?.Value;
                hash = hash * 17 + (name == null ? obj.GetHashCode() : name.ToLower().GetHashCode());
	            return hash;
            }
        }
    }
}
