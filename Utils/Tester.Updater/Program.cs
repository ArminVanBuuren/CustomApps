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
using Utils.AppUpdater;
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
            ApplicationUpdater up = new ApplicationUpdater(Assembly.GetExecutingAssembly(), "Tester.Updater", "QJedWja49u4vlnS.zip", 1);
            up.OnFetch += Up_OnFetch;
            up.OnUpdate += Up_OnUpdate;
            up.OnProcessingError += Up_OnProcessingError;
            up.Start();
            System.Console.WriteLine($"{nameof(ApplicationUpdater)} created!");
        }

        private static void Up_OnFetch(object sender, ApplicationUpdaterArgs buildPack)
        {
            Console.WriteLine($"Start fetching... Object=[{sender}]");
            Thread.Sleep(2000);
        }

        private static void Up_OnUpdate(object sender, ApplicationUpdaterArgs buildPack)
        {
            Console.WriteLine($"Start update... Object=[{sender}]");
            Thread.Sleep(2000);
            buildPack.Result = UpdateBuildResult.Update;
        }

        private static void Up_OnProcessingError(object sender, ApplicationUpdaterProcessingArgs args)
        {
            Console.WriteLine($"Object=[{sender}] Error=[{args.Error}] InnerErrorCount=[{args.InnerException.Count}]");
        }


        public static void Upload()
        {
            start:

            Repository repo = null;
            try
            {   
                //System.Console.WriteLine(@"Enter builds directory path. Like - C:\!MyRepos\CustomApp\Utils\Tester.Updater\bin\Uploader\test1\OnServer");
                string sourcePath = @"C:\!Builds\Builds";
                string destPath = @"C:\!Builds\Git";
                System.Console.WriteLine($"Enter project name. Like - TFSAssist; Tester.Updater");
                string projectStr = System.Console.ReadLine();

                if (!Directory.Exists(sourcePath))
                {
                    System.Console.WriteLine($"Incorrect path '{sourcePath}'");
                    goto start;
                }

                if (Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories).Length == 0)
                {
                    System.Console.WriteLine($"No files was found in '{sourcePath}'");
                    goto start;
                }

                if (Directory.Exists(destPath))
                {
                    IO.AccessToDirectory(destPath);
                    IO.DeleteReadOnlyDirectory(destPath);
                }

                UsernamePasswordCredentials credentials = new UsernamePasswordCredentials
                {
                    Username = "ArminVanBuuren",
                    Password = "ArminOnly#3"
                };

                CloneOptions fetchOptions = new CloneOptions
                {
                    CredentialsProvider = (_url, _user, _cred) => credentials
                };

                PushOptions pushOptions = new PushOptions
                {
                    CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) => credentials)
                };

                Identity identity = new Identity(credentials.Username, "vkhovanskiy@gmail.com");
                Signature signature = new Signature(identity, DateTimeOffset.Now);

                Console.WriteLine($"Start");
                Directory.CreateDirectory(destPath);
                string path = Repository.Init(destPath, false);
                using (repo = new Repository(path, new RepositoryOptions {Identity = identity}))
                {
                    // создаем локальную ветку
                    Remote remote = repo.Network.Remotes.Where(p => p.Name == "origin").FirstOrDefault();
                    if (remote == null)
                        remote = repo.Network.Remotes.Add("origin", BuildsInfo.DEFAULT_PROJECT_GIT);

                    // вытягиваем весь репозиторий с сервера 
                    Commands.Fetch(repo, "origin", new string[0], new FetchOptions
                    {
                        CredentialsProvider = fetchOptions.CredentialsProvider
                    }, null);
                    // переносим все в локальную папку Fetch + Merge - это тоже самое что и pull
                    repo.Merge(repo.Branches["origin/master"], signature);


                    string fileVersionsPath = Path.Combine(destPath, BuildsInfo.FILE_NAME);
                    BuildsInfo buildVersions = null;
                    if (File.Exists(fileVersionsPath))
                    {
                        string currentFileVersions = File.ReadAllText(fileVersionsPath);
                        buildVersions = BuildsInfo.Deserialize(currentFileVersions);
                    }
                    else
                    {
                        buildVersions = new BuildsInfo();
                    }

                    buildVersions.Add(projectStr, sourcePath, destPath);
                    Console.WriteLine($"You should correct file:'{fileVersionsPath}'. Then press any key for continue.");
                    Console.ReadKey();


                    // git Add. Индексируем изменения
                    Commands.Stage(repo, "*");
                    // Делаем снимок проиндексированных файлов
                    repo.Commit($"Initialized latest build of the project \"{projectStr}\"", signature, signature);


                    // локальную ветку origin/master переносим в master что на сервере
                    repo.Branches.Update(repo.Head,
                        b => b.Remote = remote.Name,
                        b => b.UpstreamBranch = repo.Head.CanonicalName);
                    // заливаем на сервер в ветку master
                    repo.Network.Push(repo.Branches["master"], pushOptions);
                }

                System.Console.WriteLine($"Build successful assembled! Project=[{projectStr}]");
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
            finally
            {
                if(repo != null)
                    repo.Dispose();
                System.Console.Write($"End");
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
                {
                    if (!deleteGit)
                        continue;
                }

                ClearAllRepos(dir.FullName);
                dir.Delete(true);
            }
        }
    }
}
