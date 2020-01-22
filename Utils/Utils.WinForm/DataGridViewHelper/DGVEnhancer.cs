using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils.WinForm.DataGridViewHelper
{
    public static class DGVEnhancer
    {
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

        const BindingFlags PropertyFlags = BindingFlags.Instance | BindingFlags.Public;
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
            if (SystemInformation.TerminalServerSession)
                return;

            var dgvType = dgv.GetType();
            var pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi?.SetValue(dgv, value, null);
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
            public int ID { get; }

            public DGVColumn(int id, string propName, DGVColumnAttribute gridColumnattr, Type propType)
            {
                ID = id;
                Attribute = gridColumnattr;
                PropertyName = propName;
                PropertyType = propType;
            }

            public DGVColumnAttribute Attribute { get; }
            public string PropertyName { get; }
            public Type PropertyType { get; }

            public override string ToString()
            {
                return $"{PropertyName}=[{Attribute.ColumnName}]";
            }
        }

        public static async Task AssignCollectionAsync<T>(this DataGridView grid, IEnumerable<T> data, Padding? cellPadding = null, bool stretchColumnsToAllCells = false)
        {
            await Task.Factory.StartNew(() =>
            {
                grid.AssignCollection(data, cellPadding, stretchColumnsToAllCells);
            });
        }

        public static void AssignCollection<T>(this DataGridView grid, IEnumerable<T> data, Padding? cellPadding = null, bool stretchColumnsToAllCells = false)
        {
            if (data == null)
                throw new ArgumentException(nameof(data));

            var table = new DataTable();
            var typeParameterType = typeof(T);
            var props = typeParameterType.GetProperties(PropertyFlags);


            var i1 = 0;
            var columnList = new List<DGVColumn>();
            foreach (var prop in props)
            {
                var attrs = prop.GetCustomAttributes(true);
                foreach (var attr in attrs)
                {
                    if (attr is DGVColumnAttribute columnAttr)
                    {
                        columnList.Add(new DGVColumn(i1++, prop.Name, columnAttr, prop.PropertyType));
                    }
                }
            }

            columnList = columnList.OrderBy(p => p.Attribute.Visible).ThenBy(p => p.Attribute.Position).ThenBy(p => p.ID).ToList();

            //var sortedColumns = new LinkedList<DGVColumn>();
            //LinkedListNode<DGVColumn> last = null;
            //foreach (var column in columnList)
            //{
            //    var current = new LinkedListNode<DGVColumn>(column);
            //    SwitchPosition(sortedColumns, column.Attribute.Position, current, ref last);
            //}


            var i2 = 0;
            var positionOfColumn = new Dictionary<string, KeyValuePair<int, DGVColumn>>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var column in columnList)
            {
                table.Columns.Add(column.Attribute.ColumnName, column.PropertyType);
                positionOfColumn.Add(column.PropertyName, new KeyValuePair<int, DGVColumn>(i2++, column));
            }

            var columnsCount = positionOfColumn.Count;
            foreach (var instance in data)
            {
                var tp = instance.GetType();
                var props2 = tp.GetProperties(PropertyFlags);
                var objs = new object[columnsCount];

                foreach (var prop in props2)
                {
                    if (positionOfColumn.TryGetValue(prop.Name, out var pos))
                    {
                        var result = prop.GetValue(instance, null);
                        objs[pos.Key] = result;
                    }
                }

                table.Rows.Add(objs);
            }


            if (grid.InvokeRequired)
            {
                grid.Invoke(new MethodInvoker(delegate
                {
                    grid.AssignToDataGridView(table, columnList, cellPadding, stretchColumnsToAllCells);
                }));
            }
            else
            {
                grid.AssignToDataGridView(table, columnList, cellPadding, stretchColumnsToAllCells);
            }
        }

        static void AssignToDataGridView(this DataGridView grid, DataTable table, IEnumerable<DGVColumn> columnList, Padding? cellPadding, bool stretchColumnsToAllCells)
        {
            var prevResize = grid.RowHeadersWidthSizeMode;
            var prevVisible = grid.RowHeadersVisible;
            try
            {
                grid.BeginInit();

                grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
                grid.RowHeadersVisible = false;

                grid.DataSource = null;
                grid.Columns.Clear();
                grid.Rows.Clear();
                grid.ClearSelection();

                
                grid.DataSource = table;
                foreach (var column in columnList)
                {
                    if (column.Attribute.Visible)
                        continue;

                    var hiddenColumn = grid.Columns[column.Attribute.ColumnName];
                    if (hiddenColumn != null)
                        hiddenColumn.Visible = false;
                }
                

                if (cellPadding != null)
                {
                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        row.DefaultCellStyle.Padding = cellPadding.Value;
                    }
                }

                if (!stretchColumnsToAllCells)
                    return;

                StretchColumnsToAllCells(grid);
                grid.DataBindingComplete += Grid_DataBindingComplete;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                grid.RowHeadersWidthSizeMode = prevResize;
                grid.RowHeadersVisible = prevVisible;

                grid.EndInit();
            }
        }


        private static void Grid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!(sender is DataGridView grid))
                return;

            StretchColumnsToAllCells(grid);
            grid.DataBindingComplete -= Grid_DataBindingComplete;
        }


        public static void StretchColumnsToAllCells(this DataGridView grid)
        {
            for (var index = 0; index < grid.Columns.Count; index++)
            {
                var column = grid.Columns[index];
                if (index < grid.Columns.Count - 1)
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                else
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            if (grid.Columns.Count >= 2)
            {
                for (var t = 0; t <= grid.Columns.Count - 2; t++)
                {
                    var column = grid.Columns[t].Width;
                    grid.Columns[t].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    grid.Columns[t].Width = column;
                }
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
