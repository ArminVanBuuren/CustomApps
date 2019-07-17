using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace Utils.WinForm.Notepad
{
    internal class Editor : IDisposable
    {
        private readonly Encoding _defaultEncoding = Encoding.Unicode;
        readonly MarkerStyle _sameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));
        static object syncWhenFileChanged { get; } = new object();
        bool _isDisposed = false;
        FileSystemWatcher _watcher;

        public event EventHandler OnSomethingChanged;
        public string Name { get; private set; }
        public string FilePath { get; private set; } = null;
        public string Source { get; private set; }
        public FastColoredTextBox FCTB { get; private set; }
        public bool IsContentChanged => !FCTB.Text.Equals(Source);

        public Encoding Encoding
        {
            get
            {
                try
                {
                    return FilePath != null ? IO.GetEncoding(FilePath) : _defaultEncoding;
                }
                catch (Exception)
                {
                    return Encoding.Default;
                }
            }
        }

        internal Editor(string name, string bodyText, bool wordWrap)
        {
            Source = bodyText;
            Name = name;
            InitializeFCTB(Language.Custom, wordWrap);
        }

        internal Editor(string filePath, bool wordWrap)
        {
            if (!File.Exists(filePath))
                throw new ArgumentException($"File \"{filePath}\" not found!");

            var langByExtension = InitializeFile(filePath);
            InitializeFCTB(langByExtension, wordWrap);
        }

        Language InitializeFile(string filePath)
        {
            var langByExtension = GetLanguage(filePath);

            FilePath = filePath;
            Name = IO.GetLastNameInPath(filePath, true);
            Source = IO.SafeReadFile(filePath);

            if (langByExtension == Language.XML && !Source.IsXml(out _))
                MessageBox.Show($"Xml file \"{filePath}\" is incorrect!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

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

            return langByExtension;
        }

        void InitializeFCTB(Language language, bool wordWrap)
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
            FCTB.TextChanged += Fctb_TextChanged;
            FCTB.KeyDown += UserTriedToSaveDocument;
            FCTB.SelectionChangedDelayed += Fctb_SelectionChangedDelayed;
        }

        public void ChangeLanguage(Language lang)
        {
            FCTB.ClearStylesBuffer();
            FCTB.Range.ClearStyle(StyleIndex.All);
            FCTB.Language = lang;
            FCTB.OnSyntaxHighlight(new TextChangedEventArgs(FCTB.Range));
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

        private void OnForeignFileChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Deleted:
                        MessageBox.Show($"File \"{FilePath}\" was deleted.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Source = string.Empty;
                        OnSomethingChanged?.Invoke(this, null);
                        return;
                    case WatcherChangeTypes.Created:
                    case WatcherChangeTypes.Changed:
                    {
                        lock (syncWhenFileChanged)
                        {
                            int tryCount = 0;

                            while (!IO.IsFileReady(FilePath))
                            {
                                if (tryCount >= 5)
                                {
                                    MessageBox.Show($"File \"{FilePath}\" was changed. The process cannot access the file because it is being used by another process.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                if (_isDisposed)
                                    return;

                                System.Threading.Thread.Sleep(500);
                                tryCount++;
                            }

                            if (_isDisposed)
                                return;

                            Source = File.ReadAllText(FilePath);
                        }

                        OnSomethingChanged?.Invoke(this, null);

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
            OnSomethingChanged?.Invoke(this, null); //  e.OldFullPath, e.FullPath
        }

        private void Fctb_SelectionChangedDelayed(object sender, EventArgs e)
        {
            if(FCTB.Language != Language.XML)
                return;

            FCTB.VisibleRange.ClearStyle(_sameWordsStyle);
            if (!FCTB.Selection.IsEmpty)
                return; //user selected diapason

            //get fragment around caret
            var fragment = FCTB.Selection.GetFragment(@"\w");
            string text = fragment.Text;
            if (text.Length == 0)
                return;

            //highlight same words
            var ranges = FCTB.VisibleRange.GetRanges("\\b" + text + "\\b").ToArray();
            if (ranges.Length > 1)
                foreach (var r in ranges)
                    r.SetStyle(_sameWordsStyle);
        }


        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        private static bool KeyIsDown(Keys key)
        {
            return (GetAsyncKeyState(key) < 0);
        }

        public void UserTriedToSaveDocument(object sender, KeyEventArgs e)
        {
            try
            {
                if ((e.Control && KeyIsDown(Keys.ControlKey) && KeyIsDown(Keys.S)) || e.KeyCode == Keys.F2)
                {
                    if (!IsContentChanged)
                        return;

                    if (FCTB.Language == Language.XML && !FCTB.Text.IsXml(out _))
                    {
                        var saveFailedXmlFile = MessageBox.Show(@"XML-File is incorrect! Save anyway?", @"Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                        if (saveFailedXmlFile == DialogResult.Cancel)
                            return;
                    }

                    if (FilePath.IsNullOrEmptyTrim())
                    {
                        using (var sfd = new SaveFileDialog())
                        {
                            sfd.Filter = @"All files(*.*)|*.*";
                            if (sfd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(sfd.FileName))
                            {
                                FilePath = sfd.FileName;
                            }
                        }

                        lock (syncWhenFileChanged)
                        {
                            IO.WriteFile(FilePath, FCTB.Text, _defaultEncoding);
                        }

                        InitializeFile(FilePath);
                        OnSomethingChanged?.Invoke(this, null);
                    }
                    else
                    {
                        lock (syncWhenFileChanged)
                        {
                            IO.WriteFile(FilePath, FCTB.Text);
                        }
                    }
                }
                else if (e.KeyCode == Keys.F5 && FCTB.Language == Language.XML && FCTB.Text.IsXml(out var document))
                {
                    FCTB.Text = document.PrintXml();
                    OnSomethingChanged?.Invoke(this, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Fctb_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnSomethingChanged?.Invoke(this, null);
        }

        public void Dispose()
        {
            _isDisposed = true;

            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }

            FCTB.Dispose();
        }
    }
}