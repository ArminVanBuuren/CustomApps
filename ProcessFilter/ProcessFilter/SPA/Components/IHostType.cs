using SPAFilter.SPA.Collection;

namespace SPAFilter.SPA.Components
{
    public interface IHostType : IObjectTemplate
    {
        CollectionTemplate<IOperation> Operations { get; }
    }
}
