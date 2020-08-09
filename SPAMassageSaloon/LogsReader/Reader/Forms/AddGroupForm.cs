using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LogsReader.Properties;
using Utils;

namespace LogsReader.Reader.Forms
{
	public partial class AddGroupForm : Form
	{
		private readonly GroupType _groupType;
		private readonly Dictionary<string, (int, List<string>)> _groups;

		public AddGroupForm(Dictionary<string, (int, List<string>)> groups, GroupType groupType)
		{
			InitializeComponent();

			_groups = groups;
			_groupType = groupType;

			Icon = Icon.FromHandle(_groupType == GroupType.Server ? Resources.server_group.GetHicon() : Resources.types_group.GetHicon());
			
			base.Text = Resources.Txt_Forms_AddGroup;
			labelGroupName.Text = Resources.Txt_Forms_GroupName;
			buttonCancel.Text = Resources.Txt_Forms_Cancel;
			labelPriority.Text = Resources.Txt_Forms_GroupPriority;

			buttonOK.Enabled = false;
			MinimizeBox = false;
			MaximizeBox = false;

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
			_groups.Add(textBoxGroupName.Text, (GetGroupPriority(textBoxPriority.Text), new List<string>()));
			DialogResult = ShowGroupItemsForm(textBoxGroupName.Text, _groups, _groupType);
			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		public static DialogResult ShowGroupItemsForm(string groupName, Dictionary<string, (int, List<string>)> groups, GroupType groupType)
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

		private void textBoxPriority_TextChanged(object sender, EventArgs e)
		{
			try
			{
				textBoxPriority.TextChanged -= textBoxPriority_TextChanged;
				if (!textBoxPriority.Text.IsNullOrWhiteSpace())
					textBoxPriority.Text = GetGroupPriority(textBoxPriority.Text).ToString();
			}
			catch (Exception)
			{
				textBoxPriority.Text = @"0";
			}
			finally
			{
				textBoxPriority.TextChanged += textBoxPriority_TextChanged;
			}
		}

		internal static int GetGroupPriority(string value)
		{
			if (!value.IsNullOrWhiteSpace() && int.TryParse(value, out var result) && result >= 0)
				return result;
			return 0;
		}
	}
}