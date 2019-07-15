using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAFilter.SPA
{
    public class OperationsComparer : IEqualityComparer<ObjectTemplate>
    {
        public bool Equals(ObjectTemplate x, ObjectTemplate y)
        {
            return x != null && y != null && x.Name.Equals(y.Name, StringComparison.CurrentCultureIgnoreCase);
        }


        public int GetHashCode(ObjectTemplate obj)
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
