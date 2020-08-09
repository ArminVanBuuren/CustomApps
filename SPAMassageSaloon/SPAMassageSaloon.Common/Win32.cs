using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SPAMassageSaloon.Common
{
	public class Win32
	{
		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
		public const int WM_SETREDRAW = 0xB;
	}
}
