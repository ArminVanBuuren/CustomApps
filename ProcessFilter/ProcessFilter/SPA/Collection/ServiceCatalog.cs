using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using SPAFilter.SPA.Components;
using SPAFilter.SPA.Components.SRI;
using Utils;
using Utils.CollectionHelper;

namespace SPAFilter.SPA.Collection
{
    public class ServiceCatalog : CollectionHostType
    {
        readonly List<string> defaultLinkTypes = new List<string> { "Add", "Remove" };
        public string Prefix { get; }
        internal DuplicateDictionary<string, RFSOperation> AllRFS { get; }
        internal Dictionary<string, ScenarioOperation> AllScenarios { get; }

        public ServiceCatalog(string filePath)
        {
            if (!XML.IsFileXml(filePath, out var document))
                throw new Exception($"Incorrect xml file \"{filePath}\"!");

            var navigator = document.CreateNavigator();

            var prefix = XPATH.Execute(navigator, @"/Configuration/@scenarioPrefix");
            if (prefix == null || prefix.Count == 0)
                throw new Exception("Service Catalog is invalid. Not found attribute \"scenarioPrefix\".");
            Prefix = prefix.First().Value;

            AllRFS = new DuplicateDictionary<string, RFSOperation>();
            AllScenarios = new Dictionary<string, ScenarioOperation>();

            var allRFSCFSsList = XPATH.Execute(navigator, @"/Configuration/RFSList/RFS")
                .ToDictionary(x => x.Node, x => XPATH.Execute(navigator, $"/Configuration/CFSList/CFS[RFS/@name='{x.Node.Attributes?["name"].Value}']")
                ?.Where(t => t != null)
                .Select(p => p.Node));

            var subscriptionRFSCFSsList = allRFSCFSsList
                .Where(p => p.Key.Attributes != null && p.Key.Attributes["base"] == null && p.Key.Attributes["processType"] != null && p.Key.Attributes["processType"].Value.Equals("Subscription", StringComparison.CurrentCultureIgnoreCase))
                .ToDictionary(p => p.Key, p => p.Value);

            var notBaseRFSCFSsList = allRFSCFSsList.Except(subscriptionRFSCFSsList).ToDictionary(p => p.Key, p => p.Value);

            GetRFS(subscriptionRFSCFSsList, navigator, true);
            GetRFS(notBaseRFSCFSsList, navigator);

            var scenarioList = XPATH.Execute(navigator, @"/Configuration/ScenarioList/Scenario");
            foreach (var scenario in scenarioList)
            {
                var scenarioOp = new ScenarioOperation(scenario.Node, navigator, this);
                if(AllScenarios.ContainsKey(scenarioOp.Name))
                    throw new Exception($"Service Catalog is invalid. {scenarioOp.Name} already exist.");

                AllScenarios.Add(scenarioOp.Name, scenarioOp);
            }

            var filteredOperations = new List<IOperation>();
            foreach (var rfsOperationList in AllRFS)
            {
                foreach (var rfs in rfsOperationList.Value)
                {
                    if (rfs.IsSeparated && !rfs.IsDropped)
                    {
                        filteredOperations.Add(rfs);
                    }
                }
            }

            filteredOperations.AddRange(AllScenarios.Values.Where(p => !p.IsDropped));

            foreach (var hostTypeName in filteredOperations.Select(p => p.HostTypeName).Distinct())
            {
                Add(new CatalogHostType(hostTypeName, filteredOperations));
            }
        }


        void GetRFS(IDictionary<XmlNode, IEnumerable<XmlNode>> collection, XPathNavigator navigator, bool isSubscriptions = false)
        {
            foreach (var rfsCFSs in collection)
            {
                if (rfsCFSs.Key.Attributes == null)
                    throw new Exception("Service Catalog is invalid. Not found any attributes in some RFS.");

                var rfsName = string.Empty;
                var baseRFSName = string.Empty;
                var parentRFSName = string.Empty;
                var hostType = string.Empty;
                var processType = string.Empty;
                

                foreach (XmlAttribute attribute in rfsCFSs.Key.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "name":
                            rfsName = attribute.Value;
                            break;
                        case "parent":
                            parentRFSName = attribute.Value;
                            break;
                        case "base":
                            baseRFSName = attribute.Value;
                            break;
                        case "hostType":
                            hostType = attribute.Value;
                            break;
                        case "processType":
                            processType = attribute.Value;
                            break;
                    }
                }

                if (hostType.IsNullOrEmptyTrim())
                    throw new Exception($"Service Catalog is invalid. Not found attribute \"hostType\" in {rfsName}.");
                if (processType.Equals("CancelHostType", StringComparison.CurrentCultureIgnoreCase))
                    continue;
                if(AllRFS.ContainsKey(rfsName))
                    throw new Exception($"Service Catalog is invalid. {rfsName} already exist.");

                if (isSubscriptions)
                {
                    foreach (var linkType in defaultLinkTypes)
                    {
                        var baseRFS = new RFSOperation(rfsName, linkType, hostType, navigator, this)
                        {
                            IsSubscription = true
                        };
                        baseRFS.ChildRFS.Add(rfsCFSs.Key);
                        AllRFS.Add(rfsName, baseRFS);
                    }

                    CheckRFSInnerScenarios(rfsName, rfsCFSs.Key);
                    continue;
                }
                else if (!baseRFSName.IsNullOrEmptyTrim())
                {
                    if (AllRFS.TryGetValue(baseRFSName, out var result))
                    {
                        foreach (var baseRFS in result)
                        {
                            baseRFS.ChildRFS.Add(rfsCFSs.Key);
                        }

                        if (result.All(p => !p.IsSubscription) && result.All(p => p.LinkType != "Modify"))
                        {
                            var baseModifyRFS = new RFSOperation(baseRFSName, "Modify", hostType, navigator, this);
                            baseModifyRFS.ChildRFS.Add(rfsCFSs.Key);
                            AllRFS.Add(baseRFSName, baseModifyRFS);
                        }
                    }
                    else
                    {
                        foreach (var linkType in defaultLinkTypes)
                        {
                            var baseRFS = new RFSOperation(baseRFSName, linkType, hostType, navigator, this);
                            baseRFS.ChildRFS.Add(rfsCFSs.Key);
                            AllRFS.Add(baseRFSName, baseRFS);
                        }
                    }

                    continue;
                }
                else if (!parentRFSName.IsNullOrEmptyTrim())
                {
                    if (AllRFS.TryGetValue(parentRFSName, out var result))
                    {
                        foreach (var parentRFS in result)
                        {
                            parentRFS.ChildRFS.Add(rfsCFSs.Key);
                        }
                    }
                    else
                    {
                        foreach (var linkType in defaultLinkTypes)
                        {
                            var parentRfs = new RFSOperation(parentRFSName, linkType, hostType, navigator, this);
                            parentRfs.ChildRFS.Add(rfsCFSs.Key);
                            AllRFS.Add(parentRFSName, parentRfs);
                        }
                    }

                    continue;
                }

                var oneTimeLinkTypes = new DistinctList<string>();
                if (rfsCFSs.Value != null && rfsCFSs.Value.Any())
                {
                    foreach (var cfs in rfsCFSs.Value)
                    {
                        if (cfs == null)
                            continue;

                        var cfsType = cfs.Attributes?["type"]?.Value;
                        if (cfsType.IsNullOrEmpty() || cfsType != "OneTime")
                        {
                            oneTimeLinkTypes.Clear();
                            break;
                        }

                        oneTimeLinkTypes.Add(cfs.Clone().SelectSingleNode($"/RFS[@name='{rfsName}']/@linkType").Value);
                    }
                }

                if (oneTimeLinkTypes.Count > 0)
                {
                    foreach (var linkType in oneTimeLinkTypes)
                    {
                        var rfs = new RFSOperation(rfsName, linkType, hostType, navigator, this);
                        rfs.ChildCFS.AddRange(rfsCFSs.Value);
                        AllRFS.Add(rfsName, rfs);
                    }
                }
                else
                {
                    foreach (var linkType in defaultLinkTypes)
                    {
                        var rfs = new RFSOperation(rfsName, linkType, hostType, navigator, this);
                        if (rfsCFSs.Value != null)
                            rfs.ChildCFS.AddRange(rfsCFSs.Value);
                        AllRFS.Add(rfsName, rfs);
                    }

                    if (processType.Equals("DynamicList", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var modifyRFS = new RFSOperation(rfsName, "Modify", hostType, navigator, this);
                        if (rfsCFSs.Value != null)
                            modifyRFS.ChildCFS.AddRange(rfsCFSs.Value);
                        AllRFS.Add(rfsName, modifyRFS);
                    }
                }

                CheckRFSInnerScenarios(rfsName, rfsCFSs.Key);
            }
        }

        void CheckRFSInnerScenarios(string rfsName, XmlNode rfs)
        {
            foreach (XmlNode childNode in rfs.ChildNodes)
            {
                if (!childNode.Name.Equals("Scenario"))
                    continue;

                var scenarioOp = new ScenarioOperation(childNode, rfsName, this);
                if (AllScenarios.ContainsKey(scenarioOp.Name))
                    throw new Exception($"Service Catalog is invalid. {scenarioOp.Name} already exist.");

                AllScenarios.Add(scenarioOp.Name, scenarioOp);
            }
        }
    }
}