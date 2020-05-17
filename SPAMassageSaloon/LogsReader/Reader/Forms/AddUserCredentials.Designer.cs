using LogsReader.Properties;
using Utils.WinForm;

namespace LogsReader.Reader.Forms
{
	partial class AddUserCredentials
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
			this.labelDomain = new System.Windows.Forms.Label();
			this.textBoxDomain = new System.Windows.Forms.TextBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.textBoxUser = new System.Windows.Forms.TextBox();
			this.labelUser = new System.Windows.Forms.Label();
			this.textBoxPassword = new Utils.WinForm.PasswordTextBox();
			this.labelPassword = new System.Windows.Forms.Label();
			this.groupBoxInfo = new System.Windows.Forms.GroupBox();
			this.labelInformation = new System.Windows.Forms.Label();
			this.groupBoxInfo.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelDomain
			// 
			this.labelDomain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelDomain.AutoSize = true;
			this.labelDomain.Location = new System.Drawing.Point(5, 61);
			this.labelDomain.Name = "labelDomain";
			this.labelDomain.Size = new System.Drawing.Size(43, 13);
			this.labelDomain.TabIndex = 0;
			this.labelDomain.Text = "Domain";
			// 
			// textBoxDomain
			// 
			this.textBoxDomain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxDomain.Location = new System.Drawing.Point(64, 58);
			this.textBoxDomain.Name = "textBoxDomain";
			this.textBoxDomain.Size = new System.Drawing.Size(223, 20);
			this.textBoxDomain.TabIndex = 1;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Image = global::LogsReader.Properties.Resources.cancel;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(208, 133);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(80, 25);
			this.buttonCancel.TabIndex = 39;
			this.buttonCancel.Text = global::LogsReader.Properties.Resources.Txt_Forms_Cancel;
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Image = global::LogsReader.Properties.Resources.Ok;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(142, 133);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(60, 25);
			this.buttonOK.TabIndex = 40;
			this.buttonOK.Text = global::LogsReader.Properties.Resources.Txt_Forms_OK;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// textBoxUser
			// 
			this.textBoxUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxUser.Location = new System.Drawing.Point(64, 84);
			this.textBoxUser.Name = "textBoxUser";
			this.textBoxUser.Size = new System.Drawing.Size(223, 20);
			this.textBoxUser.TabIndex = 42;
			this.textBoxUser.TextChanged += new System.EventHandler(this.textBoxUser_TextChanged);
			// 
			// labelUser
			// 
			this.labelUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelUser.AutoSize = true;
			this.labelUser.Location = new System.Drawing.Point(5, 87);
			this.labelUser.Name = "labelUser";
			this.labelUser.Size = new System.Drawing.Size(29, 13);
			this.labelUser.TabIndex = 41;
			this.labelUser.Text = "User";
			// 
			// textBoxPassword
			// 
			this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxPassword.Location = new System.Drawing.Point(64, 110);
			this.textBoxPassword.Name = "textBoxPassword";
			this.textBoxPassword.Size = new System.Drawing.Size(224, 20);
			this.textBoxPassword.TabIndex = 44;
			this.textBoxPassword.TextChanged += new System.EventHandler(this.textBoxPassword_TextChanged);
			// 
			// labelPassword
			// 
			this.labelPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelPassword.AutoSize = true;
			this.labelPassword.Location = new System.Drawing.Point(5, 113);
			this.labelPassword.Name = "labelPassword";
			this.labelPassword.Size = new System.Drawing.Size(53, 13);
			this.labelPassword.TabIndex = 43;
			this.labelPassword.Text = "Password";
			// 
			// groupBoxInfo
			// 
			this.groupBoxInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxInfo.AutoSize = true;
			this.groupBoxInfo.Controls.Add(this.labelInformation);
			this.groupBoxInfo.Location = new System.Drawing.Point(7, 7);
			this.groupBoxInfo.Name = "groupBoxInfo";
			this.groupBoxInfo.Size = new System.Drawing.Size(281, 45);
			this.groupBoxInfo.TabIndex = 45;
			this.groupBoxInfo.TabStop = false;
			this.groupBoxInfo.Text = "Info";
			// 
			// labelInformation
			// 
			this.labelInformation.AutoSize = true;
			this.labelInformation.Location = new System.Drawing.Point(7, 16);
			this.labelInformation.Name = "labelInformation";
			this.labelInformation.Size = new System.Drawing.Size(59, 13);
			this.labelInformation.TabIndex = 46;
			this.labelInformation.Text = "Information";
			// 
			// AddUserCredentials
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(294, 160);
			this.Controls.Add(this.groupBoxInfo);
			this.Controls.Add(this.textBoxPassword);
			this.Controls.Add(this.labelPassword);
			this.Controls.Add(this.textBoxUser);
			this.Controls.Add(this.labelUser);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.textBoxDomain);
			this.Controls.Add(this.labelDomain);
			this.MaximumSize = new System.Drawing.Size(999, 999);
			this.MinimumSize = new System.Drawing.Size(231, 199);
			this.Name = "AddUserCredentials";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Authorization";
			this.groupBoxInfo.ResumeLayout(false);
			this.groupBoxInfo.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelDomain;
		private System.Windows.Forms.TextBox textBoxDomain;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.TextBox textBoxUser;
		private System.Windows.Forms.Label labelUser;
		private PasswordTextBox textBoxPassword;
		private System.Windows.Forms.Label labelPassword;
		private System.Windows.Forms.GroupBox groupBoxInfo;
		private System.Windows.Forms.Label labelInformation;
	}
}