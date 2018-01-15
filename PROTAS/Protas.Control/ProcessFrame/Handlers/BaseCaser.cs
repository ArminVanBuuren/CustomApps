using System.Collections.Generic;
using Protas.Components.PerformanceLog;
using Protas.Components.XPackage;
using Protas.Control.Resource;
using Protas.Control.Resource.Base;

namespace Protas.Control.ProcessFrame.Handlers
{

    internal class BaseCaser : ShellLog3Net
    {
        List<BaseCaser> ChildsCases { get; } = new List<BaseCaser>();
        public IHandler Calling { get; }
        public bool IsCorrect { get; } = false;


        public BaseCaser(XPack pack, Process process)
        {
            
        }

        public void Run(ResourceComplex complex)
        {
            
        }

        public void ForceStop()
        {
            
        }
    }
}
