

namespace XPathEvaluator
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.XPathText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonFind = new System.Windows.Forms.Button();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabXmlBody = new System.Windows.Forms.TabPage();
            this.fctb = new FastColoredTextBoxNS.FastColoredTextBox();
            this.tabXPathResult = new System.Windows.Forms.TabPage();
            this.exceptionMessage = new System.Windows.Forms.Label();
            this.xpathResultDataGrid = new System.Windows.Forms.DataGridView();
            this.exceptionLabel = new System.Windows.Forms.Label();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonPrettyPrint = new System.Windows.Forms.Button();
            this.tabMain.SuspendLayout();
            this.tabXmlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fctb)).BeginInit();
            this.tabXPathResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xpathResultDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // XPathText
            // 
            this.XPathText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.XPathText.Location = new System.Drawing.Point(99, 12);
            this.XPathText.Name = "XPathText";
            this.XPathText.Size = new System.Drawing.Size(1052, 20);
            this.XPathText.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "XPath Expression:";
            // 
            // buttonFind
            // 
            this.buttonFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFind.Location = new System.Drawing.Point(1157, 11);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = new System.Drawing.Size(75, 21);
            this.buttonFind.TabIndex = 2;
            this.buttonFind.Text = "Find";
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.buttonFind_Click);
            // 
            // tabMain
            // 
            this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabMain.Controls.Add(this.tabXmlBody);
            this.tabMain.Controls.Add(this.tabXPathResult);
            this.tabMain.Location = new System.Drawing.Point(0, 38);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(1325, 666);
            this.tabMain.TabIndex = 6;
            // 
            // tabXmlBody
            // 
            this.tabXmlBody.Controls.Add(this.fctb);
            this.tabXmlBody.Location = new System.Drawing.Point(4, 22);
            this.tabXmlBody.Name = "tabXmlBody";
            this.tabXmlBody.Padding = new System.Windows.Forms.Padding(3);
            this.tabXmlBody.Size = new System.Drawing.Size(1317, 640);
            this.tabXmlBody.TabIndex = 0;
            this.tabXmlBody.Text = "XmlBody";
            this.tabXmlBody.UseVisualStyleBackColor = true;
            // 
            // fctb
            // 
            this.fctb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fctb.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.fctb.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);";
            this.fctb.AutoScrollMinSize = new System.Drawing.Size(0, 14);
            this.fctb.BackBrush = null;
            this.fctb.CharHeight = 14;
            this.fctb.CharWidth = 8;
            this.fctb.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.fctb.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.fctb.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.fctb.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.fctb.IsReplaceMode = false;
            this.fctb.Location = new System.Drawing.Point(-4, 0);
            this.fctb.Name = "fctb";
            this.fctb.Paddings = new System.Windows.Forms.Padding(0);
            this.fctb.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.fctb.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("fctb.ServiceColors")));
            this.fctb.Size = new System.Drawing.Size(1321, 640);
            this.fctb.TabIndex = 0;
            this.fctb.WordWrap = true;
            this.fctb.Zoom = 100;
            // 
            // tabXPathResult
            // 
            this.tabXPathResult.Controls.Add(this.exceptionMessage);
            this.tabXPathResult.Controls.Add(this.xpathResultDataGrid);
            this.tabXPathResult.Controls.Add(this.exceptionLabel);
            this.tabXPathResult.Location = new System.Drawing.Point(4, 22);
            this.tabXPathResult.Name = "tabXPathResult";
            this.tabXPathResult.Padding = new System.Windows.Forms.Padding(3);
            this.tabXPathResult.Size = new System.Drawing.Size(1317, 640);
            this.tabXPathResult.TabIndex = 1;
            this.tabXPathResult.Text = "XPath Result";
            this.tabXPathResult.UseVisualStyleBackColor = true;
            // 
            // exceptionMessage
            // 
            this.exceptionMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.exceptionMessage.AutoSize = true;
            this.exceptionMessage.ForeColor = System.Drawing.Color.Red;
            this.exceptionMessage.Location = new System.Drawing.Point(60, 622);
            this.exceptionMessage.Name = "exceptionMessage";
            this.exceptionMessage.Size = new System.Drawing.Size(0, 13);
            this.exceptionMessage.TabIndex = 4;
            // 
            // xpathResultDataGrid
            // 
            this.xpathResultDataGrid.AllowDrop = true;
            this.xpathResultDataGrid.AllowUserToAddRows = false;
            this.xpathResultDataGrid.AllowUserToDeleteRows = false;
            this.xpathResultDataGrid.AllowUserToOrderColumns = true;
            this.xpathResultDataGrid.AllowUserToResizeRows = false;
            this.xpathResultDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xpathResultDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.xpathResultDataGrid.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.xpathResultDataGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.xpathResultDataGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Sunken;
            this.xpathResultDataGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Sunken;
            this.xpathResultDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.xpathResultDataGrid.ImeMode = System.Windows.Forms.ImeMode.On;
            this.xpathResultDataGrid.Location = new System.Drawing.Point(3, 3);
            this.xpathResultDataGrid.Name = "xpathResultDataGrid";
            this.xpathResultDataGrid.ReadOnly = true;
            this.xpathResultDataGrid.RowHeadersVisible = false;
            this.xpathResultDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.xpathResultDataGrid.Size = new System.Drawing.Size(1311, 616);
            this.xpathResultDataGrid.TabIndex = 3;
            // 
            // exceptionLabel
            // 
            this.exceptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.exceptionLabel.AutoSize = true;
            this.exceptionLabel.Location = new System.Drawing.Point(4, 622);
            this.exceptionLabel.Name = "exceptionLabel";
            this.exceptionLabel.Size = new System.Drawing.Size(57, 13);
            this.exceptionLabel.TabIndex = 1;
            this.exceptionLabel.Text = "Exception:";
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Column1";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Column2";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // buttonPrettyPrint
            // 
            this.buttonPrettyPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPrettyPrint.Location = new System.Drawing.Point(1238, 11);
            this.buttonPrettyPrint.Name = "buttonPrettyPrint";
            this.buttonPrettyPrint.Size = new System.Drawing.Size(75, 21);
            this.buttonPrettyPrint.TabIndex = 7;
            this.buttonPrettyPrint.Text = "XML Print";
            this.buttonPrettyPrint.UseVisualStyleBackColor = true;
            this.buttonPrettyPrint.Click += new System.EventHandler(this.buttonPrettyPrint_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1325, 704);
            this.Controls.Add(this.buttonPrettyPrint);
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.buttonFind);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.XPathText);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(257, 135);
            this.Name = "MainWindow";
            this.Text = "XPath Tester";
            this.tabMain.ResumeLayout(false);
            this.tabXmlBody.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fctb)).EndInit();
            this.tabXPathResult.ResumeLayout(false);
            this.tabXPathResult.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xpathResultDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


        private System.Windows.Forms.TextBox XPathText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonFind;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabXmlBody;
        private System.Windows.Forms.TabPage tabXPathResult;
        private FastColoredTextBoxNS.FastColoredTextBox fctb;
        private System.Windows.Forms.Label exceptionLabel;
        private System.Windows.Forms.DataGridView xpathResultDataGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.Label exceptionMessage;
        private System.Windows.Forms.Button buttonPrettyPrint;
    }
}