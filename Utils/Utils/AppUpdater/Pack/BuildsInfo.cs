using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Utils.AppUpdater.Pack
{
    [Serializable, XmlRoot("Builds")]
    public class BuildsInfo
    {
        [XmlElement("Pack")]
        public List<BuildPackInfo> Packs { get; set; } = new List<BuildPackInfo>();

        public void Add(string project, string assembliesDirPath, string destinationDirPath, string buildsInfoFileName)
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
                Serialize(Path.Combine(destinationDirPath, buildsInfoFileName));
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

        void Serialize(string fileVersionsPath)
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