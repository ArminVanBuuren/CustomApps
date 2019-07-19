using System.Text;
using System.Xml.XPath;
using SPAFilter.SPA.Collection;
using Utils;

namespace SPAFilter.SPA.Components.SC
{
    public sealed class RFSOperation : CatalogOperation
    {
        private readonly XPathNavigator _navigator;
        private RFSBindings _bindings;

        internal string RFSName { get; }
        internal string RFSAction { get; set; }

        internal override RFSBindings Bindings
        {
            get
            {
                if (_bindings != null)
                    return _bindings;

                _bindings = new RFSBindings(RFSName, _navigator);
                return _bindings;
            }
        }

        /// <summary>
        /// true - если текущий RFS используется как отдельная операция или false - относится к каталожному сценарию
        /// </summary>
        public bool IsSeparated { get; internal set; } = true;

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
                AppendXmlNode(builder, "HandlerList", Bindings.HandlerList);
                AppendXmlNode(builder, "RFSGroupList", Bindings.RFSGroupList);
                AppendXmlNode(builder, "RestrictionList", Bindings.RestrictionList);
                AppendXmlNode(builder, "ScenarioList", Bindings.ScenarioList);

                builder.Append("</Configuration>");
                return XML.PrintXml(builder.ToString());
            }
        }

        public RFSOperation(string name, string action, string hostTypeName, XPathNavigator navigator, ServiceCatalog catalog)
        {
            _navigator = navigator;
            RFSName = name;
            RFSAction = action;

            HostTypeName = hostTypeName;
            Name = $"{catalog.Prefix}{RFSAction}.{RFSName}";
        }
    }
}