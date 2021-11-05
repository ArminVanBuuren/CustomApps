using System;
using System.IO;
using Utils.AppUpdater.Pack;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    public abstract class BuildUpdater
    {
        private readonly BuildPackUpdaterBase _parent;
        /// <summary>
        /// Пусть скачанного файла билда
        /// </summary>
        public string FileSource => Path.Combine(_parent.DiretoryTempPath ?? string.Empty, RemoteFile.Location);
        /// <summary>
        /// Путь назначения файла билда
        /// </summary>
        public virtual string FileDestination { get; }
        /// <summary>
        /// Явлиется ли билд контрольным объектом
        /// </summary>
        public virtual bool IsExecutable => LocalFile != null && LocalFile.IsExecutingFile;

        /// <summary>
        /// Информация билда на локальном сервере
        /// </summary>
        public FileBuildInfo LocalFile { get; }
        /// <summary>
        /// Информация билда на удаленном сервере
        /// </summary>
        public FileBuildInfo RemoteFile { get; }

        protected internal BuildUpdater(BuildPackUpdaterBase parent, FileBuildInfo localFile, FileBuildInfo remoteFile)
        {
            _parent = parent;
            LocalFile = localFile;
            RemoteFile = remoteFile ?? throw new ArgumentNullException($"Remote {nameof(FileBuildInfo)} cannot be null");
            FileDestination = Path.Combine(Path.GetDirectoryName(parent.LocationApp), LocalFile != null ? LocalFile.Location : RemoteFile.Location);
        }

        public override string ToString()
        {
            return $"[{RemoteFile.AssemblyName}] Local = \"{LocalFile?.Version}\" Remote = \"{RemoteFile.Version}\"";
        }
    }
}
