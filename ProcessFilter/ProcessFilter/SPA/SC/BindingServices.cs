using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Utils.XmlRtfStyle;
using Utils.XPathHelper;

namespace ProcessFilter.SPA.SC
{
    public class BindingServices
    {
        public List<string> XML_BODY { get; } = new List<string>();
        public HashSet<string> DependenceServices { get; } = new HashSet<string>();
        public HashSet<string> RestrictedServices { get; } = new HashSet<string>();
        public string DependenceType { get; private set; } = "Any";


        public BindingServices(XmlDocument document)
        {
            XPathNavigator navigator = document.CreateNavigator();
            XPathResultCollection getServices = XPathHelper.Execute(document.CreateNavigator(), "//RegisteredList/*");

            if (getServices != null)
            {
                foreach (XPathResult srv in getServices)
                {
                    if (isExistValueType(srv, "Restricted"))
                    {
                        RestrictedServices.Add(srv.NodeName);
                        continue;
                    }
                    else if (isExistValueType(srv, "Mandatory"))
                    {
                        DependenceType = "All";
                    }

                    if (!srv.NodeName.Equals("Include", StringComparison.CurrentCultureIgnoreCase))
                        DependenceServices.Add(srv.NodeName);
                }
            }


            if (RestrictedServices.Count > 0 || DependenceServices.Count > 0)
                XML_BODY.Add(RtfFromXml.GetXmlString(document.OuterXml));
        }


        public static bool isExistValueType(XPathResult xmlResult, string typeValue)
        {
            if (xmlResult.Node.Attributes != null)
            {
                foreach (XmlAttribute attr in xmlResult.Node.Attributes)
                {
                    if (attr.Name.Equals("Type", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (attr.Value.Equals(typeValue, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        public void AddRange(BindingServices bindServ)
        {
            if (DependenceType != bindServ.DependenceType && bindServ.DependenceType == "All")
                DependenceType = "All";

            foreach (string serviceCode in bindServ.DependenceServices)
            {
                DependenceServices.Add(serviceCode);
            }

            foreach (string serviceCode in bindServ.RestrictedServices)
            {
                RestrictedServices.Add(serviceCode);
            }

            XML_BODY.AddRange(bindServ.XML_BODY);
        }

        public override string ToString()
        {
            return $"DEP:[{DependenceServices.Count}];RESTR:[{RestrictedServices.Count}]";
        }
    }
}