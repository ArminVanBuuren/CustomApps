using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessFilter.SPA.SC
{
    using System.Xml;

    public class CollectionCFS
    {
        Dictionary<string, CFS> _listCFS  = new Dictionary<string, CFS>();
        private DataTable _serviceTable;
        private Func<string, string> _getDescription;
        public CollectionCFS(DataTable serviceTable)
        {
            _serviceTable = serviceTable;
            if (_serviceTable != null)
                _getDescription = GetDescription;
        }

        public void Add(XmlDocument document, string networkElement)
        {
            List<string> regList = EvaluateXPath(document, "//RegisteredList/*");

            LoadService(EvaluateXPath(document, "//ProvisionList/*"), regList, networkElement);
            LoadService(EvaluateXPath(document, "//WithdrawalList/*"), regList, networkElement);
        }

        public static List<string> EvaluateXPath(XmlDocument document, string xpath)
        {
            List<string> collection = new List<string>();
            XmlNodeList listByXpath = document.SelectNodes(xpath);
            if (listByXpath == null)
                return collection;

            foreach (XmlNode xm in listByXpath)
            {
                //if (!collection.Any(p => p.Equals(xm.InnerText)))
                collection.Add(xm.Name);
            }
            return collection;
        }

        void LoadService(List<string> srvList, List<string> regList, string networkElement)
        {
            if(srvList.Count == 0)
                return;

            foreach (string srv in srvList)
            {
                if (!_listCFS.ContainsKey(srv))
                {
                    string description = _getDescription?.Invoke(srv);
                    description = string.IsNullOrEmpty(description) ? "-" : description;

                    _listCFS.Add(srv, new CFS(networkElement, srv, description, regList));
                }
                else
                    _listCFS[srv].AddAddition(networkElement, regList);
            }
        }

        string GetDescription(string serviceName)
        {
            //_serviceTable.Select()

            var dd = _serviceTable.Rows[10];

            var results = from myRow in _serviceTable.AsEnumerable()
                where myRow.Field<string>("SERVICE_CODE") == serviceName
                          select myRow["SERVICE_NAME"];
                //select myRow;

            if (results.Count() == 0)
                return null;


            return results.First().ToString();
        }

        public int Count => _listCFS.Count;

        public void GenerateRFS()
        {
            foreach (CFS cfs in _listCFS.Values)
            {
                cfs.GenerateRFS();
            }
        }

        public string ToXml()
        {
            string cfsXmlListMiddle = string.Empty;
            string rfsXmlListMiddle = string.Empty;

            foreach (CFS cfs in _listCFS.Values)
            {
                cfsXmlListMiddle += "\r\n" + cfs.ToXml();
                foreach (RFS rfs in cfs.RFSList)
                {
                    rfsXmlListMiddle += "\r\n" + rfs.ToXml();
                }
            }

            string result = "\r\n  <RFSList>" + rfsXmlListMiddle + "\r\n  </RFSList>\r\n" + "  <CFSList>" + cfsXmlListMiddle
                            + "\r\n  </CFSList>\r\n";

            return result;
        }
    }

    public class CFS
    {
        private Dictionary<string, HashSet<string>> networkElements = new Dictionary<string, HashSet<string>>();
        //HashSet<string> networkElements = new HashSet<string>();
        //HashSet<string> dependedServices = new HashSet<string>();

        public List<RFS> RFSList { get; } = new List<RFS>();

        public string ServiceName { get; }
        public string Description { get; }

        public CFS(string networkElement, string srvName, string description, List<string> depServices)
        {
            ServiceName = srvName;
            Description = description;
            AddAddition(networkElement, depServices);
        }

        public void AddAddition(string netElement, List<string> depServices)
        {
            if (!networkElements.ContainsKey(netElement))
            {
                networkElements.Add(netElement, new HashSet<string>());
            }

            HashSet<string> dependedServices = networkElements[netElement];
            foreach (var regSrv in depServices)
            {
                dependedServices.Add(regSrv);
            }
        }

        public void GenerateRFS()
        {
            foreach (KeyValuePair<string, HashSet<string>> element in networkElements)
            {
                RFSList.Add(new RFS(ServiceName, Description, element.Key, element.Value));
            }
        }

        public string ToXml()
        {
            string xmlStrStart = $"    <CFS name=\"{ServiceName}\" description=\"{Description}\">";
            string xmlStrMiddle = string.Empty;

            foreach (RFS rfs in RFSList)
            {
                xmlStrMiddle += "\r\n" + rfs.ToXmlCFSChild();
            }

            return xmlStrStart + xmlStrMiddle + "\r\n    </CFS>";
        }
    }
}
