using System.Drawing;
using System.Windows.Forms;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SPAFilterForm));
            this.ProcessesTextBox = new System.Windows.Forms.TextBox();
            this.ROBPOperationTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ROBPOperationButtonOpen = new System.Windows.Forms.Button();
            this.ProcessesButtonOpen = new System.Windows.Forms.Button();
            this.OperationComboBox = new System.Windows.Forms.ComboBox();
            this.NetSettComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.ProcessesComboBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonFilter = new System.Windows.Forms.Button();
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.ServiceInstances = new System.Windows.Forms.TabPage();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.addServiceInstancesButton = new System.Windows.Forms.ToolStripButton();
            this.removeServiceInstancesButton = new System.Windows.Forms.ToolStripButton();
            this.refreshServiceInstancesButton = new System.Windows.Forms.ToolStripButton();
            this.reloadServiceInstancesButton = new System.Windows.Forms.ToolStripButton();
            this.dataGridServiceInstances = new System.Windows.Forms.DataGridView();
            this.Processes = new System.Windows.Forms.TabPage();
            this.dataGridProcesses = new System.Windows.Forms.DataGridView();
            this.Operations = new System.Windows.Forms.TabPage();
            this.dataGridOperations = new System.Windows.Forms.DataGridView();
            this.Scenarios = new System.Windows.Forms.TabPage();
            this.dataGridScenarios = new System.Windows.Forms.DataGridView();
            this.Commands = new System.Windows.Forms.TabPage();
            this.dataGridCommands = new System.Windows.Forms.DataGridView();
            this.GenerateSC = new System.Windows.Forms.TabPage();
            this.RootSCExportPathButton = new System.Windows.Forms.Button();
            this.ExportPathLabel = new System.Windows.Forms.Label();
            this.ExportSCPath = new System.Windows.Forms.TextBox();
            this.OpenSevExelButton = new System.Windows.Forms.Button();
            this.RDServicesLabel = new System.Windows.Forms.Label();
            this.ButtonGenerateSC = new System.Windows.Forms.Button();
            this.OpenSCXlsx = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonReset = new System.Windows.Forms.Button();
            this.PrintXMLButton = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.ServiceCatalogTextBox = new System.Windows.Forms.TextBox();
            this.ServiceCatalogOpenButton = new System.Windows.Forms.Button();
            this.ROBPOperationsRadioButton = new System.Windows.Forms.RadioButton();
            this.ServiceCatalogRadioButton = new System.Windows.Forms.RadioButton();
            this.panelTop = new System.Windows.Forms.Panel();
            this.progressBar = new SPAFilter.CustomProgressBar();
            this.mainTabControl.SuspendLayout();
            this.ServiceInstances.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridServiceInstances)).BeginInit();
            this.Processes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridProcesses)).BeginInit();
            this.Operations.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridOperations)).BeginInit();
            this.Scenarios.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridScenarios)).BeginInit();
            this.Commands.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCommands)).BeginInit();
            this.GenerateSC.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // ProcessesTextBox
            // 
            this.ProcessesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProcessesTextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ProcessesTextBox.Location = new System.Drawing.Point(69, 11);
            this.ProcessesTextBox.Name = "ProcessesTextBox";
            this.ProcessesTextBox.Size = new System.Drawing.Size(763, 23);
            this.ProcessesTextBox.TabIndex = 1;
            // 
            // ROBPOperationTextBox
            // 
            this.ROBPOperationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ROBPOperationTextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ROBPOperationTextBox.Location = new System.Drawing.Point(130, 40);
            this.ROBPOperationTextBox.Name = "ROBPOperationTextBox";
            this.ROBPOperationTextBox.Size = new System.Drawing.Size(702, 23);
            this.ROBPOperationTextBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label1.Location = new System.Drawing.Point(9, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Processes";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ROBPOperationButtonOpen
            // 
            this.ROBPOperationButtonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ROBPOperationButtonOpen.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ROBPOperationButtonOpen.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ROBPOperationButtonOpen.Image = global::SPAFilter.Properties.Resources.folder2;
            this.ROBPOperationButtonOpen.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ROBPOperationButtonOpen.Location = new System.Drawing.Point(838, 40);
            this.ROBPOperationButtonOpen.Name = "ROBPOperationButtonOpen";
            this.ROBPOperationButtonOpen.Size = new System.Drawing.Size(69, 23);
            this.ROBPOperationButtonOpen.TabIndex = 6;
            this.ROBPOperationButtonOpen.Text = "Open";
            this.ROBPOperationButtonOpen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ROBPOperationButtonOpen.UseVisualStyleBackColor = true;
            // 
            // ProcessesButtonOpen
            // 
            this.ProcessesButtonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ProcessesButtonOpen.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ProcessesButtonOpen.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ProcessesButtonOpen.Image = global::SPAFilter.Properties.Resources.folder2;
            this.ProcessesButtonOpen.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ProcessesButtonOpen.Location = new System.Drawing.Point(838, 11);
            this.ProcessesButtonOpen.Name = "ProcessesButtonOpen";
            this.ProcessesButtonOpen.Size = new System.Drawing.Size(69, 23);
            this.ProcessesButtonOpen.TabIndex = 5;
            this.ProcessesButtonOpen.Text = "Open";
            this.ProcessesButtonOpen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ProcessesButtonOpen.UseVisualStyleBackColor = true;
            // 
            // OperationComboBox
            // 
            this.OperationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OperationComboBox.FormattingEnabled = true;
            this.OperationComboBox.Location = new System.Drawing.Point(521, 22);
            this.OperationComboBox.Name = "OperationComboBox";
            this.OperationComboBox.Size = new System.Drawing.Size(386, 23);
            this.OperationComboBox.TabIndex = 10;
            // 
            // NetSettComboBox
            // 
            this.NetSettComboBox.FormattingEnabled = true;
            this.NetSettComboBox.Location = new System.Drawing.Point(67, 49);
            this.NetSettComboBox.Name = "NetSettComboBox";
            this.NetSettComboBox.Size = new System.Drawing.Size(385, 23);
            this.NetSettComboBox.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label6.Location = new System.Drawing.Point(6, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 15);
            this.label6.TabIndex = 8;
            this.label6.Text = "HostType";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label7.Location = new System.Drawing.Point(458, 27);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 15);
            this.label7.TabIndex = 7;
            this.label7.Text = "Operation";
            // 
            // ProcessesComboBox
            // 
            this.ProcessesComboBox.FormattingEnabled = true;
            this.ProcessesComboBox.Location = new System.Drawing.Point(67, 22);
            this.ProcessesComboBox.Name = "ProcessesComboBox";
            this.ProcessesComboBox.Size = new System.Drawing.Size(385, 23);
            this.ProcessesComboBox.TabIndex = 12;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label8.Location = new System.Drawing.Point(5, 27);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 15);
            this.label8.TabIndex = 11;
            this.label8.Text = "Process";
            // 
            // buttonFilter
            // 
            this.buttonFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFilter.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonFilter.Image = global::SPAFilter.Properties.Resources.find;
            this.buttonFilter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonFilter.Location = new System.Drawing.Point(612, 49);
            this.buttonFilter.Name = "buttonFilter";
            this.buttonFilter.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.buttonFilter.Size = new System.Drawing.Size(92, 23);
            this.buttonFilter.TabIndex = 13;
            this.buttonFilter.Text = "     Filter [F5]";
            this.buttonFilter.UseVisualStyleBackColor = true;
            this.buttonFilter.Click += new System.EventHandler(this.FilterButton_Click);
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.ServiceInstances);
            this.mainTabControl.Controls.Add(this.Processes);
            this.mainTabControl.Controls.Add(this.Operations);
            this.mainTabControl.Controls.Add(this.Scenarios);
            this.mainTabControl.Controls.Add(this.Commands);
            this.mainTabControl.Controls.Add(this.GenerateSC);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.mainTabControl.Location = new System.Drawing.Point(3, 174);
            this.mainTabControl.MinimumSize = new System.Drawing.Size(450, 0);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(912, 326);
            this.mainTabControl.TabIndex = 14;
            // 
            // ServiceInstances
            // 
            this.ServiceInstances.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.ServiceInstances.Controls.Add(this.toolStrip1);
            this.ServiceInstances.Controls.Add(this.dataGridServiceInstances);
            this.ServiceInstances.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ServiceInstances.Location = new System.Drawing.Point(4, 24);
            this.ServiceInstances.Name = "ServiceInstances";
            this.ServiceInstances.Size = new System.Drawing.Size(904, 298);
            this.ServiceInstances.TabIndex = 5;
            this.ServiceInstances.Text = "Service Instances";
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addServiceInstancesButton,
            this.removeServiceInstancesButton,
            this.refreshServiceInstancesButton,
            this.reloadServiceInstancesButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(904, 25);
            this.toolStrip1.TabIndex = 7;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // addServiceInstancesButton
            // 
            this.addServiceInstancesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addServiceInstancesButton.Image = global::SPAFilter.Properties.Resources.icons8_plus_20;
            this.addServiceInstancesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addServiceInstancesButton.Margin = new System.Windows.Forms.Padding(4, 1, 2, 2);
            this.addServiceInstancesButton.Name = "addServiceInstancesButton";
            this.addServiceInstancesButton.Size = new System.Drawing.Size(23, 22);
            this.addServiceInstancesButton.Click += new System.EventHandler(this.AddActivatorButton_Click);
            // 
            // removeServiceInstancesButton
            // 
            this.removeServiceInstancesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.removeServiceInstancesButton.Image = global::SPAFilter.Properties.Resources.icons8_minus_20;
            this.removeServiceInstancesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.removeServiceInstancesButton.Margin = new System.Windows.Forms.Padding(0, 1, 3, 2);
            this.removeServiceInstancesButton.Name = "removeServiceInstancesButton";
            this.removeServiceInstancesButton.Size = new System.Drawing.Size(23, 22);
            this.removeServiceInstancesButton.Click += new System.EventHandler(this.RemoveActivatorButton_Click);
            // 
            // refreshServiceInstancesButton
            // 
            this.refreshServiceInstancesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.refreshServiceInstancesButton.Image = global::SPAFilter.Properties.Resources.icons8_synchronize_20;
            this.refreshServiceInstancesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.refreshServiceInstancesButton.Margin = new System.Windows.Forms.Padding(0, 1, 3, 2);
            this.refreshServiceInstancesButton.Name = "refreshServiceInstancesButton";
            this.refreshServiceInstancesButton.Size = new System.Drawing.Size(23, 22);
            this.refreshServiceInstancesButton.Click += new System.EventHandler(this.RefreshActivatorButton_Click);
            // 
            // reloadServiceInstancesButton
            // 
            this.reloadServiceInstancesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.reloadServiceInstancesButton.Image = global::SPAFilter.Properties.Resources.reload;
            this.reloadServiceInstancesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.reloadServiceInstancesButton.Margin = new System.Windows.Forms.Padding(0, 1, 2, 2);
            this.reloadServiceInstancesButton.Name = "reloadServiceInstancesButton";
            this.reloadServiceInstancesButton.Size = new System.Drawing.Size(23, 22);
            this.reloadServiceInstancesButton.Click += new System.EventHandler(this.ReloadActivatorButton_Click);
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
            this.dataGridServiceInstances.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridServiceInstances.Location = new System.Drawing.Point(0, 24);
            this.dataGridServiceInstances.Name = "dataGridServiceInstances";
            this.dataGridServiceInstances.ReadOnly = true;
            this.dataGridServiceInstances.RowHeadersVisible = false;
            this.dataGridServiceInstances.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridServiceInstances.Size = new System.Drawing.Size(905, 275);
            this.dataGridServiceInstances.TabIndex = 6;
            this.dataGridServiceInstances.DoubleClick += new System.EventHandler(this.DataGridServiceInstances_DoubleClick);
            // 
            // Processes
            // 
            this.Processes.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Processes.Controls.Add(this.dataGridProcesses);
            this.Processes.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Processes.Location = new System.Drawing.Point(4, 24);
            this.Processes.Name = "Processes";
            this.Processes.Size = new System.Drawing.Size(904, 298);
            this.Processes.TabIndex = 0;
            this.Processes.Text = "Processes";
            // 
            // dataGridProcesses
            // 
            this.dataGridProcesses.AllowUserToAddRows = false;
            this.dataGridProcesses.AllowUserToDeleteRows = false;
            this.dataGridProcesses.AllowUserToOrderColumns = true;
            this.dataGridProcesses.AllowUserToResizeRows = false;
            this.dataGridProcesses.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridProcesses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridProcesses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridProcesses.Location = new System.Drawing.Point(0, 0);
            this.dataGridProcesses.Name = "dataGridProcesses";
            this.dataGridProcesses.ReadOnly = true;
            this.dataGridProcesses.RowHeadersVisible = false;
            this.dataGridProcesses.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridProcesses.Size = new System.Drawing.Size(904, 298);
            this.dataGridProcesses.TabIndex = 0;
            this.dataGridProcesses.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridProcessesResults_CellMouseDoubleClick);
            // 
            // Operations
            // 
            this.Operations.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Operations.Controls.Add(this.dataGridOperations);
            this.Operations.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Operations.Location = new System.Drawing.Point(4, 24);
            this.Operations.Name = "Operations";
            this.Operations.Size = new System.Drawing.Size(904, 298);
            this.Operations.TabIndex = 1;
            this.Operations.Text = "Operations";
            // 
            // dataGridOperations
            // 
            this.dataGridOperations.AllowUserToAddRows = false;
            this.dataGridOperations.AllowUserToDeleteRows = false;
            this.dataGridOperations.AllowUserToOrderColumns = true;
            this.dataGridOperations.AllowUserToResizeRows = false;
            this.dataGridOperations.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridOperations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridOperations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridOperations.Location = new System.Drawing.Point(0, 0);
            this.dataGridOperations.Name = "dataGridOperations";
            this.dataGridOperations.ReadOnly = true;
            this.dataGridOperations.RowHeadersVisible = false;
            this.dataGridOperations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridOperations.Size = new System.Drawing.Size(904, 298);
            this.dataGridOperations.TabIndex = 0;
            this.dataGridOperations.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridOperationsResult_CellDoubleClick);
            // 
            // Scenarios
            // 
            this.Scenarios.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Scenarios.Controls.Add(this.dataGridScenarios);
            this.Scenarios.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Scenarios.Location = new System.Drawing.Point(4, 24);
            this.Scenarios.Name = "Scenarios";
            this.Scenarios.Size = new System.Drawing.Size(904, 298);
            this.Scenarios.TabIndex = 2;
            this.Scenarios.Text = "Scenarios";
            // 
            // dataGridScenarios
            // 
            this.dataGridScenarios.AllowUserToAddRows = false;
            this.dataGridScenarios.AllowUserToDeleteRows = false;
            this.dataGridScenarios.AllowUserToOrderColumns = true;
            this.dataGridScenarios.AllowUserToResizeRows = false;
            this.dataGridScenarios.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridScenarios.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridScenarios.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridScenarios.Location = new System.Drawing.Point(0, 0);
            this.dataGridScenarios.Name = "dataGridScenarios";
            this.dataGridScenarios.ReadOnly = true;
            this.dataGridScenarios.RowHeadersVisible = false;
            this.dataGridScenarios.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridScenarios.Size = new System.Drawing.Size(904, 298);
            this.dataGridScenarios.TabIndex = 5;
            this.dataGridScenarios.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridScenariosResult_CellDoubleClick);
            // 
            // Commands
            // 
            this.Commands.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Commands.Controls.Add(this.dataGridCommands);
            this.Commands.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Commands.Location = new System.Drawing.Point(4, 24);
            this.Commands.Name = "Commands";
            this.Commands.Size = new System.Drawing.Size(904, 298);
            this.Commands.TabIndex = 3;
            this.Commands.Text = "Commands";
            // 
            // dataGridCommands
            // 
            this.dataGridCommands.AllowUserToAddRows = false;
            this.dataGridCommands.AllowUserToDeleteRows = false;
            this.dataGridCommands.AllowUserToOrderColumns = true;
            this.dataGridCommands.AllowUserToResizeRows = false;
            this.dataGridCommands.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridCommands.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridCommands.Location = new System.Drawing.Point(0, 0);
            this.dataGridCommands.Name = "dataGridCommands";
            this.dataGridCommands.ReadOnly = true;
            this.dataGridCommands.RowHeadersVisible = false;
            this.dataGridCommands.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridCommands.Size = new System.Drawing.Size(904, 298);
            this.dataGridCommands.TabIndex = 1;
            this.dataGridCommands.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridCommandsResult_CellMouseDoubleClick);
            // 
            // GenerateSC
            // 
            this.GenerateSC.BackColor = System.Drawing.Color.DarkGray;
            this.GenerateSC.Controls.Add(this.RootSCExportPathButton);
            this.GenerateSC.Controls.Add(this.ExportPathLabel);
            this.GenerateSC.Controls.Add(this.ExportSCPath);
            this.GenerateSC.Controls.Add(this.OpenSevExelButton);
            this.GenerateSC.Controls.Add(this.RDServicesLabel);
            this.GenerateSC.Controls.Add(this.ButtonGenerateSC);
            this.GenerateSC.Controls.Add(this.OpenSCXlsx);
            this.GenerateSC.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.GenerateSC.Location = new System.Drawing.Point(4, 24);
            this.GenerateSC.Name = "GenerateSC";
            this.GenerateSC.Size = new System.Drawing.Size(904, 298);
            this.GenerateSC.TabIndex = 4;
            this.GenerateSC.Text = "Generate SC";
            // 
            // RootSCExportPathButton
            // 
            this.RootSCExportPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RootSCExportPathButton.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.RootSCExportPathButton.Image = global::SPAFilter.Properties.Resources.folder2;
            this.RootSCExportPathButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.RootSCExportPathButton.Location = new System.Drawing.Point(797, 16);
            this.RootSCExportPathButton.Name = "RootSCExportPathButton";
            this.RootSCExportPathButton.Size = new System.Drawing.Size(104, 23);
            this.RootSCExportPathButton.TabIndex = 30;
            this.RootSCExportPathButton.Text = "     Root";
            this.RootSCExportPathButton.UseVisualStyleBackColor = true;
            this.RootSCExportPathButton.Click += new System.EventHandler(this.RootSCExportPathButton_Click);
            // 
            // ExportPathLabel
            // 
            this.ExportPathLabel.AutoSize = true;
            this.ExportPathLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.ExportPathLabel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ExportPathLabel.Location = new System.Drawing.Point(13, 19);
            this.ExportPathLabel.Name = "ExportPathLabel";
            this.ExportPathLabel.Size = new System.Drawing.Size(68, 15);
            this.ExportPathLabel.TabIndex = 29;
            this.ExportPathLabel.Text = "Export Path";
            this.ExportPathLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ExportSCPath
            // 
            this.ExportSCPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportSCPath.Location = new System.Drawing.Point(90, 16);
            this.ExportSCPath.Name = "ExportSCPath";
            this.ExportSCPath.Size = new System.Drawing.Size(701, 23);
            this.ExportSCPath.TabIndex = 28;
            this.ExportSCPath.TextChanged += new System.EventHandler(this.ExportSCPath_TextChanged);
            // 
            // OpenSevExelButton
            // 
            this.OpenSevExelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenSevExelButton.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.OpenSevExelButton.Image = global::SPAFilter.Properties.Resources.xls1;
            this.OpenSevExelButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.OpenSevExelButton.Location = new System.Drawing.Point(797, 43);
            this.OpenSevExelButton.Name = "OpenSevExelButton";
            this.OpenSevExelButton.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.OpenSevExelButton.Size = new System.Drawing.Size(104, 23);
            this.OpenSevExelButton.TabIndex = 27;
            this.OpenSevExelButton.Text = "     Open xlsx";
            this.OpenSevExelButton.UseVisualStyleBackColor = true;
            this.OpenSevExelButton.Click += new System.EventHandler(this.OpenRDServiceExelButton_Click);
            // 
            // RDServicesLabel
            // 
            this.RDServicesLabel.AutoSize = true;
            this.RDServicesLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.RDServicesLabel.Location = new System.Drawing.Point(13, 47);
            this.RDServicesLabel.Name = "RDServicesLabel";
            this.RDServicesLabel.Size = new System.Drawing.Size(69, 15);
            this.RDServicesLabel.TabIndex = 26;
            this.RDServicesLabel.Text = "RD Services";
            this.RDServicesLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ButtonGenerateSC
            // 
            this.ButtonGenerateSC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonGenerateSC.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ButtonGenerateSC.Enabled = false;
            this.ButtonGenerateSC.Image = global::SPAFilter.Properties.Resources.generatesc;
            this.ButtonGenerateSC.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonGenerateSC.Location = new System.Drawing.Point(797, 70);
            this.ButtonGenerateSC.Name = "ButtonGenerateSC";
            this.ButtonGenerateSC.Size = new System.Drawing.Size(106, 24);
            this.ButtonGenerateSC.TabIndex = 26;
            this.ButtonGenerateSC.Text = "      Generate SC";
            this.ButtonGenerateSC.UseVisualStyleBackColor = true;
            this.ButtonGenerateSC.Click += new System.EventHandler(this.ButtonGenerateSC_Click);
            // 
            // OpenSCXlsx
            // 
            this.OpenSCXlsx.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenSCXlsx.Location = new System.Drawing.Point(90, 43);
            this.OpenSCXlsx.Name = "OpenSCXlsx";
            this.OpenSCXlsx.Size = new System.Drawing.Size(701, 23);
            this.OpenSCXlsx.TabIndex = 0;
            this.OpenSCXlsx.TextChanged += new System.EventHandler(this.OpenSCXlsx_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonReset);
            this.groupBox1.Controls.Add(this.PrintXMLButton);
            this.groupBox1.Controls.Add(this.OperationComboBox);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.NetSettComboBox);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.ProcessesComboBox);
            this.groupBox1.Controls.Add(this.buttonFilter);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.groupBox1.Location = new System.Drawing.Point(3, 95);
            this.groupBox1.MinimumSize = new System.Drawing.Size(830, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(912, 79);
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filter";
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReset.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonReset.Image = global::SPAFilter.Properties.Resources.reset2;
            this.buttonReset.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonReset.Location = new System.Drawing.Point(710, 49);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(92, 23);
            this.buttonReset.TabIndex = 15;
            this.buttonReset.Text = "      Reset [F6]";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // PrintXMLButton
            // 
            this.PrintXMLButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PrintXMLButton.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.PrintXMLButton.Image = ((System.Drawing.Image)(resources.GetObject("PrintXMLButton.Image")));
            this.PrintXMLButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.PrintXMLButton.Location = new System.Drawing.Point(808, 49);
            this.PrintXMLButton.Name = "PrintXMLButton";
            this.PrintXMLButton.Size = new System.Drawing.Size(99, 23);
            this.PrintXMLButton.TabIndex = 14;
            this.PrintXMLButton.Text = "Pretty print";
            this.PrintXMLButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.PrintXMLButton.UseVisualStyleBackColor = true;
            this.PrintXMLButton.Click += new System.EventHandler(this.PrintXMLButton_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Location = new System.Drawing.Point(3, 509);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip.Size = new System.Drawing.Size(912, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 27;
            this.statusStrip.Text = "statusStrip1";
            // 
            // ServiceCatalogTextBox
            // 
            this.ServiceCatalogTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ServiceCatalogTextBox.Enabled = false;
            this.ServiceCatalogTextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ServiceCatalogTextBox.Location = new System.Drawing.Point(130, 67);
            this.ServiceCatalogTextBox.Name = "ServiceCatalogTextBox";
            this.ServiceCatalogTextBox.Size = new System.Drawing.Size(702, 23);
            this.ServiceCatalogTextBox.TabIndex = 33;
            // 
            // ServiceCatalogOpenButton
            // 
            this.ServiceCatalogOpenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ServiceCatalogOpenButton.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ServiceCatalogOpenButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ServiceCatalogOpenButton.Image = global::SPAFilter.Properties.Resources.file7;
            this.ServiceCatalogOpenButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ServiceCatalogOpenButton.Location = new System.Drawing.Point(838, 67);
            this.ServiceCatalogOpenButton.Margin = new System.Windows.Forms.Padding(0);
            this.ServiceCatalogOpenButton.Name = "ServiceCatalogOpenButton";
            this.ServiceCatalogOpenButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ServiceCatalogOpenButton.Size = new System.Drawing.Size(69, 23);
            this.ServiceCatalogOpenButton.TabIndex = 34;
            this.ServiceCatalogOpenButton.Text = "Open";
            this.ServiceCatalogOpenButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ServiceCatalogOpenButton.UseVisualStyleBackColor = true;
            // 
            // ROBPOperationsRadioButton
            // 
            this.ROBPOperationsRadioButton.AutoSize = true;
            this.ROBPOperationsRadioButton.Checked = true;
            this.ROBPOperationsRadioButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ROBPOperationsRadioButton.Location = new System.Drawing.Point(12, 41);
            this.ROBPOperationsRadioButton.Name = "ROBPOperationsRadioButton";
            this.ROBPOperationsRadioButton.Size = new System.Drawing.Size(116, 19);
            this.ROBPOperationsRadioButton.TabIndex = 35;
            this.ROBPOperationsRadioButton.TabStop = true;
            this.ROBPOperationsRadioButton.Text = "ROBP Operations";
            this.ROBPOperationsRadioButton.UseVisualStyleBackColor = true;
            // 
            // ServiceCatalogRadioButton
            // 
            this.ServiceCatalogRadioButton.AutoSize = true;
            this.ServiceCatalogRadioButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ServiceCatalogRadioButton.Location = new System.Drawing.Point(12, 69);
            this.ServiceCatalogRadioButton.Name = "ServiceCatalogRadioButton";
            this.ServiceCatalogRadioButton.Size = new System.Drawing.Size(106, 19);
            this.ServiceCatalogRadioButton.TabIndex = 36;
            this.ServiceCatalogRadioButton.Text = "Service Catalog";
            this.ServiceCatalogRadioButton.UseVisualStyleBackColor = true;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.ProcessesTextBox);
            this.panelTop.Controls.Add(this.ROBPOperationTextBox);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.ProcessesButtonOpen);
            this.panelTop.Controls.Add(this.ServiceCatalogRadioButton);
            this.panelTop.Controls.Add(this.ROBPOperationButtonOpen);
            this.panelTop.Controls.Add(this.ROBPOperationsRadioButton);
            this.panelTop.Controls.Add(this.ServiceCatalogTextBox);
            this.panelTop.Controls.Add(this.ServiceCatalogOpenButton);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(3, 3);
            this.panelTop.MinimumSize = new System.Drawing.Size(300, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(912, 92);
            this.panelTop.TabIndex = 38;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(3, 500);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(912, 9);
            this.progressBar.TabIndex = 23;
            this.progressBar.Visible = false;
            // 
            // SPAFilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(918, 534);
            this.Controls.Add(this.mainTabControl);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SPAFilterForm";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SPA Filter";
            this.mainTabControl.ResumeLayout(false);
            this.ServiceInstances.ResumeLayout(false);
            this.ServiceInstances.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridServiceInstances)).EndInit();
            this.Processes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridProcesses)).EndInit();
            this.Operations.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridOperations)).EndInit();
            this.Scenarios.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridScenarios)).EndInit();
            this.Commands.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCommands)).EndInit();
            this.GenerateSC.ResumeLayout(false);
            this.GenerateSC.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox ProcessesTextBox;
        private System.Windows.Forms.TextBox ROBPOperationTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ROBPOperationButtonOpen;
        private System.Windows.Forms.Button ProcessesButtonOpen;
        private System.Windows.Forms.ComboBox OperationComboBox;
        private System.Windows.Forms.ComboBox NetSettComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox ProcessesComboBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button buttonFilter;
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage Processes;
        private System.Windows.Forms.TabPage Operations;
        private System.Windows.Forms.TabPage Scenarios;
        private System.Windows.Forms.TabPage Commands;
        private System.Windows.Forms.DataGridView dataGridScenarios;
        private System.Windows.Forms.DataGridView dataGridCommands;
        private CustomProgressBar progressBar;
        private System.Windows.Forms.Button ButtonGenerateSC;
        private System.Windows.Forms.TabPage GenerateSC;
        private System.Windows.Forms.Button OpenSevExelButton;
        private System.Windows.Forms.Label RDServicesLabel;
        private System.Windows.Forms.TextBox OpenSCXlsx;
        private System.Windows.Forms.Button RootSCExportPathButton;
        private System.Windows.Forms.Label ExportPathLabel;
        private System.Windows.Forms.TextBox ExportSCPath;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.StatusStrip statusStrip;

        private System.Windows.Forms.Button PrintXMLButton;
        private System.Windows.Forms.TextBox ServiceCatalogTextBox;
        private System.Windows.Forms.RadioButton ROBPOperationsRadioButton;
        private System.Windows.Forms.RadioButton ServiceCatalogRadioButton;
        private System.Windows.Forms.TabPage ServiceInstances;
        private System.Windows.Forms.DataGridView dataGridServiceInstances;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton addServiceInstancesButton;
        private System.Windows.Forms.ToolStripButton removeServiceInstancesButton;
        private System.Windows.Forms.ToolStripButton refreshServiceInstancesButton;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.DataGridView dataGridProcesses;
        private System.Windows.Forms.DataGridView dataGridOperations;
        private Button ServiceCatalogOpenButton;
        private ToolStripButton reloadServiceInstancesButton;
        private Button buttonReset;
    }
}

