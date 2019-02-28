using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Utils.IOExploitation
{
    public static class Customs
    {
        public static string GetLastNameInPath(this string path, bool trimFormat = false)
        {
            string[] spltStr = path.Split('\\');
            string fileName = spltStr[spltStr.Length - 1];
            if (!trimFormat)
                return fileName;
            else
            {
                string[] spltFile = fileName.Split('.');
                return spltFile.Length <= 1 ? fileName : string.Join(".", spltFile.Take(spltFile.Length - 1));
            }
        }

        public static string GetParentDirectoryInPath(this string path)
        {
            string[] spltStr = path.Split('\\');
            if (spltStr.Length <= 1)
                return path;
            IEnumerable<string> parentDir = spltStr.Take(spltStr.Length - 1);
            return string.Join("\\", parentDir);
        }

        public static void AddAllAccessPermissions(string filePath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(filePath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl,
                InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);

            FileSecurity access = File.GetAccessControl(filePath);
            SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            access.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.ReadAndExecute, AccessControlType.Allow));
            //access.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            File.SetAccessControl(filePath, access);
        }
    }
}
