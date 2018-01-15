using System;
using System.Collections.Generic;

namespace Protas.Components.DynamicArea
{
    
    public class RegularAttribute
    {
        Dictionary<string, string> StringCollection { get; set; }
        Dictionary<string, Func<object>> FunkCollection { get; set; }
        internal delegate string AttrGetValue(string key);
        AttrGetValue _getValue;
        internal delegate void AttrSetValue(string key, object value, ProcEx type);
        AttrSetValue _setValue;
        internal static readonly string _default = "default";

        public object this[string key]
        {
            get
            {
                string inputKey = key;
                if (string.IsNullOrEmpty(key))
                    inputKey = _default;
                return _getValue.Invoke(inputKey);
            }

            set
            {
                string inputKey = key;
                if (string.IsNullOrEmpty(key))
                    inputKey = _default;
                _setValue.Invoke(inputKey, value, ProcEx.Insert);
            }
        }

        public RegularAttribute()
        {
            InitStringCollection(_default, string.Empty);
        }
        public RegularAttribute(string value)
        {
            InitStringCollection(_default, value);
        }
        public RegularAttribute(string name, string value)
        {
            InitStringCollection(name, value);
        }
        void InitStringCollection(string name, string value)
        {
            StringCollection = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            StringCollection.Add(name, value);
            _getValue = GetString;
            _setValue = SetString;
        }
        public RegularAttribute(Func<object> functionValue)
        {
            InitFunkStringCollection(_default, functionValue);
        }
        public RegularAttribute(string name, Func<object> functionValue)
        {
            InitFunkStringCollection(name, functionValue);
        }
        void InitFunkStringCollection(string name, Func<object> functionValue)
        {
            FunkCollection = new Dictionary<string, Func<object>>(StringComparer.OrdinalIgnoreCase);
            FunkCollection.Add(name, functionValue);
            _getValue = GetFunkString;
            _setValue = SetFunkString;
        }
        string GetString(string key)
        {
            return StringCollection[key];
        }
        string GetFunkString(string key)
        {
            return FunkCollection[key].Invoke().ToString();
        }
        void SetString(string key, object value, ProcEx type)
        {
            switch (type)
            {
                case ProcEx.Insert:
                    StringCollection[key] = (string)value;
                    break;
                case ProcEx.Add:
                    StringCollection.Add(key, (string)value);
                    break;
                default: FunkCollection.Remove(key); break;
            }
        }
        void SetFunkString(string key, object funk, ProcEx type)
        {
            switch(type)
            {
                case ProcEx.Insert:
                    {
                        Func<object> newFunk = funk as Func<string>;
                        if (newFunk == null)
                            return;
                        FunkCollection[key] = newFunk;
                    }
                    break;
                case ProcEx.Add:
                    {
                        Func<object> newFunk = funk as Func<string>;
                        if (newFunk == null)
                            return;
                        FunkCollection.Add(key, newFunk);
                    }
                    break;
                default: FunkCollection.Remove(key); break;
            }
        }
        public void Add(string key, object value)
        {
            string inputKey = key;
            if (string.IsNullOrEmpty(key))
                inputKey = _default;
            _setValue.Invoke(inputKey, value, ProcEx.Add);
        }
        public void Remove(string key)
        {
            string inputKey = key;
            if (string.IsNullOrEmpty(key))
                inputKey = _default;
            _setValue.Invoke(inputKey, null, ProcEx.Remove);
        }

        internal enum ProcEx
        {
            Insert = 0,
            Add = 1,
            Remove = 2
        }
    }
}
