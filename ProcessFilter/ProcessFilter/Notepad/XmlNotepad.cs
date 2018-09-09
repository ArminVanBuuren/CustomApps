﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ProcessFilter.Notepad
{
    public partial class XmlNotepad : Form
    {
        public bool WindowIsClosed { get; private set; } = false;
        List<XmlEditor> listOfXmlFiles = new List<XmlEditor>();


        public XmlNotepad(string path)
        {
            InitializeComponent();

            Closed += XmlNotepad_Closed;

            this.TabControlObj.Padding = new Point(12, 4);
            this.TabControlObj.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.TabControlObj.DrawItem += tabControl1_DrawItem;

            this.TabControlObj.MouseDown += tabControl1_MouseDown;


            this.TabControlObj.Selecting += tabControl1_Selecting;
            this.TabControlObj.HandleCreated += tabControl1_HandleCreated;
            TabControlObj.BackColor = Color.White;

            

            if (!AddNewDocument(path))
            {
                Close();
            }
        }

        private void XmlNotepad_Closed(object sender, EventArgs e)
        {
            WindowIsClosed = true;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private const int TCM_SETMINTABWIDTH = 0x1300 + 49;

        private void tabControl1_HandleCreated(object sender, EventArgs e)
        {
            SendMessage(this.TabControlObj.Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr) 16);
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (TabControlObj.TabPages.Count == 0)
            {
                Close();
                return;
            }

            if (TabControlObj.SelectedTab.Controls.Count == 0)
                return;

            XmlEditor editor = listOfXmlFiles.First(p => p.FCTextBox == TabControlObj.SelectedTab.Controls[0]);
            Text = editor.Path;

        }

        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            for (var i = 0; i < this.TabControlObj.TabPages.Count; i++)
            {
                var tabRect = this.TabControlObj.GetTabRect(i);
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
                    this.TabControlObj.TabPages.Remove(TabControlObj.TabPages[i]);
                    if (this.TabControlObj.TabPages.Count > 0)
                        this.TabControlObj.SelectedIndex = i == 0 ? 0 : i - 1;
                    break;
                }
            }
        }



        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tabPage = this.TabControlObj.TabPages[e.Index];
            var tabRect = this.TabControlObj.GetTabRect(e.Index);
            tabRect.Inflate(-2, -2);

            var closeImage = Properties.Resources.Close;
            e.Graphics.DrawImage(closeImage, (tabRect.Right - closeImage.Width), (tabRect.Top + (tabRect.Height - closeImage.Height) / 2) + 2);
            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font, tabRect, tabPage.ForeColor, tabPage.BackColor, TextFormatFlags.VerticalCenter);
        }


        public bool AddNewDocument(string path)
        {
            XmlEditor editor = new XmlEditor();
            if (editor.Load(path))
            {
                Text = path;
                listOfXmlFiles.Add(editor);

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
                page.Controls.Add(editor.FCTextBox);
                page.Margin = new Padding(0);
                page.Padding = new Padding(0);

                editor.FCTextBox.TextChanged += (sender, args) =>
                {
                    ChangePageColor(page, editor);
                };
                editor.OnSomethingChanged += (sender, args) =>
                {
                    ChangePageColor(page, editor);
                };
                return true;
            }

            MessageBox.Show($"File \"{path}\" is incorrect or not found!");
            return false;

        }

        void ChangePageColor(TabPage page, XmlEditor editor)
        {
            Invoke(new MethodInvoker(delegate
            {
                page.ForeColor = editor.IsContentChanged ? Color.Red : Color.Green;
                page.Text = editor.Name;
                tabControl1_Selecting(null, null);
            }));
        }
    }
}
