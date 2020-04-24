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

        /// <summary>
        /// Исходный файл с информацией о версиях
        /// </summary>
        public BuildPackInfo ProjectBuildPack { get; }

        /// <summary>
        /// Указывает на необходимость выкачивания файла с пакетом обновлений из сервера на локальный компютер
        /// Необходимо проверять т.к. возмоно пакет пришел только на удаление файлов
        /// </summary>
        public bool NeedToFetchPack { get; protected set; }

        public string FileTempPath { get; }

        public string DiretoryTempPath { get; }

        protected BuildPackUpdaterBase(Assembly runningApp, BuildPackInfo projectBuildPack) :base(runningApp)
        {
            ProjectBuildPack = projectBuildPack ?? throw new ArgumentNullException(nameof(projectBuildPack));

            var localVersions = BuildPackInfo.GetLocalVersions(runningApp);
            var needToFetchPack = 0;

            foreach (var remoteFile in ProjectBuildPack.Builds)
            {
                if (localVersions.TryGetValue(remoteFile.Location, out var localFile))
                {
                    if ((remoteFile.Type == BuldPerformerType.Update || remoteFile.Type == BuldPerformerType.CreateOrUpdate) && remoteFile.Version > localFile.Version)
                    {
                        Add(localFile, remoteFile);
                        needToFetchPack++;
                    }
                    else if ((remoteFile.Type == BuldPerformerType.RollBack || remoteFile.Type == BuldPerformerType.CreateOrRollBack) && remoteFile.Version < localFile.Version)
                    {
                        Add(localFile, remoteFile);
                        needToFetchPack++;
                    }
                    else if (remoteFile.Type == BuldPerformerType.Remove)
                    {
                        Add(localFile, remoteFile);
                    }
                }
                else if (remoteFile.Type == BuldPerformerType.CreateOrUpdate || remoteFile.Type == BuldPerformerType.CreateOrRollBack)
                {
                    Add(null, remoteFile);
                    needToFetchPack++;
                }
            }

            NeedToFetchPack = needToFetchPack > 0;

            if (Count > 0)
            {
                FileTempPath = Path.GetTempFileName();
                DiretoryTempPath = Path.Combine(Path.GetTempPath(), STRING.RandomString(15));
            }
        }

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

            var runningApp = this.First(x => x.IsExecutable);

            // рестарт приложения, если обновляется исполняемый exe файл, даже если указано NeedRestartApplication = false
            if (runningApp != null)
            {
                ((BuildUpdaterBase)runningApp).Pull(DelayBeforeMove, DelayAfterMoveAndRunApp);
                Status = UploaderStatus.Pulled;

                Process.GetCurrentProcess().Kill();
            }
            else if (ProjectBuildPack.NeedRestartApplication) // рестарт приложения, если указана метка в пакете с обновлениями, иначе рестарта не будет
            {
                CMD.StartApplication(LocationApp, DelayAfterMoveAndRunApp);
                Status = UploaderStatus.Pulled;

                Process.GetCurrentProcess().Kill();
            }

            return true;
        }

        void RemoveTempObjects()
        {
            try
            {
                if (!DiretoryTempPath.IsNullOrEmptyTrim() && Directory.Exists(DiretoryTempPath))
                    IO.DeleteReadOnlyDirectory(DiretoryTempPath);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                DeleteTempFile();
            }
        }

        protected void DeleteTempFile()
        {
            try
            {
                if (!FileTempPath.IsNullOrEmptyTrim() && File.Exists(FileTempPath))
                {
                    IO.GetAccessToFile(FileTempPath);
                    File.Delete(FileTempPath);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            RemoveTempObjects();
        }
    }
}