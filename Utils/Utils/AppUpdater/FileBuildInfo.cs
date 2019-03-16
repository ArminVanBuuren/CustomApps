using System;
using System.Xml.Serialization;

namespace Utils.AppUpdater
{
    [Serializable]
    public enum BuldPerformerType
    {
        None = 0,
        Update = 1,
        RollBack = 2,
        Remove = 4,
        CreateOrUpdate = 8,
        CreateOrRollBack = 16
    }

    [Serializable, XmlRoot("Build", IsNullable = false)]
    public class FileBuildInfo
    {
        private string _location = string.Empty;

        public FileBuildInfo()
        {
        }

        public FileBuildInfo(string file, string assemblyDirPath, bool isExecFile)
        {
            Version = BuildNumber.FromFile(file);
            Type = BuldPerformerType.None;
            Location = file.Replace(assemblyDirPath, string.Empty).Trim('\\');
            Description = @"Fixed and improved";
            IsExecutingFile = isExecFile;
        }

        [XmlElement("Version")]
        public string VersionString
        {
            get => Version.ToString();
            set
            {
                if (!BuildNumber.TryParse(value, out BuildNumber getVers))
                {
                    BuildNumber.TryParse("1.0.0.0", out getVers);
                }

                Version = getVers;
            }
        }

        [XmlIgnore]
        public BuildNumber Version { get; set; }

        [XmlElement("Type")]
        public string TypeString
        {
            get => Type.ToString("G");
            set
            {
                if (value.Like("Update"))
                    Type = BuldPerformerType.Update;
                else if (value.Like("RollBack"))
                    Type = BuldPerformerType.RollBack;
                else if (value.Like("Remove"))
                    Type = BuldPerformerType.Remove;
                else if (value.Like("CreateOrUpdate") || value.Like("Create"))
                    Type = BuldPerformerType.CreateOrUpdate;
                else if (value.Like("CreateOrRollBack"))
                    Type = BuldPerformerType.CreateOrRollBack;
                else
                    Type = BuldPerformerType.None;
            }
        }

        [XmlIgnore]
        public BuldPerformerType Type { get; set; }

        /// <summary>
        /// названия файлов и папок в ноде location регистрзависимы для URI!!!!
        /// </summary>
        [XmlElement]
        public string Location
        {
            get => _location;
            set
            {
                if (value.IsNullOrEmptyTrim())
                    throw new ArgumentNullException("Location of file is null");

                _location = value;
            }
        }

        [XmlElement]
        public string Description { get; set; }

        [XmlIgnore]
        public bool IsExecutingFile { get; set; }

        public override string ToString()
        {
            return $"Location=[{Location}] Version=[{Version}] Type=[{Type:G}]";
        }
    }
}
