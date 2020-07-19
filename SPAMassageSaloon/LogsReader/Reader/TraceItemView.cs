﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using Utils;
using Utils.WinForm.Notepad;
using LogsReader.Config;

namespace LogsReader.Reader
{
	public partial class TraceItemView : UserControl
	{
		private DataTemplate currentTemplate;
		private DataTemplate prevTemplateMessage;
		private DataTemplate prevTemplateTraceMessage;

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
				if (!(this.Parent is TabPage page))
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

			descriptionText.AutoWordSelection = true;
			descriptionText.AutoWordSelection = false;

			notepad.TabsFont = LogsReaderMainForm.DgvFont;
			notepad.TextFont = LogsReaderMainForm.TxtFont;

			descriptionText.Font = new Font(LogsReaderMainForm.MainFontFamily, 9F, System.Drawing.FontStyle.Bold);

			EditorMessage = notepad.AddDocument(new BlankDocument { HeaderName = DataTemplate.HeaderMessage, Language = Language.XML });
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

		public void ChangeTemplate(DataTemplate template, bool showTransactionsInformation, out bool noChanged)
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
					var minus = prevTemplate.TransactionBindings.Except(CurrentTemplate.TransactionBindings); // разность последовательностей. Вычитаем лишнее.
					var plus = CurrentTemplate.TransactionBindings.Except(prevTemplate.TransactionBindings); // разность последовательностей. Добавляем недостоющее.
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
				if (showTransactionsInformation)
				{
					noChanged = SelectTransactions(CurrentTemplate.TransactionBindings);
				}
				else
				{
					noChanged = DeselectTransactions(CurrentTemplate.TransactionBindings);
				}
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

			if (showTransactionInformation)
				noChanged = SelectTransactions(CurrentTemplate.TransactionBindings);
			else
				noChanged = DeselectTransactions(CurrentTemplate.TransactionBindings);

			RefreshDescription(showTransactionInformation);
		}

		void RefreshDescription(bool showTrnsInformation)
		{
			descriptionText.Clear();

			descriptionText.AppendText($"{nameof(CurrentTemplate.FoundLineID)} = {CurrentTemplate.FoundLineID}", Color.Black);

			if (showTrnsInformation)
			{
				if (CurrentTemplate.Transactions.Any(x => !x.Value.Trn.IsNullOrEmptyTrim()))
				{
					descriptionText.AppendText("\r\nTransactions = \"", Color.Black);
					var i = 0;
					foreach (var (_, value) in CurrentTemplate.Transactions)
					{
						descriptionText.AppendText(value.Trn, value.FoundByTrn ? Color.Green : Color.Black);

						i++;
						if (CurrentTemplate.Transactions.Count > i)
							descriptionText.AppendText("\", \"", Color.Black);
					}

					descriptionText.AppendText("\"", Color.Black);
				}

				if (CurrentTemplate.ElapsedSecTotal >= 0)
					descriptionText.AppendText($"\r\n{CurrentTemplate.ElapsedSecDescription}");

			}

			if (!CurrentTemplate.Description.IsNullOrEmptyTrim())
			{
				descriptionText.AppendText($"\r\n{new string('-', 50)}\r\n");
				descriptionText.AppendText(CurrentTemplate.Description);
			}

			descriptionText.AutoWordSelection = true;
			descriptionText.AutoWordSelection = false;
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

		void SetMessage()
		{
			if (CurrentTemplate == null)
			{
				Clear();
				return;
			}

			if (notepad.CurrentEditor == EditorMessage)
			{
				if (prevTemplateMessage != null && prevTemplateMessage.Equals(CurrentTemplate))
					return;

				var messageString = CurrentTemplate.Message.TrimWhiteSpaces();
				if (EditorMessage.Language == Language.XML
				    || EditorMessage.Language == Language.HTML
				    || CurrentTemplate.Message.Length <= 50000 && messageString.StartsWith("<") && messageString.EndsWith(">"))
				{
					var messageXML = XML.RemoveUnallowable(CurrentTemplate.Message, " ");
					messageString = messageXML.IsXml(out var xmlDoc) ? xmlDoc.PrintXml() : messageXML.TrimWhiteSpaces();
				}

				EditorMessage.Text = messageString;
				EditorMessage.DelayedEventsInterval = 10;

				prevTemplateMessage = CurrentTemplate;
			}
			else if (notepad.CurrentEditor == EditorTraceMessage)
			{
				if (prevTemplateTraceMessage != null && prevTemplateTraceMessage.Equals(CurrentTemplate))
					return;

				EditorTraceMessage.Text = CurrentTemplate.TraceMessage;
				EditorTraceMessage.DelayedEventsInterval = 10;

				prevTemplateTraceMessage = CurrentTemplate;
			}
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

		static bool SelectTransactions(IEnumerable<DataTemplate> collection)
		{
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

		static bool DeselectTransactions(IEnumerable<DataTemplate> collection)
		{
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

		public void Clear()
		{
			descriptionText.Clear();

			EditorMessage?.Clear();
			EditorTraceMessage?.Clear();

			DeselectTransactions();

			CurrentTemplate = null;
			prevTemplateMessage = null;
			prevTemplateTraceMessage = null;
		}

		private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
		{
			SplitterMoved?.Invoke(this, EventArgs.Empty);
		}
	}
}