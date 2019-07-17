using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    public class Operation : ObjectTemplate
    {
        [DGVColumn(ColumnPosition.After, "Operation")]
        public sealed override string Name { get; protected set; }

        [DGVColumn(ColumnPosition.Before, "HostType")]
        public string HostTypeName { get; protected set; }

        /// <summary>
        /// Сценарий для этой операции существует
        /// </summary>
        [DGVColumn(ColumnPosition.Last, "IsScenarioExist", false)]
        public bool IsScenarioExist { get; internal set; } = true;

        public Operation() { }

        public Operation(string path) : base(path)
        {

        }
    }
}
