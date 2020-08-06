using LogsReader.Properties;

namespace LogsReader.Reader.Forms
{
	partial class TypesGroupForm
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.textBoxGroupPriority = new System.Windows.Forms.TextBox();
			this.labelPriority = new System.Windows.Forms.Label();
			this.comboboxGroup = new System.Windows.Forms.ComboBox();
			this.labelGroupName = new System.Windows.Forms.Label();
			this.richTextBoxTypes = new System.Windows.Forms.RichTextBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.textBoxGroupPriority);
			this.panel1.Controls.Add(this.labelPriority);
			this.panel1.Controls.Add(this.comboboxGroup);
			this.panel1.Controls.Add(this.labelGroupName);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(557, 55);
			this.panel1.TabIndex = 40;
			// 
			// textBoxGroupPriority
			// 
			this.textBoxGroupPriority.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxGroupPriority.Location = new System.Drawing.Point(80, 30);
			this.textBoxGroupPriority.Name = "textBoxGroupPriority";
			this.textBoxGroupPriority.Size = new System.Drawing.Size(474, 20);
			this.textBoxGroupPriority.TabIndex = 2;
			this.textBoxGroupPriority.TextChanged += new System.EventHandler(this.textBoxGroupPriority_TextChanged);
			// 
			// labelPriority
			// 
			this.labelPriority.AutoSize = true;
			this.labelPriority.Location = new System.Drawing.Point(7, 33);
			this.labelPriority.Name = "labelPriority";
			this.labelPriority.Size = new System.Drawing.Size(38, 13);
			this.labelPriority.TabIndex = 102;
			this.labelPriority.Text = "Priority";
			// 
			// comboboxGroup
			// 
			this.comboboxGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboboxGroup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.comboboxGroup.FormattingEnabled = true;
			this.comboboxGroup.Location = new System.Drawing.Point(80, 5);
			this.comboboxGroup.MaxDropDownItems = 2;
			this.comboboxGroup.Name = "comboboxGroup";
			this.comboboxGroup.Size = new System.Drawing.Size(474, 21);
			this.comboboxGroup.TabIndex = 1;
			// 
			// labelGroupName
			// 
			this.labelGroupName.AutoSize = true;
			this.labelGroupName.Location = new System.Drawing.Point(7, 8);
			this.labelGroupName.Name = "labelGroupName";
			this.labelGroupName.Size = new System.Drawing.Size(67, 13);
			this.labelGroupName.TabIndex = 0;
			this.labelGroupName.Text = "Group Name";
			// 
			// richTextBoxTypes
			// 
			this.richTextBoxTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBoxTypes.Location = new System.Drawing.Point(10, 56);
			this.richTextBoxTypes.Name = "richTextBoxTypes";
			this.richTextBoxTypes.Size = new System.Drawing.Size(539, 81);
			this.richTextBoxTypes.TabIndex = 3;
			this.richTextBoxTypes.Text = "";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Image = global::LogsReader.Properties.Resources.cancel;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(469, 139);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(80, 25);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = global::LogsReader.Properties.Resources.Txt_Forms_Cancel;
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Image = global::LogsReader.Properties.Resources.Ok;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(403, 139);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(60, 25);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.Text = global::LogsReader.Properties.Resources.Txt_Forms_OK;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// TypesGroupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(557, 166);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.richTextBoxTypes);
			this.Controls.Add(this.panel1);
			this.MaximumSize = new System.Drawing.Size(725, 461);
			this.MinimumSize = new System.Drawing.Size(179, 153);
			this.Name = "TypesGroupForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "File Types Group";
			this.Resize += new System.EventHandler(this.TypesGroupForm_Resize);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ComboBox comboboxGroup;
		private System.Windows.Forms.Label labelGroupName;
		private System.Windows.Forms.RichTextBox richTextBoxTypes;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.TextBox textBoxGroupPriority;
		private System.Windows.Forms.Label labelPriority;
	}
}