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
        Update = 1,
        SelfUpdate = 2
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
        private readonly Uri _uriToServerProject;
        private readonly Assembly _runningApp;
        private BuildPackCollection _newBuildVersions;
        private readonly int _updateMSec;
        private bool _waitSelfUpdate = false;
        object _lock = new object();

        public BuildUpdater(Assembly runningApp, string uriProject, int updateSec = 10)
        {
            _runningApp = runningApp;
            _uriToServerProject = new Uri(uriProject.TrimEnd('/'));
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
                    if(_waitSelfUpdate)
                        return;

                    if (UpdateOnNewVersion == null)
                    {
                        EnableTimer();
                        return;
                    }


                    _newBuildVersions = new BuildPackCollection(_runningApp, _uriToServerProject);
                    if (_newBuildVersions.Count > 0)
                    {
                        _newBuildVersions.OnFetchComplete += DeltaList_OnFetchComplete;
                        _newBuildVersions.Fetch();
                        return;
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
                    if (sender == null || _newBuildVersions == null || !(sender is BuildPackCollection) || ((BuildPackCollection) sender) != _newBuildVersions)
                        return;

                    if (!_newBuildVersions.IsUploaded || e.Error != null)
                    {
                        _newBuildVersions.RemoveTempFiles();
                        OnProcessingError?.Invoke(this, e.Error == null ? new BuildUpdaterProcessingArgs("Not all files were successfully upload!", _newBuildVersions) : e);
                        EnableTimer();
                        return;
                    }

                    var eventListeners = UpdateOnNewVersion?.GetInvocationList();
                    if (eventListeners == null || !eventListeners.Any())
                    {
                        EnableTimer();
                        return;
                    }

                    BuildUpdaterArgs buildArgs = new BuildUpdaterArgs(_newBuildVersions);
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
                    OnProcessingError?.Invoke(this, new BuildUpdaterProcessingArgs(ex, _newBuildVersions));
                }

                EnableTimer();
            }
        }

        public void SelfUpdate()
        {
            lock (_lock)
            {
                if (_waitSelfUpdate)
                {
                    try
                    {
                        if (_newBuildVersions == null || _newBuildVersions.Count == 0)
                        {
                            OnProcessingError?.Invoke(this, new BuildUpdaterProcessingArgs("Internal error. Server builds not initialized.", _newBuildVersions));
                            _waitSelfUpdate = false;
                            EnableTimer();
                            return;
                        }
                        _newBuildVersions.Commit();
                    }
                    catch (Exception ex)
                    {
                        OnProcessingError?.Invoke(this, new BuildUpdaterProcessingArgs(ex, _newBuildVersions));
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