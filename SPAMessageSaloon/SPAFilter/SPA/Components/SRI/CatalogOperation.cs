using System.Collections.Generic;
using System.Text;
using System.Xml;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components.SRI
{
    public abstract class CatalogOperation : ObjectTemplate, IOperation
    {
        // Обязательно сделать override в этом классе, т.к. коллекция каталожных операций в основной форме будет приводится именно к типу CatalogOperation, свойство UniqueName должен быть виден
        [DGVColumn(ColumnPosition.First, "UniqueName", false)]
        public override string UniqueName { get; protected set; }

        [DGVColumn(ColumnPosition.First, "HostType")]
        public virtual string HostTypeName { get; protected set; }

        [DGVColumn(ColumnPosition.After, "Operation")]
        public override string Name { get; set; }

        /// <summary>
        /// Если сценарий помечен как sendType="Drop"
        /// </summary>
        protected internal virtual bool IsDropped { get; protected set; } = false;

        /// <summary>
        /// Если сценарий для этой операции существует
        /// </summary>
        public bool IsScenarioExist { get; set; } = true;

        internal virtual RFSBindings Bindings { get; } = null;
        public virtual string Body { get; } = string.Empty;

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