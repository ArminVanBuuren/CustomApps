using System.Drawing;
using System.Windows.Forms;
using Utils.WinForm.Notepad;

namespace XPathTester
{
    partial class XPathTesterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XPathTesterForm));
            this.XPathText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonFind = new System.Windows.Forms.Button();
            this.editor = new Utils.WinForm.Notepad.Editor();
            this.xpathResultDataGrid = new SPAMassageSaloon.Common.CustomDataGridView();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NodeType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NodeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonPrettyPrint = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.xpathResultDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // XPathText
            // 
            this.XPathText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.XPathText.Location = new System.Drawing.Point(42, 7);
            this.XPathText.Name = "XPathText";
            this.XPathText.Size = new System.Drawing.Size(1117, 20);
            this.XPathText.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "XPath:";
            // 
            // buttonFind
            // 
            this.buttonFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFind.Image = global::XPathTester.Properties.Resources.find;
            this.buttonFind.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonFind.Location = new System.Drawing.Point(1165, 7);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.buttonFind.Size = new System.Drawing.Size(76, 21);
            this.buttonFind.TabIndex = 2;
            this.buttonFind.Text = "Test [F5]";
            this.buttonFind.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.ButtonFind_Click);
            // 
            // editor
            // 
            this.editor.AutoScroll = true;
            this.editor.AutoScrollMinSize = new System.Drawing.Size(0, 14);
            this.editor.BackBrush = null;
            this.editor.ColoredOnlyVisible = false;
            this.editor.DelayedEventsInterval = 100;
            this.editor.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editor.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.editor.HeaderName = null;
            this.editor.Highlights = false;
            this.editor.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.editor.IsChanged = false;
            this.editor.IsReplaceMode = false;
            this.editor.Location = new System.Drawing.Point(0, 0);
            this.editor.Name = "editor";
            this.editor.ReadOnly = false;
            this.editor.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.editor.Size = new System.Drawing.Size(1061, 666);
            this.editor.SizingGrip = true;
            this.editor.TabIndex = 0;
            this.editor.WordWrap = true;
            // 
            // xpathResultDataGrid
            // 
            this.xpathResultDataGrid.AllowDrop = true;
            this.xpathResultDataGrid.AllowUserToAddRows = false;
            this.xpathResultDataGrid.AllowUserToDeleteRows = false;
            this.xpathResultDataGrid.AllowUserToOrderColumns = true;
            this.xpathResultDataGrid.AllowUserToResizeRows = false;
            this.xpathResultDataGrid.BackgroundColor = System.Drawing.Color.White;
            this.xpathResultDataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.xpathResultDataGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Sunken;
            this.xpathResultDataGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Sunken;
            this.xpathResultDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.xpathResultDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.NodeType,
            this.NodeName,
            this.Value});
            this.xpathResultDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xpathResultDataGrid.ImeMode = System.Windows.Forms.ImeMode.On;
            this.xpathResultDataGrid.Location = new System.Drawing.Point(0, 0);
            this.xpathResultDataGrid.Name = "xpathResultDataGrid";
            this.xpathResultDataGrid.ReadOnly = true;
            this.xpathResultDataGrid.RowHeadersVisible = false;
            this.xpathResultDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.xpathResultDataGrid.Size = new System.Drawing.Size(289, 666);
            this.xpathResultDataGrid.TabIndex = 3;
            // 
            // ID
            // 
            this.ID.DataPropertyName = "ID";
            this.ID.FillWeight = 8.418125F;
            this.ID.HeaderText = "ID";
            this.ID.MinimumWidth = 25;
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            this.ID.Width = 30;
            // 
            // NodeType
            // 
            this.NodeType.DataPropertyName = "NodeType";
            this.NodeType.FillWeight = 12.70355F;
            this.NodeType.HeaderText = "NodeType";
            this.NodeType.MinimumWidth = 65;
            this.NodeType.Name = "NodeType";
            this.NodeType.ReadOnly = true;
            this.NodeType.Width = 75;
            // 
            // NodeName
            // 
            this.NodeName.DataPropertyName = "Name";
            this.NodeName.FillWeight = 9.335174F;
            this.NodeName.HeaderText = "Name";
            this.NodeName.MinimumWidth = 65;
            this.NodeName.Name = "Name";
            this.NodeName.ReadOnly = true;
            this.NodeName.Width = 75;
            // 
            // Value
            // 
            this.Value.DataPropertyName = "Value";
            this.Value.FillWeight = 369.5432F;
            this.Value.HeaderText = "Value";
            this.Value.MinimumWidth = 350;
            this.Value.Name = "Value";
            this.Value.ReadOnly = true;
            this.Value.Width = 1000;
            // 
            // buttonPrettyPrint
            // 
            this.buttonPrettyPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPrettyPrint.Image = global::XPathTester.Properties.Resources.xml8;
            this.buttonPrettyPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonPrettyPrint.Location = new System.Drawing.Point(1247, 7);
            this.buttonPrettyPrint.Name = "buttonPrettyPrint";
            this.buttonPrettyPrint.Size = new System.Drawing.Size(110, 21);
            this.buttonPrettyPrint.TabIndex = 7;
            this.buttonPrettyPrint.Text = "Pretty Print [F6]";
            this.buttonPrettyPrint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonPrettyPrint.UseVisualStyleBackColor = true;
            this.buttonPrettyPrint.Click += new System.EventHandler(this.ButtonPrettyPrint_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 34);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.xpathResultDataGrid);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.editor);
            this.splitContainer1.Panel2MinSize = 41;
            this.splitContainer1.Size = new System.Drawing.Size(1362, 670);
            this.splitContainer1.SplitterDistance = 293;
            this.splitContainer1.TabIndex = 9;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.XPathText);
            this.panel1.Controls.Add(this.buttonPrettyPrint);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.buttonFind);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.MinimumSize = new System.Drawing.Size(250, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1362, 34);
            this.panel1.TabIndex = 11;
            // 
            // XPathTesterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1362, 704);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "XPathTesterForm";
            this.Text = "XPath Tester";
            ((System.ComponentModel.ISupportInitialize)(this.xpathResultDataGrid)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion


        private System.Windows.Forms.TextBox XPathText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonFind;
        private Editor editor;
        private SPAMassageSaloon.Common.CustomDataGridView xpathResultDataGrid;
        private System.Windows.Forms.Button buttonPrettyPrint;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private Panel panel1;
        private DataGridViewTextBoxColumn ID;
        private DataGridViewTextBoxColumn NodeType;
        private DataGridViewTextBoxColumn NodeName;
        private DataGridViewTextBoxColumn Value;
    }
}