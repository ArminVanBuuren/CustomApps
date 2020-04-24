using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Utils.AppUpdater.Pack
{
    [Serializable, XmlRoot("Pack")]
    public class BuildPackInfo
    {
        /// <summary>
        /// Проект к которому предназначено обновление
        /// </summary>
        [XmlAttribute]
        public string Project { get; set; }

        /// <summary>
        /// Название пакета с обновлениями
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Хэш файла
        /// </summary>
        [XmlAttribute]
        public string MD5 { get; set; }

        /// <summary>
        /// Необходимо ли перезагрузить приложеие после апдейта
        /// </summary>
        [XmlAttribute]
        public bool NeedRestartApplication { get; set; }

        [XmlElement("Build")]
        public List<FileBuildInfo> Builds { get; set; } = new List<FileBuildInfo>();

        public BuildPackInfo() { }

        /// <summary>
        /// ПОдготовка пакета обновлений
        /// </summary>
        /// <param name="project"></param>
        /// <param name="assembliesDirPath"></param>
        /// <param name="destinationDirPath"></param>
        public BuildPackInfo(string project, string assembliesDirPath, string destinationDirPath)
        {
            Project = project;
            Name = STRING.RandomStringNumbers(15) + ".zip";
            var packFileDestinationPath = Path.Combine(destinationDirPath, Name);
            var packFileTempPath = Path.GetTempFileName();

            try
            {
                Builds = GetLocalVersions(assembliesDirPath).Values.ToList();
                if (Builds.Count == 0)
                    throw new ArgumentException($"Directory=[{assembliesDirPath}] has no one file.");

                File.Delete(packFileTempPath);
                ZipFile.CreateFromDirectory(assembliesDirPath, packFileTempPath, CompressionLevel.Optimal, false);
                File.Copy(packFileTempPath, packFileDestinationPath, true);
                File.Delete(packFileTempPath);

                MD5 = Hasher.HashFile(packFileDestinationPath, HashType.MD5);

                // по дефолту всегда надо перегружать приложение после обновления
                NeedRestartApplication = true;
            }
            catch (Exception)
            {
                File.Delete(packFileTempPath);
                File.Delete(packFileDestinationPath);
                throw;
            }
        }

        public static Dictionary<string, FileBuildInfo> GetLocalVersions(Assembly runningApp)
        {
            var assembliesDirPath = runningApp.GetDirectory();
            return GetLocalVersions(assembliesDirPath, runningApp.Location);
        }

        internal static Dictionary<string, FileBuildInfo> GetLocalVersions(string assembliesDirPath, string runningAppLocation = null)
        {
            var localVersions = new Dictionary<string, FileBuildInfo>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var file in Directory.GetFiles(assembliesDirPath, "*.*", SearchOption.AllDirectories))
            {
                var localFileInfo = new FileBuildInfo(file, assembliesDirPath, file.Like(runningAppLocation));
                localVersions.Add(localFileInfo.Location, localFileInfo);
            }

            return localVersions;
        }

        BuildsInfo SerializeAndDeserialize(BuildsInfo versions)
        {
            var xsSubmit = new XmlSerializer(typeof(BuildsInfo));
            string xml;
            using (var sww = new StringWriter())
            {
                using (var writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, versions);
                    xml = sww.ToString();
                }
            }

            using (TextReader reader = new StringReader(xml))
                return (BuildsInfo)xsSubmit.Deserialize(reader);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BuildPackInfo input))
                return false;

            return Project.Equals(input.Project) && Name.Equals(input.Name) && MD5.Equals(input.MD5);
        }

        public override int GetHashCode()
        {
            return Project?.GetHashCode() ?? 0 + Name?.GetHashCode() ?? 0 + MD5?.GetHashCode() ?? 0 + 34;
        }

        public override string ToString()
        {
            return $"Name=[{Name}] Project=[{Project}]";
        }
    }
}
