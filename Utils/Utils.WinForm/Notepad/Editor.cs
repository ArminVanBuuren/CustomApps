using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace Utils.WinForm.Notepad
{
	public partial class Editor : UserControl, IDisposable
    {
        protected static readonly MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));
        private string _source = null;
        private bool _isDisposed = false;
        private bool _coloredOnlyVisible = false;
        private Encoding _default = Encoding.Default;
        private Language _editorLanguage;

        private readonly FastColoredTextBox FCTB;
        private readonly StatusStrip _statusStrip;

        //private readonly Style XmlLiteAttributeStyle;
        //private readonly Style XmlLiteAttributeValueStyle;
        //private readonly Style XmlLiteCDataStyle;
        //private readonly Style XmlLiteTagNameStyle;
        private readonly Regex _xmlLiteCommentRegex;
        private readonly Regex _xmlLiteTagRegex;
        private readonly Regex _xmlLiteAttrRegex;
        private readonly Regex _xmlLiteAttrValRegex;

        private readonly ToolStripLabel _contentLengthInfo;
        private readonly ToolStripLabel _contentLinesInfo;
        private readonly ToolStripLabel _currentLineInfo;
        private readonly ToolStripLabel _currentPosition;
        private readonly ToolStripLabel _selectedInfo;
        private readonly ToolStripLabel _encodingInfo;
        private readonly ToolStripComboBox _listOfLanguages;
        private readonly CheckBox _wordWrapping;
        private readonly CheckBox _highlights;

        public event EventHandler KeyPressed;
        public event EventHandler Pasting;
        public event EventHandler TextChangedDelayed;
        public event EventHandler TextChanging;
        public new event EventHandler TextChanged;
        public event EventHandler LanguageChanged;
        public event EventHandler WordWrapStateChanged;
        public event EventHandler WordHighlightsStateChanged;
        public event EventHandler SelectionChanged;
        public event EventHandler SelectionChangedDelayed;

        public string HeaderName { get; set; }

        [Browsable(false)]
        public string Source
        {
            get => _source;
            protected set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // var indexOfDifference = Source.Zip(FCTB.Text, (c1, c2) => c1 == c2).TakeWhile(b => b).Count() + 1;
                    // реплейсим Tab на Spaces, потмоу что FCTB для выравнивание не поддерживает Tab. Поэтому сразу заменяем на Spaces, для корректного компаринга.
                    _source = value.Replace('\u0009'.ToString(), new string(' ', FCTB.TabLength));
                }
                else
                {
                    _source = value;
                }
            }
        }

        [Browsable(false)]
        public virtual Encoding Encoding { get; protected set; } = null;

        [Browsable(false)]
        public Encoding DefaultEncoding
        {
            get => _default;
            set
            {
                _default = value;
                RefreshForm();
            }
        }

        [Browsable(false)]
        public bool IsContentChanged => !Text.Equals(Source, StringComparison.Ordinal);

        private bool _autoPrintToXml = false;
        public bool AutoPrintToXml
        {
	        get => _autoPrintToXml;
	        set
	        {
		        _autoPrintToXml = value;
		        if (_autoPrintToXml)
			        ChageStyleIfLargeLine();
	        }
        }

        public override string Text
        {
            get => FCTB.Text;
            set
            {
                try
                {
	                FCTB.Clear();
	                Source = value;
	                ChageStyleIfLargeLine();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public bool SizingGrip
        {
            get => _statusStrip.SizingGrip;
            set => _statusStrip.SizingGrip = value;
        }

        public bool WordWrap
        {
	        get => FCTB.WordWrap;
	        set
	        {
		        if (_wordWrapping.Checked == value && FCTB.WordWrap == value)
			        return;

		        _wordWrapping.Checked = FCTB.WordWrap = value;
		        WordWrapStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Highlights
        {
            get => _highlights.Checked;
            set
            {
                if (_highlights.Checked == value)
                    return;

                _highlights.Checked = value;
            }
        }

        public bool ReadOnly
        {
            get => FCTB.ReadOnly;
            set => FCTB.ReadOnly = value;
        }

        public override Font Font
        {
            get => FCTB.Font;
            set => FCTB.Font = value;
        }

        [Browsable(false)]
        public bool IsChanged
        {
            get => FCTB.IsChanged;
            set => FCTB.IsChanged = value;
        }

        public int DelayedEventsInterval
        {
            get => FCTB.DelayedEventsInterval;
            set => FCTB.DelayedEventsInterval = value;
        }

        [Browsable(false)]
        public Brush BackBrush
        {
            get => FCTB.BackBrush;
            set => FCTB.BackBrush = value;
        }

        public new Cursor Cursor
        {
            get => FCTB.Cursor;
            set => FCTB.Cursor = value;
        }

        public Color DisabledColor
        {
            get => FCTB.DisabledColor;
            set => FCTB.DisabledColor = value;
        }

        [Browsable(false)]
        public bool IsReplaceMode
        {
            get => FCTB.IsReplaceMode;
            set => FCTB.IsReplaceMode = value;
        }

        public Color SelectionColor
        {
            get => FCTB.SelectionColor;
            set => FCTB.SelectionColor = value;
        }

        public int TextLength => FCTB.TextLength;

        [Browsable(false)]
        public int LinesCount => FCTB.LinesCount;

        [Browsable(false)]
        public Range Selection
        {
            get => FCTB.Selection;
            set => FCTB.Selection = value;
        }

        [Browsable(false)]
        public string SelectedText => FCTB.SelectedText;

        public Language Language
        {
	        get => _editorLanguage;
	        private set => _editorLanguage = FCTB.Language = value;
        }

        public bool ColoredOnlyVisible
        {
            get => _coloredOnlyVisible;
            set
            {
                if(_coloredOnlyVisible == value)
                    return;

                _coloredOnlyVisible = value;
                FCTB.ClearStylesBuffer();
                FCTB.Range.ClearStyle(StyleIndex.All);
                if (_coloredOnlyVisible)
                {
                    FCTB.OnSyntaxHighlight(new TextChangedEventArgs(FCTB.VisibleRange));
                    FCTB.VisibleRangeChangedDelayed += FctbOnVisibleRangeChangedDelayed;
                }
                else
                {
                    FCTB.VisibleRangeChangedDelayed -= FctbOnVisibleRangeChangedDelayed;
                    FCTB.OnSyntaxHighlight(new TextChangedEventArgs(FCTB.Range));
                }
            }
        }

        [Browsable(false)]
        protected new bool IsDisposed
        {
            get => base.IsDisposed && _isDisposed;
            private set => _isDisposed = value;
        }

        public Editor()
        {
            InitializeComponent();
            FCTB = new FastColoredTextBox();
            _statusStrip = new StatusStrip();

            try
            {
                //this.SuspendLayout();
                //FCTB.SuspendLayout();
                //_statusStrip.SuspendLayout();

                BorderStyle = BorderStyle.None;

                FCTB.AutoCompleteBracketsList = new[] { '(', ')', '{', '}', '[', ']', '\"', '\"', '\'', '\'' };
                FCTB.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);";
                FCTB.AutoScrollMinSize = new Size(27, 14);
                FCTB.BackBrush = null;
                FCTB.CharHeight = 14;
                FCTB.CharWidth = 8;
                FCTB.DisabledColor = Color.FromArgb(100, 180, 180, 180);
                FCTB.IsReplaceMode = false;
                FCTB.Paddings = new Padding(0);
                FCTB.ImeMode = ImeMode.Off;
                FCTB.TabLength = 2;
                FCTB.Zoom = 100;
                FCTB.Dock = DockStyle.Fill;
                FCTB.BorderStyle = BorderStyle.None;
                FCTB.MinimumSize = new Size(100, 0);
                _editorLanguage = FCTB.Language;

                _statusStrip.Cursor = Cursors.Default;
                _statusStrip.ForeColor = Color.Black;
                _statusStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
                _statusStrip.Padding = new Padding(0, 2, 0, 0);
                _statusStrip.MinimumSize = new Size(710, 0);

                //XmlLiteAttributeStyle = new TextStyle(((TextStyle)FCTB.SyntaxHighlighter.RedStyle).ForeBrush, null, ((TextStyle)FCTB.SyntaxHighlighter.RedStyle).FontStyle);
                //XmlLiteAttributeValueStyle = new TextStyle(((TextStyle)FCTB.SyntaxHighlighter.BlueStyle).ForeBrush, null, ((TextStyle)FCTB.SyntaxHighlighter.BlueStyle).FontStyle);
                //XmlLiteCDataStyle = new TextStyle(((TextStyle)FCTB.SyntaxHighlighter.BlackStyle).ForeBrush, null, ((TextStyle)FCTB.SyntaxHighlighter.BlackStyle).FontStyle);
                //XmlLiteTagNameStyle = new TextStyle(((TextStyle)FCTB.SyntaxHighlighter.MaroonStyle).ForeBrush, null, ((TextStyle)FCTB.SyntaxHighlighter.MaroonStyle).FontStyle);
                _xmlLiteCommentRegex = new Regex(@"<!--.*?-->|<!\s*\[CDATA\s*\[(?<text>(?>[^]]+|](?!]>))*)]]>", RegexOptions.Singleline | RegexOptions.Compiled);
                _xmlLiteTagRegex = new Regex(@"(?<range><[!\w:]+)|(?<range></[\w:]+>)|<\?|<|/>|</|>|\?>", RegexOptions.Compiled);
                _xmlLiteAttrRegex = new Regex(@"(?<range>\w+\=)", RegexOptions.Compiled);
                _xmlLiteAttrValRegex = new Regex(@"\=\""(?<range>.*?)\""", RegexOptions.Compiled);

                var worker1 = new System.ComponentModel.BackgroundWorker();
                worker1.DoWork += (sender, e) => this.SafeInvoke(() => { Controls.AddRange(new[] { (Control)FCTB, _statusStrip }); });
                worker1.RunWorkerAsync();

                var toolStripItems = new List<ToolStripItem>();

                _listOfLanguages = new ToolStripComboBox { BackColor = SystemColors.Control };
                foreach (Language lang in Enum.GetValues(typeof(Language)))
                    _listOfLanguages.Items.Add(lang);
                _listOfLanguages.DropDownStyle = ComboBoxStyle.DropDownList;
                _listOfLanguages.Size = new Size(100, _listOfLanguages.Size.Height);
                toolStripItems.Add(_listOfLanguages);
                _listOfLanguages.SelectedIndexChanged += (sender, args) =>
                {
                    if (_listOfLanguages.SelectedItem is Language lang && Language != lang)
                    {
                        bool isChanged = ChangeLanguage(lang);
                        if (isChanged)
                            LanguageChanged?.Invoke(this, EventArgs.Empty);
                    }
                };

                toolStripItems.Add(GetSeparator());
                _wordWrapping = new CheckBox { BackColor = Color.Transparent, Text = @"Wrap", Checked = WordWrap, Padding = new Padding(5, 2, 0, 0) };
                _wordWrapping.CheckStateChanged += (s, e) =>
                {
                    if (WordWrap == _wordWrapping.Checked)
                        return;

                    WordWrap = _wordWrapping.Checked;
                };
                var wordWrapToolStrip = new ToolStripControlHost(_wordWrapping);
                toolStripItems.Add(wordWrapToolStrip);

                toolStripItems.Add(GetSeparator());
                _highlights = new CheckBox { BackColor = Color.Transparent, Text = @"Highlights", Checked = false, Padding = new Padding(5, 2, 0, 0) };
                _highlights.CheckStateChanged += (s, e) =>
                {
                    FCTB.VisibleRange.ClearStyle(SameWordsStyle);
                    if (Highlights)
                        FCTB.SelectionChangedDelayed += FCTB_SelectionChangedDelayed;
                    else
                        FCTB.SelectionChangedDelayed -= FCTB_SelectionChangedDelayed;
                    WordHighlightsStateChanged?.Invoke(this, EventArgs.Empty);
                };
                var highlightsToolStrip = new ToolStripControlHost(_highlights);
                toolStripItems.Add(highlightsToolStrip);

                toolStripItems.Add(GetSeparator());
                _encodingInfo = GetStripLabel("");
                toolStripItems.Add(_encodingInfo);

                toolStripItems.Add(GetSeparator());
                toolStripItems.Add(GetStripLabel("length:"));
                _contentLengthInfo = GetStripLabel("");
                toolStripItems.Add(_contentLengthInfo);
                toolStripItems.Add(GetStripLabel("lines:"));
                _contentLinesInfo = GetStripLabel("");
                toolStripItems.Add(_contentLinesInfo);

                toolStripItems.Add(GetSeparator());
                toolStripItems.Add(GetStripLabel("Ln:"));
                _currentLineInfo = GetStripLabel("");
                toolStripItems.Add(_currentLineInfo);
                toolStripItems.Add(GetStripLabel("Col:"));
                _currentPosition = GetStripLabel("");
                toolStripItems.Add(_currentPosition);
                toolStripItems.Add(GetStripLabel("Sel:"));
                _selectedInfo = GetStripLabel("");
                toolStripItems.Add(_selectedInfo);

                var worker2 = new System.ComponentModel.BackgroundWorker();
                worker2.DoWork += (sender, e) => _statusStrip.SafeInvoke(() => { _statusStrip.Items.AddRange(toolStripItems.ToArray()); });
                worker2.RunWorkerAsync();

                FCTB.ClearStylesBuffer();
                FCTB.Range.ClearStyle(StyleIndex.All);
                FCTB.ClearUndo(); // если убрать метод то при Undo все вернется к пустоте а не к исходнику

                FCTB.TextChanged += FCTB_TextChanged;
                FCTB.KeyPressed += (sender, args) => { KeyPressed?.Invoke(this, args); };
                FCTB.Pasting += (sender, args) => { Pasting?.Invoke(this, args); };
                FCTB.TextChangedDelayed += (sender, args) => { TextChangedDelayed?.Invoke(this, args); };
                FCTB.TextChanging += (sender, args) => { TextChanging?.Invoke(this, args); };
                FCTB.SelectionChanged += (sender, args) => { RefreshForm(); SelectionChanged?.Invoke(this, args); };
                FCTB.SelectionChangedDelayed += (sender, args) => { SelectionChangedDelayed?.Invoke(this, args); };
            }
            finally
            {
	            //_statusStrip.ResumeLayout();
	            //FCTB.ResumeLayout();
                //this.ResumeLayout();
            }
        }

        /// <summary>
        /// костыль. Если строка очень длинная то меняем язык на простой, иначе зависает все нахуй текст врапован
        /// </summary>
        void ChageStyleIfLargeLine()
        {
	        bool? isXml = null;
	        void TryConvertToXml()
	        {
		        var messageXML = XML.RemoveUnallowable(Source, @" ");
		        isXml = messageXML.IsXml(out var xmlDoc);
		        Source = isXml.Value ? xmlDoc.PrintXml() : messageXML.Trim();
	        }

	        if (AutoPrintToXml && !Source.IsNullOrEmpty())
	        {
		        if (Language == Language.XML)
		        {
			        TryConvertToXml();
		        }
		        else
		        {
			        var trimWhiteSpace = Source.Trim();
			        if (trimWhiteSpace.StartsWith(@"<") && trimWhiteSpace.EndsWith(@">"))
				        TryConvertToXml();
		        }
	        }

	        var satisfied = IsLanguageSatisfied(Source, isXml);

	        if (FCTB.Text == Source && !satisfied)
		        FCTB.Text += "\0";
	        else
		        FCTB.Text = Source;

	        FCTB.IsChanged = true;
	        FCTB.ClearUndo();
        }

        bool IsLanguageSatisfied(string input, bool? isXml)
        {
	        if (!input.IsNullOrEmpty() && (input.Split('\n').Any(x => x.Length > 1200)
	                                       || (Language == Language.XML && (isXml != null && !isXml.Value || isXml == null && !input.IsXml(out _)))))
	        {
		        if (FCTB.Language != Language.Custom)
			        FCTB.Language = Language.Custom;
	        }
	        else if (FCTB.Language != Language)
	        {
		        FCTB.IsChanged = true;
		        FCTB.ClearStylesBuffer();
		        FCTB.Range.ClearStyle(StyleIndex.All);
		        FCTB.Language = Language;
		        FCTB.SyntaxHighlighter.HighlightSyntax(FCTB.Language, FCTB.Range);
	        }

	        return FCTB.Language == Language;
        }

        private void FCTB_TextChanged(object sender, TextChangedEventArgs e)
        {
	        if (!FCTB.Text.IsNullOrEmpty() && !IsLanguageSatisfied(FCTB.Text, null))
                if((Language == Language.XML || Language == Language.HTML) && !FCTB.Text.Split('\n').Any(x => x.Length > 13000))
                    if(Language == Language.XML)
						XmlLiteSyntaxHighlight(e);
					else
	                    HtmlLiteSyntaxHighlight(e);

            TextChangedChanged(this, e);
        }

        private void XmlLiteSyntaxHighlight(TextChangedEventArgs e)
        {
	        e.ChangedRange.ClearStyle(
		        FCTB.SyntaxHighlighter.XmlAttributeStyle,
		        FCTB.SyntaxHighlighter.XmlAttributeValueStyle,
		        FCTB.SyntaxHighlighter.XmlCDataStyle,
		        FCTB.SyntaxHighlighter.XmlTagNameStyle);

	        e.ChangedRange.SetStyle(FCTB.SyntaxHighlighter.XmlCDataStyle, _xmlLiteCommentRegex);
	        e.ChangedRange.SetStyle(FCTB.SyntaxHighlighter.XmlAttributeStyle, _xmlLiteAttrRegex);
	        e.ChangedRange.SetStyle(FCTB.SyntaxHighlighter.XmlAttributeValueStyle, _xmlLiteAttrValRegex);
	        e.ChangedRange.SetStyle(FCTB.SyntaxHighlighter.XmlTagNameStyle, _xmlLiteTagRegex);

	        e.ChangedRange.ClearFoldingMarkers();
        }

        private void HtmlLiteSyntaxHighlight(TextChangedEventArgs e)
        {
	        e.ChangedRange.ClearStyle(
		        FCTB.SyntaxHighlighter.XmlCDataStyle,
		        FCTB.SyntaxHighlighter.AttributeStyle,
		        FCTB.SyntaxHighlighter.AttributeValueStyle,
		        FCTB.SyntaxHighlighter.TagNameStyle);

	        e.ChangedRange.SetStyle(FCTB.SyntaxHighlighter.XmlCDataStyle, _xmlLiteCommentRegex);
	        e.ChangedRange.SetStyle(FCTB.SyntaxHighlighter.AttributeStyle, _xmlLiteAttrRegex);
	        e.ChangedRange.SetStyle(FCTB.SyntaxHighlighter.AttributeValueStyle, _xmlLiteAttrValRegex);
	        e.ChangedRange.SetStyle(FCTB.SyntaxHighlighter.TagNameStyle, _xmlLiteTagRegex);

	        e.ChangedRange.ClearFoldingMarkers();
        }

        private void FctbOnVisibleRangeChangedDelayed(object sender, EventArgs e)
        {
            FCTB.VisibleRange.ClearStyle(StyleIndex.All);
            FCTB.OnSyntaxHighlight(new TextChangedEventArgs(FCTB.VisibleRange));
        }

        public ToolStripLabel AddToolStripLabel(string text = "")
        {
            _statusStrip.Items.Add(GetSeparator());
            var lable = GetStripLabel(text);
            _statusStrip.Items.Add(lable);
            return lable;
        }

        static ToolStripSeparator GetSeparator()
        {
            return new ToolStripSeparator() {Margin = new Padding(0, 1, 0, 0)};
        }

        static ToolStripLabel GetStripLabel(string text, int leftPadding = 0, int rightPadding = 0)
        {
            return new ToolStripStatusLabel(text)
            {
                BackColor = Color.Transparent,
                Margin = new Padding(0, 5, 0, 0),
                Padding = new Padding(0,0,0,0)
            };
        }

        internal void RefreshForm()
        {
	        try
	        {
		        _statusStrip.SuspendLayout();

		        var textLength = TextLength.ToString();
		        var linesCount = LinesCount.ToString();
		        var selectionLine = (Selection.FromLine + 1).ToString();
		        var selectionX = (Selection.FromX + 1).ToString();
		        var selectedText = $"{SelectedText.Length}|{(SelectedText.Length > 0 ? SelectedText.Split('\n').Length : 0)}";
		        var encoding = Encoding?.HeaderName ?? DefaultEncoding.HeaderName;
		        var language = Language.ToString();

		        if (_contentLengthInfo.Text != textLength)
			        _contentLengthInfo.Text = textLength;

		        if (_contentLinesInfo.Text != linesCount)
			        _contentLinesInfo.Text = linesCount;

		        if (_currentLineInfo.Text != selectionLine)
			        _currentLineInfo.Text = selectionLine;

		        if (_currentPosition.Text != selectionX)
			        _currentPosition.Text = selectionX;

		        if (_selectedInfo.Text != selectedText)
			        _selectedInfo.Text = selectedText;

		        if (_encodingInfo.Text != encoding)
			        _encodingInfo.Text = encoding;

		        if (_listOfLanguages.Text != language)
			        _listOfLanguages.Text = language;
            }
	        finally
	        {
		        _statusStrip.ResumeLayout();
            }
        }

        public void PrintXml(bool commitChanges = true)
        {
            try
            {
                if (Language == Language.XML && Text.IsXml(out var document))
                {
                    var prettyPrintedXml = document.PrintXml();
                    if (commitChanges)
                        Text = document.PrintXml();
                    else
                        ChangeTextWithoutCommit(prettyPrintedXml);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ChangeTextWithoutCommit(string text)
        {
            FCTB.Text = text;
        }

        public void ClearUndo()
        {
            FCTB.ClearUndo();
        }

        public bool ChangeLanguage(Language language)
        {
            if (_listOfLanguages.Items.Cast<Language>().All(x => x != language))
                return false;

            bool isChanged = Language != language;

            FCTB.ClearStylesBuffer();
            FCTB.Range.ClearStyle(StyleIndex.All);
            Language = language;

            ChageStyleIfLargeLine();

            if (ColoredOnlyVisible)
                FCTB.OnSyntaxHighlight(new TextChangedEventArgs(FCTB.VisibleRange));
            else
                FCTB.OnSyntaxHighlight(new TextChangedEventArgs(FCTB.Range));

            if (isChanged)
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            return isChanged;
        }

        public void SetLanguages(IEnumerable<Language> list, Language @default)
        {
            if(!list.Any())
                throw new Exception("No languages found");
            if (list.All(x => x != @default))
                throw new Exception("The default language must be in language list");

            _listOfLanguages.Items.Clear();
            foreach (Language lang in list.GroupBy(p => p).Select(x => x.Key))
                _listOfLanguages.Items.Add(lang);
            _listOfLanguages.SelectedItem = @default;
            ChangeLanguage(@default);
        }

        private void FCTB_SelectionChangedDelayed(object sender, EventArgs e)
        {
            FCTB.VisibleRange.ClearStyle(SameWordsStyle);
            if (!FCTB.Selection.IsEmpty)
                return; //user selected diapason

            //get fragment around caret
            var fragment = FCTB.Selection.GetFragment(@"\w");
            var text = fragment.Text;
            if (text.Length == 0)
                return;

            //highlight same words
            var ranges = FCTB.VisibleRange.GetRanges("\\b" + text + "\\b").ToArray();
            if (ranges.Length > 1)
                foreach (var r in ranges)
                    r.SetStyle(SameWordsStyle);
        }

        protected virtual void TextChangedChanged(Editor editor, TextChangedEventArgs args)
        {
	        TextChanged?.Invoke(this, args);
        }

        public void DoSelectionVisible()
        {
            FCTB.DoSelectionVisible();
        }

        /// <summary>
        /// Get range of text
        /// </summary>
        /// <param name="fromPos">Absolute start position</param>
        /// <param name="toPos">Absolute finish position</param>
        /// <returns>Range</returns>
        public Range GetRange(int fromPos, int toPos)
        {
            return FCTB.GetRange(fromPos, toPos);
        }

        /// <summary>
        /// Get range of text
        /// </summary>
        /// <param name="fromPlace">Line and char position</param>
        /// <param name="toPlace">Line and char position</param>
        /// <returns>Range</returns>
        public Range GetRange(Place fromPlace, Place toPlace)
        {
            return FCTB.GetRange(fromPlace, toPlace);
        }

        public void Clear()
        {
	        FCTB.Clear();
        }

        public new void Focus()
        {
            FCTB.Focus();
        }

        public new virtual void Dispose()
        {
	        this.RemoveAllEvents();
            base.Dispose();
            IsDisposed = true;
        }
    }
}
