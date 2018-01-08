using System;
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

        public static string ReplaceXmlSpecSymbols(this string value)
        {
            string result = value;
            // реплейсим спец символы xml в обычный текст
            while (result.IndexOf("&amp;", StringComparison.Ordinal) != -1)
            {
                result = result.Replace(@"&amp;", @"&");
            }
            result = result.Replace(@"&lt;", @"<").Replace(@"&gt;", @">").Replace(@"&quot;", "\"").Replace(@"&apos;", @"'");
            return result;
        }
    }
}
