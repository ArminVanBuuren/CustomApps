using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using LogsReader.Config;
using LogsReader.Properties;

namespace LogsReader.Reader.Forms
{
	public partial class ConfigureForm : Form
	{
		private LRSettingsScheme _lastSuccessResult = null;

		private string SchemeName { get; }

		public LRSettingsScheme SettingsOfScheme { get; private set; }

		public ConfigureForm(LRSettingsScheme currentSettings)
		{
			InitializeComponent();
			ReloadhButton.Text = Resources.TxtConfigureReload;
			ValidateOrOk.Text = Resources.TxtConfigureSuccess;
			ValidateOrOk.Image = Resources.finished;
			CancelButton.Text = Resources.TxtConfigureCancel;

			SchemeName = currentSettings.Name;
			SettingsOfScheme = currentSettings;

			Text = Resources.Txt_Form_ConfigureButton;
			Icon = Icon.FromHandle(Resources.settings.GetHicon());

			editor.Text = SettingsOfScheme.Serialize();
			editor.SizingGrip = false;
			editor.WordWrap = false;
			editor.SetLanguages(new[] { Language.XML }, Language.XML);
			editor.TextChanged += Editor_TextChanged;

			KeyPreview = true;
			KeyDown += (sender, args) =>
			{
				switch (args.KeyCode)
				{
					case Keys.S when args.Control:
						ValidateOrOk_Click(ValidateOrOk, EventArgs.Empty);
						break;

					case Keys.Escape:
						Cancel_Click(CancelButton, EventArgs.Empty);
						break;
				}
			};
		}

		private void Editor_TextChanged(object sender, EventArgs e)
		{
			ValidateOrOk.Text = Resources.TxtConfigureValidate;
			ValidateOrOk.Image = Resources.check;
		}

		private void ValidateOrOk_Click(object sender, EventArgs e)
		{
			// Если пользователь нажал на применить
			if (ValidateOrOk.Text == Resources.TxtConfigureSuccess)
			{
				SettingsOfScheme = _lastSuccessResult;
				DialogResult = DialogResult.OK;
				Close();
				return;
			}

			try
			{
				// Если пользователь нажал на проверить
				if (LRSettingsScheme.TryDeserialize(editor.Text, out var result))
				{
					if (result.Name != SchemeName)
					{
						MessageBox.Show(string.Format(Resources.TxtConfigureSchemeNameFailed, result.Name), @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					editor.Text = result.Serialize();
					_lastSuccessResult = result;
					ValidateOrOk.Text = Resources.TxtConfigureSuccess;
					ValidateOrOk.Image = Resources.finished;
					MessageBox.Show(Resources.TxtConfigureSuccessful, @"Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					MessageBox.Show(string.Format(Resources.TxtConfigureFailed, SchemeName), @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format(Resources.TxtConfigureFailed, SchemeName), @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void Reload_Click(object sender, EventArgs e)
		{
			editor.Text = SettingsOfScheme.Serialize();
			_lastSuccessResult = null;
			ValidateOrOk.Text = Resources.TxtConfigureSuccess;
			ValidateOrOk.Image = Resources.finished;
		}
	}
}
