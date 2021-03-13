using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LogsReader.Properties;
using Utils;

namespace LogsReader.Reader.Forms
{
	public partial class TypesGroupForm : Form
	{
		private readonly Dictionary<string, (int, List<string>)> _typesGroups;
		private string _currentGroup = null;

		private readonly string selectedGroupPriority = @"0";

		public TypesGroupForm(string selectedGroup, Dictionary<string, (int, List<string>)> typesGroups)
		{
			InitializeComponent();

			Icon = Icon.FromHandle(Resources.types_group.GetHicon());
			base.Text = Resources.Txt_Forms_FileTypesGroup;
			labelGroupName.Text = Resources.Txt_Forms_GroupName;
			buttonCancel.Text = Resources.Txt_Forms_Cancel;
			labelPriority.Text = Resources.Txt_Forms_GroupPriority;

			MinimizeBox = false;
			MaximizeBox = false;

			_typesGroups = typesGroups;

			comboboxGroup.Items.AddRange(_typesGroups.Keys.ToArray());
			comboboxGroup.Text = selectedGroup;
			comboboxGroup.SelectedText = selectedGroup;
			ComboboxGroup_SelectionChangeCommitted(this, EventArgs.Empty);
			comboboxGroup.SelectionChangeCommitted += ComboboxGroup_SelectionChangeCommitted;
			comboboxGroup.TextChanged += comboboxGroup_TextChanged;

			if (typesGroups.TryGetValue(selectedGroup, out var res))
				textBoxGroupPriority.Text = selectedGroupPriority = res.Item1.ToString();

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

		private void ComboboxGroup_SelectionChangeCommitted(object sender, EventArgs e)
		{
			_currentGroup = comboboxGroup.SelectedItem.ToString();

			if (_typesGroups.TryGetValue(_currentGroup, out var fileTypes) && fileTypes.Item2.Count > 0)
			{
				richTextBoxTypes.Text = string.Join(", ", fileTypes.Item2);
				textBoxGroupPriority.Text = fileTypes.Item1.ToString();
			}
			else
			{
				richTextBoxTypes.Text = string.Empty;
				textBoxGroupPriority.Text = selectedGroupPriority;
			}

			buttonOK.Enabled = true;
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;

			if (_currentGroup != null && buttonOK.Enabled)
			{
				_typesGroups[_currentGroup] =
					(AddGroupForm.GetGroupPriority(textBoxGroupPriority.Text),
					 new List<string>(richTextBoxTypes.Text.Split(',')
					                                  .Select(x => x.Trim())
					                                  .Where(x => !x.IsNullOrWhiteSpace())
					                                  .Distinct(StringComparer.InvariantCultureIgnoreCase)));
			}

			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
			=> Close();

		private void comboboxGroup_TextChanged(object sender, EventArgs e)
		{
			if (comboboxGroup.Text.IsNullOrWhiteSpace() || _typesGroups.TryGetValue(comboboxGroup.Text.Trim(), out var _) && _currentGroup != comboboxGroup.Text.Trim())
			{
				buttonOK.Enabled = false;
				comboboxGroup.BackColor = Color.LightPink;
			}
			else
			{
				_typesGroups.RenameKey(_currentGroup, comboboxGroup.Text);
				_currentGroup = comboboxGroup.Text;
				buttonOK.Enabled = true;
				comboboxGroup.BackColor = Color.White;
			}
		}

		private void textBoxGroupPriority_TextChanged(object sender, EventArgs e)
		{
			try
			{
				textBoxGroupPriority.TextChanged -= textBoxGroupPriority_TextChanged;
				if (!textBoxGroupPriority.Text.IsNullOrWhiteSpace())
					textBoxGroupPriority.Text = AddGroupForm.GetGroupPriority(textBoxGroupPriority.Text).ToString();
			}
			catch (Exception)
			{
				// ignored
			}
			finally
			{
				textBoxGroupPriority.TextChanged += textBoxGroupPriority_TextChanged;
			}
		}

		private void TypesGroupForm_Resize(object sender, EventArgs e)
			=> Refresh();
	}
}