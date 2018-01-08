using System;
using System.Collections.Generic;

namespace Script.Control.Handlers.Timesheet.Project
{
    [Serializable]
    public class TFSProject : List<TFS>
    {
        public string Name { get; }

        public TFSProject(string prjName)
        {
            Name = prjName;
        }
    }
}
