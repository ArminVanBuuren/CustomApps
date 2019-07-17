using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using SPAFilter.SPA.Collection;

namespace SPAFilter.SPA.Components.SC
{
    public class CatalogOperation : Operation
    {
        internal virtual RFSBindings Bindings { get; } = null;
        public virtual string Body { get; } = string.Empty;

        public override double FileSize { get; } = 0;
        public override string FilePath { get; } = null;

        public CatalogOperation() { }

        internal static void AppendXmlNode(StringBuilder builder, string parentNodeName, IEnumerable<XmlNode> collection)
        {
            builder.Append("<");
            builder.Append(parentNodeName);
            builder.Append(">");
            foreach (var allBinds in collection)
            {
                builder.Append(allBinds.OuterXml);
            }
            builder.Append("</");
            builder.Append(parentNodeName);
            builder.Append(">");
        }
    }
}