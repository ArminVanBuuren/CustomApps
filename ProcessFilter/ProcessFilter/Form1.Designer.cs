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
            ((System.ComponentModel.ISupportInitialize)(this.dataGridProcessesResults)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridOperationsResult)).BeginInit();
            this.SuspendLayout();
            // 
            // ProcessesTextBox
            // 
            this.ProcessesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProcessesTextBox.Location = new System.Drawing.Point(77, 12);
            this.ProcessesTextBox.Name = "ProcessesTextBox";
            this.ProcessesTextBox.Size = new System.Drawing.Size(355, 20);
            this.ProcessesTextBox.TabIndex = 1;
            this.ProcessesTextBox.TextChanged += new System.EventHandler(this.ProcessesTextBox_TextChanged);
            // 
            // OperationTextBox
            // 
            this.OperationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OperationTextBox.Location = new System.Drawing.Point(77, 38);
            this.OperationTextBox.Name = "OperationTextBox";
            this.OperationTextBox.Size = new System.Drawing.Size(355, 20);
            this.OperationTextBox.TabIndex = 2;
            this.OperationTextBox.TextChanged += new System.EventHandler(this.OperationTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Processes";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Operations";
            // 
            // OperationButtonOpen
            // 
            this.OperationButtonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OperationButtonOpen.Location = new System.Drawing.Point(438, 36);
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
            this.ProcessesButtonOpen.Location = new System.Drawing.Point(438, 10);
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
            this.dataGridProcessesResults.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader;
            this.dataGridProcessesResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridProcessesResults.Location = new System.Drawing.Point(0, 0);
            this.dataGridProcessesResults.Name = "dataGridProcessesResults";
            this.dataGridProcessesResults.ReadOnly = true;
            this.dataGridProcessesResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridProcessesResults.Size = new System.Drawing.Size(510, 120);
            this.dataGridProcessesResults.TabIndex = 4;
            // 
            // OperationComboBox
            // 
            this.OperationComboBox.FormattingEnabled = true;
            this.OperationComboBox.Location = new System.Drawing.Point(77, 91);
            this.OperationComboBox.Name = "OperationComboBox";
            this.OperationComboBox.Size = new System.Drawing.Size(251, 21);
            this.OperationComboBox.TabIndex = 10;
            // 
            // NetSettComboBox
            // 
            this.NetSettComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NetSettComboBox.FormattingEnabled = true;
            this.NetSettComboBox.Location = new System.Drawing.Point(438, 64);
            this.NetSettComboBox.Name = "NetSettComboBox";
            this.NetSettComboBox.Size = new System.Drawing.Size(73, 21);
            this.NetSettComboBox.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(334, 68);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Сетевой элемент:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 94);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Операция:";
            // 
            // ProcessesComboBox
            // 
            this.ProcessesComboBox.FormattingEnabled = true;
            this.ProcessesComboBox.Location = new System.Drawing.Point(77, 64);
            this.ProcessesComboBox.Name = "ProcessesComboBox";
            this.ProcessesComboBox.Size = new System.Drawing.Size(251, 21);
            this.ProcessesComboBox.TabIndex = 12;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 67);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Процесс:";
            // 
            // buttonFilter
            // 
            this.buttonFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFilter.Location = new System.Drawing.Point(436, 92);
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
            this.tabControl1.Location = new System.Drawing.Point(3, 118);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(518, 146);
            this.tabControl1.TabIndex = 14;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dataGridProcessesResults);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(510, 120);
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
            this.tabPage2.Size = new System.Drawing.Size(510, 120);
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
            this.dataGridOperationsResult.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader;
            this.dataGridOperationsResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridOperationsResult.Location = new System.Drawing.Point(0, 0);
            this.dataGridOperationsResult.Name = "dataGridOperationsResult";
            this.dataGridOperationsResult.ReadOnly = true;
            this.dataGridOperationsResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridOperationsResult.Size = new System.Drawing.Size(510, 120);
            this.dataGridOperationsResult.TabIndex = 0;
            // 
            // ProcessFilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(523, 267);
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
            this.MinimumSize = new System.Drawing.Size(539, 306);
            this.Name = "ProcessFilterForm";
            this.Text = "Process Filter";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridProcessesResults)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridOperationsResult)).EndInit();
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
    }
}

