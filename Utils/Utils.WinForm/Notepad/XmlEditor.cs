using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using FastColoredTextBoxNS;
using Utils.IOExploitation;
using Utils.XmlRtfStyle;
using Utils.IOHelper;

namespace Utils.WinForm.Notepad
{
    public class XmlEditor : IDisposable
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        public string Source { get; private set; }
        public FastColoredTextBox FCTextBox { get; private set; }
        public event EventHandler OnSomethingChanged;
        private FileSystemWatcher watcher;
        public bool IsContentChanged { get; private set; } = false;
        static object Sync { get; } = new object();
        private bool isDisposed = false;

        public XmlEditor()
        {

        }

        public bool Load(string path)
        {
            if (!XmlHelper.XmlHelper.IsXml(path, out XmlDocument document, out string source))
                return false;

            Path = path;
            Name = path.GetLastNameInPath(true);
            Source = source;
            FCTextBox = InitTextBox(Source);


            DisposeWatcher();
            watcher = new FileSystemWatcher();
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

        void DisposeWatcher()
        {
            if (watcher == null)
                return;
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;
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
                    if (WaitForFile(Path, out string result))
                        Source = result;
                    else
                        MessageBox.Show($"Can't access to file: {Path}");
                }

                if (isDisposed)
                    return;

                IsContentChanged = !FCTextBox.Text.Equals(Source);
                OnSomethingChanged?.Invoke(this, null);
            }
        }

        public bool WaitForFile(string path, out string result)
        {
            int tryCount = 0;
            result = null;
            while (!FilesEmployee.IsFileReady(path))
            {
                if (tryCount >= 5)
                    return false;


                if (isDisposed)
                    return false;


                System.Threading.Thread.Sleep(500);
                tryCount++;
            }
            result = File.ReadAllText(path);
            return true;
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
            //fctb.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
            //fctb.AutoScrollMinSize = new Size(0, 14);
            //fctb.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);";
            //fctb.ImeMode = System.Windows.Forms.ImeMode.Off;
            //fctb.BackBrush = null;
            //fctb.CharHeight = 14;
            //fctb.CharWidth = 8;
            //fctb.Cursor = Cursors.IBeam;
            //fctb.DisabledColor = Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            //fctb.IsReplaceMode = false;
            //!!!!!fctb.LeftBracket = '(';
            //fctb.Name = "fctb";
            //fctb.Paddings = new Padding(0);
            //!!!!!fctb.RightBracket = ')';
            //fctb.SelectionColor = Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            //fctb.ServiceColors = null;
            //fctb.TabIndex = 4;
            //fctb.WordWrap = true;
            //fctb.Zoom = 100;
            //fctb.Text = source;


            //fctb.SyntaxHighlighter.InitStyleSchema(Language.XML);
            //fctb.SyntaxHighlighter.XMLSyntaxHighlight(fctb.Range);
            //fctb.Range.ClearFoldingMarkers();
            //fctb.Dock = DockStyle.Fill;

            //fctb.TextChanged += Fctb_TextChanged;
            //fctb.KeyDown += Fctb_KeyDownSaveDocument;

            //fctb.ClearStylesBuffer();
            //fctb.Range.ClearStyle(StyleIndex.All);
            //fctb.Language = Language.XML;
            //fctb.SelectionChangedDelayed += fctb_SelectionChangedDelayed;

            fctb.ClearStylesBuffer();
            fctb.Range.ClearStyle(StyleIndex.All);
            fctb.Language = Language.XML;
            fctb.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
            fctb.Text = source;
            fctb.AutoCompleteBracketsList = new char[] {'(',')','{','}','[',']','\"','\"','\'','\''};
            fctb.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);";
            fctb.AutoScrollMinSize = new System.Drawing.Size(0, 14);
            fctb.BackBrush = null;
            fctb.CharHeight = 14;
            fctb.CharWidth = 8;
            fctb.Cursor = Cursors.IBeam;
            fctb.DisabledColor = Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            fctb.ImeMode = ImeMode.Off;
            fctb.IsReplaceMode = false;
            fctb.Name = "fctb";
            fctb.Paddings = new Padding(0);
            fctb.SelectionColor = Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            fctb.ServiceColors = null;
            fctb.TabIndex = 0;
            fctb.WordWrap = true;
            fctb.Zoom = 100;
            fctb.Dock = DockStyle.Fill;

            fctb.TextChanged += Fctb_TextChanged;
            fctb.KeyDown += Fctb_KeyDownSaveDocument;
            fctb.SelectionChangedDelayed += fctb_SelectionChangedDelayed;

            ((ISupportInitialize)(fctb)).EndInit();



            //fctb.Location = new Point(0, 0);
            //fctb.Size = new System.Drawing.Size(1047, 695);




            return fctb;
        }

        MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));
        private void fctb_SelectionChangedDelayed(object sender, EventArgs e)
        {
            FCTextBox.VisibleRange.ClearStyle(SameWordsStyle);
            if (!FCTextBox.Selection.IsEmpty)
                return;//user selected diapason

            //get fragment around caret
            var fragment = FCTextBox.Selection.GetFragment(@"\w");
            string text = fragment.Text;
            if (text.Length == 0)
                return;
            //highlight same words
            var ranges = FCTextBox.VisibleRange.GetRanges("\\b" + text + "\\b").ToArray();
            if (ranges.Length > 1)
                foreach (var r in ranges)
                    r.SetStyle(SameWordsStyle);
        }


        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        private void Fctb_KeyDownSaveDocument(object sender, KeyEventArgs e)
        {
            SaveOrFormatDocument(e);
        }

        public void SaveOrFormatDocument(KeyEventArgs e)
        {
            lock (Sync)
            {
                if (e.Control && KeyIsDown(Keys.ControlKey) && KeyIsDown(Keys.S))
                {
                    try
                    {
                        if (!IsContentChanged)
                            return;

                        if (XmlHelper.XmlHelper.IsXml(FCTextBox.Text, out XmlDocument document))
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
                else if (e.KeyCode == Keys.F5)
                {
                    XMLPrint();
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

            //fctbInput.Language = Language.XML;
            //fctbInput.ClearStylesBuffer();
            //fctbInput.Range.ClearStyle(StyleIndex.All);
            //fctbInput.AddStyle(SameWordsStyle);
            //fctbInput.OnSyntaxHighlight(new TextChangedEventArgs(fctbInput.Range));

            IsContentChanged = !fctbInput.Text.Equals(Source);
        }

        void XMLPrint()
        {
            bool isXml = XmlHelper.XmlHelper.IsXml(FCTextBox.Text, out XmlDocument document);
            if (isXml)
            {
                string formatting = RtfFromXml.GetXmlString(document);
                FCTextBox.Text = formatting;
            }
        }

        public void Dispose()
        {
            isDisposed = true;
            FCTextBox.Dispose();
            DisposeWatcher();
        }
    }
}
