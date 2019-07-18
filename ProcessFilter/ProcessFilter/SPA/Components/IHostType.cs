using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPAFilter.SPA.Collection;

namespace SPAFilter.SPA.Components
{
    public interface IHostType : IObjectTemplate
    {
        CollectionTemplate<IOperation> Operations { get; }
    }
}
