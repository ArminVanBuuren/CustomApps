using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Utils.CollectionHelper
{
    [Serializable]
    public class DoubleDictionary<TKey, TValue> : IDictionary, ISerializable, IDisposable
    {
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
            if(keys == null)
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
            get => _values.TryGetValue((TKey)key, out var tValue) ? tValue : null;
            set
            {
                switch (value)
                {
                    case TValue value1:
                        SetValue((TKey)key, value1);
                        break;
                    case List<TValue> value2:
                        SetValue((TKey)key, value2);
                        break;
                }
            }
        }

        void SetValue(TKey index, TValue value)
        {
            if (_values.ContainsKey(index))
            {
                _values[index] = new List<TValue> { value };
            }
            else
            {
                _values.Add(index, new List<TValue> { value });
            }
        }

        void SetValue(TKey index, List<TValue> value)
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

        public IEnumerable<TKey> Keys => _values.Keys;

        public IEnumerable<TValue> Values => _values.Values.SelectMany(x => x);

        public int Count => _values.Count;

        public bool IsReadOnly => ((IDictionary<TKey, List<TValue>>)_values).IsReadOnly;

        public bool IsFixedSize => ((IDictionary)_values).IsFixedSize;

        public object SyncRoot => ((IDictionary)_values).SyncRoot;

        public bool IsSynchronized => ((IDictionary)_values).IsSynchronized;

        ICollection IDictionary.Keys => (ICollection)Keys;

        ICollection IDictionary.Values => (ICollection)Values.ToList();

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
            if (_values.TryGetValue(key, out var tValue))
            {
                tValue.Add(value);
            }
            else
            {
                _values.Add(key, new List<TValue> { value });
            }
        }

        /// <summary>
        /// Добавляет значения в имеющийся ключ, любо создает новую запись
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, List<TValue> value)
        {
            if (_values.TryGetValue(key, out var tValue))
            {
                tValue.AddRange(value);
            }
            else
            {
                _values.Add(key, value);
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
            if (_values.TryGetValue(key, out var tValue))
            {
                value = tValue;
                return true;
            }

            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary)_values).GetEnumerator();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)_values).GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, List<TValue>>> GetEnumerator()
        {
            return ((IDictionary<TKey, List<TValue>>)_values).GetEnumerator();
        }

        public void Dispose()
        {
            if (_values is IDisposable disposable)
                disposable.Dispose();
        }

        public override string ToString()
        {
            return _values.ToString();
        }
    }
}
