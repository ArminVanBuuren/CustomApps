using System;
using System.Collections.Generic;

namespace SPAFilter.SPA
{
    public class OperationsComparer : IEqualityComparer<IObjectTemplate>
    {
        public bool Equals(IObjectTemplate x, IObjectTemplate y)
        {
            return x != null && y != null && x.Name.Equals(y.Name, StringComparison.CurrentCultureIgnoreCase);
        }


        public int GetHashCode(IObjectTemplate obj)
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + obj.Name.ToLower().GetHashCode();
                return hash;
            }
        }
    }
}
