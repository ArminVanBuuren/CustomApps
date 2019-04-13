using System.IO;
using System.Text.RegularExpressions;

namespace Script.Control.Handlers.SysObj.Based
{
    public class SystemObjectMatch
    {
        FindBase Parent { get; }
        public FindType SysObjType { get; }
        public string FullPath => Path.Combine(Parent.MainDirectoryPath, SubDirectoryPathWithCurrentName);
        public string DirectoryPath => Path.Combine(Parent.MainDirectoryPath, SubDirectoryPath);
        public string SubDirectoryPathWithCurrentName { get; }
        public string SubDirectoryPath { get; }
        public string Name { get; }

        public SystemObjectMatch(FindBase parent, string subPathAndSysObjName, FindType type)
        {
            Parent = parent;
            Name = Regex.Match(subPathAndSysObjName, @"[^\\]+$").Value;
            SubDirectoryPath = Regex.Match(subPathAndSysObjName, @"^.+\\").Value.Trim('\\');
            SubDirectoryPathWithCurrentName = subPathAndSysObjName;
            SysObjType = type;
        }
        /// <summary>
        /// клонируем входящий объект
        /// </summary>
        /// <param name="sourceSysObj"></param>
        public SystemObjectMatch(SystemObjectMatch sourceSysObj)
        {
            Parent = sourceSysObj.Parent;
            Name = sourceSysObj.Name;
            SubDirectoryPath = sourceSysObj.SubDirectoryPath;
            SubDirectoryPathWithCurrentName = sourceSysObj.SubDirectoryPathWithCurrentName;
            SysObjType = sourceSysObj.SysObjType;
        }
    }
}
