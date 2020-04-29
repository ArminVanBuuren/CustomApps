using LibGit2Sharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Microsoft.Win32;
using Utils;
using Utils.AppUpdater;
using Utils.AppUpdater.Pack;
using Utils.AppUpdater.Updater;
using Utils.Handles;

namespace Tester.Updater
{
    class Program
    {
        private static ApplicationUpdater up;
        private static BuildPackUpdater _updater;
        static void Main(string[] args)
        {
            //try
            //{
            //    var dd = BuildsInfo.Deserialize(IO.SafeReadFile(@"C:\!Builds\Git\versions.xml"));
            //    var dd2 = new BuildPackUpdaterSimple(typeof(Class1).GetAssemblyInfo().CurrentAssembly, dd.Packs.First(), null);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}


            //var test0 = new FileBuildInfo(@"C:\!Builds\Builds\SPAMassageSaloon.exe", @"C:\!Builds\Builds", false);
            //var test1 = new FileBuildInfo(@"C:\!Builds\Builds\SPAMassageSaloon1.exe", @"C:\!Builds\Builds", false);
            //var test2 = new FileBuildInfo(@"C:\!Builds\Builds\Utils1.dll", @"C:\!Builds\Builds", false);
            //var test3 = new FileBuildInfo(@"C:\!Builds\Builds\SPAFilter.bin", @"C:\!Builds\Builds", false);
            //var test4 = new FileBuildInfo(@"C:\!Builds\Builds\LogsReader.xml", @"C:\!Builds\Builds", false);

            //var ass = Assembly.LoadFile(@"C:\!Builds\Builds\SPAMassageSaloon1.exe");
            //var pr = ass.GetName().Name;
            //try
            //{
            //    var ass2 = Assembly.LoadFile(@"C:\!Builds\Builds\Utils1.dll");
            //    var pr2 = ass2.GetName().Name;
            //}
            //catch (Exception e)
            //{
            //    var type = e.GetType();
            //}
            //try
            //{
            //    var ass3 = Assembly.LoadFile(@"C:\!Builds\Builds\SPAFilter.bin");
            //}
            //catch (Exception e)
            //{
            //    var type = e.GetType();
            //}
            //try
            //{
            //    var ass4 = Assembly.LoadFile(@"C:\!Builds\Builds\LogsReader.xml");
            //}
            //catch (Exception e)
            //{
            //    var type = e.GetType();
            //}

            Start:
            try
            {
                Console.WriteLine("Press \"1\" for UPDATE or \"2\" for UPLOAD.");
                var res = Console.ReadLine();
                switch (res)
                {
                    case "1":
                        Update();

                        while (true)
                        {
                            var key = Console.ReadKey().Key;
                            if (key == ConsoleKey.Enter)
                            {
                                if (_updater == null)
                                {
                                    Console.WriteLine("No updates found.");
                                }
                                else
                                {
                                    up = new ApplicationUpdater(Assembly.GetExecutingAssembly(), 1);
                                    //up.Start();
                                    up?.DoUpdate(_updater);
                                }
                            }
                        }

                        break;
                    case "2":
                        Upload();
                        Console.ReadKey();
                        break;
                    default:
                        goto Start;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }

        
        public static void Update()
        {
            up = new ApplicationUpdater(Assembly.GetExecutingAssembly(), 10);
            up.OnSuccessfulUpdated += Up_OnSuccessfulUpdated;
            //up.OnFetch += Up_OnFetch;
            //up.OnUpdate += Up_OnUpdate;
            //up.OnProcessingError += Up_OnProcessingError;
            up.Start();
            //up.CheckUpdates();
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {nameof(ApplicationUpdater)} created!");
        }

        private static void Up_OnSuccessfulUpdated(object sender, ApplicationUpdaterArgs args)
        {
            Console.WriteLine("Succeessful Updated!!!");
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

        private static void Up_OnUpdate(object sender, ApplicationUpdaterArgs args)
        {
            try
            {
                _updater = args.Control;
                Console.WriteLine($"[{DateTime.Now:hh:mm:ss}] [{Thread.CurrentThread.ManagedThreadId}] Update. Status=[{up.Status:G}]");


                //using (var streamSer = args.Control.SerializeToStream())
                //{
                    
                //    var streamDes1 = streamSer.DeserializeFromStream();

                //    try
                //    {
                //        using (var reg = new RegeditControl(Assembly.GetExecutingAssembly().GetAssemblyInfo().ApplicationName))
                //            reg["Stream", RegistryValueKind.Binary] = streamSer.ToArray();

                //        using (var reg = new RegeditControl(Assembly.GetExecutingAssembly().GetAssemblyInfo().ApplicationName))
                //        {
                //            var byteArray = (byte[])reg["Stream"];
                //            var test1 = new MemoryStream(byteArray);
                //            var test2 = new BinaryFormatter().Deserialize(test1) as BuildPackUpdater;
                //        }
                //    }
                //    catch (Exception)
                //    {
                //        // ignored
                //    }
                //}

                Console.WriteLine("Waiting command...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
           

            //Console.WriteLine($"Up_OnUpdate. ThreadId=[{Thread.CurrentThread.ManagedThreadId}] Status=[{up.Status}] Updater{args.Control}");
            //Console.WriteLine("Sleep 5 second and DoUpdate");
            //Thread.Sleep(5000);
            //up.DoUpdate(args.Control);

            //args.Control.Dispose();
            //up.Refresh();
        }

        private static void Up_OnProcessingError(object sender, ApplicationUpdaterArgs args)
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
                // https://www.shellhacks.com/ru/git-remove-all-commits-clear-git-history-local-remote/
                // git fetch origin master
                // при ошибке fatal: unable to access 'https://github.com/ArminVanBuuren/Builds/': Could not resolve host: github.com выполнить:
                // git config --global --unset http.proxy 
                // git config --global--unset https.proxy

                //Console.WriteLine(@"Enter builds directory path. Like - C:\!MyRepos\CustomApp\Utils\Tester.Updater\bin\Uploader\test1\OnServer");
                const string sourcePath = @"C:\!Builds\Builds";
                const string destPath = @"C:\!Builds\Git";
                Console.WriteLine($"Run only on x86 Platform. Do not forget library git2-572e4d8.dll with correct bit-depth (x86 or x64)\r\nEnter project name. Like - TFSAssist; Tester.Updater");
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
                        remote = repo.Network.Remotes.Add("origin", @"https://github.com/ArminVanBuuren/Builds");

                    // вытягиваем весь репозиторий с сервера 
                    Commands.Fetch(repo, "origin", new string[0], new FetchOptions
                    {
                        CredentialsProvider = fetchOptions.CredentialsProvider
                    }, null);
                    // переносим все в локальную папку Fetch + Merge - это тоже самое что и pull
                    repo.Merge(repo.Branches["origin/master"], signature);


                    var fileVersionsPath = Path.Combine(destPath, "versions.xml");
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

                    buildVersions.Add(projectStr, sourcePath, destPath, "versions.xml");
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
