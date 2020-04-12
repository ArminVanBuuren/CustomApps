﻿using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components.ROBP
{
    public sealed class ROBPOperation : DriveTemplate, IOperation
    {
        [DGVColumn(ColumnPosition.After, "HostType")]
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

        public ROBPOperation(string path, IObjectTemplate parentElement, bool isFailed) : base(path)
        {
            if (GetNameWithId(Name, out var newName, out var newId))
            {
                Name = newName;
                ID = newId;
            }

            HostTypeName = parentElement.Name;
            IsFailed = isFailed;
        }

        public override bool Equals(object obj)
        {
            if (obj is ROBPOperation operation)
                return Name.Like(operation.Name) && HostTypeName.Equals(operation.HostTypeName);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}