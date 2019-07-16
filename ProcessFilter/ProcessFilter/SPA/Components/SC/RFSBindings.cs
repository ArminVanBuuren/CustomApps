﻿using System;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using SPAFilter.SPA.Collection;
using Utils;

namespace SPAFilter.SPA.Components.SC
{
    internal class RFSBindings
    {
        public DistinctList<XmlNode> HostTypeList { get; } = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> RFSList { get; } = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> CFSList { get; } = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> CFSGroupList { get;} = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> RestrictionList { get; } = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> RFSGroupList { get;} = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> HandlerList { get; } = new DistinctList<XmlNode>();
        public DistinctList<XmlNode> ScenarioList { get; } = new DistinctList<XmlNode>();

        internal RFSBindings()
        {

        }

        internal RFSBindings(string rfsName, XPathNavigator navigator)
        {
            BuildRFSList(XPATH.Execute(navigator, $"/Configuration/RFSList/RFS[@name='{rfsName}']"));
            BuildRFSList(XPATH.Execute(navigator, $"/Configuration/RFSList/RFS[@base='{rfsName}']"));
            BuildRFSList(XPATH.Execute(navigator, $"/Configuration/RFSList/RFS[@parent='{rfsName}']"));

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
                var getRFSHostType = XPATH.Execute(navigator, $"/Configuration/RFSList/RFS[@name='{rfsName}']/@hostType");
                if(getRFSHostType == null || getRFSHostType.Count == 0)
                    throw new Exception("RFS must have attribute \"hostType\"");

                AddXmlNode(XPATH.Execute(navigator, $"/Configuration/HostTypeList/HostType[@name='{getRFSHostType.First().Value}']"), HostTypeList);

                var CFSListConfig = XPATH.Execute(navigator, $"/Configuration/CFSList/CFS[RFS[@name='{rfsName}']]");
                if (CFSListConfig != null && CFSListConfig.Count > 0)
                {
                    foreach (var cfs in CFSListConfig)
                    {
                        CFSList.Add(cfs.Node);
                        if (cfs.Node.Attributes == null)
                            continue;

                        foreach (XmlAttribute cfsAttr in cfs.Node.Attributes)
                        {
                            if (cfsAttr.Name != "name")
                                continue;

                            AddXmlNode(XPATH.Execute(navigator, $"/Configuration/CFSGroupList/CFSGroup[CFS[@name='{cfsAttr.Value}']]"), CFSGroupList);
                            break;
                        }
                    }
                }

                AddXmlNode(XPATH.Execute(navigator, $"/Configuration/RestrictionList/Restriction[RFS[@name='{rfsName}']]"), RestrictionList);
                AddXmlNode(XPATH.Execute(navigator, $"/Configuration/RFSGroupList/RFSGroup[RFS[@name='{rfsName}']]"), RFSGroupList);
                AddXmlNode(XPATH.Execute(navigator, $"/Configuration/HandlerList/Handler[Configuration/RFS[@name='{rfsName}']]"), HandlerList);
                AddXmlNode(XPATH.Execute(navigator, $"/Configuration/ScenarioList/Scenario[RFS[@name='{rfsName}']]"), ScenarioList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        static void AddXmlNode(XPathResultCollection collection, DistinctList<XmlNode> catalogComponent)
        {
            if (collection == null || collection.Count <= 0)
                return;

            foreach (var item in collection)
            {
                catalogComponent.Add(item.Node);
            }
        }
    }
}