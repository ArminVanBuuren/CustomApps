using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using SPAFilter.SPA.Collection;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components.SC
{
    public abstract class CatalogOperation : ObjectTemplate, IOperation
    {
        [DGVColumn(ColumnPosition.Before, "HostType")]
        public string HostTypeName { get; protected set; }

        /// <summary>
        /// Сценарий для этой операции существует
        /// </summary>
        [DGVColumn(ColumnPosition.Last, "IsScenarioExist", false)]
        public bool IsScenarioExist { get; set; } = true;

        [DGVColumn(ColumnPosition.Last, "Operation")]
        public override string Name { get; set; }

        internal virtual RFSBindings Bindings { get; } = null;
        public virtual string Body { get; } = string.Empty;

        protected CatalogOperation() { }

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