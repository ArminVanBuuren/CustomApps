using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Utils.WinForm
{
	[Designer(typeof(Splitter))]
	public class VerticalSeparator : Control
	{
		private Color lineColor;
		private Pen linePen;

		public VerticalSeparator()
		{
			this.LineColor = Color.LightGray;
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		}

		[Browsable(true)]
		[Description("Colors of line.")]
		public Color LineColor
		{
			get => this.lineColor;
			set
			{
				this.lineColor = value;

				this.linePen = new Pen(this.lineColor, 1)
				{
					Alignment = PenAlignment.Inset
				};

				Refresh();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.linePen != null)
			{
				this.linePen.Dispose();
				this.linePen = null;
			}

			base.Dispose(disposing);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;
			var x = this.Width / 2;

			g.DrawLine(linePen, x, 0, x, this.Height);

			base.OnPaint(e);
		}
    }
}
