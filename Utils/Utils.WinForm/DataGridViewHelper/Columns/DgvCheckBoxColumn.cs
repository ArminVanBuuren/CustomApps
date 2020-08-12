using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Utils.WinForm.DataGridViewHelper
{
	[ToolboxBitmap(typeof(DataGridViewCheckBoxColumn))]
	public class DgvCheckBoxColumn : DataGridViewCheckBoxColumn
	{
		private readonly DgvColumnCheckBoxHeaderCell dgvCheckBoxHeaderCell;

		public sealed override DataGridViewCell CellTemplate
		{
			get => base.CellTemplate;
			set => base.CellTemplate = value;
		}

		public bool Checked
		{
			get => dgvCheckBoxHeaderCell.Checked;
			set
			{
				dgvCheckBoxHeaderCell.Checked = value;
				//DataGridView.Invalidate();  // request a delayed Repaint by the normal MessageLoop system
				//DataGridView.Update();      // forces Repaint of invalidated area
				DataGridView.Refresh();       // Combines Invalidate() and Update()
			}
		}

		public DgvCheckBoxColumn()
		{
			CellTemplate = new DgvCheckBoxCell(); 

			dgvCheckBoxHeaderCell = new DgvColumnCheckBoxHeaderCell();

			HeaderCell = dgvCheckBoxHeaderCell;
			MinimumWidth = Width = 50;

			dgvCheckBoxHeaderCell.OnCheckBoxClicked += DgvCheckBoxHeaderCell_OnCheckBoxClicked;
		}

		void DgvCheckBoxHeaderCell_OnCheckBoxClicked(int columnIndex, bool state)
		{
			if (DataGridView == null)
				return;
			try
			{
				DataGridView.RefreshEdit();

				foreach (DataGridViewRow row in DataGridView.Rows)
					if (!row.Cells[columnIndex].ReadOnly)
						if (row.Cells[columnIndex] is DgvCheckBoxCell dgvCbxCell)
							dgvCbxCell.Checked = state;
						else
							row.Cells[columnIndex].Value = state;
			}
			finally
			{
				DataGridView.RefreshEdit();
			}
		}
	}

	public delegate void CheckBoxClickedHandler(int columnIndex, bool state);

	public class DgvCheckBoxHeaderCellEventArgs : EventArgs
	{
		public DgvCheckBoxHeaderCellEventArgs(int columnIndex, bool bChecked)
		{
			ColumnIndex = columnIndex;
			Checked = bChecked;
		}

		public int ColumnIndex { get; }

		public bool Checked { get; }
	}

	public class DgvColumnCheckBoxHeaderCell : DataGridViewColumnHeaderCell
	{
		private Point checkBoxLocation;
		private Size checkBoxSize;
		private Point _cellLocation = new Point();
		private bool enabled = true;
		private CheckBoxState _cbState = CheckBoxState.UncheckedNormal;

		public event CheckBoxClickedHandler OnCheckBoxClicked;

		public bool Checked
		{
			get => _cbState == CheckBoxState.CheckedNormal || _cbState == CheckBoxState.CheckedDisabled;
			set => UpdateState(value, false);
		}

		public bool Enabled
		{
			get => enabled;
			set
			{
				if(enabled == value)
					return;

				enabled = value;
				UpdateState(Checked, false);
				DataGridView?.Invalidate();
			}
		}

		internal void UpdateState(bool @checked, bool onlyHeader)
		{
			var state = @checked
				? Enabled
					? CheckBoxState.CheckedNormal
					: CheckBoxState.CheckedDisabled
				: Enabled
					? CheckBoxState.UncheckedNormal
					: CheckBoxState.UncheckedDisabled;

			if (_cbState == state)
				return;

			_cbState = state;

			if (onlyHeader)
			{
				DataGridView?.InvalidateCell(this);
			}
			else
			{
				OnCheckBoxClicked?.Invoke(this.ColumnIndex, Checked);
				DataGridView?.Refresh();
			}
		}

		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
			DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText,
			DataGridViewCellStyle cellStyle,
			DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			base.Paint(graphics, clipBounds, cellBounds, rowIndex,
				dataGridViewElementState, value,
				formattedValue, errorText, cellStyle,
				advancedBorderStyle, paintParts);

			var p = new Point();
			var s = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal);

			p.X = cellBounds.Location.X + cellBounds.Width / 2 - s.Width / 2;
			p.Y = cellBounds.Location.Y + cellBounds.Height / 2 - s.Height / 2;

			_cellLocation = cellBounds.Location;
			checkBoxLocation = p;
			checkBoxSize = s;

			CheckBoxRenderer.DrawCheckBox(graphics, checkBoxLocation, _cbState);
		}

		protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
		{
			var p = new Point(e.X + _cellLocation.X, e.Y + _cellLocation.Y);
			if (p.X >= checkBoxLocation.X && p.X <=
										  checkBoxLocation.X + checkBoxSize.Width
										  && p.Y >= checkBoxLocation.Y && p.Y <=
										  checkBoxLocation.Y + checkBoxSize.Height)
			{
				if(!Enabled)
					return;

				Checked = !Checked;
			}

			base.OnMouseClick(e);
		}
	}

	public class DgvCheckBoxCell : DataGridViewCheckBoxCell
	{
		private Point checkBoxLocation;
		private Size checkBoxSize;
		private Point _cellLocation = new Point();
		private bool enabled = true;
		private CheckBoxState _cbState = CheckBoxState.UncheckedNormal;

		public bool Checked
		{
			get => _cbState == CheckBoxState.CheckedNormal || _cbState == CheckBoxState.CheckedDisabled;
			set => UpdateState(value);
		}

		public bool Enabled
		{
			get => enabled;
			set
			{
				if (enabled == value)
					return;

				enabled = value;
				UpdateState(Checked);
			}
		}

		void UpdateState(bool @checked)
		{
			var state = @checked
				? Enabled
					? CheckBoxState.CheckedNormal
					: CheckBoxState.CheckedDisabled
				: Enabled
					? CheckBoxState.UncheckedNormal
					: CheckBoxState.UncheckedDisabled;

			if (_cbState == state)
				return;

			_cbState = state;
			DataGridView?.InvalidateCell(this);
		}

		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
			DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText,
			DataGridViewCellStyle cellStyle,
			DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			base.Paint(graphics, clipBounds, cellBounds, rowIndex,
				dataGridViewElementState, value,
				formattedValue, errorText, cellStyle,
				advancedBorderStyle, paintParts);

			var p = new Point();
			var s = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal);

			p.X = Math.Max(0, (cellBounds.Location.X + cellBounds.Width / 2 - s.Width / 2) - 1);
			p.Y = Math.Max(0, (cellBounds.Location.Y + cellBounds.Height / 2 - s.Height / 2) - 1);

			_cellLocation = cellBounds.Location;
			checkBoxLocation = p;
			checkBoxSize = s;

			CheckBoxRenderer.DrawCheckBox(graphics, checkBoxLocation, _cbState);
		}

		protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
		{
			var p = new Point(e.X + _cellLocation.X, e.Y + _cellLocation.Y);
			if (p.X >= checkBoxLocation.X && p.X <=
			                              checkBoxLocation.X + checkBoxSize.Width
			                              && p.Y >= checkBoxLocation.Y && p.Y <=
			                              checkBoxLocation.Y + checkBoxSize.Height)
			{
				if (!Enabled)
					return;

				Checked = !Checked;
				CheckColumn();
			}

			base.OnMouseClick(e);
		}

		void CheckColumn()
		{
			if (DataGridView == null || !(DataGridView.Columns[ColumnIndex] is DgvCheckBoxColumn column))
				return;

			DataGridView.CheckStatusHeader(column);
			DataGridView.InvalidateCell(this);
		}

		public override object Clone()
		{
			if (base.Clone() is DgvCheckBoxCell c)
			{
				c.Checked = Checked;
				c.Enabled = Enabled;
				return c;
			}

			return null;
		}
	}

	public class DgvColumnSelector
	{
		// the DataGridView to which the DataGridViewColumnSelector is attached
		private DataGridView mDataGridView = null;

		// a CheckedListBox containing the column header text and checkboxes
		private readonly CheckedListBox mCheckedListBox;

		// a ToolStripDropDown object used to show the popup
		private readonly ToolStripDropDown mPopup;

		/// <summary>
		/// The max height of the popup
		/// </summary>
		public int MaxHeight { get; set; } = 300;

		/// <summary>
		/// The width of the popup
		/// </summary>
		public int Width { get; set; } = 200;

		// The constructor creates an instance of CheckedListBox and ToolStripDropDown.
		// the CheckedListBox is hosted by ToolStripControlHost, which in turn is
		// added to ToolStripDropDown.
		public DgvColumnSelector()
		{
			mCheckedListBox = new CheckedListBox
			{
				CheckOnClick = true
			};
			mCheckedListBox.ItemCheck += MCheckedListBox_ItemCheck;

			var mControlHost = new ToolStripControlHost(mCheckedListBox)
			{
				Padding = Padding.Empty,
				Margin = Padding.Empty,
				AutoSize = false
			};

			mPopup = new ToolStripDropDown { Padding = Padding.Empty };
			mPopup.Items.Add(mControlHost);
		}

		public DgvColumnSelector(DataGridView dgv) : this()
		{
			DataGridView = dgv;
		}

		/// <summary>
		/// Gets or sets the DataGridView to which the DataGridViewColumnSelector is attached
		/// </summary>
		public DataGridView DataGridView
		{
			get => mDataGridView;
			set
			{
				// If any, remove handler from current DataGridView 
				if (mDataGridView != null)
					mDataGridView.CellMouseClick -= MDataGridView_CellMouseClick;
				// Set the new DataGridView
				mDataGridView = value;
				// Attach CellMouseClick handler to DataGridView
				if (mDataGridView != null)
					mDataGridView.CellMouseClick += MDataGridView_CellMouseClick;
			}
		}

		// When user right-clicks the cell origin, it clears and fill the CheckedListBox with
		// columns header text. Then it shows the popup. 
		// In this way the CheckedListBox items are always refreshed to reflect changes occurred in 
		// DataGridView columns (column additions or name changes and so on).
		void MDataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right && e.RowIndex == -1 && e.ColumnIndex == 0)
			{
				mCheckedListBox.Items.Clear();
				foreach (DataGridViewColumn c in mDataGridView.Columns)
					mCheckedListBox.Items.Add(c.HeaderText, c.Visible);

				var PreferredHeight = mCheckedListBox.Items.Count * 16 + 7;
				mCheckedListBox.Height = PreferredHeight < MaxHeight ? PreferredHeight : MaxHeight;
				mCheckedListBox.Width = Width;
				mPopup.Show(mDataGridView.PointToScreen(new Point(e.X, e.Y)));
			}
		}

		// When user checks / unchecks a checkbox, the related column visibility is 
		// switched.
		void MCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			mDataGridView.Columns[e.Index].Visible = e.NewValue == CheckState.Checked;
		}
	}
}
