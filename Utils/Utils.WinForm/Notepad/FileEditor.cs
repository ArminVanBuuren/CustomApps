using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using Utils.WinForm.Properties;

namespace Utils.WinForm.Notepad
{
    public partial class FileEditor : Editor
    {
        private static readonly object syncWhenFileChanged = new object();
        private string _filePath = null;
        private FileSystemWatcher _watcher;

        public event EventHandler FileChanged;

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (!File.Exists(value))
                {
                    MessageBox.Show(string.Format(Resources.FileNotFound, value), @"Error!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _filePath = value;
                HeaderName = Path.GetFileName(_filePath);

                Text = IO.SafeReadFile(_filePath, Encoding);
                Encoding = IO.GetEncoding(_filePath);

                var langByExtension = GetLanguage(_filePath);
                ChangeLanguage(langByExtension);

                EnableWatcher();
            }
        }

        public override Encoding Encoding { get; protected set; }

        public FileEditor() : base()
        {
            InitializeComponent();
        }

        FileEditor(Editor editor) : this()
        {
            base.Source = editor.Text;
            base.Text = editor.Text;
            ChangeLanguage(editor.Language);
            base.WordWrap = editor.WordWrap;
            base.Highlights = editor.Highlights;
        }

        public static FileEditor ConvertToFileEditor(Editor editor, Encoding encoding)
        {
            var newEditor = new FileEditor(editor)
            {
                Encoding = encoding
            };
            return newEditor;
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
            switch (Language)
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

        private void FileOnForeignChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                var oldSource = Source;
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Deleted:
                        MessageBox.Show(string.Format(Resources.FileWasDeleted, FilePath), "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Source = string.Empty;
                        FileChanged?.Invoke(this, new FileEditorEventArgs(this, e.ChangeType) {OldSource = oldSource });
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
                                        
                                        MessageBox.Show(string.Format(Resources.FileWasChangedAndNotAccess, FilePath), @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }

                                    if (IsDisposed)
                                        return;

                                    System.Threading.Thread.Sleep(500);
                                    tryCount++;
                                }

                                if (IsDisposed)
                                    return;

                                Source = IO.SafeReadFile(FilePath, Encoding);
                            }

                            FileChanged?.Invoke(this, new FileEditorEventArgs(this, e.ChangeType) { OldSource = oldSource });
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
            FileChanged?.Invoke(this, new FileEditorEventArgs(this, e.ChangeType) {OldFilePath = e.OldFullPath});
        }

        protected override void TextChangedChanged(Editor editor, TextChangedEventArgs args)
        {
            FileChanged?.Invoke(this, new FileEditorEventArgs(this, WatcherChangeTypes.Changed));
        }

        public void SaveDocument(string newFileDestination = null)
        {
            try
            {
                if (!IsContentChanged && newFileDestination == null && FilePath != null)
                    return;

                var oldFilePath = FilePath;
                var oldSource = Source;
                var oldText = Text;
                WatcherChangeTypes type = WatcherChangeTypes.All;

                if (Language == Language.XML && !Text.IsXml(out _))
                {
                    var saveFailedXmlFile = MessageBox.Show(Resources.IncorrectXmlAndQuestion, @"Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                    if (saveFailedXmlFile == DialogResult.Cancel)
                        return;
                }

                if (FilePath.IsNullOrEmpty() && newFileDestination.IsNullOrEmpty())
                {
                    using (var sfd = new SaveFileDialog())
                    {
                        sfd.Filter = GetFileFilter();
                        if (sfd.ShowDialog() != DialogResult.OK)
                            return;

                        newFileDestination = sfd.FileName;
                    }

                    if (newFileDestination.IsNullOrEmpty())
                        return;

                    type = WatcherChangeTypes.Created;
                }

                if (newFileDestination != null)
                {
                    SaveFile(newFileDestination);
                    FilePath = newFileDestination;
                    type = type == WatcherChangeTypes.All ? WatcherChangeTypes.Renamed : type;
                }
                else
                {
                    Source = Text;
                    SaveFile(FilePath, true);
                    type = WatcherChangeTypes.Changed;
                }

                FileChanged?.Invoke(this, new FileEditorEventArgs(this, type) {OldFilePath = oldFilePath, OldSource = oldSource, OldText = oldText});
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void SaveFile(string destinationFile, bool enableWatcher = false)
        {
            try
            {
                DisableWatcher();
                lock (syncWhenFileChanged)
                {
                    IO.WriteFile(destinationFile, Text, false, Encoding);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (enableWatcher)
                    EnableWatcher();
            }
        }

        void EnableWatcher()
        {
            DisableWatcher();

            _watcher = new FileSystemWatcher();
            var paths = FilePath.Split('\\');
            _watcher.Path = string.Join("\\", paths.Take(paths.Length - 1)) + "\\";
            _watcher.Filter = paths[paths.Length - 1];
            _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _watcher.Changed += FileOnForeignChanged;
            _watcher.Created += FileOnForeignChanged;
            _watcher.Deleted += FileOnForeignChanged;
            _watcher.Renamed += OnFileRenamed;
            _watcher.EnableRaisingEvents = true;
        }

        void DisableWatcher()
        {
            if (_watcher == null)
                return;

            _watcher.Changed -= FileOnForeignChanged;
            _watcher.Created -= FileOnForeignChanged;
            _watcher.Deleted -= FileOnForeignChanged;
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
