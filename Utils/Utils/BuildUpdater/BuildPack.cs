using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utils.BuildUpdater
{
    [Serializable]
    public class WebDownload : WebClient
    {
        /// <summary>
        /// Time in milliseconds
        /// </summary>
        public int Timeout { get; set; } // default timeout is 100 seconds (ASP .NET is 90 seconds)

        public WebDownload() : this(60000) { }

        public WebDownload(int timeout)
        {
            this.Timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request != null)
            {
                request.Timeout = this.Timeout;
            }
            return request;
        }
    }

    [Serializable]
    public class BuildPack : UploadProgress, IUploadProgress
    {
        const string argument_start = "/C choice /C Y /N /D Y /T 4 & Start \"\" /D \"{0}\" \"{1}\"";
        const string argument_update = "/C choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\" & choice /C Y /N /D Y /T 2 & Move /Y \"{1}\" \"{2}\"";
        const string argument_update_start = argument_update + " & Start \"\" /D \"{3}\" \"{4}\" {5}";
        const string argument_add = "/C choice /C Y /N /D Y /T 4 & Move /Y \"{0}\" \"{1}\"";
        const string argument_remove = "/C choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\"";

        public event UploadBuildHandler OnFetchComplete;
        public LocalAssemblyInfo CurrentFile { get; private set; }
        public ServerAssemblyInfo ServerFile { get; private set; }
        /// <summary>
        /// The web client to download the update
        /// </summary>
        private WebClient webClient;

        public BuildPack(LocalAssemblyInfo currentFile, ServerAssemblyInfo serverFile)
        {
            CurrentFile = currentFile;
            ServerFile = serverFile;

            webClient = new WebClient();
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
        }

        // Download file
        public override void Fetch()
        {
            try
            {
                if (ServerFile.UriFilePath != null)
                    webClient.DownloadFileAsync(ServerFile.UriFilePath, ServerFile.FilePath);
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
                    if (CurrentFile == null)
                        return;
                    argument_complete = string.Format(argument_remove, CurrentFile.FilePath);
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
            BuildUpdaterProcessingArgs args = new BuildUpdaterProcessingArgs(this);
            if (e.Error != null || e.Cancelled)
            {
                args.Error = e.Error ?? new Exception($"Upload build \"{ServerFile.ToString()}\" cancelled!");
                IsUploaded = false;
            }
            else
            {
                string downloadedMD5 = Hasher.HashFile(ServerFile.FilePath, HashType.MD5); //md5;
                // Hash the file and compare to the hash in the update xml
                //IsUploaded = downloadedMD5.Like(ServerFile.MD5);
                
                IsUploaded = true;
            }

            OnFetchComplete?.BeginInvoke(this, args, null, null);
        }

        public override string ToString()
        {
            return $"Type=[{this.GetType()}] ServerFile: {ServerFile}";
        }
    }
}
