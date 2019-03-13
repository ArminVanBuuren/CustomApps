using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp.Handlers;
using Utils;
using Utils.Builds;
using Utils.Builds.Updater;
using System.Diagnostics;

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
            //System.Console.WriteLine(@"Enter builds directory path. Like - C:\!MyRepos\CustomApp\Utils\TesterConsole\bin\Uploader\test1\OnServer");
            //string sourcePath = System.Console.ReadLine();
            //System.Console.WriteLine(@"Enter destination directory path. Like - C:\!Builds\TFSAssist");
            //string destPath = System.Console.ReadLine();
            //System.Console.WriteLine($"Enter git repos, to push changes in remote server. Like - https://github.com/ArminVanBuuren/TFSAssist");
            //string remoteGitRepos = System.Console.ReadLine();

            string sourcePath = @"C:\!MyRepos\CustomApp\Utils\TesterConsole\bin\Uploader\test1\OnServer";
            string destPath = @"C:\!Builds\TFSAssist";
            string remoteGitRepos = @"https://github.com/ArminVanBuuren/TFSAssist";

            if (!Directory.Exists(sourcePath))
            {
                System.Console.WriteLine("Incorrect path!");
                goto start;
            }

            try
            {
                if (Directory.Exists(destPath))
                    ClearAllRepos(destPath);

                UsernamePasswordCredentials credentials = new UsernamePasswordCredentials
                {
                    Username = "ArminVanBuuren",
                    Password = "ArminOnly#3"
                };

                CloneOptions options = new CloneOptions
                {
                    CredentialsProvider = (_url, _user, _cred) => credentials
                };

                PushOptions pushIOpt = new PushOptions
                {
                    CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) => credentials)
                };

                Identity identity = new Identity("ArminVanBuuren", "vkhovanskiy@gmail.com");
                Signature signature = new Signature(identity, DateTimeOffset.Now);

                BuildVersionsInfo bldVers;
                string path = Repository.Init(destPath, false);
                using (var repo = new Repository(path, new RepositoryOptions { Identity = identity }))
                {
                    // создаем локальную ветку
                    //https://stackoverflow.com/questions/8285627/linq-exception-as-sequence-contains-no-elements
                    Remote remote = repo.Network.Remotes.Where(p => p.Name == "origin").FirstOrDefault();
                    if (remote == null)
                        remote = repo.Network.Remotes.Add("origin", remoteGitRepos);

                    // вытягиваем весь репозиторий с сервера 
                    Commands.Fetch(repo, "origin", new string[0], new FetchOptions
                    {
                        CredentialsProvider = options.CredentialsProvider
                    }, null);
                    // переносим все в локальную папку Fetch + Merge - это тоже самое что и pull
                    repo.Merge(repo.Branches["origin/master"], signature);

                    ClearAllRepos(destPath, false);
                    bldVers = new BuildVersionsInfo(sourcePath, destPath);

                    // git Add. Индексируем изменения
                    Commands.Stage(repo, "*");
                    // Делаем снимок проиндексированных файлов
                    repo.Commit($"Initialized newest build - \"{bldVers.BuildPack}\"", signature, signature);


                    // локальную ветку origin/master переносим в master что на сервере
                    repo.Branches.Update(repo.Head,
                        b => b.Remote = remote.Name,
                        b => b.UpstreamBranch = repo.Head.CanonicalName);
                    // заливаем на сервер в ветку master
                    repo.Network.Push(repo.Branches["master"], pushIOpt);
                }

                ClearAllRepos(destPath);
                Directory.Delete(destPath);

                System.Console.WriteLine($"Build successful assembled! Build:{bldVers.BuildPack}");
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }

        public static void ClearAllRepos(string directoryPath, bool deleteGit = true)
        {
            DirectoryInfo di = new DirectoryInfo(directoryPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                if (dir.Name.Like(".git"))
                    if (!deleteGit)
                        continue;

                dir.Delete(true);
            }
        }
    }
}
