using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using SPAFilter.SPA.Collection;
using Utils;
using Utils.CollectionHelper;

namespace SPAFilter.SPA.Components.SRI
{
    internal class RFSBindings
    {
        readonly CatalogOperation _baseOperation;
        public DistinctList<XmlNode> HostTypeList { get; } = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> ResourceList { get; } = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> RFSParameterList { get; } = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> RFSList { get; } = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> CFSList { get; } = new DistinctList<XmlNode>(new CatalogItemEqualityComparer());
        public DistinctList<XmlNode> CFSGroupList { get;} = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> RestrictionList { get; } = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> RFSGroupList { get;} = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> HandlerList { get; } = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> ScenarioList { get; } = new DistinctList<XmlNode>();

        internal RFSBindings(CatalogOperation baseOperation)
        {
            _baseOperation = baseOperation;
        }

        internal RFSBindings(CatalogOperation baseOperation, string rfsName, XPathNavigator navigator)
        {
            _baseOperation = baseOperation;

            BuildRFSList(XPATH.Select(navigator, $"/Configuration/RFSList/RFS[@name='{rfsName}']"));
            BuildRFSList(XPATH.Select(navigator, $"/Configuration/RFSList/RFS[@base='{rfsName}']"));
            BuildRFSList(XPATH.Select(navigator, $"/Configuration/RFSList/RFS[@parent='{rfsName}']"));

            foreach (var rfs in RFSList)
            {
                if (rfs.Attributes == null)
                    continue;

                foreach (XmlAttribute cfsAttr in rfs.Attributes)
                {
                    if (cfsAttr.Name != "name")
                        continue;

                    Add(cfsAttr.Value, navigator);
                    break;
                }
            }
        }

        void BuildRFSList(XPathResultCollection collection)
        {
            if (collection == null || collection.Count == 0)
                return;

            foreach (var rfs in collection)
            {
                RFSList.Add(rfs.Node);
            }
        }

        public void CombineWith(RFSBindings foreignBindings)
        {
            HostTypeList.AddRange(foreignBindings.HostTypeList);
            ResourceList.AddRange(foreignBindings.ResourceList);
            RFSParameterList.AddRange(foreignBindings.RFSParameterList);
            RFSList.AddRange(foreignBindings.RFSList);
            CFSList.AddRange(foreignBindings.CFSList);
            CFSGroupList.AddRange(foreignBindings.CFSGroupList);
            RestrictionList.AddRange(foreignBindings.RestrictionList);
            RFSGroupList.AddRange(foreignBindings.RFSGroupList);
            HandlerList.AddRange(foreignBindings.HandlerList);
            ScenarioList.AddRange(foreignBindings.ScenarioList);
        }

        void Add(string rfsName, XPathNavigator navigator)
        {
            try
            {
                if(!navigator.SelectFirst($"/Configuration/RFSList/RFS[@name='{rfsName}']", out var rfs))
                    throw new Exception($"Not found {rfsName}");

                var rfsHostType = rfs.Node.Attributes?["hostType"].Value;
                if (rfsHostType == null)
                    throw new Exception($"{rfsName} hasn't attribute \"hostType\"");

                AddXmlNode(XPATH.Select(navigator, $"/Configuration/HostTypeList/HostType[@name='{rfsHostType}']"), HostTypeList);

                foreach (XmlNode rfsChild in rfs.Node.ChildNodes)
                {
                    var itemName = rfsChild.Attributes?["name"].Value;
                    switch (rfsChild.Name)
                    {
                        case "Resource" when itemName != null:
                            AddXmlNode(XPATH.Select(navigator, $"/Configuration/ResourceList/Resource[@name='{itemName}']"), ResourceList);
                            break;
                        case "RFSParameter" when itemName != null:
                            AddXmlNode(XPATH.Select(navigator, $"/Configuration/RFSParameterList/RFSParameter[@name='{itemName}']"), RFSParameterList);
                            break;
                    }
                }

                AddXmlNode(XPATH.Select(navigator, $"/Configuration/RestrictionList/Restriction[RFS[@name='{rfsName}']]"), RestrictionList);
                AddXmlNode(XPATH.Select(navigator, $"/Configuration/RFSGroupList/RFSGroup[RFS[@name='{rfsName}']]"), RFSGroupList);
                AddXmlNode(XPATH.Select(navigator, $"/Configuration/HandlerList/Handler[Configuration/RFS[@name='{rfsName}']]"), HandlerList);
                AddXmlNode(XPATH.Select(navigator, $"/Configuration/ScenarioList/Scenario[RFS[@name='{rfsName}']]"), ScenarioList);


                if (!navigator.Select($"/Configuration/CFSList/CFS[RFS[@name='{rfsName}']]", out var CFSListConfig))
                    return;

                foreach (var cfs in CFSListConfig)
                {
                    var cfsName = cfs.Node.Attributes?["name"].Value;
                    if (cfsName != null)
                    {
                        AddXmlNode(XPATH.Select(navigator, $"/Configuration/CFSGroupList/CFSGroup[CFS[@name='{cfsName}']]"), CFSGroupList);
                    }

                    var cloneCFS = cfs.Node.Clone();
                    for (var index = 0; index < cloneCFS.ChildNodes.Count; index++)
                    {
                        var childNode = cloneCFS.ChildNodes[index];
                        var childNodeName = childNode.Attributes?["name"].Value;
                        switch (childNode.Name)
                        {
                            case "RFS":
                                if (navigator.SelectFirst($"/Configuration/RFSList/RFS[@name='{childNodeName}']/@hostType", out var hostType1) && hostType1.Value.Equals(rfsHostType))
                                    continue;
                                break;
                            case "RFSGroup":
                                if (navigator.Select($"/Configuration/RFSGroupList/RFSGroup[@name='{childNodeName}']/RFS/@name", out var rfsGroupList))
                                {
                                    var isExist = false;
                                    foreach (var rfsInGroup in rfsGroupList)
                                    {
                                        if (navigator.SelectFirst($"/Configuration/RFSList/RFS[@name='{rfsInGroup.Value}']/@hostType", out var hostType2) && hostType2.Value.Equals(rfsHostType))
                                        {
                                            isExist = true;
                                            break;
                                        }
                                    }

                                    if(isExist)
                                        continue;
                                }
                                break;
                        }

                        cloneCFS.RemoveChild(childNode);
                        index--;
                    }

                    CFSList.Add(cloneCFS);
                }


            }
            catch (Exception ex)
            {
                throw;
            }
        }

        static void AddXmlNode(XPathResultCollection collection, ICollection<XmlNode> catalogComponent)
        {
            if (collection == null || collection.Count <= 0)
                return;

            foreach (var item in collection)
            {
                catalogComponent.Add(item.Node);
            }
        }

        ///// <summary>
        ///// Финальная обработка биндинга.
        ///// Очищаем в RFS список ненужных RFS. Оставляем только те которые относятся непосредственно к хосту базовой операции
        ///// </summary>
        //public void Finnaly()
        //{
        //    var rfsList = RFSList.ToDictionary(x => x.Attributes?["name"]?.Value, x => x, StringComparer.CurrentCultureIgnoreCase);
        //    var rfsGroupList = RFSGroupList.ToDictionary(x => x.Attributes?["name"]?.Value, x => x, StringComparer.CurrentCultureIgnoreCase);

        //    //var dd = CFSList.SelectMany(p => p.ChildNodes.OfType<XmlNode>().Select(t => t.Attributes?["name"].Value)).Distinct().ToDictionary(x => x, x => );



        //    //foreach (var rfs in RFSList)
        //    //{
        //    //    var rfsName = rfs.Attributes?["name"]?.Value;
        //    //    var hostType = rfs.Attributes?["hostType"]?.Value;
        //    //    if (rfsName != null && hostType != null && hostType.Equals(_baseOperation.HostTypeName))
        //    //        rfsList.Add(rfsName, rfs);
        //    //}

        //    //ExcludeRedundantRFSInCFS(rfsList, rfsGroupList);
        //}

        

        ///// <summary>
        ///// Финальная обработка биндинга.
        ///// Очищаем в RFS список ненужных RFS. Если к основному сценарию или основному отсепарированному RFS отношения не имеют, то удалем ноды.
        ///// Только для сценариев и отсепарированных RFS!!!!
        ///// </summary>
        //void FinnalyTEMP()
        //{
        //    var rfsList = new Dictionary<string, XmlNode>(StringComparer.CurrentCultureIgnoreCase);
        //    var rfsGroupList = new Dictionary<string, XmlNode>(StringComparer.CurrentCultureIgnoreCase);

        //    AddCollectionDict(RFSList, rfsList, "name");
        //    AddCollectionDict(RFSGroupList, rfsGroupList, "name");

        //    ExcludeRedundantRFSInCFS(rfsList, rfsGroupList);
        //}

        //void ExcludeRedundantRFSInCFS(IReadOnlyDictionary<string, XmlNode> rfsList, IReadOnlyDictionary<string, XmlNode> rfsGroupList)
        //{
        //    for (var index = 0; index < CFSList.Count; index++)
        //    {
        //        var cfsNode = CFSList[index].Clone();
        //        for (var index2 = 0; index2 < cfsNode.ChildNodes.Count; index2++)
        //        {
        //            var childNode = cfsNode.ChildNodes[index2];
        //            switch (childNode.Name)
        //            {
        //                case "RFS":
        //                    var rfsName = childNode.Attributes?["name"]?.Value;
        //                    if (rfsName != null && rfsList.ContainsKey(rfsName))
        //                        continue;
        //                    break;
        //                case "RFSGroup":
        //                    var rfsGroupName = childNode.Attributes?["name"]?.Value;
        //                    if (rfsGroupName != null && rfsGroupList.ContainsKey(rfsGroupName))
        //                        continue;
        //                    break;
        //            }

        //            cfsNode.RemoveChild(childNode);
        //            index2--;
        //        }

        //        CFSList[index] = cfsNode;
        //    }
        //}

        //static void AddCollectionDict(IEnumerable<XmlNode> sourceCollection, Dictionary<string, XmlNode> assignCollection, string attributeName)
        //{
        //    foreach (var rfs in sourceCollection)
        //    {
        //        var attributeValue = rfs.Attributes?[attributeName]?.Value;
        //        if (attributeValue != null && !assignCollection.ContainsKey(attributeValue))
        //            assignCollection.Add(attributeValue, rfs);
        //    }
        //}
    }
}
