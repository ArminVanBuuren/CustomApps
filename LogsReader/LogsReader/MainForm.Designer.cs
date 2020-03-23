using FastColoredTextBoxNS;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using LogsReader.Properties;

namespace LogsReader
{
    sealed partial class MainForm
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
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Servers");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Types");
            this.dgvFiles = new System.Windows.Forms.DataGridView();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Server = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Trace = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PrivateID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsMatched = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.txtPattern = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.maxLinesStackText = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.logDirText = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.maxThreadsText = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.fileNames = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chooseScheme = new System.Windows.Forms.ComboBox();
            this.serversText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.trvMain = new LogsReader.MyTreeView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.descriptionText = new System.Windows.Forms.RichTextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusTextLable = new System.Windows.Forms.ToolStripStatusLabel();
            this.useRegex = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.traceLikeText = new System.Windows.Forms.TextBox();
            this.dateTimePickerEnd = new System.Windows.Forms.DateTimePicker();
            this.label10 = new System.Windows.Forms.Label();
            this.dateTimePickerStart = new System.Windows.Forms.DateTimePicker();
            this.traceNotLikeText = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonFilter = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.msgFilterText = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvFiles
            // 
            this.dgvFiles.AllowUserToAddRows = false;
            this.dgvFiles.AllowUserToDeleteRows = false;
            this.dgvFiles.AllowUserToOrderColumns = true;
            this.dgvFiles.AllowUserToResizeRows = false;
            this.dgvFiles.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.Server,
            this.Date,
            this.FileName,
            this.Trace,
            this.PrivateID,
            this.IsMatched});
            this.dgvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvFiles.GridColor = System.Drawing.SystemColors.ControlLight;
            this.dgvFiles.Location = new System.Drawing.Point(0, 0);
            this.dgvFiles.MultiSelect = false;
            this.dgvFiles.Name = "dgvFiles";
            this.dgvFiles.ReadOnly = true;
            this.dgvFiles.RowHeadersVisible = false;
            this.dgvFiles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvFiles.Size = new System.Drawing.Size(413, 593);
            this.dgvFiles.TabIndex = 1;
            this.dgvFiles.SelectionChanged += new System.EventHandler(this.dgvFiles_SelectionChanged);
            this.dgvFiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dgvFiles_MouseDown);
            // 
            // ID
            // 
            this.ID.DataPropertyName = "ID";
            this.ID.HeaderText = "ID";
            this.ID.MinimumWidth = 40;
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            this.ID.Width = 60;
            // 
            // Server
            // 
            this.Server.DataPropertyName = "Server";
            this.Server.HeaderText = "Server";
            this.Server.MinimumWidth = 40;
            this.Server.Name = "Server";
            this.Server.ReadOnly = true;
            this.Server.Width = 120;
            // 
            // Date
            // 
            this.Date.DataPropertyName = "Date";
            this.Date.HeaderText = "Date";
            this.Date.MinimumWidth = 40;
            this.Date.Name = "Date";
            this.Date.ReadOnly = true;
            this.Date.Width = 160;
            // 
            // FileName
            // 
            this.FileName.DataPropertyName = "FileName";
            this.FileName.HeaderText = "FileName";
            this.FileName.MinimumWidth = 40;
            this.FileName.Name = "FileName";
            this.FileName.ReadOnly = true;
            this.FileName.Width = 200;
            // 
            // Trace
            // 
            this.Trace.DataPropertyName = "Trace";
            this.Trace.HeaderText = "Trace";
            this.Trace.MinimumWidth = 40;
            this.Trace.Name = "Trace";
            this.Trace.ReadOnly = true;
            this.Trace.Width = 120;
            // 
            // PrivateID
            // 
            this.PrivateID.DataPropertyName = "PrivateID";
            this.PrivateID.HeaderText = "PrivateID";
            this.PrivateID.MinimumWidth = 10;
            this.PrivateID.Name = "PrivateID";
            this.PrivateID.ReadOnly = true;
            this.PrivateID.Visible = false;
            this.PrivateID.Width = 10;
            // 
            // IsMatched
            // 
            this.IsMatched.DataPropertyName = "IsMatched";
            this.IsMatched.HeaderText = "IsMatched";
            this.IsMatched.MinimumWidth = 10;
            this.IsMatched.Name = "IsMatched";
            this.IsMatched.ReadOnly = true;
            this.IsMatched.Visible = false;
            this.IsMatched.Width = 10;
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Image = global::LogsReader.Properties.Resources.Search;
            this.btnSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSearch.Location = new System.Drawing.Point(1244, 7);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(91, 24);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "      Search [F5]";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.ButtonStartStop_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Image = global::LogsReader.Properties.Resources.Symbol_Refresh_3;
            this.btnClear.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClear.Location = new System.Drawing.Point(1341, 7);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(87, 24);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "      Clear [F4]";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // txtPattern
            // 
            this.txtPattern.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPattern.Location = new System.Drawing.Point(12, 8);
            this.txtPattern.Name = "txtPattern";
            this.txtPattern.Size = new System.Drawing.Size(1141, 23);
            this.txtPattern.TabIndex = 4;
            this.txtPattern.TextChanged += new System.EventHandler(this.txtPattern_TextChanged);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 753);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1422, 10);
            this.progressBar.TabIndex = 6;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Location = new System.Drawing.Point(12, 100);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.maxLinesStackText);
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            this.splitContainer1.Panel1.Controls.Add(this.logDirText);
            this.splitContainer1.Panel1.Controls.Add(this.label5);
            this.splitContainer1.Panel1.Controls.Add(this.maxThreadsText);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.fileNames);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.chooseScheme);
            this.splitContainer1.Panel1.Controls.Add(this.serversText);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.trvMain);
            this.splitContainer1.Panel1MinSize = 130;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2MinSize = 300;
            this.splitContainer1.Size = new System.Drawing.Size(1422, 651);
            this.splitContainer1.SplitterDistance = 180;
            this.splitContainer1.TabIndex = 7;
            // 
            // maxLinesStackText
            // 
            this.maxLinesStackText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.maxLinesStackText.Location = new System.Drawing.Point(86, 112);
            this.maxLinesStackText.Name = "maxLinesStackText";
            this.maxLinesStackText.Size = new System.Drawing.Size(87, 23);
            this.maxLinesStackText.TabIndex = 14;
            this.maxLinesStackText.TextChanged += new System.EventHandler(this.maxLinesStackText_TextChanged);
            this.maxLinesStackText.Leave += new System.EventHandler(this.maxLinesStackText_Leave);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 115);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 15);
            this.label6.TabIndex = 13;
            this.label6.Text = "Max Lines:";
            // 
            // logDirText
            // 
            this.logDirText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logDirText.Location = new System.Drawing.Point(86, 60);
            this.logDirText.Name = "logDirText";
            this.logDirText.Size = new System.Drawing.Size(87, 23);
            this.logDirText.TabIndex = 12;
            this.logDirText.TextChanged += new System.EventHandler(this.logDirText_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 64);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 15);
            this.label5.TabIndex = 11;
            this.label5.Text = "Logs folder:";
            // 
            // maxThreadsText
            // 
            this.maxThreadsText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.maxThreadsText.Location = new System.Drawing.Point(86, 138);
            this.maxThreadsText.Name = "maxThreadsText";
            this.maxThreadsText.Size = new System.Drawing.Size(87, 23);
            this.maxThreadsText.TabIndex = 10;
            this.maxThreadsText.TextChanged += new System.EventHandler(this.maxThreadsText_TextChanged);
            this.maxThreadsText.Leave += new System.EventHandler(this.maxThreadsText_Leave);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 141);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 15);
            this.label4.TabIndex = 9;
            this.label4.Text = "Max Threads:";
            // 
            // fileNames
            // 
            this.fileNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileNames.Location = new System.Drawing.Point(86, 86);
            this.fileNames.Name = "fileNames";
            this.fileNames.Size = new System.Drawing.Size(87, 23);
            this.fileNames.TabIndex = 8;
            this.fileNames.TextChanged += new System.EventHandler(this.typesText_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "File Types:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Scheme:";
            // 
            // chooseScheme
            // 
            this.chooseScheme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chooseScheme.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.chooseScheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.chooseScheme.FormattingEnabled = true;
            this.chooseScheme.Location = new System.Drawing.Point(86, 8);
            this.chooseScheme.Name = "chooseScheme";
            this.chooseScheme.Size = new System.Drawing.Size(87, 21);
            this.chooseScheme.TabIndex = 5;
            this.chooseScheme.SelectedIndexChanged += new System.EventHandler(this.chooseScheme_SelectedIndexChanged);
            // 
            // serversText
            // 
            this.serversText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serversText.Location = new System.Drawing.Point(86, 34);
            this.serversText.Name = "serversText";
            this.serversText.Size = new System.Drawing.Size(87, 23);
            this.serversText.TabIndex = 4;
            this.serversText.TextChanged += new System.EventHandler(this.serversText_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Servers:";
            // 
            // trvMain
            // 
            this.trvMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trvMain.CheckBoxes = true;
            this.trvMain.Location = new System.Drawing.Point(1, 167);
            this.trvMain.Name = "trvMain";
            treeNode5.Name = "trvServers";
            treeNode5.Text = "Servers";
            treeNode6.Name = "trvTypes";
            treeNode6.Text = "Types";
            this.trvMain.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode5,
            treeNode6});
            this.trvMain.Size = new System.Drawing.Size(172, 482);
            this.trvMain.TabIndex = 2;
            this.trvMain.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.trvMain_AfterCheck);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            this.splitContainer2.Panel1MinSize = 100;
            this.splitContainer2.Panel2MinSize = 190;
            this.splitContainer2.Size = new System.Drawing.Size(1238, 651);
            this.splitContainer2.SplitterDistance = 417;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.dgvFiles);
            this.splitContainer3.Panel1MinSize = 300;
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.descriptionText);
            this.splitContainer3.Size = new System.Drawing.Size(413, 647);
            this.splitContainer3.SplitterDistance = 593;
            this.splitContainer3.TabIndex = 2;
            // 
            // descriptionText
            // 
            this.descriptionText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.descriptionText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.descriptionText.Location = new System.Drawing.Point(0, 0);
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.ReadOnly = true;
            this.descriptionText.Size = new System.Drawing.Size(413, 50);
            this.descriptionText.TabIndex = 0;
            this.descriptionText.Text = "";
            // 
            // statusStrip
            // 
            this.statusStrip.GripMargin = new System.Windows.Forms.Padding(1);
            this.statusStrip.Location = new System.Drawing.Point(0, 766);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1446, 22);
            this.statusStrip.TabIndex = 8;
            this.statusStrip.Text = "statusStrip1";
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
            this.useRegex.Location = new System.Drawing.Point(1159, 11);
            this.useRegex.Name = "useRegex";
            this.useRegex.Size = new System.Drawing.Size(79, 19);
            this.useRegex.TabIndex = 9;
            this.useRegex.Text = "Use Regex";
            this.useRegex.UseVisualStyleBackColor = true;
            this.useRegex.CheckedChanged += new System.EventHandler(this.useRegex_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(236, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 15);
            this.label7.TabIndex = 24;
            this.label7.Text = "Trace Like:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 47);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(57, 15);
            this.label9.TabIndex = 23;
            this.label9.Text = "Date End:";
            // 
            // traceLikeText
            // 
            this.traceLikeText.Location = new System.Drawing.Point(326, 15);
            this.traceLikeText.Name = "traceLikeText";
            this.traceLikeText.Size = new System.Drawing.Size(328, 23);
            this.traceLikeText.TabIndex = 25;
            this.traceLikeText.TextChanged += new System.EventHandler(this.traceLikeText_TextChanged);
            // 
            // dateTimePickerEnd
            // 
            this.dateTimePickerEnd.Checked = false;
            this.dateTimePickerEnd.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            this.dateTimePickerEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerEnd.Location = new System.Drawing.Point(74, 44);
            this.dateTimePickerEnd.Name = "dateTimePickerEnd";
            this.dateTimePickerEnd.ShowCheckBox = true;
            this.dateTimePickerEnd.ShowUpDown = true;
            this.dateTimePickerEnd.Size = new System.Drawing.Size(156, 23);
            this.dateTimePickerEnd.TabIndex = 21;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(236, 47);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(85, 15);
            this.label10.TabIndex = 26;
            this.label10.Text = "Trace Not Like:";
            // 
            // dateTimePickerStart
            // 
            this.dateTimePickerStart.Checked = false;
            this.dateTimePickerStart.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            this.dateTimePickerStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerStart.Location = new System.Drawing.Point(74, 15);
            this.dateTimePickerStart.Name = "dateTimePickerStart";
            this.dateTimePickerStart.ShowCheckBox = true;
            this.dateTimePickerStart.ShowUpDown = true;
            this.dateTimePickerStart.Size = new System.Drawing.Size(156, 23);
            this.dateTimePickerStart.TabIndex = 20;
            // 
            // traceNotLikeText
            // 
            this.traceNotLikeText.Location = new System.Drawing.Point(326, 43);
            this.traceNotLikeText.Name = "traceNotLikeText";
            this.traceNotLikeText.Size = new System.Drawing.Size(328, 23);
            this.traceNotLikeText.TabIndex = 27;
            this.traceNotLikeText.TextChanged += new System.EventHandler(this.traceNotLikeText_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 18);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 15);
            this.label8.TabIndex = 22;
            this.label8.Text = "Date Start:";
            // 
            // buttonFilter
            // 
            this.buttonFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFilter.Image = global::LogsReader.Properties.Resources.filter;
            this.buttonFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonFilter.Location = new System.Drawing.Point(1232, 44);
            this.buttonFilter.Name = "buttonFilter";
            this.buttonFilter.Size = new System.Drawing.Size(91, 24);
            this.buttonFilter.TabIndex = 28;
            this.buttonFilter.Text = "      Filter [F7]";
            this.buttonFilter.UseVisualStyleBackColor = true;
            this.buttonFilter.Click += new System.EventHandler(this.buttonFilter_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReset.Image = global::LogsReader.Properties.Resources.Symbol_Refresh;
            this.buttonReset.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonReset.Location = new System.Drawing.Point(1329, 44);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(87, 24);
            this.buttonReset.TabIndex = 29;
            this.buttonReset.Text = "      Reset [F6]";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(660, 18);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(56, 15);
            this.label11.TabIndex = 30;
            this.label11.Text = "Message:";
            // 
            // msgFilterText
            // 
            this.msgFilterText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.msgFilterText.Location = new System.Drawing.Point(722, 15);
            this.msgFilterText.Name = "msgFilterText";
            this.msgFilterText.Size = new System.Drawing.Size(694, 23);
            this.msgFilterText.TabIndex = 31;
            this.msgFilterText.TextChanged += new System.EventHandler(this.msgFilterText_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.msgFilterText);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.buttonReset);
            this.groupBox1.Controls.Add(this.buttonFilter);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.traceNotLikeText);
            this.groupBox1.Controls.Add(this.dateTimePickerStart);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.dateTimePickerEnd);
            this.groupBox1.Controls.Add(this.traceLikeText);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Location = new System.Drawing.Point(12, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1422, 73);
            this.groupBox1.TabIndex = 28;
            this.groupBox1.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1446, 788);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.txtPattern);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.useRegex);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.Icon = global::LogsReader.Properties.Resources.icon;
            this.MinimumSize = new System.Drawing.Size(975, 39);
            this.Name = "MainForm";
            this.Text = "Logs Reader";
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView dgvFiles;
        private MyTreeView trvMain;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TextBox txtPattern;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusTextLable;
        private System.Windows.Forms.CheckBox useRegex;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox serversText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox chooseScheme;
        private System.Windows.Forms.TextBox fileNames;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox maxThreadsText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox logDirText;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox maxLinesStackText;
        private System.Windows.Forms.Label label6;
        private Label label7;
        private Label label9;
        private TextBox traceLikeText;
        private DateTimePicker dateTimePickerEnd;
        private Label label10;
        private DateTimePicker dateTimePickerStart;
        private TextBox traceNotLikeText;
        private Label label8;
        private Button buttonFilter;
        private Button buttonReset;
        private Label label11;
        private TextBox msgFilterText;
        private GroupBox groupBox1;
        private DataGridViewTextBoxColumn ID;
        private DataGridViewTextBoxColumn Server;
        private DataGridViewTextBoxColumn Date;
        private DataGridViewTextBoxColumn FileName;
        private DataGridViewTextBoxColumn Trace;
        private DataGridViewTextBoxColumn PrivateID;
        private DataGridViewTextBoxColumn IsMatched;
        private SplitContainer splitContainer3;
        private RichTextBox descriptionText;
    }
}

