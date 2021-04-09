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

		private readonly Dictionary<string, (int, List<string>)> _serverGroups;
		private readonly HashSet<Panel> _serverPanels = new HashSet<Panel>();
		private readonly string selectedGroupPriority = @"0";

		private string _currentGroup;
		private int _prevPriority;

		public ServerGroupForm(string selectedGroup, Dictionary<string, (int, List<string>)> serverGroups)
		{
			InitializeComponent();
			Icon = Icon.FromHandle(Resources.server_group.GetHicon());
			base.Text = Resources.Txt_Forms_ServerGroup;
			base.AutoSize = false;
			MinimizeBox = false;
			MaximizeBox = false;
			labelGroup.Text = Resources.Txt_Forms_GroupName;
			groupBoxServers.Text = Resources.Txt_LogsReaderForm_Servers;
			buttonAdd.Text = Resources.Txt_Forms_Add;
			buttonRemoveAll.Text = Resources.Txt_Forms_RemoveAll;
			buttonCancel.Text = Resources.Txt_Forms_Cancel;
			labelPriority.Text = Resources.Txt_Forms_GroupPriority;
			_serverGroups = serverGroups;
			comboboxGroup.Items.AddRange(_serverGroups.Keys.ToArray());
			comboboxGroup.Text = selectedGroup;
			comboboxGroup.SelectedText = selectedGroup;
			ComboboxGroup_SelectionChangeCommitted(this, EventArgs.Empty);
			comboboxGroup.SelectionChangeCommitted += ComboboxGroup_SelectionChangeCommitted;
			comboboxGroup.TextChanged += comboboxGroup_TextChanged;
			if (_serverGroups.TryGetValue(selectedGroup, out var res))
				textBoxGroupPriority.Text = selectedGroupPriority = res.Item1.ToString();
			CenterToScreen();
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

		private void ComboboxGroup_SelectionChangeCommitted(object sender, EventArgs e)
		{
			try
			{
				this.SuspendHandle();
				_serverPanels.Clear();
				_currentGroup = comboboxGroup.SelectedItem.ToString();

				if (_serverGroups.TryGetValue(_currentGroup, out var servers) && servers.Item2.Count > 0)
				{
					foreach (var server in servers.Item2)
						AddServer(server);
					textBoxGroupPriority.Text = servers.Item1.ToString();
				}
				else
				{
					AddServer(string.Empty);
					textBoxGroupPriority.Text = selectedGroupPriority;
				}

				AssignForm();
				buttonOK.Enabled = true;
			}
			finally
			{
				this.ResumeHandle();
			}
		}

		private void comboboxGroup_TextChanged(object sender, EventArgs e)
		{
			if (comboboxGroup.Text.IsNullOrWhiteSpace()
			 || _serverGroups.TryGetValue(comboboxGroup.Text.Trim(), out var _) && _currentGroup != comboboxGroup.Text.Trim())
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
			try
			{
				this.SuspendHandle();
				AddServer(string.Empty);
				AssignForm();
			}
			finally
			{
				this.ResumeHandle();
			}
		}

		private void buttonRemoveAll_Click(object sender, EventArgs e)
		{
			try
			{
				this.SuspendHandle();

				foreach (var panel in groupBoxServers.Controls.OfType<Panel>().ToList())
				{
					var button = panel.Controls.OfType<Button>().ToList().FirstOrDefault(x => x.Name == REMOVE_SERVER);
					button?.PerformClick();
				}
			}
			finally
			{
				this.ResumeHandle();
			}
		}

		private void buttonPingAll_Click(object sender, EventArgs e)
		{
			foreach (var panel in groupBoxServers.Controls.OfType<Panel>().Reverse().ToList())
			{
				var button = panel.Controls.OfType<Button>().ToList().FirstOrDefault(x => x.Name == PING_SERVER);
				button?.PerformClick();
			}
		}

		private void AssignForm()
		{
			foreach (var panel in groupBoxServers.Controls.OfType<Panel>().ToList())
				groupBoxServers.Controls.Remove(panel);
			var formHeightSize = panelChooseGroup.Size.Height + panelBottom.Size.Height + 60;

			foreach (var panel in _serverPanels.Reverse())
			{
				groupBoxServers.Controls.Add(panel);
				formHeightSize += panel.Size.Height;
			}

			MaximumSize = new Size(999, formHeightSize);
			Size = new Size(Size.Width, formHeightSize);
			MinimumSize = new Size(MinimumSize.Width, formHeightSize);
			// необходим повтор, т.к. это сраные формы
			MaximumSize = new Size(999, formHeightSize);
			Size = new Size(Size.Width, formHeightSize);
			MinimumSize = new Size(MinimumSize.Width, formHeightSize);
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;

			if (_currentGroup != null && buttonOK.Enabled)
			{
				_serverGroups[_currentGroup] = (AddGroupForm.GetGroupPriority(textBoxGroupPriority.Text, _prevPriority),
				                                new List<string>(_serverPanels.Select(x => x.Controls.OfType<TextBox>().FirstOrDefault()?.Text)
				                                                              .Where(x => !x.IsNullOrWhiteSpace())
				                                                              .Distinct(StringComparer.InvariantCultureIgnoreCase)));
			}

			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e) => Close();

		private void AddServer(string serverText)
		{
			var objectPingSync = new object();
			var serverTemplate = new Panel();
			var textBoxServer = new TextBox
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
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
				if (IsDisposed)
					return;

				try
				{
					SuspendLayout();
					_serverPanels.Remove(serverTemplate);
					if (_serverPanels.Count == 0)
						AddServer(string.Empty);
					AssignForm();
				}
				catch (Exception ex)
				{
					// igored
				}
				finally
				{
					ResumeLayout();
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
					if (textBoxServer.Text.IsNullOrWhiteSpace())
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
										color = reply != null && reply.Status == IPStatus.Success
											? LogsReaderMainForm.READER_COLOR_BACK_SUCCESS
											: Color.LightPink;
									}
									catch (Exception)
									{
										color = Color.LightPink;
									}
									finally
									{
										pinger.Dispose();
									}

									if (!IsDisposed)
									{
										this.SafeInvoke(() =>
										{
											if (IsDisposed || textBoxServer.IsDisposed)
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
			_serverPanels.Add(serverTemplate);
			var tabIndex = 1;

			foreach (var panel in _serverPanels)
			{
				panel.TabIndex = tabIndex++;

				foreach (var control in panel.Controls.OfType<Control>())
				{
					control.TabIndex = tabIndex++;
				}
			}
		}

		private void textBoxGroupPriority_TextChanged(object sender, EventArgs e)
		{
			try
			{
				textBoxGroupPriority.TextChanged -= textBoxGroupPriority_TextChanged;
				if (!textBoxGroupPriority.Text.IsNullOrWhiteSpace())
					textBoxGroupPriority.Text = (_prevPriority = AddGroupForm.GetGroupPriority(textBoxGroupPriority.Text, _prevPriority)).ToString();
				else
					_prevPriority = 0;
			}
			catch (Exception)
			{
				textBoxGroupPriority.Text = @"0";
			}
			finally
			{
				textBoxGroupPriority.TextChanged += textBoxGroupPriority_TextChanged;
			}
		}

		private void ServerGroupForm_Resize(object sender, EventArgs e) => Refresh();
	}
}