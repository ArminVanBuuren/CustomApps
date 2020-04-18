using System;
using System.CodeDom;

namespace Utils.WinForm.DataGridViewHelper
{
    public enum ColumnPosition
    {
        First = 0,
        After = 1,
        Before = 2,
        Last = 3
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class DGVColumnAttribute : Attribute
    {
        public DGVColumnAttribute(ColumnPosition pos, string columnName, bool visible = true)
        {
            Position = pos;
            ColumnName = columnName;
            Visible = visible;
        }

        public DGVColumnAttribute(ColumnPosition pos, string columnName, string format, bool visible = true)
        {
            Position = pos;
            ColumnName = columnName;
            Format = format;
            Visible = visible;
        }

        public ColumnPosition Position { get; }
        public string ColumnName { get; }
        public string Format { get; } = string.Empty;
        public bool Visible { get; }
    }
}
