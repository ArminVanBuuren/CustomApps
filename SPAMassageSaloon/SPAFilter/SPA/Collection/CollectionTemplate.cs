using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SPAFilter.SPA.Collection
{
    public class CollectionTemplate<T> : IEnumerable<T>, IDisposable where T : class, IObjectTemplate
    {
        private readonly object sync = new object();

        int _seqID = 0;

        readonly Dictionary<IObjectTemplate, T> _collection;

        public CollectionTemplate()
        {
            _collection = new Dictionary<IObjectTemplate, T>(50);
        }

        public CollectionTemplate(IEnumerable<T> collection)
        {
            _collection = new Dictionary<IObjectTemplate, T>(collection.Count());
            AddCollection(collection);
        }

        public virtual void AddRange(IEnumerable<T> collection)
        {
            AddCollection(collection);
        }

        void AddCollection(IEnumerable<T> collection)
        {
            foreach (var template in collection)
                Add(template);
        }

        public virtual void Add(T template)
        {
            lock (sync)
            {
                template.ID = ++_seqID;
                _collection.Add(template, template);
            }
        }

        public void ResetPublicID()
        {
            lock (sync)
            {
                _seqID = 0;
                foreach (var template in this)
                    template.ID = ++_seqID;
            }
        }

        public int Count
        {
            get
            {
                lock (sync)
                    return _collection.Count;
            }
        }

        public T this[string UniqueName]
        {
            get
            {
                lock (sync)
                {
                    var input = new BlankTemplate(UniqueName);
                    return _collection.TryGetValue(input, out var originalTemplate) ? originalTemplate : null;
                }
            }
        }

        public T this[IObjectTemplate template]
        {
            get
            {
                lock (sync)
                    return _collection.TryGetValue(template, out var originalTemplate) ? originalTemplate : null;
            }
        }

        public void Remove(T template)
        {
            lock (sync)
                _collection.Remove(template);
        }

        public void Clear()
        {
            lock (sync)
                _collection.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (sync)
                return _collection.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (sync)
                return _collection.Values.GetEnumerator();
        }

        public void Dispose()
        {
            Clear();
        }

        public override string ToString()
        {
            return $"Count = {_collection.Count}";
        }
    }
}