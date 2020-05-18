using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LogsReader.Properties;

namespace LogsReader.Reader.Forms
{
	public enum GroupType
	{
		Server = 0,
		FileType = 1
	}

	public partial class AddGroupForm : Form
	{
		private readonly GroupType _groupType;
		private readonly Dictionary<string, List<string>> _groups;

		public AddGroupForm(Dictionary<string, List<string>> groups, GroupType groupType)
		{
			InitializeComponent();

			_groups = groups;
			_groupType = groupType;

			Icon = Icon.FromHandle(_groupType == GroupType.Server ? Resources.server_group.GetHicon() : Resources.types_group.GetHicon());
			base.Text = Resources.Txt_Forms_AddGroup;
			labelGroupName.Text =  Resources.Txt_Forms_GroupName;
			buttonCancel.Text = Resources.Txt_Forms_Cancel;
			buttonOK.Enabled = false;
			MinimizeBox = false;
			MaximizeBox = false;

			CenterToScreen();

			KeyPreview = true;
			KeyDown += (sender, args) =>
			{
				if (args.KeyCode == Keys.Enter && buttonOK.Enabled)
					buttonOK_Click(this, EventArgs.Empty);
				else if (args.KeyCode == Keys.Escape)
					Close();
			};
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			_groups.Add(textBoxGroupName.Text, new List<string>());
			DialogResult = ShowGroupItemsForm(textBoxGroupName.Text, _groups, _groupType);
			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		public static DialogResult ShowGroupItemsForm(string groupName, Dictionary<string, List<string>> groups, GroupType groupType)
		{
			DialogResult result;
			if (groupType == GroupType.Server)
				result = new ServerGroupForm(groupName, groups).ShowDialog();
			else
				result = new TypesGroupForm(groupName, groups).ShowDialog();
			return result;
		}

		private void textBoxGroupName_TextChanged(object sender, EventArgs e)
		{
			if (_groups.TryGetValue(textBoxGroupName.Text.Trim(), out var _))
			{
				textBoxGroupName.BackColor = Color.LightPink;
				buttonOK.Enabled = false;
				return;
			}

			textBoxGroupName.BackColor = Color.White;
			buttonOK.Enabled = true;
		}
	}
}