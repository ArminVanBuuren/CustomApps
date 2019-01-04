using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using Utils.XPathHelper;
using System.Xml;
using Utils.XmlRtfStyle;

namespace ProcessFilter.SPA.SC
{
    public class CFS
    {
        public string ServiceCode { get; }
        public string Description { get; }
        internal Dictionary<string, HostOperation> HostOperations = new Dictionary<string, HostOperation>();
        public List<RFS> RFSList { get; } = new List<RFS>();


        public CFS(string srvCode, string hostType, string description, BindingServices bindSrv, string operationName)
        {
            ServiceCode = srvCode;
            Description = description;
            AddAddition(hostType, operationName, bindSrv);
        }

        public CFS(string srvCode, string hostType, string description, BindingServices bindSrv, CFS baseCFS)
        {
            ServiceCode = srvCode;
            Description = description;
            AddAddition(hostType, baseCFS, bindSrv);
        }

        internal void AddAddition(string hostType, string operationName, BindingServices bindSrv)
        {
            HostOperation hostOp;
            if (!HostOperations.TryGetValue(hostType, out hostOp))
            {
                hostOp = new HostOperation(operationName, hostType, bindSrv);
                HostOperations.Add(hostType, hostOp);
            }
            else
                hostOp.BindServices.AddRange(bindSrv);
        }

        internal void AddAddition(string hostType, CFS baseCFS, BindingServices bindSrv)
        {
            HostOperation hostOp;
            if (!HostOperations.TryGetValue(hostType, out hostOp))
            {
                HostOperation parentHostOp = baseCFS.HostOperations[hostType];
                parentHostOp.ChildCFS.Add(this);

                hostOp = new HostOperation(baseCFS, parentHostOp.OperationName, hostType, parentHostOp.ChildCFS.Count + 1, bindSrv);
                HostOperations.Add(hostType, hostOp);
            }
            else
                hostOp.BindServices.AddRange(bindSrv);
        }

        public void GenerateRFS()
        {
            foreach (KeyValuePair<string, HostOperation> element in HostOperations)
            {
                RFSList.Add(element.Value.GenerateRFS(this));
            }
        }

        public string ToXml(CatalogComponents allCompontens)
        {
            string xmlStrStart = $"<CFS name=\"{ServiceCode}\" description=\"{Description}\">";
            string xmlStrMiddle = string.Empty;

            foreach (RFS rfs in RFSList)
            {
                xmlStrMiddle += rfs.ToXmlCFSChild(allCompontens.CollectionCFS, allCompontens.CollectionRFSGroup);
            }

            if (ServiceCode == "FRPOPMAP")
            {

            }

            //if (HostOperations.Values.All(p => p.BindServices.RestrictedServices.Count > 0))
            {
                StringBuilder allOperationsWithCFS = new StringBuilder();
                List<string> listOfRestrictedOnCFS = new List<string>();

                var restSrv = HostOperations.Values.Select(p => p.BindServices.RestrictedServices);
                var intersection = restSrv
                    .Skip(1)
                    .Aggregate(new HashSet<string>(restSrv.First()), (h, e) => { h.IntersectWith(e); return h; });

                allCompontens.CollectionMutexCFSGroup.AddCFSGroup(ServiceCode, intersection);

                HashSet<string> xmlBody = new HashSet<string>();
                foreach (var VARIABLE in HostOperations.Values.Where(p => p.BindServices.RestrictedServices.Count > 0).Select(x => x.BindServices.XML_BODY))
                {
                    foreach (string VARIABLE2 in VARIABLE)
                    {
                        xmlBody.Add(VARIABLE2);
                    }
                }

                allOperationsWithCFS.Append(string.Join("\r\n", xmlBody));
                string allOperationsWithCFSRes = "<XML>" + allOperationsWithCFS.ToString() + "</XML>";
            }

            

            return xmlStrStart + xmlStrMiddle + "</CFS>";
        }

        public override string ToString()
        {
            return ServiceCode;
        }
    }
}
