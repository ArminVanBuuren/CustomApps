using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Utils.WinForm.DataGridViewHelper
{
	[ToolboxBitmap(typeof(DataGridViewButtonColumn))]
	public class DgvDisableButtonColumn : DataGridViewButtonColumn
	{
		public sealed override DataGridViewCell CellTemplate
		{
			get => base.CellTemplate;
			set => base.CellTemplate = value;
		}

		public DgvDisableButtonColumn()
		{
			base.CellTemplate = new DgvDisableButtonCell();
		}

		public override object Clone()
		{
			if (base.Clone() is DgvDisableButtonColumn c)
			{
				((DgvDisableButtonCell)c.CellTemplate).Enabled = ((DgvDisableButtonCell)CellTemplate).Enabled;
				return c;
			}

			return null;
		}
	}

	public class DgvDisableButtonCell : DataGridViewButtonCell
	{
		private bool enabledValue;

		public bool Enabled
		{
			get => enabledValue;
			set
			{
				if (enabledValue == value)
					return;
				enabledValue = value;
				// force the cell to be re-painted
				DataGridView?.InvalidateCell(this);
			}
		}

		public DgvDisableButtonCell()
		{
			enabledValue = true;
		}

		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
			DataGridViewElementStates elementState, object value, object formattedValue, string errorText,
			DataGridViewCellStyle cellStyle,
			DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			// The button cell is disabled, so paint the border, background, and disabled button for the cell. 
			if (!enabledValue)
			{
				var currentContext = BufferedGraphicsManager.Current;

				using (var myBuffer = currentContext.Allocate(graphics, cellBounds))
				{
					// Draw the cell background, if specified. 
					if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
					{
						using (var cellBackground = new SolidBrush(cellStyle.BackColor))
						{
							myBuffer.Graphics.FillRectangle(cellBackground, cellBounds);
						}
					}

					// Draw the cell borders, if specified. 
					if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border)
					{
						PaintBorder(myBuffer.Graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
					}

					// Calculate the area in which to draw the button.
					var buttonArea = cellBounds;
					var buttonAdjustment = BorderWidths(advancedBorderStyle);
					buttonArea.X += buttonAdjustment.X;
					buttonArea.Y += buttonAdjustment.Y;
					buttonArea.Height -= buttonAdjustment.Height;
					buttonArea.Width -= buttonAdjustment.Width;

					// Draw the disabled button.                
					ButtonRenderer.DrawButton(myBuffer.Graphics, buttonArea, PushButtonState.Disabled);

					// Draw the disabled button text.  
					if (FormattedValue is string formattedValueString)
					{
						TextRenderer.DrawText(myBuffer.Graphics, formattedValueString, DataGridView.Font, buttonArea, SystemColors.GrayText, TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
					}

					myBuffer.Render();
				}
			}
			else
			{
				// The button cell is enabled, so let the base class handle the painting. 
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
			}
		}

		public override object Clone()
		{
			var cell = (DgvDisableButtonCell)base.Clone();
			cell.Enabled = Enabled;
			return cell;
		}
	}
}
