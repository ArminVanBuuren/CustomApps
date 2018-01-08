using System;
using System.Collections.Generic;
using System.Linq;

namespace Script.Control.Handlers.Timesheet.Stats
{
    public class Statistic
    {
        /// <summary>
        /// Если не был указан аттрибут GroupBy, то все помещается в форму ALL или чтобы сгруппировать остальные проекты с таксками необходимо в GroupBY указать параметр GroupBy="Остальное[]"
        /// </summary>
        private string OTHER_STAT { get; }
        private Dictionary<string, string[]> parentGroups;
        public List<Statistic> ChildItems { get; private set; }
        public string Name { get; }
        public virtual DateTime PeriodStart => ChildItems.Min(m => m.PeriodStart);
        public virtual DateTime PeriodEnd => ChildItems.Max(m => m.PeriodEnd);
        public virtual double TotalTimeByAnyDay => ChildItems.Sum(x => x.TotalTimeByAnyDay);
        public Statistic(string name)
        {
            Name = name;
            ChildItems = new List<Statistic>();
        }
        public Statistic(string name, string[] groupBy)
        {
            Name = name;
            ChildItems = new List<Statistic>();
            parentGroups = new Dictionary<string, string[]>();

            if (groupBy == null || groupBy.Length == 0)
            {
                OTHER_STAT = "All";
                return;
            }

            foreach (string item in groupBy)
            {
                string groupName = item.Substring(0, item.IndexOf('['));
                string projects = item.Substring(item.IndexOf('[') + 1, item.IndexOf(']') - item.IndexOf('[') - 1);
                string[] projects_collection = projects.Split(',');
                if (!string.IsNullOrEmpty(projects.Trim()))
                    parentGroups.Add(groupName, projects_collection);
                else
                {
                    OTHER_STAT = groupName;
                    parentGroups.Add(OTHER_STAT, new string[] { });
                }
            }
        }
        public void Add(Statistic item)
        {
            if (parentGroups != null)
            {
                Statistic getGroupStat;
                foreach (KeyValuePair<string, string[]> stats in parentGroups)
                {
                    string[] strs = stats.Value.Where(projectName => item.Name.StartsWith(projectName, StringComparison.CurrentCultureIgnoreCase)).ToArray();
                    if (strs.Length > 0)
                    {
                        if (!TryGetValue(stats.Key, out getGroupStat))
                            getGroupStat = CreateChildItem(stats.Key);

                        getGroupStat.Add(item);
                        return;
                    }
                }
                if (!string.IsNullOrEmpty(OTHER_STAT))
                {
                    if (!TryGetValue(OTHER_STAT, out getGroupStat))
                        getGroupStat = CreateChildItem(OTHER_STAT);

                    getGroupStat.Add(item);
                }
                return;
            }
            ChildItems.Add(item);
        }

        public Statistic CreateChildItem(string groupName)
        {
            Statistic getGroupStat = new Statistic(groupName);
            ChildItems.Add(getGroupStat);
            return getGroupStat;
        }

        public void AddChild(Statistic stat)
        {
            ChildItems.Add(stat);
        }
        public bool TryGetValue(string name, out Statistic stat)
        {
            stat = null;
            foreach (Statistic st in ChildItems)
            {
                if (st.Name != null && st.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    stat = st;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Сортировка регионов по параметру GroupBy
        /// </summary>
        public void OrderByGroups()
        {
            if (parentGroups != null && parentGroups.Count > 1)
            {
		        //Collection = Collection.OrderBy(x => parentGroups.Keys.ToList().FindIndex(a => a == x.Name)).ToList();
                ChildItems = ChildItems.OrderBy(x => parentGroups.Keys.ToList().IndexOf(x.Name)).ToList();
            }
        }
        public override string ToString()
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(Name))
                result = string.Format("[{0}];", Name);
            if (ChildItems.Count > 0)
                result = result + string.Format("[{0} Items];", ChildItems.Count);
            return result + string.Format("[{0} Hours]; [{1:dd.MM.yyyy} - {2:dd.MM.yyyy}];", TotalTimeByAnyDay, PeriodStart, PeriodEnd);
        }

        public virtual string GetStat()
        {
            return string.Format("Период: {0:dd.MM.yyyy} - {1:dd.MM.yyyy}{3}Затрачено часов: {2}", PeriodStart, PeriodEnd, TotalTimeByAnyDay, Environment.NewLine);
        }

        public int Count => ChildItems?.Count ?? -1;
        public IEnumerator<Statistic> GetEnumerator() => ChildItems?.GetEnumerator();
    }
}
