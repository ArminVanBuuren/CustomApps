using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LogsReader.Config;
using SPAMassageSaloon.Common;
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
		    var buttonBack = new Button
		    {
			    BackColor = Color.White,
			    FlatStyle = FlatStyle.Flat,
			    Location = new Point(3, 4),
			    Size = new Size(20, 17),
			    UseVisualStyleBackColor = false
		    };
		    var labelBack = new Label {AutoSize = true, Location = new Point(25, 4), Size = new Size(34, 15), Text = @"Back"};

		    var buttonFore = new Button
		    {
			    BackColor = Color.Black,
			    FlatStyle = FlatStyle.Flat,
			    Location = new Point(63, 4),
			    Size = new Size(20, 17),
			    UseVisualStyleBackColor = false
		    };
		    var labelFore = new Label {AutoSize = true, Location = new Point(85, 4), Size = new Size(32, 15), Text = @"Fore"};

		    var splitContainerInner = new SplitContainer
		    {
			    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
			    FixedPanel = FixedPanel.Panel1,
			    IsSplitterFixed = true,
			    Location = new Point(0, 24),
			    Orientation = Orientation.Horizontal,
			    AutoSize = true,
			    SplitterDistance = 25
		    };
		    splitContainerInner.Panel1.Controls.Add(buttonBack);
		    splitContainerInner.Panel1.Controls.Add(labelBack);
		    splitContainerInner.Panel1.Controls.Add(buttonFore);
            splitContainerInner.Panel1.Controls.Add(labelFore);
            splitContainerInner.Panel2.Controls.Add(readerForm.TreeMain);

            var schemeExpander = new ExpandCollapsePanel
		    {
			    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
			    BackColor = SystemColors.Control,
			    BordersThickness = 3,
			    ButtonSize = ExpandButtonSize.Small,
			    ButtonStyle = ExpandButtonStyle.Circle,
			    CheckBoxShown = true,
			    ExpandedHeight = 0,
			    HeaderBackColor = Color.Azure,
			    HeaderBorderBrush = SystemColors.Control,
			    HeaderLineColor = Color.Azure,
			    IsChecked = false,
			    IsExpanded = false,
			    Size = new Size(FlowPanelForExpanders.Size.Width - 6, 255),
			    Text = readerForm.CurrentSettings.Name,
			    UseAnimation = true
		    };
		    schemeExpander.Controls.Add(splitContainerInner);

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