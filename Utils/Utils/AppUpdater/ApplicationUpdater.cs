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
    public enum AutoUpdaterStatus
    {
        Stopped = 0,
        Working = 1
    }

    [Serializable]
    public delegate void UploadBuildHandler(object sender, ApplicationUpdaterProcessingArgs args);

    [Serializable]
    public delegate void BuildUpdaterHandler(object sender, ApplicationUpdaterArgs buildPack);

    public class ApplicationUpdater
    {
        public event BuildUpdaterHandler OnFetch;
        public event BuildUpdaterHandler OnUpdate;
        public event UploadBuildHandler OnProcessingError;
        private readonly Timer _stopWatch;
        object sync = new object();

        public Assembly RunningApp { get; }
        public string ProjectName { get; }
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
            ProjectName = projectName;
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
            try
            {
                Uri versionsInfo = new Uri($"{ProjectUri}/{BuildsInfo.FILE_NAME}");
                string contextStr = WEB.WebHttpStringData(versionsInfo, out HttpStatusCode resHttp, HttpRequestCacheLevel.NoCacheNoStore);
                if (resHttp == HttpStatusCode.OK)
                {
                    BuildsInfo remoteBuilds = BuildsInfo.Deserialize(contextStr);
                    BuildUpdaterCollection control = new BuildUpdaterCollection(RunningApp, ProjectUri, ProjectName, remoteBuilds);

                    if (control.Count > 0 && control.ProjectBuildPack.Name != PrevPackName)
                    {
                        ApplicationUpdaterArgs responce = GetResponseFromControlObject(OnFetch, control);
                        if (responce == null || responce.Result == UpdateBuildResult.Update)
                        {
                            control.OnFetchComplete += DeltaList_OnFetchComplete;
                            control.Fetch();
                            return;
                        }
                    }

                    control.Dispose();
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

        private void DeltaList_OnFetchComplete(object sender, ApplicationUpdaterProcessingArgs e)
        {
            if (sender == null || !(sender is BuildUpdaterCollection))
            {
                OnProcessingError?.Invoke(sender, new ApplicationUpdaterProcessingArgs($"Internal error. Input object is not [{nameof(BuildUpdaterCollection)}]"));
                EnableTimer();
                return;
            }

            BuildUpdaterCollection control = (BuildUpdaterCollection) sender;

            try
            {
                if (e.Error != null || control.Status != UploaderStatus.Fetched)
                {
                    OnProcessingError?.Invoke(sender, e.Error == null ? new ApplicationUpdaterProcessingArgs("Error when fetching pack of builds!") : e);
                    control.Dispose();
                }
                else
                {
                    ApplicationUpdaterArgs responce = GetResponseFromControlObject(OnUpdate, control);
                    if (responce == null || responce.Result == UpdateBuildResult.Update)
                    {
                        control.CommitAndPull();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessingError?.Invoke(sender, new ApplicationUpdaterProcessingArgs(ex));
                control.Dispose();
            }

            //диспоузить control нельзя т.к. его будут потом использовать в качестве обновления вручную
            EnableTimer();
        }

        static ApplicationUpdaterArgs GetResponseFromControlObject(BuildUpdaterHandler eventReference, IUpdater updater)
        {
            var eventListeners = eventReference?.GetInvocationList();
            if (eventListeners == null || !eventListeners.Any())
            {
                return null;
            }

            ApplicationUpdaterArgs buildArgs = new ApplicationUpdaterArgs();
            foreach (BuildUpdaterHandler del in eventListeners)
            {
                del.Invoke(updater, buildArgs);
                if (buildArgs.Result == UpdateBuildResult.Cancel)
                {
                    return buildArgs;
                }
            }

            return buildArgs;
        }

        /// <summary>
        /// Запустить автопроверку наличия обновления
        /// </summary>
        public void Start()
        {
            lock (sync)
                Status = AutoUpdaterStatus.Working;
            EnableTimer();
        }

        void EnableTimer()
        {
            lock (sync)
            {
                if (Status == AutoUpdaterStatus.Working)
                {
                    _stopWatch.Interval = CheckUpdatesEachMSec;
                    _stopWatch.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Остановить автопроверку наличия обновления
        /// </summary>
        public void Stop()
        {
            lock (sync)
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
            lock (sync)
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
        public bool DoUpdate(IUpdater control)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            if (control.Count == 0)
                throw new ArgumentException("No builds found for update.");

            if (!(control is BuildUpdaterCollection))
                throw new ArgumentException("control is incorrect.");


            return ((BuildUpdaterCollection)control).CommitAndPull();
        }
    }
}