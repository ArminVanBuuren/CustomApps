using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XPackage
{
    public class XPackAttribute : XPackGetValue
    {
        internal XPackAttribute(XPack currentPack, string key, string value) : base(currentPack, value)
        {
            Key = key;
        }
        public string Key { get; }

        public override string ToString()
        {
            return string.Format("{0}=\"{1}\"", Key, SourceValue);
        }
    }
    public class XPackAttributeCollection
    {
        internal List<XPackAttribute> Collection { get; }
        StringComparison Comparer { get; }
        public int Count => Collection.Count;
        private XPack Parent { get; }

        public XPackAttributeCollection(XPack currentPack, StringComparison comparer)
        {
            Parent = currentPack;
            Comparer = comparer;
            Collection = new List<XPackAttribute>();
        }
        public void Clear()
        {
            Collection.Clear();
        }

        public string this[string key]
        {
            get
            {
                XPackAttribute finded;
                if (TryGetValue(key, out finded))
                    return finded.Value;
                return null;
            }
            set
            {
                XPackAttribute getAttribute;
                if (TryGetValue(key, out getAttribute))
                    getAttribute.Value = value;
                else
                    Collection.Add(new XPackAttribute(Parent, key, value));
            }
        }
        internal void Add(string key, string value)
        {
            XPackAttribute finded;
            if (TryGetValue(key, out finded))
                throw new Exception(string.Format("XPackAttribute=[{0}] With Key=[{1}] Already Exist! Name Of Key By Comparer=[{2}] Must Be Unique", finded, finded.Key, Comparer));
            Collection.Add(new XPackAttribute(Parent, key, value));
        }
        public bool TryGetValue(string key, out XPackAttribute getterAttribute)
        {
            getterAttribute = Collection.Find(p => p.Key.Equals(key, Comparer));
            if (getterAttribute != null)
                return true;
            return false;
        }
        public List<XPackAttribute>.Enumerator GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(" ", Collection.Select(p => p.ToString()));
        }
    }
}
