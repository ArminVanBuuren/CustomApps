using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPAFilter.SPA.SC
{
    public class CFSGroups : SComponentBase
    {
        public Dictionary<string, List<string>> _tempCFSGroupCollection { get; } = new Dictionary<string, List<string>>();
        public Dictionary<string, IEnumerable<string>> CFSGroupCollection { get; private set; }

        protected internal void AddCFSGroup(string mainCFS, IEnumerable<string> listRestrictedCFS)
        {
            foreach (string cfsRestr in listRestrictedCFS)
            {
                List<string> rest = new List<string> {mainCFS, cfsRestr};
                string key = string.Join(":", rest.OrderBy(p => p));
                if (!_tempCFSGroupCollection.ContainsKey(key))
                    _tempCFSGroupCollection.Add(key, rest);
            }
        }

        public override string ToXml()
        {
            StringBuilder cfsGroupsStr = new StringBuilder();
            CFSGroupCollection = Calculate(_tempCFSGroupCollection);

            foreach (KeyValuePair<string, IEnumerable<string>> cfsGroup in CFSGroupCollection)
            {
                cfsGroupsStr.Append(GetCFSGroupString(cfsGroup.Key, cfsGroup.Value));
            }

            return cfsGroupsStr.ToString();
        }

        static string GetCFSGroupString(string name, IEnumerable<string> cfsList)
        {
            StringBuilder cfsGroupStr = new StringBuilder();
            string header = $"<CFSGroup name=\"{name}\" type=\"Mutex\" description=\"Группа взаимоисключающих услуг\">";
            
            foreach (string cfsName in cfsList)
            {
                cfsGroupStr.Append($"<CFS name=\"{cfsName}\" />");
            }

            return header + cfsGroupStr.ToString() + "</CFSGroup>";
        }


        Dictionary<string, IEnumerable<string>> Calculate(Dictionary<string, List<string>> allList)
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

            Dictionary<string, IEnumerable<string>> filtered3 = new Dictionary<string, IEnumerable<string>>();
            foreach (KeyValuePair<string, List<string>> filt1 in filtered2)
            {
                foreach (KeyValuePair<string, List<string>> filt2 in filtered2)
                {
                    if (filt1.Key != filt2.Key)
                    {
                        IOrderedEnumerable<string> intersectCFS = filt1.Value.Intersect(filt2.Value).OrderBy(p => p);
                        string getKey = string.Join(":", intersectCFS);
                        if (intersectCFS.Count() == 1 && !filtered3.ContainsKey(filt1.Key))
                        {
                            filtered3.Add(filt1.Key, (IEnumerable<string>)filt1.Value);
                        }
                        else if (!filtered3.ContainsKey(getKey) && intersectCFS.Count() > 1)
                        {
                            filtered3.Add(getKey, intersectCFS);
                        }
                    }
                }
            }

            int i = 0;
            filtered3 = filtered3.OrderByDescending(x => x.Key.Length).ToDictionary(x => x.Key, x => x.Value);
            Dictionary<string, IEnumerable<string>> filtered4 = new Dictionary<string, IEnumerable<string>>();
            foreach (KeyValuePair<string, IEnumerable<string>> dd in filtered3)
            {
                bool isExist = false;
                foreach (IEnumerable<string> cfss in filtered4.Values)
                {
                    int count = cfss.Intersect(dd.Value.ToList()).Count();
                    if (count == dd.Value.Count())
                    {
                        isExist = true;
                    }
                }

                if (!isExist)
                    filtered4.Add("CFS_GROUP_" + ++i, dd.Value);
            }

            return filtered4;
        }

        static List<string> Calculate(string name, Dictionary<string, List<string>> allList)
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
