using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Utils.CollectionHelper
{
    [Serializable]
    public sealed class DistinctList<T> : IList<T>, ISerializable, IDisposable
    {
        private readonly IEqualityComparer<T> _comparer;
        readonly Dictionary<T, bool> _items;

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public DistinctList()
        {
            _items = new Dictionary<T, bool>();
        }

        public DistinctList(IEqualityComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _items = new Dictionary<T, bool>(comparer);
        }

        public DistinctList(IEnumerable<T> items)
        {
            _items = new Dictionary<T, bool>();
            AddRange(items);
        }

        public DistinctList(IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _items = new Dictionary<T, bool>(comparer);
            AddRange(items);
        }

        public void AddRange(IEnumerable<T> list)
        {
            foreach (var item in list)
            {
                Add(item);
            }
        }

        public void Add(T item)
        {
            if (item != null && !_items.ContainsKey(item))
                _items.Add(item, true);
        }

        public bool Contains(T item)
        {
            return item != null && _items.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return item != null && _items.Remove(item);
        }

        /// <inheritdoc />
        /// <summary>
        /// Выполняется дольше чем с обычной коллекцией
        /// </summary>
        public T this[int index]
        {
            get => _items.Keys.ElementAt(index);
            set
            {
                var existElement = _items.ElementAt(index);
                if (existElement.Key == null)
                    return;

                _items.Remove(existElement.Key);
                _items.Add(value, existElement.Value);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Выполняется дольше чем с обычной коллекцией
        /// </summary>
        public int IndexOf(T item)
        {
            if (item == null) 
                throw new ArgumentNullException(nameof(item));
            if (_items.ContainsKey(item))
                throw new Exception($"{nameof(item)} doesn't exist.");

            var i = -1;
            foreach (var key in _items.Keys)
            {
                i++;
                if (item.Equals(key))
                    break;
            }

            return i;
        }

        /// <inheritdoc />
        /// <summary>
        /// Выполняется дольше чем с обычной коллекцией
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            var existElement = _items.ElementAt(index);
            if (existElement.Key == null)
                return;

            _items.Remove(existElement.Key);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)_items.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)_items.Keys.GetEnumerator();
        }

        public DistinctList<T> Clone()
        {
            var clone = _comparer == null ? new DistinctList<T>() : new DistinctList<T>(_comparer);
            clone.AddRange(this);
            return clone;
        }

        public override string ToString()
        {
            return _items != null ? _items.Keys.ToString() : base.ToString();
        }

        public void Dispose()
        {
            var disposable = _items as IDisposable;
            disposable?.Dispose();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ((ISerializable)_items).GetObjectData(info, context);
        }

        /// <inheritdoc />
        /// <summary>
        /// Не поддерживается. Т.к. коллекция индексируется по типу &lt;T&gt;
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            throw new System.NotSupportedException();
        }
    }
}