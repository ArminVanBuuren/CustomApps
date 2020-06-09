using System.Windows.Forms;

namespace Utils.WinForm.Expander
{
    partial class ExpandCollapsePanel
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
	        this.components = new System.ComponentModel.Container();
	        this._btnExpandCollapse = new ExpandCollapseButton();
	        this.animationTimer = new System.Windows.Forms.Timer(this.components);
	        this.panel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panel
            // 
            this.panel.Controls.Add(this._btnExpandCollapse);
            this.panel.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.TabIndex = 0;
			// 
			// _btnExpandCollapse
			// 
			this._btnExpandCollapse.Anchor = ((System.Windows.Forms.AnchorStyles)
		        (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
		          | System.Windows.Forms.AnchorStyles.Right)));
	        this._btnExpandCollapse.ButtonSize = ExpandButtonSize.Normal;
	        this._btnExpandCollapse.ButtonStyle = ExpandButtonStyle.Circle;
	        this._btnExpandCollapse.IsExpanded = false;
	        this._btnExpandCollapse.Location = new System.Drawing.Point(3, 3);
	        this._btnExpandCollapse.Name = "_btnExpandCollapse";
	        this._btnExpandCollapse.TabIndex = 1;
	        // 
	        // animationTimer
	        // 
	        this.animationTimer.Interval = 50;
	        this.animationTimer.Tick += new System.EventHandler(this.animationTimer_Tick);
	        // 
	        // ExpandCollapsePanel
	        // 
	        this.Controls.Add(this.panel);
	        this.Size = new System.Drawing.Size(365, 319);
	        this.ResumeLayout(false);

        }

        #endregion

        private Panel panel;
        private ExpandCollapseButton _btnExpandCollapse;
        private System.Windows.Forms.Timer animationTimer;
    }
}
