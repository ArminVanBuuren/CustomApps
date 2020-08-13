using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogsReader.Properties;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader.Reader.Forms
{
	public partial class TraceNameFilterForm : Form
	{
		public IDictionary<string, TraceNameFilter> TraceNames { get; private set; }

		TraceNameFilterForm(IDictionary<string, TraceNameFilter> traceNames)
		{
			InitializeComponent();
			TraceNames = traceNames;

			Icon = Icon.FromHandle(Resources.filter.GetHicon());
			MinimizeBox = false;
			MaximizeBox = false;

			CountMatchesColumn.HeaderText = Resources.TxtReader_DgvMatches;
			CountErrorsColumn.HeaderText = Resources.TxtReader_DgvErrors;

			CenterToScreen();

			DgvTraceNames.CellFormatting += (sender, args) =>
			{
				if (args.RowIndex < 0 || args.ColumnIndex < 0)
					return;
				ColorizationRow(((DataGridView)sender).Rows[args.RowIndex]);
			};
			DgvTraceNames.DataBindingComplete += (sender, args) =>
			{
				foreach (var row in DgvTraceNames.Rows.OfType<DataGridViewRow>())
				{
					var checkedCell = row.Cells[SelectColumn.Name] as DgvCheckBoxCell;
					var traceName = row.Cells[TraceNameColumn.Name]?.Value?.ToString();
					if (checkedCell == null || traceName.IsNullOrWhiteSpace() || !traceNames.TryGetValue(traceName, out var traceNameFilter))
						continue;
					checkedCell.Checked = traceNameFilter.Checked;
				}
				DgvTraceNames.Refresh();
			};
			DgvTraceNames.Resize += (sender, args) => DgvTraceNames.Invalidate(); // необходим для столбца checkox
			DgvTraceNames.Scroll += (sender, args) => DgvTraceNames.Invalidate(); // необходим для столбца checkox
			DgvTraceNames.Sorted += (sender, args) =>
			{
				if(DgvTraceNames.RowCount > 0)
					DgvTraceNames.SelectRow(DgvTraceNames.Rows[0]);
			};

			KeyPreview = true;
			KeyDown += (sender, args) =>
			{
				switch (args.KeyCode)
				{
					case Keys.Enter when buttonOK.Enabled:
						buttonOK_Click(this, EventArgs.Empty);
						break;
					case Keys.Escape:
						Close();
						break;
				}
			};
		}

		public static async Task<TraceNameFilterForm> GetAsync(IDictionary<string, TraceNameFilter> traceNames)
		{
			var form = new TraceNameFilterForm(traceNames);
			await form.DgvTraceNames.AssignCollectionAsync(traceNames.Values, null, true);
			form.DgvTraceNames.CheckStatusHeader(form.SelectColumn);

			form.RefreshAllRows();

			form.MaximumSize = new Size(form.MaximumSize.Width, Math.Max(130, (630 / 30) * Math.Min(30, traceNames.Count())));
			form.Size = new Size(form.Size.Width, form.MaximumSize.Height);

			return form;
		}

		void RefreshAllRows()
		{
			if (DgvTraceNames == null || DgvTraceNames.RowCount == 0)
				return;

			foreach (var row in DgvTraceNames.Rows.OfType<DataGridViewRow>())
				ColorizationRow(row);
		}

		protected virtual void ColorizationRow(DataGridViewRow row)
		{
			if (row == null)
				return;

			var color = row.Cells[SelectColumn.Name] is DgvCheckBoxCell cellCheckBox && cellCheckBox.Checked
				? LogsReaderMainForm.READER_COLOR_BACK_SUCCESS
				: row.Index.IsParity()
					? Color.White
					: Color.FromArgb(245, 245, 245);

			if (row.DefaultCellStyle.BackColor != color)
				row.DefaultCellStyle.BackColor = color;
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			foreach (var row in DgvTraceNames.Rows.OfType<DataGridViewRow>())
			{
				var traceNameCell = row.Cells[TraceNameColumn.Name]?.Value.ToString();
				if(traceNameCell == null)
					continue;

				if (TraceNames.TryGetValue(traceNameCell, out var traceNameFilter))
					traceNameFilter.Checked = row.Cells[SelectColumn.Name] is DgvCheckBoxCell cellCheckBox && cellCheckBox.Checked;
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}