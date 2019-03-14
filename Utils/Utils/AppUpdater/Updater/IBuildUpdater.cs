using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.AppUpdater.Updater
{
    public interface IBuildUpdater
    {
        /// <summary>
        /// Пусть скачанного файла билда
        /// </summary>
        string FileSource { get; }
        /// <summary>
        /// Путь назначения файла билда
        /// </summary>
        string FileDestination { get; }
        /// <summary>
        /// Явлиется ли билд контрольным объектом
        /// </summary>
        bool IsExecutable { get; }
        /// <summary>
        /// Информация билда на локальном сервере
        /// </summary>
        FileBuildInfo LocalFile { get; }
        /// <summary>
        /// Информация билда на удаленном сервере
        /// </summary>
        FileBuildInfo ServerFile { get; }
    }
}
