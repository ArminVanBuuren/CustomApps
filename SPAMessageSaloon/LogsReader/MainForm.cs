using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using LogsReader.Reader;
using Microsoft.WindowsAPICodePack.Taskbar;
using SPAMessageSaloon.Common;
using Utils;
using Utils.WinForm;

namespace LogsReader
{
    public partial class MainForm : Form, ISPAMessageSaloonItems
    {
        private LogsReaderForm _current;
        /// <summary>
        /// Настройки схем
        /// </summary>
        public LRSettings AllSettings { get; }

        public Dictionary<TabPage, LogsReaderForm> AllForms { get; }

        public LogsReaderForm CurrentForm
        {
            get
            {
                return this.SafeInvoke(() =>
                {
                    if (MainTabControl.SelectedTab != null && AllForms.TryGetValue(MainTabControl.SelectedTab, out var current))
                        return current;
                    return _current;
                });
            }
        }

        public int ActiveProcessesCount => AllForms.Values.Count(x => (x.Progress > 0 && x.Progress < 100) || (x.Progress == 0 && x.IsWorking));

        public int ActiveTotalProgress => AllForms.Values.Where(x => (x.Progress > 0 && x.Progress < 100) || (x.Progress == 0 && x.IsWorking)).Sum(x => x.Progress);

        public MainForm()
        {
            InitializeComponent();

            try
            {
                base.Text = $"Logs Reader {ASSEMBLY.CurrentVersion}";
                KeyPreview = true;
                KeyDown += MainForm_KeyDown;
                Closing += (s, e) =>
                {
                    if (AllSettings != null)
                        LRSettings.Serialize(AllSettings);
                    if (AllForms != null)
                        foreach (var form in AllForms.Values)
                            form.SaveInterfaceParams();
                    SaveInterfaceParams();
                };
                Shown += (s, e) =>
                {
                    if (AllForms == null)
                        return;
                    foreach (var form in AllForms)
                        form.Value.ApplySettings();
                };


                MainTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
                MainTabControl.DrawItem += MainTabControl_DrawItem;

                AllSettings = LRSettings.Deserialize();
                AllForms = new Dictionary<TabPage, LogsReaderForm>(AllSettings.SchemeList.Length);

                foreach (var scheme in AllSettings.SchemeList)
                {
                    try
                    {
                        var logsReader = new LogsReaderForm();
                        logsReader.LoadForm(scheme);
                        logsReader.Dock = DockStyle.Fill;
                        logsReader.OnSchemeChanged += SaveSchemas;

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
                        ReportMessage.Show($"Failed to load schema \"{scheme.Name}\"\r\n{ex}", MessageBoxIcon.Error, @"Load scheme");
                    }
                }
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, @"Initialization");
            }
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

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            CurrentForm?.LogsReaderKeyDown(sender, e);
        }

        public void ApplySettings()
        {
            try
            {
                if (!IsMdiChild)
                {
                    WindowState = Settings.Default.FormState;
                    if (WindowState != FormWindowState.Maximized &&
                        (Settings.Default.FormSize.Height < 100 ||
                         Settings.Default.FormSize.Width < 100 ||
                         Settings.Default.FormLocation.X < 0 ||
                         Settings.Default.FormLocation.Y < 0))
                    { WindowState = FormWindowState.Maximized; return; }
                    else if (Settings.Default.FormSize.Height > 300 && Settings.Default.FormSize.Width > 300)
                        Size = Settings.Default.FormSize;

                    CenterToScreen();
                    //Location = Settings.Default.FormLocation;
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        async void SaveSchemas(object sender, EventArgs args)
        {
            if (AllSettings != null)
                await LRSettings.SerializeAsync(AllSettings);
        }

        void SaveInterfaceParams()
        {
            try
            {
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

        public void ChangeLanguage(NationalLanguage language)
        {
            
        }
    }
}