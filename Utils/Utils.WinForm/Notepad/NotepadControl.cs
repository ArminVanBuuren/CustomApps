using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace Utils.WinForm.Notepad
{
    public partial class NotepadControl : UserControl, IEnumerable<KeyValuePair<Editor, TabPage>>
    {
        private bool _wordWrap = false;
        private bool _highlights = false;
        private bool _sizingGrip = false;
        private bool _allowUserCloseItems = false;
        private bool _readOnly = false;

        private int _lastSelectedPage = 0;
        private Font _textFont = new Font("Segoe UI", 9F);
        private Encoding _encoding = Encoding.Default;
        private Color _tabsForeColor = Color.Green;
        private Color _textForeColor = Color.Black;

        public event EventHandler OnRefresh;
        public event EventHandler LanguageChanged;
        public event EventHandler WordWrapStateChanged;
        public event EventHandler WordHighlightsStateChanged;

        Dictionary<Editor, TabPage> ListOfEditors { get; } = new Dictionary<Editor, TabPage>();

        public KeyValuePair<Editor, TabPage>? Current { get; private set; } = null;

        public Encoding DefaultEncoding
        {
            get => _encoding;
            set
            {
                _encoding = value;
                foreach (var editor in ListOfEditors.Keys.Where(x => !Equals(x.DefaultEncoding, _encoding)))
                    editor.DefaultEncoding = _encoding;
            }
        }

        public bool WordWrap
        {
            get => _wordWrap;
            set
            {
                _wordWrap = value;
                foreach (var editor in ListOfEditors.Keys.Where(x => x.WordWrap != _wordWrap))
                    editor.WordWrap = _wordWrap;
            }
        }

        public bool Highlights
        {
            get => _highlights;
            set
            {
                _highlights = value;
                foreach (var editor in ListOfEditors.Keys.Where(x => x.Highlights != _highlights))
                    editor.Highlights = _highlights;
            }
        }

        public bool ReadOnly
        {
            get => _readOnly;
            set
            {
                _readOnly = value;
                foreach (var editor in ListOfEditors.Keys.Where(x => x.ReadOnly != _readOnly))
                    editor.ReadOnly = _readOnly;
            }
        }

        public bool SizingGrip
        {
            get => _sizingGrip;
            set
            {
                _sizingGrip = value;
                foreach (var editor in ListOfEditors.Keys.Where(x => x.SizingGrip != _sizingGrip))
                    editor.SizingGrip = _sizingGrip;
            }
        }

        public Font TextFont
        {
            get => _textFont;
            set
            {
                _textFont = value;
                foreach (var editor in ListOfEditors.Keys.Where(x => !Equals(x.Font, _textFont)))
                    editor.Font = _textFont;
            }
        }

        public Color TextForeColor
        {
            get => _textForeColor;
            set
            {
                _textForeColor = value;
                foreach (var editor in ListOfEditors.Keys.Where(x => x.ForeColor != _textForeColor))
                    editor.ForeColor = _textForeColor;
            }
        }

        public int SelectedIndex
        {
            get => TabControlObj.SelectedIndex;
            set => TabControlObj.SelectedIndex = value;
        }

        public Font TabsFont
        {
            get => TabControlObj.Font;
            set => TabControlObj.Font = value;
        }

        public Color TabsForeColor
        {
            get => _tabsForeColor;
            set
            {
                _tabsForeColor = value;
                foreach (var tabPage in ListOfEditors.Values.Where(x => x.ForeColor != _tabsForeColor))
                    tabPage.ForeColor = _tabsForeColor;
            }
        }

        public bool AllowUserCloseItems
        {
            get => _allowUserCloseItems;
            set
            {
                if(_allowUserCloseItems == value)
                    return;

                _allowUserCloseItems = value;
                if (_allowUserCloseItems)
                {
                    TabControlObj.DrawMode = TabDrawMode.OwnerDrawFixed;
                    TabControlObj.MouseClick += TabControlObj_MouseClick;
                    TabControlObj.MouseDown += TabControl1_MouseDown;
                    TabControlObj.HandleCreated += TabControlObj_HandleCreated;
                    TabControlObj.DrawItem += TabControl1_DrawItem;
                }
                else
                {
                    TabControlObj.DrawMode = TabDrawMode.Normal;
                    TabControlObj.MouseClick -= TabControlObj_MouseClick;
                    TabControlObj.MouseDown -= TabControl1_MouseDown;
                    TabControlObj.HandleCreated -= TabControlObj_HandleCreated;
                    TabControlObj.DrawItem -= TabControl1_DrawItem;
                }
            }
        }

        public NotepadControl()
        {
            InitializeComponent();

            TabControlObj.DrawMode = TabDrawMode.Normal;
            TabControlObj.Padding = new Point(12, 4);
            TabControlObj.BackColor = Color.White;
            TabControlObj.Deselected += TabControlObj_Deselected;
            TabControlObj.Selecting += RefreshForm;
        }

        private void TabControlObj_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                try
                {
                    for (var i = 0; i < TabControlObj.TabCount; ++i)
                    {
                        if (TabControlObj.GetTabRect(i).Contains(e.Location))
                        {
                            TabControlObj.SelectedIndex = i;
                        }
                    }

                    var closeMenuStrip = new ContextMenuStrip
                    {
                        Tag = TabControlObj.TabPages[TabControlObj.SelectedIndex]
                    };
                    closeMenuStrip.Items.Add("Close");
                    closeMenuStrip.Items.Add("Close All But This");
                    closeMenuStrip.Items.Add("Close All Documents");
                    closeMenuStrip.ItemClicked += CloseMenuStrip_ItemClicked;
                    closeMenuStrip.Show(TabControlObj, e.Location);
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }
        }

        private void CloseMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "Close":
                    CloseTab(TabControlObj.SelectedIndex);
                    break;
                case "Close All But This":
                    for (var i = 0; i < TabControlObj.TabCount; ++i)
                    {
                        if (TabControlObj.SelectedIndex == i)
                            continue;

                        CloseTab(i, false);
                        i--;
                    }
                    break;
                case "Close All Documents":
                    for (var i = 0; i < TabControlObj.TabCount; ++i)
                    {
                        CloseTab(i, false);
                        i--;
                    }
                    break;
            }
        }

        /// <summary>
        /// Добавить текстовый контент
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="bodyText"></param>
        /// <param name="language"></param>
        public Editor AddDocument(string headerName, string bodyText, Language language = Language.Custom)
        {
            if (headerName.Length > 70)
                throw new Exception("Header name is too longer");

            var existEditor = ListOfEditors.FirstOrDefault(x => x.Key?.HeaderName != null
                                                                && x.Key?.Source != null
                                                                && !(x.Key is FileEditor)
                                                                && x.Key.HeaderName.Equals(headerName, StringComparison.InvariantCultureIgnoreCase)
                                                                && x.Key.Source.Equals(bodyText));

            if (existEditor.Key != null && existEditor.Value != null)
            {
                TabControlObj.SelectedTab = existEditor.Value;
                return existEditor.Key;
            }

            var editor = new Editor(headerName, bodyText, WordWrap, language, Highlights);
            InitializePage(editor);
            return editor;
        }

        /// <summary>
        /// Добавить файловый документ
        /// </summary>
        /// <param name="filePath"></param>
        public Editor AddDocument(string filePath)
        {
            if (filePath.IsNullOrEmptyTrim())
                throw new ArgumentNullException(nameof(filePath));

            var existFileEditor = ListOfEditors.FirstOrDefault(x => x.Key is FileEditor fileEditor
                                                                    && fileEditor.FilePath != null
                                                                    && fileEditor.FilePath.Equals(filePath, StringComparison.InvariantCultureIgnoreCase));

            if (existFileEditor.Key != null && existFileEditor.Value != null)
            {
                TabControlObj.SelectedTab = existFileEditor.Value;
                return existFileEditor.Key;
            }

            var newFileEditor = new FileEditor(filePath, WordWrap, Highlights);
            InitializePage(newFileEditor);
            return newFileEditor;
        }

        public void SelectEditor(int tabIndex)
        {
            if (TabControlObj.TabCount - 1 >= tabIndex)
            {
                TabControlObj.SelectedIndex = tabIndex;
                RefreshForm(null, null);
            }
        }

        public void SelectEditor(Editor editor)
        {
            if (ListOfEditors.TryGetValue(editor, out var tabPage) && tabPage != TabControlObj.SelectedTab)
            {
                TabControlObj.SelectedTab = tabPage;
                RefreshForm(null, null);
            }
        }

        public void ReplaceEditor(Editor removeEditor, Editor newEditor)
        {
            try
            {
                FinnalizePage(removeEditor);
                InitializePage(newEditor);
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        void InitializePage(Editor editor)
        {
            var index = TabControlObj.TabPages.Count;

            var page = new TabPage
            {
                Text = editor.HeaderName + new string(' ', 2),
                UseVisualStyleBackColor = true,
                ForeColor = TabsForeColor,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            page.Controls.Add(editor);

            TabControlObj.TabPages.Add(page);
            if (TabControlObj.TabPages.Count == index)
                TabControlObj.TabPages.Insert(index, editor.HeaderName);

            TabControlObj.SelectedIndex = index;

            editor.Dock = DockStyle.Fill;
            editor.ForeColor = TextForeColor;
            editor.WordWrap = WordWrap;
            editor.Highlights = Highlights;
            editor.ReadOnly = ReadOnly;
            editor.SizingGrip = SizingGrip;
            editor.Font = TextFont;
            editor.DefaultEncoding = DefaultEncoding;
            editor.LanguageChanged += (sender, args) => { LanguageChanged?.Invoke(sender, args); };
            editor.WordWrapStateChanged += (sender, args) => { WordWrapStateChanged?.Invoke(sender, args); };
            editor.WordHighlightsStateChanged += (sender, args) => { WordHighlightsStateChanged?.Invoke(sender, args); };
            editor.OnSomethingChanged += EditorOnSomethingChanged;

            Current = new KeyValuePair<Editor, TabPage>(editor, page);
            ListOfEditors.Add(editor, page);

            if (TabControlObj.TabCount == 1)
                RefreshForm(this, null);
        }

        int FinnalizePage(Editor removeEditor)
        {
            if (removeEditor == null || !ListOfEditors.TryGetValue(removeEditor, out var tabPage))
                return -1;

            var prevSelectedIndex = TabControlObj.SelectedIndex;
            TabControlObj.TabPages.Remove(tabPage);
            ListOfEditors.Remove(removeEditor);
            removeEditor.Dispose();
            return prevSelectedIndex;
        }

        void EditorOnSomethingChanged(object sender, EventArgs args)
        {
            if (!(sender is Editor editor) || !ListOfEditors.TryGetValue(editor, out var tabPage))
                return;

            void RefreshTabPage()
            {
                tabPage.ForeColor = editor.IsContentChanged ? Color.Red : Color.Green;
                tabPage.Text = editor.HeaderName.Trim() + new string(' ', 1);
            }

            // InvokeRequired всегда вернет true, если это работает контекст чужого потока 
            if (InvokeRequired)
                tabPage.BeginInvoke(new MethodInvoker(RefreshTabPage));
            else
                RefreshTabPage();

            RefreshForm(null, null);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private const int TCM_SETMINTABWIDTH = 0x1300 + 49;

        private void TabControlObj_HandleCreated(object sender, EventArgs e)
        {
            SendMessage(TabControlObj.Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr) 16);
        }

        private void RefreshForm(object sender, TabControlCancelEventArgs e)
        {
            try
            {
                if (TabControlObj.TabPages.Count == 0)
                {
                    Current = null;
                    return;
                }

                if (TabControlObj.SelectedTab == null && _lastSelectedPage <= TabControlObj.TabCount - 1)
                    TabControlObj.SelectedIndex = _lastSelectedPage;

                if (TabControlObj.SelectedTab == null || TabControlObj.SelectedTab.Controls.Count == 0)
                    return;

                var editor = TabControlObj.SelectedTab.Controls[0];
                if (!(editor is Editor editor2))
                    return;

                Current = new KeyValuePair<Editor, TabPage>(editor2, TabControlObj.SelectedTab);
            }
            catch (Exception ex)
            {
                // ignored
            }
            finally
            {
                OnRefresh?.Invoke(this, EventArgs.Empty);
            }
        }

        private void TabControlObj_Deselected(object sender, TabControlEventArgs e)
        {
            if (e.TabPageIndex != -1)
                _lastSelectedPage = e.TabPageIndex;
        }

        private void TabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            for (var i = 0; i < TabControlObj.TabPages.Count; i++)
            {
                var tabRect = TabControlObj.GetTabRect(i);
                tabRect.Inflate(-2, -2);
                var closeImage = Properties.Resources.Close;
                var imageRect = new Rectangle(
                    (tabRect.Right - closeImage.Width),
                    tabRect.Top + (tabRect.Height - closeImage.Height) / 2,
                    closeImage.Width,
                    closeImage.Height);

                if (e != null && imageRect.Contains(e.Location))
                {
                    CloseTab(i);
                    break;
                }
            }
        }

        void CloseTab(int tabIndex, bool calcSelectionTab = true)
        {
            var tabPage = TabControlObj.TabPages[tabIndex];
            if (tabPage == null)
                return;

            var editor = tabPage.Controls[0];
            if(!(editor is Editor editor2))
                return;

            var prevSelectedIndex = FinnalizePage(editor2);

            if(!calcSelectionTab)
                return;

            if (TabControlObj.TabPages.Count > 0)
            {
                if (prevSelectedIndex == tabIndex && _lastSelectedPage > tabIndex)
                {
                    TabControlObj.SelectedIndex = _lastSelectedPage - 1;
                }
                else if (prevSelectedIndex == tabIndex && _lastSelectedPage < tabIndex)
                {
                    TabControlObj.SelectedIndex = _lastSelectedPage;
                }
                else if (prevSelectedIndex > tabIndex)
                {
                    TabControlObj.SelectedIndex = prevSelectedIndex - 1;
                }
                else if (prevSelectedIndex < tabIndex)
                {
                    TabControlObj.SelectedIndex = prevSelectedIndex;
                }
                else if (tabIndex > 0)
                {
                    TabControlObj.SelectedIndex = tabIndex - 1;
                }
            }
        }

        private void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tabPage = TabControlObj.TabPages[e.Index];
            var tabRect = TabControlObj.GetTabRect(e.Index);

            if (AllowUserCloseItems)
            {
                tabRect.Inflate(-2, -2);
                var closeImage = Properties.Resources.Close;
                e.Graphics.DrawImage(closeImage, (tabRect.Right - closeImage.Width) - 1, (tabRect.Top + (tabRect.Height - closeImage.Height) / 2) + 1);
            }

            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font, tabRect, tabPage.ForeColor, tabPage.BackColor, TextFormatFlags.VerticalCenter);
        }

        public new void Focus()
        {
            Current?.Key.Focus();
        }

        public void Clear()
        {
            foreach (var editor in ListOfEditors.Keys)
                editor.Dispose();
        }
        
        public IEnumerator<KeyValuePair<Editor, TabPage>> GetEnumerator()
        {
            return ListOfEditors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ListOfEditors.Keys.GetEnumerator();
        }
    }
}
