using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Security;
using System.Windows.Forms;
using LogsReader.Properties;

namespace LogsReader.Reader.Forms
{
	public partial class AddUserCredentials : Form
	{
		private readonly string _information;

		public NetworkCredential Credential { get; private set; }

		public AddUserCredentials(string information, string domain = null, string userName = null)
		{
			InitializeComponent();

			Icon = Icon.FromHandle(Resources.connectionLocked.GetHicon());
			labelDomain.Text =  Resources.Txt_Forms_GroupName;
			buttonCancel.Text = Resources.Txt_Forms_Cancel;
			buttonOK.Enabled = false;

			_information = information;
			SetInformation(_information);
			textBoxDomain.Text = domain ?? string.Empty;
			textBoxUser.Text = userName ?? string.Empty;

			CenterToScreen();

			KeyPreview = true;
			KeyDown += (sender, args) =>
			{
				if (args.KeyCode == Keys.Enter && buttonOK.Enabled)
					buttonOK_Click(this, EventArgs.Empty);
				else if (args.KeyCode == Keys.Escape)
					Close();
			};
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			try
			{
				var password = new SecureString();
				foreach (var ch in textBoxPassword.AdminPassword)
					password.AppendChar(ch);

				if (textBoxDomain.Text.Length > 0)
					Credential = new NetworkCredential(textBoxUser.Text, password, textBoxDomain.Text);
				else
					Credential = new NetworkCredential(textBoxUser.Text, password);

				this.DialogResult = DialogResult.OK;
				Close();
			}
			catch (Exception ex)
			{
				SetInformation($"{_information}\r\n{ex.Message}");
			}
		}

		void SetInformation(string info)
		{
			this.MaximumSize = new Size(this.MaximumSize.Width, 999);
			labelInformation.Text = info;
			this.MaximumSize = new Size(this.MaximumSize.Width, this.Size.Height);
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void textBoxUser_TextChanged(object sender, EventArgs e)
		{
			Check();
		}

		private void textBoxPassword_TextChanged(object sender, EventArgs e)
		{
			Check();
		}

		void Check()
		{
			if (textBoxUser.Text.Length > 0 && textBoxPassword.Text.Length > 0)
			{
				buttonOK.Enabled = true;
				return;
			}
			buttonOK.Enabled = false;
		}
	}
}