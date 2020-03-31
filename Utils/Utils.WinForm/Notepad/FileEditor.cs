using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace Utils.WinForm.Notepad
{

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

        internal FileEditor(Editor editor)
        {
            //todo: when save
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

                    if (fileDestination.IsNullOrEmpty())
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
