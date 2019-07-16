using System.Collections.Generic;
using System.Linq;

namespace SPAFilter.SPA.Collection
{
    public class CollectionTemplate<T> : List<T> where T : ObjectTemplate
    {
        public List<string> ItemsName => this.Select(x => x.Name).ToList();

        public void InitId()
        {
            var i = 1;
            foreach (var template in this)
            {
                if (template.ID != -1)
                    template.ID = i++;
            }
        }

        public static CollectionTemplate<T> ToCollection(IEnumerable<T> input)
        {
            var collection = new CollectionTemplate<T>();
            collection.AddRange(input);
            collection.InitId();
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
