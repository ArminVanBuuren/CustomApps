using System;
using System.Drawing;
using System.Windows.Forms;
using LogsReader.Properties;
using Utils;

namespace LogsReader.Reader.Forms
{
	public partial class AddUserCredentials : Form
	{
		private readonly string _sourceInformation;
		private readonly ContextMenuStrip _contextMenuStrip;
		public CryptoNetworkCredential Credential { get; private set; }

		public AddUserCredentials(string information, string userName = null)
		{
			InitializeComponent();
			Icon = Icon.FromHandle(Resources.authorization.GetHicon());
			buttonCancel.Text = Resources.Txt_Forms_Cancel;
			buttonOK.Enabled = false;
			MinimizeBox = false;
			MaximizeBox = false;
			TopLevel = true;
			TopMost = true;
			_sourceInformation = information;
			SetInformation(information);
			textBoxUser.Text = userName ?? string.Empty;
			CenterToScreen();
			_contextMenuStrip = new ContextMenuStrip { Tag = labelInformation };
			_contextMenuStrip.Items.Add("Copy text", null, (sender, args) => { Clipboard.SetText(labelInformation.Text); });
			labelInformation.MouseClick += Information_MouseClick;
			KeyPreview = true;
			KeyDown += (sender, args) =>
			{
				switch (args.KeyCode)
				{
					case Keys.Enter when buttonOK.Enabled:
						buttonOK_Click(this, EventArgs.Empty);
						break;

					case Keys.Escape:
						Close();
						break;
				}
			};
		}

		private void Information_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right)
				return;

			_contextMenuStrip?.Show(labelInformation, e.Location);
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Credential = new CryptoNetworkCredential(textBoxUser.Text, textBoxPassword.AdminPassword);
			Close();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			ChangeFormSize();
		}

		private void SetInformation(string info)
		{
			labelInformation.MaximumSize = new Size(groupBoxInfo.Size.Width - 20, 0);
			labelInformation.AutoSize = true;
			labelInformation.Text = info;
			ChangeFormSize();
		}

		private void ChangeFormSize()
		{
			labelInformation.MaximumSize = new Size(groupBoxInfo.Size.Width - 20, 0);
			labelInformation.AutoSize = true;
			var formHeight = groupBoxInfo.Size.Height + panelAuthorization.Size.Height + 40;
			Size = new Size(Size.Width, formHeight);
			MinimumSize = new Size(MinimumSize.Width, formHeight);
			MaximumSize = new Size(999, formHeight);
			// необходим повтор, т.к. это сраные формы
			Size = new Size(Size.Width, formHeight);
			MinimumSize = new Size(MinimumSize.Width, formHeight);
			MaximumSize = new Size(999, formHeight);
		}

		private void buttonCancel_Click(object sender, EventArgs e) => Close();

		private void textBoxUser_TextChanged(object sender, EventArgs e)
		{
			if (!textBoxUser.Text.IsNullOrWhiteSpace())
			{
				var domain_username = textBoxUser.Text.Split('\\');
				if (domain_username.Length > 1)
					SetInformation($"{_sourceInformation}\r\n\r\nDomain:{domain_username[0]}");
				else if (labelInformation.Text != _sourceInformation)
					SetInformation(_sourceInformation);
			}
			else if (labelInformation.Text != _sourceInformation)
			{
				SetInformation(_sourceInformation);
			}

			Check();
		}

		private void textBoxPassword_TextChanged(object sender, EventArgs e) => Check();

		private void Check()
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