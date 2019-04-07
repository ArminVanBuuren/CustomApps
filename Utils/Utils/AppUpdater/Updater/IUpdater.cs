using System;
using System.Collections.Generic;

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

    public interface IUpdater : IEnumerable<IBuildUpdater>, IEnumerator<IBuildUpdater>
    {
        event UpdaterDownloadProgressChangedHandler DownloadProgressChanged;
        /// <summary>
        /// Текущий статус выполнения обновления
        /// </summary>
        UploaderStatus Status { get; }
        /// <summary>
        /// Местоположение основного процесса
        /// </summary>
        string LocationApp { get; }
        /// <summary>
        /// Путь к проекту с обновлениями
        /// </summary>
        Uri ProjectUri { get; }
        /// <summary>
        /// Исходный файл с информацией о версиях
        /// </summary>
        BuildPackInfo ProjectBuildPack { get; }
        /// <summary>
        /// Указывает на необходимость выкачивания файла с пакетом обновлений из сервера на локальный компютер
        /// </summary>
        bool NeedToFetchPack { get; }
        /// <summary>
        /// Количество скачанных байт
        /// </summary>
        long UploadedBytes { get; }
        string UploadedString { get; }
        /// <summary>
        /// Размер файлов на сервере
        /// </summary>
        long TotalBytes { get; }
        string TotalString { get; }
        /// <summary>
        /// Прогресс в процентах скачиванных файлов с сервера
        /// </summary>
        int ProgressPercent { get; }
        /// <summary>
        /// Количество файлов для обновления
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Прогресс скачиванных файлов с сервера
        /// </summary>
        /// <returns></returns>
        string GetProgressString();
    }
}
