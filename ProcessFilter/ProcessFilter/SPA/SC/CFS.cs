using System;
using System.CodeDom.Compiler;
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
        internal Dictionary<string, List<HostOperation>> HostOperations = new Dictionary<string, List<HostOperation>>();
        public List<RFS> RFSList { get; } = new List<RFS>();

        public CFS(string srvCode, string description, [NotNull] HostOperation hostOp)
        {
            ServiceCode = srvCode;
            Description = description;
            IsNewHost(hostOp);
        }

        internal bool IsNewHost([NotNull] HostOperation hostOp)
        {
            if (HostOperations.TryGetValue(hostOp.HostType, out List<HostOperation> hostOpList))
            {
                hostOpList.Add(hostOp);
                return false;
            }

            HostOperations.Add(hostOp.HostType, new List<HostOperation> { hostOp });
            return true;
        }

        public string ToXml(CatalogComponents allCompontens)
        {
            string xmlStrStart = $"<CFS name=\"{ServiceCode}\" description=\"{Description}\">";
            string xmlStrMiddle = string.Empty;

            foreach (RFS rfs in RFSList)
            {
                xmlStrMiddle += rfs.ToXmlCFSChild(allCompontens.CollectionCFS, allCompontens.CollectionRFSGroup);
            }


            List<string> servicesRestrictionInAllHosts = new List<string>();
            foreach (List<HostOperation> aaa in HostOperations.Values)
            {
                foreach (HostOperation bbb in aaa)
                {
                    foreach (string srv in bbb.BindServices.RestrictedServices)
                    {
                        servicesRestrictionInAllHosts.Add(srv);
                    }
                }
            }

            allCompontens.CollectionMutexCFSGroup.AddCFSGroup(ServiceCode, servicesRestrictionInAllHosts);

            return xmlStrStart + xmlStrMiddle + "</CFS>";
        }

        public override string ToString()
        {
            return ServiceCode;
        }
    }
}
