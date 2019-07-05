using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SPAFilter.SPA.Components;
using Utils;
using Utils.CollectionHelper;

namespace SPAFilter.SPA.Collection
{
    public class ServiceCatalog : Dictionary<string, CatalogNetworkElement>
    {
        private readonly string _filePath;
        public string Prefix { get; private set; }
        public List<CatalogOperation> Operations { get; private set; } = new List<CatalogOperation>();

        public ServiceCatalog(string filePath)
        {
            _filePath = filePath;
            Refresh();
        }

        public void Refresh()
        {
            Operations.Clear();
            if (!XML.IsFileXml(_filePath, out var document))
                throw new Exception($"Incorrect xml file \"{_filePath}\"!");

            var navigator = document.CreateNavigator();
            var prefix = XPATH.Execute(navigator, @"/Configuration/@scenarioPrefix");
            if(prefix == null || prefix.Count == 0)
                throw new Exception("Invalid config. Catalog must have attribute \"scenarioPrefix\".");
            Prefix = prefix.First().Value;

            var rfsList = XPATH.Execute(navigator, @"/Configuration/RFSList/RFS");
            var baseRfsList = new Dictionary<string, CatalogOperation>();
            var allRfs = new DuplicateDictionary<string, CatalogOperation>();
            var rfsId = 0;
            var neId = 0;
            foreach (var rfs in rfsList)
            {
                rfsId++;

                if(rfs.Node.Attributes == null)
                    throw new Exception("RFS must have attributes!");

                var rfsName = string.Empty;
                var isBase = string.Empty;
                var isParent = string.Empty;
                var hostType = string.Empty;

                foreach (XmlAttribute attribute in rfs.Node.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "name": rfsName = attribute.Value; break;
                        case "base": isBase = attribute.Value;  break;
                        case "parent": isParent = attribute.Value;  break;
                        case "hostType": hostType = attribute.Value;  break;
                    }
                }
                

                if(rfsName.IsNullOrEmptyTrim())
                    throw new Exception("Invalid config. RFS must have attribute \"name\".");

                if (hostType.IsNullOrEmptyTrim())
                    throw new Exception($"Invalid config. RFS \"{rfsName}\" must have attribute \"hostType\".");

                if (!TryGetValue(hostType, out var netElement))
                {
                    neId++;
                    netElement = new CatalogNetworkElement(neId, hostType);
                    Add(hostType, netElement);
                }

                var currentType = CatalogOperationType.Add | CatalogOperationType.Remove;
                if (!isBase.IsNullOrEmptyTrim())
                {
                    if (allRfs.TryGetValue(isBase, out var result))
                        allRfs.Remove(isBase);

                    if (!baseRfsList.ContainsKey(isBase))
                    {
                        if (result == null)
                        {
                            baseRfsList.Add(isBase, new CatalogOperation(rfsId, isBase, "", netElement, this));
                        }
                        else
                        {
                            var baseRfs = result.First();
                            baseRfs.Action = string.Empty;
                            baseRfsList.Add(isBase, baseRfs);
                        }
                    }

                    currentType = currentType | CatalogOperationType.Modify;
                }

                foreach (var flag in currentType.GetFlags())
                {
                    allRfs.Add(rfsName, new CatalogOperation(rfsId, rfsName, flag.ToString(), netElement, this));
                }
            }


            
            
            var scenarioList = XPATH.Execute(navigator, @"/Configuration/ScenarioList/Scenario");

        }
    }
}