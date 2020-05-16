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
		private readonly Dictionary<string, List<string>> _typesGroups;
		private string _currentGroup = null;

		public TypesGroupForm(string selectedGroup, Dictionary<string, List<string>> typesGroups)
		{
			InitializeComponent();

			Icon = Icon.FromHandle(Resources.types_group.GetHicon());
			base.Text = Resources.Txt_Forms_FileTypesGroup;
			labelGroup.Text = Resources.Txt_Forms_Group;
			buttonCancel.Text = Resources.Txt_Forms_Cancel;

			_typesGroups = typesGroups;

			comboboxGroup.Items.AddRange(_typesGroups.Keys.ToArray());
			comboboxGroup.Text = selectedGroup;
			comboboxGroup.SelectedText = selectedGroup;
			ComboboxGroup_SelectionChangeCommitted(this, EventArgs.Empty);
			comboboxGroup.SelectionChangeCommitted += ComboboxGroup_SelectionChangeCommitted;
			comboboxGroup.TextChanged += comboboxGroup_TextChanged;

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

			if (_typesGroups.TryGetValue(_currentGroup, out var fileTypes) && fileTypes.Count > 0)
			{
				richTextBoxTypes.Text = string.Join(", ", fileTypes);
			}
			else
			{
				richTextBoxTypes.Text = string.Empty;
			}

			buttonOK.Enabled = true;
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;

			if (_currentGroup != null && buttonOK.Enabled)
			{
				_typesGroups[_currentGroup] =
					new List<string>(richTextBoxTypes.Text.Split(',')
						.Select(x => x.Trim())
						.Where(x => !x.IsNullOrEmptyTrim())
						.Distinct(StringComparer.InvariantCultureIgnoreCase));
			}

			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void comboboxGroup_TextChanged(object sender, EventArgs e)
		{
			if (comboboxGroup.Text.IsNullOrEmptyTrim() || (_typesGroups.TryGetValue(comboboxGroup.Text.Trim(), out var _) && _currentGroup != comboboxGroup.Text.Trim()))
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
	}
}
