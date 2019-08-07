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
        private string _fileName = null;

        private readonly Encoding _defaultEncoding = Encoding.Unicode;
        readonly MarkerStyle _sameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));
        static object syncWhenFileChanged { get; } = new object();
        bool _isDisposed = false;
        FileSystemWatcher _watcher;

        public event EventHandler OnSomethingChanged;
        public string HeaderName { get; private set; }

        public string FilePath
        {
            get => _fileName;
            private set
            {
                _fileName = value;
                HeaderName = IO.GetLastNameInPath(_fileName, true);
            }
        }

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

        internal Editor(string headerName, string bodyText, bool wordWrap, Language language)
        {
            Source = bodyText;
            HeaderName = headerName;
            InitializeFCTB(language, wordWrap);
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
            Source = IO.SafeReadFile(filePath);

            if (langByExtension == Language.XML && !Source.IsXml(out _))
                MessageBox.Show($"Xml file \"{filePath}\" is incorrect!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            EnableWatcher(FilePath);

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
                        OnSomethingChanged?.Invoke(this, null);
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
            var text = fragment.Text;
            if (text.Length == 0)
                return;

            //highlight same words
            var ranges = FCTB.VisibleRange.GetRanges("\\b" + text + "\\b").ToArray();
            if (ranges.Length > 1)
                foreach (var r in ranges)
                    r.SetStyle(_sameWordsStyle);
        }

        public void PrintXml()
        {
            try
            {
                if (FCTB.Language == Language.XML && FCTB.Text.IsXml(out var document))
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

        public void SaveDocumnet(string newFileDestination = null)
        {
            try
            {
                if (!IsContentChanged && newFileDestination == null)
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
                        IO.WriteFile(fileDestination, FCTB.Text, _defaultEncoding);
                    }

                    var newLanguage = InitializeFile(fileDestination);
                    ChangeLanguage(newLanguage);

                    EnableWatcher(FilePath);

                    OnSomethingChanged?.Invoke(this, null);
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
            DisableWatcher();
            FCTB.Dispose();
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
    }
}