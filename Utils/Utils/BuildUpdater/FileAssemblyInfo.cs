using System;
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
        Update = 1,
        RollBack = 2,
        Remove = 4,
        Mandatory = 8
    }

    public struct FileAssemblyInfo
    {
        public FileAssemblyInfo(DuplicateDictionary<string, string> dictionary)
        {
            string versionStr = dictionary["Version"]?.First() ?? "1.0.0.0";
            Build = GetBuild(versionStr);
            DirectoryPath = dictionary["DirPath"]?.First() ?? string.Empty;
            FileName = dictionary["FileName"]?.First() ?? string.Empty;
            Description = dictionary["Description"]?.First() ?? string.Empty;
            string buildPerfType = dictionary["Type"]?.First() ?? string.Empty;
            if (buildPerfType.Equals("Update", StringComparison.CurrentCultureIgnoreCase))
                Type = BuldPerformerType.Update;
            else if (buildPerfType.Equals("RollBack", StringComparison.CurrentCultureIgnoreCase))
                Type = BuldPerformerType.RollBack;
            else
                Type = BuldPerformerType.None;
        }

        public FileAssemblyInfo(string version, string directoryPath, string fileName, string description = null)
        {
            Build = GetBuild(version);
            DirectoryPath = directoryPath;
            FileName = fileName;
            Description = description;
            Type = BuldPerformerType.None;
        }

        public FileAssemblyInfo(FileVersionInfo fileInfo, string directoryPath)
        {
            Build = GetBuild(fileInfo.FileVersion);
            DirectoryPath = directoryPath;
            FileName = fileInfo.OriginalFilename ?? Path.GetFileName(fileInfo.FileName);
            Description = string.Empty;
            Type = BuldPerformerType.None;
        }

        static BuildNumber GetBuild(string version)
        {
            if (!BuildNumber.TryParse(version, out BuildNumber getVers))
            {
                BuildNumber.TryParse("1.0.0.0", out getVers);
            }

            return getVers;
        }

        public BuildNumber Build { get; }
        public string DirectoryPath { get; }
        public string FileName { get; }
        public string Description { get; }
        public BuldPerformerType Type { get; }
    }
}
