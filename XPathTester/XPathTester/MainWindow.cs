using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using FastColoredTextBoxNS;
using Utils;
using Utils.XmlRtfStyle;

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

        private Brush _resultTabBrush = new SolidBrush(Color.Transparent);
        private Brush _mainTabBrush = new SolidBrush(Color.Transparent);

        readonly MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));


        public XmlDocument XmlBody { get; private set; }
        public bool IsInserted { get; private set; } = false;
        
        private Brush MainTabBrush
        {
            get => _mainTabBrush;
            set
            {
                _mainTabBrush = value;
                //tabMain.Invalidate();
            }
        }

        
        private Brush ResultTabBrush
        {
            get => _resultTabBrush;
            set
            {
                _resultTabBrush = value;
                //tabMain.Invalidate();
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            fctb.TextChanged += XmlBodyRichTextBoxOnTextChanged;
            xpathResultDataGrid.CellMouseDoubleClick += XpathResultDataGrid_CellMouseDoubleClick;
            xpathResultDataGrid.ColumnHeaderMouseClick += XpathResultDataGrid_ColumnHeaderMouseClick;

            //tabMain.DrawMode = TabDrawMode.OwnerDrawFixed;
            //tabMain.DrawItem += tabControl1_DrawItem;


            KeyPreview = true;
            KeyDown += XPathWindow_KeyDown;
            fctb.KeyDown += XmlBodyRichTextBox_KeyDown;

            //fctb.ClearStylesBuffer();
            //fctb.Range.ClearStyle(StyleIndex.All);
            fctb.Language = Language.XML;
            /////////////////////fctb.DescriptionFile = "htmlDesc.xml";
            fctb.SelectionChangedDelayed += fctb_SelectionChangedDelayed;

            IsWordWrap.Checked = fctb.WordWrap;
            IsWordWrap.CheckStateChanged += (s, e) => fctb.WordWrap = IsWordWrap.Checked;

            //var wordWrapStat = new CheckBox {BackColor = Color.Transparent, Text = @"Wrap ", Checked = fctb.WordWrap, Padding = new Padding(0, 3, 0, 0)};
            //wordWrapStat.CheckStateChanged += (s, e) => fctb.WordWrap = wordWrapStat.Checked;
            //var wordWrapStatHost = new ToolStripControlHost(wordWrapStat);
            //toolStrip1.Items.Add(wordWrapStatHost);
        }

        private void fctb_SelectionChangedDelayed(object sender, EventArgs e)
        {
            fctb.VisibleRange.ClearStyle(SameWordsStyle);
            if (!fctb.Selection.IsEmpty)
                return;//user selected diapason

            //get fragment around caret
            var fragment = fctb.Selection.GetFragment(@"\w");
            string text = fragment.Text;
            if (text.Length == 0)
                return;

            //highlight same words
            var ranges = fctb.VisibleRange.GetRanges("\\b" + text + "\\b").ToArray();
            if (ranges.Length > 1)
                foreach (var r in ranges)
                    r.SetStyle(SameWordsStyle);
        }

        private void XPathWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                buttonFind_Click(this, EventArgs.Empty);
                e.SuppressKeyPress = true;  // Stops other controls on the form receiving event.
            }
            else if (e.KeyCode == Keys.F10)
            {
                buttonPrettyPrint_Click(this, EventArgs.Empty);
                e.SuppressKeyPress = true;
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
                //xpathResultDataGrid.Sort(xpathResultDataGrid.Columns[e.ColumnIndex], ListSortDirection.Ascending);
                IEnumerable<XPathResult> OrderedItems = null;
                if (_prevSortedColumn != e.ColumnIndex)
                {
                    if (e.ColumnIndex == 0)
                        OrderedItems = from p in _strLines.AsEnumerable() orderby p.ID descending select p;
                    else if (e.ColumnIndex == 1)
                        OrderedItems = from p in _strLines.AsEnumerable() orderby p.NodeType descending select p;
                    else if (e.ColumnIndex == 2)
                        OrderedItems = from p in _strLines.AsEnumerable() orderby p.NodeName descending select p;
                    else if (e.ColumnIndex == 3)
                        OrderedItems = from p in _strLines.AsEnumerable() orderby p.Value descending select p;
                    _prevSortedColumn = e.ColumnIndex;
                }
                else
                {
                    if (e.ColumnIndex == 0)
                        OrderedItems = from p in _strLines.AsEnumerable() orderby p.ID select p;
                    else if (e.ColumnIndex == 1)
                        OrderedItems = from p in _strLines.AsEnumerable() orderby p.NodeType select p;
                    else if (e.ColumnIndex == 2)
                        OrderedItems = from p in _strLines.AsEnumerable() orderby p.NodeName select p;
                    else if (e.ColumnIndex == 3)
                        OrderedItems = from p in _strLines.AsEnumerable() orderby p.Value select p;
                    _prevSortedColumn = -1;
                }

                if (OrderedItems != null)
                {
                    var ordered = new XPathCollection();
                    ordered.AddRange(OrderedItems);
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
                    xpathResultDataGrid.CellMouseDoubleClick -= XpathResultDataGrid_CellMouseDoubleClick;
                    xpathResultDataGrid.ColumnHeaderMouseClick -= XpathResultDataGrid_ColumnHeaderMouseClick;

                    XmlNodeResult xmlObject = await Task<XmlNodeResult>.Factory.StartNew(() => XML.GetPositionByXmlNode(fctb.Text, XmlBody, node));

                    if (xmlObject != null)
                    {
                        //tabMain.SelectTab(tabXmlBody);
                        Range range = fctb.GetRange(xmlObject.IndexStart, xmlObject.IndexEnd);

                        fctb.Selection = range;
                        fctb.DoSelectionVisible();
                        fctb.Invalidate();
                    }
                }
                catch (Exception ex)
                {
                    AddMessageException(ex.Message);
                }
                finally
                {
                    fctb.TextChanged += XmlBodyRichTextBoxOnTextChanged;
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

                bool isXml = XML.IsXml(fctb.Text, out XmlDocument document);

                if (isXml)
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

                //fctb.Language = Language.XML;
                //fctb.ClearStylesBuffer();
                //fctb.Range.ClearStyle(StyleIndex.All);
                //fctb.AddStyle(SameWordsStyle);
                //fctb.OnSyntaxHighlight(new TextChangedEventArgs(fctb.Range));
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

        private void buttonPrettyPrint_Click(object sender, EventArgs e)
        {
            if (XmlBody == null)
                return;

            string formatting = RtfFromXml.GetXmlString(XmlBody);

            fctb.Text = formatting;
        }

        //private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    Brush setBrush = e.Index == 0 ? MainTabBrush : ResultTabBrush;

        //    e.Graphics.FillRectangle(setBrush, e.Bounds);
        //    SizeF sz = e.Graphics.MeasureString(tabMain.TabPages[e.Index].Text, e.Font);
        //    e.Graphics.DrawString(tabMain.TabPages[e.Index].Text, e.Font, Brushes.Black, e.Bounds.Left + (e.Bounds.Width - sz.Width) / 2, e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2 + 1);

        //    Rectangle rect = e.Bounds;
        //    rect.Offset(0, 1);
        //    rect.Inflate(0, -1);
        //    e.Graphics.DrawRectangle(Pens.DarkGray, rect);
        //    e.DrawFocusRectangle();
        //}


        void buttonFind_Click(object sender, EventArgs e)
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

                string getNodeNamesValue = Regex.Replace(XPathText.Text, @"^\s*(name|local-name)\s*\((.+?)\)$", "$2", RegexOptions.IgnoreCase);
                _strLines = (XPathCollection)XPATH.Execute(XmlBody.CreateNavigator(), getNodeNamesValue);
                if (XPathText.Text.Length > getNodeNamesValue.Length)
                    _strLines.ChangeNodeType();


                if (_strLines == null || _strLines.Count == 0)
                {
                    AddMessageException("No data found", false);
                    return;
                }

                //tabMain.SelectTab(tabXPathResult);
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
            xpathResultDataGrid.Columns[0].Width = TextRenderer.MeasureText(items.MaxWidthId, xpathResultDataGrid.Columns[0].DefaultCellStyle.Font).Width + 10;
            xpathResultDataGrid.Columns[1].Width = TextRenderer.MeasureText(items.MaxWidthNodeType, xpathResultDataGrid.Columns[1].DefaultCellStyle.Font).Width + 10;
            xpathResultDataGrid.Columns[2].Width = TextRenderer.MeasureText(items.MaxWidthNodeName, xpathResultDataGrid.Columns[2].DefaultCellStyle.Font).Width + 10;
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
            if (!string.IsNullOrEmpty(strEx))
            {
                ResultTabBrush = solidRed;
                //tabMain.SelectTab(tabXPathResult);
            }
            else
            {
                ResultTabBrush = solidTransparent;
            }
        }
        
    }
}
