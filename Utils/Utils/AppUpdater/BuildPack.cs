using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Utils.AppUpdater
{
    [Serializable, XmlRoot("Pack")]
    public class BuildPack
    {
        [XmlAttribute]
        public string Project { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string MD5 { get; set; }

        [XmlElement("Build")]
        public List<FileBuildInfo> Builds { get; set; } = new List<FileBuildInfo>();

        public BuildPack()
        {

        }

        public BuildPack(string project, string assembliesDirPath, string destinationDirPath)
        {
            Project = project;
            Name = STRING.RandomStringNumbers(15) + ".zip";
            string packFileDestinationPath = Path.Combine(destinationDirPath, Name);
            string packFileTempPath = Path.GetTempFileName();

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
            }
            catch (Exception e)
            {
                File.Delete(packFileTempPath);
                File.Delete(packFileDestinationPath);
                throw e;
            }
        }

        internal static Dictionary<string, FileBuildInfo> GetLocalVersions(Assembly runningApp)
        {
            string assembliesDirPath = runningApp.GetDirectory();
            return GetLocalVersions(assembliesDirPath, runningApp.Location);
        }

        internal static Dictionary<string, FileBuildInfo> GetLocalVersions(string assembliesDirPath, string runningAppLocation = null)
        {
            Dictionary<string, FileBuildInfo> localVersions = new Dictionary<string, FileBuildInfo>(StringComparer.CurrentCultureIgnoreCase);
            foreach (string file in Directory.GetFiles(assembliesDirPath, "*.*", SearchOption.AllDirectories))
            {
                FileBuildInfo localFileInfo = new FileBuildInfo(file, assembliesDirPath, file.Like(runningAppLocation));
                localVersions.Add(localFileInfo.Location, localFileInfo);
            }

            return localVersions;
        }

        public void SerializeAndDeserialize(Builds versions)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(Builds));
            var xml = "";
            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, versions);
                    xml = sww.ToString();
                }
            }

            Builds res;
            using (TextReader reader = new StringReader(xml))
            {
                res = (Builds)xsSubmit.Deserialize(reader);
            }
        }

        public override string ToString()
        {
            return $"Name=[{Name}] Project=[{Project}]";
        }
    }
}
