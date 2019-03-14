using System;
using System.IO;
using System.Reflection;

namespace Utils
{
    public static class ASSEMBLY
    {
        //ApplicationName = Assembly.GetCallingAssembly().GetName().Name;
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

        public static DateTime GetBuildDate(this Assembly assembly)
        {
            Version version = assembly.GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                .AddDays(version.Build).AddSeconds(version.Revision * 2);
            return buildDate;
        }

        public static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }
    }
}
