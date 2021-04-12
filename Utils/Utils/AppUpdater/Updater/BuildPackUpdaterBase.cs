using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Utils.AppUpdater.Pack;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    public delegate void UpdaterFetchHandler(object sender, UpdaterProcessingArgs args);

    [Serializable]
    public abstract class BuildPackUpdaterBase : BuildPackUpdater, IDisposable
    {        
        /// <summary>
        /// Когда новые версии билдов скачались на локальный диск в темповую папку. Либо загрузка завершилась неудачей
        /// </summary>
        [field: NonSerialized]
        public abstract event UpdaterFetchHandler OnFetchComplete;

        protected BuildPackUpdaterBase(Assembly runningApp, 
                                       BuildPackInfo buildPack,
                                       ILogger logger) :base(runningApp, buildPack, logger) { }

        public abstract void Fetch();

        public virtual bool Commit()
        {
            if (Status != UploaderStatus.Fetched)
                return false;

            foreach (BuildUpdaterBase build in this)
            {
                if (build.IsExecutable)
                    continue;

                build.Commit();
            }

            Status = UploaderStatus.Commited;
            return true;
        }

        public virtual bool Pull()
        {
            if (Status != UploaderStatus.Commited)
                return false;

            var runningApp = this.FirstOrDefault(x => x.IsExecutable);

            // рестарт приложения, если обновляется исполняемый exe файл, даже если указано NeedRestartApplication = false
            if (runningApp != null)
            {
                ((BuildUpdaterBase)runningApp).Pull(DelayBeforeDelete, DelayBetweenMoveAndRunApp);
                Status = UploaderStatus.Pulled;

                Process.GetCurrentProcess().Kill();
            }
            else if (BuildPack.NeedRestartApplication) // рестарт приложения, если указана метка в пакете с обновлениями, иначе рестарта не будет
            {
                CMD.StartApplication(LocationApp, DelayBetweenMoveAndRunApp);
                Status = UploaderStatus.Pulled;

                Process.GetCurrentProcess().Kill();
            }

            return true;
        }
    }
}