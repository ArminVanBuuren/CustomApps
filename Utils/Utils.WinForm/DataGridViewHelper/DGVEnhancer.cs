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
            public int ClassPosition { get; }

            public DGVColumn(int classPosition, string propName, DGVColumnAttribute gridColumnattr, Type propType)
            {
                ClassPosition = classPosition;
                Attribute = gridColumnattr;
                PropertyName = propName;
                PropertyType = propType ?? typeof(object);
            }

            public DGVColumnAttribute Attribute { get; }
            public string PropertyName { get; }
            public Type PropertyType { get; }

            public override string ToString()
            {
                return $"{PropertyName}=[{Attribute.ColumnName}]";
            }
        }

        public static async Task<DataTable> ToTableAsync<T>(this IEnumerable<T> data)
        {
            return await Task.Factory.StartNew(data.ToTable);
        }

        public static DataTable ToTable<T>(this IEnumerable<T> data)
        {
            return GetTableAndAssignCollection(null, data, null, false);
        }

        public static async Task AssignCollectionAsync<T>(this DataGridView grid, IEnumerable<T> data, Padding? cellPadding = null, bool stretchColumnsToAllCells = false)
        {
            await Task.Factory.StartNew(() => { GetTableAndAssignCollection(grid, data, cellPadding, stretchColumnsToAllCells); });
        }

        public static void AssignCollection<T>(this DataGridView grid, IEnumerable<T> data, Padding? cellPadding = null, bool stretchColumnsToAllCells = false)
        {
            GetTableAndAssignCollection(grid, data, cellPadding, stretchColumnsToAllCells);
        }

        static DataTable GetTableAndAssignCollection<T>(DataGridView grid, IEnumerable<T> data, Padding? cellPadding, bool stretchColumnsToAllCells)
        {
            if (data == null)
                throw new ArgumentException(nameof(data));

            var table = new DataTable();
            var columnList = new Dictionary<string, DGVColumn>();
            var positionOfColumns = new Dictionary<string, KeyValuePair<int, DGVColumn>>();

            var classPosition = 0;
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var attrs = property.GetCustomAttributes(true);
                foreach (var attr in attrs)
                {
                    if (attr is DGVColumnAttribute columnAttr)
                    {
                        columnList.Add(property.Name, new DGVColumn(classPosition++, property.Name, columnAttr, property.PropertyType));
                    }
                }
            }

            if (grid == null || grid.ColumnCount == 0)
            {
                columnList = columnList.OrderBy(p => p.Value.Attribute.Position).ThenBy(p => p.Value.ClassPosition).ToDictionary(p => p.Key, p => p.Value);
                var columnPosition = 0;
                foreach (var newColumn in columnList.Values)
                {
                    var dataColumn = new DataColumn(newColumn.PropertyName, newColumn.PropertyType)
                    {
                        Caption = newColumn.Attribute.ColumnName.IsNullOrEmptyTrim() ? newColumn.PropertyName : newColumn.Attribute.ColumnName
                    };
                    table.Columns.Add(dataColumn);
                    positionOfColumns.Add(newColumn.PropertyName, new KeyValuePair<int, DGVColumn>(columnPosition++, newColumn));
                }
            }
            else
            {
                int position = 0;
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    var propertyName = column.DataPropertyName.IsNullOrEmpty() ? column.Name : column.DataPropertyName;

                    if (!columnList.TryGetValue(propertyName, out var newColumn))
                        throw new Exception($"DataPropertyName or Name \"{propertyName}\" of DatagridView columns is incorrect. Property \"{propertyName}\" in class \"{typeof(T)}\" doesn't have attribute \"{nameof(DGVColumn)}\"");

                    var dataColumn = new DataColumn(newColumn.PropertyName, newColumn.PropertyType)
                    {
                        Caption = newColumn.Attribute.ColumnName.IsNullOrEmptyTrim() ? newColumn.PropertyName : newColumn.Attribute.ColumnName
                    };
                    table.Columns.Add(dataColumn);

                    positionOfColumns.Add(newColumn.PropertyName, new KeyValuePair<int, DGVColumn>(position, newColumn));
                    position++;
                }
            }


            foreach (var instance in data)
            {
                DataRow dr = table.NewRow();

                foreach (var property in properties)
                {
                    if (positionOfColumns.TryGetValue(property.Name, out var result))
                    {
                        var resultValue = property.GetValue(instance, null);
                        dr[result.Key] = resultValue;
                    }
                }

                table.Rows.Add(dr);
            }

            grid?.SafeInvoke(() => { grid.AssignToDataGridView(table, columnList.Values, cellPadding, stretchColumnsToAllCells); });

            return table;
        }

        static void AssignToDataGridView(this DataGridView grid, DataTable table, IEnumerable<DGVColumn> columnList, Padding? cellPadding, bool stretchColumnsToAllCells)
        {
            var prevResize = grid.RowHeadersWidthSizeMode;
            var prevVisible = grid.RowHeadersVisible;
            try
            {
                if (stretchColumnsToAllCells)
                {
                    grid.BeginInit();
                    grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
                    grid.RowHeadersVisible = false;

                    grid.DataSource = null;
                    grid.Rows.Clear();
                    grid.ClearSelection();
                }

                grid.DataSource = table;

                foreach (var column in columnList)
                {
                    var dgvColumn = grid.Columns[column.PropertyName];
                    if (dgvColumn == null)
                        continue;

                    if(!column.Attribute.Visible)
                        dgvColumn.Visible = false;

                    if (!dgvColumn.HeaderText.Equals(column.Attribute.ColumnName))
                        dgvColumn.HeaderText = column.Attribute.ColumnName;
                }

                if (cellPadding != null)
                {
                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        row.DefaultCellStyle.Padding = cellPadding.Value;
                    }
                }

                if (stretchColumnsToAllCells)
                {
                    StretchColumnsToAllCells(grid);
                    grid.DataBindingComplete += Grid_DataBindingComplete;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (stretchColumnsToAllCells)
                {
                    grid.RowHeadersWidthSizeMode = prevResize;
                    grid.RowHeadersVisible = prevVisible;
                    grid.EndInit();
                }
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
            var columnNumber = 0;
            DataGridViewColumn lastColumn = null;
            foreach (DataGridViewColumn column in grid.Columns.OfType<DataGridViewColumn>().OrderBy(p => p.Visible))
            {
                columnNumber++;
                if(!column.Visible)
                    continue;

                if (grid.Columns.Count > columnNumber)
                {
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                else
                {
                    lastColumn = column;
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column == lastColumn || !column.Visible)
                    continue;
                var prevColumnWidth = column.Width;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                column.Width = prevColumnWidth;
                column.Frozen = false;
            }
        }

        static void SwitchPosition<T>(LinkedList<T> sortedColumns, ColumnPosition position, LinkedListNode<T> current, ref LinkedListNode<T> last)
        {
            // старая реализация установки позиций колонок
            //var sortedColumns = new LinkedList<DGVColumn>();
            //LinkedListNode<DGVColumn> last = null;
            //foreach (var column in columnList)
            //{
            //    var current = new LinkedListNode<DGVColumn>(column);
            //    SwitchPosition(sortedColumns, column.Attribute.Position, current, ref last);
            //}

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
