using System;
using System.IO;
using System.Windows.Forms;

namespace Utils.WinForm
{
	public static class Folder
	{
		public static bool Open(string lastDir, out string result)
		{
			result = null;
			try
			{
				using (var fbd = new FolderBrowserDialog())
				{
					if (!lastDir.IsNullOrEmpty())
						fbd.SelectedPath = lastDir;

					if (fbd.ShowDialog() == DialogResult.OK
					    && !string.IsNullOrWhiteSpace(fbd.SelectedPath)
					    && Directory.Exists(fbd.SelectedPath))
					{
						result = fbd.SelectedPath;
						return true;
					}
				}

				return false;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}
}