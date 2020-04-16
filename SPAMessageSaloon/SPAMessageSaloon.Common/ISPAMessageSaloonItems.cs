using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace SPAMessageSaloon.Common
{
    public interface ISaloonForm : IUserForm
    {
        int ActiveProcessesCount { get; }

        int ActiveTotalProgress { get; }
    }

    public interface IUserForm
    {
        void ApplySettings();

        void SaveData();
    }
}
