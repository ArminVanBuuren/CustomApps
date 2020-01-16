using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XPackage
{
    [Serializable]
    public class CollectionXPack : List<XPack>,  IDisposable
    {
        public List<XPack> FindAllByName(string name)
        {
            var result = new List<XPack>();
            result.AddRange(this.Where(c => string.Equals(c.Name, name, StringComparison.CurrentCultureIgnoreCase)).ToList());
            return result;
        }
        /// <summary>
        /// Полуение коллекции XPack, по условие если из дочерних элементов определенный аттрибут key, будет равен заданному value
        /// Проврека производится в игнор регистра
        /// </summary>
        /// <param name="key">название аттрибута</param>
        /// <param name="value">значение которое должно быть в аттрибуте</param>
        /// <returns></returns>
        public List<XPack> this[string key, string value]
        {
            get
            {
                var result = new List<XPack>();
                foreach(var xpck in this)
                {
                    if (string.Equals(xpck.Attributes[key], value, StringComparison.CurrentCultureIgnoreCase))
                        result.Add(xpck);
                }
                return result;
            }
        }

        public bool IsUniqueAttributeValueByKey(string attributeName)
        {
            var lst = new List<string>();
            foreach (var xp in this)
            {
                var value = xp.Attributes[attributeName];
                if (string.IsNullOrEmpty(value))
                    return false;
                lst.Add(value);
                if (!IsUnique(lst))
                    return false;
            }
            return true;
        }

        bool IsUnique(List<string> lst)
        {
            var grouped = lst.GroupBy(s => s)
                              .Select(group => new { Word = group.Key, Count = group.Count() });
            if (grouped.Count() == lst.Count)
                return true;
            return false;
        }

        public bool IsUniqueNames
        {
            get
            {
                var grouped = this
                              .GroupBy(s => s.Name)
                              .Select(group => new { Word = group.Key, Count = group.Count() });
                if (grouped.Count() == Count)
                    return true;
                return false;
            }
        }

        public new void Add(XPack child)
        {
            if (child == null)
                throw new Exception("Child Object Is Null");
            base.Add(child);
        }

        public void AddRangeWithParentBinding(IEnumerable<XPack> collection)
        {
            foreach(var pack in collection)
            {
                Add(pack);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}", string.Join<XPack>(",", ToArray()));
        }

        public void Dispose()
        {
            foreach (var pck in this)
                pck?.Dispose();
        }
        //public string GetNotNodeValue
        //{
        //    get { return string.Join("", this.Where(u => (u.IsNode)).Select(v => v.Value)); }
        //}
    }
}
