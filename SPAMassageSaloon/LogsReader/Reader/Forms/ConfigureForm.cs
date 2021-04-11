using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using LogsReader.Config;
using LogsReader.Properties;

namespace LogsReader.Reader.Forms
{
	public partial class ConfigureForm : Form
	{
		private readonly string[] keywords =
		{
			"name", "priority", "ID", "Date", "TraceName", "Description", "Message", "Trn",
		};

		private readonly string[] snippets = {
			"cultureList=\"\"",
			"cultureList=\"ru-RU\"",
			"cultureList=\"en-US\"",
			"cultureList=\"ru-RU;en-US\"",
			"culture=\"\"",
			"culture=\"ru-RU\"",
			"culture=\"en-US\"",
			"displayDateFormat=\"\"",
			"displayDateFormat=\"dd.MM.yyyy HH:mm:ss.fff\"",
			"transactionsMarkingType=\"Color\"",
			"transactionsMarkingType=\"Prompt\"",
			"transactionsMarkingType=\"Both\"",
			"allDirSearching=\"true\"",
			"allDirSearching=\"false\"",
			"searchingRegExTimeOutMSec=\"\"",
			"searchingRegExTimeOutMSec=\"1000\"",
			"parsingTraceRegExTimeOutMSec=\"\"",
			"parsingTraceRegExTimeOutMSec=\"1000\"",
			"encoding=\"\"",
			"encoding=\"windows-1251\"",
			"disableHintComments=\"true\"",
			"disableHintComments=\"false\"",
			"disableHintToolTip=\"true\"",
			"disableHintToolTip=\"false\"",
		};

		private readonly string[] declarationSnippets = {
			"<Servers>\n<Group name=\"\" priority=\"0\"></Group>\n</Servers>",
			"<FileTypes>\n<Group name=\"\" priority=\"0\"></Group>\n</FileTypes>",
			"<Group name=\"\" priority=\"0\"></Group>",
			"<LogsFolderGroup>\n<Folder allDirSearching=\"true\"></Folder>\n</LogsFolderGroup>",
			"<Folder allDirSearching=\"true\"></Folder>",
			"<MaxLines></MaxLines>", "<MaxLines>100</MaxLines>",
			"<MaxThreads></MaxThreads>", "<MaxThreads>-1</MaxThreads>",
			"<RowsLimit></RowsLimit>", "<RowsLimit>9999</RowsLimit>",
			"<OrderBy></OrderBy>", "<OrderBy>Date, Priority, File, FoundLineID</OrderBy>",
			"<TraceParse displayDateFormat=\"\" culture=\"\" transactionsMarkingType=\"\">\n</TraceParse>",
			"<TraceParse displayDateFormat=\"dd.MM.yyyy HH:mm:ss.fff\" culture=\"\" transactionsMarkingType=\"Color\">\n</TraceParse>",
			"<Pattern ID=\"\" Date=\"\" TraceName=\"\" Description=\"\" Message=\"\"><![CDATA[]]></Pattern>",
			"<Custom>\n<Assemblies>\n<Assembly>System.dll</Assembly>\n</Assemblies>\n<Namespaces>using System;\nusing LogsReader.Config;</Namespaces>\n<Code>\n<Function><![CDATA[\npublic class UserClass : ICustomTraceParse\n{\n\npublic bool IsLineMatch(string input)\n{\nthrow new NotImplementedException();\n}\n\npublic TraceParseResult IsTraceMatch(string input)\n{\nthrow new NotImplementedException();\n}\n\n}\n]]></Function>\n</Code>\n</Custom>",
			"<TransactionPattern Trn=\"\"><![CDATA[]]></TransactionPattern>",
			"<StartTraceLineWith><![CDATA[]]></StartTraceLineWith>",
			"<EndTraceLineWith><![CDATA[]]></EndTraceLineWith>",
			"<IsError><![CDATA[]]></IsError>",
		};

		private readonly string whiteSpaces = "\n" + new string(' ', 50);

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

			editor.Text = Serialize(SettingsOfScheme);
			editor.SizingGrip = false;
			editor.WordWrap = false;
			editor.SetLanguages(new[] { Language.XML, Language.CSharp }, Language.XML);
			editor.TextChanged += Editor_TextChanged;
			InitIntelliSense();

			KeyPreview = true;
			KeyDown += (sender, args) =>
			{
				switch (args.KeyCode)
				{
					case Keys.S when args.Control:
						ValidateOrOk_Click(ValidateOrOk, EventArgs.Empty);
						break;
				}
			};
		}

		private void InitIntelliSense()
		{
			var imageList = new ImageList { ImageSize = new Size(16, 16) };
			imageList.Images.Add("0", Resources.add);
			imageList.Images.Add("1", Resources.filtered);

			var items = new List<AutocompleteItem>();
			var popupMenu = new AutocompleteMenu(editor)
			{
				MinFragmentLength = 1,
				MinimumSize = new Size(300, 180),
				MaximumSize = new Size(400, 180),
				MaxTooltipSize = new Size(300, 180),
			};
			popupMenu.Items.ImageList = imageList;
			popupMenu.SearchPattern = @"[\w<]";
			popupMenu.AllowTabKey = true;

			foreach (var item in snippets)
				items.Add(new SnippetAutocompleteItem(item) { ImageIndex = 0 });
			foreach (var item in declarationSnippets)
				items.Add(new DeclarationSnippet(item) { ImageIndex = 1 });
			foreach (var item in keywords)
				items.Add(new AutocompleteItem(item));

			popupMenu.Items.SetAutocompleteItems(items);
		}

		class DeclarationSnippet : SnippetAutocompleteItem
		{
			public DeclarationSnippet(string snippet)
				: base(snippet)
			{
			}

			public override CompareResult Compare(string fragmentText)
			{
				if (fragmentText.TrimStart()[0] != '<')
					return CompareResult.Hidden;

				var pattern = Regex.Escape(fragmentText);
				var res = Regex.IsMatch(Text, pattern, RegexOptions.IgnoreCase) ? CompareResult.Visible : CompareResult.Hidden;
				return res;
			}
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
				if (LRSettingsScheme.TryDeserialize(editor.Text, out var result, out var exception))
				{
					if (result.Name != SchemeName)
					{
						MessageBox.Show(string.Format(Resources.TxtConfigureSchemeNameFailed, result.Name), @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					editor.Text = Serialize(result);
					_lastSuccessResult = result;
					ValidateOrOk.Text = Resources.TxtConfigureSuccess;
					ValidateOrOk.Image = Resources.finished;
					MessageBox.Show(Resources.TxtConfigureSuccessful, @"Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					var detail = string.Empty;
					if (exception != null)
						detail = $"\r\n\r\n{exception}";

					MessageBox.Show(string.Format(Resources.TxtConfigureFailed, SchemeName, detail), @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format(Resources.TxtConfigureFailed, SchemeName, string.Empty), @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void Reload_Click(object sender, EventArgs e)
		{
			editor.Text = Serialize(SettingsOfScheme);
			_lastSuccessResult = null;
			ValidateOrOk.Text = Resources.TxtConfigureSuccess;
			ValidateOrOk.Image = Resources.finished;
		}

		//whiteSpaces - чинит косяк, когда после вставки удаляется последний символ
		private string Serialize(LRSettingsScheme scheme) => scheme.Serialize() + whiteSpaces;
	}
}
