using System.Collections.Generic;
using Script.Control.Handlers.SysObj.Based;

namespace Script.Control.Handlers.GetValue.Based
{
    public class InnerTextMatch : SystemObjectMatch
    {
        public List<string> Values { get; }
        public int Count => Values.Count;
        public InnerTextMatch(FindBase parent, string subPathAndSysObjName, FindType type) : base(parent, subPathAndSysObjName, type)
        {
            Values = new List<string>();
        }
        public InnerTextMatch(SystemObjectMatch parent) : base(parent)
        {
            Values = new List<string>();
        }
        public void Add(string str)
        {
            Values.Add(str);
        }
        public void AddRange(List<string> strList)
        {
            Values.AddRange(strList);
        }

    }
}
