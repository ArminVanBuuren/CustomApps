using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Xml.Serialization;
using Utils.Properties;

namespace Utils.AppUpdater.Pack
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

        /// <summary>
        /// Названия файлов и папок в ноде location регистрзависимы для URI!!!!
        /// </summary>
        [XmlElement]
        public string Location
        {
            get => _location;
            set => _location = value.IsNullOrWhiteSpace() ? throw new ArgumentNullException(Resources.InvalidFileLocation) : value;
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement]
        public string AssemblyName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement]
        public string ScopeName { get; set; }

        [XmlElement("Version")]
        public string VersionString
        {
            get => Version.ToString();
            set
            {
                if (!BuildNumber.TryParse(value, out var getVers))
                    BuildNumber.TryParse("1.0.0.0", out getVers);

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

        [XmlElement]
        public string Description { get; set; }

        [XmlIgnore]
        public bool IsExecutingFile { get; set; }

        public FileBuildInfo() { }

        public FileBuildInfo(string file, string assemblyDirPath, bool isExecFile)
        {
            Location = file.Replace(assemblyDirPath, string.Empty).Trim('\\');

            try
            {
                var fileExt = Path.GetExtension(file).ToLower();
                if (fileExt.Equals(".exe") || fileExt.Equals(".dll"))
                {
                    var ass = Assembly.LoadFile(file);
                    AssemblyName = ass.GetName().Name;
                    ScopeName = ass.ManifestModule.ScopeName;
                }
                else
                {
                    AssemblyName = ScopeName = Path.GetFileName(file);
                }
            }
            catch (Exception)
            {
                AssemblyName = ScopeName = Path.GetFileName(file);
            }

            Version = BuildNumber.FromFile(file);
            Type = BuldPerformerType.None;
            Description = @"None";
            IsExecutingFile = isExecFile;
        }

        public override string ToString()
        {
            return $"Assembly = \"{AssemblyName}\" Version = {Version}";
        }
    }
}