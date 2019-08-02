using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using SPAFilter.SPA.Collection;
using Utils;
using Utils.CollectionHelper;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components.SRI
{
    public sealed class RFSOperation : CatalogOperation
    {
        private readonly XPathNavigator _navigator;
        private RFSBindings _bindings;

        public string RFSName { get; }
        public string LinkType { get; set; }

        internal bool IsSubscription { get; set; } = false;
        protected internal override bool IsDropped => ChildRFS.Count == 0 && ChildCFS.Count == 0 && IncludedToScenario.Count == 0;

        public XmlNode Node { get; }
        public DistinctList<XmlNode> ChildRFS { get; } = new DistinctList<XmlNode>();
        public DistinctList<string> ChildCFS { get; } = new DistinctList<string>();
        public List<ScenarioOperation> IncludedToScenario { get; } = new List<ScenarioOperation>();

        internal override RFSBindings Bindings
        {
            get
            {
                if (_bindings != null)
                    return _bindings;

                _bindings = new RFSBindings(this, RFSName, _navigator);

                return _bindings;
            }
        }

        /// <summary>
        /// true - если текущий RFS используется как отдельная операция или false - относится к каталожному сценарию
        /// </summary>
        public bool IsSeparated => !(IncludedToScenario.Count > 0 && ChildCFS.Count == 0);
        //{ get; internal set; } = true;

        public override string Body
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append("<Configuration>");

                AppendXmlNode(builder, "HostTypeList", Bindings.HostTypeList);
                AppendXmlNode(builder, "ResourceList", Bindings.ResourceList);
                AppendXmlNode(builder, "RFSParameterList", Bindings.RFSParameterList);
                AppendXmlNode(builder, "RFSList", Bindings.RFSList);
                AppendXmlNode(builder, "CFSList", Bindings.CFSList);
                AppendXmlNode(builder, "CFSGroupList", Bindings.CFSGroupList);
                AppendXmlNode(builder, "RFSGroupList", Bindings.RFSGroupList);
                AppendXmlNode(builder, "HandlerList", Bindings.HandlerList);
                AppendXmlNode(builder, "RestrictionList", Bindings.RestrictionList);
                AppendXmlNode(builder, "ScenarioList", Bindings.ScenarioList);

                builder.Append("</Configuration>");
                return XML.PrintXml(builder.ToString());
            }
        }

        public RFSOperation(XmlNode node, string rfsName, string linkType, string hostTypeName, XPathNavigator navigator, ServiceCatalog catalog)
        {
            Node = node;
            if (Node == null && navigator.SelectFirst($"/Configuration/RFSList/RFS[@name='{rfsName}']", out var getRFS))
            {
                Node = getRFS.Node;
            }

            _navigator = navigator;
            RFSName = rfsName;
            LinkType = linkType;

            HostTypeName = hostTypeName;
            Name = $"{catalog.Prefix}{LinkType}.{RFSName}";
        }

        public override string ToString()
        {
            return $"{base.ToString()} RFS=[{ChildRFS.Count}] CFS=[{ChildCFS.Count}] Scenario=[{IncludedToScenario.Count}]";
        }
    }
}