using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utils.CollectionHelper
{
    [Serializable]
    public class Hashtable<T1, T2> : IList<KeyValuePair<T1, T2>>, ICollection<KeyValuePair<T1, T2>>, IEnumerable<KeyValuePair<T1, T2>>, IEnumerable, IList, ICollection, IReadOnlyList<KeyValuePair<T1, T2>>, IReadOnlyCollection<KeyValuePair<T1, T2>>
    {
        private readonly List<KeyValuePair<T1, T2>> hashTable;

        public int Count => hashTable.Count;
        public bool IsReadOnly => ((IList<KeyValuePair<T1, T2>>)hashTable).IsReadOnly;

        public bool IsFixedSize => ((IList)hashTable).IsFixedSize;

        public object SyncRoot => ((IList)hashTable).SyncRoot;

        public bool IsSynchronized => ((IList)hashTable).IsSynchronized;

        object IList.this[int index]
        {
            get => hashTable[index];
            set => ((IList)hashTable)[index] = value;
        }

        public Hashtable() : this(4)
        {

        }
        public Hashtable(IEnumerable<T1> collection) : this(collection.Count())
        {
            foreach (var item in collection)
            {
                Add(item, default(T2));
            }
        }

        public Hashtable(int capacity)
        {
            this.hashTable = new List<KeyValuePair<T1, T2>>(capacity);
        }

        public void Add(T1 input1, T2 input2)
        {
            hashTable.Add(new KeyValuePair<T1, T2>(input1, input2));
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return hashTable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<T1, T2> item)
        {
            hashTable.Add(item);
        }

        public void Clear()
        {
            hashTable.Clear();
        }

        public bool Contains(KeyValuePair<T1, T2> item)
        {
            return hashTable.Contains(item);
        }

        public void CopyTo(KeyValuePair<T1, T2>[] array, int arrayIndex)
        {
            hashTable.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<T1, T2> item)
        {
            return hashTable.Remove(item);
        }

        public int IndexOf(KeyValuePair<T1, T2> item)
        {
            return hashTable.IndexOf(item);
        }

        public void Insert(int index, KeyValuePair<T1, T2> item)
        {
            hashTable.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            hashTable.RemoveAt(index);
        }

        public int Add(object value)
        {
            return ((IList) hashTable).Add(value);
        }

        public bool Contains(object value)
        {
            return ((IList)hashTable).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)hashTable).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)hashTable).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)hashTable).Remove(value);
        }

        public void CopyTo(Array array, int index)
        {
            ((IList)hashTable).CopyTo(array, index);
        }

        public KeyValuePair<T1, T2> this[int index]
        {
            get => hashTable[index];
            set => hashTable[index] = value;
        }
    }
}
