using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public string FileSource => Path.Combine(_parent.DiretoryTempPath, ServerFile.Location);
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
        public FileBuildInfo ServerFile { get; }

        protected internal BuildUpdater(BuildPackUpdaterBase parent, FileBuildInfo localFile, FileBuildInfo serverFile)
        {
            if (serverFile == null)
                throw new ArgumentNullException($"[{nameof(FileBuildInfo)}] from server cannot be null");

            _parent = parent;
            LocalFile = localFile;
            ServerFile = serverFile;
            FileDestination = Path.Combine(Path.GetDirectoryName(parent.LocationApp), ServerFile.Location);
        }

        public override string ToString()
        {
            return $"[{ServerFile.Location}] Local = {LocalFile?.Version} Server = {ServerFile.Version}";
        }
    }
}
