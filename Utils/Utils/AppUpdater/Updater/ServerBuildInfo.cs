using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.CollectionHelper;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    public sealed class ServerBuildInfo : FileBuildInfo
    {
        public Uri UriFilePath { get; private set; }
        public string DestinationFilePath { get; private set; }

        public ServerBuildInfo(DuplicateDictionary<string, string> collectionAttr, Uri server, string assemblyDirPath)
        {
            VersionString = collectionAttr["Version"]?.First() ?? "1.0.0.0";
            TypeString = collectionAttr["Type"]?.First() ?? string.Empty;
            Location = (collectionAttr["Location"]?.First() ?? string.Empty).Trim('\\');
            Description = collectionAttr["Description"]?.First() ?? string.Empty;

            Common(server, assemblyDirPath);
        }

        public ServerBuildInfo(FileBuildInfo build, Uri server, string assemblyDirPath)
        {
            Version = build.Version;
            Type = build.Type;
            Location = build.Location;
            Description = build.Description;

            Common(server, assemblyDirPath);
        }

        void Common(Uri server, string assemblyDirPath)
        {
            DestinationFilePath = Path.Combine(assemblyDirPath, Location);

            if (Type != BuldPerformerType.Remove)
            {
                FilePath = Path.GetTempFileName(); // Gets the temp file path for the downloaded file
                UriFilePath = new Uri($"{server.AbsoluteUri}/{Location.Replace("\\", "/")}");
            }
        }

        public override string ToString()
        {
            return $"[{Type:G}] {base.ToString()} Uri=[{UriFilePath}]";
        }
    }
}
