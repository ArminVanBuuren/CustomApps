using System;

namespace Utils.AppUpdater
{
    [Serializable]
    class UpdaterProjectSimple : IUpdaterProject
    {
        public Uri Uri { get; }
        public string BuildsInfoName { get; }
        public Uri BuildsInfoUri { get; }

        internal UpdaterProjectSimple()
        {
            Uri = new Uri(@"https://raw.githubusercontent.com/ArminVanBuuren/Builds/master");
            BuildsInfoName = "versions.xml";
            BuildsInfoUri = new Uri($"{Uri}/{BuildsInfoName}");
        }
    }
}
