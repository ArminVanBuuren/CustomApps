using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using Utils.AssemblyHelper;
using Utils.CollectionHelper;
using Utils.XmlHelper;

namespace Utils.BuildUpdater
{
    public enum UpdateBuildResult
    {
        Cancel = 0,
        Update = 1
    }

    public class BuildUpdaterArgs
    {
        internal BuildUpdaterArgs(BuildPackCollection buildPacks)
        {
            BuildPacks = buildPacks;
            Result = UpdateBuildResult.Cancel;
        }
        public BuildPackCollection BuildPacks { get; }
        public UpdateBuildResult Result { get; set; }
    }

    [Serializable]
    public class BuildUpdaterProcessingArgs
    {
        internal BuildUpdaterProcessingArgs(string exception, IUploadProgress faildObj = null)
        {
            FaildObject = faildObj;
            Error = new Exception(exception);
        }

        internal BuildUpdaterProcessingArgs(Exception error = null, IUploadProgress faildObj = null)
        {
            FaildObject = faildObj;
            Error = error;
        }

        internal BuildUpdaterProcessingArgs(IUploadProgress faildObj)
        {
            FaildObject = faildObj;
        }

        public IUploadProgress FaildObject { get; }
        public Exception Error { get; internal set; }
        public List<Exception> InnerException { get; } = new List<Exception>();
    }

    [Serializable]
    public delegate void UploadBuildHandler(object sender, BuildUpdaterProcessingArgs args);

    [Serializable]
    public delegate void BuildUpdaterHandler(object sender, BuildUpdaterArgs buildPack);

    public class BuildUpdater
    {
        public event BuildUpdaterHandler UpdateOnNewVersion;
        public event UploadBuildHandler OnProcessingError;
        private readonly Timer _stopWatch;
        private readonly Uri _xmlVersionPath;
        private readonly Uri _uriToServerProject;
        private readonly Assembly _runningApp;
        private BuildPackCollection deltaList;
        private readonly int _updateMSec;
        object _lock = new object();

        public BuildUpdater(Assembly runningApp, string uriProject, int updateSec = 10)
        {
            _runningApp = runningApp;
            _uriToServerProject = new Uri(uriProject.TrimEnd('/'));
            _xmlVersionPath = new Uri(_uriToServerProject + "/version.xml");
            _updateMSec = updateSec * 1000;

            _stopWatch = new Timer();
            _stopWatch.Elapsed += GetNewestBuildsVersion;
            _stopWatch.Interval = _updateMSec;
            _stopWatch.AutoReset = false;
            EnableTimer();
        }

        public void CheckNewVersion()
        {
            lock (_lock)
            {
                if (_stopWatch.Interval != _updateMSec)
                    _stopWatch.Interval = _updateMSec;
                if (_stopWatch.Enabled == true)
                    EnableTimer();
            }

            GetNewestBuildsVersion(this, null);
        }

        private void GetNewestBuildsVersion(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                try
                {
                    if (UpdateOnNewVersion == null)
                    {
                        EnableTimer();
                        return;
                    }

                    Dictionary<string, LocalAssemblyInfo> currentVersions = new Dictionary<string, LocalAssemblyInfo>(StringComparer.CurrentCultureIgnoreCase);
                    string assemblyDirPath = _runningApp.GetDirectory();
                    foreach (string file in Directory.GetFiles(assemblyDirPath, "*.*", SearchOption.AllDirectories))
                    {
                        LocalAssemblyInfo localFileInfo = new LocalAssemblyInfo(file, assemblyDirPath, file.Like(_runningApp.Location));
                        currentVersions.Add(localFileInfo.FileName, localFileInfo);
                    }

                    string resultStr = WEB.WebHttpStringData(_xmlVersionPath, out HttpStatusCode resHttp, HttpRequestCacheLevel.NoCacheNoStore);

                    if (resHttp == HttpStatusCode.OK)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(resultStr);

                        XmlNodeList updateNodes = doc.DocumentElement.SelectNodes("/Versions/Build");
                        List<ServerAssemblyInfo> serverVersions = new List<ServerAssemblyInfo>();
                        foreach (XmlNode updateNode in updateNodes)
                        {
                            if (updateNode == null)
                                continue;

                            var getNodesWithoutCase = updateNode.GetChildNodes(StringComparer.CurrentCultureIgnoreCase);
                            serverVersions.Add(new ServerAssemblyInfo(getNodesWithoutCase, _uriToServerProject, assemblyDirPath));
                        }

                        deltaList = new BuildPackCollection(_runningApp);
                        foreach (ServerAssemblyInfo server in serverVersions)
                        {
                            if (currentVersions.TryGetValue(server.FileName, out LocalAssemblyInfo current))
                            {
                                if ((server.Type == BuldPerformerType.Update || server.Type == BuldPerformerType.CreateOrUpdate) && server.Build > current.Build)
                                    deltaList.Add(current, server);
                                else if (server.Type == BuldPerformerType.RollBack && server.Build < current.Build)
                                    deltaList.Add(current, server);
                                else if (server.Type == BuldPerformerType.Remove)
                                    deltaList.Add(current, server);
                            }
                            else if (server.Type == BuldPerformerType.CreateOrUpdate)
                            {
                                deltaList.Add(null, server);
                            }
                        }

                        if (deltaList.Count > 0)
                        {
                            deltaList.OnFetchComplete += DeltaList_OnFetchComplete;
                            deltaList.Fetch();
                            return;
                        }
                    }
                    else
                    {
                        OnProcessingError?.Invoke(this,
                            new BuildUpdaterProcessingArgs($"Catched exception when get status from server. HttpStatus=[{resHttp:G}] Uri=[{_xmlVersionPath.AbsoluteUri}]"));
                    }
                }
                catch (Exception ex)
                {
                    OnProcessingError?.Invoke(this, new BuildUpdaterProcessingArgs(ex));
                }

                EnableTimer();
            }
        }

        

        private void DeltaList_OnFetchComplete(object sender, BuildUpdaterProcessingArgs e)
        {
            lock (_lock)
            {
                try
                {
                    if (sender == null || deltaList == null || !(sender is BuildPackCollection) || ((BuildPackCollection) sender) != deltaList)
                        return;

                    if (!deltaList.IsUploaded || e.Error != null)
                    {
                        deltaList.RemoveTempFiles();
                        OnProcessingError?.Invoke(this, e.Error == null ? new BuildUpdaterProcessingArgs("Not all files were successfully upload!", deltaList) : e);
                        EnableTimer();
                        return;
                    }

                    var eventListeners = UpdateOnNewVersion?.GetInvocationList();
                    if (eventListeners == null || !eventListeners.Any())
                    {
                        EnableTimer();
                        return;
                    }

                    BuildUpdaterArgs buildArgs = new BuildUpdaterArgs(deltaList);
                    foreach (BuildUpdaterHandler del in eventListeners)
                    {
                        del.Invoke(this, buildArgs);
                        if (buildArgs.Result == UpdateBuildResult.Cancel)
                        {
                            deltaList.RemoveTempFiles();
                            EnableTimer();
                            return;
                        }
                    }

                    deltaList.Commit();
                    return;
                }
                catch (Exception ex)
                {
                    OnProcessingError?.Invoke(this, new BuildUpdaterProcessingArgs(ex, deltaList));
                }

                EnableTimer();
            }
        }

        void EnableTimer()
        {
            _stopWatch.Enabled = true;
        }

        /// <summary>
        /// Checks the Uri to make sure file exist
        /// </summary>
        /// <param name="location">The Uri of the update.xml</param>
        /// <returns>If the file exists</returns>
        public static bool ExistsOnServer(Uri location)
        {
            if (location.ToString().StartsWith("file"))
            {
                return System.IO.File.Exists(location.LocalPath);
            }
            else
            {
                try
                {
                    // Request the update.xml
                    HttpWebRequest req = (HttpWebRequest) WebRequest.Create(location.AbsoluteUri);
                    // Read for response
                    HttpWebResponse resp = (HttpWebResponse) req.GetResponse();
                    resp.Close();

                    return resp.StatusCode == HttpStatusCode.OK;
                }
                catch
                {
                    return false; 
                }
            }
        }
    }
}