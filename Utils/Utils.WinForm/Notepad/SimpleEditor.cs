using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace Utils.WinForm.Notepad
{
    public class SimpleEditor : UserControl
    {
        protected static readonly MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));
        private bool _validateEditorOnChange = false;
        private string _source = null;
        private bool isDisposed = false;
        private Encoding _default = Encoding.Default;

        private readonly FastColoredTextBox FCTB;
        private readonly StatusStrip statusStrip;

        readonly ToolStripLabel _contentLengthInfo;
        readonly ToolStripLabel _contentLinesInfo;
        readonly ToolStripLabel _currentLineInfo;
        readonly ToolStripLabel _currentPosition;
        readonly ToolStripLabel _selectedInfo;
        readonly ToolStripLabel _encodingInfo;
        readonly ToolStripComboBox _listOfLanguages;
        readonly CheckBox _wordWrapping;
        readonly CheckBox _highlights;


        public event EventHandler OnSomethingChanged;
        public event EventHandler SelectionChanged;
        public event EventHandler LanguageChanged;
        public event EventHandler WordWrapStateChanged;
        public event EventHandler WordHighlightsStateChanged;

        public TabPage Page { get; protected set; }

        public string HeaderName { get; protected set; }

        public string Source
        {
            get => _source;
            protected set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // var indexOfDifference = Source.Zip(FCTB.Text, (c1, c2) => c1 == c2).TakeWhile(b => b).Count() + 1;
                    // реплейсим Tab на Spaces, потмоу что FCTB для выравнивание не поддерживает Tab. Поэтому сразу заменяем на Spaces, для корректного компаринга.
                    _source = value.Replace('\u0009'.ToString(), new string(' ', FCTB?.TabLength ?? 2));
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

        public bool IsContentChanged => !FCTB.Text.Equals(Source, StringComparison.Ordinal);

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
            get => statusStrip.SizingGrip;
            set => statusStrip.SizingGrip = value;
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

        public bool ValidateOnChange
        {
            get => _validateEditorOnChange;
            set
            {
                if (_validateEditorOnChange == value)
                    return;

                _validateEditorOnChange = value;
                if (_validateEditorOnChange)
                    FCTB.TextChanged += Fctb_TextChanged;
                else
                    FCTB.TextChanged -= Fctb_TextChanged;
            }
        }

        public new bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                FCTB.Enabled = value;
            }
        }

        public bool ReadOnly
        {
            get => FCTB.ReadOnly;
            set => FCTB.ReadOnly = value;
        }

        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                FCTB.Font = value;
            }
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

        public override Cursor Cursor
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

        public virtual Color SelectionColor
        {
            get => FCTB.SelectionColor;
            set => FCTB.SelectionColor = value;
        }

        protected new bool IsDisposed
        {
            get => base.IsDisposed && isDisposed;
            private set => isDisposed = value;
        }

        static ToolStripLabel GetStripLabel(string text, int leftPadding = 0, int rightPadding = 0)
        {
            return new ToolStripLabel(text)
            {
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 0)
            };
        }

        public int TextLength => FCTB.TextLength;
        public int LinesCount => FCTB.LinesCount;
        public Range Selection => FCTB.Selection;
        public string SelectedText => FCTB.SelectedText;
        public Language Language => FCTB.Language;

        protected internal SimpleEditor()
        {
            FCTB = new FastColoredTextBox();
            statusStrip = new StatusStrip();
            statusStrip.Cursor = Cursors.Default;
            statusStrip.ForeColor = Color.Black;

            FCTB.AutoCompleteBracketsList = new char[] { '(', ')', '{', '}', '[', ']', '\"', '\"', '\'', '\'' };
            FCTB.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);";
            FCTB.AutoScrollMinSize = new Size(27, 14);
            FCTB.BackBrush = null;
            FCTB.CharHeight = 14;
            FCTB.CharWidth = 8;
            FCTB.Cursor = Cursors.IBeam;
            FCTB.DisabledColor = Color.FromArgb(100, 180, 180, 180);
            FCTB.Dock = DockStyle.Fill;
            FCTB.IsReplaceMode = false;
            FCTB.Location = new Point(0, 0);
            FCTB.Paddings = new Padding(0);
            FCTB.Size = new Size(764, 312);
            FCTB.ImeMode = ImeMode.Off;
            FCTB.ForeColor = Color.Black;
            FCTB.TabIndex = 0;
            FCTB.TabLength = 2;
            FCTB.Zoom = 100;

            Controls.Add(FCTB);
            Controls.Add(statusStrip);

            _listOfLanguages = new ToolStripComboBox { BackColor = SystemColors.Control, Padding = new Padding(0, 2, 0, 0) };
            foreach (Language lang in Enum.GetValues(typeof(Language)))
            {
                _listOfLanguages.Items.Add(lang);
            }
            _listOfLanguages.DropDownStyle = ComboBoxStyle.DropDownList;
            statusStrip.Items.Add(_listOfLanguages);
            _listOfLanguages.SelectedIndexChanged += (sender, args) =>
            {
                if (sender is ToolStripComboBox comboBoxLanguages && comboBoxLanguages.SelectedItem is Language lang && Language != lang)
                {
                    bool isChanged = ChangeLanguage(lang);
                    if (isChanged)
                        LanguageChanged?.Invoke(this, EventArgs.Empty);
                }
            };

            statusStrip.Items.Add(new ToolStripSeparator());
            _wordWrapping = new CheckBox { BackColor = Color.Transparent, Text = @"Wrap", Checked = FCTB.WordWrap, Padding = new Padding(10, 0, 0, 0) };
            _wordWrapping.CheckStateChanged += (s, e) =>
            {
                if (FCTB.WordWrap == _wordWrapping.Checked)
                    return;

                FCTB.WordWrap = _wordWrapping.Checked;
                WordWrapStateChanged?.Invoke(this, EventArgs.Empty);
            };
            var wordWrapToolStrip = new ToolStripControlHost(_wordWrapping);
            statusStrip.Items.Add(wordWrapToolStrip);

            statusStrip.Items.Add(new ToolStripSeparator());
            _highlights = new CheckBox { BackColor = Color.Transparent, Text = @"Highlights", Checked = false, Padding = new Padding(10, 0, 0, 0) };
            _highlights.CheckStateChanged += (s, e) =>
            {
                FCTB.VisibleRange.ClearStyle(SameWordsStyle);
                if (Highlights)
                {
                    FCTB.SelectionChangedDelayed += FCTB_SelectionChangedDelayed;
                }
                else
                {
                    FCTB.SelectionChangedDelayed -= FCTB_SelectionChangedDelayed;
                }
                WordHighlightsStateChanged?.Invoke(this, EventArgs.Empty);
            };
            var highlightsToolStrip = new ToolStripControlHost(_highlights);
            statusStrip.Items.Add(highlightsToolStrip);

            statusStrip.Items.Add(new ToolStripSeparator());
            _encodingInfo = GetStripLabel("");
            statusStrip.Items.Add(_encodingInfo);

            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(GetStripLabel("length:"));
            _contentLengthInfo = GetStripLabel("");
            statusStrip.Items.Add(_contentLengthInfo);
            statusStrip.Items.Add(GetStripLabel("lines:"));
            _contentLinesInfo = GetStripLabel("");
            statusStrip.Items.Add(_contentLinesInfo);

            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(GetStripLabel("Ln:"));
            _currentLineInfo = GetStripLabel("");
            statusStrip.Items.Add(_currentLineInfo);
            statusStrip.Items.Add(GetStripLabel("Col:"));
            _currentPosition = GetStripLabel("");
            statusStrip.Items.Add(_currentPosition);
            statusStrip.Items.Add(GetStripLabel("Sel:"));
            _selectedInfo = GetStripLabel("");
            statusStrip.Items.Add(_selectedInfo);
        }

        protected internal SimpleEditor(string headerName, string bodyText, bool wordWrap, Language language, bool wordHighlights) : this()
        {
            Source = bodyText;
            HeaderName = headerName;

            Initialize(language, wordWrap);
            Highlights = wordHighlights;
        }

        protected void Initialize(Language language, bool wordWrap)
        {
            FCTB.ClearStylesBuffer();
            FCTB.Range.ClearStyle(StyleIndex.All);
            FCTB.WordWrap = wordWrap;
            FCTB.Language = language;
            FCTB.Text = Source;
            ValidateOnChange = true;
            FCTB.ClearUndo(); // если убрать метод то при Undo все вернется к пустоте а не к исходнику

            FCTB.SelectionChanged += FCTB_SelectionChanged;

            RefreshForm();
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

        protected virtual void FCTB_SelectionChanged(object sender, EventArgs e)
        {
            SelectionChanged?.Invoke(sender, e);
            RefreshForm();
        }

        public void PrintXml()
        {
            try
            {
                if (FCTB.Language == Language.XML && FCTB.Text.IsXml(out var document))
                {
                    FCTB.Text = document.PrintXml();
                    CheckOnSomethingChanged();
                }
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

        public bool ChangeLanguage(Language lang)
        {
            bool isChanged = FCTB.Language != lang;

            FCTB.ClearStylesBuffer();
            FCTB.Range.ClearStyle(StyleIndex.All);
            FCTB.Language = lang;
            FCTB.OnSyntaxHighlight(new TextChangedEventArgs(FCTB.Range));

            if (isChanged)
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            return isChanged;
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

        private void Fctb_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckOnSomethingChanged();
        }

        protected void CheckOnSomethingChanged(bool isChanged = false)
        {
            // InvokeRequired всегда вернет true, если это работает контекст чужого потока 
            if (Page.InvokeRequired)
            {
                Page.BeginInvoke(new MethodInvoker(delegate
                {
                    Page.ForeColor = IsContentChanged ? Color.Red : Color.Green;
                    Page.Text = HeaderName.Trim() + new string(' ', 2);
                }));
            }
            else
            {
                Page.ForeColor = IsContentChanged ? Color.Red : Color.Green;
                Page.Text = HeaderName.Trim() + new string(' ', 2);
            }

            if (IsContentChanged || isChanged)
                OnSomethingChanged?.Invoke(this, null);
        }

        public new void Focus()
        {
            FCTB.Focus();
        }

        public new virtual void Dispose()
        {
            IsDisposed = true;
            FCTB.Dispose();
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
