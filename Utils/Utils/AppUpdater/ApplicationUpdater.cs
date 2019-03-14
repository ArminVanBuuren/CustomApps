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
        public event BuildUpdaterHandler OnUpdate;
        public event UploadBuildHandler OnProcessingError;
        private readonly Timer _stopWatch;
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
            ProjectUri = new Uri(uriProject == null ? BuildsInfo.DEFAULT_PROJECT_RAW : uriProject.TrimEnd('/'));
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

                    if (OnUpdate == null)
                    {
                        EnableTimer();
                        return;
                    }

                    Uri versionsInfo = new Uri($"{ProjectUri}/{BuildsInfo.FILE_NAME}");
                    string contextStr = WEB.WebHttpStringData(versionsInfo, out HttpStatusCode resHttp, HttpRequestCacheLevel.NoCacheNoStore);
                    if (resHttp == HttpStatusCode.OK)
                    {
                        BuildsInfo remoteBuilds = BuildsInfo.Deserialize(contextStr);

                        BuildUpdaterCollection projectRemoteBuilds = new BuildUpdaterCollection(this, remoteBuilds);
                        if (projectRemoteBuilds.Count > 0 && projectRemoteBuilds.ProjectBuildPack.Name != PrevPackName)
                        {
                            projectRemoteBuilds.OnFetchComplete += DeltaList_OnFetchComplete;
                            projectRemoteBuilds.Fetch();
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
                    if (e.Error != null || e.Control.Status != UploaderStatus.Fetched)
                    {
                        OnProcessingError?.Invoke(this, e.Error == null ? new ApplicationUpdaterProcessingArgs("Error fetching build pack!", e.Control) : e);
                        e.Control.Dispose();
                        EnableTimer();
                        return;
                    }

                    var eventListeners = OnUpdate?.GetInvocationList();
                    if (eventListeners == null || !eventListeners.Any())
                    {
                        e.Control.Dispose();
                        EnableTimer();
                        return;
                    }

                    ApplicationUpdaterArgs buildArgs = new ApplicationUpdaterArgs(e.Control);
                    foreach (BuildUpdaterHandler del in eventListeners)
                    {
                        del.Invoke(this, buildArgs);
                        if (buildArgs.Result == UpdateBuildResult.Cancel)
                        {
                            e.Control.Dispose();
                            EnableTimer();
                            return;
                        }
                    }

                    if (buildArgs.Result == UpdateBuildResult.SelfUpdate)
                    {
                        _waitSelfUpdate = true;
                        return;
                    }

                    ((BuildUpdaterCollection)e.Control).CommitAndPull();
                    return;
                }
                catch (Exception ex)
                {
                    OnProcessingError?.Invoke(this, new ApplicationUpdaterProcessingArgs(ex, e.Control));
                    e.Control.Dispose();
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

        /// <summary>
        /// Если при вызове эвента OnUpdate, был выбран статус - UpdateBuildResult.SelfUpdate, то контрольный процесс обязуется выполнить апдейт самостоятельно, при завершении своих внутренних процессов. При это проверка на наличия обновлений останавливается.
        /// </summary>
        /// <param name="control"></param>
        public void Update(IUpdater control)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            if (control.Count == 0)
                throw new ArgumentException("No builds found for update.");

            lock (_lock)
            {
                try
                {
                    if (!_waitSelfUpdate)
                        return;

                    _waitSelfUpdate = false;
                    ((BuildUpdaterCollection) control).CommitAndPull();
                }
                catch (Exception ex)
                {
                    OnProcessingError?.Invoke(this, new ApplicationUpdaterProcessingArgs(ex, control));
                    control.Dispose();
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