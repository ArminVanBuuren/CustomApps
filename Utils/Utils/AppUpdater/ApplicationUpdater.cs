using System;
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
        Checking = 1,
        Processing = 2
    }

    [Serializable]
    public delegate void AppUpdaterHandler(object sender, ApplicationUpdaterProcessingArgs args);

    [Serializable]
    public delegate void AppUpdaterErrorHandler(object sender, ApplicationUpdaterProcessingArgs args);

    public class ApplicationUpdater
    {
        public event AppUpdaterHandler OnFetch;
        public event AppUpdaterHandler OnUpdate;
        public event AppUpdaterErrorHandler OnProcessingError;
        private readonly Timer _watcher;
        object sync = new object();
        private AutoUpdaterStatus _satus = AutoUpdaterStatus.Stopped;

        public Assembly RunningApp { get; }
        public string ProjectName { get; }
        public string PrevPackName { get; }
        public Uri ProjectUri { get; }
        public int CheckUpdatesIntervalSec { get; }

        public AutoUpdaterStatus Status
        {
            get
            {
                lock (sync)
                    return _satus;
            }
            private set
            {
                lock (sync)
                {
                    switch (value)
                    {
                        case AutoUpdaterStatus.Processing:
                        case AutoUpdaterStatus.Stopped:
                            if (_watcher.Enabled)
                                _watcher.Stop();
                            break;
                        case AutoUpdaterStatus.Checking:
                            _watcher.Interval = CheckUpdatesIntervalSec * 1000;
                            if (!_watcher.Enabled)
                                _watcher.Enabled = true;
                            break;
                    }

                    _satus = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runningApp">Основное запущенное приложение (необходимо для того чтобы его перезагрузить в случае обновления)</param>
        /// <param name="prevPackName">Предыдущее имя пакета обновлений</param>
        /// <param name="checkUpdatesIntervalSec">Интервал проверка наличия новых билдов каждые (5 минут - дефолтное)</param>
        /// <param name="uriProject">URI пути к удаленному серверу для выкачивания обновления</param>
        public ApplicationUpdater(Assembly runningApp, string prevPackName, int checkUpdatesIntervalSec = 300, string uriProject = null)
        {
            if(runningApp == null)
                throw new ArgumentNullException(nameof(runningApp));
            if (checkUpdatesIntervalSec <= 0)
                throw new ArgumentException($"Check update interval must be more than one second.");

            RunningApp = runningApp;
            ProjectName = RunningApp.GetName().Name;
            PrevPackName = prevPackName;
            ProjectUri = new Uri(uriProject == null ? BuildsInfo.DEFAULT_PROJECT_RAW : uriProject.TrimEnd('/'));
            CheckUpdatesIntervalSec = checkUpdatesIntervalSec;

            _watcher = new Timer();
            _watcher.Elapsed += CheckUpdates;
            _watcher.Interval = CheckUpdatesIntervalSec * 1000;
            _watcher.AutoReset = false;
        }

        private void CheckUpdates(object sender, ElapsedEventArgs e)
        {
            try
            {
                Uri versionsInfo = new Uri($"{ProjectUri}/{BuildsInfo.FILE_NAME}");
                string contextStr = WEB.WebHttpStringData(versionsInfo, out HttpStatusCode resHttp, HttpRequestCacheLevel.NoCacheNoStore);
                if (resHttp == HttpStatusCode.OK)
                {
                    BuildsInfo remoteBuilds = BuildsInfo.Deserialize(contextStr);
                    BuildPackInfo projectBuildPack = remoteBuilds.Packs.FirstOrDefault(p => p.Project == ProjectName);
                    //Dictionary<string, FileBuildInfo> serverVersions = ProjectBuildPack.ToDictionary(x => x.Location, x => x);
                    if (projectBuildPack?.Name != PrevPackName && projectBuildPack?.Builds.Count > 0)
                    {
                        BuildUpdaterCollection control = new BuildUpdaterCollection(RunningApp, ProjectUri, projectBuildPack);
                        if (control.Count > 0)
                        {
                            ApplicationUpdaterProcessingArgs responce = GetResponseFromControlObject(OnFetch, new ApplicationUpdaterProcessingArgs(control));
                            if (responce.Result == UpdateBuildResult.Update)
                            {
                                ProcessingStatus();
                                control.OnFetchComplete += FetchingCompleted;
                                control.Fetch();
                                return;
                            }
                        }

                        control.Dispose();
                    }
                }
                else
                {
                    throw new Exception($"Exception when get status from server. HttpStatus=[{resHttp:G}] Uri=[{versionsInfo.AbsoluteUri}]");
                }
            }
            catch (Exception ex)
            {
                OnProcessingError?.Invoke(this, new ApplicationUpdaterProcessingArgs(null, ex));
            }

            ReinitStatusToChecking();
        }

        private void FetchingCompleted(object sender, BuildUpdaterProcessingArgs e)
        {
            BuildUpdaterCollection control = null;
            try
            {
                control = (BuildUpdaterCollection)sender;
                if (e.Error != null || control.Status != UploaderStatus.Fetched)
                {
                    OnProcessingError?.Invoke(this, new ApplicationUpdaterProcessingArgs(control, e.Error, "Error when fetching pack of builds!"));
                    control.Dispose();
                    ReturnBackStatus();
                }
                else
                {
                    ApplicationUpdaterProcessingArgs responce = GetResponseFromControlObject(OnUpdate, new ApplicationUpdaterProcessingArgs(control));
                    if (responce.Result == UpdateBuildResult.Update)
                    {
                        control.CommitAndPull();
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessingError?.Invoke(this, new ApplicationUpdaterProcessingArgs(control, ex, "Error when trying commit and pull!"));
                control?.Dispose();
                ReturnBackStatus();
            }
        }

        ApplicationUpdaterProcessingArgs GetResponseFromControlObject(AppUpdaterHandler eventReference, ApplicationUpdaterProcessingArgs args)
        {
            var eventListeners = eventReference?.GetInvocationList();
            if (eventListeners == null || !eventListeners.Any())
            {
                return args;
            }

            foreach (AppUpdaterHandler del in eventListeners)
            {
                del.Invoke(this, args);
                if (args.Result == UpdateBuildResult.Cancel)
                {
                    return args;
                }
            }

            return args;
        }


        /// <summary>
        /// Запустить автопроверку наличия обновления
        /// </summary>
        public bool Start()
        {
            if (Status == AutoUpdaterStatus.Processing)
                return false;

            // если был в стопе или в чеке, то принудительно переводим в проверку
            Status = AutoUpdaterStatus.Checking;
            // если таймер на полуяение обновлений больше 20 секунд, то выполняем проверку на наличие обновлений сразу
            if (CheckUpdatesIntervalSec > 20)
                new Action<object, ElapsedEventArgs>(CheckUpdates).BeginInvoke(this, null, null, null);
            return true;
        }

        /// <summary>
        /// Проверить наличие обновлений
        /// </summary>
        public bool CheckUpdates()
        {
            if (Status == AutoUpdaterStatus.Processing)
                return false;

            // если был в стопе то не меняем статус, а если в проверке то реинитем
            ReinitStatusToChecking();
            // если таймер на полуяение обновлений больше 20 секунд, то выполняем проверку на наличие обновлений сразу
            if (CheckUpdatesIntervalSec > 20)
                new Action<object, ElapsedEventArgs>(CheckUpdates).BeginInvoke(this, null, null, null);
            return true;
        }

        /// <summary>
        /// Переводит статус из процессинга в начальное состояние, состояние которые было до процессинга
        /// </summary>
        public void Refresh()
        {
            ReturnBackStatus();
        }

        /// <summary>
        /// Остановить автопроверку наличия обновления
        /// </summary>
        public void Stop()
        {
            Status = AutoUpdaterStatus.Stopped;
        }

        private AutoUpdaterStatus _prevStatusUpdate = AutoUpdaterStatus.Stopped;

        /// <summary>
        /// Остановить автопроверку наличия обновления
        /// </summary>
        void ProcessingStatus()
        {
            lock (sync)
            {
                if (Status == AutoUpdaterStatus.Processing)
                    return;
                _prevStatusUpdate = Status;
                Status = AutoUpdaterStatus.Processing;
            }
        }

        /// <summary>
        /// Запустить автопроверку наличия обновления
        /// </summary>
        void ReturnBackStatus()
        {
            lock (sync)
            {
                Status = _prevStatusUpdate;
            }
        }

        void ReinitStatusToChecking()
        {
            if (Status == AutoUpdaterStatus.Checking)
                Status = AutoUpdaterStatus.Checking;
        }

        /// <summary>
        /// Статус обязательно должен быть Processing иначе он не ожидает обновление
        /// </summary>
        public void DoUpdate(IUpdater control)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            if (control.Count == 0)
                throw new ArgumentException("No builds found for update.");

            if (!(control is BuildUpdaterCollection))
                throw new ArgumentException($"Incoming control '{nameof(IUpdater)}' is incorrect.");

            if(Status != AutoUpdaterStatus.Processing || control.Status != UploaderStatus.Fetched)
                throw new Exception($"{nameof(ApplicationUpdater)} does not expect an update.");


            try
            {
                ((BuildUpdaterCollection)control).CommitAndPull();
            }
            catch (Exception ex)
            {
                OnProcessingError?.Invoke(this, new ApplicationUpdaterProcessingArgs(control, ex, "Error when trying commit and pull!"));
                control?.Dispose();
                ReturnBackStatus();
            }
        }

        public override string ToString()
        {
            return $"{nameof(ApplicationUpdater)} ProjectName=[{ProjectName}] Status=[{Status:G}] Version=[{RunningApp.GetName().Version}]";
        }
    }
}