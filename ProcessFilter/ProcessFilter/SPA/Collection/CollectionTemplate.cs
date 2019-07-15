using System.Collections.Generic;

namespace SPAFilter.SPA.Collection
{
    public class CollectionTemplate<T> : List<T> where T : ObjectTemplate
    {
        public CollectionTemplate<T> Clone()
        {
            var clone = new CollectionTemplate<T>();
            clone.AddRange(this);
            return clone;
        }
    }
}
