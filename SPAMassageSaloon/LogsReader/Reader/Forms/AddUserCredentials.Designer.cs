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
			this.textBoxUser = new System.Windows.Forms.TextBox();
			this.textBoxPassword = new Utils.WinForm.PasswordTextBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.labelUser = new System.Windows.Forms.Label();
			this.labelPassword = new System.Windows.Forms.Label();
			this.labelInformation = new System.Windows.Forms.Label();
			this.panelAuthorization = new System.Windows.Forms.Panel();
			this.groupBoxInfo = new System.Windows.Forms.GroupBox();
			this.panelAuthorization.SuspendLayout();
			this.groupBoxInfo.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelDomain
			// 
			this.labelDomain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelDomain.AutoSize = true;
			this.labelDomain.Location = new System.Drawing.Point(3, 9);
			this.labelDomain.Name = "labelDomain";
			this.labelDomain.Size = new System.Drawing.Size(43, 13);
			this.labelDomain.TabIndex = 0;
			this.labelDomain.Text = "Domain";
			// 
			// textBoxDomain
			// 
			this.textBoxDomain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxDomain.Location = new System.Drawing.Point(62, 6);
			this.textBoxDomain.Name = "textBoxDomain";
			this.textBoxDomain.Size = new System.Drawing.Size(178, 20);
			this.textBoxDomain.TabIndex = 2;
			// 
			// textBoxUser
			// 
			this.textBoxUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxUser.Location = new System.Drawing.Point(62, 32);
			this.textBoxUser.Name = "textBoxUser";
			this.textBoxUser.Size = new System.Drawing.Size(177, 20);
			this.textBoxUser.TabIndex = 3;
			this.textBoxUser.TextChanged += new System.EventHandler(this.textBoxUser_TextChanged);
			// 
			// textBoxPassword
			// 
			this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxPassword.Location = new System.Drawing.Point(62, 58);
			this.textBoxPassword.Name = "textBoxPassword";
			this.textBoxPassword.Size = new System.Drawing.Size(178, 20);
			this.textBoxPassword.TabIndex = 4;
			this.textBoxPassword.TextChanged += new System.EventHandler(this.textBoxPassword_TextChanged);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Image = global::LogsReader.Properties.Resources.cancel;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(160, 83);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(80, 25);
			this.buttonCancel.TabIndex = 6;
			this.buttonCancel.Text = global::LogsReader.Properties.Resources.Txt_Forms_Cancel;
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Image = global::LogsReader.Properties.Resources.Ok;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(94, 83);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(60, 25);
			this.buttonOK.TabIndex = 5;
			this.buttonOK.Text = global::LogsReader.Properties.Resources.Txt_Forms_OK;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// labelUser
			// 
			this.labelUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelUser.AutoSize = true;
			this.labelUser.Location = new System.Drawing.Point(3, 35);
			this.labelUser.Name = "labelUser";
			this.labelUser.Size = new System.Drawing.Size(29, 13);
			this.labelUser.TabIndex = 3;
			this.labelUser.Text = "User";
			// 
			// labelPassword
			// 
			this.labelPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelPassword.AutoSize = true;
			this.labelPassword.Location = new System.Drawing.Point(3, 61);
			this.labelPassword.Name = "labelPassword";
			this.labelPassword.Size = new System.Drawing.Size(53, 13);
			this.labelPassword.TabIndex = 5;
			this.labelPassword.Text = "Password";
			// 
			// labelInformation
			// 
			this.labelInformation.AutoSize = true;
			this.labelInformation.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.labelInformation.Location = new System.Drawing.Point(7, 16);
			this.labelInformation.Name = "labelInformation";
			this.labelInformation.Size = new System.Drawing.Size(69, 13);
			this.labelInformation.TabIndex = 6;
			this.labelInformation.Text = "Information";
			// 
			// panelAuthorization
			// 
			this.panelAuthorization.Controls.Add(this.labelDomain);
			this.panelAuthorization.Controls.Add(this.textBoxDomain);
			this.panelAuthorization.Controls.Add(this.textBoxUser);
			this.panelAuthorization.Controls.Add(this.textBoxPassword);
			this.panelAuthorization.Controls.Add(this.buttonOK);
			this.panelAuthorization.Controls.Add(this.buttonCancel);
			this.panelAuthorization.Controls.Add(this.labelPassword);
			this.panelAuthorization.Controls.Add(this.labelUser);
			this.panelAuthorization.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelAuthorization.Location = new System.Drawing.Point(0, 45);
			this.panelAuthorization.MaximumSize = new System.Drawing.Size(999, 111);
			this.panelAuthorization.Name = "panelAuthorization";
			this.panelAuthorization.Size = new System.Drawing.Size(243, 111);
			this.panelAuthorization.TabIndex = 47;
			// 
			// groupBoxInfo
			// 
			this.groupBoxInfo.AutoSize = true;
			this.groupBoxInfo.Controls.Add(this.labelInformation);
			this.groupBoxInfo.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBoxInfo.Location = new System.Drawing.Point(0, 0);
			this.groupBoxInfo.Name = "groupBoxInfo";
			this.groupBoxInfo.Size = new System.Drawing.Size(243, 45);
			this.groupBoxInfo.TabIndex = 1;
			this.groupBoxInfo.TabStop = false;
			this.groupBoxInfo.Text = "Message";
			// 
			// AddUserCredentials
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(243, 157);
			this.Controls.Add(this.panelAuthorization);
			this.Controls.Add(this.groupBoxInfo);
			this.MaximumSize = new System.Drawing.Size(999, 999);
			this.MinimumSize = new System.Drawing.Size(259, 100);
			this.Name = "AddUserCredentials";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Authorization";
			this.panelAuthorization.ResumeLayout(false);
			this.panelAuthorization.PerformLayout();
			this.groupBoxInfo.ResumeLayout(false);
			this.groupBoxInfo.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelDomain;
		private System.Windows.Forms.TextBox textBoxDomain;
		private System.Windows.Forms.TextBox textBoxUser;
		private PasswordTextBox textBoxPassword;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Label labelUser;
		private System.Windows.Forms.Label labelPassword;
		private System.Windows.Forms.Label labelInformation;
		private System.Windows.Forms.Panel panelAuthorization;
		private System.Windows.Forms.GroupBox groupBoxInfo;
	}
}