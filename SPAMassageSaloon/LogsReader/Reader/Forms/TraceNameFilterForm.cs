using System;
using System.Collections.Generic;
using System.Drawing;
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

				var row = ((DataGridView)sender).Rows[args.RowIndex];
				row.DefaultCellStyle.BackColor = row.Index.IsParity() ? Color.White : Color.FromArgb(245, 245, 245);
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
			return form;
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