using Protas.Components.PerformanceLog;
using Protas.Components.XPackage;
using Protas.Control.Resource;

namespace Protas.Control.ProcessFrame.Triggers
{
    internal sealed class CoreTrigger : Trigger, IProcessor
    {
        public const string ATTR_TARGET = "Target";
        ResourceKernel DirectResource { get; }
        public string Property { get; }
        public bool IsCorrect { get; private set; } = false;

        public CoreTrigger(int id, XPack pack, Process proces) :base(id, pack, proces)
        {
            string prop;
            if (!pack.Attributes.TryGetValue(ATTR_TARGET, out prop))
            {
                AddLogForm(Log3NetSeverity.Error, "Attrubute {0} Not Exist", ATTR_TARGET);
                return;
            }
            Property = prop;

            if (!DirectResource.IsResource)
            {
                AddLog(Log3NetSeverity.Error, "Property Is Not Resource");
                return;
            }
            DirectResource.ResourcesChanged += (ResourcesChanged);

            IsCorrect = true;
        }

        void ResourcesChanged(ResourceKernel resource, XPack result)
        {
            CallHandlers(GetComplex(result));
        }

        public bool Start()
        {
            if (!IsCorrect)
                return false;
            DirectResource.ProcessingMode = ResourceMode.Core;
            AddLog(Log3NetSeverity.Debug, "Started...");
            return true;
        }
        public bool Stop()
        {
            if (!IsCorrect)
                return false;
            DirectResource.ProcessingMode = ResourceMode.External;
            AddLog(Log3NetSeverity.Debug, "Stopped...");
            return true;
        }
        
        public override void Dispose()
        {
            Stop();
            DirectResource?.Dispose();
        }
    }
}
