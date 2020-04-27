using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Utils.Properties;

namespace Utils.AppUpdater.Pack
{
    [Serializable, XmlRoot("Pack")]
    public class BuildPackInfo
    {
        /// <summary>
        /// Проект к которому предназначено обновление
        /// </summary>
        [XmlAttribute("Project")]
        public string ProjectName { get; set; }

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
        /// <param name="projectName"></param>
        /// <param name="assembliesDirPath"></param>
        /// <param name="destinationDirPath"></param>
        public BuildPackInfo(string projectName, string assembliesDirPath, string destinationDirPath)
        {
            ProjectName = projectName;
            Name = $"{STRING.RandomStringNumbers(15)}.zip";
            var packFileDestinationPath = Path.Combine(destinationDirPath, Name);
            var packFileTempPath = Path.GetTempFileName();

            try
            {
                Builds = GetLocalVersions(assembliesDirPath);
                if (Builds.Count == 0)
                    throw new ArgumentException(string.Format(Resources.DirectoryHasNoFiles, assembliesDirPath));

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
                File.Delete(packFileDestinationPath);
                throw;
            }
            finally
            {
                File.Delete(packFileTempPath);
            }
        }

        public static List<FileBuildInfo> GetLocalVersions(Assembly runningApp)
        {
            return GetLocalVersions(runningApp.GetDirectory(), runningApp.Location);
        }

        internal static List<FileBuildInfo> GetLocalVersions(string assembliesDirPath, string runningAppLocation = null)
        {
            return Directory.GetFiles(assembliesDirPath, "*.*", SearchOption.AllDirectories).Select(file => new FileBuildInfo(file, assembliesDirPath, file.Like(runningAppLocation))).ToList();
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

            return ProjectName.Equals(input.ProjectName) && Name.Equals(input.Name) && MD5.Equals(input.MD5);
        }

        public override int GetHashCode()
        {
            return ProjectName?.GetHashCode() ?? 0 + Name?.GetHashCode() ?? 0 + MD5?.GetHashCode() ?? 0 + 34;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
