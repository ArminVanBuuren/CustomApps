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
        public HashSet<string> DependenceServices { get; } = new HashSet<string>();
        public HashSet<string> RestrictedServices { get; } = new HashSet<string>();
        public string DependenceType { get; private set; } = "Any";


        public BindingServices(XmlDocument document)
        {
            XPathNavigator navigator = document.CreateNavigator();
            XPathResultCollection getServices = XPathHelper.Execute(document.CreateNavigator(), "//RegisteredList/*");
            XPathResultCollection getHaltMode = XPathHelper.Execute(document.CreateNavigator(), "//RegisteredList/@HaltMode");
            bool isDependency = getHaltMode != null && getHaltMode.Count != 0 && (getHaltMode.First().Value.Equals("CancelOperation"));
            

            if (getServices != null)
            {
                foreach (XPathResult srv in getServices)
                {
                    if (srv.NodeName.Equals("Include", StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    if (isAttributeContains(srv, "Type", "Restricted"))
                    {
                        RestrictedServices.Add(srv.NodeName);
                        continue;
                    }
                    else if (isAttributeContains(srv, "Type", "Mandatory"))
                    {
                        DependenceType = "All";
                    }

                    
                    if (isDependency || isAttributeContains(srv, "HaltMode", "CancelOperation"))
                    {
                        DependenceServices.Add(srv.NodeName);
                    }
                }
            }
        }


        public static bool isAttributeContains(XPathResult xmlResult, string attributeName, string typeValue)
        {
            if (xmlResult.Node.Attributes != null)
            {
                foreach (XmlAttribute attr in xmlResult.Node.Attributes)
                {
                    if (attr.Name.Equals(attributeName, StringComparison.CurrentCultureIgnoreCase))
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
        }

        public override string ToString()
        {
            return $"DEP:[{DependenceServices.Count}];RESTR:[{RestrictedServices.Count}]";
        }
    }
}