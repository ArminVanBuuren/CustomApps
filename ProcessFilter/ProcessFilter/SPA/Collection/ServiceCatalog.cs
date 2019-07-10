using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SPAFilter.SPA.Components;
using SPAFilter.SPA.Components.SC;
using Utils;
using Utils.CollectionHelper;

namespace SPAFilter.SPA.Collection
{
    public class ServiceCatalog : CollectionHostType
    {
        public string Prefix { get; }
        public List<RFSOperation> Operations { get; }
        public DuplicateDictionary<string, RFSOperation> AllRFS { get; }
        public List<ScenarioOperation> AllScenarios { get; }

        public ServiceCatalog(string filePath)
        {
            Operations = new List<RFSOperation>();

            if (!XML.IsFileXml(filePath, out var document))
                throw new Exception($"Incorrect xml file \"{filePath}\"!");

            var navigator = document.CreateNavigator();

            var prefix = XPATH.Execute(navigator, @"/Configuration/@scenarioPrefix");
            if (prefix == null || prefix.Count == 0)
                throw new Exception("Service Catalog is invalid. Not found attribute \"scenarioPrefix\".");
            Prefix = prefix.First().Value;

            AllRFS = new DuplicateDictionary<string, RFSOperation>();


            var rfsList = XPATH.Execute(navigator, @"/Configuration/RFSList/RFS");
            var rfsId = 0;
            foreach (var rfs in rfsList)
            {
                if (rfs.Node.Attributes == null)
                    throw new Exception("Service Catalog is invalid. Not found any attributes in some RFS.");

                var rfsName = string.Empty;
                var isBase = string.Empty;
                var isParent = string.Empty;
                var hostType = string.Empty;

                foreach (XmlAttribute attribute in rfs.Node.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "name":
                            rfsName = attribute.Value;
                            break;
                        case "parent":
                            isParent = attribute.Value;
                            break;
                        case "base":
                            isBase = attribute.Value;
                            break;
                        case "hostType":
                            hostType = attribute.Value;
                            break;
                    }
                }


                if (rfsName.IsNullOrEmptyTrim())
                    throw new Exception("Service Catalog is invalid. Not found attribute \"name\" in RFS.");
                if (hostType.IsNullOrEmptyTrim())
                    throw new Exception($"Service Catalog is invalid. Not found attribute \"hostType\" in RFS \"{rfsName}\".");

                if (!isBase.IsNullOrEmptyTrim())
                {
                    if (AllRFS.TryGetValue(isBase, out var result))
                    {
                        if (result.All(x => x.RFSAction != "Modify"))
                        {
                            AllRFS.Add(isBase, new RFSOperation(rfsId, isBase, "Modify", hostType, navigator, this));
                        }
                    }
                    else
                    {
                        foreach (var flag in new[] { "Add", "Remove", "Modify" })
                        {
                            rfsId++;
                            AllRFS.Add(isBase, new RFSOperation(rfsId, isBase, flag, hostType, navigator, this));
                        }
                    }

                    continue;
                }

                if (!isParent.IsNullOrEmptyTrim())
                {
                    if (!AllRFS.TryGetValue(isParent, out var result))
                    {
                        foreach (var flag in new[] { "Add", "Remove" })
                        {
                            rfsId++;
                            AllRFS.Add(isParent, new RFSOperation(rfsId, isParent, flag, hostType, navigator, this));
                        }
                    }

                    continue;
                }

                foreach (var flag in new[] { "Add", "Remove" })
                {
                    rfsId++;
                    AllRFS.Add(rfsName, new RFSOperation(rfsId, rfsName, flag, hostType, navigator, this));
                }
            }

            AllScenarios = new List<ScenarioOperation>();
            var scenarioList = XPATH.Execute(navigator, @"/Configuration/ScenarioList/Scenario");
            foreach (var scenario in scenarioList)
            {
                AllScenarios.Add(new ScenarioOperation(rfsId++, scenario.Node, navigator, this));
            }

            var filteredOperations = new List<Operation>();
            foreach (var rfsOperationList in AllRFS)
            {
                foreach (var rfs in rfsOperationList.Value)
                {
                    if (rfs.IsSeparated)
                    {
                        filteredOperations.Add(rfs);
                    }
                }
            }
            filteredOperations.AddRange(AllScenarios);


            var netId = 0;
            foreach (var hostTypeName in filteredOperations.Select(p => p.HostTypeName).Distinct())
            {
                Add(new CatalogHostType(netId++, hostTypeName, filteredOperations));
            }
        }
    }
}