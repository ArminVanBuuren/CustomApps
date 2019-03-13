using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Timers;
using Utils.AppUpdater.Updater;

namespace Utils.AppUpdater
{
    [Serializable]
    public delegate void UploadBuildHandler(object sender, ApplicationUpdaterProcessingArgs args);

    [Serializable]
    public delegate void BuildUpdaterHandler(object sender, ApplicationUpdaterArgs buildPack);

    public class ApplicationUpdater
    {
        public event BuildUpdaterHandler UpdateOnNewVersion;
        public event UploadBuildHandler OnProcessingError;
        private readonly Timer _stopWatch;
        private BuildUpdaterCollection _newBuildVersions;
        private bool _waitSelfUpdate = false;
        object _lock = new object();

        public Assembly RunningApp { get; }
        public string Project { get; }
        public string PrevPackName { get; }
        public Uri ProjectUri { get; }
        public int UpdateMSec { get; }
        

        public ApplicationUpdater(Assembly runningApp, string projectName, string prevPackName, int updateSec = 10, string uriProject = null)
        {
            RunningApp = runningApp;
            Project = projectName;
            PrevPackName = prevPackName;
            ProjectUri = new Uri(uriProject == null ? Builds.DEFAULT_PROJECT_RAW : uriProject.TrimEnd('/'));
            UpdateMSec = updateSec * 1000;

            _stopWatch = new Timer();
            _stopWatch.Elapsed += TimerCheckVersions;
            _stopWatch.Interval = UpdateMSec;
            _stopWatch.AutoReset = false;
            EnableTimer();
        }

        private void TimerCheckVersions(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                try
                {
                    if(_waitSelfUpdate)
                        return;

                    if (UpdateOnNewVersion == null)
                    {
                        EnableTimer();
                        return;
                    }

                    Uri versionsInfo = new Uri($"{ProjectUri}/{Builds.FILE_NAME}");
                    string contextStr = WEB.WebHttpStringData(versionsInfo, out HttpStatusCode resHttp, HttpRequestCacheLevel.NoCacheNoStore);
                    if (resHttp == HttpStatusCode.OK)
                    {
                        Builds remoteBuilds = Builds.Deserialize(contextStr);

                        _newBuildVersions = new BuildUpdaterCollection(this, remoteBuilds);
                        if (_newBuildVersions.Count > 0 && _newBuildVersions.ProjectBuildPack.Name != PrevPackName)
                        {
                            _newBuildVersions.OnFetchComplete += DeltaList_OnFetchComplete;
                            _newBuildVersions.Fetch();
                        }
                        return;
                    }

                    throw new Exception($"Catched exception when get status from server. HttpStatus=[{resHttp:G}] Uri=[{versionsInfo.AbsoluteUri}]");
                }
                catch (Exception ex)
                {
                    OnProcessingError?.Invoke(this, new ApplicationUpdaterProcessingArgs(ex));
                }

                EnableTimer();
            }
        }


        private void DeltaList_OnFetchComplete(object sender, ApplicationUpdaterProcessingArgs e)
        {
            lock (_lock)
            {
                try
                {
                    if (e.Error != null || _newBuildVersions.Status != UploaderStatus.Fetched)
                    {
                        _newBuildVersions.Dispose();
                        OnProcessingError?.Invoke(this, e.Error == null ? new ApplicationUpdaterProcessingArgs("Faild in upload build pack!", _newBuildVersions) : e);
                        EnableTimer();
                        return;
                    }

                    var eventListeners = UpdateOnNewVersion?.GetInvocationList();
                    if (eventListeners == null || !eventListeners.Any())
                    {
                        _newBuildVersions.Dispose();
                        EnableTimer();
                        return;
                    }

                    ApplicationUpdaterArgs buildArgs = new ApplicationUpdaterArgs(_newBuildVersions);
                    foreach (BuildUpdaterHandler del in eventListeners)
                    {
                        del.Invoke(this, buildArgs);
                        if (buildArgs.Result == UpdateBuildResult.Cancel)
                        {
                            _newBuildVersions.Dispose();
                            EnableTimer();
                            return;
                        }
                    }

                    if (buildArgs.Result == UpdateBuildResult.SelfUpdate)
                    {
                        _waitSelfUpdate = true;
                        return;
                    }

                    _newBuildVersions.CommitAndPull();
                    return;
                }
                catch (Exception ex)
                {
                    OnProcessingError?.Invoke(this, new ApplicationUpdaterProcessingArgs(ex, _newBuildVersions));
                    _newBuildVersions.Dispose();
                }

                EnableTimer();
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                _stopWatch.Stop();
            }
        }

        public void Check()
        {
            lock (_lock)
            {
                if (_stopWatch.Interval != UpdateMSec)
                    _stopWatch.Interval = UpdateMSec;
                if (_stopWatch.Enabled == true)
                    EnableTimer();
            }

            TimerCheckVersions(this, null);
        }

        public void Update()
        {
            lock (_lock)
            {
                if (!_waitSelfUpdate)
                    return;

                try
                {
                    if (_newBuildVersions == null || _newBuildVersions.Count == 0)
                    {
                        OnProcessingError?.Invoke(this, new ApplicationUpdaterProcessingArgs("Internal error. Server builds not initialized.", _newBuildVersions));
                        _waitSelfUpdate = false;
                        EnableTimer();
                        return;
                    }

                    _newBuildVersions.CommitAndPull();
                }
                catch (Exception ex)
                {
                    OnProcessingError?.Invoke(this, new ApplicationUpdaterProcessingArgs(ex, _newBuildVersions));
                    _newBuildVersions.Dispose();
                    _waitSelfUpdate = false;
                    EnableTimer();
                }
            }
        }

        void EnableTimer()
        {
            _stopWatch.Enabled = true;
        }
    }
}