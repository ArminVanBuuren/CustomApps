using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SPAMassageSaloon.Common
{
	public static class Win32
	{
		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
		public const int WM_SETREDRAW = 0xB;

		public static void SuspendHandle(this Control control)
		{
			Win32.SendMessage(control.Handle, Win32.WM_SETREDRAW, 0, 0);
		}

		public static void ResumeHandle(this Control control)
		{
			Win32.SendMessage(control.Handle, Win32.WM_SETREDRAW, 1, 0);
			control.Refresh();
		}
	}
}
