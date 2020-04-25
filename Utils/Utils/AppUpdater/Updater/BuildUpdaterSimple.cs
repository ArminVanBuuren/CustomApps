using System;
using System.IO;
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

                        CMD.CopyFile(FileSource, FileDestination);
                    }
                    else
                    {
                        CMD.OverwriteFile(FileSource, FileDestination);
                    }
                    break;

                case BuldPerformerType.RollBack:
                case BuldPerformerType.Update:
                    CMD.OverwriteFile(FileSource, FileDestination);
                    break;

                case BuldPerformerType.Remove:
                    CMD.DeleteFile(FileDestination);
                    break;

                default:
                    return;
            }
        }

        public override void Pull(int delayBeforeDeleteAndMove, int delayBetweenMoveAndRun)
        {
            CMD.OverwriteAndStartApplication(FileSource, FileDestination, delayBeforeDeleteAndMove, delayBetweenMoveAndRun);
        }
    }
}
