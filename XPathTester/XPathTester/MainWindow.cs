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

namespace XPathTester
{
    public partial class MainWindow : Form
    {
        private readonly SolidBrush solidRed = new SolidBrush(Color.PaleVioletRed);
        private readonly SolidBrush solidTransparent = new SolidBrush(Color.Transparent);
        private readonly object sync = new object();

        bool _xmlBodyChanged = false;
        
        XPathCollection _strLines;
        int _prevSortedColumn = -1;

        readonly MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));


        public XmlDocument XmlBody { get; private set; }
        public bool IsInserted { get; private set; } = false;
        
        private Brush MainTabBrush { get; set; } = new SolidBrush(Color.Transparent);


        private Brush ResultTabBrush { get; set; } = new SolidBrush(Color.Transparent);


        public MainWindow()
        {
            InitializeComponent();
            fctb.TextChanged += XmlBodyRichTextBoxOnTextChanged;
            xpathResultDataGrid.KeyDown += XpathResultDataGrid_KeyDown;
            xpathResultDataGrid.CellMouseDoubleClick += XpathResultDataGrid_CellMouseDoubleClick;
            xpathResultDataGrid.ColumnHeaderMouseClick += XpathResultDataGrid_ColumnHeaderMouseClick;

            KeyPreview = true;
            KeyDown += XPathWindow_KeyDown;
            fctb.KeyDown += XmlBodyRichTextBox_KeyDown;
            fctb.Language = Language.XML;
            /////////////////////fctb.DescriptionFile = "htmlDesc.xml";
            fctb.SelectionChangedDelayed += Fctb_SelectionChangedDelayed;

            IsWordWrap.Checked = fctb.WordWrap;
            IsWordWrap.CheckStateChanged += (s, e) => fctb.WordWrap = IsWordWrap.Checked;
        }


        private void Fctb_SelectionChangedDelayed(object sender, EventArgs e)
        {
            fctb.VisibleRange.ClearStyle(SameWordsStyle);
            if (!fctb.Selection.IsEmpty)
                return;//user selected diapason

            //get fragment around caret
            var fragment = fctb.Selection.GetFragment(@"\w");
            var text = fragment.Text;
            if (text.Length == 0)
                return;

            //highlight same words
            var ranges = fctb.VisibleRange.GetRanges("\\b" + text + "\\b").ToArray();
            if (ranges.Length > 1)
                foreach (var r in ranges)
                    r.SetStyle(SameWordsStyle);
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
                IEnumerable<XPathResult> OrderedItems = null;
                if (_prevSortedColumn != e.ColumnIndex)
                {
                    switch (e.ColumnIndex)
                    {
                        case 0:
                            OrderedItems = from p in _strLines.AsEnumerable() orderby p.ID descending select p;
                            break;
                        case 1:
                            OrderedItems = from p in _strLines.AsEnumerable() orderby p.NodeType descending select p;
                            break;
                        case 2:
                            OrderedItems = from p in _strLines.AsEnumerable() orderby p.NodeName descending select p;
                            break;
                        case 3:
                            OrderedItems = from p in _strLines.AsEnumerable() orderby p.Value descending select p;
                            break;
                    }

                    _prevSortedColumn = e.ColumnIndex;
                }
                else
                {
                    switch (e.ColumnIndex)
                    {
                        case 0:
                            OrderedItems = from p in _strLines.AsEnumerable() orderby p.ID select p;
                            break;
                        case 1:
                            OrderedItems = from p in _strLines.AsEnumerable() orderby p.NodeType select p;
                            break;
                        case 2:
                            OrderedItems = from p in _strLines.AsEnumerable() orderby p.NodeName select p;
                            break;
                        case 3:
                            OrderedItems = from p in _strLines.AsEnumerable() orderby p.Value select p;
                            break;
                    }

                    _prevSortedColumn = -1;
                }

                if (OrderedItems != null)
                {
                    var ordered = new XPathCollection(OrderedItems);
                    UpdateResultDataGrid(ordered);
                }
            }
            catch (Exception ex)
            {
                AddMessageException(ex.Message);
            }
        }

        private async void XpathResultDataGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (!(sender is DataGridView data))
                return;

            if (data.CurrentRow?.Cells.Count < 5)
                return;

            if (data.CurrentRow?.Cells[4].Value is XmlNode node && XmlBody != null)
            {
                try
                {
                    fctb.TextChanged -= XmlBodyRichTextBoxOnTextChanged;
                    xpathResultDataGrid.KeyDown -= XpathResultDataGrid_KeyDown;
                    xpathResultDataGrid.CellMouseDoubleClick -= XpathResultDataGrid_CellMouseDoubleClick;
                    xpathResultDataGrid.ColumnHeaderMouseClick -= XpathResultDataGrid_ColumnHeaderMouseClick;

                    var xmlObject = await Task<XmlNodeResult>.Factory.StartNew(() => XML.GetPositionByXmlNode(fctb.Text, XmlBody, node));
                    if (xmlObject == null)
                        return;

                    var range = fctb.GetRange(xmlObject.IndexStart, xmlObject.IndexEnd);

                    fctb.Selection = range;
                    fctb.DoSelectionVisible();
                    fctb.Invalidate();
                }
                catch (Exception ex)
                {
                    AddMessageException(ex.Message);
                }
                finally
                {
                    fctb.TextChanged += XmlBodyRichTextBoxOnTextChanged;
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

                if (fctb.Text.IsXml(out var document))
                {
                    XmlBody = document;
                    MainTabBrush = solidTransparent;
                }
                else
                {
                    XmlBody = null;
                    MainTabBrush = solidRed;
                    IsInserted = false;
                    AddMessageException(@"XML-Body is incorrect!");
                }
            }
            catch (Exception ex)
            {
                AddMessageException(ex.Message);
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

            fctb.Text = XmlBody.PrintXml();
        }


        void ButtonFind_Click(object sender, EventArgs e)
        {
            ClearResultTap();

            if (XmlBody == null)
            {
                AddMessageException(@"XML-Body is incorrect!");
                return;
            }

            if (string.IsNullOrEmpty(XPathText.Text))
            {
                AddMessageException(@"XPath expression is empty!");
                return;
            }

            try
            {

                var getNodeNamesValue = Regex.Replace(XPathText.Text, @"^\s*(name|local-name)\s*\((.+?)\)$", "$2", RegexOptions.IgnoreCase);
                _strLines = new XPathCollection(XPATH.Select(XmlBody.CreateNavigator(), getNodeNamesValue));
                if (XPathText.Text.Length > getNodeNamesValue.Length)
                    _strLines.ModifyValueToNodeName();


                if (_strLines == null || _strLines.Count == 0)
                {
                    AddMessageException("No data found", false);
                    return;
                }

                UpdateResultDataGrid(_strLines);
            }
            catch (Exception ex)
            {
                AddMessageException(ex.Message);
            }
        }

        void UpdateResultDataGrid(XPathCollection items)
        {
            xpathResultDataGrid.DataSource = null;
            xpathResultDataGrid.DataSource = items;
            xpathResultDataGrid.Columns[0].Width = TextRenderer.MeasureText(items.MaxWidthId, xpathResultDataGrid.Columns[0].DefaultCellStyle.Font).Width;
            xpathResultDataGrid.Columns[1].Width = TextRenderer.MeasureText(items.MaxWidthNodeType, xpathResultDataGrid.Columns[1].DefaultCellStyle.Font).Width;
            xpathResultDataGrid.Columns[2].Width = TextRenderer.MeasureText(items.MaxWidthNodeName, xpathResultDataGrid.Columns[2].DefaultCellStyle.Font).Width;
            xpathResultDataGrid.Columns[4].Visible = false;
        }

        void ClearResultTap()
        {
            AddMessageException(string.Empty);
            xpathResultDataGrid.DataSource = null;
            _prevSortedColumn = -1;
        }

        void AddMessageException(string strEx, bool isException = true)
        {
            exceptionMessage.ForeColor = isException ? Color.Red : Color.Black;
            exceptionMessage.Text = strEx;
            ResultTabBrush = !string.IsNullOrEmpty(strEx) ? solidRed : solidTransparent;
        }
        
    }
}