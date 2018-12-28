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
        Dictionary<string, CFS> _collectionCFS = new Dictionary<string, CFS>();
        private DataTable _serviceTable;
        private Func<string, string> _getDescription;
        public CollectionCFS(DataTable serviceTable)
        {
            _serviceTable = serviceTable;
            if (_serviceTable != null)
                _getDescription = GetDescription;
        }

        public void Add(string fileName, XmlDocument document, string hostType)
        {
            string operationName = fileName;
            if (operationName.StartsWith("Add", StringComparison.CurrentCultureIgnoreCase))
            {
                operationName = operationName.Substring(3, operationName.Length - 3);
            }

            if (operationName.StartsWith("Assign", StringComparison.CurrentCultureIgnoreCase))
            {
                operationName = operationName.Substring(6, operationName.Length - 6);
            }

            if (operationName.StartsWith("Delete", StringComparison.CurrentCultureIgnoreCase))
            {
                operationName = operationName.Substring(6, operationName.Length - 6);
            }

            if (operationName.StartsWith("Del", StringComparison.CurrentCultureIgnoreCase))
            {
                operationName = operationName.Substring(3, operationName.Length - 3);
            }

            if (operationName.StartsWith("Remove", StringComparison.CurrentCultureIgnoreCase))
            {
                operationName = operationName.Substring(6, operationName.Length - 6);
            }

            if (operationName.StartsWith("FR", StringComparison.CurrentCultureIgnoreCase))
            {
                operationName = operationName.Substring(2, operationName.Length - 2);
            }

            if (operationName.StartsWith("CB", StringComparison.CurrentCultureIgnoreCase))
            {
                operationName = operationName.Substring(2, operationName.Length - 2);
            }

            if (operationName.EndsWith(".xml", StringComparison.CurrentCultureIgnoreCase))
            {
                operationName = operationName.Substring(0, operationName.Length - 4);
            }



            List<string> regList = EvaluateXPath(document, "//RegisteredList/*");

            LoadService(operationName, EvaluateXPath(document, "//ProvisionList/*"), regList, hostType);
            LoadService(operationName, EvaluateXPath(document, "//WithdrawalList/*"), regList, hostType);
        }

        public static List<string> EvaluateXPath(XmlDocument document, string xpath)
        {
            List<string> collection = new List<string>();
            XmlNodeList listByXpath = document.SelectNodes(xpath);
            if (listByXpath == null)
                return collection;

            foreach (XmlNode xm in listByXpath)
            {
                collection.Add(xm.Name);
            }
            return collection;
        }

        void LoadService(string operationName, List<string> srvCodeList, List<string> regList, string hostType)
        {
            if(srvCodeList == null || srvCodeList.Count == 0)
                return;


            CFS mainCFS = null;
            foreach (string srvCode in srvCodeList)
            {
                CFS ifExist;
                if (!_collectionCFS.TryGetValue(srvCode, out ifExist))
                {
                    string description = _getDescription?.Invoke(srvCode);
                    description = string.IsNullOrEmpty(description) ? "-" : description;

                    CFS newCFS;
                    if (mainCFS == null)
                    {
                        newCFS = new CFS(srvCode, hostType,  description, regList, operationName);
                        mainCFS = newCFS;
                    }
                    else
                    {
                        newCFS = new CFS(srvCode, hostType, description, regList, mainCFS);
                    }
                    _collectionCFS.Add(srvCode, newCFS);
                }
                else
                {
                    if (operationName == "ChangeMSISDNOnSCG")
                    {

                    }
                    ifExist.AddAddition(hostType, operationName, regList);
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

        public int Count => _collectionCFS.Count;

        public void GenerateRFS()
        {
            foreach (CFS cfs in _collectionCFS.Values)
            {
                cfs.GenerateRFS();
            }
        }

        public string ToXml()
        {
            StringBuilder cfsXmlList = new StringBuilder();
            List<RFS> rfsList = new List<RFS>();
            StringBuilder rfsXmlList = new StringBuilder();
            StringBuilder rfsGroupXmlList = new StringBuilder();
            StringBuilder resourceXmlList = new StringBuilder();

            foreach (CFS cfs in _collectionCFS.Values)
            {
                cfsXmlList.Append(cfs.ToXml(_collectionCFS));
                rfsList.AddRange(cfs.RFSList);
            }

            foreach (RFS rfs in rfsList.OrderBy(p => p.HostType))
            {
                rfsXmlList.Append(rfs.ToXml());
                if (rfs.RFSGroupDependence != null)
                {
                    rfsGroupXmlList.Append(rfs.RFSGroupDependence.ToXml());
                }

                if (rfs.SC_Resource != null)
                {
                    resourceXmlList.Append(rfs.SC_Resource.ToXml());
                }
            }

            string result = "<ResourceList>" + resourceXmlList.ToString() + "</ResourceList>"
                          + "<RFSList>" + rfsXmlList.ToString() + "</RFSList>" 
                          + "<CFSList>" + cfsXmlList.ToString() + "</CFSList>"
                          + "<RFSGroup>" + rfsGroupXmlList.ToString() + "</RFSGroup>";

            return result;
        }
    }

    public class HostOperation
    {
        public string HostType { get; }
        public CFS BaseCFS { get; }

        public HashSet<string> DependenceServiceCodes { get; } = new HashSet<string>();
        public List<CFS> ChildCFS { get; } = new List<CFS>();
        
        public RFS SC_RFS { get; private set; }
        

        public int Index { get; } = 1;
        private string _operationName;
        public string OperationName
        {
            get
            {
                if(BaseCFS != null)
                    return _operationName + "_" + Index;
                return _operationName;
            }
        }

        public HostOperation(string opName, string hostType)
        {
            _operationName = opName;
            HostType = hostType;
        }

        public HostOperation(CFS baseCFS, string opName, string hostType, int index)
        {
            _operationName = opName;
            BaseCFS = baseCFS;
            HostType = hostType;
            Index = index;
        }

        public void AddDependenceService(List<string> dependsServices)
        {
            foreach (string serviceCode in dependsServices)
            {
                DependenceServiceCodes.Add(serviceCode);
            }
        }

        internal RFS GenerateRFS(CFS parentCFS)
        {
            SC_RFS = new RFS(parentCFS, this);
            return SC_RFS;
        }
    }

    public class CFS
    {
        public string ServiceCode { get; }
        public string Description { get; }
        internal Dictionary<string, HostOperation> HostOperations = new Dictionary<string, HostOperation>();
        public List<RFS> RFSList { get; } = new List<RFS>();


        public CFS(string srvCode, string hostType, string description, List<string> dependsServices, string operationName)
        {
            ServiceCode = srvCode;
            Description = description;
            AddAddition(hostType, operationName, dependsServices);
        }

        public CFS(string srvCode, string hostType, string description, List<string> dependsServices, CFS baseCFS)
        {
            ServiceCode = srvCode;
            Description = description;
            AddAddition(hostType, baseCFS, dependsServices);
        }

        internal void AddAddition(string hostType, string operationName, List<string> dependsServices)
        {
            HostOperation hostOp;
            if (!HostOperations.TryGetValue(hostType, out hostOp))
            {
                hostOp = new HostOperation(operationName, hostType);
                HostOperations.Add(hostType, hostOp);
            }

            hostOp.AddDependenceService(dependsServices);
        }

        void AddAddition(string hostType, CFS baseCFS, List<string> dependsServices)
        {
            HostOperation hostOp;
            if (!HostOperations.TryGetValue(hostType, out hostOp))
            {
                HostOperation parentHostOp = baseCFS.HostOperations[hostType];
                parentHostOp.ChildCFS.Add(this);

                hostOp = new HostOperation(baseCFS, parentHostOp.OperationName, hostType, parentHostOp.ChildCFS.Count + 1);
                HostOperations.Add(hostType, hostOp);
            }

            
            hostOp.AddDependenceService(dependsServices);
        }

        public void GenerateRFS()
        {
            foreach (KeyValuePair<string, HostOperation> element in HostOperations)
            {
                RFSList.Add(element.Value.GenerateRFS(this));
            }
        }

        public string ToXml(Dictionary<string, CFS> allCFSs)
        {
            string xmlStrStart = $"<CFS name=\"{ServiceCode}\" description=\"{Description}\">";
            string xmlStrMiddle = string.Empty;

            foreach (RFS rfs in RFSList)
            {
                xmlStrMiddle += rfs.ToXmlCFSChild(allCFSs);
            }

            return xmlStrStart + xmlStrMiddle + "</CFS>";
        }
    }
}
