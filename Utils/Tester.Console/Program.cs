using System;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using Utils;
using Utils.Builds.Updater;


namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Fcuk yeah!");
            
            BuildUpdater up = new BuildUpdater(Assembly.GetExecutingAssembly(), @"https://raw.githubusercontent.com/ArminVanBuuren/TFSAssist/master", 1);
            up.UpdateOnNewVersion += Up_FindedNewVersions;
            System.Console.ReadLine();
        }

        private static void Up_FindedNewVersions(object sender, BuildUpdaterArgs buildPack)
        {
            buildPack.Result = UpdateBuildResult.Update;
        }

        static string test()
        {
            using (var client = new WebClient())
            {
                return client.DownloadString("https://raw.githubusercontent.com/ArminVanBuuren/TFSAssist/master/version.xml");
            }
        }
    }
}
