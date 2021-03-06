namespace SPAMassageSaloon
{
    partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.toolButtonAbout = new System.Windows.Forms.ToolStripSplitButton();
			this.languageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.englishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.russianToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripLogsReaderButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSpaFilterButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripXPathButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripRegexTester = new System.Windows.Forms.ToolStripButton();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip
			// 
			this.statusStrip.Location = new System.Drawing.Point(0, 617);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(1127, 22);
			this.statusStrip.TabIndex = 0;
			this.statusStrip.Text = "statusStrip1";
			// 
			// toolStrip
			// 
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolButtonAbout,
            this.toolStripLogsReaderButton,
            this.toolStripSpaFilterButton,
            this.toolStripXPathButton,
            this.toolStripRegexTester});
			this.toolStrip.Location = new System.Drawing.Point(0, 0);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size(1127, 25);
			this.toolStrip.TabIndex = 1;
			this.toolStrip.Text = "toolStrip1";
			// 
			// toolButtonAbout
			// 
			this.toolButtonAbout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolButtonAbout.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.languageToolStripMenuItem,
            this.aboutToolStripMenuItem});
			this.toolButtonAbout.Image = global::SPAMassageSaloon.Properties.Resources.question;
			this.toolButtonAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolButtonAbout.Name = "toolButtonAbout";
			this.toolButtonAbout.Size = new System.Drawing.Size(32, 22);
			this.toolButtonAbout.ButtonClick += new System.EventHandler(this.toolButtonAbout_ButtonClick);
			// 
			// languageToolStripMenuItem
			// 
			this.languageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.englishToolStripMenuItem,
            this.russianToolStripMenuItem});
			this.languageToolStripMenuItem.Image = global::SPAMassageSaloon.Properties.Resources.language;
			this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
			this.languageToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
			this.languageToolStripMenuItem.Text = "Language";
			// 
			// englishToolStripMenuItem
			// 
			this.englishToolStripMenuItem.Name = "englishToolStripMenuItem";
			this.englishToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
			this.englishToolStripMenuItem.Text = "English";
			this.englishToolStripMenuItem.Click += new System.EventHandler(this.englishToolStripMenuItem_Click);
			// 
			// russianToolStripMenuItem
			// 
			this.russianToolStripMenuItem.Name = "russianToolStripMenuItem";
			this.russianToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
			this.russianToolStripMenuItem.Text = "Русский";
			this.russianToolStripMenuItem.Click += new System.EventHandler(this.russianToolStripMenuItem_Click);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Image = global::SPAMassageSaloon.Properties.Resources.info;
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
			this.aboutToolStripMenuItem.Text = "About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// toolStripLogsReaderButton
			// 
			this.toolStripLogsReaderButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripLogsReaderButton.Image = global::SPAMassageSaloon.Properties.Resources.iconLogsReader;
			this.toolStripLogsReaderButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripLogsReaderButton.Name = "toolStripLogsReaderButton";
			this.toolStripLogsReaderButton.Size = new System.Drawing.Size(23, 22);
			this.toolStripLogsReaderButton.Text = "Logs Reader";
			this.toolStripLogsReaderButton.Click += new System.EventHandler(this.toolStripLogsReaderButton_Click);
			// 
			// toolStripSpaFilterButton
			// 
			this.toolStripSpaFilterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripSpaFilterButton.Image = global::SPAMassageSaloon.Properties.Resources.iconsSpaFilter;
			this.toolStripSpaFilterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSpaFilterButton.Name = "toolStripSpaFilterButton";
			this.toolStripSpaFilterButton.Size = new System.Drawing.Size(23, 22);
			this.toolStripSpaFilterButton.Text = "SPA Filter";
			this.toolStripSpaFilterButton.Click += new System.EventHandler(this.toolStripSpaFilterButton_Click);
			// 
			// toolStripXPathButton
			// 
			this.toolStripXPathButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripXPathButton.Image = global::SPAMassageSaloon.Properties.Resources.iconsXpath;
			this.toolStripXPathButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripXPathButton.Name = "toolStripXPathButton";
			this.toolStripXPathButton.Size = new System.Drawing.Size(23, 22);
			this.toolStripXPathButton.Text = "XPath Tester";
			this.toolStripXPathButton.Click += new System.EventHandler(this.toolStripXPathButton_Click);
			// 
			// toolStripRegexTester
			// 
			this.toolStripRegexTester.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripRegexTester.Image = ((System.Drawing.Image)(resources.GetObject("toolStripRegexTester.Image")));
			this.toolStripRegexTester.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripRegexTester.Name = "toolStripRegexTester";
			this.toolStripRegexTester.Size = new System.Drawing.Size(23, 22);
			this.toolStripRegexTester.Text = "RegEx Tester";
			this.toolStripRegexTester.Click += new System.EventHandler(this.toolStripRegexTester_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(1127, 639);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.statusStrip);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainForm";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripSplitButton toolButtonAbout;
        private System.Windows.Forms.ToolStripMenuItem languageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem englishToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem russianToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripLogsReaderButton;
        private System.Windows.Forms.ToolStripButton toolStripSpaFilterButton;
        private System.Windows.Forms.ToolStripButton toolStripXPathButton;
		private System.Windows.Forms.ToolStripButton toolStripRegexTester;
	}
}

