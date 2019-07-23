namespace SPAFilter.SPA.Components
{
    public interface IOperation : IObjectTemplate
    {
        string HostTypeName { get; }

        bool IsScenarioExist { get; set; }
    }
}
