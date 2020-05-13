using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogsReader.Properties;
using Utils;
using Utils.WinForm;

namespace LogsReader.Reader.Forms
{
	public partial class ServerGroupForm : Form
	{
		private const string PING_SERVER = "PingServer";
		private const string REMOVE_SERVER = "RemoveServer";
		private readonly Dictionary<string, List<string>> _serverGroups;
		private readonly Dictionary<Panel, bool> _serverPanels = new Dictionary<Panel, bool>();
		private string _currentGroup = null;

		public ServerGroupForm(string selectedGroup, Dictionary<string, List<string>> serverGroups)
		{
			InitializeComponent();

			Icon = Icon.FromHandle(Resources.server_group.GetHicon());
			base.Text = Resources.Txt_Forms_ServerGroup;
			base.AutoSize = false;

			labelGroup.Text = Resources.Txt_Forms_Group;
			groupBoxServers.Text = Resources.Txt_LogsReaderForm_Servers;
			buttonAdd.Text = Resources.Txt_Forms_Add;
			buttonRemoveAll.Text = Resources.Txt_Forms_RemoveAll;
			buttonCancel.Text = Resources.Txt_Forms_Cancel;

			_serverGroups = serverGroups;

			comboboxGroup.Items.AddRange(_serverGroups.Keys.ToArray());
			comboboxGroup.Text = selectedGroup;
			comboboxGroup.SelectedText = selectedGroup;
			ComboboxGroup_SelectionChangeCommitted(this, EventArgs.Empty);
			comboboxGroup.SelectionChangeCommitted += ComboboxGroup_SelectionChangeCommitted;
			comboboxGroup.TextChanged += comboboxGroup_TextChanged;

			CenterToScreen();

			KeyPreview = true;
			KeyDown += (sender, args) =>
			{
				if (args.KeyCode == Keys.Enter)
					buttonOK_Click(this, EventArgs.Empty);
				else if (args.KeyCode == Keys.Escape)
					Close();
			};
		}

		private void ComboboxGroup_SelectionChangeCommitted(object sender, EventArgs e)
		{
			_serverPanels.Clear();
			_currentGroup = comboboxGroup.SelectedItem.ToString();

			if (_serverGroups.TryGetValue(_currentGroup, out var servers) && servers.Count > 0)
			{
				foreach (var server in servers)
					AddServer(server);
			}
			else
			{
				AddServer(string.Empty);
			}

			AssignForm();
			buttonOK.Enabled = true;
		}

		private void comboboxGroup_TextChanged(object sender, EventArgs e)
		{
			if (comboboxGroup.Text.IsNullOrEmptyTrim() || (_serverGroups.TryGetValue(comboboxGroup.Text, out var _) && _currentGroup != comboboxGroup.Text))
			{
				buttonOK.Enabled = false;
				comboboxGroup.BackColor = Color.LightPink;
			}
			else
			{
				_serverGroups.RenameKey(_currentGroup, comboboxGroup.Text);
				_currentGroup = comboboxGroup.Text;
				buttonOK.Enabled = true;
				comboboxGroup.BackColor = Color.White;
			}
		}

		private void buttonAdd_Click(object sender, EventArgs e)
		{
			AddServer(string.Empty);
			AssignForm();
		}

		private void buttonRemoveAll_Click(object sender, EventArgs e)
		{
			foreach (var panel in groupBoxServers.Controls.OfType<Panel>().ToList())
			{
				var button = panel.Controls.OfType<Button>().ToList().FirstOrDefault(x => x.Name == REMOVE_SERVER);
				button?.PerformClick();
			}

			AddServer(string.Empty);
			AssignForm();
		}

		private void buttonPingAll_Click(object sender, EventArgs e)
		{
			foreach (var panel in groupBoxServers.Controls.OfType<Panel>().Reverse().ToList())
			{
				var button = panel.Controls.OfType<Button>().ToList().FirstOrDefault(x => x.Name == PING_SERVER);
				button?.PerformClick();
			}
		}

		void AssignForm()
		{
			foreach (var panel in groupBoxServers.Controls.OfType<Panel>().ToList())
				groupBoxServers.Controls.Remove(panel);

			var formHeightSize = panelChooseGroup.Size.Height + panelBottom.Size.Height + 60;
			foreach (var panel in _serverPanels.Keys.Reverse())
			{
				groupBoxServers.Controls.Add(panel);
				formHeightSize += panel.Size.Height;
			}

			this.Size = new Size(this.Size.Width, formHeightSize);
			this.MinimumSize = new Size(this.MinimumSize.Width, formHeightSize);
			this.Size = new Size(this.Size.Width, formHeightSize);
			this.MinimumSize = new Size(this.MinimumSize.Width, formHeightSize);
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;

			if (_currentGroup != null && buttonOK.Enabled)
			{
				_serverGroups[_currentGroup] =
					new List<string>(_serverPanels.Keys
						.Select(x => x.Controls.OfType<TextBox>().FirstOrDefault()?.Text)
						.Where(x => !x.IsNullOrEmptyTrim())
						.Distinct(StringComparer.InvariantCultureIgnoreCase));
			}

			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		void AddServer(string serverText)
		{
			var objectPingSync = new object();
			var serverTemplate = new Panel();

			var textBoxServer = new TextBox
			{
				Anchor = (AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right,
				Location = new Point(10, 6),
				Size = new Size(30, 20),
				Text = serverText
			};
			textBoxServer.TextChanged += (sender, args) => { textBoxServer.BackColor = Color.White; };

			var buttonRemove = new Button
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Image = Resources.remove,
				ImageAlign = ContentAlignment.MiddleLeft,
				Location = new Point(115, 3),
				Size = new Size(76, 25),
				Name = REMOVE_SERVER,
				Text = Resources.Txt_Forms_Remove,
				UseVisualStyleBackColor = true
			};
			buttonRemove.Click += (sender, args) =>
			{
				try
				{
					if (this.IsDisposed)
						return;

					_serverPanels.Remove(serverTemplate);
					AssignForm();
				}
				catch (Exception ex)
				{
					// igored
				}
			};

			var buttonPing = new Button
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Image = Resources.ping,
				ImageAlign = ContentAlignment.MiddleLeft,
				Location = new Point(50, 3),
				Size = new Size(60, 25),
				Name = PING_SERVER,
				Text = "   Ping",
				UseVisualStyleBackColor = true
			};
			buttonPing.Click += (sender, args) =>
			{
				try
				{
					if (textBoxServer.Text.IsNullOrEmptyTrim())
					{
						textBoxServer.BackColor = Color.LightPink;
					}
					else
					{
						string serverText2;
						lock (objectPingSync)
						{
							serverText2 = textBoxServer.Text;
							buttonPing.Enabled = false;
							buttonRemove.Enabled = false;
							textBoxServer.Enabled = false;
						}

						Task.Factory.StartNew(() =>
						{
							lock (objectPingSync)
							{
								try
								{
									var pinger = new Ping();
									Color color;
									try
									{
										var reply = pinger.Send(serverText2, 10000);
										color = reply != null && reply.Status == IPStatus.Success ? Color.LightGreen : Color.LightPink;
									}
									catch (Exception)
									{
										color = Color.LightPink;
									}
									finally
									{
										pinger.Dispose();
									}

									if (!this.IsDisposed)
									{
										this.SafeInvoke(() =>
										{

											if (this.IsDisposed || textBoxServer.IsDisposed)
												return;

											textBoxServer.BackColor = color;
											buttonPing.Enabled = true;
											buttonRemove.Enabled = true;
											textBoxServer.Enabled = true;
										});
									}
								}
								catch (Exception)
								{
									// ignored
								}
							}
						});
					}
				}
				catch (Exception ex)
				{
					// igored
				}
			};

			serverTemplate.Controls.Add(textBoxServer);
			serverTemplate.Controls.Add(buttonPing);
			serverTemplate.Controls.Add(buttonRemove);
			serverTemplate.Dock = DockStyle.Top;
			serverTemplate.Location = new Point(0, 26);
			serverTemplate.Size = new Size(400, 30);

			_serverPanels.Add(serverTemplate, true);
		}
	}
}
