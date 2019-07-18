using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    public interface IOperation : IObjectTemplate
    {
        string HostTypeName { get; }

        bool IsScenarioExist { get; set; }
    }
}
