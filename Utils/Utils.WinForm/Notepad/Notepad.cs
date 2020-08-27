using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils.WinForm.Properties;

namespace Utils.WinForm.Notepad
{
    public partial class Notepad : Form
    {
        private int _numberOfNewDocument = 0;

        public bool WindowIsClosed { get; private set; } = false;

        [Browsable(false)]
        public Editor CurrentEditor => NotepadControlItem.CurrentEditor;

        [Browsable(false)]
        public Encoding DefaultEncoding
        {
            get => NotepadControlItem.DefaultEncoding;
            set => NotepadControlItem.DefaultEncoding = value;
        }

        public bool WordWrap
        {
            get => NotepadControlItem.WordWrap;
            set => NotepadControlItem.WordWrap = value;
        }

        public bool Highlights
        {
            get => NotepadControlItem.Highlights;
            set => NotepadControlItem.Highlights = value;
        }

        public bool ReadOnly
        {
            get => NotepadControlItem.ReadOnly;
            set => NotepadControlItem.ReadOnly = value;
        }

        public bool SizingGrip
        {
            get => NotepadControlItem.SizingGrip;
            set => NotepadControlItem.SizingGrip = value;
        }

        public Font TextFont
        {
            get => NotepadControlItem.TextFont;
            set => NotepadControlItem.TextFont = value;
        }

        public Color TextForeColor
        {
            get => NotepadControlItem.TextForeColor;
            set => NotepadControlItem.TextForeColor = value;
        }

        [Browsable(false)]
        public int SelectedIndex
        {
            get => NotepadControlItem.SelectedIndex;
            set => NotepadControlItem.SelectedIndex = value;
        }

        public Font TabsFont
        {
            get => NotepadControlItem.TabsFont;
            set => NotepadControlItem.TabsFont = value;
        }

        public Color TabsForeColor
        {
            get => NotepadControlItem.TabsForeColor;
            set => NotepadControlItem.TabsForeColor = value;
        }

        public bool AllowUserCloseItems
        {
            get => NotepadControlItem.AllowUserCloseItems;
            set => NotepadControlItem.AllowUserCloseItems = value;
        }

        private readonly ToolStripMenuItem fileToolStripMenuItem;
        private readonly ToolStripMenuItem saveToolStripMenuItem;
        private readonly ToolStripMenuItem saveAsToolStripMenuItem;
        private readonly ToolStripMenuItem newToolStripMenuItem;
        private readonly ToolStripMenuItem openToolStripMenuItem;
        private readonly ToolStripMenuItem closeToolStripMenuItem;
        private readonly ToolStripMenuItem formatXmlF5ToolStripMenuItem;
        private NotepadControl NotepadControlItem;

        public Notepad()
        {
            InitializeComponent();

            KeyPreview = true; // для того чтобы работали горячие клавиши по всей форме и всем контролам

            // 
            // NotepadControlItem
            // 
            NotepadControlItem = new NotepadControl
            {
	            AllowUserCloseItems = false,
	            Dock = DockStyle.Fill,
	            Highlights = false,
	            Location = new Point(0, 0),
	            Name = "NotepadControlItem",
	            ReadOnly = false,
	            SelectedIndex = -1,
	            Size = new Size(1035, 648),
	            SizingGrip = false,
	            TabIndex = 3,
	            TabsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 204),
	            TabsForeColor = Color.Green,
	            TextFont = new Font("Segoe UI", 9F),
	            TextForeColor = Color.Black,
	            WordWrap = true
            };

            fileToolStripMenuItem = new ToolStripMenuItem();
            newToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            formatXmlF5ToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            closeToolStripMenuItem = new ToolStripMenuItem();

            // 
            // MainMenuStrip
            // 
            MainMenuStrip = new MenuStrip
            {
	            AccessibleRole = AccessibleRole.None,
	            BackColor = Color.White,
	            LayoutStyle = ToolStripLayoutStyle.Flow,
	            Location = new Point(0, 0),
	            Name = "MainMenuStrip",
	            RenderMode = ToolStripRenderMode.Professional,
	            Size = new Size(1035, 23),
	            TabIndex = 2,
	            Text = "mainMenuStrip"
            };
            MainMenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            Controls.Add(NotepadControlItem);
            Controls.Add(MainMenuStrip);

            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            newToolStripMenuItem,
            openToolStripMenuItem,
            new ToolStripSeparator(),
            formatXmlF5ToolStripMenuItem,
            new ToolStripSeparator(),
            saveToolStripMenuItem,
            saveAsToolStripMenuItem,
            new ToolStripSeparator(),
            closeToolStripMenuItem});
            fileToolStripMenuItem.Font = new Font("Segoe UI", 8F);
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 17);
            fileToolStripMenuItem.Text = Resources.Notepad_File;
            fileToolStripMenuItem.Click += fileToolStripMenuItem_Click;
            fileToolStripMenuItem.DropDownItemClicked += FileToolStripMenuItem_DropDownItemClicked;
            // 
            // newToolStripMenuItem
            // 
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.Size = new Size(205, 22);
            newToolStripMenuItem.Text = Resources.Notepad_New;
            newToolStripMenuItem.Image = Resources.notepad_newItem;
            newToolStripMenuItem.TextAlign = ContentAlignment.MiddleRight;
            newToolStripMenuItem.TextDirection = ToolStripTextDirection.Horizontal;
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(205, 22);
            openToolStripMenuItem.Text = Resources.Notepad_Open;
            openToolStripMenuItem.Image = Resources.notepad_openFile;
            // 
            // formatXmlF5ToolStripMenuItem
            // 
            formatXmlF5ToolStripMenuItem.Name = "formatXmlF5ToolStripMenuItem";
            formatXmlF5ToolStripMenuItem.Size = new Size(205, 22);
            formatXmlF5ToolStripMenuItem.Text = "XML Pretty Print     F5";
            formatXmlF5ToolStripMenuItem.Image = Resources.notepad_XMLPretty;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(205, 22);
            saveToolStripMenuItem.Text = Resources.Notepad_Save;
            saveToolStripMenuItem.Image = Resources.notepad_saveItem;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new Size(205, 22);
            saveAsToolStripMenuItem.Text = Resources.Notepad_SaveAs;
            saveAsToolStripMenuItem.Image = Resources.notepad_saveItemAs;
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new Size(205, 22);
            closeToolStripMenuItem.Text = Resources.Notepad_CLose;
            closeToolStripMenuItem.Image = Resources.notepad_closeWindow;

            Closed += XmlNotepad_Closed;
            KeyDown += Notepad_KeyDown;

            NotepadControlItem.OnRefresh += NotepadControlItem_OnRefresh;
            NotepadControlItem.Dock = DockStyle.Fill;
        }

        private void NotepadControlItem_OnRefresh(object sender, EventArgs e)
        {
            if (NotepadControlItem.Current == null)
            {
                Text = nameof(Notepad);
            }
            else
            {
                Text = NotepadControlItem.Current?.Key is FileEditor fileEditor ? fileEditor.FilePath : NotepadControlItem.Current?.Key.HeaderName;
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
            await Task.Factory.StartNew(() =>
            {
                this.SafeInvoke(() => { PerformCommand(e.ClickedItem); });
            });
        }

        void PerformCommand(ToolStripItem item)
        {
            if (item == newToolStripMenuItem)
            {
                NotepadControlItem.AddDocument(new BlankDocument(){ HeaderName = $"new {++_numberOfNewDocument}" });
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
                NotepadControlItem.Current?.Key.PrintXml();
            }
            else if (item == closeToolStripMenuItem)
            {
                Close();
            }
            else if (NotepadControlItem.Current != null)
            {
                if (NotepadControlItem.Current?.Key is FileEditor fileEditor)
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
                    var newFileEditor = FileEditor.ConvertToFileEditor(NotepadControlItem.Current?.Key, NotepadControlItem.DefaultEncoding);
                    if (SaveAsFileEditor(newFileEditor))
                    {
                        NotepadControlItem.ReplaceEditor(NotepadControlItem.Current?.Key, newFileEditor);
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
        /// Добавить текстовый контент
        /// </summary>
        /// <param name="documentList"></param>
        public async Task AddDocumentListAsync(IEnumerable<BlankDocument> documentList)
        {
            await NotepadControlItem.AddDocumentListAsync(documentList);
        }

        /// <summary>
        /// Добавить текстовый контент
        /// </summary>
        /// <param name="documentList"></param>
        public void AddDocumentList(IEnumerable<BlankDocument> documentList)
        {
            NotepadControlItem.AddDocumentList(documentList);
        }

        /// <summary>
        /// Добавить текстовый контент
        /// </summary>
        /// <param name="document"></param>
        public async Task AddDocumentAsync(BlankDocument document)
        {
            await NotepadControlItem.AddDocumentAsync(document);
        }

        /// <summary>
        /// Добавить текстовый контент
        /// </summary>
        /// <param name="document"></param>
        public void AddDocument(BlankDocument document)
        {
            NotepadControlItem.AddDocument(document);
        }


        /// <summary>
        /// Добавить фаловый документ
        /// </summary>
        /// <param name="filePathList"></param>
        public async Task AddFileDocumentListAsync(IEnumerable<string> filePathList)
        {
            await NotepadControlItem.AddFileDocumentListAsync(filePathList);
        }

        /// <summary>
        /// Добавить фаловый документ
        /// </summary>
        /// <param name="filePathList"></param>
        public void AddFileDocumentList(IEnumerable<string> filePathList)
        {
            NotepadControlItem.AddFileDocumentList(filePathList);
        }

        /// <summary>
        /// Добавить фаловый документ
        /// </summary>
        /// <param name="filePath"></param>
        public async Task AddFileDocumentAsync(string filePath)
        {
            await NotepadControlItem.AddFileDocumentAsync(filePath);
        }

        /// <summary>
        /// Добавить фаловый документ
        /// </summary>
        /// <param name="filePath"></param>
        public void AddFileDocument(string filePath)
        {
            NotepadControlItem.AddFileDocument(filePath);
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

        public new void SuspendLayout()
        {
	        base.SuspendLayout();
	        NotepadControlItem?.SuspendLayout();
        }

        public new void ResumeLayout()
        {
	        NotepadControlItem?.ResumeLayout();
            base.ResumeLayout();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileToolStripMenuItem.Text = Resources.Notepad_File;
            newToolStripMenuItem.Text = Resources.Notepad_New;
            openToolStripMenuItem.Text = Resources.Notepad_Open;
            saveToolStripMenuItem.Text = Resources.Notepad_Save;
            saveAsToolStripMenuItem.Text = Resources.Notepad_SaveAs;
            closeToolStripMenuItem.Text = Resources.Notepad_CLose;
        }
    }
}