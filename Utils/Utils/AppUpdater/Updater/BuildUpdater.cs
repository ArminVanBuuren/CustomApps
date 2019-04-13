using System;
using System.Diagnostics;
using System.IO;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    internal class BuildUpdater : IBuildUpdater
    {
        private readonly BuildUpdaterCollection _parent;

        public string FileSource => Path.Combine(_parent.DiretoryTempPath, ServerFile.Location);
        public string FileDestination { get; }

        public bool IsExecutable
        {
            get
            {
                if (LocalFile != null)
                    return LocalFile.IsExecutingFile;
                return false;
            }
        }

        public FileBuildInfo LocalFile { get; }
        public FileBuildInfo ServerFile { get; }

        internal BuildUpdater(BuildUpdaterCollection parent, FileBuildInfo localFile, FileBuildInfo serverFile)
        {
            if(serverFile == null)
                throw new ArgumentNullException($"[{nameof(FileBuildInfo)}] from server cannot be null");

            _parent = parent;
            LocalFile = localFile;
            ServerFile = serverFile;
            FileDestination = Path.Combine(Path.GetDirectoryName(parent.LocationApp), ServerFile.Location);
        }

        internal void Commit()
        {
            switch (ServerFile.Type)
            {
                case BuldPerformerType.CreateOrUpdate:
                case BuldPerformerType.CreateOrRollBack:
                    if (LocalFile == null)
                    {
                        string destinationDir = Path.GetDirectoryName(FileDestination);
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

        internal void Pull()
        {
            CMD.OverwriteAndStartApplication(FileSource, FileDestination, 15); // задержка на запуск 15 секунд
        }

        internal static void Pull(string runningAppLocation)
        {
            CMD.StartApplication(runningAppLocation, 15);  // задержка на запуск 15 секунд
        }

        public override string ToString()
        {
            return $"[{ServerFile.Location}] Local=[{LocalFile?.Version}] Server=[{ServerFile.Version}]";
        }
    }
}
