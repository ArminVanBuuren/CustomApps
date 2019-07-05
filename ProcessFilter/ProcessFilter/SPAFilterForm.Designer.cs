using System.Drawing;

namespace SPAFilter
{
    partial class SPAFilterForm
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
            this.ProcessesTextBox = new System.Windows.Forms.TextBox();
            this.OperationTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.OperationButtonOpen = new System.Windows.Forms.Button();
            this.ProcessesButtonOpen = new System.Windows.Forms.Button();
            this.dataGridProcesses = new System.Windows.Forms.DataGridView();
            this.OperationComboBox = new System.Windows.Forms.ComboBox();
            this.NetSettComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.ProcessesComboBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.FilterButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridOperations = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dataGridScenarios = new System.Windows.Forms.DataGridView();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.dataGridCommands = new System.Windows.Forms.DataGridView();
            this.GenerateSC = new System.Windows.Forms.TabPage();
            this.RootSCExportPathButton = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.ExportSCPath = new System.Windows.Forms.TextBox();
            this.OpenSevExelButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.ButtonGenerateSC = new System.Windows.Forms.Button();
            this.OpenSCXlsx = new System.Windows.Forms.TextBox();
            this.progressBar = new SPAFilter.CustomProgressBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.PrintXMLButton = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.BPCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.NEElementsCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.OperationsCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.ScenariosCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.CommandsCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.ActivatorList = new System.Windows.Forms.ListBox();
            this.label10 = new System.Windows.Forms.Label();
            this.AddActivatorButton = new System.Windows.Forms.Button();
            this.RemoveActivatorButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.ServiceCatalogTextBox = new System.Windows.Forms.TextBox();
            this.ServiceCatalogOpenButton = new System.Windows.Forms.Button();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.dataGridSCOperations = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize) (this.dataGridProcesses)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.dataGridOperations)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.dataGridScenarios)).BeginInit();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.dataGridCommands)).BeginInit();
            this.GenerateSC.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.dataGridSCOperations)).BeginInit();
            this.SuspendLayout();
            // 
            // ProcessesTextBox
            // 
            this.ProcessesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                                                  | System.Windows.Forms.AnchorStyles.Right)));
            this.ProcessesTextBox.Location = new System.Drawing.Point(77, 12);
            this.ProcessesTextBox.Name = "ProcessesTextBox";
            this.ProcessesTextBox.Size = new System.Drawing.Size(639, 20);
            this.ProcessesTextBox.TabIndex = 1;
            this.ProcessesTextBox.TextChanged += new System.EventHandler(this.ProcessesTextBox_TextChanged);
            // 
            // OperationTextBox
            // 
            this.OperationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                                                  | System.Windows.Forms.AnchorStyles.Right)));
            this.OperationTextBox.Location = new System.Drawing.Point(77, 38);
            this.OperationTextBox.Name = "OperationTextBox";
            this.OperationTextBox.Size = new System.Drawing.Size(639, 20);
            this.OperationTextBox.TabIndex = 2;
            this.OperationTextBox.TextChanged += new System.EventHandler(this.OperationTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Processes:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Operations:";
            // 
            // OperationButtonOpen
            // 
            this.OperationButtonOpen.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OperationButtonOpen.Location = new System.Drawing.Point(722, 36);
            this.OperationButtonOpen.Name = "OperationButtonOpen";
            this.OperationButtonOpen.Size = new System.Drawing.Size(75, 23);
            this.OperationButtonOpen.TabIndex = 6;
            this.OperationButtonOpen.Text = "Open";
            this.OperationButtonOpen.UseVisualStyleBackColor = true;
            this.OperationButtonOpen.Click += new System.EventHandler(this.OperationButtonOpen_Click);
            // 
            // ProcessesButtonOpen
            // 
            this.ProcessesButtonOpen.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ProcessesButtonOpen.Location = new System.Drawing.Point(722, 10);
            this.ProcessesButtonOpen.Name = "ProcessesButtonOpen";
            this.ProcessesButtonOpen.Size = new System.Drawing.Size(75, 23);
            this.ProcessesButtonOpen.TabIndex = 5;
            this.ProcessesButtonOpen.Text = "Open";
            this.ProcessesButtonOpen.UseVisualStyleBackColor = true;
            this.ProcessesButtonOpen.Click += new System.EventHandler(this.ProcessesButtonOpen_Click);
            // 
            // dataGridProcesses
            // 
            this.dataGridProcesses.AllowUserToAddRows = false;
            this.dataGridProcesses.AllowUserToDeleteRows = false;
            this.dataGridProcesses.AllowUserToOrderColumns = true;
            this.dataGridProcesses.AllowUserToResizeRows = false;
            this.dataGridProcesses.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                                                    | System.Windows.Forms.AnchorStyles.Left)
                                                                                   | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridProcesses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridProcesses.Location = new System.Drawing.Point(0, 0);
            this.dataGridProcesses.Name = "dataGridProcesses";
            this.dataGridProcesses.ReadOnly = true;
            this.dataGridProcesses.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridProcesses.Size = new System.Drawing.Size(794, 310);
            this.dataGridProcesses.TabIndex = 4;
            this.dataGridProcesses.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridProcessesResults_CellMouseDoubleClick);
            // 
            // OperationComboBox
            // 
            this.OperationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                                                   | System.Windows.Forms.AnchorStyles.Right)));
            this.OperationComboBox.FormattingEnabled = true;
            this.OperationComboBox.Location = new System.Drawing.Point(456, 19);
            this.OperationComboBox.Name = "OperationComboBox";
            this.OperationComboBox.Size = new System.Drawing.Size(320, 21);
            this.OperationComboBox.TabIndex = 10;
            // 
            // NetSettComboBox
            // 
            this.NetSettComboBox.FormattingEnabled = true;
            this.NetSettComboBox.Location = new System.Drawing.Point(102, 49);
            this.NetSettComboBox.Name = "NetSettComboBox";
            this.NetSettComboBox.Size = new System.Drawing.Size(280, 21);
            this.NetSettComboBox.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Network Element:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(394, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Operation:";
            // 
            // ProcessesComboBox
            // 
            this.ProcessesComboBox.FormattingEnabled = true;
            this.ProcessesComboBox.Location = new System.Drawing.Point(62, 19);
            this.ProcessesComboBox.Name = "ProcessesComboBox";
            this.ProcessesComboBox.Size = new System.Drawing.Size(320, 21);
            this.ProcessesComboBox.TabIndex = 12;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Process:";
            // 
            // buttonFilter
            // 
            this.FilterButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.FilterButton.Location = new System.Drawing.Point(701, 58);
            this.FilterButton.Name = "buttonFilter";
            this.FilterButton.Size = new System.Drawing.Size(75, 23);
            this.FilterButton.TabIndex = 13;
            this.FilterButton.Text = "Get [F5]";
            this.FilterButton.UseVisualStyleBackColor = true;
            this.FilterButton.Click += new System.EventHandler(this.FilterButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                                              | System.Windows.Forms.AnchorStyles.Left)
                                                                             | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.GenerateSC);
            this.tabControl1.Location = new System.Drawing.Point(3, 312);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(802, 336);
            this.tabControl1.TabIndex = 14;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.DarkGray;
            this.tabPage1.Controls.Add(this.dataGridProcesses);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(794, 310);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Processes";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.DarkGray;
            this.tabPage2.Controls.Add(this.dataGridOperations);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(794, 310);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Operations";
            // 
            // dataGridOperations
            // 
            this.dataGridOperations.AllowUserToAddRows = false;
            this.dataGridOperations.AllowUserToDeleteRows = false;
            this.dataGridOperations.AllowUserToOrderColumns = true;
            this.dataGridOperations.AllowUserToResizeRows = false;
            this.dataGridOperations.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                                                     | System.Windows.Forms.AnchorStyles.Left)
                                                                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridOperations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridOperations.Location = new System.Drawing.Point(0, 0);
            this.dataGridOperations.Name = "dataGridOperations";
            this.dataGridOperations.ReadOnly = true;
            this.dataGridOperations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridOperations.Size = new System.Drawing.Size(794, 310);
            this.dataGridOperations.TabIndex = 0;
            this.dataGridOperations.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridOperationsResult_CellDoubleClick);
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.Color.DarkGray;
            this.tabPage3.Controls.Add(this.dataGridScenarios);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(794, 310);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Scenarios";
            // 
            // dataGridScenarios
            // 
            this.dataGridScenarios.AllowUserToAddRows = false;
            this.dataGridScenarios.AllowUserToDeleteRows = false;
            this.dataGridScenarios.AllowUserToOrderColumns = true;
            this.dataGridScenarios.AllowUserToResizeRows = false;
            this.dataGridScenarios.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                                                    | System.Windows.Forms.AnchorStyles.Left)
                                                                                   | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridScenarios.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridScenarios.Location = new System.Drawing.Point(0, 0);
            this.dataGridScenarios.Name = "dataGridScenarios";
            this.dataGridScenarios.ReadOnly = true;
            this.dataGridScenarios.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridScenarios.Size = new System.Drawing.Size(794, 310);
            this.dataGridScenarios.TabIndex = 5;
            this.dataGridScenarios.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridScenariosResult_CellDoubleClick);
            // 
            // tabPage4
            // 
            this.tabPage4.BackColor = System.Drawing.Color.DarkGray;
            this.tabPage4.Controls.Add(this.dataGridCommands);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(794, 310);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Commands";
            // 
            // dataGridCommands
            // 
            this.dataGridCommands.AllowUserToAddRows = false;
            this.dataGridCommands.AllowUserToDeleteRows = false;
            this.dataGridCommands.AllowUserToOrderColumns = true;
            this.dataGridCommands.AllowUserToResizeRows = false;
            this.dataGridCommands.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                                                   | System.Windows.Forms.AnchorStyles.Left)
                                                                                  | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridCommands.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridCommands.Location = new System.Drawing.Point(0, 0);
            this.dataGridCommands.Name = "dataGridCommands";
            this.dataGridCommands.ReadOnly = true;
            this.dataGridCommands.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridCommands.Size = new System.Drawing.Size(794, 310);
            this.dataGridCommands.TabIndex = 1;
            this.dataGridCommands.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridCommandsResult_CellMouseDoubleClick);
            // 
            // GenerateSC
            // 
            this.GenerateSC.BackColor = System.Drawing.Color.DarkGray;
            this.GenerateSC.Controls.Add(this.RootSCExportPathButton);
            this.GenerateSC.Controls.Add(this.label9);
            this.GenerateSC.Controls.Add(this.ExportSCPath);
            this.GenerateSC.Controls.Add(this.OpenSevExelButton);
            this.GenerateSC.Controls.Add(this.label5);
            this.GenerateSC.Controls.Add(this.ButtonGenerateSC);
            this.GenerateSC.Controls.Add(this.OpenSCXlsx);
            this.GenerateSC.Location = new System.Drawing.Point(4, 22);
            this.GenerateSC.Name = "GenerateSC";
            this.GenerateSC.Size = new System.Drawing.Size(794, 310);
            this.GenerateSC.TabIndex = 4;
            this.GenerateSC.Text = "Generate SC";
            // 
            // RootSCExportPathButton
            // 
            this.RootSCExportPathButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RootSCExportPathButton.Location = new System.Drawing.Point(711, 12);
            this.RootSCExportPathButton.Name = "RootSCExportPathButton";
            this.RootSCExportPathButton.Size = new System.Drawing.Size(80, 23);
            this.RootSCExportPathButton.TabIndex = 30;
            this.RootSCExportPathButton.Text = "Root";
            this.RootSCExportPathButton.UseVisualStyleBackColor = true;
            this.RootSCExportPathButton.Click += new System.EventHandler(this.RootSCExportPathButton_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 17);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 13);
            this.label9.TabIndex = 29;
            this.label9.Text = "Export Path:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ExportSCPath
            // 
            this.ExportSCPath.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                                              | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportSCPath.Location = new System.Drawing.Point(82, 14);
            this.ExportSCPath.Name = "ExportSCPath";
            this.ExportSCPath.Size = new System.Drawing.Size(627, 20);
            this.ExportSCPath.TabIndex = 28;
            this.ExportSCPath.TextChanged += new System.EventHandler(this.ExportSCPath_TextChanged);
            // 
            // OpenSevExelButton
            // 
            this.OpenSevExelButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenSevExelButton.Location = new System.Drawing.Point(711, 41);
            this.OpenSevExelButton.Name = "OpenSevExelButton";
            this.OpenSevExelButton.Size = new System.Drawing.Size(80, 23);
            this.OpenSevExelButton.TabIndex = 27;
            this.OpenSevExelButton.Text = "Open xlsx";
            this.OpenSevExelButton.UseVisualStyleBackColor = true;
            this.OpenSevExelButton.Click += new System.EventHandler(this.OpenRDServiceExelButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 46);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 26;
            this.label5.Text = "Services:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ButtonGenerateSC
            // 
            this.ButtonGenerateSC.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonGenerateSC.Enabled = false;
            this.ButtonGenerateSC.Location = new System.Drawing.Point(711, 68);
            this.ButtonGenerateSC.Name = "ButtonGenerateSC";
            this.ButtonGenerateSC.Size = new System.Drawing.Size(80, 23);
            this.ButtonGenerateSC.TabIndex = 26;
            this.ButtonGenerateSC.Text = "Generate SC";
            this.ButtonGenerateSC.UseVisualStyleBackColor = true;
            this.ButtonGenerateSC.Click += new System.EventHandler(this.ButtonGenerateSC_Click);
            // 
            // OpenSCXlsx
            // 
            this.OpenSCXlsx.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                                            | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenSCXlsx.Location = new System.Drawing.Point(82, 43);
            this.OpenSCXlsx.Name = "OpenSCXlsx";
            this.OpenSCXlsx.Size = new System.Drawing.Size(627, 20);
            this.OpenSCXlsx.TabIndex = 0;
            this.OpenSCXlsx.TextChanged += new System.EventHandler(this.OpenSCXlsx_TextChanged);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                                                                             | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(7, 634);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(794, 10);
            this.progressBar.TabIndex = 23;
            this.progressBar.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.PrintXMLButton);
            this.groupBox1.Controls.Add(this.OperationComboBox);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.NetSettComboBox);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.ProcessesComboBox);
            this.groupBox1.Controls.Add(this.FilterButton);
            this.groupBox1.Location = new System.Drawing.Point(15, 219);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(782, 87);
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filter Settings";
            // 
            // buttonPrintXML
            // 
            this.PrintXMLButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PrintXMLButton.Location = new System.Drawing.Point(606, 58);
            this.PrintXMLButton.Name = "buttonPrintXML";
            this.PrintXMLButton.Size = new System.Drawing.Size(89, 23);
            this.PrintXMLButton.TabIndex = 14;
            this.PrintXMLButton.Text = "Print XML";
            this.PrintXMLButton.UseVisualStyleBackColor = true;
            this.PrintXMLButton.Click += new System.EventHandler(this.PrintXMLButton_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                this.BPCount,
                this.NEElementsCount,
                this.OperationsCount,
                this.ScenariosCount,
                this.CommandsCount
            });
            this.statusStrip1.Location = new System.Drawing.Point(0, 651);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(807, 22);
            this.statusStrip1.TabIndex = 27;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // BPCount
            // 
            this.BPCount.Name = "BPCount";
            this.BPCount.Size = new System.Drawing.Size(61, 17);
            this.BPCount.Text = "Processes:";
            // 
            // NEElementsCount
            // 
            this.NEElementsCount.Name = "NEElementsCount";
            this.NEElementsCount.Size = new System.Drawing.Size(30, 17);
            this.NEElementsCount.Text = "NEs:";
            // 
            // OperationsCount
            // 
            this.OperationsCount.Name = "OperationsCount";
            this.OperationsCount.Size = new System.Drawing.Size(68, 17);
            this.OperationsCount.Text = "Operations:";
            // 
            // ScenariosCount
            // 
            this.ScenariosCount.Name = "ScenariosCount";
            this.ScenariosCount.Size = new System.Drawing.Size(60, 17);
            this.ScenariosCount.Text = "Scenarios:";
            // 
            // CommandsCount
            // 
            this.CommandsCount.Name = "CommandsCount";
            this.CommandsCount.Size = new System.Drawing.Size(72, 17);
            this.CommandsCount.Text = "Commands:";
            // 
            // ActivatorList
            // 
            this.ActivatorList.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                                               | System.Windows.Forms.AnchorStyles.Right)));
            this.ActivatorList.FormattingEnabled = true;
            this.ActivatorList.Location = new System.Drawing.Point(15, 109);
            this.ActivatorList.Name = "ActivatorList";
            this.ActivatorList.Size = new System.Drawing.Size(701, 95);
            this.ActivatorList.TabIndex = 28;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 91);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(181, 13);
            this.label10.TabIndex = 29;
            this.label10.Text = "Activators (configuration.application):";
            this.label10.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // buttonAddActivator
            // 
            this.AddActivatorButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddActivatorButton.Location = new System.Drawing.Point(720, 109);
            this.AddActivatorButton.Name = "buttonAddActivator";
            this.AddActivatorButton.Size = new System.Drawing.Size(75, 23);
            this.AddActivatorButton.TabIndex = 30;
            this.AddActivatorButton.Text = "Add";
            this.AddActivatorButton.UseVisualStyleBackColor = true;
            this.AddActivatorButton.Click += new System.EventHandler(this.AddActivatorButton_Click);
            // 
            // buttonRemoveActivator
            // 
            this.RemoveActivatorButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoveActivatorButton.Location = new System.Drawing.Point(720, 138);
            this.RemoveActivatorButton.Name = "buttonRemoveActivator";
            this.RemoveActivatorButton.Size = new System.Drawing.Size(75, 23);
            this.RemoveActivatorButton.TabIndex = 31;
            this.RemoveActivatorButton.Text = "Remove";
            this.RemoveActivatorButton.UseVisualStyleBackColor = true;
            this.RemoveActivatorButton.Click += new System.EventHandler(this.RemoveActivatorButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 32;
            this.label3.Text = "Service Catalog:";
            // 
            // textBoxServiceCatalogPath
            // 
            this.ServiceCatalogTextBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                                                       | System.Windows.Forms.AnchorStyles.Right)));
            this.ServiceCatalogTextBox.Location = new System.Drawing.Point(103, 64);
            this.ServiceCatalogTextBox.Name = "textBoxServiceCatalogPath";
            this.ServiceCatalogTextBox.Size = new System.Drawing.Size(613, 20);
            this.ServiceCatalogTextBox.TabIndex = 33;
            ServiceCatalogTextBox.LostFocus += ServiceCatalogTextBox_LostFocus;
            this.ServiceCatalogTextBox.TextChanged += new System.EventHandler(this.ServiceCatalogTextBox_TextChanged);
            // 
            // buttonOpenSC
            // 
            this.ServiceCatalogOpenButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ServiceCatalogOpenButton.Location = new System.Drawing.Point(722, 62);
            this.ServiceCatalogOpenButton.Name = "buttonOpenSC";
            this.ServiceCatalogOpenButton.Size = new System.Drawing.Size(75, 23);
            this.ServiceCatalogOpenButton.TabIndex = 34;
            this.ServiceCatalogOpenButton.Text = "Open";
            this.ServiceCatalogOpenButton.UseVisualStyleBackColor = true;
            this.ServiceCatalogOpenButton.Click += new System.EventHandler(this.ServiceCatalogOpenButton_Click);
            // 
            // tabPage5
            // 
            this.tabPage5.BackColor = System.Drawing.Color.DarkGray;
            this.tabPage5.Controls.Add(this.dataGridSCOperations);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(794, 310);
            this.tabPage5.TabIndex = 5;
            this.tabPage5.Text = "SC Operations";
            // 
            // dataGridSCOperations
            // 
            this.dataGridSCOperations.AllowUserToAddRows = false;
            this.dataGridSCOperations.AllowUserToDeleteRows = false;
            this.dataGridSCOperations.AllowUserToOrderColumns = true;
            this.dataGridSCOperations.AllowUserToResizeRows = false;
            this.dataGridSCOperations.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                                                       | System.Windows.Forms.AnchorStyles.Left)
                                                                                      | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridSCOperations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridSCOperations.Location = new System.Drawing.Point(0, 0);
            this.dataGridSCOperations.Name = "dataGridSCOperations";
            this.dataGridSCOperations.ReadOnly = true;
            this.dataGridSCOperations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridSCOperations.Size = new System.Drawing.Size(794, 310);
            this.dataGridSCOperations.TabIndex = 1;
            // 
            // SPAFilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(807, 673);
            this.Controls.Add(this.ServiceCatalogOpenButton);
            this.Controls.Add(this.ServiceCatalogTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.RemoveActivatorButton);
            this.Controls.Add(this.AddActivatorButton);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.ActivatorList);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.OperationButtonOpen);
            this.Controls.Add(this.ProcessesButtonOpen);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.OperationTextBox);
            this.Controls.Add(this.ProcessesTextBox);
            this.Icon = global::SPAFilter.Properties.Resources.icons8;
            this.MinimumSize = new System.Drawing.Size(823, 500);
            this.Name = "SPAFilterForm";
            this.Text = "SPA Filter";
            ((System.ComponentModel.ISupportInitialize) (this.dataGridProcesses)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.dataGridOperations)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.dataGridScenarios)).EndInit();
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.dataGridCommands)).EndInit();
            this.GenerateSC.ResumeLayout(false);
            this.GenerateSC.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.dataGridSCOperations)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox ProcessesTextBox;
        private System.Windows.Forms.TextBox OperationTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button OperationButtonOpen;
        private System.Windows.Forms.Button ProcessesButtonOpen;
        private System.Windows.Forms.DataGridView dataGridProcesses;
        private System.Windows.Forms.ComboBox OperationComboBox;
        private System.Windows.Forms.ComboBox NetSettComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox ProcessesComboBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button FilterButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dataGridOperations;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.DataGridView dataGridScenarios;
        private System.Windows.Forms.DataGridView dataGridCommands;
        private CustomProgressBar progressBar;
        private System.Windows.Forms.Button ButtonGenerateSC;
        private System.Windows.Forms.TabPage GenerateSC;
        private System.Windows.Forms.Button OpenSevExelButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox OpenSCXlsx;
        private System.Windows.Forms.Button RootSCExportPathButton;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox ExportSCPath;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel BPCount;
        private System.Windows.Forms.ToolStripStatusLabel OperationsCount;
        private System.Windows.Forms.ToolStripStatusLabel ScenariosCount;
        private System.Windows.Forms.ToolStripStatusLabel CommandsCount;
        private System.Windows.Forms.ToolStripStatusLabel NEElementsCount;
        private System.Windows.Forms.Button PrintXMLButton;
        private System.Windows.Forms.ListBox ActivatorList;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button AddActivatorButton;
        private System.Windows.Forms.Button RemoveActivatorButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ServiceCatalogTextBox;
        private System.Windows.Forms.Button ServiceCatalogOpenButton;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.DataGridView dataGridSCOperations;
    }
}

