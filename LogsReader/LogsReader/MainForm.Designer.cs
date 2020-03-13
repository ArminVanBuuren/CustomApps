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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Servers");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Types");
            this.FCTB = new FastColoredTextBoxNS.FastColoredTextBox();
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Message = new System.Windows.Forms.TabPage();
            this.FullTraceStack = new System.Windows.Forms.TabPage();
            this.FCTBFullsStackTrace = new FastColoredTextBoxNS.FastColoredTextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel0 = new System.Windows.Forms.ToolStripStatusLabel();
            this.completedFilesStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.totalFilesStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusTextLable = new System.Windows.Forms.ToolStripStatusLabel();
            this.useRegex = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.FCTB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.Message.SuspendLayout();
            this.FullTraceStack.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FCTBFullsStackTrace)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // FCTB
            // 
            this.FCTB.AutoCompleteBracketsList = new char[] {
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
            this.FCTB.AutoIndentCharsPatterns = "";
            this.FCTB.AutoScrollMinSize = new System.Drawing.Size(0, 14);
            this.FCTB.BackBrush = null;
            this.FCTB.CharHeight = 14;
            this.FCTB.CharWidth = 8;
            this.FCTB.CommentPrefix = null;
            this.FCTB.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.FCTB.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.FCTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FCTB.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.FCTB.ForeColor = System.Drawing.Color.Black;
            this.FCTB.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.FCTB.IsReplaceMode = false;
            this.FCTB.Language = FastColoredTextBoxNS.Language.XML;
            this.FCTB.LeftBracket = '<';
            this.FCTB.LeftBracket2 = '(';
            this.FCTB.Location = new System.Drawing.Point(3, 3);
            this.FCTB.Name = "FCTB";
            this.FCTB.Paddings = new System.Windows.Forms.Padding(0);
            this.FCTB.ReadOnly = true;
            this.FCTB.RightBracket = '>';
            this.FCTB.RightBracket2 = ')';
            this.FCTB.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.FCTB.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("FCTB.ServiceColors")));
            this.FCTB.Size = new System.Drawing.Size(861, 304);
            this.FCTB.TabIndex = 0;
            this.FCTB.TabLength = 2;
            this.FCTB.WordWrap = true;
            this.FCTB.Zoom = 100;
            // 
            // dgvFiles
            // 
            this.dgvFiles.AllowUserToAddRows = false;
            this.dgvFiles.AllowUserToDeleteRows = false;
            this.dgvFiles.AllowUserToOrderColumns = true;
            this.dgvFiles.AllowUserToResizeRows = false;
            this.dgvFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvFiles.Location = new System.Drawing.Point(0, 0);
            this.dgvFiles.MultiSelect = false;
            this.dgvFiles.Name = "dgvFiles";
            this.dgvFiles.ReadOnly = true;
            this.dgvFiles.RowHeadersVisible = false;
            this.dgvFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvFiles.Size = new System.Drawing.Size(875, 173);
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
            treeNode5.Name = "trvServers";
            treeNode5.Text = "Servers";
            treeNode6.Name = "trvTypes";
            treeNode6.Text = "Types";
            this.trvMain.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode5,
            treeNode6});
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
            this.maxLinesStackText.Leave += new System.EventHandler(this.maxLinesStackText_Leave);
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
            this.maxThreadsText.Leave += new System.EventHandler(this.maxThreadsText_Leave);
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
            this.splitContainer2.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer2.Size = new System.Drawing.Size(879, 521);
            this.splitContainer2.SplitterDistance = 177;
            this.splitContainer2.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.Message);
            this.tabControl1.Controls.Add(this.FullTraceStack);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(875, 336);
            this.tabControl1.TabIndex = 1;
            // 
            // Message
            // 
            this.Message.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Message.Controls.Add(this.FCTB);
            this.Message.Location = new System.Drawing.Point(4, 22);
            this.Message.Name = "Message";
            this.Message.Padding = new System.Windows.Forms.Padding(3);
            this.Message.Size = new System.Drawing.Size(867, 310);
            this.Message.TabIndex = 0;
            this.Message.Text = "Message";
            // 
            // FullTraceStack
            // 
            this.FullTraceStack.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.FullTraceStack.Controls.Add(this.FCTBFullsStackTrace);
            this.FullTraceStack.Location = new System.Drawing.Point(4, 22);
            this.FullTraceStack.Name = "FullTraceStack";
            this.FullTraceStack.Padding = new System.Windows.Forms.Padding(3);
            this.FullTraceStack.Size = new System.Drawing.Size(867, 310);
            this.FullTraceStack.TabIndex = 1;
            this.FullTraceStack.Text = "Full Trace";
            // 
            // FCTBFullsStackTrace
            // 
            this.FCTBFullsStackTrace.AutoCompleteBracketsList = new char[] {
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
            this.FCTBFullsStackTrace.AutoIndentCharsPatterns = "";
            this.FCTBFullsStackTrace.AutoScrollMinSize = new System.Drawing.Size(0, 14);
            this.FCTBFullsStackTrace.BackBrush = null;
            this.FCTBFullsStackTrace.CharHeight = 14;
            this.FCTBFullsStackTrace.CharWidth = 8;
            this.FCTBFullsStackTrace.CommentPrefix = null;
            this.FCTBFullsStackTrace.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.FCTBFullsStackTrace.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.FCTBFullsStackTrace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FCTBFullsStackTrace.ForeColor = System.Drawing.Color.Black;
            this.FCTBFullsStackTrace.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.FCTBFullsStackTrace.IsReplaceMode = false;
            this.FCTBFullsStackTrace.Language = FastColoredTextBoxNS.Language.XML;
            this.FCTBFullsStackTrace.LeftBracket = '<';
            this.FCTBFullsStackTrace.LeftBracket2 = '(';
            this.FCTBFullsStackTrace.Location = new System.Drawing.Point(3, 3);
            this.FCTBFullsStackTrace.Name = "FCTBFullsStackTrace";
            this.FCTBFullsStackTrace.Paddings = new System.Windows.Forms.Padding(0);
            this.FCTBFullsStackTrace.ReadOnly = true;
            this.FCTBFullsStackTrace.RightBracket = '>';
            this.FCTBFullsStackTrace.RightBracket2 = ')';
            this.FCTBFullsStackTrace.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.FCTBFullsStackTrace.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("FCTBFullsStackTrace.ServiceColors")));
            this.FCTBFullsStackTrace.Size = new System.Drawing.Size(861, 304);
            this.FCTBFullsStackTrace.TabIndex = 1;
            this.FCTBFullsStackTrace.TabLength = 2;
            this.FCTBFullsStackTrace.WordWrap = true;
            this.FCTBFullsStackTrace.Zoom = 100;
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
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(398, 283);
            this.Name = "MainForm";
            this.Text = "Logs Reader";
            ((System.ComponentModel.ISupportInitialize)(this.FCTB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.Message.ResumeLayout(false);
            this.FullTraceStack.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FCTBFullsStackTrace)).EndInit();
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
        private FastColoredTextBox FCTBFullsStackTrace;
        private TabControl tabControl1;
        private TabPage Message;
        private TabPage FullTraceStack;
    }
}

