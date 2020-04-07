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
        private TabControl _tabControl;

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
            get => _tabControl.SelectedIndex;
            set => _tabControl.SelectedIndex = value;
        }

        public Font TabsFont
        {
            get => _tabControl.Font;
            set => _tabControl.Font = value;
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
                if (_allowUserCloseItems == value)
                    return;

                _allowUserCloseItems = value;
                if (_allowUserCloseItems)
                {
                    _tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
                    _tabControl.MouseClick += TabControlObj_MouseClick;
                    _tabControl.MouseDown += TabControl1_MouseDown;
                    _tabControl.HandleCreated += TabControlObj_HandleCreated;
                    _tabControl.DrawItem += TabControl1_DrawItem;
                }
                else
                {
                    _tabControl.DrawMode = TabDrawMode.Normal;
                    _tabControl.MouseClick -= TabControlObj_MouseClick;
                    _tabControl.MouseDown -= TabControl1_MouseDown;
                    _tabControl.HandleCreated -= TabControlObj_HandleCreated;
                    _tabControl.DrawItem -= TabControl1_DrawItem;
                }
            }
        }

        public override Color BackColor
        {
            get => _tabControl.BackColor;
            set => _tabControl.BackColor = value;
        }

        public NotepadControl()
        {
            InitializeComponent();

            _tabControl.DrawMode = TabDrawMode.Normal;
            _tabControl.Padding = new Point(12, 4);
            _tabControl.BackColor = Color.White;
            _tabControl.Deselected += TabControlObj_Deselected;
            _tabControl.Selecting += RefreshForm;
        }

        private void TabControlObj_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                try
                {
                    for (var i = 0; i < _tabControl.TabCount; ++i)
                    {
                        if (_tabControl.GetTabRect(i).Contains(e.Location))
                        {
                            _tabControl.SelectedIndex = i;
                        }
                    }

                    var closeMenuStrip = new ContextMenuStrip
                    {
                        Tag = _tabControl.TabPages[_tabControl.SelectedIndex]
                    };
                    closeMenuStrip.Items.Add("Close");
                    closeMenuStrip.Items.Add("Close All But This");
                    closeMenuStrip.Items.Add("Close All Documents");
                    closeMenuStrip.ItemClicked += CloseMenuStrip_ItemClicked;
                    closeMenuStrip.Show(_tabControl, e.Location);
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
                    CloseTab(_tabControl.SelectedIndex);
                    break;
                case "Close All But This":
                    for (var i = 0; i < _tabControl.TabCount; ++i)
                    {
                        if (_tabControl.SelectedIndex == i)
                            continue;

                        CloseTab(i, false);
                        i--;
                    }

                    break;
                case "Close All Documents":
                    for (var i = 0; i < _tabControl.TabCount; ++i)
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
                throw new Exception("Header name is too long");

            var existEditor = ListOfEditors.FirstOrDefault(x => x.Key?.HeaderName != null
                                                                && x.Key?.Source != null
                                                                && !(x.Key is FileEditor)
                                                                && x.Key.HeaderName.Equals(headerName, StringComparison.InvariantCultureIgnoreCase)
                                                                && x.Key.Source.Equals(bodyText));

            if (existEditor.Key != null && existEditor.Value != null)
            {
                _tabControl.SelectedTab = existEditor.Value;
                return existEditor.Key;
            }

            var editor = new Editor {HeaderName = headerName, Text = bodyText};
            editor.ChangeLanguage(language);

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
                _tabControl.SelectedTab = existFileEditor.Value;
                return existFileEditor.Key;
            }

            var newFileEditor = new FileEditor {FilePath = filePath};
            InitializePage(newFileEditor);
            return newFileEditor;
        }

        public void SelectEditor(int tabIndex)
        {
            if (_tabControl.TabCount - 1 >= tabIndex)
            {
                _tabControl.SelectedIndex = tabIndex;
                RefreshForm(null, null);
            }
        }

        public void SelectEditor(Editor editor)
        {
            if (ListOfEditors.TryGetValue(editor, out var tabPage) && tabPage != _tabControl.SelectedTab)
            {
                _tabControl.SelectedTab = tabPage;
                RefreshForm(null, null);
            }
        }

        public void ReplaceEditor(Editor removeEditor, Editor newEditor)
        {
            try
            {
                removeEditor.DelegateAllEvents(newEditor);
                FinnalizePage(removeEditor);
                InitializePage(newEditor, true);
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        void InitializePage(Editor editor, bool convertToFileEditor = false)
        {
            var index = _tabControl.TabPages.Count;

            var tabPage = new TabPage
            {
                Text = editor.HeaderName + new string(' ', 2),
                UseVisualStyleBackColor = true,
                ForeColor = TabsForeColor,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            tabPage.Controls.Add(editor);

            _tabControl.TabPages.Add(tabPage);
            if (_tabControl.TabPages.Count == index)
                _tabControl.TabPages.Insert(index, editor.HeaderName);

            _tabControl.SelectedIndex = index;

            editor.Dock = DockStyle.Fill;
            editor.ForeColor = TextForeColor;
            editor.WordWrap = WordWrap;
            editor.Highlights = Highlights;
            editor.ReadOnly = ReadOnly;
            editor.SizingGrip = SizingGrip;
            editor.Font = TextFont;
            editor.DefaultEncoding = DefaultEncoding;

            // в конвертируемом FileEditor уже делегируются прошлые эвенты от класса Editor
            if (!convertToFileEditor)
            {
                editor.LanguageChanged += (sender, args) => { LanguageChanged?.Invoke(sender, args); };
                editor.WordWrapStateChanged += (sender, args) => { WordWrapStateChanged?.Invoke(sender, args); };
                editor.WordHighlightsStateChanged += (sender, args) => { WordHighlightsStateChanged?.Invoke(sender, args); };
            }

            if (editor is FileEditor fileEditor)
                fileEditor.FileChanged += EditorOnSomethingChanged;

            Current = new KeyValuePair<Editor, TabPage>(editor, tabPage);
            ListOfEditors.Add(editor, tabPage);

            if (_tabControl.TabCount == 1)
                RefreshForm(this, null);
        }

        int FinnalizePage(Editor removeEditor)
        {
            if (removeEditor == null || !ListOfEditors.TryGetValue(removeEditor, out var tabPage))
                return -1;

            var prevSelectedIndex = _tabControl.SelectedIndex;
            _tabControl.TabPages.Remove(tabPage);
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
                tabPage.ForeColor = editor.IsContentChanged ? Color.Red : TabsForeColor;
                tabPage.Text = editor.HeaderName.Trim() + new string(' ', 2);
                RefreshForm(null, null);
            }

            // InvokeRequired всегда вернет true, если это работает контекст чужого потока 
            if (InvokeRequired)
                tabPage.BeginInvoke(new MethodInvoker(RefreshTabPage));
            else
                RefreshTabPage();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private const int TCM_SETMINTABWIDTH = 0x1300 + 49;

        private void TabControlObj_HandleCreated(object sender, EventArgs e)
        {
            SendMessage(_tabControl.Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr) 16);
        }

        private void RefreshForm(object sender, TabControlCancelEventArgs e)
        {
            try
            {
                if (_tabControl.TabPages.Count == 0)
                {
                    Current = null;
                    return;
                }

                if (_tabControl.SelectedTab == null && _lastSelectedPage <= _tabControl.TabCount - 1)
                    _tabControl.SelectedIndex = _lastSelectedPage;

                if (_tabControl.SelectedTab == null || _tabControl.SelectedTab.Controls.Count == 0)
                    return;

                var editor = _tabControl.SelectedTab.Controls[0];
                if (!(editor is Editor editor2))
                    return;

                Current = new KeyValuePair<Editor, TabPage>(editor2, _tabControl.SelectedTab);
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
            for (var i = 0; i < _tabControl.TabPages.Count; i++)
            {
                var tabRect = _tabControl.GetTabRect(i);
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
            var tabPage = _tabControl.TabPages[tabIndex];
            if (tabPage == null)
                return;

            var editor = tabPage.Controls[0];
            if (!(editor is Editor editor2))
                return;

            var prevSelectedIndex = FinnalizePage(editor2);

            if (!calcSelectionTab)
                return;

            if (_tabControl.TabPages.Count > 0)
            {
                if (prevSelectedIndex == tabIndex && _lastSelectedPage > tabIndex)
                {
                    _tabControl.SelectedIndex = _lastSelectedPage - 1;
                }
                else if (prevSelectedIndex == tabIndex && _lastSelectedPage < tabIndex)
                {
                    _tabControl.SelectedIndex = _lastSelectedPage;
                }
                else if (prevSelectedIndex > tabIndex)
                {
                    _tabControl.SelectedIndex = prevSelectedIndex - 1;
                }
                else if (prevSelectedIndex < tabIndex)
                {
                    _tabControl.SelectedIndex = prevSelectedIndex;
                }
                else if (tabIndex > 0)
                {
                    _tabControl.SelectedIndex = tabIndex - 1;
                }
            }
        }

        private void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tabPage = _tabControl.TabPages[e.Index];
            var tabRect = _tabControl.GetTabRect(e.Index);

            if (AllowUserCloseItems)
            {
                tabRect.Inflate(-2, -2);
                var closeImage = Properties.Resources.Close;
                e.Graphics.DrawImage(closeImage, (tabRect.Right - closeImage.Width) - 2, (tabRect.Top + (tabRect.Height - closeImage.Height) / 2) + 1);
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