using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        public int DelayBeforeMove { get; set; } = 0;

        /// <summary>
        /// задержка в секундах перед запуском приложения после переноса файла (ограниение 1 час). Учесть размер файла и примерное время замены и удаления изначального местоположения файла
        /// </summary>
        public int DelayAfterMoveAndRunApp { get; set; } = 5;

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
                return int.Parse(((upload / total) * 100).ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Прогресс скачиванных файлов с сервера
        /// </summary>
        /// <returns></returns>
        public virtual string ProgressString => $"{UploadedString} of {TotalString}";

        #endregion

        protected internal BuildPackUpdater(Assembly runningApp)
        {
            if (runningApp == null)
                throw new ArgumentNullException(nameof(runningApp));

            _collection = new List<BuildUpdater>();
            LocationApp = runningApp.Location;
        }

        protected internal void Add(FileBuildInfo localFile, FileBuildInfo remoteFile)
        {
            Add(GetBuildUpdater(localFile, remoteFile));
        }

        protected internal void Add(BuildUpdater item)
        {
            _collection.Add(item);
        }

        public byte[] SerializeToStreamOfBytes()
        {
            using (var stream = this.SerializeToStream())
                return stream.ToArray();
        }

        public BuildPackUpdater Deserialize(byte[] streamOfBytes)
        {
            using (var stream = new MemoryStream(streamOfBytes))
                return new BinaryFormatter().Deserialize(stream) as BuildPackUpdater;
        }

        protected abstract BuildUpdater GetBuildUpdater(FileBuildInfo localFile, FileBuildInfo remoteFile);

        public IEnumerator<BuildUpdater> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public virtual void Dispose()
        {
            Status = UploaderStatus.Disposed;
        }

        public override string ToString()
        {
            return $"Status = {Status:G} BuildsCount = {Count}";
        }
    }
}