using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessFilter.SPA.SC
{
    public class RFS
    {
        public string Name => $"RFS_{HostType}_{HostOperation.OperationName}";

        public CFS ParentCFS { get; }
        public string HostType { get; }
        HostOperation HostOperation { get; }
        public RFSGroup RFSGroupDependence { get; private set; }
        public Resource SC_Resource { get; }
        public RFS(CFS parentCFS, HostOperation hostOp)
        {
            ParentCFS = parentCFS;
            HostType = hostOp.HostType;
            HostOperation = hostOp;

            if (HostOperation.ChildCFS.Count > 0)
            {
                SC_Resource = new Resource(parentCFS, hostOp);
            }
        }

        public string ToXmlCFSChild(Dictionary<string, CFS> allCFSs)
        {
            string currentRFSName = Name;
            if (HostOperation.ChildCFS.Count > 0)
                currentRFSName = $"{currentRFSName}_1";

            if (HostOperation.DependenceServiceCodes.Any())
            {
                HashSet<string> RFSDeps = new HashSet<string>();
                foreach (string depSrv in HostOperation.DependenceServiceCodes)
                {
                    if (!allCFSs.TryGetValue(depSrv, out var getDepCFS))
                        continue;

                    HostOperation getHostOp;
                    if (!getDepCFS.HostOperations.TryGetValue(HostType, out getHostOp))
                        continue;

                    if (getHostOp.BaseCFS != null && getHostOp.BaseCFS.HostOperations.TryGetValue(HostType, out HostOperation hostOp))
                    {
                        RFSDeps.Add(hostOp.SC_RFS.Name + "_BASE");
                    }
                    else
                    {
                        RFSDeps.Add(getHostOp.SC_RFS.Name);
                    }
                }

                if (RFSDeps.Count > 1)
                {
                    RFSGroupDependence = new RFSGroup("All", RFSDeps);
                    return $"<RFS name=\"{currentRFSName}\" linkType=\"Add\" dependsOn=\"{RFSGroupDependence.Name}\" />";
                }
                else if (RFSDeps.Count > 0)
                {
                    return $"<RFS name=\"{currentRFSName}\" linkType=\"Add\" dependsOn=\"{RFSDeps.First()}\" />";
                }
            }

            return $"<RFS name=\"{currentRFSName}\" linkType=\"Add\" />";
        }

        public string ToXml()
        {
            //если CFS является базовым
            if (HostOperation.ChildCFS.Count > 0)
            {
                return $"<RFS name=\"{Name}_BASE\" hostType=\"{HostType}\" description=\"Базовая RFS\">" + SC_Resource.GetBaseCFSResource() + "</RFS>" +
                       $"<RFS name=\"{Name}_1\" base=\"{Name}_BASE\" hostType=\"{HostType}\" description=\"{ParentCFS.Description}\">" 
                       + SC_Resource.GetChildCFSResource(ParentCFS.ServiceCode) + "</RFS>";
            }

            //если CFS не является базовым
            if (HostOperation.BaseCFS != null && HostOperation.BaseCFS.HostOperations.TryGetValue(HostType, out var baseHostOp))
            {
                return
                    $"<RFS name=\"{Name}\" base=\"RFS_{HostType}_{baseHostOp.OperationName}_BASE\" hostType=\"{HostType}\" description=\"{ParentCFS.Description}\">" +
                    baseHostOp.SC_RFS.SC_Resource.GetChildCFSResource(ParentCFS.ServiceCode) + "</RFS>";
            }

            return $"<RFS name=\"{Name}\" hostType=\"{HostType}\" description=\"{ParentCFS.Description}\" />";
        }
    }
}