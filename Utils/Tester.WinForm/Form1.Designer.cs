namespace Tester.WinForm
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.butStop = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.advancedFlowLayoutPanel1 = new Utils.WinForm.Expander.AdvancedFlowLayoutPanel();
			this.expandCollapsePanel1 = new Utils.WinForm.Expander.ExpandCollapsePanel();
			this.TestExpander = new Utils.WinForm.Expander.ExpandCollapsePanel();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.fastColoredTextBox1 = new FastColoredTextBoxNS.FastColoredTextBox();
			this.customTreeView1 = new Utils.WinForm.CustomTreeView();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.advancedFlowLayoutPanel1.SuspendLayout();
			this.expandCollapsePanel1.SuspendLayout();
			this.TestExpander.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.Location = new System.Drawing.Point(12, 671);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(1101, 92);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// butStop
			// 
			this.butStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butStop.Location = new System.Drawing.Point(1037, 769);
			this.butStop.Name = "butStop";
			this.butStop.Size = new System.Drawing.Size(75, 23);
			this.butStop.TabIndex = 1;
			this.butStop.Text = "Stop";
			this.butStop.UseVisualStyleBackColor = true;
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Location = new System.Drawing.Point(794, 768);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "Preview";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.Location = new System.Drawing.Point(875, 768);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 3;
			this.button2.Text = "Record";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// button3
			// 
			this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button3.Location = new System.Drawing.Point(956, 768);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 4;
			this.button3.Text = "Take Pic";
			this.button3.UseVisualStyleBackColor = true;
			// 
			// advancedFlowLayoutPanel1
			// 
			this.advancedFlowLayoutPanel1.Controls.Add(this.expandCollapsePanel1);
			this.advancedFlowLayoutPanel1.Controls.Add(this.TestExpander);
			this.advancedFlowLayoutPanel1.Location = new System.Drawing.Point(257, 54);
			this.advancedFlowLayoutPanel1.Name = "advancedFlowLayoutPanel1";
			this.advancedFlowLayoutPanel1.Size = new System.Drawing.Size(397, 536);
			this.advancedFlowLayoutPanel1.TabIndex = 6;
			// 
			// expandCollapsePanel1
			// 
			this.expandCollapsePanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.expandCollapsePanel1.BackColor = System.Drawing.Color.OrangeRed;
			this.expandCollapsePanel1.BordersThickness = 3;
			this.expandCollapsePanel1.ButtonSize = Utils.WinForm.Expander.ExpandButtonSize.Small;
			this.expandCollapsePanel1.ButtonStyle = Utils.WinForm.Expander.ExpandButtonStyle.Circle;
			this.expandCollapsePanel1.CheckBoxShown = true;
			this.expandCollapsePanel1.Controls.Add(this.customTreeView1);
			this.expandCollapsePanel1.Controls.Add(this.fastColoredTextBox1);
			this.expandCollapsePanel1.ExpandedHeight = 319;
			this.expandCollapsePanel1.HeaderBackColor = System.Drawing.Color.Azure;
			this.expandCollapsePanel1.HeaderBorderBrush = System.Drawing.Color.Chartreuse;
			this.expandCollapsePanel1.HeaderLineColor = System.Drawing.Color.Black;
			this.expandCollapsePanel1.IsChecked = false;
			this.expandCollapsePanel1.IsExpanded = true;
			this.expandCollapsePanel1.Location = new System.Drawing.Point(3, 3);
			this.expandCollapsePanel1.Name = "expandCollapsePanel1";
			this.expandCollapsePanel1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
			this.expandCollapsePanel1.Size = new System.Drawing.Size(391, 319);
			this.expandCollapsePanel1.TabIndex = 6;
			this.expandCollapsePanel1.Text = "TestTestTestTestTestTestTestTestTestTestTestTest";
			this.expandCollapsePanel1.UseAnimation = true;
			// 
			// TestExpander
			// 
			this.TestExpander.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TestExpander.BackColor = System.Drawing.Color.OrangeRed;
			this.TestExpander.BordersThickness = 3;
			this.TestExpander.ButtonSize = Utils.WinForm.Expander.ExpandButtonSize.Small;
			this.TestExpander.ButtonStyle = Utils.WinForm.Expander.ExpandButtonStyle.Circle;
			this.TestExpander.CheckBoxShown = true;
			this.TestExpander.Controls.Add(this.richTextBox1);
			this.TestExpander.ExpandedHeight = 319;
			this.TestExpander.HeaderBackColor = System.Drawing.Color.Azure;
			this.TestExpander.HeaderBorderBrush = System.Drawing.Color.Chartreuse;
			this.TestExpander.HeaderLineColor = System.Drawing.Color.Silver;
			this.TestExpander.IsChecked = false;
			this.TestExpander.IsExpanded = true;
			this.TestExpander.Location = new System.Drawing.Point(3, 328);
			this.TestExpander.Name = "TestExpander";
			this.TestExpander.Size = new System.Drawing.Size(391, 207);
			this.TestExpander.TabIndex = 5;
			this.TestExpander.Text = "TestTestTestTestTestTestTestTestTestTestTestTest";
			this.TestExpander.UseAnimation = true;
			// 
			// richTextBox1
			// 
			this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.richTextBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.richTextBox1.Location = new System.Drawing.Point(0, 27);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(330, 180);
			this.richTextBox1.TabIndex = 1;
			this.richTextBox1.Text = "";
			// 
			// fastColoredTextBox1
			// 
			this.fastColoredTextBox1.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
			this.fastColoredTextBox1.AutoScrollMinSize = new System.Drawing.Size(179, 14);
			this.fastColoredTextBox1.BackBrush = null;
			this.fastColoredTextBox1.CharHeight = 14;
			this.fastColoredTextBox1.CharWidth = 8;
			this.fastColoredTextBox1.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.fastColoredTextBox1.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
			this.fastColoredTextBox1.Font = new System.Drawing.Font("Courier New", 9.75F);
			this.fastColoredTextBox1.IsReplaceMode = false;
			this.fastColoredTextBox1.Location = new System.Drawing.Point(0, 36);
			this.fastColoredTextBox1.Name = "fastColoredTextBox1";
			this.fastColoredTextBox1.Paddings = new System.Windows.Forms.Padding(0);
			this.fastColoredTextBox1.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
			this.fastColoredTextBox1.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("fastColoredTextBox1.ServiceColors")));
			this.fastColoredTextBox1.Size = new System.Drawing.Size(295, 150);
			this.fastColoredTextBox1.TabIndex = 1;
			this.fastColoredTextBox1.TabLength = 2;
			this.fastColoredTextBox1.Text = "fastColoredTextBox1";
			this.fastColoredTextBox1.Zoom = 100;
			// 
			// customTreeView1
			// 
			this.customTreeView1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
			this.customTreeView1.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
			this.customTreeView1.Location = new System.Drawing.Point(68, 219);
			this.customTreeView1.Name = "customTreeView1";
			this.customTreeView1.Size = new System.Drawing.Size(276, 97);
			this.customTreeView1.TabIndex = 2;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.ClientSize = new System.Drawing.Size(1125, 804);
			this.Controls.Add(this.advancedFlowLayoutPanel1);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.butStop);
			this.Controls.Add(this.pictureBox1);
			this.Name = "Form1";
			this.Text = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.advancedFlowLayoutPanel1.ResumeLayout(false);
			this.expandCollapsePanel1.ResumeLayout(false);
			this.TestExpander.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBox1)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button butStop;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
		private Utils.WinForm.Expander.ExpandCollapsePanel TestExpander;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private Utils.WinForm.Expander.AdvancedFlowLayoutPanel advancedFlowLayoutPanel1;
		private Utils.WinForm.Expander.ExpandCollapsePanel expandCollapsePanel1;
		private FastColoredTextBoxNS.FastColoredTextBox fastColoredTextBox1;
		private Utils.WinForm.CustomTreeView customTreeView1;
	}
}

