using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using SPAFilter.SPA.Collection;
using Utils;

namespace SPAFilter.SPA.Components.SC
{
    public sealed class ScenarioOperation : CatalogOperation
    {
        private RFSBindings _bindings;

        internal XmlNode ScenarioNode { get; }
        internal string ScenarioName { get; }
        internal override RFSBindings Bindings
        {
            get
            {
                if (_bindings != null)
                    return _bindings;

                _bindings = new RFSBindings();
                foreach (var rfsOperation in RFSList)
                {
                    _bindings.CombineWith(rfsOperation.Bindings);
                }

                foreach (var rfsOperation in AppendRFSList)
                {
                    _bindings.CombineWith(rfsOperation.Bindings);
                }

                return _bindings;
            }
        }

        public DistinctList<RFSOperation> RFSList { get; } = new DistinctList<RFSOperation>();
        public DistinctList<RFSOperation> AppendRFSList { get; } = new DistinctList<RFSOperation>();

        public override string Body
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append("<Configuration>");

                AppendXmlNode(builder, "HostTypeList", Bindings.HostTypeList);
                AppendXmlNode(builder, "RFSList", Bindings.RFSList);
                AppendXmlNode(builder, "CFSList", Bindings.CFSList);
                AppendXmlNode(builder, "CFSGroupList", Bindings.CFSGroupList);
                AppendXmlNode(builder, "HandlerList", Bindings.HandlerList);
                AppendXmlNode(builder, "RFSGroupList", Bindings.RFSGroupList);
                AppendXmlNode(builder, "RestrictionList", Bindings.RestrictionList);

                builder.Append("<ScenarioList>");
                builder.Append(ScenarioNode.OuterXml);
                builder.Append("</ScenarioList>");

                builder.Append("</Configuration>");
                return XML.PrintXml(builder.ToString());
            }
        }

        public ScenarioOperation(int id, XmlNode scenarioNode, XPathNavigator navigator, ServiceCatalog catalog) : base(id)
        {
            if (scenarioNode.Attributes == null || scenarioNode.Attributes.Count == 0)
                throw new Exception("Invalid config. Scenario must have attributes.");

            foreach (XmlAttribute scenarioAttr in scenarioNode.Attributes)
            {
                if (scenarioAttr.Name != "name")
                    continue;

                ScenarioName = scenarioAttr.Value;
                break;
            }

            if (ScenarioName.IsNullOrEmptyTrim())
                throw new Exception("Invalid config. Scenario must have attribute \"name\" or value is empty");

            ScenarioNode = scenarioNode;

            var childRFS = XPATH.Execute(navigator, $"/Configuration/ScenarioList/Scenario[@name='{ScenarioName}']/RFS");
            if (childRFS != null && childRFS.Count > 0)
            {
                foreach (var rfsNode in childRFS)
                {
                    if (rfsNode.Node.Attributes == null || rfsNode.Node.Attributes.Count == 0)
                        continue;

                    string rfsName = null;
                    string[] scenarioTypeList = null;
                    foreach (XmlAttribute rfsAttr in rfsNode.Node.Attributes)
                    {
                        switch (rfsAttr.Name)
                        {
                            case "name":
                                rfsName = rfsAttr.Value;
                                break;
                            case "scenarioType":
                                scenarioTypeList = rfsAttr.Value.Split(',');
                                break;
                        }
                    }

                    if (rfsName.IsNullOrEmptyTrim() || scenarioTypeList == null || scenarioTypeList.Length <= 0 || !catalog.AllRFS.TryGetValue(rfsName, out var rfsOperationList))
                        continue;

                    foreach (var type in scenarioTypeList)
                    {
                        var rfs = rfsOperationList.FirstOrDefault(p => p.RFSAction.Equals(type.Trim(), StringComparison.CurrentCultureIgnoreCase));
                        if (rfs == null)
                            continue;

                        rfs.IsSeparated = false;
                        RFSList.Add(rfs);
                    }
                }
            }


            var appendRFS = XPATH.Execute(navigator, $"/Configuration/ScenarioList/Scenario[@name='{ScenarioName}']/RFS");
            if (appendRFS != null && appendRFS.Count > 0)
            {
                foreach (var rfsNode in appendRFS)
                {
                    if (rfsNode.Node.Attributes == null || rfsNode.Node.Attributes.Count == 0)
                        continue;

                    string rfsName = null;
                    foreach (XmlAttribute rfsAttr in rfsNode.Node.Attributes)
                    {
                        if (rfsAttr.Name != "name")
                            continue;

                        rfsName = rfsAttr.Value;
                        break;
                    }

                    if (rfsName.IsNullOrEmptyTrim() || !catalog.AllRFS.TryGetValue(rfsName, out var rfsOperationList))
                        continue;

                    AppendRFSList.Add(rfsOperationList.First());
                }
            }



            Name = $"{catalog.Prefix}{ScenarioName}";
            var builder = new StringBuilder();
            var hostTypes = new List<string>();
            foreach (var rfsOperation in RFSList)
            {
                if (hostTypes.Contains(rfsOperation.HostTypeName))
                    continue;

                hostTypes.Add(rfsOperation.HostTypeName);
                builder.Append(rfsOperation.HostTypeName);
                builder.Append(';');
            }
            HostTypeName = builder.ToString().Trim(';');
        }
    }
}