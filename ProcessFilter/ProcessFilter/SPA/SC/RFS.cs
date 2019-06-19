using System.Collections.Generic;
using System.Linq;

namespace SPAFilter.SPA.SC
{
    public class RFS : SComponentBase
    {
        private readonly int _index;
        public override string Name
        {
            get
            {
                if (_index > 0)
                    return $"RFS_{MainHostOperation.HostOperationName}_{(_index <= 9 ? "0" + _index : _index.ToString())}";
                return $"RFS_{MainHostOperation.HostOperationName}";
            }
        }

        public override string Description => ParentCFS_RFS.ParentCFS.Description;

        public string HostType => MainHostOperation.HostType;
        internal HostOperation.CFS_RFS ParentCFS_RFS { get; }
        HostOperation MainHostOperation { get; }
        public Resource SC_Resource { get; }
        internal RFS(HostOperation.CFS_RFS parentCFS, HostOperation hostOp, Resource resource = null, int index = -1)
        {
            ParentCFS_RFS = parentCFS;
            MainHostOperation = hostOp;
            SC_Resource = resource;
            _index = index;
        }

        public string ToXmlCFSChild(Dictionary<string, CFS> allCFSs, Dictionary<string, RFSGroup> allRfsGroup)
        {
            if (MainHostOperation.BindServices.DependenceServices.Count > 0)
            {
                var RFSDeps = new Dictionary<HostOperation, HashSet<string>>();
                foreach (string depSrv in MainHostOperation.BindServices.DependenceServices)
                {
                    if (!allCFSs.TryGetValue(depSrv, out CFS getDepCFS))
                        continue;

                    RFS getExistRFS = getDepCFS.RFSList.FirstOrDefault(p => p.HostType == HostType);
                    if (getExistRFS == null)
                        continue;

                    if (RFSDeps.TryGetValue(getExistRFS.MainHostOperation, out HashSet<string> srvsOp))
                    {
                        srvsOp.Add(getExistRFS.Name);
                    }
                    else
                    {
                        RFSDeps.Add(getExistRFS.MainHostOperation, new HashSet<string>() { getExistRFS.Name });
                    }
                }


                if (RFSDeps.Count > 0)
                {
                    var RFSDepsFiltered = new HashSet<string>();
                    foreach (KeyValuePair<HostOperation, HashSet<string>> finded in RFSDeps)
                    {
                        AllServiceIsBaseRFS(finded, out HashSet<string> res);
                        foreach (string depRFS in res)
                        {
                            RFSDepsFiltered.Add(depRFS);
                        }
                    }

                    if (RFSDepsFiltered.Count == 1)
                    {
                        return $"<RFS name=\"{Name}\" linkType=\"{ParentCFS_RFS.Link}\" dependsOn=\"{RFSDepsFiltered.First()}\" />";
                    }
                    else
                    {
                        RFSGroup newRFSGroup = new RFSGroup(MainHostOperation.BindServices.DependenceType, RFSDepsFiltered);
                        string rfsGroupID = newRFSGroup.ToString();

                        if (allRfsGroup.TryGetValue(rfsGroupID, out RFSGroup ifExist))
                        {
                            return $"<RFS name=\"{Name}\" linkType=\"{ParentCFS_RFS.Link}\" dependsOn=\"{ifExist.Name}\" />";
                        }

                        allRfsGroup.Add(rfsGroupID, newRFSGroup);
                        return $"<RFS name=\"{Name}\" linkType=\"{ParentCFS_RFS.Link}\" dependsOn=\"{newRFSGroup.Name}\" />";
                    }
                }
            }

            return $"<RFS name=\"{Name}\" linkType=\"{ParentCFS_RFS.Link}\" />";
        }

        static bool AllServiceIsBaseRFS(KeyValuePair<HostOperation, HashSet<string>> finded, out HashSet<string> result)
        {
            if (finded.Key.ChildCFS.Count > 1 && finded.Key.ChildCFS.Count == finded.Value.Count)
            {
                result = new HashSet<string>
                {
                    $"RFS_{finded.Key.HostOperationName}_BASE"
                };
                return true;
            }

            result = finded.Value;
            return false;
        }

        public override string ToXml()
        {
            //если в хосте операции несколько RFS
            if (_index > 0)
            {
                string currentRFS = $"<RFS name=\"{Name}\" base=\"RFS_{MainHostOperation.HostOperationName}_BASE\" hostType=\"{HostType}\" description=\"{Description}\">" + SC_Resource.GetChildCFSResource(ParentCFS_RFS.ParentCFS.Name) + "</RFS>";

                if (_index == 1)
                {
                    return $"<RFS name=\"RFS_{MainHostOperation.HostOperationName}_BASE\" hostType=\"{HostType}\" description=\"Базовая RFS\">" + SC_Resource.GetBaseCFSResource() + "</RFS>" + currentRFS;
                }
                else
                {
                    return currentRFS;
                }
            }

            return $"<RFS name=\"{Name}\" hostType=\"{HostType}\" description=\"{Description}\" />";
        }
    }
}