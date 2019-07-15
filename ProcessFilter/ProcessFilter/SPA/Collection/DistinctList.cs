using System.Collections.Generic;

namespace SPAFilter.SPA.Collection
{
    public class DistinctList<T> : List<T>
    {
        public new void Add(T item)
        {
            if (!this.Contains(item))
                base.Add(item);
        }

        public new void AddRange(IEnumerable<T> list)
        {
            foreach (var item in list)
            {
                Add(item);
            }
        }

        public DistinctList<T> Clone()
        {
            var clone = new DistinctList<T>();
            clone.AddRange(this);
            return clone;
        }
    }
}
