using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using Utils.AppUpdater.Pack;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    public sealed class BuildPackUpdaterSimple : BuildPackUpdaterBase
    {
        private readonly object sync = new object();
        private UpdaterProcessingArgs _fetchArgs;

        [NonSerialized]
        private readonly IUpdaterProject _updaterProject;

        [NonSerialized]
        private readonly WebClient _webClient;

        [field: NonSerialized]
        public override event UpdaterDownloadProgressChangedHandler DownloadProgressChanged;

        /// <inheritdoc />
        /// <summary>
        /// Когда новые версии билдов скачались на локальный диск в темповую папку. Либо загрузка завершилась неудачей
        /// </summary>
        [field: NonSerialized]
        public override event UpdaterFetchHandler OnFetchComplete;

        public BuildPackUpdaterSimple(Assembly runningApp, BuildPackInfo projectBuildPack, IUpdaterProject updaterProject) : base(runningApp, projectBuildPack)
        {
            _updaterProject = updaterProject ?? throw new ArgumentNullException(nameof(updaterProject));

            if (Count > 0)
            {
                _webClient = new WebClient();
                _webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                _webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            }
        }

        protected override BuildUpdater GetBuildUpdater(FileBuildInfo localFile, FileBuildInfo remoteFile)
        {
            return new BuildUpdaterSimple(this, localFile, remoteFile);
        }

        /// <summary>
        /// Download file
        /// </summary>
        public override void Fetch()
        {
            _fetchArgs = new UpdaterProcessingArgs();

            try
            {
                if (NeedToFetchPack)
                {
                    _webClient.DownloadFileAsync(new Uri($"{_updaterProject.Uri}/{ProjectBuildPack.Name}"), FileTempPath);
                    Status = UploaderStatus.Init;
                }
                else
                {
                    Status = UploaderStatus.Fetched;
                    OnFetchComplete?.BeginInvoke(this, _fetchArgs, null, null);
                }
            }
            catch (Exception ex)
            {
                _fetchArgs.Error = ex;
                OnFetchComplete?.BeginInvoke(this, _fetchArgs, null, null);
            }
        }
        
        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            lock (sync)
            {
                UploadedBytes = e.BytesReceived;
                TotalBytes = e.TotalBytesToReceive;
                DownloadProgressChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled)
            {
                _fetchArgs.Error = e.Error ?? new Exception($"Upload pack '{ProjectBuildPack}' was cancelled!");
            }
            else
            {
                try
                {
                    var downloadedMD5 = Hasher.HashFile(FileTempPath, HashType.MD5);
                    if (downloadedMD5.Like(ProjectBuildPack.MD5))
                    {
                        Directory.CreateDirectory(DiretoryTempPath);
                        ZipFile.ExtractToDirectory(FileTempPath, DiretoryTempPath);
                        DeleteTempFile();
                        Status = UploaderStatus.Fetched;
                    }
                    else
                    {
                        _fetchArgs.Error = new Exception($"Hash of uploaded file [{downloadedMD5}] is incorrect. Remote hash is [{ProjectBuildPack.MD5}]");
                    }
                }
                catch (Exception exception)
                {
                    _fetchArgs.Error = exception;
                }
            }

            OnFetchComplete?.BeginInvoke(this, _fetchArgs, null, null);
        }

        public override void Dispose()
        {
            try
            {
                base.Dispose();
                _webClient?.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}