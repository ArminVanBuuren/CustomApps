using System;
using System.Collections.Generic;
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
    public sealed class ScenarioOperation : CatalogOperation
    {
        private RFSBindings _bindings;

        internal XmlNode ScenarioNode { get; private set; }
        internal string ScenarioName { get; private set; }
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

        private bool IsInner { get; } = false;
        DistinctList<RFSOperation> RFSList { get; } = new DistinctList<RFSOperation>();
        DistinctList<RFSOperation> AppendRFSList { get; } = new DistinctList<RFSOperation>();

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

                if (!IsInner)
                {
                    builder.Append("<ScenarioList>");
                    builder.Append(ScenarioNode.OuterXml);
                    builder.Append("</ScenarioList>");
                }

                builder.Append("</Configuration>");
                return XML.PrintXml(builder.ToString());
            }
        }

        public ScenarioOperation(XmlNode scenarioNode, string rfsName, ServiceCatalog catalog)
        {
            IsInner = true;
            Preload(scenarioNode, catalog);

            var type = scenarioNode.Attributes?["type"]?.Value;

            if(type.IsNullOrEmpty())
                throw new Exception($"Invalid config. Scenario \"{ScenarioName}\" must have attribute \"type\".");

            if (!catalog.AllRFS.TryGetValue(rfsName, out var rfsOperationList))
                return;

            foreach (var rfs in rfsOperationList.Where(p => p.LinkType.Equals(type, StringComparison.CurrentCultureIgnoreCase)))
            {
                rfs.IsSeparated = false;
                RFSList.Add(rfs);
            }

            PostLoad();
        }

        class ScenarioRFSList
        {
            public List<RFSOperation> MandatoryList { get; } = new List<RFSOperation>();
            public List<RFSOperation> ChildList { get; } = new List<RFSOperation>();
        }

        public ScenarioOperation(XmlNode scenarioNode, XPathNavigator navigator, ServiceCatalog catalog)
        {
            Preload(scenarioNode, catalog);

            var isDropped = scenarioNode.Attributes?["sendType"]?.Value;
            if (!isDropped.IsNullOrEmpty() && isDropped.Equals("Drop", StringComparison.CurrentCultureIgnoreCase))
                IsDropped = true;

            var scenariosRFSs = XPATH.Execute(navigator, $"/Configuration/ScenarioList/Scenario[@name='{ScenarioName}']/RFS");
            if (scenariosRFSs != null && scenariosRFSs.Count > 0)
            {
                var scenarioRFSs = LoadScenario(scenariosRFSs, catalog);

                if (scenarioRFSs.MandatoryList.Count == 0)
                {
                    foreach (var rfs in scenarioRFSs.ChildList)
                    {
                        rfs.IsSeparated = false;
                        RFSList.Add(rfs);
                    }
                }
                else
                {
                    foreach (var mandRFS in scenarioRFSs.MandatoryList)
                    {
                        mandRFS.IsSeparated = false;
                        RFSList.Add(mandRFS);
                    }

                    var cfsOfMandatoryRFS = scenarioRFSs.MandatoryList.SelectMany(p => p.ChildCFS.Select(x => x.Attributes?["name"].Value)).ToList().Distinct();
                    foreach (var childRFS in scenarioRFSs.ChildList)
                    {
                        var nodes = childRFS.ChildCFS.Select(x => x.Attributes?["name"].Value).ToList();
                        if (!nodes.Except(cfsOfMandatoryRFS).Any())
                        {
                            childRFS.IsSeparated = false;
                            RFSList.Add(childRFS);
                        }
                    }
                }
            }

            var appendRFS = XPATH.Execute(navigator, $"/Configuration/ScenarioList/Scenario[@name='{ScenarioName}']/Append");
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

                    if (string.IsNullOrEmpty(rfsName) || !catalog.AllRFS.TryGetValue(rfsName, out var rfsOperationList))
                        continue;

                    AppendRFSList.Add(rfsOperationList.First());
                }
            }

            PostLoad();
        }

        static ScenarioRFSList LoadScenario(XPathResultCollection scenariosRFSs, ServiceCatalog catalog)
        {
            var result = new ScenarioRFSList();
            foreach (var rfsNode in scenariosRFSs)
            {
                if (rfsNode.Node.Attributes == null || rfsNode.Node.Attributes.Count == 0)
                    continue;

                string rfsName = null;
                string useType = null;
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
                        case "useType":
                            useType = rfsAttr.Value;
                            break;
                    }
                }

                if (string.IsNullOrEmpty(rfsName) || scenarioTypeList == null || scenarioTypeList.Length <= 0 || !catalog.AllRFS.TryGetValue(rfsName, out var rfsOperationList))
                    continue;

                foreach (var type in scenarioTypeList)
                {
                    var rfs = rfsOperationList.FirstOrDefault(p => p.LinkType.Equals(type.Trim(), StringComparison.CurrentCultureIgnoreCase));
                    if (rfs == null)
                        continue;
                    
                    if (useType != null && useType.Equals("Mandatory", StringComparison.CurrentCultureIgnoreCase))
                        result.MandatoryList.Add(rfs);
                    else
                        result.ChildList.Add(rfs);
                }
            }

            return result;
        }

        //void AssignRFSToScenario(IEnumerable<string> scenarioTypeList, List<RFSOperation> rfsOperationList)
        //{
        //    foreach (var type in scenarioTypeList)
        //    {
        //        var rfs = rfsOperationList.FirstOrDefault(p => p.LinkType.Equals(type.Trim(), StringComparison.CurrentCultureIgnoreCase));
        //        if (rfs == null)
        //            continue;

        //        rfs.IsSeparated = false;
        //        RFSList.Add(rfs);
        //    }
        //}

        void Preload(XmlNode scenarioNode, ServiceCatalog catalog)
        {
            if (scenarioNode.Attributes == null || scenarioNode.Attributes.Count == 0)
                throw new Exception("Invalid config. Scenario must have attributes.");

            ScenarioName = scenarioNode.Attributes?["name"]?.Value;

            if (ScenarioName.IsNullOrEmptyTrim())
                throw new Exception("Invalid config. Scenario must have attribute \"name\" or value is empty");

            Name = $"{catalog.Prefix}{ScenarioName}";
            ScenarioNode = scenarioNode;
        }

        void PostLoad()
        {
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