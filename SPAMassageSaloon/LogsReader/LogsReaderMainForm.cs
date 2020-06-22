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
	    private const string GLOBAL_PAGE_NAME = "Global";

        private static readonly object credentialSync = new object();
	    private static readonly Dictionary<CryptoNetworkCredential, DateTime> _userCredentials;

	    public static readonly Color GLOBAL_COLOR_BACK = Color.FromArgb(255, 0, 206);
	    public static readonly Color GLOBAL_COLOR_FORE = Color.White;
	    public static readonly Color SCHEME_COLOR_BACK = Color.FromArgb(0, 200, 205);
	    public static readonly Color SCHEME_COLOR_FORE = Color.White;

	    public static Dictionary<CryptoNetworkCredential, DateTime> Credentials
	    {
		    get
		    {
			    lock (credentialSync)
				    return _userCredentials;
		    }
	    }

	    /// <summary>
        /// Настройки схем
        /// </summary>
        public static LRSettings Settings { get; private set; }

        public LogsReaderFormGlobal Global { get; }

        public Dictionary<TabPage, LogsReaderFormScheme> AllForms { get; } = new Dictionary<TabPage, LogsReaderFormScheme>(15);

        public LogsReaderFormBase CurrentForm
        {
            get
            {
	            if (MainTabControl.SelectedTab != null)
	            {
		            if (AllForms.TryGetValue(MainTabControl.SelectedTab, out var current))
			            return current;
		            else if (MainTabControl.SelectedTab.Name == GLOBAL_PAGE_NAME)
			            return Global;
	            }

	            return null;
            }
        }

        public int ActiveProcessesCount => AllForms.Values.Count(x => (x.Progress > 0 && x.Progress < 100) || (x.Progress == 0 && x.IsWorking));

        public int ActiveTotalProgress => AllForms.Values.Where(x => (x.Progress > 0 && x.Progress < 100) || (x.Progress == 0 && x.IsWorking)).Sum(x => x.Progress);

        static LogsReaderMainForm()
        {
	        try
	        {
		        using (var reg = new RegeditControl(typeof(LogsReaderMainForm).GetAssemblyInfo().ApplicationName))
		        {
			        var obj = reg[nameof(Credentials)];
			        if (obj is byte[] array)
			        {
				        using (var stream = new MemoryStream(array))
					        _userCredentials = new BinaryFormatter().Deserialize(stream) as Dictionary<CryptoNetworkCredential, DateTime>;
			        }
		        }
	        }
	        catch (Exception ex)
	        {
		        // ignored
	        }
	        finally
	        {
                if(_userCredentials == null)
	                _userCredentials = new Dictionary<CryptoNetworkCredential, DateTime>();
            }
        }

        static void SerializeUserCreditails()
        {
	        try
	        {
		        using (var reg = new RegeditControl(typeof(LogsReaderMainForm).GetAssemblyInfo().ApplicationName))
		        using (var stream = Credentials.SerializeToStream())
			        reg[nameof(Credentials), RegistryValueKind.Binary] = stream.ToArray();
	        }
	        catch (Exception ex)
	        {
		        // ignored
	        }
        }

        public LogsReaderMainForm()
        {
            InitializeComponent();

            try
            {
                base.Text = $"Logs Reader {this.GetAssemblyInfo().Version}";
                KeyPreview = true;

                MainTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;

                Global = new LogsReaderFormGlobal(Encoding.UTF8)
                {
	                Dock = DockStyle.Fill
                };
                var globalPage = new TabPage
                {
	                Name = GLOBAL_PAGE_NAME,
	                Text = GLOBAL_PAGE_NAME,
	                UseVisualStyleBackColor = false,
	                Font = new Font("Segoe UI", 8.5F),
	                Margin = new Padding(0),
	                Padding = new Padding(0)
                };
                globalPage.Controls.Add(Global);
                MainTabControl.TabPages.Add(globalPage);

                Settings = LRSettings.Deserialize();
                foreach (var scheme in Settings.SchemeList)
                {
                    try
                    {
                        var logsReader = new LogsReaderFormScheme(scheme)
                        {
                            Dock = DockStyle.Fill
                        };

                        var page = new TabPage
                        {
                            Text = scheme.Name,
                            UseVisualStyleBackColor = false,
                            Font = new Font("Segoe UI", 8.5F),
                            Margin = new Padding(0),
                            Padding = new Padding(0)
                        };
                        page.Controls.Add(logsReader);

                        MainTabControl.TabPages.Add(page);
                        AllForms.Add(page, logsReader);
                    }
                    catch (Exception ex)
                    {
                        ReportMessage.Show(string.Format(Resources.Txt_Main_ErrLoadScheme, scheme.Name, ex), MessageBoxIcon.Error, Resources.Txt_Main_LoadScheme);
                    }
                }

                Global.Initialize(this);


                MainTabControl.DrawItem += MainTabControl_DrawItem;
                Shown += (s, e) =>
                {
	                ApplySettings();
	                Global.ApplyFormSettings();

	                foreach (var logsReader in AllForms.Values)
	                {
		                logsReader.ApplyFormSettings();
		                logsReader.OnSchemeChanged += SaveSchemas;
	                }
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
            }
        }

        static async void SaveSchemas(object sender, EventArgs args)
        {
            if (Settings != null)
                await LRSettings.SerializeAsync(Settings);
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
		            if (AllForms.TryGetValue(page, out var reader))
			            RenderTabPage(page, e, reader.UserSettings.BackColor, reader.UserSettings.ForeColor);
                    else
			            RenderTabPage(page, e, SCHEME_COLOR_BACK, SCHEME_COLOR_FORE);
                }
            }
            else
            {
                RenderTabPage(page, e, Color.DimGray, Color.White);
            }
        }

        void RenderTabPage(TabPage page, DrawItemEventArgs e, Color fore, Color text)
        {
            e.Graphics.FillRectangle(new SolidBrush(fore), e.Bounds);

            var paddedBounds = e.Bounds;
            var yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
            paddedBounds.Offset(1, yOffset);
            TextRenderer.DrawText(e.Graphics, page.Text, e.Font, paddedBounds, text);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            CurrentForm?.LogsReaderKeyDown(this, e);
        }

        public void ApplySettings()
        {
            try
            {
	            Global.ApplySettings();
                foreach (var logsReader in AllForms.Values)
                    logsReader.ApplySettings();
            }
            catch (Exception ex)
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
		        foreach (var logsReader in AllForms.Values)
			        logsReader.SaveData();

		        Properties.Settings.Default.FormLocation = Location;
		        Properties.Settings.Default.FormSize = Size;
		        Properties.Settings.Default.FormState = WindowState;
		        Properties.Settings.Default.Save();
	        }
	        catch (Exception ex)
	        {
		        // ignored
	        }
        }
    }
}