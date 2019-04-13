using System;
using System.Collections.Generic;

namespace Script.Control.Handlers.Timesheet.Project
{
    [Serializable]
    public class TFSProject : List<TFS>
    {
        public string Name { get; }
        public string PM { get; }
        public string PM_Mail { get; }

        public TFSProject(string prjName, string pm, string pm_mail)
        {
            Name = prjName;
            PM = pm;
            PM_Mail = pm_mail;
        }
    }
}
