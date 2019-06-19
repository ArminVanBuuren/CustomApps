using System.Collections.Generic;

namespace SPAFilter.SPA.SC
{
    public sealed class CFS : SComponentBase
    {
        readonly Dictionary<string, List<HostOperation>> hostOperations = new Dictionary<string, List<HostOperation>>();
        public List<RFS> RFSList { get; } = new List<RFS>();

        internal CFS(string serviceCode, string description, [NotNull] HostOperation hostOp, HostOperation.LinkType linkType)
        {
            Name = serviceCode;
            Description = description;
            IsNewHost(hostOp, linkType);
        }

        internal bool IsNewHost([NotNull] HostOperation hostOp, HostOperation.LinkType linkType)
        {
            if (hostOperations.TryGetValue(hostOp.HostType, out List<HostOperation> hostOpList))
            {
                foreach (HostOperation existHostOp in hostOpList)
                {
                    if (existHostOp == hostOp)
                        return false;
                }
                hostOpList.Add(hostOp);
                return false;
            }

            hostOp.AddChildRFS(this, linkType);
            hostOperations.Add(hostOp.HostType, new List<HostOperation> { hostOp });
            return true;
        }

        public string ToXml(CatalogComponents allCompontens)
        {
            var xmlStrStart = $"<CFS name=\"{Name}\" description=\"{Description}\">";
            var xmlStrMiddle = string.Empty;

            foreach (RFS rfs in RFSList)
            {
                xmlStrMiddle += rfs.ToXmlCFSChild(allCompontens.CollectionCFS, allCompontens.CollectionRFSGroup);
            }

            var servicesRestrictionInAllHosts = new HashSet<string>();
            foreach (List<HostOperation> aaa in hostOperations.Values)
            {
                foreach (HostOperation bbb in aaa)
                {
                    foreach (string srv in bbb.BindServices.RestrictedServices)
                    {
                        servicesRestrictionInAllHosts.Add(srv);
                    }
                }
            }

            allCompontens.CollectionMutexCFSGroup.AddCFSGroup(Name, servicesRestrictionInAllHosts);

            return xmlStrStart + xmlStrMiddle + "</CFS>";
        }
    }
}
