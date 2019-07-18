using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Utils.WinForm.DataGridViewHelper
{
    public static class DGVEnhancer
    {
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

        private const int WM_SETREDRAW = 11;

        public static void SetRedraw(this DataGridView dgv, bool value)
        {
            if (value)
            {
                SendMessage(dgv.Handle, WM_SETREDRAW, true, 0);
                dgv.Refresh();
            }
            else
            {
                SendMessage(dgv.Handle, WM_SETREDRAW, false, 0);
            }
        }

        public static void SetDoubleBuffering(this DataGridView dgv, bool value)
        {
            // Double buffering can make DGV slow in remote desktop
            if (!System.Windows.Forms.SystemInformation.TerminalServerSession)
            {
                Type dgvType = dgv.GetType();
                PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                pi.SetValue(dgv, value, null);
            }
        }

        public static void BeginInit(this DataGridView dgv)
        {
            ((ISupportInitialize) dgv).BeginInit();
        }

        public static void EndInit(this DataGridView dgv)
        {
            ((ISupportInitialize) dgv).EndInit();
        }

        class DGVColumn
        {
            public DGVColumn(string propName, DGVColumnAttribute gridColumnattr, Type propType)
            {
                Attribute = gridColumnattr;
                PropertyName = propName;
                PropertyType = propType;
            }

            public DGVColumnAttribute Attribute { get; }
            public string PropertyName { get; }
            public string ColumnName => Attribute.Name;
            public Type PropertyType { get; }

            public override string ToString()
            {
                return $"{PropertyName}=[{ColumnName}]";
            }
        }

        public static void AssignListToDataGrid<T>(this DataGridView grid, IList<T> data, Padding? cellPadding = null)
        {
            if (data == null)
                throw new ArgumentException(nameof(data));

            grid.DataSource = null;
            grid.Columns.Clear();
            grid.Rows.Clear();

            var table = new DataTable();
            var typeParameterType = typeof(T);
            var propertyFlags = BindingFlags.Instance | BindingFlags.Public;
            var props = typeParameterType.GetProperties(propertyFlags);


            var sortedColumns = new LinkedList<DGVColumn>();
            LinkedListNode<DGVColumn> last = null;
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    if (attr is DGVColumnAttribute columnAttr)
                    {
                        var current = new LinkedListNode<DGVColumn>(new DGVColumn(prop.Name, columnAttr, prop.PropertyType));
                        SwitchPosition(sortedColumns, columnAttr.Position, current, ref last);
                    }
                }
            }

            var i = 0;
            var positionOfColumn = new Dictionary<string, KeyValuePair<int, DGVColumn>>(StringComparer.CurrentCultureIgnoreCase);
            foreach (DGVColumn column in sortedColumns)
            {
                table.Columns.Add(column.ColumnName, column.PropertyType);
                positionOfColumn.Add(column.PropertyName, new KeyValuePair<int, DGVColumn>(i++, column));
            }

            var j = 0;
            var columnsCount = positionOfColumn.Count;
            foreach (T instance in data)
            {
                var tp = instance.GetType();
                var props2 = tp.GetProperties(propertyFlags);
                var objs = new object[columnsCount];

                foreach (PropertyInfo prop in props2)
                {
                    if (positionOfColumn.TryGetValue(prop.Name, out var pos))
                    {
                        object result = prop.GetValue(instance, null);
                        objs[pos.Key] = result;
                    }
                }


                table.Rows.Add(objs);
                j++;
            }

            //dtCommunication.Select("Type='Business'", "LastModifiedDate DESC");
            //table.DefaultView.Sort = "Preferance ASC";

            grid.BeginInit();
            grid.DataSource = table;
            foreach (DGVColumn column in sortedColumns)
            {
                if (column.Attribute.Visible)
                    continue;

                var hiddenColumn = grid.Columns[column.ColumnName];
                if (hiddenColumn != null)
                    hiddenColumn.Visible = false;
            }
            grid.EndInit();

            if (cellPadding != null)
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    row.DefaultCellStyle.Padding = cellPadding.Value;
                }
            }

            for (var index = 0; index < grid.Columns.Count; index++)
            {
                DataGridViewColumn column = grid.Columns[index];
                if (index < grid.Columns.Count - 1)
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                else
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        static void SwitchPosition<T>(LinkedList<T> sortedColumns, ColumnPosition position, LinkedListNode<T> current, ref LinkedListNode<T> last)
        {
            switch (position)
            {
                case ColumnPosition.First:
                    sortedColumns.AddFirst(current);
                    if (last == null)
                        last = current;
                    break;
                case ColumnPosition.After:
                    if (last == null)
                        sortedColumns.AddFirst(current);
                    else
                        sortedColumns.AddAfter(last, current);
                    last = current;
                    break;
                case ColumnPosition.Before:
                    if (last == null)
                        sortedColumns.AddFirst(current);
                    else
                        sortedColumns.AddBefore(last, current);
                    last = current;
                    break;
                case ColumnPosition.Last:
                    sortedColumns.AddLast(current);
                    break;
            }
        }
    }
}
