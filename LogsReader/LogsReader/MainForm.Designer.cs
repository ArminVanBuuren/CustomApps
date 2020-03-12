using FastColoredTextBoxNS;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LogsReader
{
    partial class MainForm
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
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Servers");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Types");
            this.FCTB = new FastColoredTextBox();
            this.dgvFiles = new System.Windows.Forms.DataGridView();
            this.trvMain = new System.Windows.Forms.TreeView();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.txtPattern = new System.Windows.Forms.TextBox();
            this.pgbThreads = new System.Windows.Forms.ProgressBar();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tracePatternText = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.maxLinesStackText = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.logDirText = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.maxThreadsText = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.typesText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chooseScheme = new System.Windows.Forms.ComboBox();
            this.serversText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusTextLable = new System.Windows.Forms.ToolStripStatusLabel();
            this.useRegex = new System.Windows.Forms.CheckBox();
            this.toolStripStatusLabel0 = new System.Windows.Forms.ToolStripStatusLabel();
            this.completedFilesStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.totalFilesStatus = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // FCTB
            // 
            ((ISupportInitialize)(FCTB)).BeginInit();
            this.FCTB.ClearStylesBuffer();
            this.FCTB.Range.ClearStyle(StyleIndex.All);
            this.FCTB.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                                      | System.Windows.Forms.AnchorStyles.Left)
                                                                     | System.Windows.Forms.AnchorStyles.Right)));
            this.FCTB.Location = new System.Drawing.Point(0, -1);
            this.FCTB.Multiline = true;
            this.FCTB.Size = new System.Drawing.Size(875, 339);
            this.FCTB.TabIndex = 0;
            this.FCTB.AutoScrollMinSize = new Size(0, 14);
            this.FCTB.BackBrush = null;
            this.FCTB.CharHeight = 14;
            this.FCTB.CharWidth = 8;
            this.FCTB.Cursor = Cursors.IBeam;
            this.FCTB.DisabledColor = Color.FromArgb(100, 180, 180, 180);
            this.FCTB.ImeMode = ImeMode.Off;
            this.FCTB.IsReplaceMode = false;
            this.FCTB.Name = "fctb";
            this.FCTB.Paddings = new Padding(0);
            this.FCTB.SelectionColor = Color.FromArgb(60, 0, 0, 255);
            this.FCTB.TabIndex = 0;
            this.FCTB.WordWrap = true;
            this.FCTB.Zoom = 100;
            this.FCTB.Dock = DockStyle.Fill;
            this.FCTB.ForeColor = Color.Black;
            this.FCTB.ReadOnly = true;
            this.FCTB.Language = Language.XML;
            ((ISupportInitialize)(FCTB)).EndInit();
            // 
            // dgvFiles
            // 
            this.dgvFiles.AllowUserToAddRows = false;
            this.dgvFiles.AllowUserToDeleteRows = false;
            this.dgvFiles.AllowUserToOrderColumns = true;
            this.dgvFiles.AllowUserToResizeRows = false;
            this.dgvFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFiles.Location = new System.Drawing.Point(0, 0);
            this.dgvFiles.MultiSelect = false;
            this.dgvFiles.Name = "dgvFiles";
            this.dgvFiles.ReadOnly = true;
            this.dgvFiles.RowHeadersVisible = false;
            this.dgvFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvFiles.Size = new System.Drawing.Size(872, 174);
            this.dgvFiles.TabIndex = 1;
            this.dgvFiles.SelectionChanged += new System.EventHandler(this.dgvFiles_SelectionChanged);
            // 
            // trvMain
            // 
            this.trvMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trvMain.CheckBoxes = true;
            this.trvMain.Location = new System.Drawing.Point(0, 199);
            this.trvMain.Name = "trvMain";
            treeNode3.Name = "trvServers";
            treeNode3.Text = "Servers";
            treeNode4.Name = "trvTypes";
            treeNode4.Text = "Types";
            this.trvMain.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode3,
            treeNode4});
            this.trvMain.Size = new System.Drawing.Size(228, 318);
            this.trvMain.TabIndex = 2;
            this.trvMain.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.trvMain_AfterCheck);
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Location = new System.Drawing.Point(879, 6);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.ButtonStartStop_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(960, 5);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // txtPattern
            // 
            this.txtPattern.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPattern.Location = new System.Drawing.Point(12, 8);
            this.txtPattern.Name = "txtPattern";
            this.txtPattern.Size = new System.Drawing.Size(861, 20);
            this.txtPattern.TabIndex = 4;
            this.txtPattern.TextChanged += new System.EventHandler(this.txtPattern_TextChanged);
            // 
            // pgbThreads
            // 
            this.pgbThreads.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pgbThreads.Location = new System.Drawing.Point(12, 562);
            this.pgbThreads.Name = "pgbThreads";
            this.pgbThreads.Size = new System.Drawing.Size(1111, 10);
            this.pgbThreads.TabIndex = 6;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Location = new System.Drawing.Point(12, 35);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tracePatternText);
            this.splitContainer1.Panel1.Controls.Add(this.label7);
            this.splitContainer1.Panel1.Controls.Add(this.maxLinesStackText);
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            this.splitContainer1.Panel1.Controls.Add(this.logDirText);
            this.splitContainer1.Panel1.Controls.Add(this.label5);
            this.splitContainer1.Panel1.Controls.Add(this.maxThreadsText);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.typesText);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.chooseScheme);
            this.splitContainer1.Panel1.Controls.Add(this.serversText);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.trvMain);
            this.splitContainer1.Panel1MinSize = 100;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1113, 521);
            this.splitContainer1.SplitterDistance = 230;
            this.splitContainer1.TabIndex = 7;
            // 
            // tracePatternText
            // 
            this.tracePatternText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tracePatternText.Location = new System.Drawing.Point(61, 173);
            this.tracePatternText.Name = "tracePatternText";
            this.tracePatternText.Size = new System.Drawing.Size(162, 20);
            this.tracePatternText.TabIndex = 16;
            this.tracePatternText.TextChanged += new System.EventHandler(this.tracePatternText_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 176);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Pattern:";
            // 
            // maxLinesStackText
            // 
            this.maxLinesStackText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.maxLinesStackText.Location = new System.Drawing.Point(61, 144);
            this.maxLinesStackText.Name = "maxLinesStackText";
            this.maxLinesStackText.Size = new System.Drawing.Size(162, 20);
            this.maxLinesStackText.TabIndex = 14;
            this.maxLinesStackText.TextChanged += new System.EventHandler(this.maxLinesStackText_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 147);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Lines:";
            // 
            // logDirText
            // 
            this.logDirText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logDirText.Location = new System.Drawing.Point(61, 118);
            this.logDirText.Name = "logDirText";
            this.logDirText.Size = new System.Drawing.Size(162, 20);
            this.logDirText.TabIndex = 12;
            this.logDirText.TextChanged += new System.EventHandler(this.logDirText_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 121);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Log dir:";
            // 
            // maxThreadsText
            // 
            this.maxThreadsText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.maxThreadsText.Location = new System.Drawing.Point(61, 92);
            this.maxThreadsText.Name = "maxThreadsText";
            this.maxThreadsText.Size = new System.Drawing.Size(162, 20);
            this.maxThreadsText.TabIndex = 10;
            this.maxThreadsText.TextChanged += new System.EventHandler(this.maxThreadsText_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "MaxThrds:";
            // 
            // typesText
            // 
            this.typesText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.typesText.Location = new System.Drawing.Point(61, 66);
            this.typesText.Name = "typesText";
            this.typesText.Size = new System.Drawing.Size(162, 20);
            this.typesText.TabIndex = 8;
            this.typesText.TextChanged += new System.EventHandler(this.typesText_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Types:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Scheme:";
            // 
            // chooseScheme
            // 
            this.chooseScheme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chooseScheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.chooseScheme.FormattingEnabled = true;
            this.chooseScheme.Location = new System.Drawing.Point(61, 13);
            this.chooseScheme.Name = "chooseScheme";
            this.chooseScheme.Size = new System.Drawing.Size(162, 21);
            this.chooseScheme.TabIndex = 5;
            this.chooseScheme.SelectedIndexChanged += new System.EventHandler(this.chooseScheme_SelectedIndexChanged);
            // 
            // serversText
            // 
            this.serversText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serversText.Location = new System.Drawing.Point(61, 40);
            this.serversText.Name = "serversText";
            this.serversText.Size = new System.Drawing.Size(162, 20);
            this.serversText.TabIndex = 4;
            this.serversText.TextChanged += new System.EventHandler(this.serversText_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Servers:";
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dgvFiles);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.FCTB);
            this.splitContainer2.Size = new System.Drawing.Size(879, 521);
            this.splitContainer2.SplitterDistance = 177;
            this.splitContainer2.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel0,
            this.completedFilesStatus,
            this.toolStripStatusLabel1,
            this.totalFilesStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 575);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1137, 22);
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // StatusTextLable
            // 
            this.StatusTextLable.Name = "StatusTextLable";
            this.StatusTextLable.Size = new System.Drawing.Size(0, 17);
            // 
            // useRegex
            // 
            this.useRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.useRegex.AutoSize = true;
            this.useRegex.Location = new System.Drawing.Point(1041, 10);
            this.useRegex.Name = "useRegex";
            this.useRegex.Size = new System.Drawing.Size(79, 17);
            this.useRegex.TabIndex = 9;
            this.useRegex.Text = "Use Regex";
            this.useRegex.UseVisualStyleBackColor = true;
            // 
            // toolStripStatusLabel0
            // 
            this.toolStripStatusLabel0.Name = "toolStripStatusLabel0";
            this.toolStripStatusLabel0.Size = new System.Drawing.Size(90, 17);
            this.toolStripStatusLabel0.Text = "Completed files";
            // 
            // completedFilesStatus
            // 
            this.completedFilesStatus.Name = "completedFilesStatus";
            this.completedFilesStatus.Size = new System.Drawing.Size(13, 17);
            this.completedFilesStatus.Text = "0";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(18, 17);
            this.toolStripStatusLabel1.Text = "of";
            // 
            // totalFilesStatus
            // 
            this.totalFilesStatus.Name = "totalFilesStatus";
            this.totalFilesStatus.Size = new System.Drawing.Size(13, 17);
            this.totalFilesStatus.Text = "0";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1137, 597);
            this.Controls.Add(this.useRegex);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.pgbThreads);
            this.Controls.Add(this.txtPattern);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnSearch);
            this.MinimumSize = new System.Drawing.Size(398, 283);
            this.Name = "MainForm";
            this.Text = "MG Logs Reader";
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FastColoredTextBox FCTB;
        private System.Windows.Forms.DataGridView dgvFiles;
        private System.Windows.Forms.TreeView trvMain;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TextBox txtPattern;
        private System.Windows.Forms.ProgressBar pgbThreads;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusTextLable;
        private System.Windows.Forms.CheckBox useRegex;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox serversText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox chooseScheme;
        private System.Windows.Forms.TextBox typesText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox maxThreadsText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox logDirText;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox maxLinesStackText;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tracePatternText;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel0;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel totalFilesStatus;
        private System.Windows.Forms.ToolStripStatusLabel completedFilesStatus;
    }
}

