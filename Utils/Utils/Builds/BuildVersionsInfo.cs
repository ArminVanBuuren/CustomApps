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

namespace Utils.Builds
{
    [Serializable, XmlRoot("Versions")]
    public class BuildVersionsInfo
    {
        public const string FILE_NAME = "version.xml";

        [XmlElement]
        public string BuildPack { get; set; }

        [XmlElement]
        public string MD5 { get; set; }

        [XmlElement("Build")]
        public List<FileBuildInfo> Builds { get; set; } = new List<FileBuildInfo>();

        public BuildVersionsInfo()
        {

        }

        public BuildVersionsInfo(Assembly assembly, string destinationPath) :this(assembly.GetDirectory(), destinationPath)
        {

        }


        public BuildVersionsInfo(string assembliesDirPath, string destinationDirPath)
        {
            if(!Directory.Exists(destinationDirPath))
                throw new ArgumentException($"Destionation dirtctory=[{destinationDirPath}] doesn't exist");

            BuildPack = STRING.RandomStringNumbers(10) + ".zip";
            string fileBuildsPath = Path.Combine(destinationDirPath, BuildPack);
            string fileVersionsPath = Path.Combine(destinationDirPath, BuildVersionsInfo.FILE_NAME);
            string tempFileBuildsPath = Path.GetTempFileName();

            try
            {
                Builds = GetLocalVersions(assembliesDirPath).Values.ToList();
                if (Builds.Count == 0)
                    throw new ArgumentException($"Directory=[{assembliesDirPath}] has no one file.");

                File.Delete(tempFileBuildsPath);
                ZipFile.CreateFromDirectory(assembliesDirPath, tempFileBuildsPath, CompressionLevel.Optimal, false);
                File.Copy(tempFileBuildsPath, fileBuildsPath, true);
                File.Delete(tempFileBuildsPath);

                MD5 = Hasher.HashFile(fileBuildsPath, HashType.MD5);

                using (FileStream stream = new FileStream(fileVersionsPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                {
                    new XmlSerializer(typeof(BuildVersionsInfo)).Serialize(stream, this);
                }
            }
            catch (Exception e)
            {
                File.Delete(tempFileBuildsPath);
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

        public void SerializeAndDeserialize(BuildVersionsInfo versions)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(BuildVersionsInfo));
            var xml = "";
            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, versions);
                    xml = sww.ToString();
                }
            }

            BuildVersionsInfo res;
            using (TextReader reader = new StringReader(xml))
            {
                res = (BuildVersionsInfo)xsSubmit.Deserialize(reader);
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