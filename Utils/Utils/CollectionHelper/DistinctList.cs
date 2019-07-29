using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Utils.CollectionHelper
{
    public sealed class DistinctList<T> : IList<T>
    {
        readonly Dictionary<T, bool> _items;

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public DistinctList(IEqualityComparer<T> comparer = null)
        {
            if (comparer != null)
                _items = new Dictionary<T, bool>(comparer);
            else
                _items = new Dictionary<T, bool>();
        }

        public void Add(T item)
        {
            if (item != null && !_items.ContainsKey(item))
                _items.Add(item, true);
        }

        public void AddRange(IEnumerable<T> list)
        {
            foreach (var item in list)
            {
                Add(item);
            }
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
            return _items.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.Keys.GetEnumerator();
        }

        public DistinctList<T> Clone()
        {
            var clone = new DistinctList<T>();
            clone.AddRange(this);
            return clone;
        }

        public void Insert(int index, T item)
        {
            throw new System.NotSupportedException();
        }
    }
}