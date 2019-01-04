using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessFilter.SPA.SC
{
    public class RFS
    {
        public string Name => $"RFS_{HostType}_{MainHostOperation.OperationName}";
        public CFS ParentCFS { get; }
        public string HostType => MainHostOperation.HostType;
        HostOperation MainHostOperation { get; }
        public Resource SC_Resource { get; }
        public RFS(CFS parentCFS, HostOperation hostOp)
        {
            ParentCFS = parentCFS;
            MainHostOperation = hostOp;

            if (MainHostOperation.ChildCFS.Count > 0)
            {
                SC_Resource = new Resource(parentCFS, hostOp);
            }
        }

        public string ToXmlCFSChild(Dictionary<string, CFS> allCFSs, Dictionary<string, RFSGroup> allRfsGroup)
        {
            string currentRFSName = Name;
            if (MainHostOperation.ChildCFS.Count > 0)
                currentRFSName = $"{currentRFSName}_01";

            if (MainHostOperation.BindServices.DependenceServices.Count > 0)
            {
                HashSet<string> RFSDeps = new HashSet<string>();
                foreach (string depSrv in MainHostOperation.BindServices.DependenceServices)
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
                        if (getHostOp.ChildCFS.Count > 0)
                        {
                            RFSDeps.Add(getHostOp.SC_RFS.Name + "_BASE");
                            continue;
                        }
                        RFSDeps.Add(getHostOp.SC_RFS.Name);
                    }
                }

                if (RFSDeps.Count == 1)
                {
                    return $"<RFS name=\"{currentRFSName}\" linkType=\"Add\" dependsOn=\"{RFSDeps.First()}\" />";
                }
                else if (RFSDeps.Count > 1)
                {
                    RFSGroup newRFSGroup = new RFSGroup(MainHostOperation.BindServices.DependenceType, RFSDeps);
                    string rfsGroupID = newRFSGroup.ToString();
                    if (allRfsGroup.TryGetValue(rfsGroupID, out var ifExist))
                    {
                        return $"<RFS name=\"{currentRFSName}\" linkType=\"Add\" dependsOn=\"{ifExist.Name}\" />";
                    }

                    allRfsGroup.Add(rfsGroupID, newRFSGroup);
                    return $"<RFS name=\"{currentRFSName}\" linkType=\"Add\" dependsOn=\"{newRFSGroup.Name}\" />";
                }
            }

            return $"<RFS name=\"{currentRFSName}\" linkType=\"Add\" />";
        }

        public string ToXml()
        {
            //если CFS является базовым
            if (MainHostOperation.ChildCFS.Count > 0)
            {
                return $"<RFS name=\"{Name}_BASE\" hostType=\"{HostType}\" description=\"Базовая RFS\">" + SC_Resource.GetBaseCFSResource() + "</RFS>" +
                       $"<RFS name=\"{Name}_01\" base=\"{Name}_BASE\" hostType=\"{HostType}\" description=\"{ParentCFS.Description}\">" 
                       + SC_Resource.GetChildCFSResource(ParentCFS.ServiceCode) + "</RFS>";
            }

            //если CFS не является базовым
            if (MainHostOperation.BaseCFS != null && MainHostOperation.BaseCFS.HostOperations.TryGetValue(HostType, out var baseHostOp))
            {
                return
                    $"<RFS name=\"{Name}\" base=\"RFS_{HostType}_{baseHostOp.OperationName}_BASE\" hostType=\"{HostType}\" description=\"{ParentCFS.Description}\">" +
                    baseHostOp.SC_RFS.SC_Resource.GetChildCFSResource(ParentCFS.ServiceCode) + "</RFS>";
            }

            return $"<RFS name=\"{Name}\" hostType=\"{HostType}\" description=\"{ParentCFS.Description}\" />";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}