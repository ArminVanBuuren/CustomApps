namespace LogsReader.Reader.Forms
{
	partial class ConfigureForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigureForm));
			this.editor = new Utils.WinForm.Notepad.Editor();
			this.ValidateOrOk = new System.Windows.Forms.Button();
			this.CancelButton = new System.Windows.Forms.Button();
			this.ReloadhButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// editor
			// 
			this.editor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.editor.AutoPrintToXml = false;
			this.editor.AutoScroll = true;
			this.editor.AutoScrollMinSize = new System.Drawing.Size(0, 14);
			this.editor.BackBrush = null;
			this.editor.ColoredOnlyVisible = false;
			this.editor.DefaultEncoding = ((System.Text.Encoding)(resources.GetObject("editor.DefaultEncoding")));
			this.editor.DelayedEventsInterval = 100;
			this.editor.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
			this.editor.HeaderName = null;
			this.editor.Highlights = false;
			this.editor.ImeMode = System.Windows.Forms.ImeMode.Off;
			this.editor.IsChanged = false;
			this.editor.IsReplaceMode = false;
			this.editor.Location = new System.Drawing.Point(3, 3);
			this.editor.Name = "editor";
			this.editor.ReadOnly = false;
			this.editor.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
			this.editor.Size = new System.Drawing.Size(1082, 575);
			this.editor.SizingGrip = true;
			this.editor.TabIndex = 1;
			this.editor.WordWrap = true;
			// 
			// ValidateOrOk
			// 
			this.ValidateOrOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ValidateOrOk.Image = global::LogsReader.Properties.Resources.finished;
			this.ValidateOrOk.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ValidateOrOk.Location = new System.Drawing.Point(881, 582);
			this.ValidateOrOk.Name = "ValidateOrOk";
			this.ValidateOrOk.Size = new System.Drawing.Size(87, 23);
			this.ValidateOrOk.TabIndex = 2;
			this.ValidateOrOk.Text = "ValidateOrOk";
			this.ValidateOrOk.UseVisualStyleBackColor = true;
			this.ValidateOrOk.Click += new System.EventHandler(this.ValidateOrOk_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Image = global::LogsReader.Properties.Resources.cancel;
			this.CancelButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.CancelButton.Location = new System.Drawing.Point(974, 582);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = new System.Drawing.Size(100, 23);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.Text = "CancelButton";
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// ReloadhButton
			// 
			this.ReloadhButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ReloadhButton.Image = global::LogsReader.Properties.Resources.reset2;
			this.ReloadhButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ReloadhButton.Location = new System.Drawing.Point(760, 582);
			this.ReloadhButton.Name = "ReloadhButton";
			this.ReloadhButton.Size = new System.Drawing.Size(115, 23);
			this.ReloadhButton.TabIndex = 4;
			this.ReloadhButton.Text = "ReloadhButton";
			this.ReloadhButton.UseVisualStyleBackColor = true;
			this.ReloadhButton.Click += new System.EventHandler(this.Reload_Click);
			// 
			// ConfigureForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1089, 614);
			this.Controls.Add(this.ReloadhButton);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.ValidateOrOk);
			this.Controls.Add(this.editor);
			this.Text = "Configure";
			this.ResumeLayout(false);

		}

		#endregion
		private Utils.WinForm.Notepad.Editor editor;
		private System.Windows.Forms.Button ValidateOrOk;
		private System.Windows.Forms.Button CancelButton;
		private System.Windows.Forms.Button ReloadhButton;
	}
}