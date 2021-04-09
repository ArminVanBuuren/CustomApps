using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SPAMassageSaloon.Properties;
using Utils;

namespace SPAMassageSaloon
{
	internal class MDIManagerButton : ToolStripStatusLabel
	{
		public Form mdiForm { get; }
		public event EventHandler MClick;
		public event EventHandler Activated;
		public event EventHandler MClose;

		private int btnTop, btnLeft;
		private Rectangle btnRectangle;
		private bool mouseOnCloseBtn;

		public MDIManagerButton(Form form)
		{
			mdiForm = form;
			mdiForm.FormClosed += (s, e) => { Owner.Items.Remove(this); };
			mdiForm.TextChanged += (s, e) => { Text = mdiForm.Text + @"     "; };
			mdiForm.Activated += (s, e) => { MDIManagerButton_Click(this, null); };
			base.Text = mdiForm.Text.RegexReplace(@"\-\s*([Vv])|([0-9.]+)", string.Empty).Trim() + @"     ";
			base.Padding = new Padding(6, 0, 0, 0);
			base.TextAlign = ContentAlignment.MiddleCenter;
			Margin = new Padding(0, 0, 0, 0);
			MouseUp += MDIManagerButton_MouseUp;
			MouseMove += MDIManagerButton_MouseMove;
			Paint += MDIManagerButton_Paint;
			Click += MDIManagerButton_Click;
			BorderSides = ToolStripStatusLabelBorderSides.All;
		}

		private void MDIManagerButton_Click(object sender, EventArgs e)
		{
			if (Parent == null)
				return;

			try
			{
				foreach (var btn in Parent.Items.OfType<MDIManagerButton>())
				{
					btn.BorderStyle = Border3DStyle.Flat;
					btn.BackColor = SystemColors.ButtonFace;
				}

				BorderStyle = Border3DStyle.Sunken;
				BackColor = Color.White;
			}
			catch (Exception ex)
			{
				// ignored
			}
			finally
			{
				Activated?.Invoke(this, null);
			}
		}

		private void MDIManagerButton_MouseUp(object sender, MouseEventArgs e)
		{
			switch (e.Button)
			{
				case MouseButtons.Left:
					if (mouseOnCloseBtn)
						MClose?.Invoke(sender, e);
					else
						MClick?.Invoke(sender, e);
					break;
			}
		}

		private void MDIManagerButton_MouseMove(object sender, MouseEventArgs e)
		{
			mouseOnCloseBtn = btnRectangle.Contains(e.Location);
			Invalidate();
		}

		private void MDIManagerButton_Paint(object sender, PaintEventArgs e)
		{
			btnLeft = Width - Resources.close.Size.Width - 6;
			btnTop = (Height - Resources.close.Size.Height) / 2;
			btnRectangle = new Rectangle(btnLeft, btnTop, Resources.close.Size.Width, Resources.close.Size.Height);
			if (mouseOnCloseBtn)
				e.Graphics.DrawImage(Resources.close_active, new Point(btnLeft, btnTop));
			else
				e.Graphics.DrawImage(Resources.close, new Point(btnLeft, btnTop));
		}
	}
}