using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components.ROBP
{
    public sealed class ROBPOperation : DriveTemplate, IOperation
    {
        [DGVColumn(ColumnPosition.Before, "HostType")]
        public string HostTypeName { get; }

        [DGVColumn(ColumnPosition.After, "Operation")]
        public override string Name { get; set; }

        /// <summary>
        /// Сценарий для этой операции существует
        /// </summary>
        [DGVColumn(ColumnPosition.Last, "IsScenarioExist", false)]
        public bool IsScenarioExist { get; set; } = true;

        public ROBPOperation(string path, IObjectTemplate parentElement) : base(path)
        {
            if (GetNameWithId(Name, out var newName, out var newId))
            {
                Name = newName;
                ID = newId;
            }
            HostTypeName = parentElement.Name;
        }
    }
}
