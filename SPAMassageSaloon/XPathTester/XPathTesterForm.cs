using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using FastColoredTextBoxNS;
using SPAMassageSaloon.Common;
using Utils;
using Utils.WinForm.DataGridViewHelper;
using XPathTester.Properties;

namespace XPathTester
{
    public partial class XPathTesterForm : Form, ISaloonForm
    {
        private readonly object sync = new object();

        private readonly ToolStripLabel _statusInfo;

        private readonly Regex _checkXmlNS = new Regex("xmlns\\s*=\\s*\"[^\"]+\"",
	        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Compiled);

        private bool _isPasting = false;

        public XPathCollection Result { get; private set; }

        public int ActiveProcessesCount => 0;

        public int ActiveTotalProgress => 0;

        public XPathTesterForm()
        {
            InitializeComponent();

            try
            {
                base.Text = $"{base.Text} {this.GetAssemblyInfo().Version}";

                xpathResultDataGrid.AutoGenerateColumns = false;
                xpathResultDataGrid.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
                xpathResultDataGrid.MouseClick += XpathResultDataGrid_SelectionChanged;
                xpathResultDataGrid.SelectionChanged += XpathResultDataGrid_SelectionChanged;
                xpathResultDataGrid.CellFormatting += (sender, e) =>
                {
	                if (e.RowIndex < 0 || e.ColumnIndex < 0)
		                return;

                    var row = ((DataGridView)sender).Rows[e.RowIndex];
                    row.DefaultCellStyle.BackColor = row.Index.IsParity() ? Color.White : Color.FromArgb(245, 245, 245);
                };

                KeyPreview = true;
                KeyDown += XPathWindow_KeyDown;

                editor.SizingGrip = false;
                editor.SetLanguages(new[] { Language.XML, Language.HTML }, Language.XML);
                _statusInfo = editor.AddToolStripLabel();
                _statusInfo.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                editor.TextChanged += EditorOnTextChanged;
                editor.Pasting += EditorOnPasting;

                ApplySettings();
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message);
            }
            finally
            {
                CenterToScreen();
            }
        }

        private void XPathWindow_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.F5:
                        ButtonFind_Click(this, EventArgs.Empty);
                        e.SuppressKeyPress = true;  // Stops other controls on the form receiving event.
                        break;
                    case Keys.F6:
                        ButtonPrettyPrint_Click(this, EventArgs.Empty);
                        e.SuppressKeyPress = true; // все эвенты KeyDown дочерних элементов отменяются
                        break;
                    case Keys.C when (ModifierKeys & Keys.Control) != 0 && xpathResultDataGrid.SelectedRows.Count > 0:
                        var results = new List<DGVXPathResult>();
                        foreach (DataGridViewRow row in xpathResultDataGrid.SelectedRows)
                        {
                            if (row.Cells[0]?.Value is int id)
                                results.Insert(0, Result[id]);
                        }

                        var clipboardText = new StringBuilder(results.Count);
                        foreach (var result in results)
                        {
                            clipboardText.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\r\n",
                                result.ID,
                                result.NodeType,
                                result.Name,
                                result.Value,
                                result.Navigator.Node?.OuterXml);
                        }

                        Clipboard.SetText(clipboardText.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message);
            }
        }

        private void XpathResultDataGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (xpathResultDataGrid.CurrentRow == null 
                || xpathResultDataGrid.SelectedRows.Count == 0 
                || Result == null 
                || !(xpathResultDataGrid.CurrentRow?.Cells[0].Value is int id))
                return;

            try
            {
                editor.TextChanged -= EditorOnTextChanged;
                xpathResultDataGrid.MouseClick -= XpathResultDataGrid_SelectionChanged;
                xpathResultDataGrid.SelectionChanged -= XpathResultDataGrid_SelectionChanged;

                var navigator = Result[id].Navigator;
                var range = editor.GetRange(navigator.Start, navigator.End);

                editor.Selection = range;
                editor.DoSelectionVisible();
                editor.Invalidate();
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message);
            }
            finally
            {
                editor.TextChanged += EditorOnTextChanged;
                xpathResultDataGrid.MouseClick += XpathResultDataGrid_SelectionChanged;
                xpathResultDataGrid.SelectionChanged += XpathResultDataGrid_SelectionChanged;
            }
        }

        private void EditorOnPasting(object sender, EventArgs e)
        {
            _isPasting = true;
        }

        void EditorOnTextChanged(object sender, EventArgs eventArgs)
        {
            lock (sync)
            {
                if (editor.Text.IsNullOrEmpty())
                {
	                ReportStatus(string.Empty);
                    return;
                }

                try
                {
                    ClearResultTap();

                    var trimSource = editor.Text.Trim();
                    if (!trimSource.StartsWith("<") || !trimSource.EndsWith(">"))
                    {
                        ReportStatus(string.Format(Resources.XmlIsIncorrect, string.Empty));
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ReportStatus(ex.Message);
                }

                try
                {
	                if (!_isPasting) 
		                return;

	                var source = XML.RemoveUnallowable(editor.Text, " ");
	                source = _checkXmlNS.Replace(source, string.Empty);
	                try
	                {
		                editor.TextChanged -= EditorOnTextChanged;
		                var prevSelection = editor.Selection.Clone();
		                editor.ChangeTextWithoutCommit(source);
		                editor.Selection = prevSelection;
	                }
	                catch (Exception)
	                {
		                // ignored
	                }
	                finally
	                {
		                editor.TextChanged += EditorOnTextChanged;
		                _isPasting = false;
	                }
                }
                catch (Exception)
                {
	                // ignored
                }
            }
        }

        private void ButtonPrettyPrint_Click(object sender, EventArgs e)
        {
	        try
	        {
		        var document = new XmlDocument();
		        document.LoadXml(editor.Text);
		        editor.ChangeTextWithoutCommit(document.PrintXml());
            }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message);
	        }
        }

        async void ButtonFind_Click(object sender, EventArgs e)
        {
	        if (string.IsNullOrEmpty(XPathText.Text))
            {
                ReportStatus(Resources.XPathIsEmpty);
                return;
            }
	        ClearResultTap();

	        try
	        {
		        var getNodeNamesValue = Regex.Replace(XPathText.Text, @"^\s*(name|local-name)\s*\((.+?)\)$", "$2", RegexOptions.IgnoreCase);

		        var navigator = await Task<XPathNavigator2>.Factory.StartNew((input) => XPATH.Select((string) input, getNodeNamesValue), editor.Text);
		        if (navigator.Result.Any())
		        {
			        Result = new XPathCollection(navigator.Result);
			        UpdateResultDataGrid(Result);
		        }
		        else
		        {
			        ReportStatus(Resources.NoDataFound, null);
		        }
	        }
	        catch (Exception ex)
	        {
		        ReportStatus(ex.Message);
	        }
        }

        async void UpdateResultDataGrid(IEnumerable<DGVXPathResult> items)
        {
            xpathResultDataGrid.DataSource = null;
            await xpathResultDataGrid.AssignCollectionAsync(items, null, true);
        }

        void ClearResultTap()
        {
            ReportStatus(string.Empty);
            xpathResultDataGrid.DataSource = null;
        }

        void ReportStatus(string message, bool? isException = true)
        {
            if(_statusInfo == null)
                return;

            if (!message.IsNullOrEmpty())
            {
                _statusInfo.BackColor = isException == null ? Color.Yellow : isException.Value ? Color.Red : Color.Green;
                _statusInfo.ForeColor = isException == null ? Color.Black : Color.White;
                _statusInfo.Text = message.Replace("\r", "").Replace("\n", " ");
            }
            else
            {
                _statusInfo.BackColor = SystemColors.Control;
                _statusInfo.ForeColor = Color.Black;
                _statusInfo.Text = string.Empty;
            }
        }

        public void ApplySettings()
        {
            buttonFind.Text = Resources.XPathText;
            buttonPrettyPrint.Text = Resources.PrettyPrintText;
            ReportStatus(string.Empty);
        }

        public void SaveData()
        {
            
        }

        public void ChangeTheme(Themes theme)
        {
	        // ignored
        }
    }
}