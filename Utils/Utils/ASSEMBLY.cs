using System;
using System.IO;
using System.Reflection;

namespace Utils
{
    public static class ASSEMBLY
    {
        public static string ApplicationName = Assembly.GetEntryAssembly().GetName().Name;
        public static string ApplicationPath = Assembly.GetEntryAssembly().Location;
        public static string ApplicationDirectory => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Assembly.GetEntryAssembly().GetName().Name);
        public static string ApplicationFilePath => System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

        public static string GetDirectory(this Assembly assembly)
        {
            //string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(assembly.CodeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
