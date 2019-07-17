using System;
using System.Collections.Generic;
using System.Linq;
using TeleSharp.TL;

namespace SPAFilter.SPA.Collection
{
    //public class CollectionTemplate<TValue> : Dictionary<string, TValue> where TValue : ObjectTemplate
    //{
    //    CollectionTemplate()
    //    {

    //    }

    //    CollectionTemplate(IDictionary<string, TValue> input) : base(input)
    //    {

    //    }

    //    public void InitSequence()
    //    {
    //        var i = 0;
    //        foreach (var template in this)
    //        {
    //            template.Value.ID = ++i;
    //        }
    //    }

    //    public static CollectionTemplate<TValue> ToCollection(IEnumerable<TValue> input, bool initSequence = true)
    //    {
    //        var collection = new CollectionTemplate<TValue>();
    //        if (initSequence)
    //        {
    //            var i = 0;
    //            foreach (var template in input)
    //            {
    //                collection.Add(template.Name, template);
    //                template.ID = ++i;
    //            }
    //        }
    //        else
    //        {
    //            foreach (var item in input)
    //            {
    //                collection.Add(item.Name, item);
    //            }
    //        }
    //        return collection;
    //    }

    //    public CollectionTemplate<TValue> Clone()
    //    {
    //        return new CollectionTemplate<TValue>(this);
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