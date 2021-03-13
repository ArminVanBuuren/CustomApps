using System.Collections.Generic;

namespace SPAFilter.SPA.SC
{
    public class HostOperation
    {
        public string HostType { get; }
        public string HostOperationName => $"{HostType}_{OperationName}";
        public string OperationName { get; }

        protected internal List<string> XML_BODY { get; } = new List<string>();
        internal Dictionary<string, CFS_RFS> ChildCFS { get; } = new Dictionary<string, CFS_RFS>();
        protected internal BindingServices BindServices { get; }

        public HostOperation(string opName, string hostType, BindingServices bindServ) //,string xmlBody
        {
            OperationName = opName;
            HostType = hostType;
            //XML_BODY.Add(xmlBody);
            BindServices = bindServ;
        }

        protected internal void CombineSameHostOperation(HostOperation hostOp)
	        //XML_BODY.AddRange(hostOp.XML_BODY);
	        => BindServices.AddRange(hostOp.BindServices);

        internal void AddChildRFS(CFS cfs, LinkType linkType)
        {
            var cfsrfs = new CFS_RFS(cfs, linkType, this);
            ChildCFS.Add(cfs.Name, cfsrfs);
        }

        public void GenerateRFS()
        {
            if (ChildCFS.Count > 1)
            {
                var resource = new Resource(this);
                var index = 0;
                foreach (var cfsRfs in ChildCFS.Values)
                {
                    var rfs = new RFS(cfsRfs, this, resource, ++index);
                    cfsRfs.ParentCFS.RFSList.Add(rfs);
                }
            }
            else
            {
                foreach (var cfsRfs in ChildCFS.Values)
                {
                    var rfs = new RFS(cfsRfs, this);
                    cfsRfs.ParentCFS.RFSList.Add(rfs);
                }
            }
        }

        public override string ToString() => HostOperationName;
    }
}