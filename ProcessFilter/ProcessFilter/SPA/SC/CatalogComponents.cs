﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Utils;
using static Utils.XPATH;

namespace SPAFilter.SPA.SC
{
    public class CatalogComponents
    {
        private readonly List<HostOperation> _hostOperations = new List<HostOperation>();
        public Dictionary<string, CFS> CollectionCFS { get; } = new Dictionary<string, CFS>();
        public Dictionary<string, RFS> CollectionRFS { get; } = new Dictionary<string, RFS>();
        public Dictionary<string, Resource> CollectionResource { get; } = new Dictionary<string, Resource>();
        public Dictionary<string, RFSGroup> CollectionRFSGroup { get; } = new Dictionary<string, RFSGroup>();
        public CFSGroups CollectionMutexCFSGroup { get; } = new CFSGroups();

        private readonly DataTable _serviceTable;
        private readonly Func<string, string> _getDescription;

        public CatalogComponents(DataTable serviceTable)
        {
            _serviceTable = serviceTable;
            if (_serviceTable != null)
                _getDescription = GetDescription;
        }

        public void Add(string fileName, XmlDocument document, string hostType)
        {
            string operationName = fileName;
            foreach (string prefix in new[] { "Add", "Assign", "Delete", "Del", "Remove", "FR", "CB" })
            {
                RemovePrefix(ref operationName, prefix);
            }

            operationName = operationName.Replace(" ", "");

            var bindSrv = new BindingServices(document);
            var hostOp = new HostOperation(operationName, hostType, bindSrv);
            var getServices = new Dictionary<string, XPathResult>();

            if (GetServices(getServices, Execute(document.CreateNavigator(), "//ProvisionList/*")))
            {
                if (!IsExistSameHostOperation(hostOp, getServices, HostOperation.LinkType.Add))
                    LoadNewService(hostOp, getServices, HostOperation.LinkType.Add); //bindSrv
            }
            else if (GetServices(getServices, Execute(document.CreateNavigator(), "//WithdrawalList/*")))
            {
                if (!IsExistSameHostOperation(hostOp, getServices, HostOperation.LinkType.Remove))
                    LoadNewService(hostOp, getServices, HostOperation.LinkType.Remove); //bindSrv
            }
        }

        static void RemovePrefix(ref string operationName, string prefix)
        {
            if (operationName.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
            {
                operationName = operationName.Substring(prefix.Length, operationName.Length - prefix.Length);
            }
        }

        static bool GetServices(Dictionary<string, XPathResult> services, XPathResultCollection result)
        {
            if(result == null || result.Count == 0)
                return false;

            foreach (XPathResult res in result)
            {
                if (res.NodeName.Equals("Include", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (BindingServices.isAttributeContains(res, "Type", "Restricted"))
                    continue;

                if (!services.ContainsKey(res.NodeName))
                    services.Add(res.NodeName, res);
            }

            return result.Count > 0;
        }

        bool IsExistSameHostOperation(HostOperation newHostOp, Dictionary<string, XPathResult> newServices, HostOperation.LinkType type)
        {
            foreach (HostOperation existhostOp in _hostOperations)
            {
                //string xmlTEMP = string.Join("\r\n\r\n", newHostOp.XML_BODY) + "\r\n\r\n" + string.Join("\r\n\r\n", existhostOp.XML_BODY);
                if (existhostOp.HostOperationName.Equals(newHostOp.HostOperationName, StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (XPathResult srvCode in newServices.Values)
                    {
                        if (existhostOp.ChildCFS.TryGetValue(srvCode.NodeName, out HostOperation.CFS_RFS cfs_rfs))
                        {
                            cfs_rfs.ChangeLinkType(type);
                        }
                        else
                        {
                            AddCFS(srvCode, existhostOp, type);
                        }
                    }

                    newServices.Clear();
                    existhostOp.CombineSameHostOperation(newHostOp);
                    return true;
                }

                if (!existhostOp.HostType.Equals(newHostOp.HostType, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                List<string> serviceForRemove = new List<string>();
                foreach (KeyValuePair<string, XPathResult> srvCode in newServices)
                {
                    if (existhostOp.ChildCFS.TryGetValue(srvCode.Key, out HostOperation.CFS_RFS cfs_rfs))
                    {
                        cfs_rfs.ChangeLinkType(type);
                        serviceForRemove.Add(srvCode.Key);
                    }
                }

                if (serviceForRemove.Count > 0)
                {
                    foreach (string serviceCode in serviceForRemove)
                    {
                        newServices.Remove(serviceCode);
                    }
                    existhostOp.CombineSameHostOperation(newHostOp);
                }
            }

            return newServices.Count == 0;
        }

        void LoadNewService(HostOperation hostOp, Dictionary<string, XPathResult> srvCodeList, HostOperation.LinkType link) //BindingServices bindSrv
        {
            //var resHostOp = new Dictionary<HostOperation, List<RFS>>();
            foreach (XPathResult srvCode in srvCodeList.Values)
            {
                AddCFS(srvCode, hostOp, link);
            }

            if (hostOp.ChildCFS.Count > 0)
                _hostOperations.Add(hostOp);
        }

        void AddCFS(XPathResult srvCode, HostOperation hostOp, HostOperation.LinkType link)
        {
            if (!CollectionCFS.TryGetValue(srvCode.NodeName, out CFS getExistCFS))
            {
                string description = _getDescription?.Invoke(srvCode.NodeName);
                description = string.IsNullOrEmpty(description) ? "-" : description;

                CFS createNewCFS = new CFS(srvCode.NodeName, description, hostOp, link);
                CollectionCFS.Add(srvCode.NodeName, createNewCFS);
            }
            else
            {
                getExistCFS.IsNewHost(hostOp, link);
            }
        }

        string GetDescription(string serviceCode)
        {
            var results = from myRow in _serviceTable.AsEnumerable()
                          where myRow.Field<string>("SERVICE_CODE") == serviceCode
                          select myRow["SERVICE_NAME"];

            if (!results.Any())
                return null;

            return results.First().ToString();
        }

        public void GenerateRFS()
        {
            foreach (HostOperation hostOp in _hostOperations)
            {
                hostOp.GenerateRFS();
            }
        }

        public string ToXml()
        {
            var cfsXmlList = new StringBuilder();
            var rfsXmlList = new StringBuilder();
            var resourceXmlList = new StringBuilder();
            var rfsGroupXmlList = new StringBuilder();

            foreach (CFS cfs in CollectionCFS.Values)
            {
                cfsXmlList.Append(cfs.ToXml(this));

                foreach (RFS rfs in cfs.RFSList)
                {
                    if (!CollectionRFS.ContainsKey(rfs.Name))
                        CollectionRFS.Add(rfs.Name, rfs);
                }
            }

            foreach (RFS rfs in CollectionRFS.Values.OrderBy(p => p.HostType).ThenBy(x => x.Name))
            {
                rfsXmlList.Append(rfs.ToXml());
                if (rfs.SC_Resource != null && !CollectionResource.ContainsKey(rfs.SC_Resource.Name))
                {
                    CollectionResource.Add(rfs.SC_Resource.Name, rfs.SC_Resource);
                    resourceXmlList.Append(rfs.SC_Resource.ToXml());
                }
            }

            // коллекция RFSGroup должна быть в конце т.к. она формируется только после того когда все RFS созданы
            foreach (RFSGroup rfsGroup in CollectionRFSGroup.Values)
            {
                rfsGroupXmlList.Append(rfsGroup.ToXml());
            }

            string result = "<ResourceList>" + resourceXmlList + "</ResourceList>"
                          + "<RFSList>" + rfsXmlList + "</RFSList>"
                          + "<CFSList>" + cfsXmlList + "</CFSList>"
                          + "<CFSGroupList>" + CollectionMutexCFSGroup.ToXml() + "</CFSGroupList>"
                          + "<RFSGroup>" + rfsGroupXmlList + "</RFSGroup>";

            return result;
        }
    }
}