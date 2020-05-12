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

			_typesGroups = typesGroups;

			comboboxGroup.Items.AddRange(_typesGroups.Keys.ToArray());
			comboboxGroup.SelectionChangeCommitted += ComboboxGroup_SelectionChangeCommitted;
			comboboxGroup.SelectedText = selectedGroup;

			CenterToScreen();
		}

		private void ComboboxGroup_SelectionChangeCommitted(object sender, EventArgs e)
		{
			Save();
			_currentGroup = comboboxGroup.SelectedText;
			buttonOK.Enabled = true;
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			Save();
			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void comboboxGroup_TextChanged(object sender, EventArgs e)
		{
			if (_typesGroups.TryGetValue(comboboxGroup.Text, out var result))
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

		void Save()
		{
			if (_currentGroup != null && buttonOK.Enabled)
			{
				_typesGroups[_currentGroup] =
					new List<string>(richTextBoxTypes.Text.Split(',')
						.Where(x => !x.IsNullOrEmptyTrim())
						.Distinct(StringComparer.InvariantCultureIgnoreCase));
			}
		}
	}
}
