using LibGit2Sharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Utils;
using Utils.AppUpdater;

namespace Tester.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            Start:
            Console.WriteLine("Press \"1\" for UPDATE or \"2\" for UPLOAD.");
            var res = Console.ReadLine();
            switch (res)
            {
                case "1":
                    Update();
                    break;
                case "2":
                    Upload();
                    break;
                default:
                    goto Start;
            }

            Console.ReadLine();
        }

        
        private static ApplicationUpdater up;
        public static void Update()
        {
            up = new ApplicationUpdater(Assembly.GetExecutingAssembly(), "QJedWja49u4vlnS.zip", 1);
            up.OnFetch += Up_OnFetch;
            up.OnUpdate += Up_OnUpdate;
            up.OnProcessingError += Up_OnProcessingError;
            up.Start();
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {nameof(ApplicationUpdater)} created!");
        }

        //private static bool infinityChecking = true;
        //static void IsSelfChekker()
        //{
        //    new Action(IsSelfChekker).BeginInvoke(null, null);
        //    Console.ReadKey();
        //    infinityChecking = false;
        //    Console.ReadKey();
        //    infinityChecking = true;
        //    Console.ReadKey();
        //    infinityChecking = false;
        //    while (true)
        //    {
        //        if (infinityChecking)
        //            up.CheckUpdates();
        //        //Console.WriteLine($"[{DateTime.Now:hh:mm:ss}] [{Thread.CurrentThread.ManagedThreadId}] ChekkerReslt=[{result}]");
        //        Thread.Sleep(1000);
        //    }
        //}

        private static void Up_OnFetch(object sender, ApplicationFetchingArgs args)
        {
            Console.WriteLine($"[{DateTime.Now:hh:mm:ss}] [{Thread.CurrentThread.ManagedThreadId}] Fetch. Status=[{up.Status:G}]");
            //Console.WriteLine($"Up_OnFetch. ThreadId=[{Thread.CurrentThread.ManagedThreadId}] Action=[{args.Result:G}] Status=[{up.Status}] IUpdater{args.Control}");

        }

        private static void Up_OnUpdate(object sender, ApplicationUpdatingArgs args)
        {
            Console.WriteLine($"[{DateTime.Now:hh:mm:ss}] [{Thread.CurrentThread.ManagedThreadId}] Update. Status=[{up.Status:G}]");
            args.Control.Dispose();
            up.Refresh();
            //Console.WriteLine($"Up_OnUpdate. ThreadId=[{Thread.CurrentThread.ManagedThreadId}] Action=[{args.Result:G}] Status=[{up.Status}] IUpdater{args.Control}");
            //up.DoUpdate(args.Control);
        }

        private static void Up_OnProcessingError(object sender, ApplicationUpdatingArgs args)
        {
            Console.WriteLine($"Up_OnProcessingError. ThreadId=[{Thread.CurrentThread.ManagedThreadId}] IUpdater{(args.Control == null ? "=[null]" : args.Control.ToString())} Error=[{args.Error}]");
        }

        public static void Upload()
        {
            start:

            Repository repo = null;
            try
            {
                // при первом использовании для начала нужно инитить папку:
                // $ cd -P -- "C:\!Builds\Git"
                // git remote -v
                // git fetch origin master
                // при ошибке fatal: unable to access 'https://github.com/ArminVanBuuren/Builds/': Could not resolve host: github.com выполнить:
                // git config --global --unset http.proxy 
                // git config --global--unset https.proxy

                //Console.WriteLine(@"Enter builds directory path. Like - C:\!MyRepos\CustomApp\Utils\Tester.Updater\bin\Uploader\test1\OnServer");
                const string sourcePath = @"C:\!Builds\Builds";
                const string destPath = @"C:\!Builds\Git";
                Console.WriteLine($"Do not forget library git2-572e4d8.dll with correct bit-depth (x86 or x64)\r\nEnter project name. Like - TFSAssist; Tester.Updater");
                var projectStr = Console.ReadLine();

                if (!Directory.Exists(sourcePath))
                {
                    Console.WriteLine($"Incorrect path '{sourcePath}'");
                    goto start;
                }

                if (Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories).Length == 0)
                {
                    Console.WriteLine($"No files was found in '{sourcePath}'");
                    goto start;
                }

                if (Directory.Exists(destPath))
                {
                    IO.GetAccessToDirectory(destPath);
                    IO.DeleteReadOnlyDirectory(destPath);
                }

                var credentials = new UsernamePasswordCredentials
                {
                    Username = "ArminVanBuuren",
                    Password = "ArminOnly#3"
                };

                var fetchOptions = new CloneOptions
                {
                    CredentialsProvider = (_url, _user, _cred) => credentials
                };

                var pushOptions = new PushOptions
                {
                    CredentialsProvider = (url, usernameFromUrl, types) => credentials
                };

                var identity = new Identity(credentials.Username, "vkhovanskiy@gmail.com");
                var signature = new Signature(identity, DateTimeOffset.Now);

                Console.WriteLine($"Start");
                Directory.CreateDirectory(destPath);
                var path = Repository.Init(destPath, false);
                using (repo = new Repository(path, new RepositoryOptions {Identity = identity}))
                {
                    // создаем локальную ветку
                    var remote = repo.Network.Remotes.FirstOrDefault(p => p.Name == "origin");
                    if (remote == null)
                        remote = repo.Network.Remotes.Add("origin", BuildsInfo.DEFAULT_PROJECT_GIT);

                    // вытягиваем весь репозиторий с сервера 
                    Commands.Fetch(repo, "origin", new string[0], new FetchOptions
                    {
                        CredentialsProvider = fetchOptions.CredentialsProvider
                    }, null);
                    // переносим все в локальную папку Fetch + Merge - это тоже самое что и pull
                    repo.Merge(repo.Branches["origin/master"], signature);


                    var fileVersionsPath = Path.Combine(destPath, BuildsInfo.FILE_NAME);
                    BuildsInfo buildVersions = null;
                    if (File.Exists(fileVersionsPath))
                    {
                        var currentFileVersions = File.ReadAllText(fileVersionsPath);
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

                Console.WriteLine($"Build successful assembled! Project=[{projectStr}]");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                repo?.Dispose();
                Console.Write($"End");
            }
        }

        public static void ClearAllRepos(string directoryPath, bool deleteGit = true)
        {
            var di = new DirectoryInfo(directoryPath);

            foreach (var file in di.GetFiles())
            {
                file.Delete();
            }

            foreach (var dir in di.GetDirectories())
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
