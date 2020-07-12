using System.Windows.Forms;

namespace LogsReader.Reader
{
	partial class TraceItemView
	{
		/// <summary> 
		/// Обязательная переменная конструктора.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Освободить все используемые ресурсы.
		/// </summary>
		/// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Код, автоматически созданный конструктором компонентов

		/// <summary> 
		/// Требуемый метод для поддержки конструктора — не изменяйте 
		/// содержимое этого метода с помощью редактора кода.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TraceItemView));
			this.notepad = new Utils.WinForm.Notepad.NotepadControl();
			this.descriptionText = new System.Windows.Forms.RichTextBox();
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// notepad
			// 
			this.notepad.AllowUserCloseItems = false;
			this.notepad.DefaultEncoding = ((System.Text.Encoding)(resources.GetObject("notepad.DefaultEncoding")));
			this.notepad.Dock = System.Windows.Forms.DockStyle.Fill;
			this.notepad.Highlights = false;
			this.notepad.Location = new System.Drawing.Point(0, 0);
			this.notepad.Name = "notepad";
			this.notepad.ReadOnly = true;
			this.notepad.SelectedIndex = -1;
			this.notepad.Size = new System.Drawing.Size(372, 316);
			this.notepad.SizingGrip = false;
			this.notepad.TabIndex = 0;
			this.notepad.TabsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.notepad.TabsForeColor = System.Drawing.Color.Green;
			this.notepad.TextFont = new System.Drawing.Font("Segoe UI", 10F);
			this.notepad.TextForeColor = System.Drawing.Color.Black;
			this.notepad.WordWrap = true;
			// 
			// descriptionText
			// 
			this.descriptionText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.descriptionText.Location = new System.Drawing.Point(0, 0);
			this.descriptionText.Name = "descriptionText";
			this.descriptionText.ReadOnly = true;
			this.descriptionText.Size = new System.Drawing.Size(372, 57);
			this.descriptionText.TabIndex = 1;
			this.descriptionText.Text = "";
			// 
			// splitContainer
			// 
			this.splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer.Location = new System.Drawing.Point(0, 0);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.descriptionText);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.notepad);
			this.splitContainer.Size = new System.Drawing.Size(376, 385);
			this.splitContainer.SplitterDistance = 61;
			this.splitContainer.TabIndex = 2;
			this.splitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer_SplitterMoved);
			// 
			// TraceItemView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer);
			this.Name = "TraceItemView";
			this.Size = new System.Drawing.Size(376, 385);
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Utils.WinForm.Notepad.NotepadControl notepad;
		private System.Windows.Forms.RichTextBox descriptionText;
		private System.Windows.Forms.SplitContainer splitContainer;
	}
}
