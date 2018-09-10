namespace ProcessFilter
{
    partial class ProcessFilterForm
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
            this.dataGridProcessesResults = new System.Windows.Forms.DataGridView();
            this.OperationComboBox = new System.Windows.Forms.ComboBox();
            this.NetSettComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.ProcessesComboBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonFilter = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridOperationsResult = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dataGridScenariosResult = new System.Windows.Forms.DataGridView();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.dataGridCommandsResult = new System.Windows.Forms.DataGridView();
            this.ScenariosTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ScenariosButtonOpen = new System.Windows.Forms.Button();
            this.CommnadsButtonOpen = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.CommandsTextBox = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SenariosStat = new System.Windows.Forms.Label();
            this.CommandsStat = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridProcessesResults)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridOperationsResult)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridScenariosResult)).BeginInit();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCommandsResult)).BeginInit();
            this.SuspendLayout();
            // 
            // ProcessesTextBox
            // 
            this.ProcessesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProcessesTextBox.Location = new System.Drawing.Point(77, 12);
            this.ProcessesTextBox.Name = "ProcessesTextBox";
            this.ProcessesTextBox.Size = new System.Drawing.Size(450, 20);
            this.ProcessesTextBox.TabIndex = 1;
            this.ProcessesTextBox.TextChanged += new System.EventHandler(this.ProcessesTextBox_TextChanged);
            // 
            // OperationTextBox
            // 
            this.OperationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OperationTextBox.Location = new System.Drawing.Point(77, 38);
            this.OperationTextBox.Name = "OperationTextBox";
            this.OperationTextBox.Size = new System.Drawing.Size(450, 20);
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
            this.OperationButtonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OperationButtonOpen.Location = new System.Drawing.Point(533, 36);
            this.OperationButtonOpen.Name = "OperationButtonOpen";
            this.OperationButtonOpen.Size = new System.Drawing.Size(75, 23);
            this.OperationButtonOpen.TabIndex = 6;
            this.OperationButtonOpen.Text = "Open";
            this.OperationButtonOpen.UseVisualStyleBackColor = true;
            this.OperationButtonOpen.Click += new System.EventHandler(this.OperationButtonOpen_Click);
            // 
            // ProcessesButtonOpen
            // 
            this.ProcessesButtonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ProcessesButtonOpen.Location = new System.Drawing.Point(533, 10);
            this.ProcessesButtonOpen.Name = "ProcessesButtonOpen";
            this.ProcessesButtonOpen.Size = new System.Drawing.Size(75, 23);
            this.ProcessesButtonOpen.TabIndex = 5;
            this.ProcessesButtonOpen.Text = "Open";
            this.ProcessesButtonOpen.UseVisualStyleBackColor = true;
            this.ProcessesButtonOpen.Click += new System.EventHandler(this.ProcessesButtonOpen_Click);
            // 
            // dataGridProcessesResults
            // 
            this.dataGridProcessesResults.AllowUserToAddRows = false;
            this.dataGridProcessesResults.AllowUserToDeleteRows = false;
            this.dataGridProcessesResults.AllowUserToOrderColumns = true;
            this.dataGridProcessesResults.AllowUserToResizeRows = false;
            this.dataGridProcessesResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridProcessesResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridProcessesResults.Location = new System.Drawing.Point(0, 0);
            this.dataGridProcessesResults.Name = "dataGridProcessesResults";
            this.dataGridProcessesResults.ReadOnly = true;
            this.dataGridProcessesResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridProcessesResults.Size = new System.Drawing.Size(605, 242);
            this.dataGridProcessesResults.TabIndex = 4;
            this.dataGridProcessesResults.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridProcessesResults_CellMouseDoubleClick);
            // 
            // OperationComboBox
            // 
            this.OperationComboBox.FormattingEnabled = true;
            this.OperationComboBox.Location = new System.Drawing.Point(77, 142);
            this.OperationComboBox.Name = "OperationComboBox";
            this.OperationComboBox.Size = new System.Drawing.Size(251, 21);
            this.OperationComboBox.TabIndex = 10;
            // 
            // NetSettComboBox
            // 
            this.NetSettComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NetSettComboBox.FormattingEnabled = true;
            this.NetSettComboBox.Location = new System.Drawing.Point(431, 115);
            this.NetSettComboBox.Name = "NetSettComboBox";
            this.NetSettComboBox.Size = new System.Drawing.Size(175, 21);
            this.NetSettComboBox.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(334, 119);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Network Element:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 145);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Operation:";
            // 
            // ProcessesComboBox
            // 
            this.ProcessesComboBox.FormattingEnabled = true;
            this.ProcessesComboBox.Location = new System.Drawing.Point(77, 115);
            this.ProcessesComboBox.Name = "ProcessesComboBox";
            this.ProcessesComboBox.Size = new System.Drawing.Size(251, 21);
            this.ProcessesComboBox.TabIndex = 12;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 118);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Process:";
            // 
            // buttonFilter
            // 
            this.buttonFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFilter.Location = new System.Drawing.Point(533, 145);
            this.buttonFilter.Name = "buttonFilter";
            this.buttonFilter.Size = new System.Drawing.Size(75, 23);
            this.buttonFilter.TabIndex = 13;
            this.buttonFilter.Text = "Filter";
            this.buttonFilter.UseVisualStyleBackColor = true;
            this.buttonFilter.Click += new System.EventHandler(this.buttonFilterClick);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(3, 169);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(613, 268);
            this.tabControl1.TabIndex = 14;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dataGridProcessesResults);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(605, 242);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Processes";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dataGridOperationsResult);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(605, 221);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Operations";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridOperationsResult
            // 
            this.dataGridOperationsResult.AllowUserToAddRows = false;
            this.dataGridOperationsResult.AllowUserToDeleteRows = false;
            this.dataGridOperationsResult.AllowUserToOrderColumns = true;
            this.dataGridOperationsResult.AllowUserToResizeRows = false;
            this.dataGridOperationsResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridOperationsResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridOperationsResult.Location = new System.Drawing.Point(0, 0);
            this.dataGridOperationsResult.Name = "dataGridOperationsResult";
            this.dataGridOperationsResult.ReadOnly = true;
            this.dataGridOperationsResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridOperationsResult.Size = new System.Drawing.Size(605, 221);
            this.dataGridOperationsResult.TabIndex = 0;
            this.dataGridOperationsResult.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridOperationsResult_CellDoubleClick);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.dataGridScenariosResult);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(605, 221);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Scenarios";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // dataGridScenariosResult
            // 
            this.dataGridScenariosResult.AllowUserToAddRows = false;
            this.dataGridScenariosResult.AllowUserToDeleteRows = false;
            this.dataGridScenariosResult.AllowUserToOrderColumns = true;
            this.dataGridScenariosResult.AllowUserToResizeRows = false;
            this.dataGridScenariosResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridScenariosResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridScenariosResult.Location = new System.Drawing.Point(0, 0);
            this.dataGridScenariosResult.Name = "dataGridScenariosResult";
            this.dataGridScenariosResult.ReadOnly = true;
            this.dataGridScenariosResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridScenariosResult.Size = new System.Drawing.Size(605, 221);
            this.dataGridScenariosResult.TabIndex = 5;
            this.dataGridScenariosResult.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridScenariosResult_CellDoubleClick);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.dataGridCommandsResult);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(605, 221);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Commands";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // dataGridCommandsResult
            // 
            this.dataGridCommandsResult.AllowUserToAddRows = false;
            this.dataGridCommandsResult.AllowUserToDeleteRows = false;
            this.dataGridCommandsResult.AllowUserToOrderColumns = true;
            this.dataGridCommandsResult.AllowUserToResizeRows = false;
            this.dataGridCommandsResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridCommandsResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridCommandsResult.Location = new System.Drawing.Point(0, 0);
            this.dataGridCommandsResult.Name = "dataGridCommandsResult";
            this.dataGridCommandsResult.ReadOnly = true;
            this.dataGridCommandsResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridCommandsResult.Size = new System.Drawing.Size(605, 221);
            this.dataGridCommandsResult.TabIndex = 1;
            this.dataGridCommandsResult.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridCommandsResult_CellMouseDoubleClick);
            // 
            // ScenariosTextBox
            // 
            this.ScenariosTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ScenariosTextBox.Location = new System.Drawing.Point(77, 63);
            this.ScenariosTextBox.Name = "ScenariosTextBox";
            this.ScenariosTextBox.Size = new System.Drawing.Size(450, 20);
            this.ScenariosTextBox.TabIndex = 15;
            this.ScenariosTextBox.TextChanged += new System.EventHandler(this.ScenariosTextBox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Scenarios:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ScenariosButtonOpen
            // 
            this.ScenariosButtonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ScenariosButtonOpen.Location = new System.Drawing.Point(533, 61);
            this.ScenariosButtonOpen.Name = "ScenariosButtonOpen";
            this.ScenariosButtonOpen.Size = new System.Drawing.Size(75, 23);
            this.ScenariosButtonOpen.TabIndex = 19;
            this.ScenariosButtonOpen.Text = "Open";
            this.ScenariosButtonOpen.UseVisualStyleBackColor = true;
            this.ScenariosButtonOpen.Click += new System.EventHandler(this.ScenariosButtonOpen_Click);
            // 
            // CommnadsButtonOpen
            // 
            this.CommnadsButtonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CommnadsButtonOpen.Location = new System.Drawing.Point(533, 89);
            this.CommnadsButtonOpen.Name = "CommnadsButtonOpen";
            this.CommnadsButtonOpen.Size = new System.Drawing.Size(75, 23);
            this.CommnadsButtonOpen.TabIndex = 22;
            this.CommnadsButtonOpen.Text = "Open";
            this.CommnadsButtonOpen.UseVisualStyleBackColor = true;
            this.CommnadsButtonOpen.Click += new System.EventHandler(this.CommnadsButtonOpen_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "Commands:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // CommandsTextBox
            // 
            this.CommandsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommandsTextBox.Location = new System.Drawing.Point(77, 89);
            this.CommandsTextBox.Name = "CommandsTextBox";
            this.CommandsTextBox.Size = new System.Drawing.Size(450, 20);
            this.CommandsTextBox.TabIndex = 20;
            this.CommandsTextBox.TextChanged += new System.EventHandler(this.CommandsTextBox_TextChanged);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(3, 414);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(613, 23);
            this.progressBar.TabIndex = 23;
            this.progressBar.Visible = false;
            // 
            // SenariosStat
            // 
            this.SenariosStat.AutoSize = true;
            this.SenariosStat.Location = new System.Drawing.Point(334, 150);
            this.SenariosStat.Name = "SenariosStat";
            this.SenariosStat.Size = new System.Drawing.Size(57, 13);
            this.SenariosStat.TabIndex = 24;
            this.SenariosStat.Text = "Scenarios:";
            // 
            // CommandsStat
            // 
            this.CommandsStat.AutoSize = true;
            this.CommandsStat.Location = new System.Drawing.Point(428, 150);
            this.CommandsStat.Name = "CommandsStat";
            this.CommandsStat.Size = new System.Drawing.Size(62, 13);
            this.CommandsStat.TabIndex = 25;
            this.CommandsStat.Text = "Commands:";
            // 
            // ProcessFilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 438);
            this.Controls.Add(this.CommandsStat);
            this.Controls.Add(this.SenariosStat);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.CommnadsButtonOpen);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.CommandsTextBox);
            this.Controls.Add(this.ScenariosButtonOpen);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ScenariosTextBox);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.buttonFilter);
            this.Controls.Add(this.ProcessesComboBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.OperationComboBox);
            this.Controls.Add(this.NetSettComboBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.OperationButtonOpen);
            this.Controls.Add(this.ProcessesButtonOpen);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.OperationTextBox);
            this.Controls.Add(this.ProcessesTextBox);
            this.MinimumSize = new System.Drawing.Size(634, 400);
            this.Name = "ProcessFilterForm";
            this.Text = "Process Filter";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridProcessesResults)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridOperationsResult)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridScenariosResult)).EndInit();
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridCommandsResult)).EndInit();
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
        private System.Windows.Forms.DataGridView dataGridProcessesResults;
        private System.Windows.Forms.ComboBox OperationComboBox;
        private System.Windows.Forms.ComboBox NetSettComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox ProcessesComboBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button buttonFilter;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dataGridOperationsResult;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.DataGridView dataGridScenariosResult;
        private System.Windows.Forms.DataGridView dataGridCommandsResult;
        private System.Windows.Forms.TextBox ScenariosTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button ScenariosButtonOpen;
        private System.Windows.Forms.Button CommnadsButtonOpen;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox CommandsTextBox;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label SenariosStat;
        private System.Windows.Forms.Label CommandsStat;
    }
}

