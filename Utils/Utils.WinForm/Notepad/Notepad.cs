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
        private int _numberOfNewDocument = 0;

        public bool WindowIsClosed { get; private set; } = false;

        public bool WordWrap
        {
            get => NotepadControlItem.WordWrap;
            set => NotepadControlItem.WordWrap = value;
        }

        public bool WordHighlights
        {
            get => NotepadControlItem.WordHighlights;
            set => NotepadControlItem.WordHighlights = value;
        }

        public bool SizingGrip
        {
            get => NotepadControlItem.SizingGrip;
            set => NotepadControlItem.SizingGrip = value;
        }

        public bool UserCanCloseTabItem
        {
            get => NotepadControlItem.UserCanCloseTabItem;
            set => NotepadControlItem.UserCanCloseTabItem = value;
        }

        public Notepad()
        {
            InitializeComponent();

            KeyPreview = true; // для того чтобы работали горячие клавиши по всей форме и всем контролам

            fileToolStripMenuItem.DropDownItemClicked += FileToolStripMenuItem_DropDownItemClicked;

            Closed += XmlNotepad_Closed;
            KeyDown += Notepad_KeyDown;

            NotepadControlItem.OnRefresh += NotepadControlItem_OnRefresh;
        }

        private void NotepadControlItem_OnRefresh(object sender, EventArgs e)
        {
            if (NotepadControlItem.Current == null)
            {
                Text = nameof(Notepad);
            }
            else
            {
                Text = NotepadControlItem.Current is FileEditor fileEditor ? fileEditor.FilePath : NotepadControlItem.Current.HeaderName;
            }
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
            else if (e.KeyCode == Keys.F4 && (ModifierKeys & Keys.Alt) != 0)
            {
                Close();
            }
        }

        private async void FileToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            await Task.Factory.StartNew(() => Invoke(new MethodInvoker(delegate { PerformCommand(e.ClickedItem); })));
        }

        void PerformCommand(ToolStripItem item)
        {
            if (item == newToolStripMenuItem)
            {
                NotepadControlItem.AddDocument($"new {++_numberOfNewDocument}", string.Empty);
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
            else if (item == formatXmlF5ToolStripMenuItem)
            {
                NotepadControlItem.Current?.PrintXml();
            }
            else if (item == closeToolStripMenuItem)
            {
                Close();
            }
            else if (NotepadControlItem.Current != null)
            {
                if (NotepadControlItem.Current is FileEditor fileEditor)
                {
                    if (item == saveToolStripMenuItem)
                    {
                        fileEditor.SaveDocument();
                    }
                    else if (item == saveAsToolStripMenuItem)
                    {
                        SaveAsFileEditor(fileEditor);
                    }
                }
                else
                {
                    var newFileEditor = FileEditor.ConvertToFileEditor(NotepadControlItem.Current, NotepadControlItem.DefaultEncoding);
                    if(SaveAsFileEditor(newFileEditor))
                    {
                        NotepadControlItem.ReplaceEditor(NotepadControlItem.Current, newFileEditor);
                    }
                }
            }
        }

        static bool SaveAsFileEditor(FileEditor fileEditor)
        {
            string fileDestination;
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = fileEditor.GetFileFilter();
                if (sfd.ShowDialog() != DialogResult.OK)
                    return false;

                fileDestination = sfd.FileName;
            }

            if (!fileDestination.IsNullOrEmpty())
            {
                fileEditor.SaveDocument(fileDestination);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Добавить фаловый документ
        /// </summary>
        /// <param name="filePath"></param>
        public void AddFileDocument(string filePath)
        {
            NotepadControlItem.AddDocument(filePath);
        }

        /// <summary>
        /// Добавить текстовый контент
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="bodyText"></param>
        /// <param name="language"></param>
        public void AddDocument(string headerName, string bodyText, Language language = Language.Custom)
        {
            NotepadControlItem.AddDocument(headerName, bodyText, language);
        }

        public new void CenterToScreen()
        {
            base.CenterToScreen();
        }

        public new void Focus()
        {
            NotepadControlItem.Focus();
        }

        private void XmlNotepad_Closed(object sender, EventArgs e)
        {
            NotepadControlItem.Clear();
            WindowIsClosed = true;
        }
    }
}
