using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private Uri _remoteRepos;
        private string _gitDirectoryPath;
        private string _transactionId;
        string gitCommand = "git";
        string gitPwdArgument = "pwd";
        string gitCdArgument = string.Empty;
        string gitCloneArgument = string.Empty;
        string gitInitArgument = $"init";
        string gitRemoteAddArgument = string.Empty;
        string gitPullArgument = @"pull origin master";
        string gitAddArgument = @"add .";
        string gitCommitArgument = string.Empty;
        string gitPushArgument = @"push -u origin master";
        public GitControl(Uri remoteRepos, string gitDirectoryPath, string transactionId)
        {
            _remoteRepos = remoteRepos;
            _gitDirectoryPath = gitDirectoryPath;
            _transactionId = transactionId;

            gitCdArgument = $"cd -P -- \"{_gitDirectoryPath}\"";
            gitCloneArgument = $"clone {_remoteRepos.AbsoluteUri}";
            gitRemoteAddArgument = $"remote add origin {_remoteRepos.AbsoluteUri}";
            gitCommitArgument = $"commit -m \"Add build {_transactionId}\"";

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
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "git",
                Arguments = command,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            Process process = Process.Start(startInfo);
            //string result = process.StandardOutput.ReadToEnd();
        }

        public void ClearRepos(bool removeConfigurationGit = false)
        {
            DirectoryInfo di = new DirectoryInfo(_gitDirectoryPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
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
