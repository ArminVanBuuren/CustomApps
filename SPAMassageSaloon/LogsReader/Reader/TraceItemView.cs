using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using LogsReader.Config;
using Utils;
using Utils.WinForm;
using Utils.WinForm.Notepad;

namespace LogsReader.Reader
{
	public partial class TraceItemView : UserControl
	{
		private DataTemplate currentTemplate;
		private DataTemplate prevTemplateMessage;
		private DataTemplate prevTemplateTraceMessage;

		private bool _isInited;

		protected UserSettings UserSettings { get; }

		public event EventHandler SplitterMoved;

		public Editor EditorMessage { get; }

		public Editor EditorTraceMessage { get; }

		public DataTemplate CurrentTemplate
		{
			get => currentTemplate;
			set
			{
				currentTemplate = value;
				if (!(Parent is TabPage page))
					return;

				if (currentTemplate != null)
					page.Text = IsMain ? $"({currentTemplate.ID})" : $"({currentTemplate.ID}) {currentTemplate.TraceName}";
				else
					page.Text = string.Empty;
			}
		}

		public Editor CurrentEditor => notepad.CurrentEditor;

		public int SplitterDistance
		{
			get => splitContainer.SplitterDistance;
			set => splitContainer.SplitterDistance = value;
		}

		public bool IsMain { get; }

		public TraceItemView(Encoding defaultEncoding, UserSettings userSettings, bool isMain)
		{
			UserSettings = userSettings;
			IsMain = isMain;
			InitializeComponent();

			try
			{
				notepad.SuspendLayout();
				notepad.TabsFont = LogsReaderMainForm.DgvDataFont;
				notepad.TextFont = LogsReaderMainForm.TxtFont;
				
				descriptionText.AutoWordSelection = true;
				descriptionText.AutoWordSelection = false;
				descriptionText.Font = new Font(LogsReaderMainForm.MainFontFamily, 9F, FontStyle.Bold);
				
				EditorMessage = notepad.AddDocument(new BlankDocument
				{
					HeaderName = DataTemplate.HeaderMessage,
					Language = Language.XML
				});
				EditorMessage.AutoPrintToXml = true;
				EditorMessage.BackBrush = null;
				EditorMessage.BorderStyle = BorderStyle.FixedSingle;
				EditorMessage.Cursor = Cursors.IBeam;
				EditorMessage.DelayedEventsInterval = 1000;
				EditorMessage.DisabledColor = Color.FromArgb(100, 171, 171, 171);
				EditorMessage.IsReplaceMode = false;
				EditorMessage.SelectionColor = Color.FromArgb(50, 0, 0, 255);
				EditorTraceMessage = notepad.AddDocument(new BlankDocument { HeaderName = DataTemplate.HeaderTraceMessage });
				EditorTraceMessage.BackBrush = null;
				EditorTraceMessage.BorderStyle = BorderStyle.FixedSingle;
				EditorTraceMessage.Cursor = Cursors.IBeam;
				EditorTraceMessage.DelayedEventsInterval = 1000;
				EditorTraceMessage.DisabledColor = Color.FromArgb(100, 171, 171, 171);
				EditorTraceMessage.IsReplaceMode = false;
				EditorTraceMessage.SelectionColor = Color.FromArgb(50, 0, 0, 255);
				
				EditorMessage.WordWrap = UserSettings.MessageWordWrap;
				EditorMessage.Highlights = UserSettings.MessageHighlights;
				EditorTraceMessage.WordWrap = UserSettings.TraceWordWrap;
				EditorTraceMessage.Highlights = UserSettings.TraceHighlights;
				
				notepad.SelectEditor(0);
				notepad.DefaultEncoding = defaultEncoding;
				
				var langMessage = UserSettings.MessageLanguage;
				var langTrace = UserSettings.TraceLanguage;
				if (EditorMessage.Language != langMessage)
					EditorMessage.ChangeLanguage(langMessage);
				if (EditorTraceMessage.Language != langTrace)
					EditorTraceMessage.ChangeLanguage(langTrace);

				if (isMain)
				{
					EditorMessage.LanguageChanged += (sender, args) => { UserSettings.MessageLanguage = EditorMessage.Language; };
					EditorTraceMessage.LanguageChanged += (sender, args) => { UserSettings.TraceLanguage = EditorTraceMessage.Language; };
					
					notepad.WordWrapStateChanged += (sender, args) =>
					{
						if (!(sender is Editor editor))
							return;

						if (editor == EditorMessage)
							UserSettings.MessageWordWrap = editor.WordWrap;
						else if (editor == EditorTraceMessage)
							UserSettings.TraceWordWrap = editor.WordWrap;
					};
					notepad.WordHighlightsStateChanged += (sender, args) =>
					{
						if (!(sender is Editor editor))
							return;

						if (editor == EditorMessage)
							UserSettings.MessageHighlights = editor.Highlights;
						else if (editor == EditorTraceMessage)
							UserSettings.TraceHighlights = editor.Highlights;
					};
				}

				notepad.SelectedIndexChanged += Notepad_TabIndexChanged;
			}
			finally
			{
				notepad.ResumeLayout();
			}
		}

		public void SetTemplate(DataTemplate template, bool showTransactionsInformation) 
			=> SetTemplate(template, showTransactionsInformation, out var _);

		public void SetTemplate(DataTemplate template, bool showTransactionsInformation, out bool noChanged)
		{
			noChanged = true;
			var prevTemplate = CurrentTemplate;
			CurrentTemplate = template;

			if (CurrentTemplate == null)
			{
				if (prevTemplate != null)
					noChanged = DeselectTransactions(prevTemplate.TransactionBindings);

				Clear();
				return;
			}

			if (prevTemplate != null)
			{
				if (showTransactionsInformation)
				{
					// разность последовательностей. Вычитаем лишнее.
					var minus = prevTemplate.TransactionBindings.Except(CurrentTemplate.TransactionBindings);
					// разность последовательностей. Добавляем недостоющее.
					var plus = CurrentTemplate.TransactionBindings.Except(prevTemplate.TransactionBindings);
					var noChangedMinus = DeselectTransactions(minus);
					var noChangedPlus = SelectTransactions(plus);
					noChanged = noChangedMinus && noChangedPlus;
				}
				else
				{
					noChanged = DeselectTransactions(prevTemplate.TransactionBindings);
				}
			}
			else
			{
				noChanged = showTransactionsInformation 
					? SelectTransactions(CurrentTemplate.TransactionBindings)
					: DeselectTransactions(CurrentTemplate.TransactionBindings);
			}

			RefreshDescription(showTransactionsInformation);
			SetMessage();
		}

		public void RefreshDescription(bool showTransactionInformation, out bool noChanged)
		{
			noChanged = true;

			if (CurrentTemplate == null)
			{
				Clear();
				return;
			}

			noChanged = showTransactionInformation 
				? SelectTransactions(CurrentTemplate.TransactionBindings) 
				: DeselectTransactions(CurrentTemplate.TransactionBindings);

			RefreshDescription(showTransactionInformation);
		}

		private void RefreshDescription(bool showTrnsInformation)
		{
			try
			{
				descriptionText.SuspendLayout();
				descriptionText.Clear();
				descriptionText.AppendText($"{nameof(CurrentTemplate.FoundLineID)} = {CurrentTemplate.FoundLineID}");

				if (showTrnsInformation)
				{
					if (CurrentTemplate.Transactions.Any(x => !x.Value.Trn.IsNullOrWhiteSpace()))
					{
						descriptionText.AppendText("\r\nTransactions = \"");
						var i = 0;

						foreach (var (_, value) in CurrentTemplate.Transactions)
						{
							if (value.FoundByTrn)
								descriptionText.AppendText(value.Trn, Color.Green);
							else
								descriptionText.AppendText(value.Trn);
							i++;
							if (CurrentTemplate.Transactions.Count > i)
								descriptionText.AppendText("\", \"");
						}

						descriptionText.AppendText("\"");
					}

					if (CurrentTemplate.ElapsedSecTotal >= 0)
						descriptionText.AppendText($"\r\n{CurrentTemplate.ElapsedSecDescription}");
				}

				if (!CurrentTemplate.Description.IsNullOrWhiteSpace())
				{
					descriptionText.AppendText($"\r\n{new string('-', 50)}\r\n");
					descriptionText.AppendText(CurrentTemplate.Description);
				}

				descriptionText.AutoWordSelection = true;
				descriptionText.AutoWordSelection = false;
			}
			finally
			{
				descriptionText.ResumeLayout();
			}
		}

		private void Notepad_TabIndexChanged(object sender, EventArgs e)
		{
			try
			{
				SetMessage();
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void SetMessage()
		{
			if (CurrentTemplate == null)
			{
				Clear();
				return;
			}

			RefreshView();

			if (notepad.CurrentEditor == EditorMessage)
			{
				// так сделано для лучшей производительности
				if (prevTemplateMessage != null && prevTemplateMessage.Equals(CurrentTemplate))
					return;

				EditorMessage.Text = CurrentTemplate.Message.Trim();
				EditorMessage.DelayedEventsInterval = 10;
				prevTemplateMessage = CurrentTemplate;
			}
			else if (notepad.CurrentEditor == EditorTraceMessage)
			{
				// так сделано для лучшей производительности
				if (prevTemplateTraceMessage != null && prevTemplateTraceMessage.Equals(CurrentTemplate))
					return;

				EditorTraceMessage.Text = CurrentTemplate.TraceMessage;
				EditorTraceMessage.DelayedEventsInterval = 10;
				prevTemplateTraceMessage = CurrentTemplate;
			}
		}

		/// <summary>
		/// чинит баг когда текст не обновляется
		/// </summary>
		public void RefreshView()
		{
			if (_isInited)
				return;

			_isInited = true;
			notepad.SelectEditor(0);
		}

		public void SelectTransactions()
		{
			if (CurrentTemplate != null)
				SelectTransactions(CurrentTemplate.TransactionBindings);
		}

		public void DeselectTransactions()
		{
			if (CurrentTemplate != null)
				DeselectTransactions(CurrentTemplate.TransactionBindings);
		}

		private bool SelectTransactions(IEnumerable<DataTemplate> collection)
		{
			if (!IsMain)
				return true;

			var noChanged = true;

			foreach (var bindTrnTemplate in collection)
			{
				if (bindTrnTemplate.IsSelected)
					continue;

				bindTrnTemplate.IsSelected = true;
				noChanged = false;
			}

			return noChanged;
		}

		private bool DeselectTransactions(IEnumerable<DataTemplate> collection)
		{
			if (!IsMain)
				return true;

			var noChanged = true;

			foreach (var bindTrnTemplate in collection)
			{
				if (!bindTrnTemplate.IsSelected)
					continue;

				bindTrnTemplate.IsSelected = false;
				noChanged = false;
			}

			return noChanged;
		}

		public void Clear(bool deselectTransactions = true)
		{
			descriptionText.Clear();
			EditorMessage?.Clear();
			EditorTraceMessage?.Clear();

			if(deselectTransactions)
				DeselectTransactions();

			CurrentTemplate = null;
			prevTemplateMessage = null;
			prevTemplateTraceMessage = null;
		}

		private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e) => SplitterMoved?.Invoke(this, EventArgs.Empty);
	}
}