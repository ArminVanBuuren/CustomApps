using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Utils.GitHelper;

namespace Utils.AppUpdater
{
    [Serializable, XmlRoot("Builds")]
    public class Builds
    {
        public const string DEFAULT_PROJECT_URI = @"https://github.com/ArminVanBuuren/Builds";
        public const string FILE_NAME = "versions.xml";

        [XmlElement("Pack")]
        public List<BuildPack> Packs { get; set; } = new List<BuildPack>();

        public void Add(string project, string assembliesDirPath, string destinationDirPath)
        {
            if (!Directory.Exists(destinationDirPath))
                throw new ArgumentException($"Destination directory=[{destinationDirPath}] doesn't exist");

            BuildPack pack = new BuildPack(project, assembliesDirPath, destinationDirPath);
            List<BuildPack> prevPacks = new List<BuildPack>();
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
                Serialize(Path.Combine(destinationDirPath, Builds.FILE_NAME));
            }
            catch (Exception ex)
            {
                File.Delete(Path.Combine(destinationDirPath, pack.Name));
                throw ex;
            }

            foreach (BuildPack prevPack in prevPacks)
            {
                File.Delete(Path.Combine(destinationDirPath, prevPack.Name));
            }
        }

        public static Builds Deserialize(string contextStr)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(Builds));
            Builds result;
            using (TextReader reader = new StringReader(contextStr))
            {
                result = (Builds)xsSubmit.Deserialize(reader);
            }

            return result;
        }

        public void Serialize(string fileVersionsPath)
        {
            if(File.Exists(fileVersionsPath))
                File.Delete(fileVersionsPath);
            
            using (FileStream stream = new FileStream(fileVersionsPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
                new XmlSerializer(typeof(Builds)).Serialize(stream, this);
            }
        }
    }

 



    [Serializable, XmlRoot("Versions")]
    public class BuildInfoVersionsTEmp : IXmlSerializable
    {
        public const string FILE_NAME = "version.xml";

        [XmlComment(Value = "The application version, NOT the file version!")]
        [XmlElement("Build")]
        public List<FileBuildInfo> Builds { get; set; } = new List<FileBuildInfo>();


        public void WriteXml(XmlWriter writer)
        {
            var properties = GetType().GetProperties();

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.IsDefined(typeof(XmlCommentAttribute), false))
                {
                    writer.WriteComment(propertyInfo.GetCustomAttributes(typeof(XmlCommentAttribute), false).Cast<XmlCommentAttribute>().Single().Value);
                }

                writer.WriteElementString(propertyInfo.Name, propertyInfo.GetValue(this, null).ToString());
            }
        }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class XmlCommentAttribute : Attribute
    {
        public string Value { get; set; }
    }

}