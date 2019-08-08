﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace Utils.WinForm.Notepad
{
    public partial class Notepad : Form
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
        

        private Editor _currentEditor = null;
        private int _lastSelectedPage = 0;
        private int numberOfNewDocument = 0;
        

        Dictionary<TabPage, Editor> ListOfEditors { get; } = new Dictionary<TabPage, Editor>();

        public bool WindowIsClosed { get; private set; } = false;

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

        public Notepad(string filePath = null, bool wordWrap = false)
        {
            InitializeComponent();

            KeyPreview = true; // для того чтобы работали горячие клавиши по всей форме и всем контролам

            fileToolStripMenuItem.DropDownItemClicked += FileToolStripMenuItem_DropDownItemClicked;

            TabControlObj.Padding = new Point(12, 4);
            TabControlObj.DrawMode = TabDrawMode.OwnerDrawFixed;
            TabControlObj.BackColor = Color.White;
            TabControlObj.DrawItem += TabControl1_DrawItem;
            TabControlObj.MouseDown += TabControl1_MouseDown;
            TabControlObj.Deselected += TabControlObj_Deselected;
            TabControlObj.Selecting += RefreshForm;
            TabControlObj.HandleCreated += TabControlObj_HandleCreated;
            TabControlObj.MouseClick += TabControlObj_MouseClick;

            Closed += XmlNotepad_Closed;
            KeyDown += Notepad_KeyDown;

            _listOfLanguages = new ToolStripComboBox {BackColor = SystemColors.Control};
            foreach (Language lang in Enum.GetValues(typeof(Language)))
            {
                _listOfLanguages.Items.Add(lang);
            }
            _listOfLanguages.Size = new Size(100, statusStrip.Height - 5);
            _listOfLanguages.DropDownStyle = ComboBoxStyle.DropDownList;
            statusStrip.Items.Add(_listOfLanguages);
            statusStrip.Items.Add(new ToolStripSeparator());

            _wordWrapping = new CheckBox {BackColor = Color.Transparent, Text = @"Wrap", Checked = wordWrap, Padding = new Padding(10, 0, 0, 0)};
            _wordWrapping.CheckStateChanged += (s, e) =>
            {
                foreach (var editor in ListOfEditors.Values.Where(p => p.FCTB.WordWrap != WordWrap))
                {
                    editor.FCTB.WordWrap = WordWrap;
                }
            };
            var wordWrapToolStrip = new ToolStripControlHost(_wordWrapping);
            statusStrip.Items.Add(wordWrapToolStrip);
            statusStrip.Items.Add(new ToolStripSeparator());


            _wordHighlights = new CheckBox { BackColor = Color.Transparent, Text = @"Highlights", Checked = wordWrap, Padding = new Padding(10, 0, 0, 0) };
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

            if (filePath != null)
            {
                var editor2 = AddDocumentAndGetEditor(filePath);
                _listOfLanguages.Text = editor2.FCTB.Language.ToString();
            }

            _listOfLanguages.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
        }

        //[DllImport("user32.dll")]
        //private static extern short GetAsyncKeyState(Keys vKey);

        //private static bool KeyIsDown(Keys key)
        //{
        //    return (GetAsyncKeyState(key) < 0);
        //}

        private void Notepad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                PerformCommand(newToolStripMenuItem);
            }
            else if (e.Control && e.KeyCode == Keys.O) // && KeyIsDown(Keys.ControlKey) && KeyIsDown(Keys.O))
            {
                PerformCommand(openToolStripMenuItem);
            }
            else if (e.KeyCode == Keys.F5)
            {
                PerformCommand(formatXmlF5ToolStripMenuItem);
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                PerformCommand(saveToolStripMenuItem);
            }
            else if(e.KeyCode == Keys.F4 && (ModifierKeys & Keys.Alt) != 0)
            {
                Close();
            }
        }

        private async void FileToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            await Task.Factory.StartNew(() =>
                Invoke(new MethodInvoker(delegate { PerformCommand(e.ClickedItem); })));
        }

        void PerformCommand(ToolStripItem item)
        {
            if (item == newToolStripMenuItem)
            {
                AddDocument($"new {++numberOfNewDocument}", string.Empty);
            }
            else if (item == openToolStripMenuItem)
            {
                using (var fbd = new OpenFileDialog())
                {
                    fbd.Filter = @"All files (*.*)|*.*";
                    fbd.Multiselect = true;
                    if (fbd.ShowDialog() != DialogResult.OK)
                        return;

                    foreach (var file in fbd.FileNames)
                    {
                        if (File.Exists(file))
                        {
                            AddFileDocument(file);
                        }
                    }
                }
            }
            else if (item == formatXmlF5ToolStripMenuItem && _currentEditor != null)
            {
                _currentEditor.PrintXml();
            }
            else if (item == saveToolStripMenuItem && _currentEditor != null)
            {
                _currentEditor.SaveDocumnet();
            }
            else if (item == saveAsToolStripMenuItem && _currentEditor != null)
            {
                string fileDestination;
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = _currentEditor.GetFileFilter();
                    if (sfd.ShowDialog() != DialogResult.OK)
                        return;

                    fileDestination = sfd.FileName;
                }

                if (!fileDestination.IsNullOrEmpty())
                {
                    _currentEditor.SaveDocumnet(fileDestination);
                }
            }
            else if (item == closeToolStripMenuItem)
            {
                Close();
            }
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
            if (sender is ToolStripComboBox comboBoxLanguages && comboBoxLanguages.SelectedItem is Language lang && _currentEditor != null && _currentEditor.FCTB.Language != lang)
            {
                _currentEditor.ChangeLanguage(lang);
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
        public void AddDocument(string headerName, string bodyText, Language language = Language.Custom)
        {
            if(headerName.Length > 70)
                throw new Exception("Header name is too longer");

            var existEditor = ListOfEditors.FirstOrDefault(x => x.Value?.HeaderName != null && x.Value?.Source != null && x.Value?.FilePath == null
                                                                   && x.Value.HeaderName.Equals(headerName, StringComparison.CurrentCultureIgnoreCase)
                                                                   && x.Value.Source.Equals(bodyText));
            if (existEditor.Key != null && existEditor.Value != null)
            {
                TabControlObj.SelectedTab = existEditor.Key;
                return;
            }

            var editor = new Editor(headerName, bodyText, WordWrap, language, WordHighlights);

            InitializePage(editor);
        }

        /// <summary>
        /// Добавить фаловый документ
        /// </summary>
        /// <param name="filePath"></param>
        public void AddFileDocument(string filePath)
        {
            AddDocumentAndGetEditor(filePath);
        }

        Editor AddDocumentAndGetEditor(string filePath)
        {
            if (filePath.IsNullOrEmptyTrim())
                throw new ArgumentNullException(nameof(filePath));

            var existEditor = ListOfEditors.FirstOrDefault(x => x.Value?.FilePath != null 
                                                                   && x.Value.FilePath.Equals(filePath, StringComparison.CurrentCultureIgnoreCase));
            if (existEditor.Key != null && existEditor.Value != null)
            {
                TabControlObj.SelectedTab = existEditor.Key;
                return existEditor.Value;
            }

            var editor = new Editor(filePath, WordWrap, WordHighlights);

            InitializePage(editor);

            return editor;
        }

        void InitializePage(Editor editor)
        {
            var index = TabControlObj.TabPages.Count;
            TabControlObj.TabPages.Add(editor.Page);
            if (TabControlObj.TabPages.Count == index)
                TabControlObj.TabPages.Insert(index, editor.HeaderName);
            TabControlObj.SelectedIndex = index;

            editor.FCTB.SelectionChanged += RefreshFormStatus;
            editor.OnSomethingChanged += EditorOnSomethingChanged;

            ListOfEditors.Add(editor.Page, editor);
            _currentEditor = editor;

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
            if (TabControlObj.TabPages.Count == 0)
            {
                _currentEditor = null;
                Text = nameof(Notepad);
                RefreshFormStatus(this, null);
                return;
            }

            if (TabControlObj.SelectedTab == null && _lastSelectedPage <= TabControlObj.TabCount - 1)
                TabControlObj.SelectedIndex = _lastSelectedPage;

            if (TabControlObj.SelectedTab == null || TabControlObj.SelectedTab.Controls.Count == 0)
                return;

            if (!ListOfEditors.TryGetValue(TabControlObj.SelectedTab, out var editor))
                return;

            _currentEditor = editor;
            Text = editor.FilePath ?? editor.HeaderName;

            RefreshFormStatus(this, null);
        }

        private void TabControlObj_Deselected(object sender, TabControlEventArgs e)
        {
            if (e.TabPageIndex != -1)
                _lastSelectedPage = e.TabPageIndex;
        }

        private void RefreshFormStatus(object sender, EventArgs e)
        {
            if (_currentEditor == null)
            {
                _contentLengthInfo.Text = @"0";
                _contentLinesInfo.Text = @"0";
                _currentLineInfo.Text = @"0";
                _currentPosition.Text = @"0";
                _selectedInfo.Text = @"0|0";
                _encodingInfo.Text = "";
                return;
            }

            _contentLengthInfo.Text = _currentEditor.FCTB.TextLength.ToString();
            _contentLinesInfo.Text = _currentEditor.FCTB.LinesCount.ToString();
            _currentLineInfo.Text = (_currentEditor.FCTB.Selection.FromLine + 1).ToString();
            _currentPosition.Text = (_currentEditor.FCTB.Selection.FromX + 1).ToString();
            _selectedInfo.Text = $"{_currentEditor.FCTB.SelectedText.Length}|{(_currentEditor.FCTB.SelectedText.Length > 0 ? _currentEditor.FCTB.SelectedText.Split('\n').Length : 0)}";
            _encodingInfo.Text = _currentEditor.Encoding?.EncodingName;

            //_listOfLanguages.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;
            _listOfLanguages.Text = _currentEditor.FCTB.Language.ToString();
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
                editor.FCTB.SelectionChanged -= RefreshFormStatus;
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
            tabRect.Inflate(-2, -2);

            var closeImage = Properties.Resources.Close;
            e.Graphics.DrawImage(closeImage, (tabRect.Right - closeImage.Width) -1 , (tabRect.Top + (tabRect.Height - closeImage.Height) / 2) +1);
            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font, tabRect, tabPage.ForeColor, tabPage.BackColor, TextFormatFlags.VerticalCenter);
        }



        private void XmlNotepad_Closed(object sender, EventArgs e)
        {
            foreach (var editor in ListOfEditors.Values)
            {
                editor.Dispose();
            }
            WindowIsClosed = true;
        }
    }
}
