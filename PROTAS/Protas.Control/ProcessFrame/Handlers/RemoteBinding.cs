using System.Collections.Generic;
using Protas.Components.XPackage;
using Protas.Control.Resource;
using Protas.Control.Resource.Base;

namespace Protas.Control.ProcessFrame.Handlers
{
    internal class RemoteBinding : IHandler
    {
        public Dictionary<string, IHandler> UseHandlers { get; } = new Dictionary<string, IHandler>();

        public RemoteBinding(string name, Process proc, XPack pack)
        {
            foreach (XPack hnd in pack.ChildPacks)
            {

            }
        }

        public string Name { get;  }


        XPack IHandler.Run(ResourceComplex complex)
        {
            throw new System.NotImplementedException();
        }

        XPack IHandler.ForceStop()
        {
            throw new System.NotImplementedException();
        }
    }
}
