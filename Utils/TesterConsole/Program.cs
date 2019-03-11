using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utils.Builds.Unloader;
using Utils;
using Utils.Builds.Updater;

namespace TesterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //BuildUnloader dd = new BuildUnloader(Assembly.GetExecutingAssembly().GetDirectory());

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
