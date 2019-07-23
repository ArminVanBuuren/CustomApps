using System;
using System.Diagnostics;
using System.IO;

namespace Utils.Handles
{
    public enum GitCommands
    {
        InitAndPull = 0,
        AddAndCommit = 1,
        Push = 2
    }

    public class GitControl
    {
        private readonly string _gitDirectoryPath;

        readonly string gitCommand = "git";
        //string gitPwdArgument = "pwd";
        string gitCdArgument;
        string gitCloneArgument;
        readonly string gitInitArgument = $"init";
        readonly string gitRemoteAddArgument;
        string gitPullArgument = @"pull origin master";
        string gitAddArgument = @"add .";
        readonly string gitCommitArgument;
        string gitPushArgument = @"push -u origin master";
        public GitControl(Uri remoteRepos, string gitDirectoryPath, string transactionId)
        {
            var remoteRepos1 = remoteRepos;
            _gitDirectoryPath = gitDirectoryPath;

            gitCdArgument = $"cd -P -- \"{_gitDirectoryPath}\"";
            gitCloneArgument = $"clone {remoteRepos1.AbsoluteUri}";
            gitRemoteAddArgument = $"remote add origin {remoteRepos1.AbsoluteUri}";
            gitCommitArgument = $"commit -m \"Add build {transactionId}\"";

            if (!Directory.Exists(gitDirectoryPath))
                Directory.CreateDirectory(gitDirectoryPath);

            ClearRepos(true);
        }

        public void GitSend(GitCommands commands)
        {
            switch (commands)
            {
                case GitCommands.InitAndPull:
                    //var processResults2 = ProcessEx.RunAsync("git.exe", @"cd C/!Builds/TFSAssist").Result;
                    //Process.Start(gitCommand, gitCdArgument);

                    Process.Start(gitCommand, gitInitArgument);
                    Process.Start(gitCommand, gitRemoteAddArgument);
                    Process.Start(gitCommand, gitPullArgument);
                    break;
                case GitCommands.AddAndCommit:
                    Process.Start(gitCommand, gitAddArgument);
                    Process.Start(gitCommand, gitCommitArgument);
                    break;
                case GitCommands.Push:
                    Process.Start(gitCommand, gitPushArgument);
                    break;
            }
        }

        static void Execute(string command)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "git",
                Arguments = command,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = Process.Start(startInfo);
            //string result = process.StandardOutput.ReadToEnd();
        }

        public void ClearRepos(bool removeConfigurationGit = false)
        {
            var di = new DirectoryInfo(_gitDirectoryPath);

            foreach (var file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (var dir in di.GetDirectories())
            {
                if (dir.Name.Like(".git"))
                {
                    if(!removeConfigurationGit)
                        continue;
                }
                dir.Delete(true);
            }
        }
    }
}
