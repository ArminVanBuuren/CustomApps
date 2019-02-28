using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utils.AssemblyHelper
{
    public static class Customs
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
