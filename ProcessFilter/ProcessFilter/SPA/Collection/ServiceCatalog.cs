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

            var prefix = XPATH.Select(navigator, @"/Configuration/@scenarioPrefix");
            if (prefix == null || prefix.Count == 0)
                throw new Exception("Service Catalog is invalid. Not found attribute \"scenarioPrefix\".");
            Prefix = prefix.First().Value;

            AllRFS = new DuplicateDictionary<string, RFSOperation>();
            AllScenarios = new Dictionary<string, ScenarioOperation>();

            // вытаскиевам по списку все RFS и все CFS которые включают в себя текущий RFS
            var allRFSCFSsList = XPATH.Select(navigator, @"/Configuration/RFSList/RFS")
                .ToDictionary(x => x.Node, x => XPATH.Select(navigator, $"/Configuration/CFSList/CFS[RFS/@name='{x.Node.Attributes?["name"]?.Value}']")
                ?.Where(t => t != null)
                .Select(p => p.Node));

            // вытаскиваем из предыдущего списка все с типом subscription
            var subscriptionRFSCFSsList = allRFSCFSsList
                .Where(p => p.Key.Attributes != null && p.Key.Attributes["base"] == null && p.Key.Attributes["processType"] != null && p.Key.Attributes["processType"].Value.Equals("Subscription", StringComparison.CurrentCultureIgnoreCase))
                .ToDictionary(p => p.Key, p => p.Value);

            // исключаем все subscription, чтобы заново не обрабатывать
            var notBaseRFSCFSsList = allRFSCFSsList.Except(subscriptionRFSCFSsList).ToDictionary(p => p.Key, p => p.Value);

            GetRFS(subscriptionRFSCFSsList, navigator, true);
            GetRFS(notBaseRFSCFSsList, navigator);

            var scenarioList = XPATH.Select(navigator, @"/Configuration/ScenarioList/Scenario");
            foreach (var scenario in scenarioList)
            {
                var scenarioName = scenario.Node.Attributes?["name"]?.Value;
                if (string.IsNullOrEmpty(scenarioName))
                    throw new Exception("Invalid config. Scenario must have attribute \"name\" or value is empty");
                if (AllScenarios.ContainsKey(scenarioName))
                    throw new Exception($"Service Catalog is invalid. {scenarioName} already exist.");

                var scenarioOp = new ScenarioOperation(scenario.Node, scenarioName, navigator, this);
                AllScenarios.Add(scenarioName, scenarioOp);
            }


            // непосредственная стадия фильтрации. Исключаем дропы и те RFS которые уже есть в сценариях
            var filteredOperations = new Dictionary<string, IOperation>();
            foreach (var rfsOperationList in AllRFS)
            {
                foreach (var rfs in rfsOperationList.Value)
                {
                    CheckRFSInnerScenarios(rfsOperationList.Key, rfs.Node, navigator);

                    if (rfs.IsSeparated && !rfs.IsDropped && !filteredOperations.ContainsKey(rfs.Name))
                    {
                        filteredOperations.Add(rfs.Name, rfs);
                    }
                }
            }

            foreach (var scenario in AllScenarios)
            {
                if (!scenario.Value.IsDropped)
                {
                    filteredOperations.Add(scenario.Key, scenario.Value);
                }
            }

            // Группируем по хостам и доавляем в каталог списки оперций
            foreach (var hostTypeOps in filteredOperations.Values.GroupBy(p => p.HostTypeName))
            {
                Add(new CatalogHostType(hostTypeOps.Key, hostTypeOps));
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

                // Сначала добавляем все с типом subscription, затем уже все дочерние от базовых и остальные
                if (isSubscriptions)
                {
                    foreach (var linkType in defaultLinkTypes)
                    {
                        var baseRFS = AddBaseRFS(rfsCFSs.Key, rfsCFSs, rfsName, linkType, hostType, navigator);
                        baseRFS.IsSubscription = true;
                    }
                    continue;
                }
                else if (!baseRFSName.IsNullOrEmptyTrim())
                {
                    if (AllRFS.TryGetValue(baseRFSName, out var result))
                    {
                        foreach (var baseRFS in result)
                        {
                            baseRFS.ChildRFS.Add(rfsCFSs.Key);
                            AddChildCFS(baseRFS, rfsCFSs.Value);
                        }

                        if (rfsCFSs.Value != null && result.All(p => !p.IsSubscription) && result.All(p => p.LinkType != "Modify"))
                        {
                            var priorityCFSList = rfsCFSs.Value.Select(p =>
                            {
                                var priority = p.Attributes?["priority"]?.Value;
                                return priority ?? "1000";
                            }).Distinct();

                            if (priorityCFSList.Count() > 1)
                            {
                                var baseModifyRFS = AddBaseRFS(result.First().Node, rfsCFSs, baseRFSName, "Modify", hostType, navigator);
                                AddChildCFS(baseModifyRFS, result.First().ChildRFS);
                            }
                            else if(navigator.Select($"/Configuration/HandlerList/Handler[@type='MergeRFS' and Configuration/RFS[@name='{rfsName}']]", out var res))
                            {
                                var baseModifyRFS = AddBaseRFS(result.First().Node, rfsCFSs, baseRFSName, "Modify", hostType, navigator);
                                AddChildCFS(baseModifyRFS, result.First().ChildRFS);
                            }
                        }
                    }
                    else
                    {
                        foreach (var linkType in defaultLinkTypes)
                        {
                            var baseRFS = AddBaseRFS(null, rfsCFSs, baseRFSName, linkType, hostType, navigator);
                            AddChildCFS(baseRFS, rfsCFSs.Value);
                        }
                    }

                    if (AllRFS.TryGetValue(baseRFSName, out var result2))
                        AllRFS.Add(rfsName, result2);

                    continue;
                }
                else if (!parentRFSName.IsNullOrEmptyTrim())
                {
                    if (AllRFS.TryGetValue(parentRFSName, out var result))
                    {
                        foreach (var parentRFS in result)
                        {
                            parentRFS.ChildRFS.Add(rfsCFSs.Key);
                            AddChildCFS(parentRFS, rfsCFSs.Value);
                        }
                    }
                    else
                    {
                        foreach (var linkType in defaultLinkTypes)
                        {
                            var parentRFS = AddBaseRFS(null, rfsCFSs, parentRFSName, linkType, hostType, navigator);
                            AddChildCFS(parentRFS, rfsCFSs.Value);
                        }
                    }

                    if (AllRFS.TryGetValue(baseRFSName, out var result2))
                        AllRFS.Add(rfsName, result2);

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

                // Если CFS который включает в себя текущий RFS с типом OneTime, то добавляем linkType тот который указан в CFS
                if (oneTimeLinkTypes.Count > 0)
                {
                    foreach (var linkType in oneTimeLinkTypes)
                    {
                        AddRFS(rfsCFSs, rfsName, linkType, hostType, navigator);
                    }
                }
                else
                {
                    foreach (var linkType in defaultLinkTypes)
                    {
                        AddRFS(rfsCFSs, rfsName, linkType, hostType, navigator);
                    }
                }
            }
        }

        RFSOperation AddBaseRFS(XmlNode node, KeyValuePair<XmlNode, IEnumerable<XmlNode>> rfsCFSs, string rfsName, string linkType, string hostType, XPathNavigator navigator)
        {
            var baseRFS = new RFSOperation(node, rfsName, linkType, hostType, navigator, this);
            baseRFS.ChildRFS.Add(rfsCFSs.Key);
            AllRFS.Add(rfsName, baseRFS);
            return baseRFS;
        }

        void AddRFS(KeyValuePair<XmlNode, IEnumerable<XmlNode>> rfsCFSs, string rfsName, string linkType, string hostType, XPathNavigator navigator)
        {
            var rfs = new RFSOperation(rfsCFSs.Key, rfsName, linkType, hostType, navigator, this);
            AddChildCFS(rfs, rfsCFSs.Value);
            AllRFS.Add(rfsName, rfs);
        }

        static void AddChildCFS(RFSOperation rfs, IEnumerable<XmlNode> cfsList)
        {
            if(cfsList != null)
                rfs.ChildCFS.AddRange(cfsList.Select(p => p.Attributes?["name"]?.Value));
        }

        /// <summary>
        /// Сценарии внутри RFS
        /// </summary>
        /// <param name="rfsName"></param>
        /// <param name="rfs"></param>
        /// <param name="navigator"></param>
        void CheckRFSInnerScenarios(string rfsName, XmlNode rfs, XPathNavigator navigator)
        {
            var rfsNode = rfs;
            if (rfs == null)
            {
                var getRFS = XPATH.Select(navigator, $"/Configuration/RFSList/RFS[@name='{rfsName}']");
                if(getRFS == null || getRFS.Count == 0)
                    return;
                rfsNode = getRFS.First().Node;
            }


            foreach (XmlNode childNode in rfsNode.ChildNodes)
            {
                if (!childNode.Name.Equals("Scenario"))
                    continue;

                var scenarioName = childNode.Attributes?["name"]?.Value;
                if (string.IsNullOrEmpty(scenarioName))
                    throw new Exception("Invalid config. Scenario must have attribute \"name\" or value is empty");

                if (!AllScenarios.ContainsKey(scenarioName))
                {
                    var scenarioOp = new ScenarioOperation(childNode, scenarioName, rfsName, this);
                    AllScenarios.Add(scenarioName, scenarioOp);
                }
            }
        }
    }
}