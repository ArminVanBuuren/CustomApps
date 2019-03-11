using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Utils.Builds.Unloader
{
    public class BuildUnloader
    {
        public BuildUnloader(string assembliesDirPath)
        {
            string fileVersions = Path.Combine(assembliesDirPath, BuildInfoVersions.FILE_NAME);
            try
            {
                BuildInfoVersions versions = new BuildInfoVersions
                {
                    Builds = GetLocalVersions(assembliesDirPath).Values.ToList()
                };

                using (FileStream stream = new FileStream(fileVersions, FileMode.Create, FileAccess.ReadWrite))
                {
                    new XmlSerializer(typeof(BuildInfoVersions)).Serialize(stream, versions);
                }
            }
            catch (Exception e)
            {
                File.Delete(fileVersions);
                Console.WriteLine(e);
            }
        }

        public void SerializeAndDeserialize(BuildInfoVersions versions)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(BuildInfoVersions));
            var xml = "";
            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, versions);
                    xml = sww.ToString();
                }
            }

            BuildInfoVersions res;
            using (TextReader reader = new StringReader(xml))
            {
                res = (BuildInfoVersions)xsSubmit.Deserialize(reader);
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
    }
}