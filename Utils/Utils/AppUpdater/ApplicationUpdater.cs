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

    public enum AutoUpdaterStatus
    {
        Stopped = 0,
        Working = 1
    }

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
        public int CheckUpdatesEachMSec { get; }
        public AutoUpdaterStatus Status { get; private set; } = AutoUpdaterStatus.Stopped;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runningApp">Основное запущенное приложение (необходимо для того чтобы его перезагрузить в случае обновления)</param>
        /// <param name="projectName">Имя проекта</param>
        /// <param name="prevPackName">Предыдущее имя пакета обновлений</param>
        /// <param name="checkUpdatesIntervalSec">Интервал проверка наличия новых билдов каждые (5 минут - дефолтное)</param>
        /// <param name="uriProject">URI пути к удаленному серверу для выкачивания обновления</param>
        public ApplicationUpdater(Assembly runningApp, string projectName, string prevPackName, int checkUpdatesIntervalSec = 300, string uriProject = null)
        {
            RunningApp = runningApp;
            Project = projectName;
            PrevPackName = prevPackName;
            ProjectUri = new Uri(uriProject == null ? BuildsInfo.DEFAULT_PROJECT_RAW : uriProject.TrimEnd('/'));
            CheckUpdatesEachMSec = checkUpdatesIntervalSec * 1000;

            _stopWatch = new Timer();
            _stopWatch.Elapsed += TimerCheckVersions;
            _stopWatch.Interval = CheckUpdatesEachMSec;
            _stopWatch.AutoReset = false;
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
                    }
                    else
                    {
                        throw new Exception($"Catched exception when get status from server. HttpStatus=[{resHttp:G}] Uri=[{versionsInfo.AbsoluteUri}]");
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

        /// <summary>
        /// Запустить автопроверку наличия обновления
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                Status = AutoUpdaterStatus.Working;
                EnableTimer();
            }
        }

        void EnableTimer()
        {
            if (Status == AutoUpdaterStatus.Working)
                _stopWatch.Enabled = true;
        }

        /// <summary>
        /// Остановить автопроверку наличия обновления
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                Status = AutoUpdaterStatus.Stopped;
                _stopWatch.Stop();
            }
        }

        /// <summary>
        /// Проверить наличие обновлений
        /// </summary>
        public void Check()
        {
            lock (_lock)
            {
                if (Status == AutoUpdaterStatus.Working)
                {
                    if (!_stopWatch.Enabled)
                        return;
                    if (_stopWatch.Interval != CheckUpdatesEachMSec)
                        _stopWatch.Interval = CheckUpdatesEachMSec;
                }
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

            if(!(control is BuildUpdaterCollection))
                throw new ArgumentException("control is incorrect.");

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
    }
}