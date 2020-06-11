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
    public sealed partial class CommonForm : UserControl, IUserForm
    {
	    private Dictionary<LogsReaderForm, ExpandCollapsePanel> AllExpanders { get; } = new Dictionary<LogsReaderForm, ExpandCollapsePanel>();
	    
	    public LogsReaderMainForm MainForm { get; private set; }

        public CommonForm()
        {
	        InitializeComponent();
        }

	    public void Initialize(LogsReaderMainForm main)
	    {
		    MainForm = main;

		    foreach (var readerForm in MainForm.AllForms.Values)
		    {
			    var expander = CreateExpander(readerForm);
                readerForm.OnTreeViewChanged += ReaderForm_OnTreeViewChanged;
                AllExpanders.Add(readerForm, expander);

                FlowPanelForExpanders.Controls.Add(expander);
		    }
	    }

        private void ReaderForm_OnTreeViewChanged(object sender, EventArgs e)
        {
            
        }

        ExpandCollapsePanel CreateExpander(LogsReaderForm readerForm)
        {
	        var schemeExpander = new ExpandCollapsePanel
	        {
		        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
		        BordersThickness = 3,
		        ButtonSize = ExpandButtonSize.Small,
		        ButtonStyle = ExpandButtonStyle.Circle,
		        CheckBoxShown = true,
		        ExpandedHeight = 200,
		        BackColor = Color.Azure,
		        HeaderBackColor = Color.Azure,
		        HeaderBorderBrush = Color.ForestGreen,
		        HeaderLineColor = Color.Black,
		        IsChecked = false,
		        IsExpanded = false,
		        Text = readerForm.CurrentSettings.Name,
		        UseAnimation = true
	        };

	        var buttonSize = Size = new Size(20, 17);

            var buttonBack = new Button
	        {
		        BackColor = Color.White,
		        FlatStyle = FlatStyle.Flat,
		        Location = new Point(3, 29),
		        Size = buttonSize,
		        UseVisualStyleBackColor = false
	        };
	        var labelBack = new Label {AutoSize = true, Location = new Point(25, 29), Size = new Size(34, 15), Text = @"Back"};

	        var buttonFore = new Button
	        {
		        BackColor = Color.Black,
		        FlatStyle = FlatStyle.Flat,
		        Location = new Point(63, 29),
		        Size = buttonSize,
		        UseVisualStyleBackColor = false
	        };
	        var labelFore = new Label {AutoSize = true, Location = new Point(85, 29), Size = new Size(34, 15), Text = @"Fore"};


	        var copy = new CustomTreeView
	        {
		        ImageList = readerForm.TreeMain.ImageList,
		        ItemHeight = readerForm.TreeMain.ItemHeight,
		        Indent = readerForm.TreeMain.Indent,
		        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
		        CheckBoxes = true,
		        DrawMode = TreeViewDrawMode.OwnerDrawAll,
		        Location = new Point(0, 50),
		        Size = new Size(schemeExpander.Size.Width, 150)
	        };

	        foreach (var treeNode in readerForm.TreeMain.Nodes.Cast<TreeNode>())
	        {
		        var clone = (TreeNode)treeNode.Clone();
		        copy.Nodes.Add(clone);
	        }

	        copy.MouseDown += readerForm.TreeMain_MouseDown;
	        copy.AfterCheck += readerForm.TrvMain_AfterCheck;


            schemeExpander.Controls.Add(buttonBack);
	        schemeExpander.Controls.Add(buttonFore);
	        schemeExpander.Controls.Add(labelBack);
            schemeExpander.Controls.Add(labelFore);
	        schemeExpander.Controls.Add(copy);



	        

	        return schemeExpander;
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