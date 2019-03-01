using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Utils.BuildUpdater
{
    public class BuildPack : UploadProgress
    {
        private LocalAssemblyInfo CurrentFile { get; }
        private ServerAssemblyInfo ServerFile { get; }

        private long _uploadedBytes = 0l;
        private long _totalBytes = 0l;

        /// <summary>
        /// Gets the temp file path for the downloaded file
        /// </summary>
        public string TempFilePath { get; }

        /// <summary>
        /// The web client to download the update
        /// </summary>
        private WebClient webClient;

        public bool IsUploaded { get; private set; } = false;

        public BuildPack(LocalAssemblyInfo currentFile, ServerAssemblyInfo serverFile)
        {
            CurrentFile = currentFile;
            ServerFile = serverFile;

            TempFilePath = Path.GetTempFileName();
            webClient = new WebClient();
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
        }

        internal override bool Upload()
        {
            // Download file
            try
            {
                webClient.DownloadFileAsync(ServerFile.UriFilePath, this.TempFilePath);
                return true;
            }
            catch
            {
                return IsUploaded = false;
            }
        }

        /// <summary>
        /// Downloads file from server
        /// </summary>
        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _uploadedBytes = e.BytesReceived;
            _totalBytes = e.TotalBytesToReceive;
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled)
            {
                IsUploaded = false;
                return;
            }
            else
            {
                string file = TempFilePath;
                string updateMD5 = ""; //md5;

                // Hash the file and compare to the hash in the update xml
                if (Hasher.HashFile(file, HashType.MD5).ToUpper() != updateMD5.ToUpper())
                    IsUploaded = false;
                else
                    IsUploaded = true;
            }
        }

        public override long GetUploadedBytes()
        {
            return _uploadedBytes;
        }

        public override long GetTotalBytes()
        {
            return _totalBytes;
        }
    }
}
