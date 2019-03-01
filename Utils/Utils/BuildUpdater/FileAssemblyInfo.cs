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
    public enum BuldPerformerType
    {
        None = 0,
        CreateOrUpdate = 1,
        Update = 2,
        RollBack = 4,
        Remove = 8
    }
    public abstract class FileAssemblyInfo
    {
        protected static BuildNumber GetBuild(string version)
        {
            if (!BuildNumber.TryParse(version, out BuildNumber getVers))
            {
                BuildNumber.TryParse("1.0.0.0", out getVers);
            }

            return getVers;
        }

        public BuildNumber Build { get; protected set; }
        public string FileName { get; protected set; }
        public string Description { get; protected set; }
    }

    public sealed class LocalAssemblyInfo : FileAssemblyInfo
    {
        public string FilePath { get; }
        public LocalAssemblyInfo(FileVersionInfo fileInfo, string directoryPath)
        {
            Build = GetBuild(fileInfo.FileVersion);
            FileName = fileInfo.OriginalFilename ?? Path.GetFileName(fileInfo.FileName);
            FilePath = Path.Combine(directoryPath, FileName);
            Description = string.Empty;
        }

        public LocalAssemblyInfo(string version, string directoryPath, string fileName, string description = null)
        {
            Build = GetBuild(version);
            FileName = fileName;
            FilePath = Path.Combine(directoryPath, FileName);
            Description = description;
        }
    }

    public sealed class ServerAssemblyInfo : FileAssemblyInfo
    {
        public Uri UriFilePath { get; }
        public BuldPerformerType Type { get; }
        public string MP5 { get; }

        public ServerAssemblyInfo(DuplicateDictionary<string, string> dictionary, Uri server)
        {
            string versionStr = dictionary["Version"]?.First() ?? "1.0.0.0";
            Build = GetBuild(versionStr);
            FileName = dictionary["FileName"]?.First() ?? string.Empty;

            string dirPath = dictionary["DirPath"]?.First();
            if (!dirPath.IsNullOrEmptyTrim())
            {
                UriFilePath = new Uri($"{server.AbsoluteUri}/{dirPath}/{FileName}");
            }
            else
            {
                UriFilePath = new Uri($"{server.AbsoluteUri}/{FileName}");
            }
            
            Description = dictionary["Description"]?.First() ?? string.Empty;
            MP5 = dictionary["MP5"]?.First() ?? string.Empty;

            string buildPerfType = dictionary["Type"]?.First() ?? string.Empty;
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
        }
    }


}
