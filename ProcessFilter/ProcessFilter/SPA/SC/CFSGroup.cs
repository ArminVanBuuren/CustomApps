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
            foreach (var cfsRestr in listRestrictedCFS)
            {
                var rest = new List<string> {mainCFS, cfsRestr};
                var key = string.Join(":", rest.OrderBy(p => p));
                if (!_tempCFSGroupCollection.ContainsKey(key))
                    _tempCFSGroupCollection.Add(key, rest);
            }
        }

        public override string ToXml()
        {
            var cfsGroupsStr = new StringBuilder();
            CFSGroupCollection = Calculate(_tempCFSGroupCollection);

            foreach (var cfsGroup in CFSGroupCollection)
            {
                cfsGroupsStr.Append(GetCFSGroupString(cfsGroup.Key, cfsGroup.Value));
            }

            return cfsGroupsStr.ToString();
        }

        static string GetCFSGroupString(string name, IEnumerable<string> cfsList)
        {
            var cfsGroupStr = new StringBuilder();
            var header = $"<CFSGroup name=\"{name}\" type=\"Mutex\" description=\"Группа взаимоисключающих услуг\">";
            
            foreach (var cfsName in cfsList)
            {
                cfsGroupStr.Append($"<CFS name=\"{cfsName}\" />");
            }

            return header + cfsGroupStr + "</CFSGroup>";
        }


        Dictionary<string, IEnumerable<string>> Calculate(Dictionary<string, List<string>> allList)
        {
            var filtered = new Dictionary<string, Dictionary<string, bool>>();
            foreach (var cfsRestrList in allList.Values)
            {
                foreach (var cfsName in cfsRestrList)
                {
                    if (filtered.ContainsKey(cfsName))
                        continue;

                    var res = Calculate(cfsName, allList);
                    var aa = new Dictionary<string, bool>();
                    foreach (var cfs in res)
                    {
                        aa.Add(cfs, true);
                    }
                    filtered.Add(cfsName, aa);
                }
            }

            var filtered2 = new Dictionary<string, List<string>>();
            foreach (var filt1 in filtered)
            {
                foreach (var filt2 in filtered)
                {
                    if (filt2.Value.ContainsKey(filt1.Key) && filt1.Key != filt2.Key)
                    {
                        var intersectCFS = filt1.Value.Keys.ToList().Intersect(filt2.Value.Keys.ToList()).OrderBy(p => p).ToList();
                        var getKey = string.Join(":", intersectCFS);
                        if (!filtered2.ContainsKey(getKey))
                        {
                            filtered2.Add(getKey, intersectCFS);
                        }
                    }
                }
            }

            var filtered3 = new Dictionary<string, IEnumerable<string>>();
            foreach (var filt1 in filtered2)
            {
                foreach (var filt2 in filtered2)
                {
                    if (filt1.Key != filt2.Key)
                    {
                        var intersectCFS = filt1.Value.Intersect(filt2.Value).OrderBy(p => p);
                        var getKey = string.Join(":", intersectCFS);
                        if (intersectCFS.Count() == 1 && !filtered3.ContainsKey(filt1.Key))
                        {
                            filtered3.Add(filt1.Key, filt1.Value);
                        }
                        else if (!filtered3.ContainsKey(getKey) && intersectCFS.Count() > 1)
                        {
                            filtered3.Add(getKey, intersectCFS);
                        }
                    }
                }
            }

            var i = 0;
            filtered3 = filtered3.OrderByDescending(x => x.Key.Length).ToDictionary(x => x.Key, x => x.Value);
            var filtered4 = new Dictionary<string, IEnumerable<string>>();
            foreach (var dd in filtered3)
            {
                var isExist = false;
                foreach (var cfss in filtered4.Values)
                {
                    var count = cfss.Intersect(dd.Value.ToList()).Count();
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

        static IEnumerable<string> Calculate(string name, Dictionary<string, List<string>> allList)
        {
            var refreshList = new List<string>();
            foreach (var cfsRestrList in allList.Values)
            {
                foreach (var cfsName in cfsRestrList)
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
