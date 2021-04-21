using System;
using System.Drawing;
using System.Windows.Forms;
using Utils.WinForm;

namespace SPAMassageSaloon
{
	public partial class ProgressForm : Form
	{
		public ProgressForm()
		{
			InitializeComponent();
			base.Text = MainForm.FormName;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ResetLocation();
		}

		public void SetProgress(int value)
		{
			if (value > progressBar.Value && value <= 100)
				progressBar.SafeInvoke(() => { progressBar.Value = value; });
		}

		public new void Show(IWin32Window owner)
		{
			base.Show(owner);
			ResetLocation();
		}

		public void ResetLocation()
		{
			if (Owner != null)
				Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2, Owner.Location.Y + Owner.Height / 2 - Height / 2);
		}
	}
}
