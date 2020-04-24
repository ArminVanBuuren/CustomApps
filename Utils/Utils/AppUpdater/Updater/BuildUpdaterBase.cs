using System;
using System.IO;
using Utils.AppUpdater.Pack;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    public abstract class BuildUpdaterBase : BuildUpdater
    {
        protected BuildUpdaterBase(BuildPackUpdaterBase parent, FileBuildInfo localFile, FileBuildInfo serverFile) : base(parent, localFile, serverFile) { }

        public abstract void Commit();

        public abstract void Pull(int delayBeforeMove, int delayAfterMoveAndRunApp);

        public override string ToString()
        {
            return $"[{ServerFile.Location}] Local = {LocalFile?.Version} Server = {ServerFile.Version}";
        }
    }
}
