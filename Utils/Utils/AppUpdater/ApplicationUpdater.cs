using System;
using System.Collections.Generic;
using System.Linq;
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

        private Assembly RunningApp { get; }
        public string ProjectName { get; }
        private Uri ProjectUri { get; }
        private int UpdateMSec { get; }
        

        public ApplicationUpdater(Assembly runningApp, string projectName, int updateSec = 10, string uriProject = null)
        {
            RunningApp = runningApp;
            ProjectName = projectName;
            ProjectUri = new Uri(uriProject == null ? Builds.DEFAULT_PROJECT_URI : uriProject.TrimEnd('/'));
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


                    _newBuildVersions = new BuildUpdaterCollection(RunningApp, ProjectUri, ProjectName);
                    if (_newBuildVersions.Count > 0)
                    {
                        _newBuildVersions.OnFetchComplete += DeltaList_OnFetchComplete;
                        _newBuildVersions.Fetch();
                        return;
                    }
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
                    if (sender == null || _newBuildVersions == null || !(sender is BuildUpdaterCollection) || ((BuildUpdaterCollection) sender) != _newBuildVersions)
                        return;

                    if (!_newBuildVersions.IsUploaded || e.Error != null)
                    {
                        _newBuildVersions.RemoveTempFiles();
                        OnProcessingError?.Invoke(this, e.Error == null ? new ApplicationUpdaterProcessingArgs("Not all files were successfully upload!", _newBuildVersions) : e);
                        EnableTimer();
                        return;
                    }

                    var eventListeners = UpdateOnNewVersion?.GetInvocationList();
                    if (eventListeners == null || !eventListeners.Any())
                    {
                        EnableTimer();
                        return;
                    }

                    ApplicationUpdaterArgs buildArgs = new ApplicationUpdaterArgs(_newBuildVersions);
                    foreach (BuildUpdaterHandler del in eventListeners)
                    {
                        del.Invoke(this, buildArgs);
                        if (buildArgs.Result == UpdateBuildResult.Cancel)
                        {
                            _newBuildVersions.RemoveTempFiles();
                            EnableTimer();
                            return;
                        }
                    }

                    if (buildArgs.Result == UpdateBuildResult.SelfUpdate)
                    {
                        _waitSelfUpdate = true;
                        return;
                    }

                    _newBuildVersions.Commit();
                    return;
                }
                catch (Exception ex)
                {
                    OnProcessingError?.Invoke(this, new ApplicationUpdaterProcessingArgs(ex, _newBuildVersions));
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
                if (_waitSelfUpdate)
                {
                    try
                    {
                        if (_newBuildVersions == null || _newBuildVersions.Count == 0)
                        {
                            OnProcessingError?.Invoke(this, new ApplicationUpdaterProcessingArgs("Internal error. Server builds not initialized.", _newBuildVersions));
                            _waitSelfUpdate = false;
                            EnableTimer();
                            return;
                        }
                        _newBuildVersions.Commit();
                    }
                    catch (Exception ex)
                    {
                        OnProcessingError?.Invoke(this, new ApplicationUpdaterProcessingArgs(ex, _newBuildVersions));
                        _waitSelfUpdate = false;
                        EnableTimer();
                    }
                }
            }
        }

        void EnableTimer()
        {
            _stopWatch.Enabled = true;
        }
    }
}