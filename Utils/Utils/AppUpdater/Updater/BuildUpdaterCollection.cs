using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Utils.XmlHelper;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    internal class BuildUpdaterCollection : UploadProgress, IUpdater
    {
        public event UploadBuildHandler OnFetchComplete;
        private List<IBuildUpdater> _collection;
        int index = -1;
        private object _lock = new object();

        ApplicationUpdaterProcessingArgs fetchArgs;
        private WebClient _webClient;

        public UploaderStatus Status { get; protected set; } = UploaderStatus.None;
        public string LocationApp { get; }
        public Uri ProjectUri { get; }
        public BuildPackInfo ProjectBuildPack { get; }
        public string FileTempPath { get; private set; }
        public string DiretoryTempPath { get; private set; }


        internal BuildUpdaterCollection(ApplicationUpdater updater, BuildsInfo remoteBuilds)
        {
            LocationApp = updater.RunningApp.Location;
            ProjectUri = updater.ProjectUri;
            ProjectBuildPack = remoteBuilds.Packs.Where(p => p.Project == updater.Project).FirstOrDefault();
            if (ProjectBuildPack == null || ProjectBuildPack.Builds.Count == 0)
                return;

            //Dictionary<string, FileBuildInfo> serverVersions = ProjectBuildPack.ToDictionary(x => x.Location, x => x);

            _collection = new List<IBuildUpdater>();
            Dictionary<string, FileBuildInfo> localVersions = BuildPackInfo.GetLocalVersions(updater.RunningApp);

            foreach (FileBuildInfo serverFile in ProjectBuildPack.Builds)
            {
                if (localVersions.TryGetValue(serverFile.Location, out FileBuildInfo localFile))
                {
                    if ((serverFile.Type == BuldPerformerType.Update || serverFile.Type == BuldPerformerType.CreateOrUpdate) && serverFile.Version > localFile.Version)
                        Add(localFile, serverFile);
                    else if (serverFile.Type == BuldPerformerType.RollBack && serverFile.Version < localFile.Version)
                        Add(localFile, serverFile);
                    else if (serverFile.Type == BuldPerformerType.Remove)
                        Add(localFile, serverFile);
                    continue;
                }
                else if (serverFile.Type == BuldPerformerType.CreateOrUpdate)
                {
                    Add(null, serverFile);
                    continue;
                }

                if (serverFile.Type == BuldPerformerType.CreateOrReplace)
                {
                    Add(localFile, serverFile);
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
        /// <param name="uriProject"></param>
        internal bool Fetch()
        {
            lock (_lock)
            {
                if (Status == UploaderStatus.None || Status == UploaderStatus.Init)
                {
                    fetchArgs = new ApplicationUpdaterProcessingArgs(this);

                    try
                    {
                        FileTempPath = Path.GetTempFileName();
                        _webClient.DownloadFileAsync(new Uri($"{ProjectUri}/{ProjectBuildPack.Name}"), FileTempPath);
                        Status = UploaderStatus.Init;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        fetchArgs.Error = ex;
                        OnFetchComplete.Invoke(this, fetchArgs);
                        return false;
                    }
                }

                return false;
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UploadedBytes = e.BytesReceived;
            TotalBytes = e.TotalBytesToReceive;
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            lock (_lock)
            {
                if (e.Error != null || e.Cancelled)
                {
                    fetchArgs.Error = e.Error ?? new Exception($"Upload pack of {ProjectBuildPack} from [{ProjectUri}] cancelled!");
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
                            fetchArgs.Error = new Exception($"Hash of uploaded file [{downloadedMD5}] is incorrect. Sever hash is [{ProjectBuildPack.MD5}]");
                        }
                    }
                    catch (Exception exception)
                    {
                        fetchArgs.Error = exception;
                    }
                }

                OnFetchComplete?.Invoke(this, fetchArgs);
            }
        }

        internal bool CommitAndPull()
        {
            lock (_lock)
            {
                if(Status != UploaderStatus.Fetched)
                    return false;

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

                return true;
            }
        }

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
            RemoveTempObjects();
            try
            {
                _webClient?.Dispose();
                if (_collection != null)
                    ((IDisposable) _collection).Dispose();
            }
            catch (Exception e)
            {
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

            }
        }

        public override string ToString()
        {
            return $"Count=[{Count}]";
        }
    }
}