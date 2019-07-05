using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using SPAFilter.SPA.Collection;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    class AllBindingsByRFS
    {
        public List<XmlNode> CFSList_INFO { get; } = new List<XmlNode>();
        public List<XmlNode> CFSGroupList_INFO { get; } = new List<XmlNode>();
        public List<XmlNode> RestrictionList_INFO { get; } = new List<XmlNode>();
        public List<XmlNode> RFSGroupList_INFO { get; } = new List<XmlNode>();
        public List<XmlNode> HandlerList_INFO { get; } = new List<XmlNode>();
        public List<XmlNode> ScenarioList_INFO { get; } = new List<XmlNode>();

        public AllBindingsByRFS()
        {

        }
        public void Add(string name, XPathNavigator navigator)
        {
            var CFSListInfo = XPATH.Execute(navigator, $"/Configuration/CFSList/CFS[RFS[@name='{name}']]");
            if (CFSListInfo != null && CFSListInfo.Count > 0)
            {
                foreach (var CFS in CFSListInfo)
                {
                    CFSList_INFO.Add(CFS.Node);
                    if (CFS.Node.Attributes == null)
                        continue;

                    foreach (XmlAttribute cfsAttr in CFS.Node.Attributes)
                    {
                        if (cfsAttr.Name != "name")
                            continue;

                        var CFSGroupListInfo = XPATH.Execute(navigator, $"/Configuration/CFSGroupList/CFSGroup[CFS[@name='{cfsAttr.Value}']]");
                        if (CFSGroupListInfo != null && CFSGroupListInfo.Count > 0)
                        {
                            foreach (var CFSGroup in CFSGroupListInfo)
                            {
                                CFSGroupList_INFO.Add(CFSGroup.Node);
                            }
                        }
                        break;
                    }
                }
            }

            var RestrictionListInfo = XPATH.Execute(navigator, $"/Configuration/RestrictionList/Restriction[RFS[@name='{name}']]");
            if (RestrictionListInfo != null && RestrictionListInfo.Count > 0)
            {
                foreach (var Restriction in RestrictionListInfo)
                {
                    RestrictionList_INFO.Add(Restriction.Node);
                }
            }

            var RFSGroupListInfo = XPATH.Execute(navigator, $"/Configuration/RFSGroupList/RFSGroup[RFS[@name='{name}']]");
            if (RFSGroupListInfo != null && RFSGroupListInfo.Count > 0)
            {
                foreach (var RFSGroup in RFSGroupListInfo)
                {
                    RFSGroupList_INFO.Add(RFSGroup.Node);
                }
            }

            var HandlerList = XPATH.Execute(navigator, $"/Configuration/HandlerList/Handler[Configuration/RFS[@name='{name}']]");
            if (HandlerList != null && HandlerList.Count > 0)
            {
                foreach (var Handler in HandlerList)
                {
                    HandlerList_INFO.Add(Handler.Node);
                }
            }

            var ScenarioList = XPATH.Execute(navigator, $"/Configuration/ScenarioList/Scenario[RFS[@name='{name}']]");
            if (ScenarioList != null && ScenarioList.Count > 0)
            {
                foreach (var Scenario in ScenarioList)
                {
                    ScenarioList_INFO.Add(Scenario.Node);
                }
            }
        }
    }


    public sealed class CatalogOperation : ObjectTemplate
    {
        private readonly string _name;
        private readonly ServiceCatalog _catalog;
        private readonly ObjectTemplate _parent;
        private readonly XmlNode _document;

        internal string Action { get; set; }

        public override double FileSize { get; } = 0;
        public override string FilePath { get; } = null;

        [DGVColumn(ColumnPosition.Before, "NE")]
        public string NetworkElement => _parent.Name;

        [DGVColumn(ColumnPosition.After, "Name")]
        public override string Name => $"{_catalog.Prefix}{Action}.{_name}";

        public string Body
        {
            get
            {
                var builder = new StringBuilder();
                var navigator = _document.CreateNavigator();
                builder.Append("<Configuration>");

                var HostTypeListInfo = XPATH.Execute(navigator, $"/Configuration/HostTypeList/HostType[@name='{_parent.Name}']");
                if (HostTypeListInfo != null && HostTypeListInfo.Count > 0)
                {
                    builder.Append("<HostTypeList>");
                    builder.Append(HostTypeListInfo.First().Node.OuterXml);
                    builder.Append("</HostTypeList>");
                }

                var allBindings = new AllBindingsByRFS();

                builder.Append("<RFSList>");
                AppendBody(builder, XPATH.Execute(navigator, $"/Configuration/RFSList/RFS[@name='{_name}']"), navigator, allBindings);
                AppendBody(builder, XPATH.Execute(navigator, $"/Configuration/RFSList/RFS[@base='{_name}']"), navigator, allBindings);
                AppendBody(builder, XPATH.Execute(navigator, $"/Configuration/RFSList/RFS[@parent='{_name}']"), navigator, allBindings);
                builder.Append("</RFSList>");

                builder.Append("<CFSList>");
                foreach (var allBinds in allBindings.CFSList_INFO.Distinct())
                {
                    builder.Append(allBinds.OuterXml);
                }
                builder.Append("</CFSList>");

                builder.Append("<CFSGroupList>");
                foreach (var allBinds in allBindings.CFSGroupList_INFO.Distinct())
                {
                    builder.Append(allBinds.OuterXml);
                }
                builder.Append("</CFSGroupList>");

                builder.Append("<HandlerList>");
                foreach (var allBinds in allBindings.HandlerList_INFO.Distinct())
                {
                    builder.Append(allBinds.OuterXml);
                }
                builder.Append("</HandlerList>");

                builder.Append("<RFSGroupList>");
                foreach (var allBinds in allBindings.RFSGroupList_INFO.Distinct())
                {
                    builder.Append(allBinds.OuterXml);
                }
                builder.Append("</RFSGroupList>");

                builder.Append("<RestrictionList>");
                foreach (var allBinds in allBindings.RestrictionList_INFO.Distinct())
                {
                    builder.Append(allBinds.OuterXml);
                }
                builder.Append("</RestrictionList>");

                builder.Append("<ScenarioList>");
                foreach (var allBinds in allBindings.ScenarioList_INFO.Distinct())
                {
                    builder.Append(allBinds.OuterXml);
                }
                builder.Append("</ScenarioList>");


                builder.Append("</Configuration>");
                return XML.PrintXml(builder.ToString());
            }
        }


        static void AppendBody(StringBuilder builder, XPathResultCollection collection, XPathNavigator navigator, AllBindingsByRFS allBindings)
        {
            if(collection == null || collection.Count == 0)
                return;

            foreach (var RFS in collection)
            {
                builder.Append(RFS.Node.OuterXml);

                if (RFS.Node.Attributes == null)
                    continue;

                foreach (XmlAttribute cfsAttr in RFS.Node.Attributes)
                {
                    if (cfsAttr.Name != "name")
                        continue;

                    allBindings.Add(cfsAttr.Value, navigator);
                    break;
                }
            }
        }

        public CatalogOperation(int id, string name, string action, XmlNode document, ObjectTemplate parentElement, ServiceCatalog catalog) :base(id)
        {
            _name = name;
            _catalog = catalog;
            _parent = parentElement;
            _document = document;
            Action = action;
        }
    }
}
