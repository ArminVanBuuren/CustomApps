using System;
using System.Drawing;
using System.Windows.Forms;
using LogsReader.Properties;
using Utils;
using Utils.WinForm;

namespace LogsReader.Reader.Forms
{
	public partial class AddFolder : Form
	{
		private string _lastDir;

		public string FolderPath { get; private set; } = string.Empty;

		public bool AllDirectoriesSearching => checkBoxAllDirectories.Checked;

		public AddFolder(string folderPath = null)
		{
			InitializeComponent();

			Icon = Icon.FromHandle(Resources.folder.GetHicon());
			textBoxFolder.Text = folderPath;
			_lastDir = folderPath;
			checkBoxAllDirectories.Text = Resources.Txt_Forms_AllDirectories;
			new ToolTip().SetToolTip(checkBoxAllDirectories, Resources.Txt_Forms_AddFolderTooltip);
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			FolderPath = textBoxFolder.Text.Trim();
			Close();
		}

		private void textBoxFolder_TextChanged(object sender, EventArgs e)
		{
			if (textBoxFolder.Text.IsNullOrEmptyTrim() || !IO.CHECK_PATH.IsMatch(textBoxFolder.Text))
			{
				textBoxFolder.BackColor = Color.LightPink;
				buttonOK.Enabled = false;
				return;
			}

			textBoxFolder.BackColor = Color.White;
			buttonOK.Enabled = true;
		}

		private void buttonOpenFolder_Click(object sender, EventArgs e)
		{
			if (!Folder.Open(_lastDir, out var folderResult)) 
				return;

			_lastDir = folderResult;
			textBoxFolder.Text = folderResult;
		}
	}
}