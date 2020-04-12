using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SPAFilter.SPA.Collection
{
    public class CollectionTemplate<T> : IEnumerable<T>, IDisposable where T : class, IObjectTemplate
    {
        private readonly object sync = new object();

        private int _seqPrivateID = 0;
        int _seqID = 0;

        readonly Dictionary<int, T> _collection;

        public List<string> ItemsName => _collection.Values.Select(x => x.Name).ToList();

        public CollectionTemplate()
        {
            _collection = new Dictionary<int, T>(50);
        }

        public CollectionTemplate(IEnumerable<T> collection)
        {
            _collection = new Dictionary<int, T>(collection.Count());
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
                template.PrivateID = ++_seqPrivateID;
                template.ID = ++_seqID;
                _collection.Add(template.ID, template);
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

        public int Count => _collection.Count;

        public T this[int PrivateID] => _collection.TryGetValue(PrivateID, out var template) ? template : default(T);

        public void Remove(T template)
        {
            lock (sync)
                _collection.Remove(template.PrivateID);
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
    }
}