using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAFilter.SPA.Components
{
    public sealed class CatalogNetworkElement : ObjectTemplate
    {
        public CatalogNetworkElement(int id, string name) :base(id)
        {
            Name = name;
        }
    }
}
