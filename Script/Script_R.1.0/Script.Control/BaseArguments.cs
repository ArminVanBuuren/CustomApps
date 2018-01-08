using System.IO;
using System.Reflection;

namespace Script.Control
{
    internal static class BaseArguments
    {
        public static string LocalPath => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        public static string GetLocalPath(this string value)
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }
    }
}
