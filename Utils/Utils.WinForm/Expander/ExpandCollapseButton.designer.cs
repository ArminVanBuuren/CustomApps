using System.Drawing;
using System.Windows.Forms;

namespace Utils.WinForm.Expander
{
    partial class ExpandCollapseButton
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.lblLine = new System.Windows.Forms.Label();
			this.lblHeader = new System.Windows.Forms.Label();
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.checkBox = new System.Windows.Forms.CheckBox();
			this.panel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblLine
			// 
			this.lblLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblLine.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblLine.Location = new System.Drawing.Point(41, 28);
			this.lblLine.Name = "lblLine";
			this.lblLine.Size = new System.Drawing.Size(874, 1);
			this.lblLine.TabIndex = 0;
			this.lblLine.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
			// 
			// lblHeader
			// 
			this.lblHeader.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblHeader.AutoSize = true;
			this.lblHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.lblHeader.ForeColor = System.Drawing.Color.Teal;
			this.lblHeader.Location = new System.Drawing.Point(42, 4);
			this.lblHeader.Name = "lblHeader";
			this.lblHeader.Size = new System.Drawing.Size(68, 15);
			this.lblHeader.TabIndex = 2;
			this.lblHeader.Text = "Заголовок";
			this.lblHeader.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
			// 
			// pictureBox
			// 
			this.pictureBox.Image = global::Utils.WinForm.Properties.Resources.expander_downarrow;
			this.pictureBox.Location = new System.Drawing.Point(3, 0);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(35, 35);
			this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox.TabIndex = 1;
			this.pictureBox.TabStop = false;
			this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
			// 
			// checkBox
			// 
			this.checkBox.AutoSize = true;
			this.checkBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.checkBox.Location = new System.Drawing.Point(-28, 0);
			this.checkBox.Name = "checkBox";
			this.checkBox.Padding = new System.Windows.Forms.Padding(8, 0, 5, 0);
			this.checkBox.Size = new System.Drawing.Size(28, 40);
			this.checkBox.TabIndex = 3;
			this.checkBox.UseVisualStyleBackColor = false;
			// 
			// panel
			// 
			this.panel.Controls.Add(this.lblHeader);
			this.panel.Controls.Add(this.pictureBox);
			this.panel.Controls.Add(this.lblLine);
			this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel.Location = new System.Drawing.Point(0, 0);
			this.panel.Name = "panel";
			this.panel.Size = new System.Drawing.Size(0, 40);
			this.panel.TabIndex = 4;
			this.panel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
			// 
			// ExpandCollapseButton
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.checkBox);
			this.Controls.Add(this.panel);
			this.MaximumSize = new System.Drawing.Size(0, 40);
			this.Name = "ExpandCollapseButton";
			this.Size = new System.Drawing.Size(0, 40);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.panel.ResumeLayout(false);
			this.panel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblLine;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label lblHeader;
		private System.Windows.Forms.CheckBox checkBox;
		private Panel panel;
	}
}
