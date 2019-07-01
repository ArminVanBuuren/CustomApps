using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        private readonly ToolStripComboBox _listOfLanguages;
        readonly CheckBox _wordWrapping;
        

        private Editor _currentEditor = null;
        private int _lastSelectedPage = 0;
        

        Dictionary<TabPage, Editor> ListOfXmlEditors { get; } = new Dictionary<TabPage, Editor>();

        public bool WindowIsClosed { get; private set; } = false;

        public bool WordWrap
        {
            get => _wordWrapping.Checked;
            set => _wordWrapping.Checked = value;
        }

        public Notepad(string filePath, bool wordWrap = true)
        {
            InitializeComponent();

            Closed += XmlNotepad_Closed;
            TabControlObj.Padding = new Point(12, 4);
            TabControlObj.DrawMode = TabDrawMode.OwnerDrawFixed;
            TabControlObj.DrawItem += TabControl1_DrawItem;
            TabControlObj.MouseDown += TabControl1_MouseDown;
            TabControlObj.KeyDown += XmlNotepad_KeyDown;
            KeyDown += XmlNotepad_KeyDown;
            TabControlObj.Deselected += TabControlObj_Deselected;
            TabControlObj.Selecting += TabControlObj_Selecting;
            TabControlObj.HandleCreated += TabControlObj_HandleCreated;
            TabControlObj.BackColor = Color.White;


            _listOfLanguages = new ToolStripComboBox {BackColor = SystemColors.Control};
            foreach (Language lang in Enum.GetValues(typeof(Language)))
            {
                _listOfLanguages.Items.Add(lang);
            }
            statusStrip.Items.Add(_listOfLanguages);
            statusStrip.Items.Add(new ToolStripSeparator());

            _wordWrapping = new CheckBox {BackColor = Color.Transparent, Text = @"Wrap", Checked = wordWrap, Padding = new Padding(10, 0, 0, 0)};
            _wordWrapping.CheckStateChanged += (s, e) =>
            {
                foreach (var editor in ListOfXmlEditors.Values.Where(p => p.FCTB.WordWrap != WordWrap))
                {
                    editor.FCTB.WordWrap = WordWrap;
                }
            };
            var wordWrapStatHost = new ToolStripControlHost(_wordWrapping);
            statusStrip.Items.Add(wordWrapStatHost);
            statusStrip.Items.Add(new ToolStripSeparator());

            _encodingInfo = GetStripLabel("");
            statusStrip.Items.Add(_encodingInfo);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(GetStripLabel("length:"));
            _contentLengthInfo = GetStripLabel("", 0);
            statusStrip.Items.Add(_contentLengthInfo);
            statusStrip.Items.Add(GetStripLabel("lines:"));
            _contentLinesInfo = GetStripLabel("", 0);
            statusStrip.Items.Add(_contentLinesInfo);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(GetStripLabel("Ln:"));
            _currentLineInfo = GetStripLabel("", 0);
            statusStrip.Items.Add(_currentLineInfo);
            statusStrip.Items.Add(GetStripLabel("Col:"));
            _currentPosition = GetStripLabel("");
            statusStrip.Items.Add(_currentPosition);
            statusStrip.Items.Add(GetStripLabel("Sel:"));
            _selectedInfo = GetStripLabel("");
            statusStrip.Items.Add(_selectedInfo);

            var editor2 = AddDocumentAndGetEditor(filePath);
            _listOfLanguages.Text = editor2.FCTB.Language.ToString();
            _listOfLanguages.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
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

        public void AddDocument(string filePath)
        {
            AddDocumentAndGetEditor(filePath);
        }

        Editor AddDocumentAndGetEditor(string filePath)
        {
            if (filePath.IsNullOrEmptyTrim())
                throw new ArgumentNullException(nameof(filePath));

            var editor = new Editor(filePath, WordWrap);
            Text = filePath;

            var page = new TabPage
            {
                UseVisualStyleBackColor = true,
                Text = editor.Name,
                ForeColor = Color.Green
            };
            page.Controls.Add(editor.FCTB);
            page.Margin = new Padding(0);
            page.Padding = new Padding(0);
            page.Text = page.Text + new string(' ', 2);

            int index = TabControlObj.TabPages.Count;
            TabControlObj.TabPages.Add(page);
            if(TabControlObj.TabPages.Count == index)
                TabControlObj.TabPages.Insert(index, editor.Name);
            TabControlObj.SelectedIndex = index;

            editor.FCTB.SelectionChanged += FCTB_SelectionChanged;
            editor.OnSomethingChanged += EditorOnSomethingChanged;

            ListOfXmlEditors.Add(page, editor);
            _currentEditor = editor;

            FCTB_SelectionChanged(this, null);
            return editor;
        }

        void EditorOnSomethingChanged(object sender, EventArgs args)
        {
            if (!(sender is Editor editor))
                return;

            Invoke(new MethodInvoker(delegate
            {
                var page = TabControlObj.SelectedTab;
                page.ForeColor = editor.IsContentChanged ? Color.Red : Color.Green;
                page.Text = editor.Name;
                TabControlObj_Selecting(null, null);
            }));
        }

        private void FCTB_SelectionChanged(object sender, EventArgs e)
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
            _encodingInfo.Text = _currentEditor.Encoding.EncodingName;

            //_listOfLanguages.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;
            _listOfLanguages.Text = _currentEditor.FCTB.Language.ToString();
            //_listOfLanguages.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
        }


        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private const int TCM_SETMINTABWIDTH = 0x1300 + 49;

        private void TabControlObj_HandleCreated(object sender, EventArgs e)
        {
            SendMessage(TabControlObj.Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr) 16);
        }

        private void TabControlObj_Deselected(object sender, TabControlEventArgs e)
        {
            if(e.TabPageIndex != -1 )
                _lastSelectedPage = e.TabPageIndex;
        }

        private void TabControlObj_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (TabControlObj.TabPages.Count == 0)
            {
                Close();
                return;
            }

            if (TabControlObj.SelectedTab == null)
                TabControlObj.SelectedIndex = _lastSelectedPage;

            if (TabControlObj.SelectedTab.Controls.Count == 0)
                return;

            if (!ListOfXmlEditors.TryGetValue(TabControlObj.SelectedTab, out var editor))
                return;

            _currentEditor = editor;
            Text = editor.FilePath;

            FCTB_SelectionChanged(this, null);
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
                    //this.TabControlObj.TabPages.RemoveAt(i);

                    var page = TabControlObj.TabPages[i];
                    if (page != null )
                    {
                        if (ListOfXmlEditors.TryGetValue(page, out var editor))
                        {
                            ListOfXmlEditors.Remove(page);
                            editor.OnSomethingChanged -= EditorOnSomethingChanged;
                            editor.FCTB.SelectionChanged -= FCTB_SelectionChanged;
                            editor.Dispose();
                        }

                        int prevSelectedIndex = TabControlObj.SelectedIndex;
                        TabControlObj.TabPages.Remove(page);

                        if (TabControlObj.TabPages.Count > 0)
                        {
                            if (prevSelectedIndex == i && _lastSelectedPage > i)
                            {
                                TabControlObj.SelectedIndex = _lastSelectedPage - 1;
                            }
                            else if (prevSelectedIndex == i && _lastSelectedPage < i)
                            {
                                TabControlObj.SelectedIndex = _lastSelectedPage;
                            }
                            else if (prevSelectedIndex > i)
                            {
                                TabControlObj.SelectedIndex = prevSelectedIndex - 1;
                            }
                            else if (prevSelectedIndex < i)
                            {
                                TabControlObj.SelectedIndex = prevSelectedIndex ;
                            }
                            else if(i  > 0)
                            {
                                TabControlObj.SelectedIndex = i - 1;
                            }
                        }
                    }

                    break;
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

        private void XmlNotepad_KeyDown(object sender, KeyEventArgs e)
        {
            if (ListOfXmlEditors.TryGetValue(TabControlObj.SelectedTab, out var editor))
                editor.UserTriedToSaveDocument(sender, e);
        }

        private void XmlNotepad_Closed(object sender, EventArgs e)
        {
            foreach (var editor in ListOfXmlEditors.Values)
            {
                editor.Dispose();
            }
            WindowIsClosed = true;
        }
    }
}
