using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using LogsReader.Reader;
using SPAMassageSaloon.Common;
using Utils;

namespace LogsReader
{
    public partial class LogsReaderMainForm : Form, ISaloonForm
    {
        /// <summary>
        /// Настройки схем
        /// </summary>
        public LRSettings AllSettings { get; }

        public Dictionary<TabPage, LogsReaderForm> AllForms { get; } = new Dictionary<TabPage, LogsReaderForm>(15);

        public LogsReaderForm CurrentForm
        {
            get
            {
                if (MainTabControl.SelectedTab != null && AllForms.TryGetValue(MainTabControl.SelectedTab, out var current))
                    return current;
                return null;
            }
        }

        public int ActiveProcessesCount => AllForms.Values.Count(x => (x.Progress > 0 && x.Progress < 100) || (x.Progress == 0 && x.IsWorking));

        public int ActiveTotalProgress => AllForms.Values.Where(x => (x.Progress > 0 && x.Progress < 100) || (x.Progress == 0 && x.IsWorking)).Sum(x => x.Progress);

        public LogsReaderMainForm()
        {
            InitializeComponent();

            try
            {
                base.Text = $"Logs Reader {this.GetAssemblyInfo().CurrentVersion}";

                KeyPreview = true;
                Closing += (s, e) => { SaveData(); };
                Shown += (s, e) =>
                {
                    ApplySettings();
                    foreach (var logsReader in AllForms.Values)
                    {
                        logsReader.ApplyFormSettings();
                        logsReader.OnSchemeChanged += SaveSchemas;
                    }
                };

                MainTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
                MainTabControl.DrawItem += MainTabControl_DrawItem;

                AllSettings = LRSettings.Deserialize();

                foreach (var scheme in AllSettings.SchemeList)
                {
                    try
                    {
                        var logsReader = new LogsReaderForm(scheme)
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
                        ReportMessage.Show(string.Format(Properties.Resources.Txt_Main_ErrLoadScheme, scheme.Name, ex), MessageBoxIcon.Error, Properties.Resources.Txt_Main_LoadScheme);
                    }
                }
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, Properties.Resources.Txt_Initialization);
            }
            finally
            {
                CenterToScreen();
            }
        }

        async void SaveSchemas(object sender, EventArgs args)
        {
            if (AllSettings != null)
                await LRSettings.SerializeAsync(AllSettings);
        }

        private void MainTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = MainTabControl.TabPages[e.Index];
            if (page == MainTabControl.SelectedTab)
            {
                RenderTabPage(page, e, Color.FromArgb(0, 200, 205), Color.White);
            }
            else
            {
                RenderTabPage(page, e, Color.DimGray, Color.White);
            }
        }

        void RenderTabPage(TabPage page, DrawItemEventArgs e, Color fore, Color text)
        {
            e.Graphics.FillRectangle(new SolidBrush(fore), e.Bounds);

            Rectangle paddedBounds = e.Bounds;
            int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
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
                if (AllSettings != null)
                    LRSettings.Serialize(AllSettings);

                foreach (var logsReader in AllForms.Values)
                {
                    logsReader.SaveData();
                }

                Settings.Default.FormLocation = Location;
                Settings.Default.FormSize = Size;
                Settings.Default.FormState = WindowState;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                // ignored
            }
        }
    }
}