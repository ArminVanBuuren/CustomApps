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
			
			_serverGroups = serverGroups;

			comboboxGroup.Items.AddRange(_serverGroups.Keys.ToArray());
			comboboxGroup.SelectionChangeCommitted += ComboboxGroup_SelectionChangeCommitted;
			comboboxGroup.SelectedText = selectedGroup;
		}

		private void ComboboxGroup_SelectionChangeCommitted(object sender, EventArgs e)
		{
			Save();

			foreach (var panel in Controls.OfType<Panel>().ToList())
			{
				if(_serverPanels.TryGetValue(panel, out var _))
					Controls.Remove(panel);
			}
			_serverPanels.Clear();

			_currentGroup = comboboxGroup.SelectedText;
			if (_serverGroups.TryGetValue(comboboxGroup.SelectedText, out var servers) && servers.Count > 0)
			{
				foreach (var server in servers)
					AddServer(server);
			}
			else
			{
				AddServer(string.Empty);
			}
			buttonOK.Enabled = true;
		}

		void Save()
		{
			if (_currentGroup != null && buttonOK.Enabled)
			{
				_serverGroups[_currentGroup] =
					new List<string>(_serverPanels.Keys
						.Select(x => x.Controls.OfType<TextBox>().FirstOrDefault()?.Text)
						.Where(x => !x.IsNullOrEmptyTrim())
						.Distinct(StringComparer.InvariantCultureIgnoreCase));
			}
		}

		private void comboboxGroup_TextChanged(object sender, EventArgs e)
		{
			if (_serverGroups.TryGetValue(comboboxGroup.Text, out var result))
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

		void AddServer(string serverText)
		{
			var objectPingSync = new object();
			var serverTemplate = new Panel();

			var labelServer = new Label
			{
				AutoSize = true,
				Location = new Point(7, 8),
				Size = new Size(38, 13),
				Text = Resources.Txt_Forms_Server
			};

			var textBoxServer = new TextBox
			{
				Anchor = (AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right,
				Location = new Point(51, 5),
				Size = new Size(193, 20),
				Text = serverText
			};

			var buttonPing = new Button
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Image = Resources.ping,
				ImageAlign = ContentAlignment.MiddleLeft,
				Location = new Point(250, 3),
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
						Task.Factory.StartNew(() =>
						{
							lock (objectPingSync)
							{
								var pinger = new Ping();
								Color color;
								try
								{
									var reply = pinger.Send(textBoxServer.Text, 10000);
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

								this.SafeInvoke(() =>
								{
									try
									{
										if (!this.IsDisposed && !textBoxServer.IsDisposed)
											textBoxServer.BackColor = color;
									}
									catch (Exception)
									{
										// ignored
									}
								});
							}
						});
					}
				}
				catch (Exception ex)
				{
					// igored
				}
			};

			var buttonRemove = new Button
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Image = Resources.remove,
				ImageAlign = ContentAlignment.MiddleLeft,
				Location = new Point(316, 3),
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

					Controls.Remove(serverTemplate);
					_serverPanels.Remove(serverTemplate);
				}
				catch (Exception e)
				{
					// igored
				}
			};

			serverTemplate.Controls.Add(labelServer);
			serverTemplate.Controls.Add(textBoxServer);
			serverTemplate.Controls.Add(buttonPing);
			serverTemplate.Controls.Add(buttonRemove);
			serverTemplate.Dock = DockStyle.Top;
			serverTemplate.Location = new Point(0, 26);
			serverTemplate.Size = new Size(400, 30);

			_serverPanels.Add(serverTemplate, true);
			Controls.Add(serverTemplate);
		}

		private void buttonAdd_Click(object sender, EventArgs e)
		{
			AddServer(string.Empty);
		}

		private void buttonPingAll_Click(object sender, EventArgs e)
		{
			foreach (var panel in Controls.OfType<Panel>().ToList())
			{
				var button = panel.Controls.OfType<Button>().ToList().FirstOrDefault(x => x.Name == PING_SERVER);
				button?.PerformClick();
			}
		}

		private void buttonRemoveAll_Click(object sender, EventArgs e)
		{
			foreach (var panel in Controls.OfType<Panel>().ToList())
			{
				var button = panel.Controls.OfType<Button>().ToList().FirstOrDefault(x => x.Name == REMOVE_SERVER);
				button?.PerformClick();
			}
			AddServer(string.Empty);
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			Save();
			Close();
		}
	}
}
