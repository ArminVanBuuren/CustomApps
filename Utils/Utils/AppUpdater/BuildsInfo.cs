using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Utils.AppUpdater
{
    [Serializable, XmlRoot("Builds")]
    public class BuildsInfo
    {
        public const string DEFAULT_PROJECT_GIT = @"https://github.com/ArminVanBuuren/Builds";
        public const string DEFAULT_PROJECT_RAW = @"https://raw.githubusercontent.com/ArminVanBuuren/Builds/master";
        public const string FILE_NAME = "versions.xml";

        [XmlElement("Pack")]
        public List<BuildPackInfo> Packs { get; set; } = new List<BuildPackInfo>();

        public void Add(string project, string assembliesDirPath, string destinationDirPath)
        {
            if (!Directory.Exists(destinationDirPath))
                throw new ArgumentException($"Destination directory=[{destinationDirPath}] doesn't exist");

            var pack = new BuildPackInfo(project, assembliesDirPath, destinationDirPath);
            var prevPacks = new List<BuildPackInfo>();
            Packs.RemoveAll(p =>
            {
                if (p.Project == project)
                {
                    prevPacks.Add(p);
                    return true;
                }

                return false;
            });
            Packs.Add(pack);

            try
            {
                Serialize(Path.Combine(destinationDirPath, FILE_NAME));
            }
            catch (Exception)
            {
                File.Delete(Path.Combine(destinationDirPath, pack.Name));
                throw;
            }

            foreach (var prevPack in prevPacks)
            {
                File.Delete(Path.Combine(destinationDirPath, prevPack.Name));
            }
        }

        public void Serialize(string fileVersionsPath)
        {
            if (File.Exists(fileVersionsPath))
                File.Delete(fileVersionsPath);

            using (var stream = new FileStream(fileVersionsPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
                new XmlSerializer(typeof(BuildsInfo)).Serialize(stream, this);
            }
        }

        public static BuildsInfo Deserialize(string contextStr)
        {
            var xsSubmit = new XmlSerializer(typeof(BuildsInfo));
            BuildsInfo result;
            using (TextReader reader = new StringReader(contextStr))
            {
                result = (BuildsInfo)xsSubmit.Deserialize(reader);
            }

            return result;
        }
    }
}