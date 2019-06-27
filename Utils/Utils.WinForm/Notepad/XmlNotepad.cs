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
    public partial class XmlNotepad : Form
    {
        private readonly ToolStripLabel _contentLengthInfo;
        private readonly ToolStripLabel _contentLinesInfo;
        private readonly ToolStripLabel _currentLineInfo;
        private readonly ToolStripLabel _currentPosition;
        private readonly ToolStripLabel _selectedInfo;
        private readonly ToolStripLabel _encodingInfo;

        private XmlEditor _activatedEditor = null;
        readonly CheckBox _wordWrapping;

        Dictionary<TabPage, XmlEditor> ListOfXmlEditors { get; } = new Dictionary<TabPage, XmlEditor>();

        public bool WindowIsClosed { get; private set; } = false;

        public bool WordWrap
        {
            get => _wordWrapping.Checked;
            set => _wordWrapping.Checked = value;
        }

        public XmlNotepad(string filePath, bool wordWrap = true)
        {
            InitializeComponent();

            Closed += XmlNotepad_Closed;
            TabControlObj.Padding = new Point(12, 4);
            TabControlObj.DrawMode = TabDrawMode.OwnerDrawFixed;
            TabControlObj.DrawItem += TabControl1_DrawItem;
            TabControlObj.MouseDown += TabControl1_MouseDown;
            TabControlObj.KeyDown += XmlNotepad_KeyDown;
            KeyDown += XmlNotepad_KeyDown;
            TabControlObj.Selecting += TabControl1_Selecting;
            TabControlObj.HandleCreated += tabControl1_HandleCreated;
            TabControlObj.BackColor = Color.White;


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

            AddDocument(filePath);

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

            RefreshStatus();
        }

        static ToolStripLabel GetStripLabel(string text, int leftPadding = 0, int rightPadding = 0)
        {
            return new ToolStripLabel(text)
            {
                BackColor = Color.Transparent
                //Padding = new Padding(leftPadding, 0, rightPadding, 0)
                //Margin = new Padding(leftPadding, 0, rightPadding, 0)
            };

            //var dd = new ToolStripLabel();
            //dd.
        }

        void RefreshStatus()
        {
            if (_activatedEditor == null)
            {
                _contentLengthInfo.Text = "0";
                _contentLinesInfo.Text = "0";
                _currentLineInfo.Text = "0";
                _currentPosition.Text =  "0";
                _selectedInfo.Text = "0|0";
                _encodingInfo.Text = "";
                return;
            }

            _contentLengthInfo.Text = _activatedEditor.FCTB.TextLength.ToString();
            _contentLinesInfo.Text = _activatedEditor.FCTB.LinesCount.ToString();
            _currentLineInfo.Text = (_activatedEditor.FCTB.Selection.FromLine + 1).ToString();
            _currentPosition.Text = (_activatedEditor.FCTB.Selection.FromX + 1).ToString();
            _selectedInfo.Text = $"{_activatedEditor.FCTB.SelectedText.Length}|{(!_activatedEditor.FCTB.SelectedText.IsNullOrEmpty() ? _activatedEditor.FCTB.SelectedText.Split('\n').Length : 0)}";
            _encodingInfo.Text = _activatedEditor.Encoding.EncodingName;
        }

        public void AddDocument(string filePath)
        {
            if (filePath.IsNullOrEmptyTrim())
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new ArgumentException($"File \"{filePath}\" not found!");

            var editor = new XmlEditor(filePath, WordWrap);
            Text = filePath;

            int lastIndex;
            if (TabControlObj.TabCount == 0)
            {
                var page1 = new TabPage();
                TabControlObj.Controls.Add(page1);
                lastIndex = 0;
            }
            else
            {
                lastIndex = TabControlObj.TabCount;
            }

            TabControlObj.TabPages.Insert(lastIndex, editor.Name);
            TabControlObj.SelectedIndex = lastIndex;

            TabPage page = TabControlObj.TabPages[lastIndex];
            page.UseVisualStyleBackColor = true;
            page.Text = editor.Name;
            page.ForeColor = Color.Green;
            page.Controls.Add(editor.FCTB);
            page.Margin = new Padding(0);
            page.Padding = new Padding(0);

            editor.FCTB.SelectionChanged += FCTB_SelectionChanged;
            editor.OnSomethingChanged += EditorOnSomethingChanged;

            ListOfXmlEditors.Add(page, editor);
            _activatedEditor = editor;
        }

        private void FCTB_SelectionChanged(object sender, EventArgs e)
        {
            RefreshStatus();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private const int TCM_SETMINTABWIDTH = 0x1300 + 49;

        private void tabControl1_HandleCreated(object sender, EventArgs e)
        {
            SendMessage(TabControlObj.Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr) 16);
        }

        private void TabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (TabControlObj.TabPages.Count == 0)
            {
                Close();
                return;
            }

            if (TabControlObj.SelectedTab.Controls.Count == 0)
                return;

            if (!ListOfXmlEditors.TryGetValue(TabControlObj.SelectedTab, out var editor))
                return;

            _activatedEditor = editor;
            Text = editor.Path;
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
                        TabControlObj.TabPages.Remove(page);
                    }

                    if (TabControlObj.TabPages.Count > 0)
                        TabControlObj.SelectedIndex = i == 0 ? 0 : i - 1;
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
            e.Graphics.DrawImage(closeImage, (tabRect.Right - closeImage.Width), (tabRect.Top + (tabRect.Height - closeImage.Height) / 2) + 2);
            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font, tabRect, tabPage.ForeColor, tabPage.BackColor, TextFormatFlags.VerticalCenter);
        }

        private void XmlNotepad_KeyDown(object sender, KeyEventArgs e)
        {
            if (ListOfXmlEditors.TryGetValue(TabControlObj.SelectedTab, out var editor))
                editor.UserTriedToSaveDocument(sender, e);
        }

        void EditorOnSomethingChanged(object sender, EventArgs args)
        {
            if(!(sender is XmlEditor editor))
                return;

            Invoke(new MethodInvoker(delegate
            {
                var page = TabControlObj.SelectedTab;
                page.ForeColor = editor.IsContentChanged ? Color.Red : Color.Green;
                page.Text = editor.Name;
                TabControl1_Selecting(null, null);
            }));
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
