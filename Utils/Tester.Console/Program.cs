using System;
using System.Reflection;
using Utils;
using Utils.BuildUpdater;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //string test = "fefewfefew.fewfew.fewfew.xml".GetLastNameInPath(true);
            BuildUpdater up = new BuildUpdater(Assembly.GetExecutingAssembly(), @"https://raw.githubusercontent.com/ArminVanBuuren/TFSAssist/master");
            System.Console.ReadKey();
        }
    }
}
