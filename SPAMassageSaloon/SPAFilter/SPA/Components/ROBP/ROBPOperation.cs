using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components.ROBP
{
    public sealed class ROBPOperation : DriveTemplate, IOperation
    {
        public override string UniqueName => FilePath;

        [DGVColumn(ColumnPosition.First, "HostType")]
        public string HostTypeName { get; }

        [DGVColumn(ColumnPosition.After, "Operation")]
        public override string Name { get; set; }

        /// <summary>
        /// Сценарий для этой операции существует
        /// </summary>
        public bool IsScenarioExist { get; set; } = true;

        /// <summary>
        /// Проверка на корректность операции
        /// </summary>
        public bool IsFailed { get; set; } = false;

        public ROBPOperation(string path, IObjectTemplate parentHostType, bool isFailed) : base(path)
        {
            if (GetNameWithId(Name, out var newName, out var newId))
            {
                Name = newName;
                ID = newId;
            }

            HostTypeName = parentHostType.Name;
            IsFailed = isFailed;
        }
    }
}