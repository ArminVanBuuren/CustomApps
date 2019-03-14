using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Policy;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    internal class BuildUpdater : IBuildUpdater
    {
        private BuildUpdaterCollection _parent;
        const string argument_start = "/C choice /C Y /N /D Y /T 4 & Start \"\" /D \"{0}\" \"{1}\"";
        const string argument_update = "/C choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\" & choice /C Y /N /D Y /T 2 & Move /Y \"{1}\" \"{2}\"";
        const string argument_update_start = argument_update + " & Start \"\" /D \"{3}\" \"{4}\" {5}";
        const string argument_add = "/C choice /C Y /N /D Y /T 4 & Move /Y \"{0}\" \"{1}\"";
        const string argument_remove = "/C choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\"";

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
            string argument_complete = "";

            switch (ServerFile.Type)
            {
                case BuldPerformerType.CreateOrUpdate:
                case BuldPerformerType.CreateOrReplace:
                    if (LocalFile == null)
                    {
                        string destinationDir = Path.GetDirectoryName(FileDestination);
                        if (!Directory.Exists(destinationDir))
                            Directory.CreateDirectory(destinationDir);
                        argument_complete = string.Format(argument_add, FileSource, FileDestination);
                    }
                    else
                    {
                        argument_complete = string.Format(argument_update, FileDestination, FileSource, FileDestination);
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(FileDestination));
                    break;
                case BuldPerformerType.RollBack:
                case BuldPerformerType.Update:
                    argument_complete = string.Format(argument_update, FileDestination, FileSource, FileDestination);
                    Directory.CreateDirectory(Path.GetDirectoryName(FileDestination));
                    break;
                case BuldPerformerType.Remove:
                    argument_complete = string.Format(argument_remove, FileDestination);
                    break;
                default:
                    return;
            }

            StartProcess(argument_complete);
        }

        internal void Pull()
        {
            string argument_complete = string.Format(argument_update_start, FileDestination, FileSource, FileDestination, Path.GetDirectoryName(FileDestination), Path.GetFileName(FileDestination), string.Empty);

            StartProcess(argument_complete);
        }

        internal static void Pull(string runningAppLocation)
        {
            string argument_complete = string.Format(argument_start, Path.GetDirectoryName(runningAppLocation), Path.GetFileName(runningAppLocation));
            StartProcess(argument_complete);
        }

        internal static void StartProcess(string arguments)
        {
            ProcessStartInfo cmd = new ProcessStartInfo
            {
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };

            Process.Start(cmd);
        }

        public override string ToString()
        {
            return $"[{ServerFile.Location}] Local=[{LocalFile?.Version}] Server=[{ServerFile.Version}]";
        }
    }
}
