using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using FastColoredTextBoxNS;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace XPathTester
{
    public partial class MainWindow : Form
    {
        private readonly object sync = new object();
        private readonly SolidBrush solidRed = new SolidBrush(Color.PaleVioletRed);
        private readonly SolidBrush solidTransparent = new SolidBrush(Color.Transparent);
        private readonly ToolStripStatusLabel _statusInfo;
        private readonly MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));

        private bool _xmlBodyChanged = false;
        private int _prevSortedColumn = -1;

        public XPathCollection Result { get; private set; }

        public XmlDocument XmlBody { get; private set; }

        public bool IsInserted { get; private set; } = false;
        
        private Brush MainTabBrush { get; set; } = new SolidBrush(Color.Transparent);

        public MainWindow()
        {
            InitializeComponent();

            xpathResultDataGrid.SelectionChanged += XpathResultDataGrid_SelectionChanged;
            xpathResultDataGrid.RowPrePaint += XpathResultDataGrid_RowPrePaint;
            xpathResultDataGrid.KeyDown += XpathResultDataGrid_KeyDown;
            xpathResultDataGrid.CellMouseDoubleClick += XpathResultDataGrid_CellMouseDoubleClick;
            xpathResultDataGrid.ColumnHeaderMouseClick += XpathResultDataGrid_ColumnHeaderMouseClick;

            KeyPreview = true;
            KeyDown += XPathWindow_KeyDown;
            
            editor.SizingGrip = true;
            editor.ChangeLanguage(Language.XML);
            editor.StatusStrip.Items.Add(new ToolStripSeparator());
            _statusInfo = new ToolStripStatusLabel("") { Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Margin = new Padding(0, 2, 0, 2) };
            editor.StatusStrip.Items.Add(_statusInfo);
            editor.KeyDown += XmlBodyRichTextBox_KeyDown;
            editor.TextChanged += XmlBodyRichTextBoxOnTextChanged;
        }

        private void XpathResultDataGrid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            xpathResultDataGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = xpathResultDataGrid.BackgroundColor;
        }

        private void XPathWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    ButtonFind_Click(this, EventArgs.Empty);
                    e.SuppressKeyPress = true;  // Stops other controls on the form receiving event.
                    break;
                case Keys.F6:
                    ButtonPrettyPrint_Click(this, EventArgs.Empty);
                    e.SuppressKeyPress = true;
                    break;
            }
        }

        
        private void XmlBodyRichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // флаг то что текст был проинсертин в окно CNTR+V, чтобы можно было сразу корреткно отформатировать
            if (e.Control && KeyIsDown(Keys.ControlKey) && KeyIsDown(Keys.V))
            {
                lock (sync)
                {
                    IsInserted = true;
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
        private static bool KeyIsDown(Keys key)
        {
            return (GetAsyncKeyState(key) < 0);
        }

        void XpathResultDataGrid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (Result == null)
                    return;

                IEnumerable<DGVXPathResult> orderedItems = null;
                if (_prevSortedColumn != e.ColumnIndex)
                {
                    switch (e.ColumnIndex)
                    {
                        case 0:
                            orderedItems = from p in Result.AsEnumerable() orderby p.ID descending select p;
                            break;
                        case 1:
                            orderedItems = from p in Result.AsEnumerable() orderby p.NodeType descending select p;
                            break;
                        case 2:
                            orderedItems = from p in Result.AsEnumerable() orderby p.NodeName descending select p;
                            break;
                        case 3:
                            orderedItems = from p in Result.AsEnumerable() orderby p.Value descending select p;
                            break;
                    }

                    _prevSortedColumn = e.ColumnIndex;
                }
                else
                {
                    switch (e.ColumnIndex)
                    {
                        case 0:
                            orderedItems = from p in Result.AsEnumerable() orderby p.ID select p;
                            break;
                        case 1:
                            orderedItems = from p in Result.AsEnumerable() orderby p.NodeType select p;
                            break;
                        case 2:
                            orderedItems = from p in Result.AsEnumerable() orderby p.NodeName select p;
                            break;
                        case 3:
                            orderedItems = from p in Result.AsEnumerable() orderby p.Value select p;
                            break;
                    }

                    _prevSortedColumn = -1;
                }

                Result = new XPathCollection(orderedItems);
                UpdateResultDataGrid(Result);
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message);
            }
        }

        private void XpathResultDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            if (!(sender is DataGridView data) || data.SelectedCells.Count == 0)
                return;

            XpathResultDataGrid_CellMouseDoubleClick(sender, null);
            // Останавливает все последующие эвенты, нужен true для того чтобы выделенная строка не переносилось на следующую строку
            e.SuppressKeyPress = true;
        }

        private void XpathResultDataGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (xpathResultDataGrid.CurrentRow == null || xpathResultDataGrid.SelectedRows.Count == 0)
                return;

            XpathResultDataGrid_CellMouseDoubleClick(xpathResultDataGrid, null);
        }

        private async void XpathResultDataGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if(Result == null)
                return;

            if (xpathResultDataGrid.CurrentRow?.Cells[0].Value is int id)
            {
                try
                {
                    editor.TextChanged -= XmlBodyRichTextBoxOnTextChanged;
                    xpathResultDataGrid.SelectionChanged -= XpathResultDataGrid_SelectionChanged;
                    xpathResultDataGrid.KeyDown -= XpathResultDataGrid_KeyDown;
                    xpathResultDataGrid.CellMouseDoubleClick -= XpathResultDataGrid_CellMouseDoubleClick;
                    xpathResultDataGrid.ColumnHeaderMouseClick -= XpathResultDataGrid_ColumnHeaderMouseClick;

                    var xmlObject = await Task<XmlNodeResult>.Factory.StartNew(() => XML.GetPositionByXmlNode(editor.Text, XmlBody, Result[id].Node));
                    if (xmlObject == null)
                        return;

                    var range = editor.GetRange(xmlObject.IndexStart, xmlObject.IndexEnd);

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
                    editor.TextChanged += XmlBodyRichTextBoxOnTextChanged;
                    xpathResultDataGrid.SelectionChanged += XpathResultDataGrid_SelectionChanged;
                    xpathResultDataGrid.KeyDown += XpathResultDataGrid_KeyDown;
                    xpathResultDataGrid.CellMouseDoubleClick += XpathResultDataGrid_CellMouseDoubleClick;
                    xpathResultDataGrid.ColumnHeaderMouseClick += XpathResultDataGrid_ColumnHeaderMouseClick;
                }
            }
        }

        void XmlBodyRichTextBoxOnTextChanged(object sender, EventArgs eventArgs)
        {
            try
            {
                if (_xmlBodyChanged)
                    return;

                ClearResultTap();
                _xmlBodyChanged = true;

                try
                {
                    var document = new XmlDocument();
                    document.LoadXml(XML.RemoveUnallowable(editor.Text, " "));
                    XmlBody = document;
                    MainTabBrush = solidTransparent;
                }
                catch (Exception ex)
                {
                    XmlBody = null;
                    MainTabBrush = solidRed;
                    IsInserted = false;
                    ReportStatus($"XML-Body is incorrect! {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message);
            }
            finally
            {
                _xmlBodyChanged = false;
            }
        }

        private void ButtonPrettyPrint_Click(object sender, EventArgs e)
        {
            if (XmlBody == null)
                return;

            editor.Text = XmlBody.PrintXml();
        }


        void ButtonFind_Click(object sender, EventArgs e)
        {
            if (XmlBody == null)
                return;

            ClearResultTap();

            if (string.IsNullOrEmpty(XPathText.Text))
            {
                ReportStatus(@"XPath expression is empty!");
                return;
            }

            try
            {

                var getNodeNamesValue = Regex.Replace(XPathText.Text, @"^\s*(name|local-name)\s*\((.+?)\)$", "$2", RegexOptions.IgnoreCase);

                if (XmlBody.CreateNavigator().Select(getNodeNamesValue, out var result))
                {
                    Result = new XPathCollection(result);
                    UpdateResultDataGrid(Result);
                }
                else
                {
                    ReportStatus("No data found", false);
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
            _prevSortedColumn = -1;
        }

        void ReportStatus(string message, bool isException = true)
        {
            if (!message.IsNullOrEmpty())
            {
                _statusInfo.BackColor = isException ? Color.Red : Color.Green;
                _statusInfo.ForeColor = Color.White;
                _statusInfo.Text = message.Replace("\r", "").Replace("\n", " ");
            }
            else
            {
                _statusInfo.BackColor = SystemColors.Control;
                _statusInfo.ForeColor = Color.Black;
                _statusInfo.Text = string.Empty;
            }
        }
    }
}