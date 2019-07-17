using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TeleSharp.TL;

namespace SPAFilter.SPA.Collection
{
    //public class CollectionTemplate1<T> : IList<T> where T : ObjectTemplate
    //{
    //    private readonly Dictionary<string, T> _collection;

    //    public int Count => _collection.Count;

    //    public bool IsReadOnly => true;

    //    public T this[string index]
    //    {
    //        get => _collection[index];
    //        set => _collection[index] = value;
    //    }

    //    public T this[int index]
    //    {
    //        get => null;
    //        set { }
    //    }

    //    public CollectionTemplate1(IEnumerable<T> input, bool initSequence = true, StringComparer comparer = null)
    //    {
    //        if (comparer != null)
    //            _collection = new Dictionary<string, T>(comparer);

    //        if (initSequence)
    //        {
    //            var i = 0;
    //            foreach (var template in input)
    //            {
    //                _collection.Add(template.Name, template);
    //                template.ID = ++i;
    //            }
    //        }
    //        else
    //        {
    //            foreach (var template in input)
    //            {
    //                _collection.Add(template.Name, template);
    //            }
    //        }
    //    }

    //    public void InitSequence()
    //    {
    //        var i = 0;
    //        foreach (var template in this)
    //        {
    //            template.ID = ++i;
    //        }
    //    }

    //    public int IndexOf(T item)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Insert(int index, T item)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void RemoveAt(int index)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Add(T item)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Clear()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool Contains(T item)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void CopyTo(T[] array, int arrayIndex)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool Remove(T item)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public IEnumerator<T> GetEnumerator()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class CollectionTemplate<T> : IList<T> where T : ObjectTemplate
    //{
    //    public T this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    //    public bool IsReadOnly => throw new NotImplementedException();

    //    public void Add(T item)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool Contains(T item)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void CopyTo(T[] array, int arrayIndex)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public int IndexOf(T item)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Insert(int index, T item)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool Remove(T item)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void RemoveAt(int index)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class CollectionTemplate<T> : List<T> where T : ObjectTemplate
    {
        public List<string> ItemsName => this.Select(x => x.Name).ToList();

        public void InitSequence()
        {
            var i = 0;
            foreach (var template in this)
            {
                template.ID = ++i;
            }
        }

        public static CollectionTemplate<T> ToCollection(IEnumerable<T> input, bool initSequence = true)
        {
            var collection = new CollectionTemplate<T>();
            collection.AddRange(input);
            if (initSequence)
                collection.InitSequence();
            return collection;
        }

        public CollectionTemplate<T> Clone()
        {
            var clone = new CollectionTemplate<T>();
            clone.AddRange(this);
            return clone;
        }
    }
}