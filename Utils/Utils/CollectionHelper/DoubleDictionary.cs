using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Utils.CollectionHelper
{
    [Serializable]
    public class DoubleDictionary<TKey, TValue> : IDictionary<TKey, List<TValue>>, IDictionary, ISerializable, IDisposable
    {
        object _syncRoot = new object();
        readonly Dictionary<TKey, List<TValue>> _values;

        public DoubleDictionary(int capacity = 4)
        {
            _values = new Dictionary<TKey, List<TValue>>(capacity);
        }

        public DoubleDictionary(IEqualityComparer<TKey> comparer, int capacity = 4)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            _values = new Dictionary<TKey, List<TValue>>(capacity, comparer);
        }

        public DoubleDictionary(IEnumerable<TKey> keys, IEqualityComparer<TKey> comparer = null)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            _values = comparer != null ? new Dictionary<TKey, List<TValue>>(keys.Count(), comparer) : new Dictionary<TKey, List<TValue>>(keys.Count());

            foreach (var key in keys)
            {
                Add(key, default(TValue));
            }
        }

        public DoubleDictionary(IEnumerable<TKey> keys, TValue @default, IEqualityComparer<TKey> comparer = null)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));
            if (@default == null)
                throw new ArgumentNullException(nameof(@default));

            _values = comparer != null ? new Dictionary<TKey, List<TValue>>(keys.Count(), comparer) : new Dictionary<TKey, List<TValue>>(keys.Count());


            foreach (var key in keys)
            {
                Add(key, @default);
            }
        }

        /// <summary>
        /// Присваивает другую коллекцию для ключа, либо создает новую запись
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public List<TValue> this[TKey index]
        {
            get
            {
                TryGetValue(index, out var tValue);
                return tValue;
            }
            set => SetValue(index, value);
        }

        public object this[object key]
        {
            get
            {
                TryGetValue((TKey) key, out var tValue);
                return tValue;
            }
            set
            {
                switch (value)
                {
                    case TValue value1:
                        SetValue((TKey) key, value1);
                        break;
                    case List<TValue> value2:
                        SetValue((TKey) key, value2);
                        break;
                }
            }
        }

        void SetValue(TKey index, TValue value)
        {
            lock (_syncRoot)
            {
                if (_values.ContainsKey(index))
                {
                    _values[index] = new List<TValue> {value};
                }
                else
                {
                    _values.Add(index, new List<TValue> {value});
                }
            }
        }

        void SetValue(TKey index, List<TValue> value)
        {
            lock (_syncRoot)
            {
                if (_values.ContainsKey(index))
                {
                    _values[index] = value;
                }
                else
                {
                    _values.Add(index, value);
                }
            }
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                lock (_syncRoot)
                    return _values.Keys;
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                lock (_syncRoot)
                    return _values.Values.SelectMany(x => x);
            }
        }

        public int Count
        {
            get
            {
                lock (_syncRoot)
                    return _values.Count;
            }
        }

        public int CountValues
        {
            get
            {
                lock (_syncRoot)
                {
                    return  _values.Values.Aggregate(new int(), (cnt, thisCount) =>
                    {
                        cnt += thisCount.Count;
                        return cnt;
                    });
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                lock (_syncRoot)
                    return ((IDictionary<TKey, List<TValue>>) _values).IsReadOnly;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                lock (_syncRoot)
                    return ((IDictionary) _values).IsFixedSize;
            }
        }

        public object SyncRoot
        {
            get
            {
                lock (_syncRoot)
                    return ((IDictionary) _values).SyncRoot;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                lock (_syncRoot)
                    return ((IDictionary) _values).IsSynchronized;
            }
        }

        ICollection IDictionary.Keys => (ICollection) Keys;

        ICollection IDictionary.Values => Values.ToList();

        ICollection<TKey> IDictionary<TKey, List<TValue>>.Keys
        {
            get
            {
                lock (_syncRoot)
                    return _values.Keys;
            }
        }

        ICollection<List<TValue>> IDictionary<TKey, List<TValue>>.Values
        {
            get
            {
                lock (_syncRoot)
                    return _values.Values;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Добавляет значения в имеющийся ключ, любо создает новую запись
        /// </summary>
        /// <param name="item"></param>
        public void Add(KeyValuePair<TKey, List<TValue>> item)
        {
            Add(item.Key, item.Value);
        }

        /// <inheritdoc />
        /// <summary>
        /// Добавляет значение в имеющийся ключ, любо создает новую запись
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(object key, object value)
        {
            if (key is TKey tKey && value is TValue tValue)
            {
                Add(tKey, tValue);
            }
        }

        /// <summary>
        /// Добавляет значение в имеющийся ключ, любо создает новую запись
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            if (TryGetValue(key, out var tValue))
            {
                tValue.Add(value);
            }
            else
            {
                lock (_syncRoot)
                {
                    _values.Add(key, new List<TValue> {value});
                }
            }
        }

        /// <summary>
        /// Добавляет значения в имеющийся ключ, любо создает новую запись
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, List<TValue> value)
        {
            if (TryGetValue(key, out var tValue))
            {
                tValue.AddRange(value);
            }
            else
            {
                lock (_syncRoot)
                    _values.Add(key, value);
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
                _values.Clear();
        }

        public bool Contains(KeyValuePair<TKey, List<TValue>> item)
        {
            lock (_syncRoot)
                return ((IDictionary) _values).Contains(item);
        }

        public bool Contains(object key)
        {
            lock (_syncRoot)
                return ((IDictionary) _values).Contains(key);
        }

        public bool ContainsKey(TKey key)
        {
            lock (_syncRoot)
                return ((IDictionary<TKey, List<TValue>>) _values).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, List<TValue>>[] array, int arrayIndex)
        {
            lock (_syncRoot)
                ((IDictionary<TKey, List<TValue>>) _values).CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            lock (_syncRoot)
                ((IDictionary) _values).CopyTo(array, index);
        }

        public bool Remove(TKey key)
        {
            lock (_syncRoot)
                return ((IDictionary<TKey, List<TValue>>) _values).Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, List<TValue>> item)
        {
            lock (_syncRoot)
                return ((IDictionary<TKey, List<TValue>>) _values).Remove(item);
        }

        public void Remove(object key)
        {
            lock (_syncRoot)
                ((IDictionary) _values).Remove(key);
        }

        public bool TryGetValue(TKey key, out List<TValue> value)
        {
            lock (_syncRoot)
            {
                if (_values.TryGetValue(key, out var tValue))
                {
                    value = tValue;
                    return true;
                }

                value = null;
                return false;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            lock (_syncRoot)
                ((ISerializable) _values).GetObjectData(info, context);
        }

        public IEnumerator<KeyValuePair<TKey, List<TValue>>> GetEnumerator()
        {
            lock (_syncRoot)
                return ((IDictionary<TKey, List<TValue>>)_values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_syncRoot)
                return ((IDictionary) _values).GetEnumerator();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            lock (_syncRoot)
                return ((IDictionary) _values).GetEnumerator();
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (_values is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        public override string ToString()
        {
            return this.GetType().ToString();
        }
    }
}