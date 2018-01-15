using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using Protas.Components.Types;

namespace Protas.Components.Functions
{
    public class FStatic
    {
        public static string MachineName => Environment.MachineName;
        public static string UserName => WindowsIdentity.GetCurrent()?.Name;

        public static string ProgramName
        {
            get
            {
                AppDomain root = AppDomain.CurrentDomain;
                //return AppDomain.CurrentDomain.FriendlyName.Replace(".vshost", "");
                return root.FriendlyName;
            }
        }
        public static string ModuleName
        {
            get
            {
                ProcessModule dgr = Process.GetCurrentProcess().MainModule;
                return dgr.ModuleName.Replace(dgr.FileVersionInfo.Comments, "").Trim(' ', '.');
            }
        }

        public static void DisposeObject(object obj)
        {
            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public |
                                                                BindingFlags.NonPublic |
                                                                BindingFlags.Instance))
            {
                //if (!Attribute.IsDefined(field, typeof(IResource)))
                //    continue;
                object val = field.GetValue(obj);
                if (val == null)
                    continue;
                IDisposable disposable = val as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
                else if (val.GetType().IsCOMObject)
                {
                    // на всякий случай добавим сюда 
                    // и освобождение COM-объектов
                    Marshal.ReleaseComObject(val);
                }
            }
        }
        public static string GetMachineName()
        {
            return MachineName;
        }

        public static string LocalPath { get
        {
            //var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //if (directoryName != null)
            //    return directoryName.Replace(ModuleName, "");
            //return string.Empty;
            string result = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return result;
        }
        }
        //string _sourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "");
        public static string GetOsVersion => GetOsVs.ToString("g");

        static OSNames GetOsVs
        {
            get
            {
                OperatingSystem osInfo = Environment.OSVersion;
                switch (osInfo.Platform)
                {
                    case PlatformID.Win32Windows:

                        switch (osInfo.Version.Minor)
                        {
                            case 0: return OSNames.Win95;
                            case 10:
                                return (osInfo.Version.Revision.ToString(CultureInfo.InvariantCulture) == "2222A") ?
                                       OSNames.Win98SecondEdition : OSNames.Win98;
                            case 90: return OSNames.WinMe;
                        }
                        break;

                    // Platform is Windows NT 3.51, Windows NT 4.0, Windows 2000,
                    // or Windows XP.
                    case PlatformID.Win32NT:
                        switch (osInfo.Version.Major)
                        {
                            case 3: return OSNames.WinNt351;
                            case 4: return OSNames.WinNt40;
                            case 5: return (osInfo.Version.Minor == 0) ? OSNames.Win2000 : OSNames.WinXp;
                            case 6: return OSNames.Win8;
                        }
                        break;
                }
                return OSNames.None;
            }
        }

        /// <summary>
        /// Принимает аргумент "0;1;2;3;4;5;6" формат дней недели
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public static bool CheckDay(string days)
        {
            DateTime dNow = DateTime.Now;
            string currentDayOfWeek = dNow.DayOfWeek.ToString("d");
            if (GetTypeEx.IsNumber(currentDayOfWeek))
            {
                int curDay = int.Parse(currentDayOfWeek);
                foreach (string day in days.Split(';'))
                {
                    if (GetTypeEx.IsNumber(day))
                    {
                        if (int.Parse(day) == curDay)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
        public static MemorySizes GetLocalRam()
        {
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            GlobalMemoryStatusEx(memStatus);
            MemorySizes sizeMem = new MemorySizes(memStatus.ullAvailPhys, true, memStatus.ullTotalPhys);
            //int divider = 1024;
            //ulong outSizeMemory = 0;
            //if (props.Count == 1 && !string.IsNullOrEmpty(props[0]))
            //    divider = Devider(props[0]);
            //if (props.Count >= 2)
            //{
            //    divider = Devider(props[1]);
            //    if (GlobalMemoryStatusEx(memStatus))
            //    {
            //        switch (props[0].Trim().ToLower())
            //        {
            //            case "avail": outSizeMemory = memStatus.ullAvailPhys; break;
            //            case "used": outSizeMemory = memStatus.ullTotalPhys - memStatus.ullAvailPhys; break;
            //            case "total": outSizeMemory = memStatus.ullTotalPhys; break;
            //            default: outSizeMemory = memStatus.ullTotalPhys; break;
            //        }
            //    }
            //}
            //else
            //{
            //    if (GlobalMemoryStatusEx(memStatus))
            //        outSizeMemory = memStatus.ullTotalPhys;
            //}
            //decimal dc = decimal.Parse(outSizeMemory.ToString());
            //decimal outDc = dc;
            //if (divider != 0)
            //    outDc = Math.Round((dc != 0) ? dc / divider : 0);
            return sizeMem;
        }

        internal static int Devider(string str)
        {
            string prop1 = str.Trim();
            if (prop1 == "b")
                return 1024;
            if (prop1 == "mb")
                return 1024*1024;
            if (prop1 == "gb")
                return 1024*1024*1024;
            return 0;
        }

        [DllImport("advapi32.DLL", SetLastError = true)]
        public static extern int LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType,
        int dwLogonProvider, ref IntPtr phToken);
        private void CopyFile(object sender, EventArgs e)
        {
            WindowsIdentity wid_current = WindowsIdentity.GetCurrent();
            WindowsImpersonationContext wic = null;
            try
            {
                IntPtr adminToken = new IntPtr();
                if (LogonUser(@"bob2\apple", "abcd.dyndns.org:1234", "password", 9, 0, ref adminToken) != 0)
                {
                    using (wic = WindowsIdentity.Impersonate(adminToken))
                    {
                        // these operations are executed as impersonated user
                        File.Copy(@"", @"", true);
                        //Copy Succeeded;
                    }
                }
                else
                {
                    //Copy Failed;
                }
            }
            catch
            {
                int ret = Marshal.GetLastWin32Error();
                //MessageBox.Show(ret.ToString(), "Error code: " + ret.ToString());
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                wic?.Undo();
            }
        }
    }

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class Impersonation : IDisposable
    {
        private readonly SafeTokenHandle _handle;
        private readonly WindowsImpersonationContext _context;

        const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        public Impersonation(string domain, string username, string password)
        {
            var ok = LogonUser(username, domain, password,
                           LOGON32_LOGON_NEW_CREDENTIALS, 0, out this._handle);
            if (!ok)
            {
                var errorCode = Marshal.GetLastWin32Error();
                throw new ApplicationException(string.Format("Could not impersonate the elevated user.  LogonUser returned error code {0}.", errorCode));
            }

            _context = WindowsIdentity.Impersonate(this._handle.DangerousGetHandle());
        }

        public void Dispose()
        {
            _context.Dispose();
            _handle.Dispose();
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeTokenHandle()
                : base(true)
            { }

            [DllImport("kernel32.dll")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }
    }

}
