using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.AppUpdater.Updater
{
    public interface IBuildUpdater
    {
        string FileSource { get; }
        string FileDestination { get; }
        bool IsExecutable { get; }
        FileBuildInfo LocalFile { get; }
        FileBuildInfo ServerFile { get; }
    }
}
