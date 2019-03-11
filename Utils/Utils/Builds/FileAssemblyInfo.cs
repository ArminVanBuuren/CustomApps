using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.AssemblyHelper;
using Utils.CollectionHelper;

namespace Utils.BuildUpdater
{
    [Serializable]
    public enum BuldPerformerType
    {
        None = 0,
        CreateOrUpdate = 1,
        Update = 2,
        RollBack = 4,
        Remove = 8
    }

    [Serializable]
    public abstract class FileAssemblyInfo
    {
        public BuildNumber Build { get; protected set; }
        public string FilePath { get; protected set; }
        public string FileName { get; protected set; }

        public override string ToString()
        {
            return $"Version=[{Build.ToString()}] File=[{FileName}]";
        }
    }

    [Serializable]
    public sealed class LocalAssemblyInfo : FileAssemblyInfo
    {
        public bool IsExecutingFile { get; }

        public LocalAssemblyInfo(string file, string assemblyDirPath, bool isExecFile = false)
        {
            Build = BuildNumber.FromFile(file);
            FilePath = file;
            FileName = file.Replace(assemblyDirPath, string.Empty).Trim('\\');
            IsExecutingFile = isExecFile;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    [Serializable]
    public sealed class ServerAssemblyInfo : FileAssemblyInfo
    {
        public Uri UriFilePath { get; }
        public string DestinationFilePath { get; }
        public BuldPerformerType Type { get; }
        public string MD5 { get; }
        public string Description { get; }

        public ServerAssemblyInfo(DuplicateDictionary<string, string> collectionAttr, Uri server, string assemblyDirPath)
        {
            string versionStr = collectionAttr["Version"]?.First() ?? "1.0.0.0";
            if (!BuildNumber.TryParse(versionStr, out BuildNumber getVers))
            {
                BuildNumber.TryParse("1.0.0.0", out getVers);
            }
            Build = getVers;
            FileName = (collectionAttr["Location"]?.First() ?? string.Empty).Trim('\\');
            DestinationFilePath = Path.Combine(assemblyDirPath, FileName);
            Description = collectionAttr["Description"]?.First() ?? string.Empty;
            MD5 = collectionAttr["MD5"]?.First() ?? string.Empty;

            string buildPerfType = collectionAttr["Type"]?.First() ?? string.Empty;
            if (buildPerfType.Like("Update"))
                Type = BuldPerformerType.Update;
            else if (buildPerfType.Like("RollBack"))
                Type = BuldPerformerType.RollBack;
            else if (buildPerfType.Like("CreateOrUpdate"))
                Type = BuldPerformerType.CreateOrUpdate;
            else if (buildPerfType.Like("Remove"))
                Type = BuldPerformerType.Remove;
            else
                Type = BuldPerformerType.None;

            if (Type != BuldPerformerType.Remove)
            {
                FilePath = Path.GetTempFileName(); // Gets the temp file path for the downloaded file
                UriFilePath = new Uri($"{server.AbsoluteUri}/{FileName.Replace("\\", "/")}");
            }
        }

        public override string ToString()
        {
            return $"[{Type:G}] {base.ToString()} Uri=[{UriFilePath}]";
        }
    }
}
