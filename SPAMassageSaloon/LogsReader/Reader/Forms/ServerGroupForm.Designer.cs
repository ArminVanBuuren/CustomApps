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
			this.panelChooseGroup = new System.Windows.Forms.Panel();
			this.textBoxGroupPriority = new System.Windows.Forms.TextBox();
			this.labelPriority = new System.Windows.Forms.Label();
			this.buttonPingAll = new System.Windows.Forms.Button();
			this.buttonRemoveAll = new System.Windows.Forms.Button();
			this.buttonAdd = new System.Windows.Forms.Button();
			this.panelBottom = new System.Windows.Forms.Panel();
			this.groupBoxServers = new System.Windows.Forms.GroupBox();
			this.panelChooseGroup.SuspendLayout();
			this.panelBottom.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelGroup
			// 
			this.labelGroup.AutoSize = true;
			this.labelGroup.Location = new System.Drawing.Point(3, 6);
			this.labelGroup.Name = "labelGroup";
			this.labelGroup.Size = new System.Drawing.Size(67, 13);
			this.labelGroup.TabIndex = 0;
			this.labelGroup.Text = "Group Name";
			// 
			// comboboxGroup
			// 
			this.comboboxGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboboxGroup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.comboboxGroup.FormattingEnabled = true;
			this.comboboxGroup.Location = new System.Drawing.Point(76, 3);
			this.comboboxGroup.MaxDropDownItems = 2;
			this.comboboxGroup.Name = "comboboxGroup";
			this.comboboxGroup.Size = new System.Drawing.Size(344, 21);
			this.comboboxGroup.TabIndex = 6;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Image = global::LogsReader.Properties.Resources.cancel;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(343, 2);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(80, 25);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = global::LogsReader.Properties.Resources.Txt_Forms_Cancel;
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Image = global::LogsReader.Properties.Resources.Ok;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(277, 2);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(60, 25);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.Text = global::LogsReader.Properties.Resources.Txt_Forms_OK;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// panelChooseGroup
			// 
			this.panelChooseGroup.Controls.Add(this.textBoxGroupPriority);
			this.panelChooseGroup.Controls.Add(this.labelPriority);
			this.panelChooseGroup.Controls.Add(this.comboboxGroup);
			this.panelChooseGroup.Controls.Add(this.labelGroup);
			this.panelChooseGroup.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelChooseGroup.Location = new System.Drawing.Point(0, 0);
			this.panelChooseGroup.Name = "panelChooseGroup";
			this.panelChooseGroup.Size = new System.Drawing.Size(426, 56);
			this.panelChooseGroup.TabIndex = 39;
			// 
			// textBoxGroupPriority
			// 
			this.textBoxGroupPriority.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxGroupPriority.Location = new System.Drawing.Point(76, 29);
			this.textBoxGroupPriority.Name = "textBoxGroupPriority";
			this.textBoxGroupPriority.Size = new System.Drawing.Size(344, 20);
			this.textBoxGroupPriority.TabIndex = 7;
			this.textBoxGroupPriority.TextChanged += new System.EventHandler(this.textBoxGroupPriority_TextChanged);
			// 
			// labelPriority
			// 
			this.labelPriority.AutoSize = true;
			this.labelPriority.Location = new System.Drawing.Point(3, 32);
			this.labelPriority.Name = "labelPriority";
			this.labelPriority.Size = new System.Drawing.Size(38, 13);
			this.labelPriority.TabIndex = 3;
			this.labelPriority.Text = "Priority";
			// 
			// buttonPingAll
			// 
			this.buttonPingAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonPingAll.Image = global::LogsReader.Properties.Resources.pingAll;
			this.buttonPingAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonPingAll.Location = new System.Drawing.Point(89, 2);
			this.buttonPingAll.Name = "buttonPingAll";
			this.buttonPingAll.Size = new System.Drawing.Size(80, 25);
			this.buttonPingAll.TabIndex = 2;
			this.buttonPingAll.Text = "     Ping All";
			this.buttonPingAll.UseVisualStyleBackColor = true;
			this.buttonPingAll.Click += new System.EventHandler(this.buttonPingAll_Click);
			// 
			// buttonRemoveAll
			// 
			this.buttonRemoveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonRemoveAll.Image = global::LogsReader.Properties.Resources.remove;
			this.buttonRemoveAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonRemoveAll.Location = new System.Drawing.Point(175, 2);
			this.buttonRemoveAll.Name = "buttonRemoveAll";
			this.buttonRemoveAll.Size = new System.Drawing.Size(96, 25);
			this.buttonRemoveAll.TabIndex = 3;
			this.buttonRemoveAll.Text = "     Remove All";
			this.buttonRemoveAll.UseVisualStyleBackColor = true;
			this.buttonRemoveAll.Click += new System.EventHandler(this.buttonRemoveAll_Click);
			// 
			// buttonAdd
			// 
			this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdd.Image = global::LogsReader.Properties.Resources.add;
			this.buttonAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonAdd.Location = new System.Drawing.Point(3, 2);
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.Size = new System.Drawing.Size(80, 25);
			this.buttonAdd.TabIndex = 1;
			this.buttonAdd.Text = "  Add";
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
			this.panelBottom.Location = new System.Drawing.Point(0, 99);
			this.panelBottom.Name = "panelBottom";
			this.panelBottom.Size = new System.Drawing.Size(426, 30);
			this.panelBottom.TabIndex = 44;
			// 
			// groupBoxServers
			// 
			this.groupBoxServers.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxServers.Location = new System.Drawing.Point(0, 56);
			this.groupBoxServers.Name = "groupBoxServers";
			this.groupBoxServers.Size = new System.Drawing.Size(426, 43);
			this.groupBoxServers.TabIndex = 2;
			this.groupBoxServers.TabStop = false;
			this.groupBoxServers.Text = "Servers";
			// 
			// ServerGroupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(426, 129);
			this.Controls.Add(this.groupBoxServers);
			this.Controls.Add(this.panelBottom);
			this.Controls.Add(this.panelChooseGroup);
			this.MinimumSize = new System.Drawing.Size(442, 138);
			this.Name = "ServerGroupForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Server Group";
			this.Resize += new System.EventHandler(this.ServerGroupForm_Resize);
			this.panelChooseGroup.ResumeLayout(false);
			this.panelChooseGroup.PerformLayout();
			this.panelBottom.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Label labelGroup;
		private ComboBox comboboxGroup;
		private Button buttonCancel;
		private Button buttonOK;
		private Panel panelChooseGroup;
		private Button buttonPingAll;
		private Button buttonRemoveAll;
		private Button buttonAdd;
		private Panel panelBottom;
		private GroupBox groupBoxServers;
		private Label labelPriority;
		private TextBox textBoxGroupPriority;
	}
}