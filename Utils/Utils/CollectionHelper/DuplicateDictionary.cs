using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Utils.CollectionHelper
{
    [Serializable]
    public class DuplicateDictionary<TKey, TValue> : IDictionary<TKey, List<TValue>>, IDictionary, ISerializable, IDisposable
    {
        readonly Dictionary<TKey, List<TValue>> _values;

        public DuplicateDictionary() : this(4)
        {

        }

        public DuplicateDictionary(int capacity)
        {
            _values = new Dictionary<TKey, List<TValue>>(capacity);
        }

        public DuplicateDictionary(IEqualityComparer<TKey> comparer) : this(4, comparer)
        {

        }

        public DuplicateDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            _values = new Dictionary<TKey, List<TValue>>(capacity, comparer);
        }

        /// <summary>
        /// Присваивает другую коллекцию для ключа, либо создает новую запись
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public List<TValue> this[TKey index]
        {
            get => _values.TryGetValue(index, out var tValue) ? tValue : null;
            set
            {
                if (_values.TryGetValue(index, out var result))
                {
                    //result.AddRange(value);
                    //_values[index].AddRange(value);
                    _values[index] = value;
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
        
         /// <inheritdoc />
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
            if (_values is IDisposable disposable)
                disposable.Dispose();
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

        public override string ToString()
        {
            return _values != null ? _values.ToString() : base.ToString();
        }
    }
}
