using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessFilter.SPA.SC
{
    public class CFSGroups
    {
        public Dictionary<string, List<string>> _tempCFSGroupCollection { get; } = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> CFSGroupCollection { get; private set; }

        public void AddCFSGroup(string mainCFS, IEnumerable<string> listRestrictedCFS)
        {
            foreach (string cfsRestr in listRestrictedCFS)
            {
                List<string> rest = new List<string> {mainCFS, cfsRestr};
                string key = string.Join(":", rest.OrderBy(p => p));
                if (!_tempCFSGroupCollection.ContainsKey(key))
                    _tempCFSGroupCollection.Add(key, rest);
            }
        }

        public string ToXml()
        {
            StringBuilder cfsGroupsStr = new StringBuilder();
            CFSGroupCollection = Calculate(_tempCFSGroupCollection);

            foreach (KeyValuePair<string, List<string>> cfsGroup in CFSGroupCollection)
            {
                cfsGroupsStr.Append(GetCFSGroupString(cfsGroup.Key, cfsGroup.Value));
            }

            return cfsGroupsStr.ToString();
        }

        string GetCFSGroupString(string name, List<string> cfsList)
        {
            StringBuilder cfsGroupStr = new StringBuilder();
            string header = $"<CFSGroup name=\"{name}\" type=\"Mutex\" description=\"Группа взаимоисключающих услуг\">";
            
            foreach (string cfsName in cfsList)
            {
                cfsGroupStr.Append($"<CFS name=\"{cfsName}\" />");
            }

            return header + cfsGroupStr.ToString() + "</CFSGroup>";
        }


        Dictionary<string, List<string>> Calculate(Dictionary<string, List<string>> allList)
        {
            Dictionary<string, Dictionary<string, bool>> filtered = new Dictionary<string, Dictionary<string, bool>>();
            foreach (List<string> cfsRestrList in allList.Values)
            {
                foreach (string cfsName in cfsRestrList)
                {
                    if (filtered.ContainsKey(cfsName))
                        continue;

                    List<string> res = Calculate(cfsName, allList);
                    Dictionary<string, bool> aa = new Dictionary<string, bool>();
                    foreach (string cfs in res)
                    {
                        aa.Add(cfs, true);
                    }
                    filtered.Add(cfsName, aa);
                }
            }

            Dictionary<string, List<string>> filtered2 = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, Dictionary<string, bool>> filt1 in filtered)
            {
                foreach (KeyValuePair<string, Dictionary<string, bool>> filt2 in filtered)
                {
                    if (filt2.Value.ContainsKey(filt1.Key) && filt1.Key != filt2.Key)
                    {
                        List<string> intersectCFS = filt1.Value.Keys.ToList().Intersect(filt2.Value.Keys.ToList()).OrderBy(p => p).ToList();
                        string getKey = string.Join(":", intersectCFS);
                        if (!filtered2.ContainsKey(getKey))
                        {
                            filtered2.Add(getKey, intersectCFS);
                        }
                    }
                }
            }

            Dictionary<string, List<string>> filtered3 = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, List<string>> filt1 in filtered2)
            {
                foreach (KeyValuePair<string, List<string>> filt2 in filtered2)
                {
                    if (filt1.Key != filt2.Key)
                    {
                        List<string> intersectCFS = filt1.Value.ToList().Intersect(filt2.Value.ToList()).OrderBy(p => p).ToList();
                        string getKey = string.Join(":", intersectCFS);
                        if (intersectCFS.Count == 1 && !filtered3.ContainsKey(filt1.Key))
                        {
                            filtered3.Add(filt1.Key, filt1.Value);
                        }
                        else if (!filtered3.ContainsKey(getKey) && intersectCFS.Count > 1)
                        {
                            filtered3.Add(getKey, intersectCFS);
                        }
                    }
                }
            }

            int i = 0;
            filtered3 = filtered3.OrderByDescending(x => x.Key.Length).ToDictionary(x => x.Key, x => x.Value);
            Dictionary<string, List<string>> filtered4 = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, List<string>> dd in filtered3)
            {
                bool isExist = false;
                foreach (List<string> cfss in filtered4.Values)
                {
                    int count = cfss.Intersect(dd.Value.ToList()).Count();
                    if (count == dd.Value.Count)
                    {
                        isExist = true;
                    }
                }

                if (!isExist)
                    filtered4.Add("CFS_GROUP_" + ++i, dd.Value);
            }

            return filtered4;
        }

        List<string> Calculate(string name, Dictionary<string, List<string>> allList)
        {
            List<string> refreshList = new List<string>();
            foreach (List<string> cfsRestrList in allList.Values)
            {
                foreach (string cfsName in cfsRestrList)
                {
                    if (cfsName == name)
                    {
                        refreshList.AddRange(cfsRestrList);
                        break;
                    }
                }
            }

            return refreshList.Distinct().ToList();
        }
    }
}
