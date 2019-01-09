﻿using System;
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


        protected internal BindingServices(XmlDocument document)
        {
            XPathNavigator navigator = document.CreateNavigator();
            XPathResultCollection getServices = XPathHelper.Execute(document.CreateNavigator(), "//RegisteredList/*");
            XPathResultCollection getHaltMode = XPathHelper.Execute(document.CreateNavigator(), "//RegisteredList/@HaltMode");
            XPathResultCollection getType = XPathHelper.Execute(document.CreateNavigator(), "//RegisteredList/@Type");
            bool isDependency = getHaltMode != null && getHaltMode.Count > 0 && (getHaltMode.First().Value.Equals("CancelOperation", StringComparison.CurrentCultureIgnoreCase));
            bool isAny = getType != null && getType.Count > 0 && getType.First().Value.Equals("AnyOfListed", StringComparison.CurrentCultureIgnoreCase);



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
                    else if (!isAny && isAttributeContains(srv, "Type", "Mandatory"))
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
            if (xmlResult.Node.Attributes == null)
                return false;

            foreach (XmlAttribute attr in xmlResult.Node.Attributes)
            {
                if (!attr.Name.Equals(attributeName, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (attr.Value.Equals(typeValue, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        protected internal void AddRange(BindingServices bindServ)
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