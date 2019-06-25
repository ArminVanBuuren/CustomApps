using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Utils.WinForm.Notepad
{
    public partial class XmlNotepad : Form
    {
        readonly List<XmlEditor> _listOfXmlFiles = new List<XmlEditor>();

        public bool WindowIsClosed { get; private set; } = false;
        

        public XmlNotepad(string filePath)
        {
            InitializeComponent();

            Closed += XmlNotepad_Closed;

            TabControlObj.Padding = new Point(12, 4);
            TabControlObj.DrawMode = TabDrawMode.OwnerDrawFixed;
            TabControlObj.DrawItem += TabControl1_DrawItem;

            TabControlObj.MouseDown += TabControl1_MouseDown;

            //button1.KeyDown += XmlNotepad_KeyDown;
            TabControlObj.KeyDown += XmlNotepad_KeyDown;
            KeyDown += XmlNotepad_KeyDown;

            TabControlObj.Selecting += TabControl1_Selecting;
            TabControlObj.HandleCreated += tabControl1_HandleCreated;
            TabControlObj.BackColor = Color.White;

            AddDocument(filePath);
        }

        public void AddDocument(string filePath)
        {
            if (filePath.IsNullOrEmptyTrim())
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new ArgumentException($"File \"{filePath}\" not found!");

            var editor = new XmlEditor(filePath);

            Text = filePath;
            _listOfXmlFiles.Add(editor);

            int lastIndex;
            if (TabControlObj.TabCount == 0)
            {
                TabPage page1 = new TabPage();
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

            editor.OnSomethingChanged += (sender, args) =>
            {
                Invoke(new MethodInvoker(delegate
                {
                    page.ForeColor = editor.IsContentChanged ? Color.Red : Color.Green;
                    page.Text = editor.Name;
                    TabControl1_Selecting(null, null);
                }));
            };
        }

        private void XmlNotepad_Closed(object sender, EventArgs e)
        {
            foreach (XmlEditor editor in _listOfXmlFiles)
            {
                editor.Dispose();
            }
            WindowIsClosed = true;
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

            if (GetCurrentEditor(out var editor))
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
                    TabControlObj.TabPages.Remove(TabControlObj.TabPages[i]);
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
            if (GetCurrentEditor(out var editor))
                editor.UserTriedToSaveDocument(sender, e);
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    if (GetCurrentEditor(out var editor))
        //        editor.XMLPrint();
        //}

        bool GetCurrentEditor(out XmlEditor editor)
        {
            editor = _listOfXmlFiles.First(p => p.FCTB == TabControlObj.SelectedTab.Controls[0]);
            return editor != null;
        }
    }
}
