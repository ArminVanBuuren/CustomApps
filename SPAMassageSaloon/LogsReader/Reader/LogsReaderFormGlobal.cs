using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using LogsReader.Config;
using SPAMassageSaloon.Common;
using Utils;
using Utils.WinForm;
using Utils.WinForm.Expander;

namespace LogsReader.Reader
{
    public sealed partial class LogsReaderFormGlobal : UserControl, IUserForm
    {
	    private Dictionary<LogsReaderFormScheme, (ExpandCollapsePanel, CustomTreeView)> AllExpanders { get; } = new Dictionary<LogsReaderFormScheme, (ExpandCollapsePanel, CustomTreeView)>();
	    
	    public LogsReaderMainForm MainForm { get; private set; }

        public LogsReaderFormGlobal()
        {
	        InitializeComponent();
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

        private void ReaderForm_OnTreeViewChanged(object sender, EventArgs e)
        {
	        //if (!(sender is LogsReaderForm readerForm) || !AllExpanders.TryGetValue(readerForm, out var expander))
		       // return;

         //   expander.Item2.Nodes.Clear();
	        //foreach (var treeNode in readerForm.TreeMain.Nodes.Cast<TreeNode>())
		       // expander.Item2.Nodes.Add((TreeNode)treeNode.Clone());
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

        public void ApplySettings()
        {
            
        }

        public void SaveData()
        {

        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {

        }

        private async void ButtonExport_Click(object sender, EventArgs e)
        {
        }

        private async void buttonFilter_Click(object sender, EventArgs e)
        {

        }

        private async void buttonReset_Click(object sender, EventArgs e)
        {
            
        }

        private void DgvFiles_SelectionChanged(object sender, EventArgs e)
        {
            
        }

        private void DgvFiles_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void OrderByText_Leave(object sender, EventArgs e)
        {

        }

        private void TxtPattern_TextChanged(object sender, EventArgs e)
        {

        }

        private void UseRegex_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void TraceNameFilter_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void traceNameFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void TraceMessageFilter_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void traceMessageFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            
        }
    }
}