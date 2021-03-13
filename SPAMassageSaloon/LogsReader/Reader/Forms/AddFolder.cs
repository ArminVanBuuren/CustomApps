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
		private readonly bool _sourceAllDirSearching = false;

		private string _lastDir;

		public string SourceFolder { get; }

		public string FolderPath { get; private set; } = string.Empty;

		public bool AllDirectoriesSearching => checkBoxAllDirectories.Checked;

		public AddFolder(string folderPath, bool allDirSearching)
		{
			InitializeComponent();

			Icon = Icon.FromHandle(Resources.folder.GetHicon());
			base.Text = Resources.Txt_Forms_AddFolder;
			new ToolTip().SetToolTip(checkBoxAllDirectories, Resources.Txt_Forms_AddFolderTooltip);
			MinimizeBox = false;
			MaximizeBox = false;

			labelFolder.Text = Resources.Txt_Forms_Folder;

			buttonOK.Enabled = !folderPath.IsNullOrWhiteSpace();
			SourceFolder = folderPath;
			_lastDir = folderPath;
			textBoxFolder.Text = folderPath;

			_sourceAllDirSearching = allDirSearching;
			checkBoxAllDirectories.Checked = allDirSearching;
			checkBoxAllDirectories.Text = Resources.Txt_Forms_AllDirectories;

			CenterToScreen();

			KeyPreview = true;
			KeyDown += (sender, args) =>
			{
				switch (args.KeyCode)
				{
					case Keys.Enter when buttonOK.Enabled:
						buttonOK_Click(this, EventArgs.Empty);
						break;
					case Keys.Escape:
						Close();
						break;
				}
			};
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			FolderPath = textBoxFolder.Text.Trim();
			DialogResult = FolderPath != SourceFolder || AllDirectoriesSearching != _sourceAllDirSearching ? DialogResult.OK : DialogResult.Cancel;
			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
			=> Close();

		private void textBoxFolder_TextChanged(object sender, EventArgs e)
		{
			if (textBoxFolder.Text.IsNullOrWhiteSpace())
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