using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TeleSharp.TL;

namespace SPAFilter.SPA.Collection
{
    public class CollectionTemplate<T> : List<T> where T : class, IObjectTemplate
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