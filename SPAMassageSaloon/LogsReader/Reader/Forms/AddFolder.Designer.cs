﻿using LogsReader.Properties;

namespace LogsReader.Reader.Forms
{
	partial class AddFolder
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
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxFolder = new System.Windows.Forms.TextBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonOpenFolder = new System.Windows.Forms.Button();
			this.checkBoxAllDirectories = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(4, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(36, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Folder";
			// 
			// textBoxFolder
			// 
			this.textBoxFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxFolder.Location = new System.Drawing.Point(46, 5);
			this.textBoxFolder.Name = "textBoxFolder";
			this.textBoxFolder.Size = new System.Drawing.Size(342, 20);
			this.textBoxFolder.TabIndex = 1;
			this.textBoxFolder.TextChanged += new System.EventHandler(this.textBoxFolder_TextChanged);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Image = global::LogsReader.Properties.Resources.cancel;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(309, 29);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(80, 25);
			this.buttonCancel.TabIndex = 39;
			this.buttonCancel.Text = global::LogsReader.Properties.Resources.Txt_Forms_Cancel;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Image = global::LogsReader.Properties.Resources.Ok;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(243, 29);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(60, 25);
			this.buttonOK.TabIndex = 40;
			this.buttonOK.Text = global::LogsReader.Properties.Resources.Txt_Forms_OK;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonOpenFolder
			// 
			this.buttonOpenFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOpenFolder.BackColor = System.Drawing.Color.Transparent;
			this.buttonOpenFolder.Image = global::LogsReader.Properties.Resources.folder;
			this.buttonOpenFolder.Location = new System.Drawing.Point(365, 6);
			this.buttonOpenFolder.Name = "buttonOpenFolder";
			this.buttonOpenFolder.Size = new System.Drawing.Size(22, 18);
			this.buttonOpenFolder.TabIndex = 41;
			this.buttonOpenFolder.Text = "..";
			this.buttonOpenFolder.UseVisualStyleBackColor = false;
			this.buttonOpenFolder.Click += new System.EventHandler(this.buttonOpenFolder_Click);
			// 
			// checkBoxAllDirectories
			// 
			this.checkBoxAllDirectories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxAllDirectories.AutoSize = true;
			this.checkBoxAllDirectories.Checked = true;
			this.checkBoxAllDirectories.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAllDirectories.Location = new System.Drawing.Point(149, 32);
			this.checkBoxAllDirectories.Name = "checkBoxAllDirectories";
			this.checkBoxAllDirectories.Size = new System.Drawing.Size(88, 17);
			this.checkBoxAllDirectories.TabIndex = 42;
			this.checkBoxAllDirectories.Text = "All directories";
			this.checkBoxAllDirectories.UseVisualStyleBackColor = true;
			// 
			// AddFolder
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(395, 56);
			this.Controls.Add(this.checkBoxAllDirectories);
			this.Controls.Add(this.buttonOpenFolder);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.textBoxFolder);
			this.Controls.Add(this.label1);
			this.MaximumSize = new System.Drawing.Size(999, 95);
			this.MinimumSize = new System.Drawing.Size(244, 95);
			this.Name = "AddFolder";
			this.Text = "Add Folder";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxFolder;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonOpenFolder;
		private System.Windows.Forms.CheckBox checkBoxAllDirectories;
	}
}