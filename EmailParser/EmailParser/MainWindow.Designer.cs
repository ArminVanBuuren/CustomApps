namespace EmailParser
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));




            this.textBoxPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.OpenButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.RunButton = new System.Windows.Forms.Button();
            this.StatusLable = new System.Windows.Forms.Label();
            this.radioButtonFile = new System.Windows.Forms.RadioButton();
            this.radioButtonFolder = new System.Windows.Forms.RadioButton();
            this.StatLable = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxPath
            // 
            this.textBoxPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPath.Location = new System.Drawing.Point(16, 38);
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.Size = new System.Drawing.Size(275, 20);
            this.textBoxPath.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = Properties.Resources.PATH_TEXT;
            // 
            // OpenButton
            // 
            this.OpenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenButton.Location = new System.Drawing.Point(297, 36);
            this.OpenButton.Name = "OpenButton";
            this.OpenButton.Size = new System.Drawing.Size(75, 23);
            this.OpenButton.TabIndex = 2;
            this.OpenButton.Text = Properties.Resources.OPEN_TEXT;
            this.OpenButton.UseVisualStyleBackColor = true;
            this.OpenButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = Properties.Resources.PROGRESS_TEXT;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 109);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = Properties.Resources.STATUS_TEXT;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(15, 77);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(275, 23);
            this.progressBar.TabIndex = 5;
            // 
            // RunButton
            // 
            this.RunButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RunButton.Location = new System.Drawing.Point(297, 77);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(75, 23);
            this.RunButton.TabIndex = 6;
            this.RunButton.Text = Properties.Resources.TEXT_START;
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // StatusLable
            // 
            this.StatusLable.AutoSize = true;
            this.StatusLable.Location = new System.Drawing.Point(63, 103);
            this.StatusLable.Name = "StatusLable";
            this.StatusLable.Size = new System.Drawing.Size(0, 13);
            this.StatusLable.TabIndex = 7;
            // 
            // radioButtonFile
            // 
            this.radioButtonFile.AutoSize = true;
            this.radioButtonFile.Location = new System.Drawing.Point(59, 17);
            this.radioButtonFile.Name = "radioButtonFile";
            this.radioButtonFile.Size = new System.Drawing.Size(59, 17);
            this.radioButtonFile.TabIndex = 8;
            this.radioButtonFile.Text = Properties.Resources.SEARCH_IN_FILE;
            this.radioButtonFile.UseVisualStyleBackColor = true;
            // 
            // radioButtonFolder
            // 
            this.radioButtonFolder.AutoSize = true;
            this.radioButtonFolder.Checked = true;
            this.radioButtonFolder.Location = new System.Drawing.Point(124, 17);
            this.radioButtonFolder.Name = "radioButtonFolder";
            this.radioButtonFolder.Size = new System.Drawing.Size(57, 17);
            this.radioButtonFolder.TabIndex = 9;
            this.radioButtonFolder.TabStop = true;
            this.radioButtonFolder.Text = Properties.Resources.SEARCH_IN_FOLDER;
            this.radioButtonFolder.UseVisualStyleBackColor = true;
            // 
            // StatLable
            // 
            this.StatLable.AutoSize = true;
            this.StatLable.Location = new System.Drawing.Point(53, 109);
            this.StatLable.Name = "StatLable";
            this.StatLable.Size = new System.Drawing.Size(19, 13);
            this.StatLable.TabIndex = 10;
            this.StatLable.Text = "    ";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 131);
            this.Controls.Add(this.StatLable);
            this.Controls.Add(this.radioButtonFolder);
            this.Controls.Add(this.radioButtonFile);
            this.Controls.Add(this.StatusLable);
            this.Controls.Add(this.RunButton);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.OpenButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxPath);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(600, 170);
            this.MinimumSize = new System.Drawing.Size(300, 170);
            this.Name = "MainWindow";
            this.Text = Properties.Resources.PROGR_NAME;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


        private System.Windows.Forms.TextBox textBoxPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OpenButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button RunButton;
        private System.Windows.Forms.Label StatusLable;
        private System.Windows.Forms.RadioButton radioButtonFile;
        private System.Windows.Forms.RadioButton radioButtonFolder;
        private System.Windows.Forms.Label StatLable;
    }
}

