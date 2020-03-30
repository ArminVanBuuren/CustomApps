using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace Utils.WinForm.Notepad
{
    public partial class NotepadControl : UserControl, ISupportInitialize
    {
        readonly ToolStripLabel _contentLengthInfo;
        readonly ToolStripLabel _contentLinesInfo;
        readonly ToolStripLabel _currentLineInfo;
        readonly ToolStripLabel _currentPosition;
        readonly ToolStripLabel _selectedInfo;
        readonly ToolStripLabel _encodingInfo;
        readonly ToolStripComboBox _listOfLanguages;
        readonly CheckBox _wordWrapping;
        readonly CheckBox _wordHighlights;

        private bool _userCanCloseTabItem = false;
        private bool _readOnly = false;
        private int _lastSelectedPage = 0;
        private Font _textFont = new Font("Segoe UI", 9F);

        public event EventHandler OnRefresh;

        Dictionary<TabPage, Editor> ListOfEditors { get; } = new Dictionary<TabPage, Editor>();

        internal Editor Current { get; private set; } = null;

        public bool WordWrap
        {
            get => _wordWrapping.Checked;
            set => _wordWrapping.Checked = value;
        }

        public bool WordHighlights
        {
            get => _wordHighlights.Checked;
            set => _wordHighlights.Checked = value;
        }

        public int SelectedIndex
        {
            get => TabControlObj.SelectedIndex;
            set => TabControlObj.SelectedIndex = value;
        }

        public bool SizingGrip
        {
            get => statusStrip.SizingGrip;
            set => statusStrip.SizingGrip = value;
        }

        public bool UserCanCloseTabItem
        {
            get => _userCanCloseTabItem;
            set
            {
                if(_userCanCloseTabItem == value)
                    return;

                _userCanCloseTabItem = value;
                if (_userCanCloseTabItem)
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

        public bool ReadOnly
        {
            get => _readOnly;
            set
            {
                if(_readOnly == value)
                    return;

                _readOnly = value;
                foreach (var editor in ListOfEditors.Values)
                    editor.ReadOnly = _readOnly;
            }
        }

        public Font TabsFont
        {
            get => TabControlObj.Font;
            set => TabControlObj.Font = value;
        }

        public Font TextFont
        {
            get => _textFont;
            set
            {
                if(Equals(_textFont, value))
                    return;

                _textFont = value;
                foreach (var editor in ListOfEditors.Values)
                    editor.Font = _textFont;
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

            _listOfLanguages = new ToolStripComboBox {BackColor = SystemColors.Control};
            foreach (Language lang in Enum.GetValues(typeof(Language)))
            {
                _listOfLanguages.Items.Add(lang);
            }
            _listOfLanguages.DropDownStyle = ComboBoxStyle.DropDownList;
            statusStrip.Items.Add(_listOfLanguages);
            _listOfLanguages.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            statusStrip.Items.Add(new ToolStripSeparator());

            _wordWrapping = new CheckBox {BackColor = Color.Transparent, Text = @"Wrap", Checked = true, Padding = new Padding(10, 0, 0, 0)};
            _wordWrapping.CheckStateChanged += (s, e) =>
            {
                foreach (var editor in ListOfEditors.Values.Where(p => p.WordWrap != WordWrap))
                {
                    editor.WordWrap = WordWrap;
                }
            };
            var wordWrapToolStrip = new ToolStripControlHost(_wordWrapping);
            statusStrip.Items.Add(wordWrapToolStrip);
            statusStrip.Items.Add(new ToolStripSeparator());


            _wordHighlights = new CheckBox { BackColor = Color.Transparent, Text = @"Highlights", Checked = false, Padding = new Padding(10, 0, 0, 0) };
            _wordHighlights.CheckStateChanged += (s, e) =>
            {
                foreach (var editor in ListOfEditors.Values.Where(p => p.WordHighlights != WordHighlights))
                {
                    editor.WordHighlights = WordHighlights;
                }
            };
            var wordHighlightsToolStrip = new ToolStripControlHost(_wordHighlights);
            statusStrip.Items.Add(wordHighlightsToolStrip);
            statusStrip.Items.Add(new ToolStripSeparator());

            _encodingInfo = GetStripLabel("");
            statusStrip.Items.Add(_encodingInfo);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(GetStripLabel("length:"));
            _contentLengthInfo = GetStripLabel("");
            statusStrip.Items.Add(_contentLengthInfo);
            statusStrip.Items.Add(GetStripLabel("lines:"));
            _contentLinesInfo = GetStripLabel("");
            statusStrip.Items.Add(_contentLinesInfo);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(GetStripLabel("Ln:"));
            _currentLineInfo = GetStripLabel("");
            statusStrip.Items.Add(_currentLineInfo);
            statusStrip.Items.Add(GetStripLabel("Col:"));
            _currentPosition = GetStripLabel("");
            statusStrip.Items.Add(_currentPosition);
            statusStrip.Items.Add(GetStripLabel("Sel:"));
            _selectedInfo = GetStripLabel("");
            statusStrip.Items.Add(_selectedInfo);
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


        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //TabControlObj.Focus();
            if (sender is ToolStripComboBox comboBoxLanguages && comboBoxLanguages.SelectedItem is Language lang && Current != null && Current.Language != lang)
            {
                Current.ChangeLanguage(lang);
            }
        }

        static ToolStripLabel GetStripLabel(string text, int leftPadding = 0, int rightPadding = 0)
        {
            return new ToolStripLabel(text)
            {
                BackColor = Color.Transparent
            };
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

            var existEditor = ListOfEditors.FirstOrDefault(x => x.Value?.HeaderName != null
                                                                && x.Value?.Source != null
                                                                && !(x.Value is FileEditor)
                                                                && x.Value.HeaderName.Equals(headerName, StringComparison.InvariantCultureIgnoreCase)
                                                                && x.Value.Source.Equals(bodyText));

            if (existEditor.Key != null && existEditor.Value != null)
            {
                TabControlObj.SelectedTab = existEditor.Key;
                return existEditor.Value;
            }

            var editor = new Editor(headerName, bodyText, WordWrap, language, WordHighlights);
            AssignOptions(editor);
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

            var existFileEditor = ListOfEditors.FirstOrDefault(x => x.Value is FileEditor fileEditor
                                                                    && fileEditor?.FilePath != null
                                                                    && fileEditor.FilePath.Equals(filePath, StringComparison.InvariantCultureIgnoreCase));

            if (existFileEditor.Key != null && existFileEditor.Value != null)
            {
                TabControlObj.SelectedTab = existFileEditor.Key;
                return existFileEditor.Value;
            }

            var newFileEditor = new FileEditor(filePath, WordWrap, WordHighlights);
            AssignOptions(newFileEditor);
            InitializePage(newFileEditor);
            return newFileEditor;
        }

        void AssignOptions(Editor editor)
        {
            editor.Font = TextFont;
            editor.ReadOnly = ReadOnly;
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
            if (ListOfEditors.TryGetValue(editor.Page, out var editor2) && editor == editor2)
            {
                TabControlObj.SelectedTab = editor.Page;
                RefreshForm(null, null);
            }
        }

        void InitializePage(Editor editor)
        {
            var index = TabControlObj.TabPages.Count;
            TabControlObj.TabPages.Add(editor.Page);
            if (TabControlObj.TabPages.Count == index)
                TabControlObj.TabPages.Insert(index, editor.HeaderName);

            TabControlObj.SelectedIndex = index;

            editor.SelectionChanged += RefreshFormStatus;
            editor.OnSomethingChanged += EditorOnSomethingChanged;

            ListOfEditors.Add(editor.Page, editor);
            Current = editor;

            RefreshFormStatus(this, null);
        }

        void EditorOnSomethingChanged(object sender, EventArgs args)
        {
            if (!(sender is Editor editor))
                return;

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
                    RefreshFormStatus(this, null);
                    return;
                }

                if (TabControlObj.SelectedTab == null && _lastSelectedPage <= TabControlObj.TabCount - 1)
                    TabControlObj.SelectedIndex = _lastSelectedPage;

                if (TabControlObj.SelectedTab == null || TabControlObj.SelectedTab.Controls.Count == 0)
                    return;

                if (!ListOfEditors.TryGetValue(TabControlObj.SelectedTab, out var editor))
                    return;

                Current = editor;
                RefreshFormStatus(this, null);
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

        private void RefreshFormStatus(object sender, EventArgs e)
        {
            if (Current == null)
            {
                _contentLengthInfo.Text = @"0";
                _contentLinesInfo.Text = @"0";
                _currentLineInfo.Text = @"0";
                _currentPosition.Text = @"0";
                _selectedInfo.Text = @"0|0";
                _encodingInfo.Text = "";
                return;
            }

            _contentLengthInfo.Text = Current.TextLength.ToString();
            _contentLinesInfo.Text = Current.LinesCount.ToString();
            _currentLineInfo.Text = (Current.Selection.FromLine + 1).ToString();
            _currentPosition.Text = (Current.Selection.FromX + 1).ToString();
            _selectedInfo.Text = $"{Current.SelectedText.Length}|{(Current.SelectedText.Length > 0 ? Current.SelectedText.Split('\n').Length : 0)}";
            _encodingInfo.Text = Current.Encoding.EncodingName;

            //_listOfLanguages.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;
            _listOfLanguages.Text = Current.Language.ToString();
            //_listOfLanguages.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
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
            var page = TabControlObj.TabPages[tabIndex];
            if (page == null)
                return;

            if (ListOfEditors.TryGetValue(page, out var editor))
            {
                ListOfEditors.Remove(page);
                editor.OnSomethingChanged -= EditorOnSomethingChanged;
                editor.SelectionChanged -= RefreshFormStatus;
                editor.Dispose();
            }

            var prevSelectedIndex = TabControlObj.SelectedIndex;
            TabControlObj.TabPages.Remove(page);

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

            if (UserCanCloseTabItem)
            {
                tabRect.Inflate(-2, -2);
                var closeImage = Properties.Resources.Close;
                e.Graphics.DrawImage(closeImage, (tabRect.Right - closeImage.Width) - 1, (tabRect.Top + (tabRect.Height - closeImage.Height) / 2) + 1);
            }

            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font, tabRect, tabPage.ForeColor, tabPage.BackColor, TextFormatFlags.VerticalCenter);
        }


        public void Clear()
        {
            foreach (var editor in ListOfEditors.Values)
            {
                editor.Dispose();
            }
        }

        public void BeginInit()
        {
            // ignored
        }

        public void EndInit()
        {
            //OnTextChanged();
            //Selection.Start = Place.Empty;
            //DoCaretVisible();
            //IsChanged = false;
            //ClearUndo();
        }
    }
}
