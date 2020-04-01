using System;
using System.Collections.Generic;
using SPAFilter.SPA.Components;
using Utils;

namespace SPAFilter.SPA
{
    public class SAComparer : IEqualityComparer<ISAComponent>
    {
        public bool Equals(ISAComponent x, ISAComponent y)
        {
            return x != null && y != null && x.Name.Like(y.Name) && x.HostTypeName.Like(y.HostTypeName);
        }


        public int GetHashCode(ISAComponent obj)
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
