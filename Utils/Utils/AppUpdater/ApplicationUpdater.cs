using System;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Timers;
using Microsoft.Win32;
using Utils.AppUpdater.Pack;
using Utils.AppUpdater.Updater;
using Utils.Handles;
using Utils.Properties;

namespace Utils.AppUpdater
{
    [Serializable]
    public enum AutoUpdaterStatus
    {
        /// <summary>
        /// пользователь сам может проверить наличе обновления без автоматической проверки
        /// </summary>
        Stopped = 0,
        /// <summary>
        /// автоматическая проверка обновлений
        /// </summary>
        Checking = 1,
        /// <summary>
        /// Началась закачка пакета обновлений или выполняется обновление
        /// </summary>
        Processing = 2
    }

    public class ApplicationUpdater
    {
        private readonly object syncStatus = new object();
        private readonly object syncChecking = new object();
        private readonly object syncUpdating = new object();
        private readonly Timer _watcher;
        private readonly AssemblyInfo _assemblyInfo;
        private ILogger _logger;

        protected ILogger Logger => _logger ?? (_logger = GetLogger());

        private int _httpRequestTimeoutSeconds = 100;

        [NonSerialized]
        private readonly IUpdaterProject _updaterProject;

        private AutoUpdaterStatus _satus = AutoUpdaterStatus.Stopped;

        public event AppFetchingHandler OnFetch;
        public event AppUpdaterHandler OnUpdate;
        public event AppUpdaterHandler OnSuccessfulUpdated;
        public event AppUpdaterErrorHandler OnProcessingError;

        public Assembly RunningApp => _assemblyInfo.CurrentAssembly;

        public string ProjectName { get; }

        public IUpdaterProject UpdaterProject { get; }

        public int CheckUpdatesIntervalMinutes { get; }

        public int HttpRequestTimeoutSeconds
        {
            get => _httpRequestTimeoutSeconds;
            set
            {
                if(value <= 0)
                    return;
                _httpRequestTimeoutSeconds = value;
            }
        }

        public AutoUpdaterStatus Status
        {
            get
            {
                lock (syncStatus)
                    return _satus;
            }
            private set
            {
                switch (value)
                {
                    case AutoUpdaterStatus.Processing:
                    case AutoUpdaterStatus.Stopped:
                        if (_watcher.Enabled)
                            _watcher.Stop();
                        break;
                    case AutoUpdaterStatus.Checking:
                        _watcher.Interval = CheckUpdatesIntervalMinutes;
                        if (!_watcher.Enabled)
                            _watcher.Enabled = true;
                        break;
                }

                _satus = value;
            }
        }

        private BuildPackUpdater LastUpdateInfo
        {
            get
            {
                try
                {
                    using (var reg = new RegeditControl(_assemblyInfo.ApplicationName))
                    {
                        var obj = reg[nameof(LastUpdateInfo)];
                        if (obj is byte[] array)
                            return BuildPackUpdater.Deserialize(array);

                        return null;
                    }
                }
                catch (Exception ex)
                {
	                Logger.LogWriteError(ex);
	                return null;
                }
            }
            set
            {
                try
                {
                    using (var reg = new RegeditControl(_assemblyInfo.ApplicationName))
                        reg[nameof(LastUpdateInfo), RegistryValueKind.Binary] = value?.SerializeToStreamOfBytes();
                }
                catch (Exception ex)
                {
	                Logger.LogWriteError(ex);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runningApp">Основное запущенное приложение (необходимо для того чтобы его перезагрузить в случае обновления)</param>
        /// <param name="checkUpdatesIntervalMinutes">Интервал проверка наличия новых билдов каждые (5 минут - дефолтное)</param>
        /// <param name="updaterProject">Проект к удаленному серверу для скачивания обновления</param>
        public ApplicationUpdater(Assembly runningApp, int checkUpdatesIntervalMinutes = 10, IUpdaterProject updaterProject = null)
        {
	        Logger.LogWriteInfo("Startup");

            if (runningApp == null)
                throw new ArgumentNullException(nameof(runningApp));
            if (checkUpdatesIntervalMinutes <= 0)
                throw new ArgumentException(Resources.ApplicationUpdater_ErrUpdateInterval);

            _assemblyInfo = new AssemblyInfo(runningApp);
            ProjectName = RunningApp.GetName().Name;
            UpdaterProject = updaterProject;
            _updaterProject = UpdaterProject ?? new UpdaterProjectSimple();
            CheckUpdatesIntervalMinutes = checkUpdatesIntervalMinutes * 1000 * 60;

            _watcher = new Timer {Enabled = false};
            _watcher.Elapsed += CheckUpdates;
            _watcher.Interval = CheckUpdatesIntervalMinutes;
            _watcher.AutoReset = false;
        }

        private void CheckUpdates(object sender, ElapsedEventArgs e)
        {
            lock (syncChecking)
            {
                try
                {
                    if (Status == AutoUpdaterStatus.Processing)
                        return;

                    var contextStr = WEB.WebHttpStringData(_updaterProject.BuildsInfoUri, out var responceHttpCode, HttpRequestCacheLevel.NoCacheNoStore, HttpRequestTimeoutSeconds * 1000);
                    if (responceHttpCode == HttpStatusCode.OK)
                    {
                        var remoteBuilds = BuildsInfo.Deserialize(contextStr);
                        var buildPack = remoteBuilds.Packs.FirstOrDefault(p => p.ProjectName == ProjectName);
                        if (buildPack?.Builds.Count > 0)
                        {
                            var control = GetBuildPackUpdater(buildPack);
                            if (control.Count > 0)
                            {
                                var responce = GetResponseFromControlObject(OnFetch, new ApplicationFetchingArgs(control));
                                if (responce.Result == UpdateBuildResult.Fetch)
                                {
                                    ProcessingStatus(); // устанавливаем Status = AutoUpdaterStatus.Processing, т.к. следующим шагом начнется процесс скачивания и обновления

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
                        throw new Exception(string.Format(Resources.ApplicationUpdater_ErrServerGetStatus, responceHttpCode, UpdaterProject?.BuildsInfoUri?.AbsoluteUri ?? "Default"));
                    }
                }
                catch (Exception ex)
                {
	                Logger.LogWriteError(ex);
                    SendError(new ApplicationUpdaterArgs(null, ex));
                }
                finally
                {
                    ReinitStatusToChecking();
                }
            }
        }

        protected virtual BuildPackUpdaterBase GetBuildPackUpdater(BuildPackInfo buildPack)
        {
	        return new BuildPackUpdaterSimple(RunningApp, buildPack, _updaterProject, Logger);
        }

        ApplicationFetchingArgs GetResponseFromControlObject(AppFetchingHandler eventReference, ApplicationFetchingArgs args)
        {
            var eventListeners = eventReference?.GetInvocationList();
            if (eventListeners == null || !eventListeners.Any())
                return args;

            foreach (AppFetchingHandler del in eventListeners)
            {
                try
                {
                    del.Invoke(this, args);
                    if (args.Result == UpdateBuildResult.Cancel)
                        return args;
                }
                catch (Exception ex)
                {
	                Logger.LogWriteError(ex);
                }
            }

            return args;
        }

        private void FetchingCompleted(object sender, UpdaterProcessingArgs e)
        {
            BuildPackUpdaterBase control = null;
            try
            {
                control = (BuildPackUpdaterBase) sender;
                control.OnFetchComplete -= FetchingCompleted;

                if (e.Error != null || control.Status != UploaderStatus.Fetched)
                {
                    SendError(new ApplicationUpdaterArgs(control, e.Error, Resources.ApplicationUpdater_ErrFetch));
                    control.Dispose();
                    ReturnBackStatus(); // если возникли какие то ошибки при скачивании пакета с обновлениями
                }
                else if (OnUpdate != null)
                {
                    SendMessage(OnUpdate, new ApplicationUpdaterArgs(control));
                }
                else
                {
                    DoUpdate(control);
                }
            }
            catch (Exception ex)
            {
	            Logger.LogWriteError(ex);
                SendError(new ApplicationUpdaterArgs(control, ex, Resources.ApplicationUpdater_ErrInstall));
                control?.Dispose();
                ReturnBackStatus();
            }
        }

        /// <summary>
        /// Запустить автопроверку наличия обновления
        /// </summary>
        public bool Start()
        {
            if (Status == AutoUpdaterStatus.Processing)
                return false;

            // если был в стопе или в чеке, то принудительно переводим в проверку
            lock (syncStatus)
                Status = AutoUpdaterStatus.Checking;

            CheckOnSuccessfulUpdated();
            CheckUpdates();

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
            new Action<object, ElapsedEventArgs>(CheckUpdates).BeginInvoke(null, null, null, null);

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
            lock (syncStatus)
                Status = AutoUpdaterStatus.Stopped;
        }

        private AutoUpdaterStatus _prevStatusUpdate = AutoUpdaterStatus.Stopped;

        /// <summary>
        /// Остановить автопроверку наличия обновления
        /// </summary>
        void ProcessingStatus()
        {
            if (Status == AutoUpdaterStatus.Processing)
                return;

            lock (syncStatus)
            {
                _prevStatusUpdate = Status;
                Status = AutoUpdaterStatus.Processing;
            }
        }

        /// <summary>
        /// Запустить автопроверку наличия обновления
        /// </summary>
        void ReturnBackStatus()
        {
            lock (syncStatus)
                Status = _prevStatusUpdate;
        }

        void ReinitStatusToChecking()
        {
            if (Status == AutoUpdaterStatus.Checking)
                lock (syncStatus)
                    Status = AutoUpdaterStatus.Checking;
        }

        /// <summary>
        /// Статус обязательно должен быть Processing иначе он не ожидает обновление
        /// </summary>
        public void DoUpdate(BuildPackUpdater control)
        {
            lock (syncUpdating)
            {
                if (control == null)
                    throw new ArgumentNullException(nameof(control));

                if (control.Count == 0)
                    throw new ArgumentException(Resources.ApplicationUpdater_NoBuildsFound);

                if (!(control is BuildPackUpdaterBase updater))
                    throw new ArgumentException(string.Format(Resources.ApplicationUpdater_IncorrectControl, control.GetType()));

                if (Status == AutoUpdaterStatus.Checking)
                    throw new Exception(string.Format(Resources.ApplicationUpdater_NotExpectedOnChecking, nameof(ApplicationUpdater)));

                if (control.Status != UploaderStatus.Fetched)
                    throw new Exception(Resources.ApplicationUpdater_NotFetched);

                try
                {
                    if (updater.Commit())
                    {
                        LastUpdateInfo = updater;
                        if (updater.Pull())
                            CheckOnSuccessfulUpdated();
                        else
                            throw new Exception(Resources.UnableExecutePull);
                    }
                    else
                    {
                        throw new Exception(Resources.UnableExecuteCommit);
                    }
                }
                catch (Exception ex)
                {
	                Logger.LogWriteError(ex);
                    SendError(new ApplicationUpdaterArgs(updater, ex, Resources.ApplicationUpdater_ErrInstall));
                    updater.Dispose();
                }
                finally
                {
                    ReturnBackStatus(); // если произошло обновление без рестарта приложения то возвращаем статус обратно
                }
            }
        }

        /// <summary>
        /// Проверка на успешность обновления по предыдущему паку
        /// </summary>
        void CheckOnSuccessfulUpdated()
        {
            var remoteControl = LastUpdateInfo;
            if (remoteControl == null)
                return;

            try
            {
                var localControl = GetBuildPackUpdater(remoteControl.BuildPack);
                if (localControl.Count == 0)
                {
                    SendMessage(OnSuccessfulUpdated, new ApplicationUpdaterArgs(remoteControl));
                    LastUpdateInfo = null;
                }
            }
            catch (Exception ex)
            {
                SendError(new ApplicationUpdaterArgs(remoteControl, ex));
            }
        }

        void SendMessage(AppUpdaterHandler handler, ApplicationUpdaterArgs args)
        {
            try
            {
                handler?.Invoke(this, args);
            }
            catch (Exception ex)
            {
	            Logger.LogWriteError(ex);
            }
        }

        void SendError(ApplicationUpdaterArgs args)
        {
            try
            {
                OnProcessingError?.Invoke(this, args);
            }
            catch (Exception ex)
            {
	            Logger.LogWriteError(ex);
            }
        }

        protected virtual ILogger GetLogger() => new Logger(nameof(ApplicationUpdater));

        public override string ToString() => $"[{nameof(ApplicationUpdater)}] Project = \"{ProjectName}\" Status = \"{Status:G}\"";
    }
}