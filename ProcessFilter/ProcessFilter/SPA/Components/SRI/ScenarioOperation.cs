using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using SPAFilter.SPA.Collection;
using SPAFilter.SPA.SC;
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

                _bindings = new RFSBindings(this);
                foreach (var rfsOperation in RFSList)
                {
                    _bindings.CombineWith(rfsOperation.Bindings);
                }

                foreach (var rfsOperation in AppendRFSList)
                {
                    _bindings.CombineWith(rfsOperation.Bindings);
                }

                //_bindings.Finnaly();

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
                AppendXmlNode(builder, "RFSGroupList", Bindings.RFSGroupList);
                AppendXmlNode(builder, "HandlerList", Bindings.HandlerList);
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

        public ScenarioOperation(XmlNode scenarioNode, string scenarioName, string rfsName, ServiceCatalog catalog)
        {
            IsInner = true;
            Preload(scenarioNode, scenarioName, catalog);

            var type = scenarioNode.Attributes?["type"]?.Value;

            if(type.IsNullOrEmpty())
                throw new Exception($"Invalid config. Scenario \"{ScenarioName}\" must have attribute \"type\".");

            if (!catalog.AllRFS.TryGetValue(rfsName, out var rfsOperationList))
                return;

            foreach (var rfs in rfsOperationList.Where(p => p.LinkType.Equals(type, StringComparison.CurrentCultureIgnoreCase)))
            {
                rfs.ChildCFS.Clear();
                rfs.IncludedToScenario.Add(this);
                RFSList.Add(rfs);
            }

            PostLoad();
        }

        class ScenarioRFSList
        {
            public List<RFSOperation> MandatoryList { get; } = new List<RFSOperation>();
            public List<RFSOperation> ChildList { get; } = new List<RFSOperation>();
        }

        public ScenarioOperation(XmlNode scenarioNode, string scenarioName, XPathNavigator navigator, ServiceCatalog catalog)
        {
            Preload(scenarioNode, scenarioName, catalog);

            var isDropped = scenarioNode.Attributes?["sendType"]?.Value;
            if (isDropped != null && isDropped.Equals("Drop", StringComparison.CurrentCultureIgnoreCase))
                IsDropped = true;

            var scenariosRFSs = XPATH.Select(navigator, $"/Configuration/ScenarioList/Scenario[@name='{ScenarioName}']/RFS");
            if (scenariosRFSs != null && scenariosRFSs.Count > 0)
            {
                var scenarioRFSs = LoadScenario(scenariosRFSs, navigator, catalog);

                if (scenarioRFSs.MandatoryList.Count == 0)
                {
                    foreach (var rfs in scenarioRFSs.ChildList)
                    {
                        rfs.ChildCFS.Clear();
                        rfs.IncludedToScenario.Add(this);
                        RFSList.Add(rfs);
                    }
                }
                else
                {
                    var cfsOfMandatoryRFS = scenarioRFSs.MandatoryList.SelectMany(p => p.ChildCFS).Distinct().ToList();

                    foreach (var mandRFS in scenarioRFSs.MandatoryList)
                    {
                        mandRFS.ChildCFS.Clear();
                        mandRFS.IncludedToScenario.Add(this);
                        RFSList.Add(mandRFS);
                    }
                    
                    foreach (var childRFS in scenarioRFSs.ChildList)
                    {
                        var commonCFS = cfsOfMandatoryRFS.Intersect(childRFS.ChildCFS).ToList();
                        foreach (var escapeCFS in commonCFS)
                        {
                            childRFS.ChildCFS.Remove(escapeCFS);
                        }

                        childRFS.IncludedToScenario.Add(this);
                        RFSList.Add(childRFS);
                    }
                }
            }

            var appendRFS = XPATH.Select(navigator, $"/Configuration/ScenarioList/Scenario[@name='{ScenarioName}']/Append");
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

                    if (string.IsNullOrEmpty(rfsName) || !catalog.AllRFS.TryGetValue(rfsName, out var rfsOperationList) || rfsOperationList.Count == 0)
                        continue;

                    AppendRFSList.Add(rfsOperationList.First());
                }
            }

            PostLoad();
        }

        static ScenarioRFSList LoadScenario(IEnumerable<XPathResult> scenariosRFSs, XPathNavigator navigator, ServiceCatalog catalog)
        {
            var result = new ScenarioRFSList();
            foreach (var rfsNode in scenariosRFSs)
            {
                if (rfsNode.Node.Attributes == null || rfsNode.Node.Attributes.Count == 0)
                    continue;

                string rfsName = null;
                string useType = null;
                List<string> scenarioTypeList = null;

                foreach (XmlAttribute rfsAttr in rfsNode.Node.Attributes)
                {
                    switch (rfsAttr.Name)
                    {
                        case "name":
                            rfsName = rfsAttr.Value;
                            break;
                        case "scenarioType":
                            scenarioTypeList = rfsAttr.Value.Split(',').Select(p => p.Trim()).ToList();
                            break;
                        case "useType":
                            useType = rfsAttr.Value;
                            break;
                    }
                }

                if (string.IsNullOrEmpty(rfsName) || scenarioTypeList == null || scenarioTypeList.Count == 0 || !catalog.AllRFS.TryGetValue(rfsName, out var rfsOperationList) || rfsOperationList.Count == 0)
                    continue;


                var currentScenarioRFS = rfsOperationList.Where(x => scenarioTypeList.Any(p => x.LinkType.Equals(p.Trim()))).ToList();
                if (currentScenarioRFS.Count == 0)
                {
                    var anyExistRFS = rfsOperationList.First();
                    foreach (var linkType in scenarioTypeList)
                    {
                        // это костыль, т.к. хэндлеры в текущей реализации прога не обрабатывает. А хэндлерах могут быть разные настройки где RFS может использоваться с типом modify например, поэтому просто костыльно создаем недостающие
                        var notExistRFS = new RFSOperation(anyExistRFS.Node, rfsName, linkType, anyExistRFS.HostTypeName, navigator, catalog);
                        notExistRFS.ChildCFS.AddRange(anyExistRFS.ChildCFS);
                        notExistRFS.ChildRFS.AddRange(notExistRFS.ChildRFS);
                        rfsOperationList.Add(notExistRFS);

                        if (useType != null && useType.Equals("Mandatory", StringComparison.CurrentCultureIgnoreCase))
                            result.MandatoryList.Add(notExistRFS);
                        else
                            result.ChildList.Add(notExistRFS);
                    }
                }
                else
                {
                    foreach (var rfs in currentScenarioRFS)
                    {
                        if (useType != null && useType.Equals("Mandatory", StringComparison.CurrentCultureIgnoreCase))
                            result.MandatoryList.Add(rfs);
                        else
                            result.ChildList.Add(rfs);
                    }
                }
            }

            return result;
        }

        void Preload(XmlNode scenarioNode, string scenarioName, ServiceCatalog catalog)
        {
            if (scenarioNode.Attributes == null || scenarioNode.Attributes.Count == 0)
                throw new Exception("Invalid config. Scenario must have any attributes.");

            ScenarioName = scenarioName;
            Name = $"{catalog.Prefix}{ScenarioName}";
            ScenarioNode = scenarioNode;
        }

        void PostLoad()
        {
            var scenarioHostType = RFSList.Select(x => x.HostTypeName).Distinct().ToList();

            if (scenarioHostType.Count == 0)
                throw new Exception($"Scenario \"{ScenarioName}\" is invalid. Not found any RFS.");

            // по идее хост должен быть один, но если каталог криво настроили то могут быть несколько хостов в одном сценарии
            if (scenarioHostType.Count > 1)
                throw new Exception($"Scenario \"{ScenarioName}\" has RFS with different hostTypes - \"{string.Join(",", scenarioHostType)}\"");

            HostTypeName = scenarioHostType.First();
        }

        public override string ToString()
        {
            return $"{base.ToString()} RFSList=[{RFSList.Count}] AppendRFS=[{AppendRFSList.Count}]";
        }
    }
}