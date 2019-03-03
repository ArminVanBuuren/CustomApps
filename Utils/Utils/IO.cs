using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace Utils
{
    public static class IO
    {
        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            //Encoding _utf8WithoutBom = new UTF8Encoding(false);
            //using (var reader = new StreamReader(filename, _utf8WithoutBom, true))
            //{
            //    reader.Peek(); // you need this!
            //    var encoding = reader.CurrentEncoding;
            //}

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.ASCII;
        }

        public static string SafeReadFile(string path, bool convertToLower = false)
        {
            if (!File.Exists(path))
                return null;

            int attempts = 0;
            while (!IsFileReady(path))
            {
                attempts++;
                System.Threading.Thread.Sleep(500);
                if (attempts > 3)
                    return null;
            }

            string context;
            using (StreamReader sr = new StreamReader(path))
            {
                context = convertToLower ? sr.ReadToEnd().ToLower() : sr.ReadToEnd();
            }
            return context;
        }

        public static bool WriteFile(string path, string content)
        {
            File.WriteAllText(path, content, GetEncoding(path));

            //using (StreamWriter writetext = new StreamWriter(path, false, GetEncoding(path)))
            //{
            //    writetext.Write(content);
            //}

            return true;
        }

        public static bool IsFileReady(string filename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return inputStream.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

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