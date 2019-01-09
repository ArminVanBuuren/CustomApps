using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessFilter.SPA.SC
{
    public class HostOperation
    {
        [Flags]
        protected internal enum LinkType
        {
            Add = 0,
            Remove = 1
        }
        protected internal class CFS_RFS
        {
            private HostOperation _parent;
            protected internal CFS_RFS(CFS cfs, LinkType link, HostOperation parent)
            {
                ParentCFS = cfs;
                Link = link;
                _parent = parent;
            }

            protected internal void ChangeLinkType(LinkType link)
            {
                Link = Link & link;
            }

            protected internal CFS ParentCFS { get; }
            protected internal LinkType Link { get; private set; }
            public override string ToString()
            {
                return $"{ParentCFS.Name}:{Link:G}";
            }
        }


        public string HostType { get; }
        public string HostOperationName => $"{HostType}_{OperationName}";
        public string OperationName { get; }
        protected internal List<string> XML_BODY { get; } = new List<string>();
        protected internal Dictionary<string, CFS_RFS> ChildCFS { get; } = new Dictionary<string, CFS_RFS>();
        protected internal BindingServices BindServices { get; }

        public HostOperation(string opName, string hostType, string xmlBody, BindingServices bindServ)
        {
            OperationName = opName;
            HostType = hostType;
            XML_BODY.Add(xmlBody);
            BindServices = bindServ;
        }

        protected internal void CombineSameHostOperation(HostOperation hostOp)
        {
            XML_BODY.AddRange(hostOp.XML_BODY);
            BindServices.AddRange(hostOp.BindServices);
        }

        protected internal void AddChildRFS(CFS cfs, LinkType linkType)
        {
            CFS_RFS cfsrfs = new CFS_RFS(cfs, linkType, this);
            ChildCFS.Add(cfs.Name, cfsrfs);
        }

        public void GenerateRFS()
        {
            if (ChildCFS.Count > 1)
            {
                Resource resource = new Resource(this);
                int index = 0;
                foreach (CFS_RFS cfsRfs in ChildCFS.Values)
                {
                    RFS rfs = new RFS(cfsRfs, this, resource, ++index);
                    cfsRfs.ParentCFS.RFSList.Add(rfs);
                }
            }
            else
            {
                foreach (CFS_RFS cfsRfs in ChildCFS.Values)
                {
                    RFS rfs = new RFS(cfsRfs, this);
                    cfsRfs.ParentCFS.RFSList.Add(rfs);
                }
            }
        }

        public override string ToString()
        {
            return HostOperationName;
        }
    }
}