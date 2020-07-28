using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Utils.AppUpdater.Pack;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    public enum UploaderStatus
    {
        None = 0,
        Init = 1,
        Fetched = 2,
        Commited = 4,
        Pulled = 8,
        Disposed = 16
    }

    [Serializable]
    public delegate void UpdaterDownloadProgressChangedHandler(object sender, EventArgs empty);

    [Serializable]
    public abstract class BuildPackUpdater : IEnumerable<BuildUpdater>, IDisposable
    {
        private readonly List<BuildUpdater> _collection;

        [field: NonSerialized]
        public abstract event UpdaterDownloadProgressChangedHandler DownloadProgressChanged;

        /// <summary>
        /// Текущий статус выполнения обновления
        /// </summary>
        public UploaderStatus Status { get; protected set; } = UploaderStatus.None;

        /// <summary>
        /// Местоположение основного процесса
        /// </summary>
        public string LocationApp { get; protected set; }

        /// <summary>
        /// задержка в секундах перед запуском переноса файла (ограниение 1 час)
        /// </summary>
        public int DelayBeforeDelete { get; set; } = 0;

        /// <summary>
        /// задержка в секундах перед запуском приложения после переноса файла (ограниение 1 час). Учесть размер файла и примерное время замены и удаления изначального местоположения файла
        /// </summary>
        public int DelayBetweenMoveAndRunApp { get; set; } = 3;

        /// <summary>
        /// Количество файлов для обновления
        /// </summary>
        public int Count => _collection?.Count ?? 0;

        #region Status of Total and Uploaded Bytes

        /// <summary>
        /// Количество скачанных байт
        /// </summary>
        public long UploadedBytes { get; protected set; } = 0;

        public string UploadedString => IO.FormatBytes(UploadedBytes, out _);

        public long TotalBytes { get; protected set; } = 0;

        /// <summary>
        /// Размер файлов на сервере
        /// </summary>
        public string TotalString => IO.FormatBytes(TotalBytes, out _);

        /// <summary>
        /// Прогресс в процентах скачиванных файлов с сервера
        /// </summary>
        public int ProgressPercent
        {
            get
            {
                IO.FormatBytes(UploadedBytes, out var upload);
                IO.FormatBytes(TotalBytes, out var total);
                if (total == 0)
                    return 0;
                return (int)((upload / total) * 100);
            }
        }

        /// <summary>
        /// Прогресс скачиванных файлов с сервера
        /// </summary>
        /// <returns></returns>
        public virtual string ProgressString => $"{UploadedString} of {TotalString}";

        #endregion

        /// <summary>
        /// Исходный файл с информацией о версиях
        /// </summary>
        public BuildPackInfo BuildPack { get; }

        /// <summary>
        /// Указывает на необходимость выкачивания файла с пакетом обновлений из сервера на локальный компютер
        /// Необходимо проверять т.к. возмоно пакет пришел только на удаление файлов
        /// </summary>
        public bool NeedToFetchPack { get; protected set; }

        public string FileTempPath { get; }

        public string DiretoryTempPath { get; }

        protected internal BuildPackUpdater(Assembly runningApp, BuildPackInfo buildPack)
        {
            if (runningApp == null)
                throw new ArgumentNullException(nameof(runningApp));

            _collection = new List<BuildUpdater>();
            LocationApp = runningApp.Location;
            BuildPack = buildPack ?? throw new ArgumentNullException(nameof(buildPack));

            var localVersions = BuildPackInfo.GetLocalVersions(runningApp, BuildPack);
            var filesToChange = 0;

            foreach (var localFile in localVersions)
            {
                var remoteFile = BuildPack.Builds.FirstOrDefault(x => x.AssemblyName == localFile.AssemblyName && x.ScopeName == localFile.ScopeName);
                if (remoteFile == null)
                    continue;

                if ((remoteFile.Type == BuldPerformerType.Update || remoteFile.Type == BuldPerformerType.CreateOrUpdate) && remoteFile.Version > localFile.Version)
                {
                    Add(localFile, remoteFile);
                    filesToChange++;
                }
                else if ((remoteFile.Type == BuldPerformerType.RollBack || remoteFile.Type == BuldPerformerType.CreateOrRollBack) && remoteFile.Version < localFile.Version)
                {
                    Add(localFile, remoteFile);
                    filesToChange++;
                }
                else if (remoteFile.Type == BuldPerformerType.Remove)
                {
                    Add(localFile, remoteFile);
                }
            }

            foreach (var remoteFile in BuildPack.Builds.Where(x => x.Type == BuldPerformerType.CreateOrUpdate || x.Type == BuldPerformerType.CreateOrRollBack))
            {
                if (this.Any(x => x.LocalFile.AssemblyName == remoteFile.AssemblyName && x.LocalFile.ScopeName == remoteFile.ScopeName))
                    continue;

                Add(null, remoteFile);
                filesToChange++;
            }

            NeedToFetchPack = filesToChange > 0;

            if (NeedToFetchPack)
            {
                FileTempPath = Path.GetTempFileName();
                DiretoryTempPath = Path.Combine(Path.GetTempPath(), STRING.RandomString(15));
            }
        }

        protected internal void Add(FileBuildInfo localFile, FileBuildInfo remoteFile)
        {
            _collection.Add(GetBuildUpdater(localFile, remoteFile));
        }

        protected abstract BuildUpdater GetBuildUpdater(FileBuildInfo localFile, FileBuildInfo remoteFile);

        public byte[] SerializeToStreamOfBytes()
        {
            using (var stream = this.SerializeToStream())
                return stream.ToArray();
        }

        public static BuildPackUpdater Deserialize(byte[] streamOfBytes)
        {
            using (var stream = new MemoryStream(streamOfBytes))
                return new BinaryFormatter().Deserialize(stream) as BuildPackUpdater;
        }

        public IEnumerator<BuildUpdater> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        void RemoveTempObjects()
        {
            try
            {
                if (!DiretoryTempPath.IsNullOrWhiteSpace() && Directory.Exists(DiretoryTempPath))
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
                if (!FileTempPath.IsNullOrWhiteSpace() && File.Exists(FileTempPath))
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

        public virtual void Dispose()
        {
            Status = UploaderStatus.Disposed;
            RemoveTempObjects();
        }

        public override string ToString()
        {
            return $"Status = \"{Status:G}\" BuildsCount = {Count}";
        }
    }
}