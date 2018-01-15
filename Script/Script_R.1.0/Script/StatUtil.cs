using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    internal static class StatUtil
    {
        public static bool IsNullOrEmpty(this string value)
        {
           
            return string.IsNullOrEmpty(value);
        }

        public static string LoadFileByPath(this string path)
        {
            string result;
            using (StreamReader stream = new StreamReader(path))
            {
                result = stream.ReadToEnd();
                stream.Close();
            }
            return result;
        }

        public static string SaveStreamToFile(this string value, string path)
        {
            using (StreamWriter tw = new StreamWriter(path, false))
            {
                // var thisExe = System.Reflection.Assembly.GetExecutingAssembly();
                tw.Write(value);
                tw.Close();
            }
            return value;
        }
    }
}
