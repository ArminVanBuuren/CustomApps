using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;

namespace Utils.Builds.Updater
{
    [Serializable]
    public class BuildPack : UploadProgress, IUploadProgress
    {
        const string argument_start = "/C choice /C Y /N /D Y /T 4 & Start \"\" /D \"{0}\" \"{1}\"";
        const string argument_update = "/C choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\" & choice /C Y /N /D Y /T 2 & Move /Y \"{1}\" \"{2}\"";
        const string argument_update_start = argument_update + " & Start \"\" /D \"{3}\" \"{4}\" {5}";
        const string argument_add = "/C choice /C Y /N /D Y /T 4 & Move /Y \"{0}\" \"{1}\"";
        const string argument_remove = "/C choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\"";

        public event UploadBuildHandler OnFetchComplete;
        public FileBuildInfo CurrentFile { get; private set; }
        public ServerBuildInfo ServerFile { get; private set; }
        /// <summary>
        /// The web client to download the update
        /// </summary>
        private WebClient webClient;

        public BuildPack(FileBuildInfo currentFile, ServerBuildInfo serverFile)
        {
            CurrentFile = currentFile;
            ServerFile = serverFile;

            webClient = new WebClient();
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
        }

        // Download file
        public override bool Fetch()
        {
            try
            {
                if (ServerFile.UriFilePath != null)
                {
                    webClient.DownloadFileAsync(ServerFile.UriFilePath, ServerFile.FilePath);
                    return true;
                }
                else
                {
                    IsUploaded = true;
                }
                return false;
            }
            catch
            {
                IsUploaded = false;
                throw;
            }
        }

        public override void Commit()
        {
            string argument_complete = "";

            switch (ServerFile.Type)
            {
                case BuldPerformerType.CreateOrUpdate:
                    if (CurrentFile == null)
                    {
                        string dirpath = Path.GetDirectoryName(ServerFile.DestinationFilePath);
                        if (!Directory.Exists(dirpath))
                            Directory.CreateDirectory(dirpath);
                        argument_complete = string.Format(argument_add, ServerFile.FilePath, ServerFile.DestinationFilePath);
                    }
                    else
                    {
                        argument_complete = string.Format(argument_update, CurrentFile.FilePath, ServerFile.FilePath, ServerFile.DestinationFilePath);
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(ServerFile.DestinationFilePath));
                    break;
                case BuldPerformerType.RollBack:
                case BuldPerformerType.Update:
                    argument_complete = string.Format(argument_update, CurrentFile.FilePath, ServerFile.FilePath, ServerFile.DestinationFilePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(ServerFile.DestinationFilePath));
                    break;
                case BuldPerformerType.Remove:
                    argument_complete = string.Format(argument_remove, ServerFile.DestinationFilePath);
                    break;
                default:
                    return;
            }

            StartProcess(argument_complete);
        }

        public override void RemoveTempFiles()
        {
            try
            {
                if (File.Exists(ServerFile.FilePath))
                    File.Delete(ServerFile.FilePath);
            }
            catch (Exception e)
            {

            }
        }

        internal static void EndOfCommit(BuildPack runningApp)
        {
            string argument_complete = string.Format(argument_update_start, runningApp.CurrentFile.FilePath, runningApp.ServerFile.FilePath, runningApp.ServerFile.DestinationFilePath, Path.GetDirectoryName(runningApp.ServerFile.DestinationFilePath), Path.GetFileName(runningApp.ServerFile.DestinationFilePath), string.Empty);

            StartProcess(argument_complete);
        }

        internal static void EndOfCommit(Assembly runningApp)
        {
            string argument_complete = string.Format(argument_start, Path.GetDirectoryName(runningApp.Location), Path.GetFileName(runningApp.Location));
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

        /// <summary>
        /// Downloads file from server
        /// </summary>
        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UploadedBytes = e.BytesReceived;
            TotalBytes = e.TotalBytesToReceive;
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string downloadedMD5;
            BuildUpdaterProcessingArgs args = new BuildUpdaterProcessingArgs(this);
            if (e.Error != null || e.Cancelled)
            {
                args.Error = e.Error ?? new Exception($"Upload build \"{ServerFile.ToString()}\" cancelled!");
                IsUploaded = false;
            }
            else
            {
                //downloadedMD5 = Hasher.HashFile(ServerFile.FilePath, HashType.MD5); //md5;
                //IsUploaded = downloadedMD5.Like(ServerFile.MD5); // Hash the file and compare to the hash in the update xml
            }

            OnFetchComplete?.BeginInvoke(this, args, null, null);
        }

        public override string ToString()
        {
            return ServerFile.ToString();
        }
    }
}
