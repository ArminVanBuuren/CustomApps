using System.Windows.Forms;

namespace SPAMassageSaloon.Common.StyleControls
{
	public class SButton : Button
	{
		private readonly SimpleStyleWorker _worker;
		public SButton() => _worker = new SimpleStyleWorker(this, ThreadStyle.Style.EnabledButton, ThreadStyle.Style.DisabledButton);
	}
}