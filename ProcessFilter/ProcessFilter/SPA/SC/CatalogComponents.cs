using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Utils.XmlRtfStyle;
using Utils.XPathHelper;

namespace ProcessFilter.SPA.SC
{
    public class CatalogComponents
    {
        private List<HostOperation> _hostOperations = new List<HostOperation>();
        public Dictionary<string, CFS> CollectionCFS { get; } = new Dictionary<string, CFS>();
        public Dictionary<string, RFS> CollectionRFS { get; } = new Dictionary<string, RFS>();
        public Dictionary<string, Resource> CollectionResource { get; } = new Dictionary<string, Resource>();
        public Dictionary<string, RFSGroup> CollectionRFSGroup { get; } = new Dictionary<string, RFSGroup>();
        public CFSGroups CollectionMutexCFSGroup { get; } = new CFSGroups();

        private DataTable _serviceTable;
        private Func<string, string> _getDescription;

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

            BindingServices bindSrv = new BindingServices(document);

            Dictionary<string, XPathResult> getServices = new Dictionary<string, XPathResult>();
            AppendServices(getServices, XPathHelper.Execute(document.CreateNavigator(), "//ProvisionList/*"));
            AppendServices(getServices, XPathHelper.Execute(document.CreateNavigator(), "//WithdrawalList/*"));

            if (getServices.Count > 0)
            {
                HostOperation hostOp = new HostOperation(operationName, hostType, RtfFromXml.GetXmlString(document.OuterXml), bindSrv);
                LoadService(hostOp, getServices, bindSrv);
            }
        }

        void RemovePrefix(ref string operationName, string prefix)
        {
            if (operationName.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
            {
                operationName = operationName.Substring(prefix.Length, operationName.Length - prefix.Length);
            }
        }

        static void AppendServices(Dictionary<string, XPathResult> services, XPathResultCollection result)
        {
            if(result == null || result.Count == 0)
                return;
            foreach (XPathResult res in result)
            {
                if (res.NodeName.Equals("Include", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (BindingServices.isAttributeContains(res, "Type", "Restricted"))
                    continue;

                if (!services.ContainsKey(res.NodeName))
                    services.Add(res.NodeName, res);
            }
        }

        void LoadService(HostOperation hostOp, Dictionary<string, XPathResult> srvCodeList, BindingServices bindSrv)
        {
            Dictionary<HostOperation, List<RFS>> resHostOp = new Dictionary<HostOperation, List<RFS>>();
            foreach (XPathResult srvCode in srvCodeList.Values)
            {
                if (!CollectionCFS.TryGetValue(srvCode.NodeName, out CFS getOrCreateCFS))
                {
                    string description = _getDescription?.Invoke(srvCode.NodeName);
                    description = string.IsNullOrEmpty(description) ? "-" : description;

                    getOrCreateCFS = new CFS(srvCode.NodeName, description, hostOp);
                    CollectionCFS.Add(srvCode.NodeName, getOrCreateCFS);

                    hostOp.ChildCFS.Add(getOrCreateCFS);
                }
                else
                {
                    if (getOrCreateCFS.IsNewHost(hostOp))
                        hostOp.ChildCFS.Add(getOrCreateCFS);
                }
            }

            if (hostOp.ChildCFS.Count > 0)
                _hostOperations.Add(hostOp);
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

        public int Count => CollectionCFS.Count;

        public void GenerateRFS()
        {
            foreach (HostOperation hostOp in _hostOperations)
            {
                hostOp.GenerateRFS();
            }
        }

        public string ToXml()
        {
            StringBuilder cfsXmlList = new StringBuilder();
            StringBuilder rfsXmlList = new StringBuilder();
            StringBuilder resourceXmlList = new StringBuilder();
            StringBuilder rfsGroupXmlList = new StringBuilder();

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

            string result = "<ResourceList>" + resourceXmlList.ToString() + "</ResourceList>"
                          + "<RFSList>" + rfsXmlList.ToString() + "</RFSList>"
                          + "<CFSList>" + cfsXmlList.ToString() + "</CFSList>"
                          + "<CFSGroupList>" + CollectionMutexCFSGroup.ToXml() + "</CFSGroupList>"
                          + "<RFSGroup>" + rfsGroupXmlList.ToString() + "</RFSGroup>";

            return result;
        }
    }
}
