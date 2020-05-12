using System.ComponentModel;
using System.Windows.Forms;
using LogsReader.Properties;

namespace LogsReader.Reader.Forms
{
	partial class ServerGroupForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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
			this.labelGroup = new System.Windows.Forms.Label();
			this.comboboxGroup = new System.Windows.Forms.ComboBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonPingAll = new System.Windows.Forms.Button();
			this.buttonRemoveAll = new System.Windows.Forms.Button();
			this.buttonAdd = new System.Windows.Forms.Button();
			this.panelBottom = new System.Windows.Forms.Panel();
			this.panel1.SuspendLayout();
			this.panelBottom.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelGroup
			// 
			this.labelGroup.AutoSize = true;
			this.labelGroup.Location = new System.Drawing.Point(7, 6);
			this.labelGroup.Name = "labelGroup";
			this.labelGroup.Size = new System.Drawing.Size(36, 13);
			this.labelGroup.TabIndex = 0;
			this.labelGroup.Text = "Group";
			// 
			// comboboxGroup
			// 
			this.comboboxGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboboxGroup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.comboboxGroup.FormattingEnabled = true;
			this.comboboxGroup.Location = new System.Drawing.Point(51, 3);
			this.comboboxGroup.MaxDropDownItems = 2;
			this.comboboxGroup.Name = "comboboxGroup";
			this.comboboxGroup.Size = new System.Drawing.Size(346, 21);
			this.comboboxGroup.TabIndex = 35;
			this.comboboxGroup.TextChanged += new System.EventHandler(this.comboboxGroup_TextChanged);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Image = global::LogsReader.Properties.Resources.cancel;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(315, 3);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(80, 25);
			this.buttonCancel.TabIndex = 37;
			this.buttonCancel.Text = global::LogsReader.Properties.Resources.Txt_Forms_Cancel;
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Image = global::LogsReader.Properties.Resources.Ok;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(249, 3);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(60, 25);
			this.buttonOK.TabIndex = 38;
			this.buttonOK.Text = global::LogsReader.Properties.Resources.Txt_Forms_OK;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.comboboxGroup);
			this.panel1.Controls.Add(this.labelGroup);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(400, 26);
			this.panel1.TabIndex = 39;
			// 
			// buttonPingAll
			// 
			this.buttonPingAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonPingAll.Image = global::LogsReader.Properties.Resources.pingAll;
			this.buttonPingAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonPingAll.Location = new System.Drawing.Point(68, 3);
			this.buttonPingAll.Name = "buttonPingAll";
			this.buttonPingAll.Size = new System.Drawing.Size(80, 25);
			this.buttonPingAll.TabIndex = 42;
			this.buttonPingAll.Text = "     Ping All";
			this.buttonPingAll.UseVisualStyleBackColor = true;
			this.buttonPingAll.Click += new System.EventHandler(this.buttonPingAll_Click);
			// 
			// buttonRemoveAll
			// 
			this.buttonRemoveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonRemoveAll.Image = global::LogsReader.Properties.Resources.remove;
			this.buttonRemoveAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonRemoveAll.Location = new System.Drawing.Point(154, 3);
			this.buttonRemoveAll.Name = "buttonRemoveAll";
			this.buttonRemoveAll.Size = new System.Drawing.Size(89, 25);
			this.buttonRemoveAll.TabIndex = 41;
			this.buttonRemoveAll.Text = global::LogsReader.Properties.Resources.Txt_Forms_RemoveAll;
			this.buttonRemoveAll.UseVisualStyleBackColor = true;
			this.buttonRemoveAll.Click += new System.EventHandler(this.buttonRemoveAll_Click);
			// 
			// buttonAdd
			// 
			this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdd.Image = global::LogsReader.Properties.Resources.add;
			this.buttonAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonAdd.Location = new System.Drawing.Point(4, 3);
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.Size = new System.Drawing.Size(58, 25);
			this.buttonAdd.TabIndex = 43;
			this.buttonAdd.Text = global::LogsReader.Properties.Resources.Txt_Forms_Add;
			this.buttonAdd.UseVisualStyleBackColor = true;
			this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
			// 
			// panelBottom
			// 
			this.panelBottom.Controls.Add(this.buttonPingAll);
			this.panelBottom.Controls.Add(this.buttonAdd);
			this.panelBottom.Controls.Add(this.buttonCancel);
			this.panelBottom.Controls.Add(this.buttonOK);
			this.panelBottom.Controls.Add(this.buttonRemoveAll);
			this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelBottom.Location = new System.Drawing.Point(0, 55);
			this.panelBottom.Name = "panelBottom";
			this.panelBottom.Size = new System.Drawing.Size(400, 30);
			this.panelBottom.TabIndex = 44;
			// 
			// ServerGroupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(400, 85);
			this.Controls.Add(this.panelBottom);
			this.Controls.Add(this.panel1);
			this.MinimumSize = new System.Drawing.Size(416, 124);
			this.Name = "ServerGroupForm";
			this.Text = "Server Group";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panelBottom.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Label labelGroup;
		private ComboBox comboboxGroup;
		private Button buttonCancel;
		private Button buttonOK;
		private Panel panel1;
		private Button buttonPingAll;
		private Button buttonRemoveAll;
		private Button buttonAdd;
		private Panel panelBottom;
	}
}