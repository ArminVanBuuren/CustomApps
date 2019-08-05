﻿using System.Collections.Generic;
using System.Linq;
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

        /// <inheritdoc />
        /// <summary>
        /// Если не найден ни один CFS и дочерний RFS и не добавлен ни в один сценарий (Тут не учитываются хэндлены!)
        /// </summary>
        protected internal override bool IsDropped
        {
            get
            {
                if (ChildCFSList.Count > 0 || IncludedToScenarios.Count > 0 
                   || _navigator.SelectFirst($"/Configuration/HandlerList/Handler//RFS[@name='{RFSName}'] | /Configuration/RFSGroupList/RFSGroup//RFS[@name='{RFSName}']", out var res))
                    return false;

                if (ChildRFSList.Count > 0 )
                {
                    foreach (var childRFS in ChildRFSList)
                    {
                        var childRFSName = childRFS?.Attributes?["name"]?.Value;
                        if (!string.IsNullOrEmpty(childRFSName) 
                              && _navigator.SelectFirst($"/Configuration/CFSList/CFS/RFS[@name='{childRFSName}'] | /Configuration/HandlerList/Handler//RFS[@name='{childRFSName}'] | /Configuration/RFSGroupList/RFSGroup//RFS[@name='{childRFSName}']", out var res2))
                            return false;
                    }
                }

                return true;
            }
        }

        public XmlNode Node { get; }
        public DistinctList<XmlNode> ChildRFSList { get; } = new DistinctList<XmlNode>();
        public DistinctList<string> ChildCFSList { get; } = new DistinctList<string>();
        public List<ScenarioOperation> IncludedToScenarios { get; } = new List<ScenarioOperation>();

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
        public bool IsSeparated => !(IncludedToScenarios.Count > 0 && ChildCFSList.Count == 0);
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
                AppendXmlNode(builder, "RFSDependencyList", Bindings.RFSDependencyList);
                AppendXmlNode(builder, "RestrictionList", Bindings.RestrictionList);
                AppendXmlNode(builder, "ScenarioList", Bindings.ScenarioList);

                builder.Append("</Configuration>");
                return XML.PrintXml(builder.ToString());
            }
        }

        public RFSOperation(XmlNode node, string rfsName, string linkType, string hostTypeName, XPathNavigator navigator, ServiceCatalog catalog)
        {
            RFSName = rfsName;
            LinkType = linkType;
            HostTypeName = hostTypeName;
            _navigator = navigator;

            Node = node;
            if (Node == null && navigator.SelectFirst($"/Configuration/RFSList/RFS[@name='{rfsName}']", out var getRFS))
            {
                Node = getRFS.Node;
            }
            
            Name = $"{catalog.Prefix}{LinkType}.{RFSName}";
        }

        public override string ToString()
        {
            return $"{base.ToString()} RFS=[{ChildRFSList.Count}] CFS=[{ChildCFSList.Count}] Scenario=[{IncludedToScenarios.Count}]";
        }
    }
}