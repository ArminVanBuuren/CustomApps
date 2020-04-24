using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.AppUpdater.Pack;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    public class BuildUpdaterSimple : BuildUpdaterBase
    {
        public BuildUpdaterSimple(BuildPackUpdaterBase parent, FileBuildInfo localFile, FileBuildInfo remoteFile) : base(parent, localFile, remoteFile) { }

        public override void Commit()
        {
            switch (RemoteFile.Type)
            {
                case BuldPerformerType.CreateOrUpdate:
                case BuldPerformerType.CreateOrRollBack:
                    if (LocalFile == null)
                    {
                        var destinationDir = Path.GetDirectoryName(FileDestination);
                        if (!string.IsNullOrWhiteSpace(destinationDir) && !Directory.Exists(destinationDir))
                            Directory.CreateDirectory(destinationDir);

                        CMD.CopyFile(FileSource, FileDestination); // задержка не нужна
                    }
                    else
                    {
                        CMD.OverwriteFile(FileSource, FileDestination); // задержка не нужна
                    }
                    break;

                case BuldPerformerType.RollBack:
                case BuldPerformerType.Update:
                    CMD.OverwriteFile(FileSource, FileDestination); // задержка не нужна
                    break;

                case BuldPerformerType.Remove:
                    CMD.DeleteFile(FileDestination); // задержка не нужна
                    break;

                default:
                    return;
            }
        }

        public override void Pull(int delayBeforeMove, int delayAfterMoveAndRunApp)
        {
            CMD.OverwriteAndStartApplication(FileSource, FileDestination, delayBeforeMove, delayAfterMoveAndRunApp);
        }
    }
}
