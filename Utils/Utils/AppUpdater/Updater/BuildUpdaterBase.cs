using System;
using System.IO;
using Utils.AppUpdater.Pack;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    public abstract class BuildUpdaterBase : BuildUpdater
    {
        protected BuildUpdaterBase(BuildPackUpdaterBase parent, FileBuildInfo localFile, FileBuildInfo remoteFile) : base(parent, localFile, remoteFile) { }

        public abstract void Commit();

        public abstract void Pull(int delayBeforeMove, int delayAfterMoveAndRunApp);
    }
}
