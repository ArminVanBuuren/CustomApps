using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using Utils;
using Utils.WinForm.Expander;

namespace LogsReader.Reader
{
    public class LogsReaderFormGlobal : LogsReaderFormBase
	{
		private readonly Panel panelFlowDoc;
		private readonly AdvancedFlowLayoutPanel flowPanelForExpanders;
		private readonly CheckBox checkBoxSelectAll;

		private Dictionary<LogsReaderFormScheme, ExpandCollapsePanel> AllExpanders { get; } = new Dictionary<LogsReaderFormScheme, ExpandCollapsePanel>();
	    
	    public LogsReaderMainForm MainForm { get; private set; }

		public override bool HasAnyResult => dgvFiles.RowCount > 0 
		                                     && AllExpanders.Any(x => x.Key.HasAnyResult && x.Value.IsChecked);

		public LogsReaderFormGlobal(Encoding defaultEncoding) : base(defaultEncoding, new UserSettings())
        {
			#region Initialize Controls

			flowPanelForExpanders = new AdvancedFlowLayoutPanel
	        {
		        Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right,
		        Location = new Point(0, -1),
		        Margin = new Padding(0),
		        Name = "FlowPanelForExpanders",
		        Size = new Size(147, 3623),
		        TabIndex = 27
	        };

	        panelFlowDoc = new Panel
	        {
		        AutoScroll = true,
		        AutoScrollMinSize = new Size(0, 1500),
		        Dock = DockStyle.Fill,
		        Location = new Point(0, 27),
		        Name = "PanelFlowDoc",
		        Size = new Size(181, 439),
		        TabIndex = 29
	        };
	        panelFlowDoc.Controls.Add(flowPanelForExpanders);

	        checkBoxSelectAll = new CheckBox
	        {
		        AutoSize = true,
		        CheckAlign = ContentAlignment.MiddleRight,
		        Dock = DockStyle.Right,
		        Location = new Point(80, 3),
		        Name = "checkBoxSelectAll",
		        Padding = new Padding(0, 0, 22, 0),
		        Size = new Size(96, 19),
		        TabIndex = 1,
		        Text = "Select All",
		        UseVisualStyleBackColor = true
	        };
	        checkBoxSelectAll.CheckedChanged += (sender, args) =>
	        {
		        foreach (var expander in AllExpanders.Values.Where(expander => expander.CheckBoxEnabled))
			        expander.IsChecked = checkBoxSelectAll.Checked;
	        };

			var panelCollapseSelectAll = new Panel
	        {
		        BorderStyle = BorderStyle.FixedSingle,
		        Dock = DockStyle.Top,
		        Location = new Point(0, 0),
		        Name = "panelCollapseSelectAll",
		        Padding = new Padding(3),
		        Size = new Size(181, 27),
		        TabIndex = 28
	        };
			panelCollapseSelectAll.Controls.Add(checkBoxSelectAll);

			MainSplitContainer.Panel1.Controls.Add(panelFlowDoc);
			MainSplitContainer.Panel1.Controls.Add(panelCollapseSelectAll);
			MainSplitContainer.Panel1MinSize = 110;

			#endregion
		}

		public void Initialize(LogsReaderMainForm main)
	    {
		    MainForm = main;

		    foreach (var readerForm in MainForm.AllForms.Values)
		    {
			    var expander = CreateExpander(readerForm);
			    AllExpanders.Add(readerForm, expander);
			    flowPanelForExpanders.Controls.Add(expander);
		    }

		    SchemeExpander_ExpandCollapse(this, null);
	    }

		ExpandCollapsePanel CreateExpander(LogsReaderFormScheme readerForm)
		{
			Button buttonBack = null;
			Button buttonFore = null;
			ExpandCollapsePanel schemeExpander = null;

			Func<Color> expanderBorderColor = () => readerForm.BTNSearch.Enabled ? Color.ForestGreen : Color.Red;
			Func<Color> expanderPanelColor = () => readerForm.BTNSearch.Enabled ? Color.FromArgb(217, 255, 217) : Color.FromArgb(255, 150, 170);

			var colorDialog = new ColorDialog
			{
				AllowFullOpen = true,
				FullOpen = true, 
				CustomColors = new[]
				{
					ColorTranslator.ToOle(LogsReaderMainForm.SCHEME_COLOR_BACK),
					ColorTranslator.ToOle(LogsReaderMainForm.SCHEME_COLOR_FORE)
				}
			};

			void ChangeColor(object sender, EventArgs e)
			{
				if (!(sender is Button button))
					return;

				colorDialog.Color = button.BackColor;

				if (colorDialog.ShowDialog() != DialogResult.OK)
					return;

				button.BackColor = colorDialog.Color;

				if (button == buttonBack)
				{
					readerForm.UserSettings.BackColor = colorDialog.Color;
					schemeExpander.HeaderBackColor = colorDialog.Color;
				}
				else if (button == buttonFore)
				{
					readerForm.UserSettings.ForeColor = colorDialog.Color;
					schemeExpander.ForeColor = colorDialog.Color;
				}
			}

			var buttonSize = new Size(20, 17);

			var labelBack = new Label { AutoSize = true, ForeColor = Color.Black, Location = new Point(25, 3), Size = new Size(34, 15), Text = @"Back" };
			buttonBack = new Button
			{
				BackColor = readerForm.UserSettings.BackColor,
				FlatStyle = FlatStyle.Flat,
				Location = new Point(3, 3),
				Size = buttonSize,
				UseVisualStyleBackColor = false
			};
			buttonBack.Click += ChangeColor;

			var labelFore = new Label { AutoSize = true, ForeColor = Color.Black, Location = new Point(85, 3), Size = new Size(34, 15), Text = @"Fore" };
			buttonFore = new Button
			{
				BackColor = readerForm.UserSettings.ForeColor,
				FlatStyle = FlatStyle.Flat,
				Location = new Point(63, 3),
				Size = buttonSize,
				UseVisualStyleBackColor = false
			};
			buttonFore.Click += ChangeColor;

			schemeExpander = new ExpandCollapsePanel
	        {
		        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
		        BordersThickness = 0,
		        ButtonSize = ExpandButtonSize.Small,
		        ButtonStyle = ExpandButtonStyle.Circle,
		        CheckBoxShown = true,
		        ExpandedHeight = 300,
		        CheckBoxEnabled = readerForm.BTNSearch.Enabled,
				BackColor = expanderBorderColor.Invoke(),
				HeaderBorderBrush = Color.Azure,
		        HeaderBackColor = buttonBack.BackColor,
		        ForeColor = buttonFore.BackColor,
				HeaderLineColor = Color.White,
				Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
		        IsChecked = false,
		        IsExpanded = false,
		        UseAnimation = false,
		        Text = readerForm.CurrentSettings.Name,
		        Padding = new Padding(2),
                Margin = new Padding(3, 3, 3, 0)
	        };
			
			var expanderPanel = new Panel
	        {
		        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = expanderPanelColor.Invoke(),
                Location = new Point(2, 23),
                Size = new Size(schemeExpander.Size.Width - 4, 275)
            };
			schemeExpander.Controls.Add(expanderPanel);

			var treeView = readerForm.TreeViewContainer.CreateNewCopy();
	        treeView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
	        treeView.DrawMode = TreeViewDrawMode.OwnerDrawAll;
	        treeView.Location = new Point(-1, 23);
	        treeView.Size = new Size(schemeExpander.Size.Width - 2, 253);

	        expanderPanel.Controls.Add(buttonBack);
	        expanderPanel.Controls.Add(buttonFore);
	        expanderPanel.Controls.Add(labelBack);
	        expanderPanel.Controls.Add(labelFore);
	        expanderPanel.Controls.Add(treeView);

			// events
	        schemeExpander.ExpandCollapse += SchemeExpander_ExpandCollapse;
	        schemeExpander.CheckedChanged += (sender, args) =>
	        {
				if(!schemeExpander.IsChecked && schemeExpander.CheckBoxEnabled)
					schemeExpander.BackColor = Color.DimGray;
				else
					schemeExpander.BackColor = expanderBorderColor.Invoke();
				ValidationCheck(true);
	        };
	        treeView.KeyDown += (sender, args) =>
	        {
		        readerForm.TreeViewContainer.MainFormKeyDown(treeView, args);
	        };
	        readerForm.TreeViewContainer.OnError += ex =>
	        {
		        ReportStatus(ex.Message, ReportStatusType.Error);
	        };
			readerForm.BTNSearch.EnabledChanged += (sender, args) =>
			{
				schemeExpander.BackColor = expanderBorderColor.Invoke();
				expanderPanel.BackColor = expanderPanelColor.Invoke();

				if (readerForm.BTNSearch.Enabled)
				{
					schemeExpander.CheckBoxEnabled = true;
					if (schemeExpander.IsChecked != checkBoxSelectAll.Checked)
						schemeExpander.IsChecked = checkBoxSelectAll.Checked;
					else
						ValidationCheck(true);
				}
				else
				{
					schemeExpander.CheckBoxEnabled = false;
					if (schemeExpander.IsChecked)
						schemeExpander.IsChecked = false;
					else
						ValidationCheck(true);
				}
			};
			readerForm.OnSearchCompleted += ReaderForm_OnSearchCompleted;

			return schemeExpander;
        }

		protected override void OnResize(EventArgs e)
        {
	        base.OnResize(e);
	        SchemeExpander_ExpandCollapse(this, null);
        }

        private void SchemeExpander_ExpandCollapse(object sender, ExpandCollapseEventArgs e)
        {
			if(flowPanelForExpanders == null)
				return;

	        var height = 0;
	        foreach (var expander in flowPanelForExpanders.Controls.OfType<ExpandCollapsePanel>())
	        {
		        if (expander.IsExpanded)
			        height += expander.ExpandedHeight;
		        else
			        height += expander.CollapsedHeight;

		        height += expander.Margin.Top + expander.Margin.Bottom;
	        }

            panelFlowDoc.AutoScrollMinSize = new Size(0, height);
            
            if (panelFlowDoc.VerticalScroll.Visible)
            {
	            flowPanelForExpanders.Size = new Size(panelFlowDoc.Size.Width - 16, height);
	            checkBoxSelectAll.Padding = new Padding(0, 0, 23, 0);
            }
            else
            {
	            flowPanelForExpanders.Size = new Size(panelFlowDoc.Size.Width - 2, height);
	            checkBoxSelectAll.Padding = new Padding(0, 0, 9, 0);
            }
        }

        private Dictionary<LogsReaderFormScheme, Task> _processing;

        internal override void BtnSearch_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				try
				{
					IsWorking = true;
					_processing = new Dictionary<LogsReaderFormScheme, Task>();

					ReportStatus(Resources.Txt_LogsReaderForm_Working, ReportStatusType.Success);

					foreach (var (readerForm, expander) in AllExpanders.Where(x => x.Value.IsChecked))
					{
						if (readerForm.IsWorking) 
							continue;

						_processing.Add(readerForm, Task.Factory.StartNew(() => readerForm.BtnSearch_Click(this, EventArgs.Empty)));
					}
				}
				catch (Exception ex)
				{
					ReportStatus(ex.Message, ReportStatusType.Error);
				}
			}
			else
			{
				foreach (var (readerForm, expander) in AllExpanders.Where(x => x.Value.IsChecked))
					if (readerForm.IsWorking)
						readerForm.BtnSearch_Click(this, EventArgs.Empty);

				ReportStatus(Resources.Txt_LogsReaderForm_Stopping, ReportStatusType.Success);
			}
		}

        private void ReaderForm_OnSearchCompleted(object sender, EventArgs e)
        {
	        if (_processing.Any(x => x.Key.IsWorking))
				return;

			IsWorking = false;
		}

		protected override Task<bool> AssignResult(DataFilter filter)
		{
			throw new NotImplementedException();
		}

		protected override bool TryGetTemplate(DataGridViewRow row, out DataTemplate template)
		{
			throw new NotImplementedException();
		}

		protected override void DgvFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			throw new NotImplementedException();
		}

		protected override void ValidationCheck(bool clearStatus)
		{
			BTNSearch.Enabled = !txtPattern.Text.IsNullOrEmpty() 
			                    && AllExpanders.Any(x => x.Key.BTNSearch.Enabled && x.Value.IsChecked);
			base.ValidationCheck(clearStatus);
		}
	}
}