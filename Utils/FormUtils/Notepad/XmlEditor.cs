using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using FastColoredTextBoxNS;
using Utils;
using Utils.IOExploitation;
using Utils.XmlHelper;

namespace FormUtils.Notepad
{
    public class XmlEditor : IDisposable
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        public string Source { get; private set; }
        public FastColoredTextBox FCTextBox { get; private set; }
        public event EventHandler OnSomethingChanged;

        public bool IsContentChanged { get; private set; } = false;
        static object Sync { get; } = new object();

        public XmlEditor()
        {

        }

        public bool Load(string path)
        {
            if (!XmlHelper.IsXml(path, out XmlDocument document, out string source))
                return false;

            Path = path;
            Name = path.GetLastNameInPath(true);
            Source = source;
            FCTextBox = InitTextBox(Source);

            FileSystemWatcher watcher = new FileSystemWatcher();
            string[] paths = path.Split('\\');
            watcher.Path = string.Join("\\", paths.Take(paths.Length - 1));
            watcher.Filter = paths[paths.Length - 1];
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.EnableRaisingEvents = true;

            return true;
        }

        

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            lock (Sync)
            {
                if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    Source = string.Empty;
                }
                else if (e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Changed)
                {
                    Source = WaitForFile(Path);
                }

                IsContentChanged = !FCTextBox.Text.Equals(Source);
                OnSomethingChanged?.Invoke(this, null);
            }
        }

        public static string WaitForFile(string path)
        {
            while (!FilesEmployee.IsFileReady(path))
            {
                System.Threading.Thread.Sleep(500);
            }
            return File.ReadAllText(path);
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            Path = e.FullPath;
            OnSomethingChanged?.Invoke(this, null);
            //Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }

        FastColoredTextBox InitTextBox(string source)
        {
            FastColoredTextBox fctb = new FastColoredTextBox();
            ((ISupportInitialize)(fctb)).BeginInit();
            fctb.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
            fctb.AutoCompleteBracketsList = new char[] { '(', ')', '{', '}', '[', ']', '\"', '\"', '\'', '\'' };
            fctb.AutoScrollMinSize = new Size(0, 14);
            fctb.BackBrush = null;
            fctb.CharHeight = 14;
            fctb.CharWidth = 8;
            fctb.Cursor = Cursors.IBeam;
            fctb.DisabledColor = Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            fctb.IsReplaceMode = false;
            fctb.LeftBracket = '(';
            fctb.Name = "fctb";
            fctb.Paddings = new Padding(0);
            fctb.RightBracket = ')';
            fctb.SelectionColor = Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            fctb.ServiceColors = null;
            fctb.TabIndex = 4;
            fctb.WordWrap = true;
            fctb.Zoom = 100;
            fctb.Text = source;
            fctb.SyntaxHighlighter.InitStyleSchema(Language.XML);
            fctb.SyntaxHighlighter.XMLSyntaxHighlight(fctb.Range);
            fctb.Range.ClearFoldingMarkers();
            fctb.TextChanged += Fctb_TextChanged;
            fctb.KeyDown += Fctb_KeyDownSaveDocument;
            fctb.Dock = DockStyle.Fill;
            ((ISupportInitialize)(fctb)).EndInit();
            //fctb.Location = new Point(0, 0);
            //fctb.Size = new System.Drawing.Size(1047, 695);
            return fctb;
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        private void Fctb_KeyDownSaveDocument(object sender, KeyEventArgs e)
        {
            lock (Sync)
            {
                if (e.Control && KeyIsDown(Keys.ControlKey) && KeyIsDown(Keys.S))
                {
                    try
                    {
                        if (!IsContentChanged)
                            return;

                        if (XmlHelper.IsXml(FCTextBox.Text, out XmlDocument document))
                        {
                            FilesEmployee.WriteFile(Path, FCTextBox.Text);
                        }
                        else
                        {
                            MessageBox.Show(@"XML-File is incorrect! Please correсt and then save.");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        OnSomethingChanged?.Invoke(this, null);
                    }
                }
            }
        }

        private bool KeyIsDown(Keys key)
        {
            return (GetAsyncKeyState(key) < 0);
        }

        private void Fctb_TextChanged(object sender, TextChangedEventArgs e)
        {
            FastColoredTextBox fctbInput = (FastColoredTextBox)sender;
            fctbInput.SyntaxHighlighter.InitStyleSchema(Language.XML);
            fctbInput.SyntaxHighlighter.XMLSyntaxHighlight(fctbInput.Range);
            fctbInput.Range.ClearFoldingMarkers();
            IsContentChanged = !fctbInput.Text.Equals(Source);
        }

        public void Dispose()
        {
            FCTextBox.Dispose();
        }
    }
}
