﻿using System;
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
    internal class XmlEditor : IDisposable
    {
        readonly MarkerStyle _sameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));
        static object syncFileChange { get; } = new object();
        bool _isDisposed = false;
        FileSystemWatcher _watcher;

        public event EventHandler OnSomethingChanged;
        public string Name { get; private set; }
        public string Path { get; private set; }
        public string Source { get; private set; }
        public FastColoredTextBox FCTB { get; private set; }
        public bool IsContentChanged => !FCTB.Text.Equals(Source);
        public Encoding Encoding => IO.GetEncoding(Path);

        internal XmlEditor(string filePath, bool wordWrap)
        {
            if (!XML.IsXml(filePath, out _, out var source))
                throw new ArgumentException($"File \"{filePath}\" is incorrect!");

            Path = filePath;
            Name = filePath.GetLastNameInPath(true);
            Source = source;

            FCTB = new FastColoredTextBox();
            ((ISupportInitialize) (FCTB)).BeginInit();
            FCTB.ClearStylesBuffer();
            FCTB.Range.ClearStyle(StyleIndex.All);
            FCTB.Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right;
            FCTB.AutoCompleteBracketsList = new[] {'(', ')', '{', '}', '[', ']', '\"', '\"', '\'', '\''};
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
            ((ISupportInitialize) (FCTB)).EndInit();

            FCTB.Language = Language.XML;
            FCTB.Text = Source;
            FCTB.TextChanged += Fctb_TextChanged;
            FCTB.KeyDown += UserTriedToSaveDocument;
            FCTB.SelectionChangedDelayed += Fctb_SelectionChangedDelayed;

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

        private void OnForeignFileChanged(object source, FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Deleted:
                    MessageBox.Show($"File \"{Path}\" was deleted.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Source = string.Empty;
                    OnSomethingChanged?.Invoke(this, null);
                    return;
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:
                {
                    int tryCount = 0;

                    lock (syncFileChange)
                    {
                        while (!IO.IsFileReady(Path))
                        {
                            if (tryCount >= 5)
                            {
                                MessageBox.Show($"File \"{Path}\" was changed. {nameof(XmlEditor)} сannot access file.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            if (_isDisposed)
                                return;

                            System.Threading.Thread.Sleep(500);
                            tryCount++;
                        }

                        if (_isDisposed)
                            return;

                        Source = File.ReadAllText(Path);
                    }

                    OnSomethingChanged?.Invoke(this, null);

                    break;
                }
            }
        }


        private void OnFileRenamed(object source, RenamedEventArgs e)
        {
            Path = e.FullPath;
            OnSomethingChanged?.Invoke(this, null); //  e.OldFullPath, e.FullPath
        }

        private void Fctb_SelectionChangedDelayed(object sender, EventArgs e)
        {
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
            if ((e.Control && KeyIsDown(Keys.ControlKey) && KeyIsDown(Keys.S)) || e.KeyCode == Keys.F2)
            {
                if (!IsContentChanged)
                    return;

                if (XML.IsXml(FCTB.Text, out _))
                {
                    try
                    {
                        lock (syncFileChange)
                        {
                            IO.WriteFile(Path, FCTB.Text);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show(@"XML-File is incorrect! File not saved.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (e.KeyCode == Keys.F5 && XML.IsXml(FCTB.Text, out var document))
            {
                FCTB.Text = document.PrintXml();
                OnSomethingChanged?.Invoke(this, null);
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