using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessFilter.SPA.SC
{
    public class RFS
    {
        private int _index;
        public string Name
        {
            get
            {
                if (_index > 0)
                    return $"RFS_{MainHostOperation.ToString()}_{(_index <= 9 ? "0" + _index : _index.ToString())}";
                return $"RFS_{MainHostOperation.ToString()}";
            }
        }

        public string HostType => MainHostOperation.HostType;
        public CFS ParentCFS { get; }
        HostOperation MainHostOperation { get; }
        public Resource SC_Resource { get; }
        public RFS(CFS parentCFS, HostOperation hostOp, Resource resource = null, int index = -1)
        {
            ParentCFS = parentCFS;
            MainHostOperation = hostOp;
            SC_Resource = resource;
            _index = index;
        }

        public string ToXmlCFSChild(Dictionary<string, CFS> allCFSs, Dictionary<string, RFSGroup> allRfsGroup)
        {
            if (MainHostOperation.BindServices.DependenceServices.Count > 0)
            {
                Dictionary<HostOperation, HashSet<string>> RFSDeps = new Dictionary<HostOperation, HashSet<string>>();
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
                    HashSet<string> RFSDepsFiltered = new HashSet<string>();
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
                        return $"<RFS name=\"{Name}\" linkType=\"Add\" dependsOn=\"{RFSDepsFiltered.First()}\" />";
                    }
                    else
                    {
                        RFSGroup newRFSGroup = new RFSGroup(MainHostOperation.BindServices.DependenceType, RFSDepsFiltered);
                        string rfsGroupID = newRFSGroup.ToString();

                        if (newRFSGroup.Name.IndexOf("RFS_GROUP_RM_BIS_02", StringComparison.Ordinal) != -1)
                        {

                        }

                        if (allRfsGroup.TryGetValue(rfsGroupID, out RFSGroup ifExist))
                        {
                            return $"<RFS name=\"{Name}\" linkType=\"Add\" dependsOn=\"{ifExist.Name}\" />";
                        }

                        allRfsGroup.Add(rfsGroupID, newRFSGroup);
                        return $"<RFS name=\"{Name}\" linkType=\"Add\" dependsOn=\"{newRFSGroup.Name}\" />";
                    }
                }
            }

            return $"<RFS name=\"{Name}\" linkType=\"Add\" />";
        }

        bool AllServiceIsBaseRFS(KeyValuePair<HostOperation, HashSet<string>> finded, out HashSet<string> result)
        {
            if (finded.Key.ChildCFS.Count > 1 && finded.Key.ChildCFS.Count == finded.Value.Count)
            {
                result = new HashSet<string>
                {
                    $"RFS_{finded.Key.ToString()}_BASE"
                };
                return true;
            }

            result = finded.Value;
            return false;
        }

        RFSGroup CreateRFSGroup(HashSet<string> RFSDeps)
        {
            RFSGroup newRFSGroup = new RFSGroup(MainHostOperation.BindServices.DependenceType, RFSDeps);
            string rfsGroupID = newRFSGroup.ToString();
            return newRFSGroup;
        }

        public string ToXml()
        {
            //если в хосте операции несколько RFS
            if (_index > 0)
            {
                string currentRFS = $"<RFS name=\"{Name}\" base=\"RFS_{MainHostOperation.ToString()}_BASE\" hostType=\"{HostType}\" description=\"{ParentCFS.Description}\">" + SC_Resource.GetChildCFSResource(ParentCFS.ServiceCode) + "</RFS>";

                if (_index == 1)
                {
                    return $"<RFS name=\"RFS_{MainHostOperation.ToString()}_BASE\" hostType=\"{HostType}\" description=\"Базовая RFS\">" + SC_Resource.GetBaseCFSResource() + "</RFS>" + currentRFS;
                }
                else
                {
                    return currentRFS;
                }
            }

            return $"<RFS name=\"{Name}\" hostType=\"{HostType}\" description=\"{ParentCFS.Description}\" />";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}