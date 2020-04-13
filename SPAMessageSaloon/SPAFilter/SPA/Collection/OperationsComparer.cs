using System.Collections.Generic;
using SPAFilter.SPA.Components;

namespace SPAFilter.SPA.Collection
{
    public class OperationsComparer : IEqualityComparer<ObjectTemplate>
    {
        public bool Equals(ObjectTemplate x, ObjectTemplate y)
        {
            return x != null && y != null && x.Name == y.Name;
        }


        public int GetHashCode(ObjectTemplate obj)
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + obj.Name.GetHashCode();
                return hash;
            }
        }
    }
}
