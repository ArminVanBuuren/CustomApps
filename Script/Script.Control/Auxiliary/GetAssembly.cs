using System;
using System.Runtime.InteropServices;

namespace Script.Control.Auxiliary
{
    public class GACAssembly
    {
        /// <summary>
        /// Gets an assembly path from the GAC given a partial name.
        /// </summary>
        /// <param name="name">An assembly partial name. May not be null.</param>
        /// <returns>
        /// The assembly path if found; otherwise null;
        /// </returns>
        public static string GetPath(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var finalName = name;
            var aInfo = new AssemblyInfo();
            aInfo.cchBuf = 1024; // should be fine...
            aInfo.currentAssemblyPath = new string('\0', aInfo.cchBuf);

            IAssemblyCache ac;
            var hr = CreateAssemblyCache(out ac, 0);
            if (hr >= 0)
            {
                hr = ac.QueryAssemblyInfo(0, finalName, ref aInfo);
                if (hr < 0)
                    return null;
            }

            return aInfo.currentAssemblyPath;
        }


        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
        private interface IAssemblyCache
        {
            void Reserved0();

            [PreserveSig]
            int QueryAssemblyInfo(int flags, [MarshalAs(UnmanagedType.LPWStr)] string assemblyName, ref AssemblyInfo assemblyInfo);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AssemblyInfo
        {
            public int cbAssemblyInfo;
            public int assemblyFlags;
            public long assemblySizeInKB;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string currentAssemblyPath;
            public int cchBuf; // size of path buf.
        }

        [DllImport("fusion.dll")]
        private static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, int reserved);
    }
}
