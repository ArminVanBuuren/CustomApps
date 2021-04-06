using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Script.Control;
using XPackage;

namespace TestScriptApp
{
    static class Program
    {
        public static string LocalPath => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        static void Main(string[] args)
        {
            try
            {
	            var _configPath = Path.Combine(LocalPath, "Script.Config.sxml");
	            if (!File.Exists(_configPath))
	            {
		            using (var writer = new StreamWriter(_configPath, false, Functions.Enc))
		            {
			            //writer.Write(Properties.Resources.Script_Config);
			            writer.Write(ScriptTemplate.GetExampleOfConfig());
			            writer.Close();
		            }
	            }
	            var cnfg = _configPath.LoadFileByPath();


                Console.WriteLine("Processing...");
                var xdoc = new XmlDocument();
                
                xdoc.Load(_configPath);
                var st = new ScriptTemplate(xdoc);
                Console.WriteLine("Complete!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception!");
                Console.WriteLine(e);
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    Console.WriteLine(e);
                }
            }

            Console.ReadKey();
        }

        public static string LoadFileByPath(this string path)
        {
	        string result;
	        using (var stream = new StreamReader(path))
	        {
		        result = stream.ReadToEnd();
		        stream.Close();
	        }
	        return result;
        }
    }
}
