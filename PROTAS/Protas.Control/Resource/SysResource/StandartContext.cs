using System;
using Protas.Control.Resource.Base;
using Protas.Components.XPackage;

namespace Protas.Control.Resource.SysResource
{
    internal class StandartContext : IResourceContext
    {
        public bool IsIntialized => true;

        public ResourceEntityCollection EntityCollection { get; } = new ResourceEntityCollection
        {
            new ResourceEntity("ismatch", typeof (RCIsMatch), RCIsMatch.MinCountParams),
            new ResourceEntity("matches", typeof (RCMatches), RCMatches.MinCountParams),
            new ResourceEntity("replace", typeof (RCReplace), RCReplace.MinCountParams),
            new ResourceEntity("localos", typeof (RCOSVersion), RCOSVersion.MinCountParams),
            new ResourceEntity("if", typeof (RCIfOperator), RCIfOperator.MinCountParams),
            new ResourceEntity("switch", typeof (RCSwitchOperator), RCSwitchOperator.MinCountParams),
            new ResourceEntity("xpath", typeof (RCXPath), RCXPath.MinCountParams),
            new ResourceEntity("math", typeof (RCMath), RCMath.MinCountParams),
            new ResourceEntity("localmachine", typeof (RCMachineName), RCMachineName.MinCountParams),
            new ResourceEntity("now", typeof (RIUNow), RIUNow.MinCountParams),
            new ResourceEntity("random", typeof (RIURandom), RIURandom.MinCountParams),
            new ResourceEntity("newguid", typeof (RIUNewGuid), RIUNewGuid.MinCountParams),
            new ResourceEntity("localram", typeof (RIULocalRAM), RIULocalRAM.MinCountParams),
            new ResourceEntity("xpathdoc", typeof (RSXpackDoc), RSXpackDoc.MinCountParams),
            new ResourceEntity("file", typeof (RSFileSystemWatcher), RSFileSystemWatcher.MinCountParams),
            new ResourceEntity("localpath", typeof (RSLocalPath), RSLocalPath.MinCountParams)
        };

        public IResource GetResource(Type Resource, ResourceConstructor constructor)
        {
            return null;
        }

        public IHandler GetHandler(XPack pack)
        {
            return null;
        }
        public void Dispose()
        {

        }

    }
}