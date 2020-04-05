namespace Utils.WinForm.Notepad
{
    partial class NotepadControl
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
            this.TabControlObj = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // TabControlObj
            // 
            this.TabControlObj.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.TabControlObj.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabControlObj.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.TabControlObj.Location = new System.Drawing.Point(0, 0);
            this.TabControlObj.Name = "TabControlObj";
            this.TabControlObj.SelectedIndex = 0;
            this.TabControlObj.Size = new System.Drawing.Size(150, 150);
            this.TabControlObj.TabIndex = 0;
            // 
            // NotepadControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.Controls.Add(this.TabControlObj);
            this.Name = "NotepadControl";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl TabControlObj;
    }
}