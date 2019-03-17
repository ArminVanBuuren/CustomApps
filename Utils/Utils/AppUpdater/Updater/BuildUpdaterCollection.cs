using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Threading;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    internal delegate void BuildUpdaterErrorHandler(object sender, BuildUpdaterProcessingArgs args);

    [Serializable]
    public delegate void UpdaterDownloadProgressChangedHandler(object sender, EventArgs empty);

    [Serializable]
    internal class BuildUpdaterCollection : UploadProgress, IUpdater
    {
        /// <summary>
        /// Когда новые версии билдов скачались на локальный диск в темповую папку. Либо загрузка завершилась неудачей
        /// </summary>
        [field: NonSerialized]
        internal event BuildUpdaterErrorHandler OnFetchComplete;

        private int index = -1;
        private readonly List<IBuildUpdater> _collection;
        private BuildUpdaterProcessingArgs _fetchArgs;
        [NonSerialized]
        private readonly WebClient _webClient;

        [field: NonSerialized]
        public event UpdaterDownloadProgressChangedHandler DownloadProgressChanged;
        public UploaderStatus Status { get; private set; } = UploaderStatus.None;
        public string LocationApp { get; }
        public Uri ProjectUri { get; }
        public BuildPackInfo ProjectBuildPack { get; }
        public string FileTempPath { get; private set; }
        public string DiretoryTempPath { get; private set; }


        internal BuildUpdaterCollection(Assembly runningApp, Uri projectUri, BuildPackInfo projectBuildPack)
        {
            if (runningApp == null)
                throw new ArgumentNullException(nameof(runningApp));
            if (projectUri == null)
                throw new ArgumentNullException(nameof(projectUri));
            if (projectBuildPack == null)
                throw new ArgumentNullException(nameof(projectBuildPack));

            LocationApp = runningApp.Location;
            ProjectUri = projectUri;
            ProjectBuildPack = projectBuildPack;

            _collection = new List<IBuildUpdater>();
            Dictionary<string, FileBuildInfo> localVersions = BuildPackInfo.GetLocalVersions(runningApp);

            foreach (FileBuildInfo serverFile in ProjectBuildPack.Builds)
            {
                if (localVersions.TryGetValue(serverFile.Location, out FileBuildInfo localFile))
                {
                    if ((serverFile.Type == BuldPerformerType.Update || serverFile.Type == BuldPerformerType.CreateOrUpdate) && serverFile.Version > localFile.Version)
                        Add(localFile, serverFile);
                    else if ((serverFile.Type == BuldPerformerType.RollBack || serverFile.Type == BuldPerformerType.CreateOrRollBack) && serverFile.Version < localFile.Version)
                        Add(localFile, serverFile);
                    else if (serverFile.Type == BuldPerformerType.Remove)
                        Add(localFile, serverFile);
                }
                else if (serverFile.Type == BuldPerformerType.CreateOrUpdate || serverFile.Type == BuldPerformerType.CreateOrRollBack)
                {
                    Add(null, serverFile);
                }
            }

            if (Count > 0)
            {
                _webClient = new WebClient();
                _webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                _webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            }
        }

        void Add(FileBuildInfo localFile, FileBuildInfo serverFile)
        {
            _collection.Add(new BuildUpdater(this, localFile, serverFile));
        }

        /// <summary>
        /// Download file
        /// </summary>
        internal void Fetch()
        {
            _fetchArgs = new BuildUpdaterProcessingArgs();

            try
            {
                FileTempPath = Path.GetTempFileName();
                _webClient.DownloadFileAsync(new Uri($"{ProjectUri}/{ProjectBuildPack.Name}"), FileTempPath);
                Status = UploaderStatus.Init;
            }
            catch (Exception ex)
            {
                _fetchArgs.Error = ex;
                OnFetchComplete.BeginInvoke(this, _fetchArgs, null, null);
            }
        }


        private object sync = new object();
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
                _fetchArgs.Error = e.Error ?? new Exception($"Upload pack '{ProjectBuildPack}' from [{ProjectUri}] cancelled!");
            }
            else
            {
                try
                {
                    string downloadedMD5 = Hasher.HashFile(FileTempPath, HashType.MD5);
                    if (downloadedMD5.Like(ProjectBuildPack.MD5))
                    {
                        DiretoryTempPath = Path.Combine(Path.GetTempPath(), STRING.RandomString(15));
                        Directory.CreateDirectory(DiretoryTempPath);
                        ZipFile.ExtractToDirectory(FileTempPath, DiretoryTempPath);
                        DeleteTempFile();
                        Status = UploaderStatus.Fetched;
                    }
                    else
                    {
                        _fetchArgs.Error = new Exception($"Hash of uploaded file [{downloadedMD5}] is incorrect. Sever hash is [{ProjectBuildPack.MD5}]");
                    }
                }
                catch (Exception exception)
                {
                    _fetchArgs.Error = exception;
                }
            }

            OnFetchComplete?.BeginInvoke(this, _fetchArgs, null, null);
        }

        internal void CommitAndPull()
        {
            BuildUpdater runningApp = null;
            foreach (BuildUpdater build in _collection)
            {
                if (build.IsExecutable)
                {
                    runningApp = build;
                    continue;
                }

                build.Commit();
            }

            Status = UploaderStatus.Commited;

            if (runningApp != null)
                runningApp.Pull();
            else
                BuildUpdater.Pull(LocationApp);

            Status = UploaderStatus.Pulled;

            Process.GetCurrentProcess().Kill();
        }

        //void ChangeStatus(UploaderStatus status)
        //{
        //    lock (sync)
        //        Status = status;
        //}

        public int Count
        {
            get
            {
                if (_collection == null)
                    return 0;
                return _collection.Count;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                if (_collection == null || index == -1)
                    return null;
                return _collection[index];
            }
        }

        public IBuildUpdater Current
        {
            get
            {
                if (_collection == null || index == -1)
                    return null;
                return _collection[index];
            }
        }

        bool IEnumerator.MoveNext()
        {
            if (index < Count - 1)
            {
                index++;
                return true;
            }
            ((IEnumerator)this).Reset();
            return false;
        }

        void IEnumerator.Reset()
        {
            index = -1;
        }

        public IEnumerator<IBuildUpdater> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public void Dispose()
        {
            try
            {
                Status = UploaderStatus.Disposed;
                RemoveTempObjects();
                _webClient?.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        void RemoveTempObjects()
        {
            try
            {
                if (!DiretoryTempPath.IsNullOrEmptyTrim() && Directory.Exists(DiretoryTempPath))
                    IO.DeleteReadOnlyDirectory(DiretoryTempPath);
            }
            catch (Exception)
            {
                // ignored
            }

            DeleteTempFile();
        }

        void DeleteTempFile()
        {
            try
            {
                if (!FileTempPath.IsNullOrEmptyTrim() && File.Exists(FileTempPath))
                {
                    IO.AccessToFile(FileTempPath);
                    File.Delete(FileTempPath);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public override string ToString()
        {
            return $"Status=[{Status:G}] Count=[{Count}]";
        }
    }
}