using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Utils
{
    public static class ASSEMBLY
    {
        //ApplicationName = Assembly.GetCallingAssembly().GetName().Name;
        public static string ApplicationName => Assembly.GetEntryAssembly()?.GetName().Name;
        public static string ApplicationPath => Assembly.GetEntryAssembly()?.Location;
        //public static string ApplicationDirectory => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Assembly.GetEntryAssembly().GetName().Name);
        public static string ApplicationDirectory => Path.GetDirectoryName(ApplicationPath);
        public static string ApplicationFilePath
        {
            get
            {
                var processModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
                return processModule?.FileName;
            }
        }

        public static string CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version.ToString();

        public static string GetDirectory(this Assembly assembly)
        {
            //string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(assembly.CodeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public static DateTime GetBuildDate(this Assembly assembly)
        {
            var version = assembly.GetName().Version;
            var buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
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

        public static Assembly CustomResolve(object sender, ResolveEventArgs args)
        {
            // System.AppDomain.CurrentDomain.AssemblyResolve += CustomResolve;
            if (args.Name.StartsWith("library"))
            {
                var fileName = Path.GetFullPath("platform\\" + Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") + "\\library.dll");
                
                if (File.Exists(fileName))
                {
                    return Assembly.LoadFile(fileName);
                }
            }

            return null;
        }


        public static string GetPropertiesToString(object @object, BindingFlags flags)
        {
            var builder = new StringBuilder();
            var props = @object.GetType().GetProperties(flags);

            foreach (var prop in props)
            {
                builder.Append($"{prop.Name}=[{prop.GetValue(@object)}]\r\n");
            }

            return builder.ToString().Trim();
        }

        public static Dictionary<string, string> GetPropertiesToList(object @object, BindingFlags flags)
        {
            var properties = new Dictionary<string, string>();
            var props = @object.GetType().GetProperties(flags);

            foreach (var prop in props)
            {
                if (!properties.ContainsKey(prop.Name))
                    properties.Add(prop.Name, prop.GetValue(@object)?.ToString());
            }

            return properties;
        }
    }
}
