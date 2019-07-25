using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.CollectionHelper
{
    public sealed class DistinctList<T> : IList<T>
    {
        readonly Dictionary<T, bool> _items = new Dictionary<T, bool>();

        public int Count => _items.Count;

        public bool IsReadOnly => false;

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

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(T item)
        {
            return item != null && _items.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.Keys.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.Keys.GetEnumerator();
        }
        public bool Remove(T item)
        {
            return item != null && _items.Remove(item);
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

        #region " Not Supported "

        public T this[int index] { get => throw new System.NotSupportedException(); set => throw new System.NotSupportedException(); }

        public int IndexOf(T item)
        {
            throw new System.NotSupportedException();
        }

        public void Insert(int index, T item)
        {
            throw new System.NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new System.NotSupportedException();
        }

        #endregion
    }
}