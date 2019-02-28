using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using Utils.AssemblyHelper;
using Utils.CollectionHelper;
using Utils.XmlHelper;

namespace Utils.BuildUpdater
{
    public class BuildUpdater
    {
        private Timer _stopWatch;
        private Uri _xmlVersionPath;
        Dictionary<string, FileAssemblyInfo> _currentAssemblies = new Dictionary<string, FileAssemblyInfo>(StringComparer.CurrentCultureIgnoreCase);
        private string _uriToServerProject;

        public BuildUpdater(Assembly mainAssembly, string uriProject, int updateSec = 600)
        {
            _uriToServerProject = uriProject.TrimEnd('/');
            _xmlVersionPath = new Uri(_uriToServerProject + "/version.xml");

            string directorypath = mainAssembly.GetDirectory();
            foreach (string file in Directory.GetFiles(directorypath))
            {
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(file);
                FileAssemblyInfo fileAssInfo = new FileAssemblyInfo(myFileVersionInfo, directorypath);
                _currentAssemblies.Add(fileAssInfo.FileName, fileAssInfo);
            }

            _stopWatch = new Timer
            {
                Interval = updateSec * 1000
            };
            
            _stopWatch.Elapsed += GetNewestBuildsVersion;
            _stopWatch.AutoReset = false;
            _stopWatch.Enabled = true;
            GetNewestBuildsVersion(this, null);
        }

        private void GetNewestBuildsVersion(object sender, ElapsedEventArgs e)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_xmlVersionPath.AbsoluteUri);
            // Read for response
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            resp.Close();

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                ServicePointManager.ServerCertificateValidationCallback = (s, ce, ch, ssl) => true;
                XmlDocument doc = new XmlDocument();
                doc.Load(_xmlVersionPath.AbsoluteUri);
                XmlNodeList updateNodes = doc.DocumentElement.SelectNodes("/Versions/Build");
                List<FileAssemblyInfo> serverVersions = new List<FileAssemblyInfo>();
                foreach (XmlNode updateNode in updateNodes)
                {
                    if (updateNode == null)
                        continue;

                    var getNodesWithoutCase = updateNode.GetChildNodes(StringComparer.CurrentCultureIgnoreCase);
                    serverVersions.Add(new FileAssemblyInfo(getNodesWithoutCase));
                }

                List<FileAssemblyInfo> deltaList = new List<FileAssemblyInfo>();

                foreach (FileAssemblyInfo server in serverVersions)
                {
                    if (server.Type == BuldPerformerType.Mandatory || server.Type == BuldPerformerType.Remove)
                    {
                        deltaList.Add(server);
                        continue;
                    }

                    if (_currentAssemblies.TryGetValue(server.FileName, out FileAssemblyInfo current))
                    {
                        if (server.Type == BuldPerformerType.Update && server.Build > current.Build)
                            deltaList.Add(server);
                        else if (server.Type == BuldPerformerType.RollBack && server.Build < current.Build)
                            deltaList.Add(server);
                    }
                }
            }

            _stopWatch.Enabled = true;
        }

        

        public void Refresh()
        {

        }
    }
}
