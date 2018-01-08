namespace Script.DataGridViewCustom
{
    internal partial class TextButton
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
            this.InnerTextBox = new System.Windows.Forms.TextBox();
            this.button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // InnerTextBox
            // 
            this.InnerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.InnerTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.InnerTextBox.Location = new System.Drawing.Point(3, 2);
            this.InnerTextBox.Name = "InnerTextBox";
            this.InnerTextBox.Size = new System.Drawing.Size(100, 13);
            this.InnerTextBox.TabIndex = 0;
            // 
            // button
            // 
            this.button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button.Location = new System.Drawing.Point(100, 0);
            this.button.Margin = new System.Windows.Forms.Padding(0);
            this.button.MaximumSize = new System.Drawing.Size(1000, 20);
            this.button.MinimumSize = new System.Drawing.Size(0, 18);
            this.button.Name = "button";
            this.button.Size = new System.Drawing.Size(22, 18);
            this.button.TabIndex = 1;
            this.button.Text = "∙∙∙";
            this.button.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button.UseVisualStyleBackColor = true;
            this.button.Click += new System.EventHandler(this.button_Click);
            // 
            // TextButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button);
            this.Controls.Add(this.InnerTextBox);
            this.Name = "TextButton";
            this.Size = new System.Drawing.Size(122, 20);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button;
        protected System.Windows.Forms.TextBox InnerTextBox;
    }
}
