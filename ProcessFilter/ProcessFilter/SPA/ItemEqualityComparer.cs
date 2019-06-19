using System;
using System.Collections.Generic;

namespace SPAFilter.SPA
{
    class ItemEqualityComparer<T> : IEqualityComparer<T> where T : ObjectTemplate
    {
        public bool Equals(T x, T y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.Name.Trim().Equals(y.Name.Trim(), StringComparison.CurrentCultureIgnoreCase);
        }

        public int GetHashCode(T obj)
        {
            return obj.Name.Trim().ToLower().GetHashCode();
        }
    }
}
