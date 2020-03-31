using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BigMath.Utils;
using FastColoredTextBoxNS;

namespace Utils.WinForm.Notepad
{
    public class Editor : IDisposable
    {
        protected static readonly Encoding DefaultEncoding = new UTF8Encoding(false);
        protected static readonly MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));
        private bool _validateEditorOnChange = false;
        private bool _wordHighLisghts = false;
        private string _source = null;

        public event EventHandler OnSomethingChanged;
        public event EventHandler SelectionChanged;

        protected FastColoredTextBox FCTB { get; set; }

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

        public virtual Encoding Encoding => DefaultEncoding;

        public bool IsContentChanged => !FCTB.Text.Equals(Source, StringComparison.Ordinal);

        public string Text
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

        public bool WordHighlights
        {
            get => _wordHighLisghts;
            set
            {
                if (FCTB == null)
                    return;

                FCTB.VisibleRange.ClearStyle(SameWordsStyle);
                _wordHighLisghts = value;
                if (_wordHighLisghts)
                {
                    FCTB.SelectionChangedDelayed += FCTB_SelectionChangedDelayed;
                }
                else
                {
                    FCTB.SelectionChangedDelayed -= FCTB_SelectionChangedDelayed;
                }
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

        public bool WordWrap
        {
            get => FCTB.WordWrap;
            set => FCTB.WordWrap = value;
        }

        public bool Enabled
        {
            get => FCTB.Enabled;
            set => FCTB.Enabled = value;
        }

        public bool ReadOnly
        {
            get => FCTB.ReadOnly;
            set => FCTB.ReadOnly = value;
        }

        public Font Font
        {
            get => FCTB.Font;
            set => FCTB.Font = value;
        }

        public int TextLength => FCTB.TextLength;
        public int LinesCount => FCTB.LinesCount;
        public Range Selection => FCTB.Selection;
        public string SelectedText => FCTB.SelectedText;
        public Language Language => FCTB.Language;

        protected bool IsDisposed { get; private set; } = false;

        protected Editor() { }

        internal Editor(string headerName, string bodyText, bool wordWrap, Language language, bool wordHighlights)
        {
            Source = bodyText;
            HeaderName = headerName;

            InitializeFCTB(language, wordWrap);
            WordHighlights = wordHighlights;

            InitializePage();
        }

        protected void InitializeFCTB(Language language, bool wordWrap)
        {
            FCTB = new FastColoredTextBox();

            ((ISupportInitialize)(FCTB)).BeginInit();
            FCTB.ClearStylesBuffer();
            FCTB.Range.ClearStyle(StyleIndex.All);
            FCTB.Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right;
            FCTB.AutoCompleteBracketsList = new[] { '(', ')', '{', '}', '[', ']', '\"', '\"', '\'', '\'' };
            FCTB.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);";
            FCTB.AutoScrollMinSize = new Size(0, 14);
            FCTB.BackBrush = null;
            FCTB.CharHeight = 14;
            FCTB.CharWidth = 8;
            FCTB.Cursor = Cursors.IBeam;
            FCTB.DisabledColor = Color.FromArgb(100, 180, 180, 180);
            FCTB.ImeMode = ImeMode.Off;
            FCTB.IsReplaceMode = false;
            FCTB.Name = "fctb";
            FCTB.Paddings = new Padding(0);
            FCTB.SelectionColor = Color.FromArgb(60, 0, 0, 255);
            FCTB.TabIndex = 0;
            FCTB.WordWrap = wordWrap;
            FCTB.Zoom = 100;
            FCTB.Dock = DockStyle.Fill;
            FCTB.ForeColor = Color.Black;
            ((ISupportInitialize)(FCTB)).EndInit();

            FCTB.Language = language;
            FCTB.Text = Source;

            ValidateOnChange = true;
            FCTB.ClearUndo(); // если убрать метод то при Undo все вернется к пустоте а не к исходнику

            FCTB.SelectionChanged += FCTB_SelectionChanged;
        }

        private void FCTB_SelectionChanged(object sender, EventArgs e)
        {
            SelectionChanged?.Invoke(sender, e);
        }

        protected void InitializePage()
        {
            Page = new TabPage
            {
                Text = HeaderName + new string(' ', 2),
                UseVisualStyleBackColor = true,
                ForeColor = Color.Green,
                Margin = new Padding(0),
                Padding = new Padding(0)

            };
            Page.Controls.Add(FCTB);
        }

        public void PrintXml()
        {
            try
            {
                if (FCTB.Language == Language.XML && FCTB.Text.IsXml(out var document))
                {
                    FCTB.Text = document.PrintXml();
                    SomethingChanged();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ChangeLanguage(Language lang)
        {
            FCTB.ClearStylesBuffer();
            FCTB.Range.ClearStyle(StyleIndex.All);
            FCTB.Language = lang;
            FCTB.OnSyntaxHighlight(new TextChangedEventArgs(FCTB.Range));
        }

        private void FCTB_SelectionChangedDelayed(object sender, EventArgs e)
        {
            //if(FCTB.Language != Language.XML)
            //    return;

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
            SomethingChanged();
        }

        protected void SomethingChanged()
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

            OnSomethingChanged?.Invoke(this, null);
        }

        public void Focus()
        {
            FCTB.Focus();
        }

        public virtual void Dispose()
        {
            IsDisposed = true;
            FCTB.Dispose();
        }
    }

}