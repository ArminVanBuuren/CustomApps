using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace Utils.WinForm.Notepad
{
    public partial class Editor : UserControl, IDisposable
    {
        protected static readonly MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));
        private string _source = null;
        private bool _isDisposed = false;
        private Encoding _default = Encoding.Default;

        private readonly FastColoredTextBox FCTB;
        private readonly StatusStrip _statusStrip;

        readonly ToolStripLabel _contentLengthInfo;
        readonly ToolStripLabel _contentLinesInfo;
        readonly ToolStripLabel _currentLineInfo;
        readonly ToolStripLabel _currentPosition;
        readonly ToolStripLabel _selectedInfo;
        readonly ToolStripLabel _encodingInfo;
        readonly ToolStripComboBox _listOfLanguages;
        readonly CheckBox _wordWrapping;
        readonly CheckBox _highlights;

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

        public virtual Encoding Encoding { get; protected set; } = null;

        public Encoding DefaultEncoding
        {
            get => _default;
            set
            {
                _default = value;
                RefreshForm();
            }
        }

        public bool IsContentChanged => !Text.Equals(Source, StringComparison.Ordinal);

        public override string Text
        {
            get => FCTB.Text;
            set
            {
                try
                {
                    FCTB.Text = Source = value;
                    FCTB.ClearUndo();
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
            set => _wordWrapping.Checked = FCTB.WordWrap = value;
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
            set => base.Font = FCTB.Font = value;
        }

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
        public int LinesCount => FCTB.LinesCount;
        public Range Selection
        {
            get => FCTB.Selection;
            set => FCTB.Selection = value;
        }
        public string SelectedText => FCTB.SelectedText;
        public Language Language => FCTB.Language;

        protected new bool IsDisposed
        {
            get => base.IsDisposed && _isDisposed;
            private set => _isDisposed = value;
        }

        public Editor()
        {
            InitializeComponent();

            FCTB = new FastColoredTextBox
            {
                AutoCompleteBracketsList = new[] {'(', ')', '{', '}', '[', ']', '\"', '\"', '\'', '\''},
                AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);",
                AutoScrollMinSize = new Size(27, 14),
                BackBrush = null,
                CharHeight = 14,
                CharWidth = 8,
                DisabledColor = Color.FromArgb(100, 180, 180, 180),
                IsReplaceMode = false,
                Paddings = new Padding(0),
                ImeMode = ImeMode.Off,
                TabLength = 2,
                Zoom = 100,
                Dock = DockStyle.Fill
            };

            _statusStrip = new StatusStrip
            {
                Cursor = Cursors.Default,
                ForeColor = Color.Black,
                LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow
            };
            Controls.Add(FCTB);
            Controls.Add(_statusStrip);

            _listOfLanguages = new ToolStripComboBox { BackColor = SystemColors.Control, Padding = new Padding(0, 2, 0, 0)};
            foreach (Language lang in Enum.GetValues(typeof(Language)))
                _listOfLanguages.Items.Add(lang);
            _listOfLanguages.DropDownStyle = ComboBoxStyle.DropDownList;
            _statusStrip.Items.Add(_listOfLanguages);
            _listOfLanguages.SelectedIndexChanged += (sender, args) =>
            {
                if (_listOfLanguages.SelectedItem is Language lang && FCTB.Language != lang)
                {
                    bool isChanged = ChangeLanguage(lang);
                    if (isChanged)
                        LanguageChanged?.Invoke(this, EventArgs.Empty);
                }
            };

            _statusStrip.Items.Add(GetSeparator());
            _wordWrapping = new CheckBox { BackColor = Color.Transparent, Text = @"Wrap", Checked = WordWrap, Padding = new Padding(5, 3, 0, 0) };
            _wordWrapping.CheckStateChanged += (s, e) =>
            {
                if (WordWrap == _wordWrapping.Checked)
                    return;

                WordWrap = _wordWrapping.Checked;
                WordWrapStateChanged?.Invoke(this, EventArgs.Empty);
            };
            var wordWrapToolStrip = new ToolStripControlHost(_wordWrapping);
            _statusStrip.Items.Add(wordWrapToolStrip);

            _statusStrip.Items.Add(GetSeparator());
            _highlights = new CheckBox { BackColor = Color.Transparent, Text = @"Highlights", Checked = false, Padding = new Padding(5, 3, 0, 0) };
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
            _statusStrip.Items.Add(highlightsToolStrip);

            _statusStrip.Items.Add(GetSeparator());
            _encodingInfo = GetStripLabel("");
            _statusStrip.Items.Add(_encodingInfo);

            _statusStrip.Items.Add(GetSeparator());
            _statusStrip.Items.Add(GetStripLabel("length:"));
            _contentLengthInfo = GetStripLabel("");
            _statusStrip.Items.Add(_contentLengthInfo);
            _statusStrip.Items.Add(GetStripLabel("lines:"));
            _contentLinesInfo = GetStripLabel("");
            _statusStrip.Items.Add(_contentLinesInfo);

            _statusStrip.Items.Add(GetSeparator());
            _statusStrip.Items.Add(GetStripLabel("Ln:"));
            _currentLineInfo = GetStripLabel("");
            _statusStrip.Items.Add(_currentLineInfo);
            _statusStrip.Items.Add(GetStripLabel("Col:"));
            _currentPosition = GetStripLabel("");
            _statusStrip.Items.Add(_currentPosition);
            _statusStrip.Items.Add(GetStripLabel("Sel:"));
            _selectedInfo = GetStripLabel("");
            _statusStrip.Items.Add(_selectedInfo);

            FCTB.ClearStylesBuffer();
            FCTB.Range.ClearStyle(StyleIndex.All);
            FCTB.ClearUndo(); // если убрать метод то при Undo все вернется к пустоте а не к исходнику

            RefreshForm();

            FCTB.KeyPressed += (sender, args) => { KeyPressed?.Invoke(this, args); };
            FCTB.Pasting += (sender, args) => { Pasting?.Invoke(this, args); };
            FCTB.TextChangedDelayed += (sender, args) => { TextChangedDelayed?.Invoke(this, args); };
            FCTB.TextChanging += (sender, args) => { TextChanging?.Invoke(this, args); };
            FCTB.TextChanged += (sender, args) => { TextChangedChanged(this, args); TextChanged?.Invoke(this, args); };
            FCTB.SelectionChanged += (sender, args) => { RefreshForm(); SelectionChanged?.Invoke(this, args); };
            FCTB.SelectionChangedDelayed += (sender, args) => { SelectionChangedDelayed?.Invoke(this, args); };
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
            return new ToolStripSeparator() {Margin = new Padding(0, 2, 0, 0)};
        }

        static ToolStripLabel GetStripLabel(string text, int leftPadding = 0, int rightPadding = 0)
        {
            return new ToolStripStatusLabel(text)
            {
                BackColor = Color.Transparent,
                Margin = new Padding(0, 6, 0, 0),
                Padding = new Padding(0,0,0,0)
            };
        }

        internal void RefreshForm()
        {
            _contentLengthInfo.Text = TextLength.ToString();
            _contentLinesInfo.Text = LinesCount.ToString();
            _currentLineInfo.Text = (Selection.FromLine + 1).ToString();
            _currentPosition.Text = (Selection.FromX + 1).ToString();
            _selectedInfo.Text = $"{SelectedText.Length}|{(SelectedText.Length > 0 ? SelectedText.Split('\n').Length : 0)}";
            _encodingInfo.Text = Encoding?.HeaderName ?? DefaultEncoding.HeaderName;
            _listOfLanguages.Text = Language.ToString();
        }

        public void PrintXml()
        {
            try
            {
                if (FCTB.Language == Language.XML && Text.IsXml(out var document))
                    Text = document.PrintXml();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ClearUndo()
        {
            FCTB.ClearUndo();
        }

        public bool ChangeLanguage(Language language)
        {
            if (_listOfLanguages.Items.Cast<Language>().All(x => x != language))
                return false;

            bool isChanged = FCTB.Language != language;

            FCTB.ClearStylesBuffer();
            FCTB.Range.ClearStyle(StyleIndex.All);
            FCTB.Language = language;
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
