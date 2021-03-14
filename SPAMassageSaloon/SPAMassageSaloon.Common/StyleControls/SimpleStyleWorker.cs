using System;
using System.Windows.Forms;

namespace SPAMassageSaloon.Common.StyleControls
{
	public class SimpleStyleWorker
	{
		private readonly Control _control;
		private readonly ControlStyleColor _enabled;
		private readonly ControlStyleColor _disabled;

		public SimpleStyleWorker(Control control, ControlStyleColor enabled, ControlStyleColor disabled)
		{
			_control = control;
			_enabled = enabled;
			_disabled = disabled;

			_control.EnabledChanged += Control_EnabledChanged;
			_control.Disposed += Control_Disposed;
			ThreadStyle.StyleChanged += StyleControl_StyleChanged;
		}

		private void StyleControl_StyleChanged(object sender, EventArgs e) => SetStyle();

		private void Control_EnabledChanged(object sender, EventArgs e) => SetStyle();


		void SetStyle()
		{
			if (_control.Enabled)
			{
				_control.BackColor = _enabled.Back;
				_control.ForeColor = _enabled.Fore;
			}
			else
			{
				_control.BackColor = _disabled.Back;
				_control.ForeColor = _disabled.Fore;
			}
		}

		private void Control_Disposed(object sender, EventArgs e)
		{
			ThreadStyle.StyleChanged -= StyleControl_StyleChanged;
			_control.EnabledChanged -= Control_EnabledChanged;
			_control.Disposed -= Control_Disposed;
		}
	}
}