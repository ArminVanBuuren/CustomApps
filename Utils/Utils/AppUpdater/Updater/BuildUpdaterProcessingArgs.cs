using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    internal class BuildUpdaterProcessingArgs
    {
        public Exception Error { get; internal set; }
    }
}
