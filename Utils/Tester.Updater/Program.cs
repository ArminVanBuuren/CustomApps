using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils.Builds;
using Utils.Builds.Updater;

namespace Tester.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            Start:
            System.Console.WriteLine("Press \"1\" for UPDATE or \"2\" for UPLOAD.");
            string res = System.Console.ReadLine();
            if (res == "1")
            {
                Update();
            }
            else if (res == "2")
            {
                Upload();
            }
            else
            {
                goto Start;
            }

            System.Console.ReadLine();
        }

        public static void Update()
        {
            System.Console.WriteLine(@"Enter uri path. Like - https://raw.githubusercontent.com/ArminVanBuuren/TFSAssist/master");
            string uriPath = System.Console.ReadLine();
            BuildUpdater up = new BuildUpdater(Assembly.GetExecutingAssembly(), uriPath, 1);
            up.UpdateOnNewVersion += Up_FindedNewVersions;
            up.OnProcessingError += Up_OnProcessingError;
            System.Console.WriteLine($"{nameof(BuildUpdater)} created!");
        }
        private static void Up_FindedNewVersions(object sender, BuildUpdaterArgs buildPack)
        {
            buildPack.Result = UpdateBuildResult.Update;
        }

        private static void Up_OnProcessingError(object sender, BuildUpdaterProcessingArgs args)
        {
            Console.WriteLine($"Error=[{args.Error}] InnerErrorCount=[{args.InnerException.Count}]");
        }


        public static void Upload()
        {
            start:
            System.Console.WriteLine(@"Enter directory path. Like - C:\!MyRepos\CustomApp\Utils\TesterConsole\bin\Uploader\test1\OnServer");
            string dirPath = System.Console.ReadLine();
            if (!Directory.Exists(dirPath))
            {
                System.Console.WriteLine("Incorrect path!");
                goto start;
            }

            try
            {
                BuildInfoVersions bldVers = new BuildInfoVersions(dirPath);
                System.Console.WriteLine($"Build successful assembled! Build:{bldVers.BuildPack}");
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }
    }
}
