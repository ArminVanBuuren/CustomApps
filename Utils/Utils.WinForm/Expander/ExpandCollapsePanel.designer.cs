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
	        this.panelHeader = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panel
            // 
            this.panelHeader.Controls.Add(this._btnExpandCollapse);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panel";
            this.panelHeader.TabIndex = 0;
			// 
			// _btnExpandCollapse
			// 
			this._btnExpandCollapse.ButtonSize = ExpandButtonSize.Small;
	        this._btnExpandCollapse.ButtonStyle = ExpandButtonStyle.Circle;
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
	        this.Controls.Add(this.panelHeader);
	        this.Size = new System.Drawing.Size(365, 319);
	        this.ResumeLayout(false);

        }

        #endregion

        private Panel panelHeader;
        private ExpandCollapseButton _btnExpandCollapse;
        private System.Windows.Forms.Timer animationTimer;
    }
}
