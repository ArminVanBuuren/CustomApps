using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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

        /// <summary>
        /// Add all access permissions to file
        /// </summary>
        /// <param name="filePath"></param>
        public static void AccessToFile(string filePath)
        {
            AccessToDirectory(filePath);

            FileSecurity access = File.GetAccessControl(filePath);
            SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            access.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.ReadAndExecute, AccessControlType.Allow));
            //access.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            File.SetAccessControl(filePath, access);
        }

        /// <summary>
        /// Add all access permissions to directory
        /// </summary>
        /// <param name="dirPath"></param>
        public static void AccessToDirectory(string dirPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dirPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl,
                InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }

        public static void SetFullControlPermissionsToEveryone(string dirPath)
        {
            const FileSystemRights rights = FileSystemRights.FullControl;

            var allUsers = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);

            // Add Access Rule to the actual directory itself
            var accessRule = new FileSystemAccessRule(
                allUsers,
                rights,
                InheritanceFlags.None,
                PropagationFlags.NoPropagateInherit,
                AccessControlType.Allow);

            var info = new DirectoryInfo(dirPath);
            var security = info.GetAccessControl(AccessControlSections.Access);

            security.ModifyAccessRule(AccessControlModification.Set, accessRule, out var result);

            if (!result)
            {
                throw new InvalidOperationException("Failed to give full-control permission to all users for path " + dirPath);
            }

            // add inheritance
            var inheritedAccessRule = new FileSystemAccessRule(
                allUsers,
                rights,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.InheritOnly,
                AccessControlType.Allow);

            security.ModifyAccessRule(AccessControlModification.Add, inheritedAccessRule, out var inheritedResult);

            if (!inheritedResult)
            {
                throw new InvalidOperationException("Failed to give full-control permission inheritance to all users for " + dirPath);
            }

            info.SetAccessControl(security);
        }

        public static void CopyDirectory(string sourcePath, string destinationPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
        }

        /// <summary>
        /// Recursively deletes a directory as well as any subdirectories and files. If the files are read-only, they are flagged as normal and then deleted.
        /// </summary>
        /// <param name="directory">The name of the directory to remove.</param>
        public static void DeleteReadOnlyDirectory(string directory)
        {
            foreach (var subdirectory in Directory.EnumerateDirectories(directory))
            {
                DeleteReadOnlyDirectory(subdirectory);
            }
            foreach (var fileName in Directory.EnumerateFiles(directory))
            {
                var fileInfo = new FileInfo(fileName);
                fileInfo.Attributes = FileAttributes.Normal;
                fileInfo.Delete();
            }
            Directory.Delete(directory);
        }

        public static Task DeleteReadOnlyDirectoryAsync(string directory)
        {
            return Task.Run(() =>
            {
                try
                {
                    foreach (var subdirectory in Directory.EnumerateDirectories(directory))
                    {
                        DeleteReadOnlyDirectory(subdirectory);
                    }

                    foreach (var fileName in Directory.EnumerateFiles(directory))
                    {
                        var fileInfo = new FileInfo(fileName);
                        fileInfo.Attributes = FileAttributes.Normal;
                        fileInfo.Delete();
                    }

                    Directory.Delete(directory);
                }
                catch(Exception)
                {
                    //null
                }
            });
        }

        public static string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        static ArrayList tempGetFileProcesses(string strFile)
        {
            ArrayList myProcessArray = new ArrayList();
            Process myProcess;

            Process[] processes = Process.GetProcesses();
            int i = 0;
            for (i = 0; i <= processes.GetUpperBound(0) - 1; i++)
            {
                myProcess = processes[i];
                //if (!myProcess.HasExited) //This will cause an "Access is denied" error
                if (myProcess.Threads.Count > 0)
                {
                    try
                    {
                        ProcessModuleCollection modules = myProcess.Modules;
                        int j = 0;
                        for (j = 0; j <= modules.Count - 1; j++)
                        {
                            if ((modules[j].FileName.ToLower().CompareTo(strFile.ToLower()) == 0))
                            {
                                myProcessArray.Add(myProcess);
                                break;
                                // TODO: might not be correct. Was : Exit For
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        //MsgBox(("Error : " & exception.Message)) 
                    }
                }
            }

            return myProcessArray;
        }

        public static long GetTotalFreeSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.TotalFreeSpace;
                }
            }
            return -1;
        }


        /// <summary>
        /// Formats the byte count to closest byte type
        /// </summary>
        /// <param name="bytes">The amount of bytes</param>
        /// <param name="newBytes"></param>
        /// <param name="decimalPlaces">How many decimal places to show</param>
        /// <param name="showByteType">Add the byte type on the end of the string</param>
        /// <returns>The bytes formatted as specified</returns>
        public static string FormatBytes(long bytes, out double newBytes, int decimalPlaces = 1, bool showByteType = true)
        {
            newBytes = bytes;
            string formatString = "{0";
            string byteType = "B";

            if (newBytes <= 1024)
            {
                newBytes = bytes;
            }
            else if (newBytes > 1024 && newBytes < 1048576)
            {
                newBytes /= 1024;
                byteType = "KB";
            }
            else if (newBytes > 1048576 && newBytes < 1073741824)
            {
                // Check if best size in MB
                newBytes /= 1048576;
                byteType = "MB";
            }
            else
            {
                // Best size in GB
                newBytes /= 1073741824;
                byteType = "GB";
            }

            // Show decimals
            if (decimalPlaces > 0)
                formatString += ":0.";

            // Add decimals
            for (int i = 0; i < decimalPlaces; i++)
                formatString += "0";

            // Close placeholder
            formatString += "}";

            // Add byte type
            if (showByteType)
                formatString += byteType;

            return string.Format(formatString, newBytes);
        }


        /// <summary>
        /// Return a string describing the value as a file size.
        /// For example, 1.23 MB.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToFileSize(this double value)
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            for (int i = 0; i < suffixes.Length; i++)
            {
                if (value <= (Math.Pow(1024, i + 1)))
                {
                    return ThreeNonZeroDigits(value / Math.Pow(1024, i)) + " " + suffixes[i];
                }
            }

            return ThreeNonZeroDigits(value / Math.Pow(1024, suffixes.Length - 1)) +
                   " " + suffixes[suffixes.Length - 1];
        }

        private static string ThreeNonZeroDigits(double value)
        {
            if (value >= 100)
            {
                // No digits after the decimal.
                return value.ToString("0,0");
            }
            else if (value >= 10)
            {
                // One digit after the decimal.
                return value.ToString("0.0");
            }
            else
            {
                // Two digits after the decimal.
                return value.ToString("0.00");
            }
        }


        #region Who is looking of file

        [StructLayout(LayoutKind.Sequential)]
        struct RM_UNIQUE_PROCESS
        {
            public int dwProcessId;
            public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }

        const int RmRebootReasonNone = 0;
        const int CCH_RM_MAX_APP_NAME = 255;
        const int CCH_RM_MAX_SVC_NAME = 63;

        enum RM_APP_TYPE
        {
            RmUnknownApp = 0,
            RmMainWindow = 1,
            RmOtherWindow = 2,
            RmService = 3,
            RmExplorer = 4,
            RmConsole = 5,
            RmCritical = 1000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct RM_PROCESS_INFO
        {
            public RM_UNIQUE_PROCESS Process;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
            public string strAppName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
            public string strServiceShortName;

            public RM_APP_TYPE ApplicationType;
            public uint AppStatus;
            public uint TSSessionId;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bRestartable;
        }

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        static extern int RmRegisterResources(uint pSessionHandle,
                                              UInt32 nFiles,
                                              string[] rgsFilenames,
                                              UInt32 nApplications,
                                              [In] RM_UNIQUE_PROCESS[] rgApplications,
                                              UInt32 nServices,
                                              string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
        static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        static extern int RmEndSession(uint pSessionHandle);

        [DllImport("rstrtmgr.dll")]
        static extern int RmGetList(uint dwSessionHandle,
                                    out uint pnProcInfoNeeded,
                                    ref uint pnProcInfo,
                                    [In, Out] RM_PROCESS_INFO[] rgAffectedApps,
                                    ref uint lpdwRebootReasons);

        /// <summary>
        /// Find out what process(es) have a lock on the specified file.
        /// </summary>
        /// <param name="path">Path of the file.</param>
        /// <returns>Processes locking the file</returns>
        /// <remarks>See also:
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa373661(v=vs.85).aspx
        /// http://wyupdate.googlecode.com/svn-history/r401/trunk/frmFilesInUse.cs (no copyright in code at time of viewing)
        /// 
        /// </remarks>
        static public List<Process> WhoIsLocking(string path)
        {
            uint handle;
            string key = Guid.NewGuid().ToString();
            List<Process> processes = new List<Process>();

            int res = RmStartSession(out handle, 0, key);
            if (res != 0) throw new Exception("Could not begin restart session.  Unable to determine file locker.");

            try
            {
                const int ERROR_MORE_DATA = 234;
                uint pnProcInfoNeeded = 0,
                     pnProcInfo = 0,
                     lpdwRebootReasons = RmRebootReasonNone;

                string[] resources = new string[] { path }; // Just checking on one resource.

                res = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);

                if (res != 0) throw new Exception("Could not register resource.");

                //Note: there's a race condition here -- the first call to RmGetList() returns
                //      the total number of process. However, when we call RmGetList() again to get
                //      the actual processes this number may have increased.
                res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);

                if (res == ERROR_MORE_DATA)
                {
                    // Create an array to store the process results
                    RM_PROCESS_INFO[] processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
                    pnProcInfo = pnProcInfoNeeded;

                    // Get the list
                    res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);
                    if (res == 0)
                    {
                        processes = new List<Process>((int)pnProcInfo);

                        // Enumerate all of the results and add them to the 
                        // list to be returned
                        for (int i = 0; i < pnProcInfo; i++)
                        {
                            try
                            {
                                processes.Add(Process.GetProcessById(processInfo[i].Process.dwProcessId));
                            }
                            // catch the error -- in case the process is no longer running
                            catch (ArgumentException) { }
                        }
                    }
                    else throw new Exception("Could not list processes locking resource.");
                }
                else if (res != 0) throw new Exception("Could not list processes locking resource. Failed to get size of result.");
            }
            finally
            {
                RmEndSession(handle);
            }

            return processes;
        }

        #endregion

        const string argument_update = "/C choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\" & choice /C Y /N /D Y /T 2 & Move /Y \"{1}\" \"{2}\"";
        const string argument_add = "/C choice /C Y /N /D Y /T 4 & Move /Y \"{0}\" \"{1}\"";
        const string argument_remove = "/C choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\"";
        public static void DeleteFileAfterAccessClose(string file)
        {
            string argument_complete = string.Format(argument_remove, file);
            StartProcess(argument_complete);
        }

        static void StartProcess(string arguments)
        {
            ProcessStartInfo cmd = new ProcessStartInfo
            {
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };

            Process.Start(cmd);
        }
    }
}