using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using LogsReader.Reader;
using Microsoft.Win32;
using SPAMassageSaloon.Common;
using Utils;
using Utils.Handles;

namespace LogsReader
{
	public partial class LogsReaderMainForm : Form, ISaloonForm
	{
		public const string MainFontFamily = "Segoe UI";
		public const string DgvFontFamily = "Courier New Bold";
		public const string FailedFontFamily = "Courier New Bold";

		public static readonly Font DefFont = new Font(MainFontFamily, 8.5F, FontStyle.Regular);
		public static readonly Font TxtFont = new Font(MainFontFamily, 10F, FontStyle.Regular);
		public static readonly Font DgvDataFont = new Font(DgvFontFamily, 8.25f, FontStyle.Regular);
		public static readonly Font DgvReaderFont = new Font(DgvFontFamily, 8, FontStyle.Regular);
		public static readonly Font ErrFont = new Font(FailedFontFamily, 8.25f, FontStyle.Bold);

		private const string GLOBAL_PAGE_NAME = "Global";

		private static readonly object credentialSync = new object();
		private static readonly Dictionary<string, CryptoNetworkCredential> _userCredentials;

		public static readonly Color GLOBAL_COLOR_BACK = Color.FromArgb(255, 0, 206);
		public static readonly Color GLOBAL_COLOR_FORE = Color.White;

		public static Color BODY_COLOR = Color.FromArgb(239, 239, 239);
		public static Color CONTENT_COLOR = Color.White;
		public static Color FOOTER_COLOR = Color.FromArgb(239, 239, 239);
		public static Color TEXT_COLOR = Color.Black;
		public static Color TAB_BACK_COLOR = Color.DimGray;
		public static Color TAB_FORE_COLOR = Color.White;
		public static Color BORDER_COLOR = Color.FromArgb(172, 172, 172);
		public static Color BUTTON_BACK_COLOR = Color.FromArgb(224, 224, 224);
		public static Color BUTTON_FORE_COLOR = Color.Black;
		public static Color SCHEME_DGV_ROW_BACK_COLOR_1 = Color.White;
		public static Color SCHEME_DGV_ROW_BACK_COLOR_2 = Color.FromArgb(245, 245, 245);
		public static Color SCHEME_DGV_ROW_FORE_COLOR = Color.Black;
		public static Color SCHEME_DGV_GRID_COLOR = SystemColors.ControlLight;

		public static readonly Color SCHEME_COLOR_BACK = Color.FromArgb(0, 200, 205);
		public static readonly Color SCHEME_COLOR_FORE = Color.White;

		public static readonly Color TRN_COLOR_BACK = Color.FromArgb(62, 255, 176);
		public static readonly Color TRN_COLOR_FORE = Color.Black;

		public static readonly Color READER_COLOR_BACK_ERROR = Color.FromArgb(255, 174, 174);
		public static readonly Color READER_COLOR_BACK_SUCCESS = Color.FromArgb(62, 255, 176);
		public static readonly Color READER_COLOR_BACK_ONPAUSE = Color.FromArgb(228, 255, 88);

		private static readonly string CredentialsRegName = "Credentials";

		internal static CryptoNetworkCredential GetLastCredentialItem()
		{
			lock (credentialSync)
				if (_userCredentials.Count > 0)
					return _userCredentials.Values.Last();

			return null;
		}

		internal static bool TryGetCredential(string serverRoot, out CryptoNetworkCredential result)
		{
			lock (credentialSync)
				return _userCredentials.TryGetValue(serverRoot, out result);
		}

		internal static void AddOrReplaceToNewCredential(string serverRoot, CryptoNetworkCredential credential)
		{
			lock (credentialSync)
				_userCredentials[serverRoot] = credential;
		}

		/// <summary>
		///     Настройки схем
		/// </summary>
		public static LRSettings Settings { get; private set; }

		public LogsReaderFormGlobal Global { get; }

		public Dictionary<TabPage, LogsReaderFormScheme> SchemeForms { get; } = new Dictionary<TabPage, LogsReaderFormScheme>(15);

		public LogsReaderFormBase CurrentForm
		{
			get
			{
				if (MainTabControl.SelectedTab != null)
				{
					if (SchemeForms.TryGetValue(MainTabControl.SelectedTab, out var current))
						return current;
					if (MainTabControl.SelectedTab.Name == GLOBAL_PAGE_NAME)
						return Global;
				}

				return null;
			}
		}

		public int ActiveProcessesCount => SchemeForms.Values.Count(x => x.Progress > 0 && x.Progress < 100 || x.Progress == 0 && x.IsWorking);

		public int ActiveTotalProgress
			=> SchemeForms.Values.Where(x => x.Progress > 0 && x.Progress < 100 || x.Progress == 0 && x.IsWorking).Sum(x => x.Progress);

		static LogsReaderMainForm()
		{
			try
			{
				using (var reg = new RegeditControl(typeof(LogsReaderMainForm).GetAssemblyInfo().ApplicationName))
				{
					var obj = reg[CredentialsRegName];

					if (obj is byte[] array)
					{
						using (var stream = new MemoryStream(array))
							_userCredentials = new BinaryFormatter().Deserialize(stream) as Dictionary<string, CryptoNetworkCredential>;
					}
				}
			}
			catch (Exception)
			{
				// ignored
			}
			finally
			{
				if (_userCredentials == null)
					_userCredentials = new Dictionary<string, CryptoNetworkCredential>();
			}
		}

		private static void SerializeUserCreditails()
		{
			try
			{
				using (var reg = new RegeditControl(typeof(LogsReaderMainForm).GetAssemblyInfo().ApplicationName))
				{
					MemoryStream stream;
					lock (credentialSync)
						stream = _userCredentials.SerializeToStream();

					using (stream)
						reg[CredentialsRegName, RegistryValueKind.Binary] = stream.ToArray();
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}

		public LogsReaderMainForm()
		{
			InitializeComponent();

			try
			{
				SuspendLayout();
				MainTabControl.SuspendLayout();
				MainTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
				base.Text = $"Logs Reader {this.GetAssemblyInfo().Version}";
				KeyPreview = true;
				Global = new LogsReaderFormGlobal(Encoding.UTF8) { Dock = DockStyle.Fill };
				Global.SuspendLayout();
				var globalPage = new TabPage
				{
					Name = GLOBAL_PAGE_NAME,
					Text = GLOBAL_PAGE_NAME,
					UseVisualStyleBackColor = false,
					Font = new Font(MainFontFamily, 8.5F),
					Margin = new Padding(0),
					Padding = new Padding(0)
				};
				globalPage.Controls.Add(Global);
				MainTabControl.TabPages.Add(globalPage);
				Settings = LRSettings.Deserialize();
				if (Settings.SchemeList == null)
					Settings.AssignDefaultSchemas();

				foreach (var scheme in Settings.SchemeList)
				{
					var schemeForm = new LogsReaderFormScheme(scheme) { Dock = DockStyle.Fill };

					try
					{
						schemeForm.SuspendLayout();
						var page = new TabPage
						{
							Text = scheme.Name,
							UseVisualStyleBackColor = false,
							Font = new Font(MainFontFamily, 8.5F),
							Margin = new Padding(0),
							Padding = new Padding(0)
						};
						page.Controls.Add(schemeForm);
						MainTabControl.TabPages.Add(page);
						SchemeForms.Add(page, schemeForm);
						schemeForm.Load += (sender, args) =>
						{
							schemeForm.ApplyFormSettings();
							schemeForm.OnSchemeChanged += SaveSchemas;
						};
					}
					catch (Exception ex)
					{
						ReportMessage.Show(string.Format(Resources.Txt_Main_ErrLoadScheme, scheme.Name, ex),
						                   MessageBoxIcon.Error,
						                   Resources.Txt_Main_LoadScheme);
					}
					finally
					{
						schemeForm.ResumeLayout();
					}
				}

				Global.Initialize(this);
				Global.Load += (sender, args) => { Global.ApplyFormSettings(); };
				MainTabControl.DrawItem += MainTabControl_DrawItem;
				LogsReaderFormBase prevSelection = null;
				MainTabControl.Selected += (sender, args) =>
				{
					try
					{
						var current = CurrentForm;

						if (current == Global)
						{
							foreach (var schemeForm in SchemeForms.Values)
								schemeForm.DeselectTransactions();
							Global.SelectTransactions();
						}
						else if (prevSelection == Global)
						{
							Global.DeselectTransactions();
							current.SelectTransactions();
						}
						else
						{
							current.SelectTransactions();
						}

						prevSelection = current;
					}
					catch (Exception)
					{
						// ignored
					}
				};
				Shown += (s, e) =>
				{
					ApplySettings();
					foreach (var schemeForm in SchemeForms.Values)
						schemeForm.SynchronizeTreeView();
				};
				Closing += (s, e) =>
				{
					SaveData();
					SerializeUserCreditails();
				};
			}
			catch (Exception ex)
			{
				ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, Resources.Txt_Initialization);
			}
			finally
			{
				CenterToScreen();
				Global?.ResumeLayout();
				MainTabControl.ResumeLayout();
				ResumeLayout();
			}
		}

		private async void SaveSchemas(object sender, EventArgs args)
		{
			if (Settings != null)
			{
				Settings.SchemeList = SchemeForms.Values.Select(x => x.CurrentSettings).ToArray();
				await LRSettings.SerializeAsync(Settings);
			}
		}

		private void MainTabControl_DrawItem(object sender, DrawItemEventArgs e)
		{
			var page = MainTabControl.TabPages[e.Index];

			if (page == MainTabControl.SelectedTab)
			{
				if (page.Name == GLOBAL_PAGE_NAME)
				{
					RenderTabPage(page, e, GLOBAL_COLOR_BACK, GLOBAL_COLOR_FORE);
				}
				else
				{
					if (SchemeForms.TryGetValue(page, out var reader))
						RenderTabPage(page, e, reader.FormBackColor, reader.FormForeColor);
					else
						RenderTabPage(page, e, SCHEME_COLOR_BACK, SCHEME_COLOR_FORE);
				}
			}
			else
			{
				RenderTabPage(page, e, TAB_BACK_COLOR, TAB_FORE_COLOR);
			}
		}

		private void RenderTabPage(TabPage page, DrawItemEventArgs e, Color fore, Color text)
		{
			e.Graphics.FillRectangle(new SolidBrush(fore), e.Bounds);
			var paddedBounds = e.Bounds;
			var yOffset = e.State == DrawItemState.Selected ? -2 : 1;
			paddedBounds.Offset(1, yOffset);
			TextRenderer.DrawText(e.Graphics, page.Text, e.Font, paddedBounds, text);
		}

		protected override void OnKeyDown(KeyEventArgs e) => CurrentForm?.LogsReaderKeyDown(this, e);

		public void ApplySettings()
		{
			try
			{
				foreach (var logsReader in SchemeForms.Values)
					logsReader.ApplySettings();
				Global.ApplySettings();
			}
			catch (Exception)
			{
				// ignored
			}
		}

		public void SaveData()
		{
			try
			{
				if (Settings != null)
					LRSettings.Serialize(Settings);
				Global.SaveData();
				foreach (var logsReader in SchemeForms.Values)
					logsReader.SaveData();
				Properties.Settings.Default.FormLocation = Location;
				Properties.Settings.Default.FormSize = Size;
				Properties.Settings.Default.FormState = WindowState;
				Properties.Settings.Default.Save();
			}
			catch (Exception)
			{
				// ignored
			}
		}

		public new async void Dispose()
		{
			try
			{
				foreach (var schemeForm in SchemeForms.Values)
				{
					schemeForm.OnSchemeChanged -= SaveSchemas;
					schemeForm.Clear();
				}

				Global.Clear();
			}
			catch (Exception)
			{
				// ignored
			}
			finally
			{
				await STREAM.GarbageCollectAsync().ConfigureAwait(false);
				base.Dispose();
			}
		}
	}
}