using System.Windows.Forms;
using Utils.WinForm.Properties;

namespace Utils.WinForm.Notepad
{
    partial class Notepad
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Notepad));
            this.MainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.formatXmlF5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NotepadControlItem = new Utils.WinForm.Notepad.NotepadControl();
            this.MainMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainMenuStrip
            // 
            this.MainMenuStrip.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.MainMenuStrip.BackColor = System.Drawing.Color.White;
            this.MainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.MainMenuStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.MainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MainMenuStrip.Name = "MainMenuStrip";
            this.MainMenuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.MainMenuStrip.Size = new System.Drawing.Size(1035, 23);
            this.MainMenuStrip.TabIndex = 2;
            this.MainMenuStrip.Text = "mainMenuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            new ToolStripSeparator(),
            this.formatXmlF5ToolStripMenuItem,
            new ToolStripSeparator(),
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            new ToolStripSeparator(), 
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 17);
            this.fileToolStripMenuItem.Text = Resources.Notepad_File;
            this.fileToolStripMenuItem.Click += new System.EventHandler(this.fileToolStripMenuItem_Click);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.newToolStripMenuItem.Text = Resources.Notepad_New;
            this.newToolStripMenuItem.Image = Resources.notepad_newItem;
            this.newToolStripMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.newToolStripMenuItem.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.openToolStripMenuItem.Text = Resources.Notepad_Open;
            this.openToolStripMenuItem.Image = Resources.notepad_openFile;
            // 
            // formatXmlF5ToolStripMenuItem
            // 
            this.formatXmlF5ToolStripMenuItem.Name = "formatXmlF5ToolStripMenuItem";
            this.formatXmlF5ToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.formatXmlF5ToolStripMenuItem.Text = "XML Pretty Print     F5";
            this.formatXmlF5ToolStripMenuItem.Image = Resources.notepad_XMLPretty;
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.saveToolStripMenuItem.Text = Resources.Notepad_Save;
            this.saveToolStripMenuItem.Image = Resources.notepad_saveItem;
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.saveAsToolStripMenuItem.Text = Resources.Notepad_SaveAs;
            this.saveAsToolStripMenuItem.Image = Resources.notepad_saveItemAs;
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.closeToolStripMenuItem.Text = Resources.Notepad_CLose;
            this.closeToolStripMenuItem.Image = Resources.notepad_closeWindow;
            // 
            // NotepadControlItem
            // 
            this.NotepadControlItem.AllowUserCloseItems = false;
            this.NotepadControlItem.DefaultEncoding = ((System.Text.Encoding)(resources.GetObject("NotepadControlItem.DefaultEncoding")));
            this.NotepadControlItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NotepadControlItem.Highlights = false;
            this.NotepadControlItem.Location = new System.Drawing.Point(0, 23);
            this.NotepadControlItem.Name = "NotepadControlItem";
            this.NotepadControlItem.ReadOnly = false;
            this.NotepadControlItem.SelectedIndex = -1;
            this.NotepadControlItem.Size = new System.Drawing.Size(1035, 625);
            this.NotepadControlItem.SizingGrip = false;
            this.NotepadControlItem.TabIndex = 3;
            this.NotepadControlItem.TabsFont = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.NotepadControlItem.TabsForeColor = System.Drawing.Color.Green;
            this.NotepadControlItem.TextFont = new System.Drawing.Font("Segoe UI", 9F);
            this.NotepadControlItem.TextForeColor = System.Drawing.Color.Black;
            this.NotepadControlItem.WordWrap = true;
            // 
            // Notepad
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(1035, 648);
            this.Controls.Add(this.NotepadControlItem);
            this.Controls.Add(this.MainMenuStrip);
            this.Icon = global::Utils.WinForm.Properties.Resources.note;
            this.Name = "Notepad";
            this.Text = "Notepad";
            this.MainMenuStrip.ResumeLayout(false);
            this.MainMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private NotepadControl NotepadControlItem;
        //private System.Windows.Forms.MenuStrip MainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem formatXmlF5ToolStripMenuItem;
    }
}