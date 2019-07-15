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
            this.ROBPOperationTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ROBPOperationButtonOpen = new System.Windows.Forms.Button();
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
            this.Processes = new System.Windows.Forms.TabPage();
            this.Operations = new System.Windows.Forms.TabPage();
            this.dataGridOperations = new System.Windows.Forms.DataGridView();
            this.ServiceInstances = new System.Windows.Forms.TabPage();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.dataGridServiceInstances = new System.Windows.Forms.DataGridView();
            this.Scenarios = new System.Windows.Forms.TabPage();
            this.dataGridScenarios = new System.Windows.Forms.DataGridView();
            this.Commands = new System.Windows.Forms.TabPage();
            this.dataGridCommands = new System.Windows.Forms.DataGridView();
            this.GenerateSC = new System.Windows.Forms.TabPage();
            this.RootSCExportPathButton = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.ExportSCPath = new System.Windows.Forms.TextBox();
            this.OpenSevExelButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.ButtonGenerateSC = new System.Windows.Forms.Button();
            this.OpenSCXlsx = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.PrintXMLButton = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.BPCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.NEElementsCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.OperationsCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.ScenariosCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.CommandsCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.ServiceCatalogTextBox = new System.Windows.Forms.TextBox();
            this.ServiceCatalogOpenButton = new System.Windows.Forms.Button();
            this.ROBPOperationsRadioButton = new System.Windows.Forms.RadioButton();
            this.ServiceCatalogRadioButton = new System.Windows.Forms.RadioButton();
            this.progressBar = new SPAFilter.CustomProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridProcesses)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.Processes.SuspendLayout();
            this.Operations.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridOperations)).BeginInit();
            this.ServiceInstances.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridServiceInstances)).BeginInit();
            this.Scenarios.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridScenarios)).BeginInit();
            this.Commands.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCommands)).BeginInit();
            this.GenerateSC.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ProcessesTextBox
            // 
            this.ProcessesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProcessesTextBox.Location = new System.Drawing.Point(74, 12);
            this.ProcessesTextBox.Name = "ProcessesTextBox";
            this.ProcessesTextBox.Size = new System.Drawing.Size(642, 20);
            this.ProcessesTextBox.TabIndex = 1;
            this.ProcessesTextBox.TextChanged += new System.EventHandler(this.ProcessesTextBox_TextChanged);
            // 
            // ROBPOperationTextBox
            // 
            this.ROBPOperationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ROBPOperationTextBox.Location = new System.Drawing.Point(130, 38);
            this.ROBPOperationTextBox.Name = "ROBPOperationTextBox";
            this.ROBPOperationTextBox.Size = new System.Drawing.Size(586, 20);
            this.ROBPOperationTextBox.TabIndex = 2;
            this.ROBPOperationTextBox.TextChanged += new System.EventHandler(this.ROBPOperationTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Processes:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ROBPOperationButtonOpen
            // 
            this.ROBPOperationButtonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ROBPOperationButtonOpen.Location = new System.Drawing.Point(722, 36);
            this.ROBPOperationButtonOpen.Name = "ROBPOperationButtonOpen";
            this.ROBPOperationButtonOpen.Size = new System.Drawing.Size(75, 23);
            this.ROBPOperationButtonOpen.TabIndex = 6;
            this.ROBPOperationButtonOpen.Text = "Open";
            this.ROBPOperationButtonOpen.UseVisualStyleBackColor = true;
            this.ROBPOperationButtonOpen.Click += new System.EventHandler(this.ROBPOperationButtonOpen_Click);
            // 
            // ProcessesButtonOpen
            // 
            this.ProcessesButtonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
            this.dataGridProcesses.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridProcesses.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridProcesses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridProcesses.Location = new System.Drawing.Point(0, 0);
            this.dataGridProcesses.Name = "dataGridProcesses";
            this.dataGridProcesses.ReadOnly = true;
            this.dataGridProcesses.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridProcesses.Size = new System.Drawing.Size(794, 266);
            this.dataGridProcesses.TabIndex = 4;
            this.dataGridProcesses.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridProcessesResults_CellMouseDoubleClick);
            // 
            // OperationComboBox
            // 
            this.OperationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OperationComboBox.FormattingEnabled = true;
            this.OperationComboBox.Location = new System.Drawing.Point(456, 19);
            this.OperationComboBox.Name = "OperationComboBox";
            this.OperationComboBox.Size = new System.Drawing.Size(323, 21);
            this.OperationComboBox.TabIndex = 10;
            // 
            // NetSettComboBox
            // 
            this.NetSettComboBox.FormattingEnabled = true;
            this.NetSettComboBox.Location = new System.Drawing.Point(62, 49);
            this.NetSettComboBox.Name = "NetSettComboBox";
            this.NetSettComboBox.Size = new System.Drawing.Size(320, 21);
            this.NetSettComboBox.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "HostType:";
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
            // FilterButton
            // 
            this.FilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.FilterButton.Location = new System.Drawing.Point(704, 58);
            this.FilterButton.Name = "FilterButton";
            this.FilterButton.Size = new System.Drawing.Size(75, 23);
            this.FilterButton.TabIndex = 13;
            this.FilterButton.Text = "Get [F5]";
            this.FilterButton.UseVisualStyleBackColor = true;
            this.FilterButton.Click += new System.EventHandler(this.FilterButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.ServiceInstances);
            this.tabControl1.Controls.Add(this.Processes);
            this.tabControl1.Controls.Add(this.Operations);
            this.tabControl1.Controls.Add(this.Scenarios);
            this.tabControl1.Controls.Add(this.Commands);
            this.tabControl1.Controls.Add(this.GenerateSC);
            this.tabControl1.Location = new System.Drawing.Point(3, 184);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(802, 288);
            this.tabControl1.TabIndex = 14;
            // 
            // Processes
            // 
            this.Processes.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Processes.Controls.Add(this.dataGridProcesses);
            this.Processes.Location = new System.Drawing.Point(4, 22);
            this.Processes.Name = "Processes";
            this.Processes.Padding = new System.Windows.Forms.Padding(3);
            this.Processes.Size = new System.Drawing.Size(794, 262);
            this.Processes.TabIndex = 0;
            this.Processes.Text = "Processes";
            // 
            // Operations
            // 
            this.Operations.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Operations.Controls.Add(this.dataGridOperations);
            this.Operations.Location = new System.Drawing.Point(4, 22);
            this.Operations.Name = "Operations";
            this.Operations.Padding = new System.Windows.Forms.Padding(3);
            this.Operations.Size = new System.Drawing.Size(794, 262);
            this.Operations.TabIndex = 1;
            this.Operations.Text = "Operations";
            // 
            // dataGridOperations
            // 
            this.dataGridOperations.AllowUserToAddRows = false;
            this.dataGridOperations.AllowUserToDeleteRows = false;
            this.dataGridOperations.AllowUserToOrderColumns = true;
            this.dataGridOperations.AllowUserToResizeRows = false;
            this.dataGridOperations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridOperations.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridOperations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridOperations.Location = new System.Drawing.Point(0, 0);
            this.dataGridOperations.Name = "dataGridOperations";
            this.dataGridOperations.ReadOnly = true;
            this.dataGridOperations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridOperations.Size = new System.Drawing.Size(794, 266);
            this.dataGridOperations.TabIndex = 0;
            this.dataGridOperations.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridOperationsResult_CellDoubleClick);
            // 
            // ServiceInstances
            // 
            this.ServiceInstances.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.ServiceInstances.Controls.Add(this.toolStrip1);
            this.ServiceInstances.Controls.Add(this.dataGridServiceInstances);
            this.ServiceInstances.Location = new System.Drawing.Point(4, 22);
            this.ServiceInstances.Name = "ServiceInstances";
            this.ServiceInstances.Size = new System.Drawing.Size(794, 262);
            this.ServiceInstances.TabIndex = 5;
            this.ServiceInstances.Text = "ServiceInstances";
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(794, 25);
            this.toolStrip1.TabIndex = 7;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::SPAFilter.Properties.Resources.icons8_plus_20;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "Add Activator";
            this.toolStripButton1.Click += new System.EventHandler(this.AddActivatorButton_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = global::SPAFilter.Properties.Resources.icons8_minus_20;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "Remove Activator";
            this.toolStripButton2.Click += new System.EventHandler(this.RemoveActivatorButton_Click_1);
            // 
            // dataGridServiceInstances
            // 
            this.dataGridServiceInstances.AllowUserToAddRows = false;
            this.dataGridServiceInstances.AllowUserToDeleteRows = false;
            this.dataGridServiceInstances.AllowUserToOrderColumns = true;
            this.dataGridServiceInstances.AllowUserToResizeRows = false;
            this.dataGridServiceInstances.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridServiceInstances.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridServiceInstances.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridServiceInstances.Location = new System.Drawing.Point(0, 28);
            this.dataGridServiceInstances.Name = "dataGridServiceInstances";
            this.dataGridServiceInstances.ReadOnly = true;
            this.dataGridServiceInstances.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridServiceInstances.Size = new System.Drawing.Size(792, 234);
            this.dataGridServiceInstances.TabIndex = 6;
            this.dataGridServiceInstances.DoubleClick += new System.EventHandler(this.DataGridServiceInstances_DoubleClick);
            // 
            // Scenarios
            // 
            this.Scenarios.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Scenarios.Controls.Add(this.dataGridScenarios);
            this.Scenarios.Location = new System.Drawing.Point(4, 22);
            this.Scenarios.Name = "Scenarios";
            this.Scenarios.Size = new System.Drawing.Size(794, 262);
            this.Scenarios.TabIndex = 2;
            this.Scenarios.Text = "Scenarios";
            // 
            // dataGridScenarios
            // 
            this.dataGridScenarios.AllowUserToAddRows = false;
            this.dataGridScenarios.AllowUserToDeleteRows = false;
            this.dataGridScenarios.AllowUserToOrderColumns = true;
            this.dataGridScenarios.AllowUserToResizeRows = false;
            this.dataGridScenarios.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridScenarios.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridScenarios.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridScenarios.Location = new System.Drawing.Point(0, 0);
            this.dataGridScenarios.Name = "dataGridScenarios";
            this.dataGridScenarios.ReadOnly = true;
            this.dataGridScenarios.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridScenarios.Size = new System.Drawing.Size(794, 266);
            this.dataGridScenarios.TabIndex = 5;
            this.dataGridScenarios.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridScenariosResult_CellDoubleClick);
            // 
            // Commands
            // 
            this.Commands.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Commands.Controls.Add(this.dataGridCommands);
            this.Commands.Location = new System.Drawing.Point(4, 22);
            this.Commands.Name = "Commands";
            this.Commands.Size = new System.Drawing.Size(794, 262);
            this.Commands.TabIndex = 3;
            this.Commands.Text = "Commands";
            // 
            // dataGridCommands
            // 
            this.dataGridCommands.AllowUserToAddRows = false;
            this.dataGridCommands.AllowUserToDeleteRows = false;
            this.dataGridCommands.AllowUserToOrderColumns = true;
            this.dataGridCommands.AllowUserToResizeRows = false;
            this.dataGridCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridCommands.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridCommands.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridCommands.Location = new System.Drawing.Point(0, 0);
            this.dataGridCommands.Name = "dataGridCommands";
            this.dataGridCommands.ReadOnly = true;
            this.dataGridCommands.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridCommands.Size = new System.Drawing.Size(794, 266);
            this.dataGridCommands.TabIndex = 1;
            this.dataGridCommands.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridCommandsResult_CellMouseDoubleClick);
            // 
            // GenerateSC
            // 
            this.GenerateSC.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.GenerateSC.Controls.Add(this.RootSCExportPathButton);
            this.GenerateSC.Controls.Add(this.label9);
            this.GenerateSC.Controls.Add(this.ExportSCPath);
            this.GenerateSC.Controls.Add(this.OpenSevExelButton);
            this.GenerateSC.Controls.Add(this.label5);
            this.GenerateSC.Controls.Add(this.ButtonGenerateSC);
            this.GenerateSC.Controls.Add(this.OpenSCXlsx);
            this.GenerateSC.Location = new System.Drawing.Point(4, 22);
            this.GenerateSC.Name = "GenerateSC";
            this.GenerateSC.Size = new System.Drawing.Size(794, 262);
            this.GenerateSC.TabIndex = 4;
            this.GenerateSC.Text = "Generate SC";
            // 
            // RootSCExportPathButton
            // 
            this.RootSCExportPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
            this.ExportSCPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportSCPath.Location = new System.Drawing.Point(82, 14);
            this.ExportSCPath.Name = "ExportSCPath";
            this.ExportSCPath.Size = new System.Drawing.Size(627, 20);
            this.ExportSCPath.TabIndex = 28;
            this.ExportSCPath.TextChanged += new System.EventHandler(this.ExportSCPath_TextChanged);
            // 
            // OpenSevExelButton
            // 
            this.OpenSevExelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
            this.ButtonGenerateSC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
            this.OpenSCXlsx.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenSCXlsx.Location = new System.Drawing.Point(82, 43);
            this.OpenSCXlsx.Name = "OpenSCXlsx";
            this.OpenSCXlsx.Size = new System.Drawing.Size(627, 20);
            this.OpenSCXlsx.TabIndex = 0;
            this.OpenSCXlsx.TextChanged += new System.EventHandler(this.OpenSCXlsx_TextChanged);
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
            this.groupBox1.Location = new System.Drawing.Point(12, 91);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(785, 87);
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filter Settings";
            // 
            // PrintXMLButton
            // 
            this.PrintXMLButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PrintXMLButton.Location = new System.Drawing.Point(609, 58);
            this.PrintXMLButton.Name = "PrintXMLButton";
            this.PrintXMLButton.Size = new System.Drawing.Size(89, 23);
            this.PrintXMLButton.TabIndex = 14;
            this.PrintXMLButton.Text = "Print XML";
            this.PrintXMLButton.UseVisualStyleBackColor = true;
            this.PrintXMLButton.Click += new System.EventHandler(this.PrintXMLButton_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BPCount,
            this.NEElementsCount,
            this.OperationsCount,
            this.ScenariosCount,
            this.CommandsCount});
            this.statusStrip1.Location = new System.Drawing.Point(0, 475);
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
            this.NEElementsCount.Size = new System.Drawing.Size(65, 17);
            this.NEElementsCount.Text = "HostTypes:";
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
            // ServiceCatalogTextBox
            // 
            this.ServiceCatalogTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ServiceCatalogTextBox.Enabled = false;
            this.ServiceCatalogTextBox.Location = new System.Drawing.Point(130, 64);
            this.ServiceCatalogTextBox.Name = "ServiceCatalogTextBox";
            this.ServiceCatalogTextBox.Size = new System.Drawing.Size(586, 20);
            this.ServiceCatalogTextBox.TabIndex = 33;
            this.ServiceCatalogTextBox.TextChanged += new System.EventHandler(this.ServiceCatalogTextBox_TextChanged);
            // 
            // ServiceCatalogOpenButton
            // 
            this.ServiceCatalogOpenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ServiceCatalogOpenButton.Location = new System.Drawing.Point(722, 62);
            this.ServiceCatalogOpenButton.Name = "ServiceCatalogOpenButton";
            this.ServiceCatalogOpenButton.Size = new System.Drawing.Size(75, 23);
            this.ServiceCatalogOpenButton.TabIndex = 34;
            this.ServiceCatalogOpenButton.Text = "Open";
            this.ServiceCatalogOpenButton.UseVisualStyleBackColor = true;
            this.ServiceCatalogOpenButton.Click += new System.EventHandler(this.ServiceCatalogOpenButton_Click);
            // 
            // ROBPOperationsRadioButton
            // 
            this.ROBPOperationsRadioButton.AutoSize = true;
            this.ROBPOperationsRadioButton.Checked = true;
            this.ROBPOperationsRadioButton.Location = new System.Drawing.Point(12, 39);
            this.ROBPOperationsRadioButton.Name = "ROBPOperationsRadioButton";
            this.ROBPOperationsRadioButton.Size = new System.Drawing.Size(112, 17);
            this.ROBPOperationsRadioButton.TabIndex = 35;
            this.ROBPOperationsRadioButton.TabStop = true;
            this.ROBPOperationsRadioButton.Text = "ROBP Operations:";
            this.ROBPOperationsRadioButton.UseVisualStyleBackColor = true;
            this.ROBPOperationsRadioButton.CheckedChanged += new System.EventHandler(this.ROBPOperationsRadioButton_CheckedChanged);
            // 
            // ServiceCatalogRadioButton
            // 
            this.ServiceCatalogRadioButton.AutoSize = true;
            this.ServiceCatalogRadioButton.Location = new System.Drawing.Point(12, 65);
            this.ServiceCatalogRadioButton.Name = "ServiceCatalogRadioButton";
            this.ServiceCatalogRadioButton.Size = new System.Drawing.Size(103, 17);
            this.ServiceCatalogRadioButton.TabIndex = 36;
            this.ServiceCatalogRadioButton.Text = "Service Catalog:";
            this.ServiceCatalogRadioButton.UseVisualStyleBackColor = true;
            this.ServiceCatalogRadioButton.CheckedChanged += new System.EventHandler(this.ServiceCatalogRadioButton_CheckedChanged);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(7, 458);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(794, 10);
            this.progressBar.TabIndex = 23;
            this.progressBar.Visible = false;
            // 
            // SPAFilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(807, 497);
            this.Controls.Add(this.ServiceCatalogRadioButton);
            this.Controls.Add(this.ROBPOperationsRadioButton);
            this.Controls.Add(this.ServiceCatalogOpenButton);
            this.Controls.Add(this.ServiceCatalogTextBox);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.ROBPOperationButtonOpen);
            this.Controls.Add(this.ProcessesButtonOpen);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ROBPOperationTextBox);
            this.Controls.Add(this.ProcessesTextBox);
            this.Icon = global::SPAFilter.Properties.Resources.icons8;
            this.MinimumSize = new System.Drawing.Size(823, 500);
            this.Name = "SPAFilterForm";
            this.Text = "SPA Filter";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridProcesses)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.Processes.ResumeLayout(false);
            this.Operations.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridOperations)).EndInit();
            this.ServiceInstances.ResumeLayout(false);
            this.ServiceInstances.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridServiceInstances)).EndInit();
            this.Scenarios.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridScenarios)).EndInit();
            this.Commands.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCommands)).EndInit();
            this.GenerateSC.ResumeLayout(false);
            this.GenerateSC.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox ProcessesTextBox;
        private System.Windows.Forms.TextBox ROBPOperationTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ROBPOperationButtonOpen;
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
        private System.Windows.Forms.TabPage Processes;
        private System.Windows.Forms.TabPage Operations;
        private System.Windows.Forms.DataGridView dataGridOperations;
        private System.Windows.Forms.TabPage Scenarios;
        private System.Windows.Forms.TabPage Commands;
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
        private System.Windows.Forms.TextBox ServiceCatalogTextBox;
        private System.Windows.Forms.Button ServiceCatalogOpenButton;
        private System.Windows.Forms.RadioButton ROBPOperationsRadioButton;
        private System.Windows.Forms.RadioButton ServiceCatalogRadioButton;
        private System.Windows.Forms.TabPage ServiceInstances;
        private System.Windows.Forms.DataGridView dataGridServiceInstances;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
    }
}

