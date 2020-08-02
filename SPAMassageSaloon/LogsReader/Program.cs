using System;
using System.Drawing;
using System.Windows.Forms;
using SPAMassageSaloon.Common;

namespace LogsReader
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new LogsReaderMainForm());
			}
			catch (Exception ex)
			{
				ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, @"Run");
			}
		}
	}

	public class DgvTextAndImageColumn : DataGridViewTextBoxColumn
	{
		private Image imageValue;
		private Size imageSize;

		public DgvTextAndImageColumn()
		{
			CellTemplate = new DgvTextAndImageCell();
		}

		public sealed override DataGridViewCell CellTemplate
		{
			get => base.CellTemplate;
			set => base.CellTemplate = value;
		}

		public Image Image
		{
			get => imageValue;
			set
			{
				if (Equals(Image, value)) 
					return;

				imageValue = value;

				if (imageValue != null)
				{
					imageSize = imageValue.Size;

					if (InheritedStyle != null)
					{
						var inheritedPadding = InheritedStyle.Padding;
						DefaultCellStyle.Padding = new Padding(imageSize.Width, inheritedPadding.Top, inheritedPadding.Right, inheritedPadding.Bottom);
					}
				}
				else
				{
					DefaultCellStyle.Padding = new Padding(0);
				}
			}
		}

		private DgvTextAndImageCell TextAndImageCellTemplate => CellTemplate as DgvTextAndImageCell;

		internal Size ImageSize => imageSize;

		public override object Clone()
		{
			if (base.Clone() is DgvTextAndImageColumn c)
			{
				c.imageValue = imageValue;
				c.imageSize = imageSize;
				return c;
			}

			return null;
		}
	}

	public class DgvTextAndImageCell : DataGridViewTextBoxCell
	{
		private Image imageValue;
		private Size imageSize;

		private DgvTextAndImageColumn OwningTextAndImageColumn => OwningColumn as DgvTextAndImageColumn;

		public Image Image
		{
			get
			{
				if (OwningColumn == null || OwningTextAndImageColumn == null)
					return imageValue;
				else if (imageValue != null)
					return imageValue;
				else
					return OwningTextAndImageColumn.Image;
			}
			set
			{
				if (Equals(imageValue, value)) 
					return;

				imageValue = value;

				if (imageValue != null)
				{
					imageSize = imageValue.Size;

					var inheritedPadding = InheritedStyle.Padding;
					Style.Padding = new Padding(imageSize.Width + 2, inheritedPadding.Top, inheritedPadding.Right, inheritedPadding.Bottom);
				}
				else
				{
					Style.Padding = new Padding(0);
				}
			}
		}

		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, 
			DataGridViewElementStates cellState, object value, object formattedValue, string errorText,
			DataGridViewCellStyle cellStyle,
			DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			// Paint the base content
			base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState,
				value, formattedValue, errorText, cellStyle,
				advancedBorderStyle, paintParts);

			if (Image == null) 
				return;

			// Draw the image clipped to the cell.
			var container = graphics.BeginContainer();

			var point = new Point(cellBounds.Location.X, cellBounds.Location.Y);
			point.X = point.X + 2;
			graphics.SetClip(cellBounds);
			graphics.DrawImageUnscaled(Image, point);

			graphics.EndContainer(container);
		}

		public override object Clone()
		{
			if (base.Clone() is DgvTextAndImageCell c)
			{
				c.imageValue = imageValue;
				c.imageSize = imageSize;
				return c;
			}

			return null;
		}
	}
}