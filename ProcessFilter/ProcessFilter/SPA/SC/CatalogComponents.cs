using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Utils.XPathHelper;

namespace ProcessFilter.SPA.SC
{
    public class CatalogComponents
    {
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

            BindingServices bindSrv = new BindingServices(document);

            LoadService(operationName, XPathHelper.Execute(document.CreateNavigator(), "//ProvisionList/*"), bindSrv, hostType);
            LoadService(operationName, XPathHelper.Execute(document.CreateNavigator(), "//WithdrawalList/*"), bindSrv, hostType);
        }

        void RemovePrefix(ref string operationName, string prefix)
        {
            if (operationName.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
            {
                operationName = operationName.Substring(prefix.Length, operationName.Length - prefix.Length);
            }
        }

        void LoadService(string operationName, XPathResultCollection srvCodeList, BindingServices bindSrv, string hostType)
        {
            if (srvCodeList == null || srvCodeList.Count == 0)
                return;

            CFS mainCFS = null;
            foreach (XPathResult srvCode in srvCodeList)
            {
                if (srvCode.NodeName.Equals("Include", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (BindingServices.isExistValueType(srvCode, "Restricted"))
                    continue;

                CFS ifExist;
                if (!CollectionCFS.TryGetValue(srvCode.NodeName, out ifExist))
                {
                    string description = _getDescription?.Invoke(srvCode.NodeName);
                    description = string.IsNullOrEmpty(description) ? "-" : description;

                    CFS newCFS;
                    if (mainCFS == null)
                    {
                        newCFS = new CFS(srvCode.NodeName, hostType, description, bindSrv, operationName);
                        mainCFS = newCFS;
                    }
                    else
                    {
                        newCFS = new CFS(srvCode.NodeName, hostType, description, bindSrv, mainCFS);
                    }

                    CollectionCFS.Add(srvCode.NodeName, newCFS);
                }
                else
                {
                    if (mainCFS == null)
                    {
                        ifExist.AddAddition(hostType, operationName, bindSrv);
                        mainCFS = ifExist;
                    }
                    else
                    {
                        ifExist.AddAddition(hostType, mainCFS, bindSrv);
                    }
                }
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

        public int Count => CollectionCFS.Count;

        public void GenerateRFS()
        {
            foreach (CFS cfs in CollectionCFS.Values)
            {
                cfs.GenerateRFS();
            }
        }

        public string ToXml()
        {
            StringBuilder cfsXmlList = new StringBuilder();
            List<RFS> rfsList = new List<RFS>();
            StringBuilder rfsXmlList = new StringBuilder();
            StringBuilder resourceXmlList = new StringBuilder();
            StringBuilder rfsGroupXmlList = new StringBuilder();

            foreach (CFS cfs in CollectionCFS.Values)
            {
                cfsXmlList.Append(cfs.ToXml(this));
                rfsList.AddRange(cfs.RFSList);
            }

            foreach (RFS rfs in rfsList.OrderBy(p => p.HostType).ThenBy(x => x.Name))
            {
                CollectionRFS.Add(rfs.Name, rfs);

                rfsXmlList.Append(rfs.ToXml());
                if (rfs.SC_Resource != null)
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
