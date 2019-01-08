using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessFilter.SPA.SC
{
    public class HostOperation
    {
        public string HostType { get; }
        public string OperationName { get; }
        public string XML_BODY { get; }
        public List<CFS> ChildCFS { get; } = new List<CFS>();
        public BindingServices BindServices { get; }

        public HostOperation(string opName, string hostType, string xmlBody, BindingServices bindServ)
        {
            OperationName = opName;
            HostType = hostType;
            XML_BODY = xmlBody;
            BindServices = bindServ;
        }

        public void GenerateRFS()
        {
            if (ChildCFS.Count > 1)
            {
                Resource resource = new Resource(this);
                int index = 0;
                foreach (CFS cfs in ChildCFS)
                {
                    RFS rfs = new RFS(cfs, this, resource, ++index);
                    cfs.RFSList.Add(rfs);
                }
            }
            else
            {
                foreach (CFS cfs in ChildCFS)
                {
                    RFS rfs = new RFS(cfs, this);
                    cfs.RFSList.Add(rfs);
                }
            }
        }

        public override string ToString()
        {
            return $"{HostType}_{OperationName}";
        }
    }
}