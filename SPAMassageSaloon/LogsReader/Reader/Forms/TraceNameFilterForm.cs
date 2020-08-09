using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogsReader.Properties;
using Utils;
using Utils.WinForm;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader.Reader.Forms
{
	public partial class TraceNameFilterForm : Form
	{
		TraceNameFilterForm()
		{
			InitializeComponent();

			DgvTraceNames.CellFormatting += (sender, args) =>
			{
				if (args.RowIndex < 0 || args.ColumnIndex < 0)
					return;
				ColorizationRow(((DataGridView) sender).Rows[args.RowIndex]);
			};

			Icon = Icon.FromHandle(Resources.filter.GetHicon());
			MinimizeBox = false;
			MaximizeBox = false;

			//buttonOK.Enabled = !folderPath.IsNullOrWhiteSpace();

			CenterToScreen();

			KeyPreview = true;
			KeyDown += (sender, args) =>
			{
				if (args.KeyCode == Keys.Enter && buttonOK.Enabled)
					buttonOK_Click(this, EventArgs.Empty);
				else if (args.KeyCode == Keys.Escape)
					Close();
			};
		}

		public static async Task<TraceNameFilterForm> Get(IEnumerable<TraceNameFilter> traceNames)
		{
			var form = new TraceNameFilterForm();
			await form.DgvTraceNames.AssignCollectionAsync(traceNames, null, true);
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
			if(row == null)
				return;

			var color = row.Index.IsParity() ? Color.White : Color.FromArgb(245, 245, 245);
			if (row.DefaultCellStyle.BackColor != color)
				row.DefaultCellStyle.BackColor = color;
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
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