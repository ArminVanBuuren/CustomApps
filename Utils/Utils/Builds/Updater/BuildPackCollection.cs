using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Utils.XmlHelper;

namespace Utils.Builds.Updater
{
    [Serializable]
    public class BuildPackCollection : UploadProgress, IUploadProgress, IEnumerable<BuildPack>, IEnumerator<BuildPack>, IDisposable
    {
        public event UploadBuildHandler OnFetchComplete;
        private List<BuildPack> _collection = new List<BuildPack>();
        int index = -1;
        private Assembly _runningApp;
        private object _lock = new object();
        private int _completedFetchCount = 0;
        private int _waitingFetchCount = 0;
        private bool _inProgress = false;
        BuildUpdaterProcessingArgs fetchArgs;

        public BuildPackCollection(Assembly runningApp, Uri serverUri)
        {
            _runningApp = runningApp;
            Dictionary<string, ServerBuildInfo> serverVersions = GetServerVersions(runningApp, serverUri);
            if(serverVersions.Count == 0)
                return;

            Dictionary<string, FileBuildInfo> localVersions = BuildVersionsInfo.GetLocalVersions(runningApp);

            foreach (ServerBuildInfo server in serverVersions.Values)
            {
                if (localVersions.TryGetValue(server.Location, out FileBuildInfo current))
                {
                    if ((server.Type == BuldPerformerType.Update || server.Type == BuldPerformerType.CreateOrUpdate) && server.Version > current.Version)
                        Add(current, server);
                    else if (server.Type == BuldPerformerType.RollBack && server.Version < current.Version)
                        Add(current, server);
                    else if (server.Type == BuldPerformerType.Remove)
                        Add(current, server);
                }
                else if (server.Type == BuldPerformerType.CreateOrUpdate)
                {
                    Add(null, server);
                }
            }
        }

        static Dictionary<string, ServerBuildInfo> GetServerVersions(Assembly runningApp, Uri serverUri)
        {
            Uri versionsInfo = new Uri($"{serverUri}/{BuildVersionsInfo.FILE_NAME}");
            string resultStr = WEB.WebHttpStringData(versionsInfo, out HttpStatusCode resHttp, HttpRequestCacheLevel.NoCacheNoStore);

            if (resHttp == HttpStatusCode.OK)
            {
                XmlSerializer xsSubmit = new XmlSerializer(typeof(BuildVersionsInfo));
                BuildVersionsInfo res;
                using (TextReader reader = new StringReader(resultStr))
                {
                    res = (BuildVersionsInfo)xsSubmit.Deserialize(reader);
                }

                string assemblyDirPath = runningApp.GetDirectory();
                Dictionary<string, ServerBuildInfo> serverVersions = new Dictionary<string, ServerBuildInfo>();
                foreach (FileBuildInfo buildInfo in res.Builds)
                {
                    ServerBuildInfo serverFileInfo = new ServerBuildInfo(buildInfo, serverUri, assemblyDirPath);
                    serverVersions.Add(serverFileInfo.Location, serverFileInfo);
                }

                return serverVersions;
            }

            throw new Exception($"Catched exception when get status from server. HttpStatus=[{resHttp:G}] Uri=[{versionsInfo.AbsoluteUri}]");
        }

        static Dictionary<string, ServerBuildInfo> GetServerVersionsViaXMLDoc(Assembly runningApp, Uri serverUri)
        {
            Uri versionsInfo = new Uri($"{serverUri}/{BuildVersionsInfo.FILE_NAME}");
            string resultStr = WEB.WebHttpStringData(versionsInfo, out HttpStatusCode resHttp, HttpRequestCacheLevel.NoCacheNoStore);

            if (resHttp == HttpStatusCode.OK)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(resultStr);

                XmlNodeList updateNodes = doc.DocumentElement.SelectNodes("/Versions/Build");
                Dictionary<string, ServerBuildInfo> serverVersions = new Dictionary<string, ServerBuildInfo>();
                foreach (XmlNode updateNode in updateNodes)
                {
                    if (updateNode == null)
                        continue;

                    var getNodesWithoutCase = updateNode.GetChildNodes(StringComparer.CurrentCultureIgnoreCase);
                    ServerBuildInfo serverFileInfo = new ServerBuildInfo(getNodesWithoutCase, serverUri, runningApp.GetDirectory());
                    serverVersions.Add(serverFileInfo.Location, serverFileInfo);
                }

                return serverVersions;
            }

            throw new Exception($"Catched exception when get status from server. HttpStatus=[{resHttp:G}] Uri=[{versionsInfo.AbsoluteUri}]");
        }

        

        void Add(FileBuildInfo currentFile, ServerBuildInfo serverFile)
        {
            BuildPack build = new BuildPack(currentFile, serverFile);
            build.OnFetchComplete += Build_OnFetchComplete;
            _collection.Add(build);
        }

        public override bool Fetch()
        {
            lock (_lock)
            {
                if (!_inProgress && Count > 0)
                {
                    fetchArgs = new BuildUpdaterProcessingArgs(this);
                    try
                    {
                        _inProgress = true;
                        _completedFetchCount = 0;
                        _waitingFetchCount = 0;
                        foreach (BuildPack build in _collection)
                        {
                            if (build.Fetch())
                            {
                                _waitingFetchCount++;
                            }
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        _inProgress = false;
                        fetchArgs.Error = ex;
                        OnFetchComplete.Invoke(this, fetchArgs);
                    }
                }
                return false;
            }
        }

        private void Build_OnFetchComplete(object sender, BuildUpdaterProcessingArgs e)
        {
            lock (_lock)
            {
                if (!_inProgress)
                {
                    if (sender != null && sender is BuildPack)
                    {
                        ((BuildPack)sender).RemoveTempFiles();
                    }
                    return;
                }

                _completedFetchCount++;

                if (e.Error != null)
                    fetchArgs.InnerException.Add(e.Error);

                if (_waitingFetchCount > _completedFetchCount)
                    return;

                if(fetchArgs.InnerException.Count > 0)
                    fetchArgs.Error = new Exception("Catched exception when download files from server!");

                _inProgress = false;
                OnFetchComplete.Invoke(this, fetchArgs);
            }
        }

        public override void Commit()
        {
            if(!IsUploaded)
                return;

            BuildPack runningApp = null;
            foreach (BuildPack build in _collection)
            {
                if (build.CurrentFile != null && build.CurrentFile.IsExecutingFile)
                {
                    runningApp = build;
                    continue;
                }

                build.Commit();
            }

            if (runningApp != null)
                BuildPack.EndOfCommit(runningApp);
            else
                BuildPack.EndOfCommit(_runningApp);

            Process.GetCurrentProcess().Kill();
        }

        public override void RemoveTempFiles()
        {
            foreach (BuildPack build in _collection)
            {
                build.RemoveTempFiles();
            }
        }

        public override bool IsUploaded
        {
            get
            {
                int count = 0;
                foreach (BuildPack build in _collection)
                {
                    count++;
                    if (!build.IsUploaded)
                        return false;
                }

                return count > 0;
            }
        }

        public override int ProgressPercent
        {
            get
            {
                if (Count == 0)
                    return 0;

                int allFileProgress = 0;
                foreach (BuildPack build in _collection)
                {
                    allFileProgress += build.ProgressPercent;
                }

                return allFileProgress / Count;
            }
        }

        public override long UploadedBytes
        {
            get
            {
                long allFileUploads = 0l;
                foreach (BuildPack build in _collection)
                {
                    allFileUploads += build.UploadedBytes;
                }

                return allFileUploads;
            }
        }

        public override long TotalBytes
        {
            get
            {
                long allFileTotla = 0l;
                foreach (BuildPack build in _collection)
                {
                    allFileTotla += build.TotalBytes;
                }

                return allFileTotla;
            }
        }

        
        public int Count => _collection.Count;

        object IEnumerator.Current => _collection[index];

        public BuildPack Current => _collection[index];

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

        public IEnumerator<BuildPack> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        void IDisposable.Dispose()
        {
            ((IDisposable)_collection).Dispose();
        }

        public override string ToString()
        {
            return $"Count=[{Count}]";
        }
    }
}