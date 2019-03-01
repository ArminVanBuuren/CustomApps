using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Utils.CollectionHelper
{
    [Serializable]
    public class DuplicateDictionary<TKey, TValue> : IDictionary<TKey, List<TValue>>, IDictionary, ISerializable, IDisposable
    {
        Dictionary<TKey, List<TValue>> _values;

        public DuplicateDictionary(IEqualityComparer<TKey> comparer = null)
        {
            if (comparer != null)
                _values = new Dictionary<TKey, List<TValue>>(comparer);
            else
                _values = new Dictionary<TKey, List<TValue>>();
        }

        public List<TValue> this[TKey index]
        {
            get
            {
                if (_values.TryGetValue(index, out List<TValue> tValue))
                {
                    return tValue;
                }

                return null;
            }
            set
            {
                if (_values.TryGetValue(index, out List<TValue> tValue))
                {
                    tValue = value;
                }
                else
                {
                    _values.Add(index, value);
                }
            }
        }

        public object this[object key]
        {
            get => ((IDictionary)_values)[key];
            set => ((IDictionary)_values)[key] = value; 
        }

        List<TValue> IDictionary<TKey, List<TValue>>.this[TKey key]
        {
            get => ((IDictionary<TKey, List<TValue>>) _values)[key];
            set => ((IDictionary<TKey, List<TValue>>) _values)[key] = value;
        }

        public ICollection<TKey> Keys => ((IDictionary<TKey, List<TValue>>)_values).Keys;

        public ICollection<List<TValue>> Values => ((IDictionary<TKey, List<TValue>>) _values).Values;

        public int Count => _values.Count;

        public bool IsReadOnly => ((IDictionary<TKey, List<TValue>>)_values).IsReadOnly;

        public bool IsFixedSize => ((IDictionary)_values).IsFixedSize;

        public object SyncRoot => ((IDictionary)_values).SyncRoot;

        public bool IsSynchronized => ((IDictionary)_values).IsSynchronized;

        ICollection IDictionary.Keys => ((IDictionary)_values).Keys;

        ICollection IDictionary.Values => ((IDictionary)_values).Values;
        

        public void Add(TKey key, List<TValue> value)
        {
            if (_values.TryGetValue(key, out List<TValue> tValue))
            {
                tValue.AddRange(value);
            }
            else
            {
                _values.Add(key, value);
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (_values.TryGetValue(key, out List<TValue> tValue))
            {
                tValue.Add(value);
            }
            else
            {
                _values.Add(key, new List<TValue> { value });
            }
        }

        public void Add(KeyValuePair<TKey, List<TValue>> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(object key, object value)
        {
            if (key is TKey && value is TValue)
            {
                Add((TKey)key, (TValue)value);
            }
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool Contains(KeyValuePair<TKey, List<TValue>> item)
        {
            return ((IDictionary)_values).Contains(item);
        }

        public bool Contains(object key)
        {
            return ((IDictionary)_values).Contains(key);
        }

        public bool ContainsKey(TKey key)
        {
            return ((IDictionary<TKey, List<TValue>>)_values).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, List<TValue>>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, List<TValue>>)_values).CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            ((IDictionary)_values).CopyTo(array, index);
        }

        public void Dispose()
        {
            ((IDisposable)_values).Dispose();
        }

        public IEnumerator<KeyValuePair<TKey, List<TValue>>> GetEnumerator()
        {
            return ((IDictionary<TKey, List<TValue>>)_values).GetEnumerator();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ((ISerializable)_values).GetObjectData(info, context);
        }

        public bool Remove(TKey key)
        {
            return ((IDictionary<TKey, List<TValue>>)_values).Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, List<TValue>> item)
        {
            return ((IDictionary<TKey, List<TValue>>)_values).Remove(item);
        }

        public void Remove(object key)
        {
            ((IDictionary)_values).Remove(key);
        }

        public bool TryGetValue(TKey key, out List<TValue> value)
        {
            return ((IDictionary<TKey, List<TValue>>)_values).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary)_values).GetEnumerator();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)_values).GetEnumerator();
        }
    }
}
