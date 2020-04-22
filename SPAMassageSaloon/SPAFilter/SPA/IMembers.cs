using SPAFilter.SPA.Collection;

namespace SPAFilter.SPA
{
    public interface IHostType : IObjectTemplate
    {
        CollectionTemplate<IOperation> Operations { get; }
    }

    public interface IOperation : ISAComponent
    {
        bool IsScenarioExist { get; set; }
    }

    public interface ISAComponent : IObjectTemplate
    {
        string HostTypeName { get; }
    }

    public interface IObjectTemplate
    {
        string UniqueName { get; }
        int ID { get; set; }
        string Name { get; set; }
    }
}
