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

        public virtual void Dispose()
        {
            IsDisposed = true;
            FCTB.Dispose();
        }
    }

    public class FileEditor : Editor
    {
        private static readonly object syncWhenFileChanged = new object();
        private string _fileName = null;
        private FileSystemWatcher _watcher;


        public string FilePath
        {
            get => _fileName;
            private set
            {
                _fileName = value;
                HeaderName = IO.GetLastNameInPath(_fileName, true);
            }
        }

        public override Encoding Encoding
        {
            get
            {
                try
                {
                    return IO.GetEncoding(FilePath);
                }
                catch (Exception)
                {
                    return base.Encoding;
                }
            }
        }

        internal FileEditor(string filePath, bool wordWrap, bool wordHighlights)
        {
            if (!File.Exists(filePath))
                throw new ArgumentException($"File \"{filePath}\" not found!");

            var langByFileExtension = InitializeFile(filePath);

            InitializeFCTB(langByFileExtension, wordWrap);
            WordHighlights = wordHighlights;

            InitializePage();
        }

        Language InitializeFile(string filePath)
        {
            var langByExtension = GetLanguage(filePath);

            FilePath = filePath;
            Source = IO.SafeReadFile(filePath);

            if (langByExtension == Language.XML && !Source.IsXml(out _))
                MessageBox.Show($"Xml file \"{filePath}\" is incorrect!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            EnableWatcher(FilePath);

            return langByExtension;
        }

        static Language GetLanguage(string filePath)
        {
            var extension = Path.GetExtension(filePath)?.ToLower();
            switch (extension)
            {
                case ".xml": return Language.XML;
                case ".cs": return Language.CSharp;
                case ".html": return Language.HTML;
                case ".js": return Language.JS;
                case ".lua": return Language.Lua;
                case ".php": return Language.PHP;
                case ".sql": return Language.SQL;
                case ".vb": return Language.VB;
                default: return Language.Custom;
            }
        }

        public string GetFileFilter()
        {
            var fileFilter = string.Empty;
            switch (FCTB.Language)
            {
                case Language.XML: fileFilter = "XML files (*.xml)|*.xml"; break;
                case Language.CSharp: fileFilter = "CSharp files (*.cs)|*.cs"; break;
                case Language.HTML: fileFilter = "HTML files (*.html)|*.html"; break;
                case Language.JS: fileFilter = "JS files (*.js)|*.js"; break;
                case Language.Lua: fileFilter = "Lua files (*.lua)|*.lua"; break;
                case Language.PHP: fileFilter = "PHP files (*.php)|*.php"; break;
                case Language.SQL: fileFilter = "SQL files (*.sql)|*.sql"; break;
                case Language.VB: fileFilter = "VB files (*.vb)|*.vb"; break;
            }

            fileFilter = fileFilter.IsNullOrEmpty() ? @"All files (*.*)|*.*" : fileFilter + @"|All files (*.*)|*.*";

            return fileFilter;
        }

        private void OnForeignFileChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Deleted:
                        MessageBox.Show($"File \"{FilePath}\" deleted.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Source = string.Empty;
                        SomethingChanged();
                        return;
                    case WatcherChangeTypes.Created:
                    case WatcherChangeTypes.Changed:
                    {
                        lock (syncWhenFileChanged)
                        {
                            var tryCount = 0;

                            while (!IO.IsFileReady(FilePath))
                            {
                                if (tryCount >= 5)
                                {
                                    MessageBox.Show($"File \"{FilePath}\" сhanged. Current process cannot access the file because it is being used by another process", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                if (IsDisposed)
                                    return;

                                System.Threading.Thread.Sleep(500);
                                tryCount++;
                            }

                            if (IsDisposed)
                                return;

                            Source = File.ReadAllText(FilePath);
                        }

                        SomethingChanged();

                        break;
                    }
                }
            }
            catch (Exception)
            {
                // null
            }
        }


        private void OnFileRenamed(object source, RenamedEventArgs e)
        {
            FilePath = e.FullPath;
            SomethingChanged(); //  e.OldFullPath, e.FullPath
        }

        public void SaveDocumnet(string newFileDestination = null)
        {
            try
            {
                if (!IsContentChanged && newFileDestination == null && FilePath != null)
                    return;

                if (FCTB.Language == Language.XML && !FCTB.Text.IsXml(out _))
                {
                    var saveFailedXmlFile = MessageBox.Show(@"Xml is incorrect! Save anyway?", @"Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                    if (saveFailedXmlFile == DialogResult.Cancel)
                        return;
                }

                if (FilePath == null && newFileDestination == null)
                {
                    string fileDestination = null;
                    using (var sfd = new SaveFileDialog())
                    {
                        sfd.Filter = GetFileFilter();
                        if (sfd.ShowDialog() != DialogResult.OK)
                            return;

                        fileDestination = sfd.FileName;
                    }

                    if(fileDestination.IsNullOrEmpty())
                        return;

                    lock (syncWhenFileChanged)
                    {
                        IO.WriteFile(fileDestination, FCTB.Text, Encoding);
                    }

                    var newLanguage = InitializeFile(fileDestination);
                    ChangeLanguage(newLanguage);

                    EnableWatcher(FilePath);

                    SomethingChanged();
                }
                else
                {
                    if (newFileDestination != null)
                    {
                        lock (syncWhenFileChanged)
                        {
                            IO.WriteFile(newFileDestination, FCTB.Text);
                        }

                        var newLanguage = InitializeFile(newFileDestination);
                        ChangeLanguage(newLanguage);

                        EnableWatcher(FilePath);

                        SomethingChanged();
                    }
                    else
                    {
                        lock (syncWhenFileChanged)
                        {
                            IO.WriteFile(FilePath, FCTB.Text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void EnableWatcher(string filePath)
        {
            DisableWatcher();

            _watcher = new FileSystemWatcher();
            var paths = filePath.Split('\\');
            _watcher.Path = string.Join("\\", paths.Take(paths.Length - 1));
            _watcher.Filter = paths[paths.Length - 1];
            _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _watcher.Changed += OnForeignFileChanged;
            _watcher.Created += OnForeignFileChanged;
            _watcher.Deleted += OnForeignFileChanged;
            _watcher.Renamed += OnFileRenamed;
            _watcher.EnableRaisingEvents = true;
        }

        void DisableWatcher()
        {
            if (_watcher == null)
                return;

            _watcher.Changed -= OnForeignFileChanged;
            _watcher.Created -= OnForeignFileChanged;
            _watcher.Deleted -= OnForeignFileChanged;
            _watcher.Renamed -= OnFileRenamed;
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
        }

        public override void Dispose()
        {
            DisableWatcher();
            base.Dispose();
        }
    }
}