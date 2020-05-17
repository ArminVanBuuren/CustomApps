using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils.WinForm
{
	public class GrowLabel : Label
	{
		private bool _mGrowing;
		public GrowLabel()
		{
			base.AutoSize = false;
		}

		private void resizeLabel()
		{
			if (_mGrowing)
				return;
			try
			{
				_mGrowing = true;
				var sz = new Size(this.Width, int.MaxValue);
				sz = TextRenderer.MeasureText(this.Text, this.Font, sz, TextFormatFlags.WordBreak);
				this.Height = sz.Height;
			}
			finally
			{
				_mGrowing = false;
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			resizeLabel();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			resizeLabel();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			resizeLabel();
		}
	}
}
