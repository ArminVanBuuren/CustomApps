using LogsReader.Properties;

namespace LogsReader.Reader.Forms
{
	partial class TraceNameFilterForm
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.DgvTraceNames = new System.Windows.Forms.DataGridView();
			this.SelectColumn = new Utils.WinForm.DataGridViewHelper.DgvCheckBoxColumn();
			this.TraceNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CountMatchesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CountErrorsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.DgvTraceNames)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Image = global::LogsReader.Properties.Resources.cancel;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(399, 521);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(80, 25);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = global::LogsReader.Properties.Resources.Txt_Forms_Cancel;
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Image = global::LogsReader.Properties.Resources.Ok;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(333, 521);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(60, 25);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.Text = global::LogsReader.Properties.Resources.Txt_Forms_OK;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// DgvTraceNames
			// 
			this.DgvTraceNames.AllowUserToAddRows = false;
			this.DgvTraceNames.AllowUserToDeleteRows = false;
			this.DgvTraceNames.AllowUserToResizeRows = false;
			this.DgvTraceNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DgvTraceNames.BackgroundColor = System.Drawing.SystemColors.Menu;
			this.DgvTraceNames.BorderStyle = System.Windows.Forms.BorderStyle.None;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.DgvTraceNames.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.DgvTraceNames.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.DgvTraceNames.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SelectColumn,
            this.TraceNameColumn,
            this.CountMatchesColumn,
            this.CountErrorsColumn});
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.DgvTraceNames.DefaultCellStyle = dataGridViewCellStyle2;
			this.DgvTraceNames.GridColor = System.Drawing.SystemColors.ControlLight;
			this.DgvTraceNames.Location = new System.Drawing.Point(5, 3);
			this.DgvTraceNames.MultiSelect = false;
			this.DgvTraceNames.Name = "DgvTraceNames";
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.DgvTraceNames.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.DgvTraceNames.RowHeadersVisible = false;
			this.DgvTraceNames.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.DgvTraceNames.RowTemplate.Height = 18;
			this.DgvTraceNames.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.DgvTraceNames.Size = new System.Drawing.Size(474, 516);
			this.DgvTraceNames.TabIndex = 6;
			// 
			// SelectColumn
			// 
			this.SelectColumn.Checked = false;
			this.SelectColumn.FalseValue = false;
			this.SelectColumn.HeaderText = "";
			this.SelectColumn.MinimumWidth = 25;
			this.SelectColumn.Name = "SelectColumn";
			this.SelectColumn.ReadOnly = false;
			this.SelectColumn.TrueValue = true;
			this.SelectColumn.Width = 25;
			// 
			// TraceNameColumn
			// 
			this.TraceNameColumn.DataPropertyName = "TraceName";
			this.TraceNameColumn.HeaderText = "TraceName";
			this.TraceNameColumn.MinimumWidth = 80;
			this.TraceNameColumn.Name = "TraceNameColumn";
			this.TraceNameColumn.ReadOnly = true;
			this.TraceNameColumn.Width = 80;
			// 
			// CountMatchesColumn
			// 
			this.CountMatchesColumn.DataPropertyName = "CountMatches";
			this.CountMatchesColumn.HeaderText = "Matches";
			this.CountMatchesColumn.MinimumWidth = 55;
			this.CountMatchesColumn.Name = "CountMatchesColumn";
			this.CountMatchesColumn.ReadOnly = true;
			this.CountMatchesColumn.Width = 55;
			// 
			// CountErrorsColumn
			// 
			this.CountErrorsColumn.DataPropertyName = "CountErrors";
			this.CountErrorsColumn.HeaderText = "Errors";
			this.CountErrorsColumn.MinimumWidth = 55;
			this.CountErrorsColumn.Name = "CountErrorsColumn";
			this.CountErrorsColumn.ReadOnly = true;
			this.CountErrorsColumn.Width = 55;
			// 
			// TraceNameFilterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(484, 548);
			this.Controls.Add(this.DgvTraceNames);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.MaximumSize = new System.Drawing.Size(700, 620);
			this.MinimumSize = new System.Drawing.Size(171, 93);
			this.Name = "TraceNameFilterForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Trace Filter";
			((System.ComponentModel.ISupportInitialize)(this.DgvTraceNames)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.DataGridView DgvTraceNames;
		private Utils.WinForm.DataGridViewHelper.DgvCheckBoxColumn SelectColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn TraceNameColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn CountMatchesColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn CountErrorsColumn;
	}
}