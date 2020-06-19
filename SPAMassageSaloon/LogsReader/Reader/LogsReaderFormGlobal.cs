using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogsReader.Config;
using Utils.WinForm;
using Utils.WinForm.Expander;

namespace LogsReader.Reader
{
    public class LogsReaderFormGlobal : LogsReaderFormBase
	{
	    private Dictionary<LogsReaderFormScheme, (ExpandCollapsePanel, CustomTreeView)> AllExpanders { get; } = new Dictionary<LogsReaderFormScheme, (ExpandCollapsePanel, CustomTreeView)>();
	    
	    public LogsReaderMainForm MainForm { get; private set; }

		public override bool HasAnyResult => false;

		private readonly Panel PanelFlowDoc;
	    private readonly AdvancedFlowLayoutPanel FlowPanelForExpanders;
	    private readonly CheckBox checkBoxSelectAll;

		public LogsReaderFormGlobal(Encoding defaultEncoding) :base(defaultEncoding, new UserSettings())
        {
	        FlowPanelForExpanders = new AdvancedFlowLayoutPanel
	        {
		        Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right,
		        Location = new Point(0, -1),
		        Margin = new Padding(0),
		        Name = "FlowPanelForExpanders",
		        Size = new Size(147, 3623),
		        TabIndex = 27
	        };

	        PanelFlowDoc = new Panel
	        {
		        AutoScroll = true,
		        AutoScrollMinSize = new Size(0, 1500),
		        Dock = DockStyle.Fill,
		        Location = new Point(0, 27),
		        Name = "PanelFlowDoc",
		        Size = new Size(181, 439),
		        TabIndex = 29
	        };
	        PanelFlowDoc.Controls.Add(FlowPanelForExpanders);

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

			MainSplitContainer.Panel1.Controls.Add(PanelFlowDoc);
			MainSplitContainer.Panel1.Controls.Add(panelCollapseSelectAll);
			MainSplitContainer.Panel1MinSize = 110;
        }

	    public void Initialize(LogsReaderMainForm main)
	    {
		    MainForm = main;

		    foreach (var readerForm in MainForm.AllForms.Values)
		    {
			    var expander = CreateExpander(readerForm);
			    AllExpanders.Add(readerForm, expander);

                FlowPanelForExpanders.Controls.Add(expander.Item1);
		    }

		    SchemeExpander_ExpandCollapse(this, null);
	    }

        (ExpandCollapsePanel, CustomTreeView) CreateExpander(LogsReaderFormScheme readerForm)
        {
	        var schemeExpander = new ExpandCollapsePanel
	        {
		        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
		        BordersThickness = 0,
		        ButtonSize = ExpandButtonSize.Small,
		        ButtonStyle = ExpandButtonStyle.Circle,
		        CheckBoxShown = true,
		        ExpandedHeight = 300,
		        BackColor = Color.ForestGreen,
		        HeaderBackColor = Color.Azure,
		        HeaderBorderBrush = Color.ForestGreen,
		        HeaderLineColor = Color.White,
		        IsChecked = false,
		        IsExpanded = false,
		        UseAnimation = false,
		        Text = readerForm.CurrentSettings.Name,
		        Padding = new Padding(2),
                Margin = new Padding(2, 2, 2, 0)
	        };
            schemeExpander.ExpandCollapse += SchemeExpander_ExpandCollapse;

            var panel = new Panel
	        {
		        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Azure,
                Location = new Point(2, 23),
                Size = new Size(schemeExpander.Size.Width - 4, 275)
            };

	        schemeExpander.Controls.Add(panel);


            var buttonSize = new Size(20, 17);

            var buttonBack = new Button
	        {
		        BackColor = Color.White,
		        FlatStyle = FlatStyle.Flat,
		        Location = new Point(3, 3),
		        Size = buttonSize,
		        UseVisualStyleBackColor = false
	        };
	        var labelBack = new Label {AutoSize = true, Location = new Point(25, 3), Size = new Size(34, 15), Text = @"Back"};

	        var buttonFore = new Button
	        {
		        BackColor = Color.Black,
		        FlatStyle = FlatStyle.Flat,
		        Location = new Point(63, 3),
		        Size = buttonSize,
		        UseVisualStyleBackColor = false
	        };
	        var labelFore = new Label {AutoSize = true, Location = new Point(85, 3), Size = new Size(34, 15), Text = @"Fore"};

	        var treeView = readerForm.TreeViewContainer.CreateNewCopy();
	        //readerForm.TreeViewContainer.OnError += ;

            treeView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
	        treeView.DrawMode = TreeViewDrawMode.OwnerDrawAll;
	        treeView.Location = new Point(-1, 23);
	        treeView.Size = new Size(schemeExpander.Size.Width - 3, 253);
	        treeView.KeyDown += (sender, args) =>
	        {
		        readerForm.TreeViewContainer.MainFormKeyDown(treeView, args);
            };


	        panel.Controls.Add(buttonBack);
	        panel.Controls.Add(buttonFore);
	        panel.Controls.Add(labelBack);
	        panel.Controls.Add(labelFore);
	        panel.Controls.Add(treeView);

	        return (schemeExpander, treeView);
        }

        protected override void OnResize(EventArgs e)
        {
	        base.OnResize(e);
	        SchemeExpander_ExpandCollapse(this, null);
        }

        private void SchemeExpander_ExpandCollapse(object sender, ExpandCollapseEventArgs e)
        {
			if(FlowPanelForExpanders == null)
				return;

	        var height = 0;
	        foreach (var expander in FlowPanelForExpanders.Controls.OfType<ExpandCollapsePanel>())
	        {
		        if (expander.IsExpanded)
			        height += expander.ExpandedHeight;
		        else
			        height += expander.CollapsedHeight;

		        height += expander.Margin.Top + expander.Margin.Bottom;
	        }

            PanelFlowDoc.AutoScrollMinSize = new Size(0, height);
            
            if (PanelFlowDoc.VerticalScroll.Visible)
            {
	            FlowPanelForExpanders.Size = new Size(PanelFlowDoc.Size.Width - 16, height);
	            checkBoxSelectAll.Padding = new Padding(0, 0, 22, 0);
            }
            else
            {
	            FlowPanelForExpanders.Size = new Size(PanelFlowDoc.Size.Width - 2, height);
	            checkBoxSelectAll.Padding = new Padding(0, 0, 8, 0);
            }
        }

		protected override void BtnSearch_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		protected override Task<bool> AssignResult(DataFilter filter)
		{
			throw new NotImplementedException();
		}

		protected override bool TryGetTemplate(DataGridViewRow row, out DataTemplate template)
		{
			throw new NotImplementedException();
		}
	}
}