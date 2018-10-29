namespace Utils.WinForm.Notepad
{
    partial class XmlNotepad
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XmlNotepad));
            this.TabControlObj = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // TabControlObj
            // 
            this.TabControlObj.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TabControlObj.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.TabControlObj.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.TabControlObj.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.TabControlObj.Location = new System.Drawing.Point(0, 0);
            this.TabControlObj.Name = "TabControlObj";
            this.TabControlObj.SelectedIndex = 0;
            this.TabControlObj.Size = new System.Drawing.Size(1035, 645);
            this.TabControlObj.TabIndex = 0;
            // 
            // XmlNotepad
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1035, 648);
            this.Controls.Add(this.TabControlObj);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "XmlNotepad";
            this.Text = "Xml Editor";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl TabControlObj;
    }
}