using System;
using System.Reflection;
using Utils.AssemblyHelper;
using Utils.BuildUpdater;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Fcuk yeah!");
            //string test = "fefewfefew.fewfew.fewfew.xml".GetLastNameInPath(true);
            BuildUpdater up = new BuildUpdater(Assembly.GetExecutingAssembly(), @"https://raw.githubusercontent.com/ArminVanBuuren/TFSAssist/master", 1);
            up.UpdateOnNewVersion += Up_FindedNewVersions;
            System.Console.ReadLine();
        }

        private static void Up_FindedNewVersions(object sender, BuildUpdaterArgs buildPack)
        {
            buildPack.Result = UpdateBuildResult.Update;
        }
    }
}
