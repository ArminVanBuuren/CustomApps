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
        public string FileSource => Path.Combine(_parent.DiretoryTempPath, RemoteFile.Location);
        /// <summary>
        /// Путь назначения файла билда
        /// </summary>
        public virtual string FileDestination { get; }
        /// <summary>
        /// Явлиется ли билд контрольным объектом
        /// </summary>
        public virtual bool IsExecutable
        {
            get
            {
                if (LocalFile != null)
                    return LocalFile.IsExecutingFile;
                return false;
            }
        }
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
            if (remoteFile == null)
                throw new ArgumentNullException($"Remote [{nameof(FileBuildInfo)}] cannot be null");

            _parent = parent;
            LocalFile = localFile;
            RemoteFile = remoteFile;
            FileDestination = Path.Combine(Path.GetDirectoryName(parent.LocationApp), RemoteFile.Location);
        }

        public override string ToString()
        {
            return $"[{RemoteFile.Location}] Local = {LocalFile?.Version} Remote = {RemoteFile.Version}";
        }
    }
}
