using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using Utils.AssemblyHelper;
using Utils.CollectionHelper;
using Utils.XmlHelper;

namespace Utils.BuildUpdater
{
    public enum BuildPackResult
    {
        Update = 0,
        Cancel = 1
    }

    public class BuildUpdaterArgs
    {
        public BuildUpdaterArgs(BuildPackCollection buildPacks)
        {
            BuildPacks = buildPacks;
            Result = BuildPackResult.Update;
        }
        public BuildPackCollection BuildPacks { get; }
        public BuildPackResult Result { get; set; }
    }

    public delegate void BuildUpdaterHandler(object sender, BuildUpdaterArgs buildPack);

    public class BuildUpdater
    {
        public event BuildUpdaterHandler FindedNewVersions;
        private Timer _stopWatch;
        private Uri _xmlVersionPath;
        private Uri _uriToServerProject;
        private Assembly _mainAssembly;
        private BuildPackCollection deltaList;

        public BuildUpdater(Assembly mainAssembly, string uriProject, int updateSec = 10)
        {
            _mainAssembly = mainAssembly;
            _uriToServerProject = new Uri(uriProject.TrimEnd('/'));
            _xmlVersionPath = new Uri(_uriToServerProject + "/version.xml");

            _stopWatch = new Timer
            {
                Interval = updateSec * 1000
            };
            
            _stopWatch.Elapsed += GetNewestBuildsVersion;
            _stopWatch.AutoReset = false;
            _stopWatch.Enabled = true;
        }

        private void GetNewestBuildsVersion(object sender, ElapsedEventArgs e)
        {
            Dictionary<string, LocalAssemblyInfo>  currentAssemblies = new Dictionary<string, LocalAssemblyInfo>(StringComparer.CurrentCultureIgnoreCase);
            string directorypath = _mainAssembly.GetDirectory();
            foreach (string file in Directory.GetFiles(directorypath))
            {
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(file);
                LocalAssemblyInfo fileAssInfo = new LocalAssemblyInfo(myFileVersionInfo, directorypath);
                currentAssemblies.Add(fileAssInfo.FileName, fileAssInfo);
            }

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
                List<ServerAssemblyInfo> serverVersions = new List<ServerAssemblyInfo>();
                foreach (XmlNode updateNode in updateNodes)
                {
                    if (updateNode == null)
                        continue;

                    var getNodesWithoutCase = updateNode.GetChildNodes(StringComparer.CurrentCultureIgnoreCase);
                    serverVersions.Add(new ServerAssemblyInfo(getNodesWithoutCase, _uriToServerProject));
                }

                deltaList = new BuildPackCollection();
                foreach (ServerAssemblyInfo server in serverVersions)
                {
                    if (currentAssemblies.TryGetValue(server.FileName, out LocalAssemblyInfo current))
                    {
                        if ((server.Type == BuldPerformerType.Update || server.Type == BuldPerformerType.CreateOrUpdate) && server.Build > current.Build)
                            deltaList.Add(current, server);
                        else if (server.Type == BuldPerformerType.RollBack && server.Build < current.Build)
                            deltaList.Add(current, server);
                        else if (server.Type == BuldPerformerType.Remove && server.Build == current.Build)
                            deltaList.Add(current, server);
                    }
                    else if(server.Type == BuldPerformerType.CreateOrUpdate)
                    {
                        deltaList.Add(null, server);
                    }
                }

                if (deltaList.Count > 0)
                {
                    var eventListeners = FindedNewVersions?.GetInvocationList();
                    if (eventListeners.Count() > 0)
                    {
                        BuildUpdaterArgs buildArgs = new BuildUpdaterArgs(deltaList);
                        ((BuildUpdaterHandler) eventListeners[0]).Invoke(this, buildArgs);
                        if (buildArgs.Result == BuildPackResult.Update)
                        {
                            Update();
                        }
                        else
                        {
                            _stopWatch.Enabled = true;
                        }
                    }
                }
            }
        }

        public void Update()
        {
            deltaList.Update();
        }

        void UpdateCompleted()
        {
            _stopWatch.Enabled = true;
        }

        /// <summary>
        /// Checks the Uri to make sure file exist
        /// </summary>
        /// <param name="location">The Uri of the update.xml</param>
        /// <returns>If the file exists</returns>
        public static bool ExistsOnServer(Uri location)
        {
            if (location.ToString().StartsWith("file"))
            {
                return System.IO.File.Exists(location.LocalPath);
            }
            else
            {
                try
                {
                    // Request the update.xml
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(location.AbsoluteUri);
                    // Read for response
                    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                    resp.Close();

                    return resp.StatusCode == HttpStatusCode.OK;
                }
                catch { return false; }
            }
        }
    }
}