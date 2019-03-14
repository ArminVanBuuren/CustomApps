﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    public enum UploaderStatus
    {
        None = 0,
        Init = 1,
        Fetched = 2,
        Commited = 4,
        Pulled = 8
    }

    public interface IUpdater : IEnumerable<IBuildUpdater>, IEnumerator<IBuildUpdater>, IDisposable
    {
        /// <summary>
        /// Когда новые версии бильдов скачались на локальный диск в темповую папку. Либо загрузка завершилась неудачей
        /// </summary>
        event UploadBuildHandler OnFetchComplete;
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
        /// Количество скачанных байт
        /// </summary>
        long UploadedBytes { get; }
        /// <summary>
        /// Размер файлов на сервере
        /// </summary>
        long TotalBytes { get; }
        /// <summary>
        /// Прогресс в процентах скачиванных файлов с сервера
        /// </summary>
        int ProgressPercent { get; }
        int Count { get; }
        /// <summary>
        /// Прогресс скачиванных файлов с сервера
        /// </summary>
        /// <returns></returns>
        string GetProgressString();
    }
}
