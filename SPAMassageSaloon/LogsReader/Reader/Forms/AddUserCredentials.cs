using System;
using System.Drawing;
using System.Windows.Forms;
using LogsReader.Properties;

namespace LogsReader.Reader.Forms
{
	public partial class AddUserCredentials : Form
	{
		private readonly ContextMenuStrip _contextMenuStrip;
		public CryptoNetworkCredential Credential { get; private set; }

		public AddUserCredentials(string information, string domain = null, string userName = null)
		{
			InitializeComponent();

			Icon = Icon.FromHandle(Resources.connectionLocked.GetHicon());
			buttonCancel.Text = Resources.Txt_Forms_Cancel;
			buttonOK.Enabled = false;

			SetInformation(information);
			textBoxDomain.Text = domain ?? string.Empty;
			textBoxUser.Text = userName ?? string.Empty;

			CenterToScreen();

			_contextMenuStrip = new ContextMenuStrip { Tag = labelInformation };
			_contextMenuStrip.Items.Add("Copy text", null, (sender, args) => { Clipboard.SetText(labelInformation.Text); });
			labelInformation.MouseClick += Information_MouseClick;

			KeyPreview = true;
			KeyDown += (sender, args) =>
			{
				if (args.KeyCode == Keys.Enter && buttonOK.Enabled)
					buttonOK_Click(this, EventArgs.Empty);
				else if (args.KeyCode == Keys.Escape)
					Close();
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
			Credential = new CryptoNetworkCredential(textBoxDomain.Text, textBoxUser.Text, textBoxPassword.AdminPassword);
			Close();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			ChangeFormSize();
		}

		void SetInformation(string info)
		{
			labelInformation.MaximumSize = new Size(groupBoxInfo.Size.Width - 20, 0);
			labelInformation.AutoSize = true;
			labelInformation.Text = info;
			ChangeFormSize();
		}

		void ChangeFormSize()
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