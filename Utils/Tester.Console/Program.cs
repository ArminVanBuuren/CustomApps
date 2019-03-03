using System.Reflection;
using Utils.BuildUpdater;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            //string test = "fefewfefew.fewfew.fewfew.xml".GetLastNameInPath(true);
            BuildUpdater up = new BuildUpdater(Assembly.GetExecutingAssembly(), @"https://raw.githubusercontent.com/ArminVanBuuren/TFSAssist/master", 1);
            up.FindedNewVersions += Up_FindedNewVersions;
            System.Console.ReadKey();
        }

        private static void Up_FindedNewVersions(object sender, BuildUpdaterArgs buildPack)
        {
            buildPack.Result = BuildPackResult.Cancel;
        }
    }
}
