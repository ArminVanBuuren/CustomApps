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
    class CFS
    {
        public bool IsOneTime { get; } = false;

        public CFS(XmlNode cfsNode)
        {
            var cfsName = cfsNode.Attributes?["name"]?.Value;
            IsOneTime = cfsNode.Attributes?["IsOneTime"]?.Value.Trim().ToLower() == "true";
        }
    }

    class RFS
    {
        public string BaseRFS { get; }
        public string ParentRFS { get; }
        public string ProcessType { get; }
        public string HostType { get; }
        public bool IsHasResources { get; }
        public bool IsOnlyRFSGroup { get; }

        public DistinctList<string> Links = new DistinctList<string>();
        public DistinctList<CFS> CFSList = new DistinctList<CFS>();

        public DuplicateDictionary<CFS, string> Depends {get;} = new DuplicateDictionary<CFS, string>();

        public RFS(XmlNode rfsNode, IReadOnlyDictionary<string, CFS> allCFS, XPathNavigator navigator)
        {
            var rfsName = rfsNode.Attributes?["name"]?.Value;
            HostType = rfsNode.Attributes?["hostType"]?.Value;
            ProcessType = rfsNode.Attributes?["processType"]?.Value;
            IsHasResources = rfsNode.ChildNodes.OfType<XmlNode>().Any(x => x.Name == "Resource");

            BaseRFS = rfsNode.Attributes?["base"]?.Value;
            ParentRFS = rfsNode.Attributes?["parent"]?.Value;

            if (navigator.Select($"/Configuration/CFSList/CFS[RFS/@name='{rfsName}']", out var cfsList))
            {
                foreach (var cfs in cfsList)
                {
                    var cfsName = cfs.Node.Attributes?["name"]?.Value;
                    if (cfsName == null || !allCFS.TryGetValue(cfsName, out var cfsCFS))
                        continue;

                    var childRFS = cfs.Node.ChildNodes.OfType<XmlNode>().Where(x => x.Attributes?["name"]?.Value == rfsName);
                    var depends = childRFS.Where(x => x.Attributes?["dependsOn"]?.Value != null).Select(x => x.Attributes?["dependsOn"]?.Value);
                    
                    foreach (var dep in depends)
                    {
                        Depends.Add(cfsCFS, dep);
                    }
                    Links.AddRange(childRFS.Where(x => x.Attributes?["linkType"]?.Value != null).Select(x => x.Attributes?["linkType"]?.Value));
                    CFSList.Add(cfsCFS);
                }
            }

            IsOnlyRFSGroup = false;
            if (cfsList.Count == 0 && navigator.Select($"/Configuration/RFSGroupList/RFSGroup[@type='Choice']/RFS[@name='{rfsName}']", out var rfsInRFSGroup))
            {
                IsOnlyRFSGroup = true;
                var linkTypes = new DistinctList<string>();
                foreach (var rfsGroupRFS in rfsInRFSGroup.Select(x => x.Node))
                {
                    if(rfsGroupRFS == null)
                        continue;

                    var rfsLinkType = rfsGroupRFS?.Attributes?["linkType"]?.Value;

                    if (!string.IsNullOrEmpty(rfsLinkType))
                        Links.Add(rfsLinkType);

                    var rfsGroupName = rfsGroupRFS.ParentNode?.Attributes?["name"]?.Value;
                    if (rfsGroupName != null && navigator.Select($"/Configuration/CFSList/CFS[RFSGroup/@name='{rfsGroupName}']", out var cfsList2))
                    {
                        foreach (var cfs in cfsList2)
                        {
                            var cfsName = cfs.Node.Attributes?["name"]?.Value;
                            if (cfsName == null || !allCFS.TryGetValue(cfsName, out var cfsCFS))
                                continue;

                            CFSList.Add(cfsCFS);
                        }
                    }
                }
            }
        }
    }

    public class ServiceCatalog : CollectionHostType
    {
        public string Prefix { get; }
        internal DuplicateDictionary<string, RFSOperation> AllRFS { get; }
        internal Dictionary<string, ScenarioOperation> AllScenarios { get; }

        public ServiceCatalog(string filePath)
        {
            if (!XML.IsFileXml(filePath, out var document))
                throw new Exception($"Xml file \"{filePath}\" is invalid");

            var navigator = document.CreateNavigator();

            if (!navigator.SelectFirst(@"/Configuration/@scenarioPrefix", out var prefix))
                throw new Exception("Service Catalog is invalid. Attribute \"scenarioPrefix\" not found.");
            Prefix = prefix.Value;

            AllRFS = new DuplicateDictionary<string, RFSOperation>();
            AllScenarios = new Dictionary<string, ScenarioOperation>();

            if(!navigator.Select(@"/Configuration/RFSList/RFS", out var rfsList))
                throw new Exception("Service Catalog is invalid. No RFS found.");

            // вытаскиевам по списку все RFS и все CFS которые включают в себя текущий RFS
            var allRFSCFSsList = rfsList
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


            if (navigator.Select(@"/Configuration/ScenarioList/Scenario", out var scenarioList))
            {
                foreach (var scenario in scenarioList)
                {
                    var scenarioName = scenario.Node.Attributes?["name"]?.Value;
                    if (string.IsNullOrEmpty(scenarioName))
                        throw new Exception("Service Catalog is invalid. Scenario doesn't have attribute \"name\" or value is empty.");
                    if (AllScenarios.ContainsKey(scenarioName))
                        throw new Exception($"Service Catalog is invalid. {scenarioName} already exist.");

                    var scenarioOp = new ScenarioOperation(scenario.Node, scenarioName, navigator, this);
                    AllScenarios.Add(scenarioName, scenarioOp);
                }
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
                    throw new Exception("Some RFS are invalid. No attributes found.");

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
                    throw new Exception($"{rfsName} is invalid. Attribute \"hostType\" not found.");
                if (processType.Equals("CancelHostType", StringComparison.CurrentCultureIgnoreCase))
                    continue;
                if(AllRFS.ContainsKey(rfsName))
                    throw new Exception($"{rfsName} already exist.");

                
                var defaultLinkTypes = new List<string> { "Add", "Remove" };
                var cfsList = new DistinctList<XmlNode>();
                if (rfsCFSs.Value != null)
                {
                    cfsList.AddRange(rfsCFSs.Value);
                }


                // Сначала добавляем все с типом subscription, затем уже все дочерние от базовых и остальные
                if (isSubscriptions)
                {
                    foreach (var linkType in defaultLinkTypes)
                    {
                        var baseRFS = AddBaseRFS(rfsCFSs.Key, rfsCFSs, rfsName, linkType, hostType, navigator);
                        baseRFS.IsSubscription = true;
                        AddChildCFS(baseRFS, cfsList);
                    }
                    continue;
                }

                // если нет ни одного CFS с текущим RFS, то проверяется RFSGroup и подставляются линки из RFSGroup
                var isRFSGroupRFS = false;
                if (cfsList.Count == 0 && navigator.Select($"/Configuration/RFSGroupList/RFSGroup[@type='Choice']/RFS[@name='{rfsName}']", out var rfsInRFSGroup))
                {
                    isRFSGroupRFS = true;
                    var linkTypes = new DistinctList<string>();
                    foreach (var rfsNode in rfsInRFSGroup.Select(x => x.Node))
                    {
                        var rfsLinkType = rfsNode?.Attributes?["linkType"]?.Value;

                        if (!string.IsNullOrEmpty(rfsLinkType))
                            linkTypes.Add(rfsLinkType);

                        var rfsGroupName = rfsNode.ParentNode?.Attributes?["name"]?.Value;
                        if (rfsGroupName != null && navigator.Select($"/Configuration/CFSList/CFS[RFSGroup/@name='{rfsGroupName}']", out var cfsList2))
                        {
                            foreach (var cfs in cfsList2)
                            {
                                cfsList.Add(cfs.Node);
                            }
                        }
                    }

                    defaultLinkTypes.Clear();
                    defaultLinkTypes.AddRange(linkTypes);
                }

                if (!baseRFSName.IsNullOrEmptyTrim())
                {
                    if (AllRFS.TryGetValue(baseRFSName, out var result))
                    {
                        foreach (var baseRFS in result)
                        {
                            baseRFS.ChildRFSList.Add(rfsCFSs.Key);
                            AddChildCFS(baseRFS, cfsList);
                        }

                        // Добавляем операцию Modify
                        // 1. В случае если RFS не является подпиской
                        // 2. Если нет уже существующей операции Modify
                        // 3. Если в RFS есть Resource (RFSParameter не модифается)
                        // 4. Если все связанные CFSы разного приоритета или есть хэндлер MergeRFS
                        if (cfsList.Count > 0 && result.All(p => !p.IsSubscription) && result.All(p => p.LinkType != "Modify"))
                        {
                            var isExistResource = false;
                            foreach (var rfsNode in result.Select(x => x.Node))
                            {
                                if(rfsNode == null)
                                    continue;
                                foreach (XmlNode rfsChild in rfsNode.ChildNodes)
                                {
                                    if (rfsChild.Name.Equals("Resource"))
                                    {
                                        isExistResource = true;
                                        break;
                                    }
                                }

                                if(isExistResource)
                                    break;
                            }

                            if (isExistResource)
                            {
                                var priorityCFSList = cfsList.Select(p =>
                                {
                                    var priority = p.Attributes?["priority"]?.Value;
                                    return priority ?? "1000";
                                }).Distinct();

                                if (priorityCFSList.Count() > 1)
                                {
                                    var baseModifyRFS = AddBaseRFS(result.First().Node, rfsCFSs, baseRFSName, "Modify", hostType, navigator);
                                    AddChildCFS(baseModifyRFS, result.First().ChildRFSList);
                                }
                                else if (navigator.Select($"/Configuration/HandlerList/Handler[@type='MergeRFS' and Configuration/RFS[@name='{rfsName}']]", out var res))
                                {
                                    var baseModifyRFS = AddBaseRFS(result.First().Node, rfsCFSs, baseRFSName, "Modify", hostType, navigator);
                                    AddChildCFS(baseModifyRFS, result.First().ChildRFSList);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var linkType in defaultLinkTypes)
                        {
                            var baseRFS = AddBaseRFS(null, rfsCFSs, baseRFSName, linkType, hostType, navigator);
                            AddChildCFS(baseRFS, cfsList);
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
                            parentRFS.ChildRFSList.Add(rfsCFSs.Key);
                            AddChildCFS(parentRFS, cfsList);
                        }
                    }
                    else
                    {
                        foreach (var linkType in defaultLinkTypes)
                        {
                            var parentRFS = AddBaseRFS(null, rfsCFSs, parentRFSName, linkType, hostType, navigator);
                            AddChildCFS(parentRFS, cfsList);
                        }
                    }

                    if (AllRFS.TryGetValue(baseRFSName, out var result2))
                        AllRFS.Add(rfsName, result2);

                    continue;
                }

                var oneTimeLinkTypes = new DistinctList<string>();
                if (!isRFSGroupRFS && cfsList.Any())
                {
                    foreach (var cfs in cfsList)
                    {
                        if (cfs == null)
                            continue;

                        var cfsType = cfs.Attributes?["type"]?.Value;
                        if (cfsType.IsNullOrEmpty() || cfsType != "OneTime")
                        {
                            oneTimeLinkTypes.Clear();
                            break;
                        }

                        var oneTimeLinkType = cfs.Clone().SelectSingleNode($"/RFS[@name='{rfsName}']/@linkType");
                        if (oneTimeLinkType != null)
                            oneTimeLinkTypes.Add(oneTimeLinkType.Value);
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
            baseRFS.ChildRFSList.Add(rfsCFSs.Key);
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
                rfs.ChildCFSList.AddRange(cfsList.Select(p => p.Attributes?["name"]?.Value));
        }

        /// <summary>
        /// Сценарии внутри RFS
        /// </summary>
        /// <param name="rfsName"></param>
        /// <param name="rfs"></param>
        /// <param name="navigator"></param>
        void CheckRFSInnerScenarios(string rfsName, XmlNode rfs, XPathNavigator navigator)
        {
            if (rfs == null)
                return;

            foreach (XmlNode childNode in rfs.ChildNodes)
            {
                if (!childNode.Name.Equals("Scenario"))
                    continue;

                var scenarioName = childNode.Attributes?["name"]?.Value;
                if (string.IsNullOrEmpty(scenarioName))
                    throw new Exception("Service Catalog is invalid. Scenario doesn't have attribute \"name\" or value is empty.");

                if (!AllScenarios.ContainsKey(scenarioName))
                {
                    var scenarioOp = new ScenarioOperation(childNode, scenarioName, rfsName, this);
                    AllScenarios.Add(scenarioName, scenarioOp);
                }
            }
        }
    }
}