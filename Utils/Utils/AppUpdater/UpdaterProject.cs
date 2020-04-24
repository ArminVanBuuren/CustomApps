using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.AppUpdater
{
    public interface IUpdaterProject
    {
        Uri Uri { get; }
        string BuildsInfoName { get; }
        Uri BuildsInfoUri { get; }
    }

    [Serializable]
    class UpdaterProject : IUpdaterProject
    {
        public Uri Uri { get; }
        public string BuildsInfoName { get; }
        public Uri BuildsInfoUri { get; }

        internal UpdaterProject()
        {
            Uri = new Uri(@"https://raw.githubusercontent.com/ArminVanBuuren/Builds/master");
            BuildsInfoName = "versions.xml";
            BuildsInfoUri = new Uri($"{Uri}/{BuildsInfoName}");
        }
    }

}
