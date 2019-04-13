using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Script.Control;

namespace TestScriptApp
{
    class Program
    {
        public static string LocalPath => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Processing...");
                XmlDocument xdoc = new XmlDocument();
                string _configPath = Path.Combine(LocalPath, "Script.Config.xml");
                xdoc.Load(_configPath);
                ScriptTemplate st = new ScriptTemplate(xdoc);
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
    }
}
