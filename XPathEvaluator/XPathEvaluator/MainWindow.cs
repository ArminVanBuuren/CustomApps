﻿using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Utils;
using Utils.XmlRtfStyle;

namespace XPathEvaluator
{
    public partial class MainWindow : Form
    {
        private readonly SolidBrush solidRed = new SolidBrush(Color.PaleVioletRed);
        private readonly SolidBrush solidTransparent = new SolidBrush(Color.Transparent);
        private readonly object sync = new object();

        bool _xmlBodyChanged = false;
        
        XpathCollection _strLines;
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
                tabMain.Invalidate();
            }
        }

        
        private Brush ResultTabBrush
        {
            get => _resultTabBrush;
            set
            {
                _resultTabBrush = value;
                tabMain.Invalidate();
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            fctb.TextChanged += XmlBodyRichTextBoxOnTextChanged;
            xpathResultDataGrid.CellMouseDoubleClick += XpathResultDataGrid_CellMouseDoubleClick;
            xpathResultDataGrid.ColumnHeaderMouseClick += XpathResultDataGrid_ColumnHeaderMouseClick;

            tabMain.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabMain.DrawItem += tabControl1_DrawItem;


            KeyPreview = true;
            KeyDown += XPathWindow_KeyDown;
            fctb.KeyDown += XmlBodyRichTextBox_KeyDown;

            fctb.ClearStylesBuffer();
            fctb.Range.ClearStyle(StyleIndex.All);
            fctb.Language = Language.XML;
            fctb.SelectionChangedDelayed += fctb_SelectionChangedDelayed;
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
            //if (e.Control && e.KeyCode == Keys.S)       // Ctrl-S Save
            if (e.KeyCode == Keys.F5)
            {
                buttonFind_Click(this, EventArgs.Empty);
                e.SuppressKeyPress = true;  // Stops other controls on the form receiving event.
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
                IEnumerable<XPathResults> OrderedItems = null;
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
                    XpathCollection ordered = new XpathCollection();
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
                        tabMain.SelectTab(tabXmlBody);
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
                }

                fctb.Language = Language.XML;
                fctb.ClearStylesBuffer();
                fctb.Range.ClearStyle(StyleIndex.All);
                fctb.AddStyle(SameWordsStyle);
                fctb.OnSyntaxHighlight(new TextChangedEventArgs(fctb.Range));
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

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            Brush setBrush = e.Index == 0 ? MainTabBrush : ResultTabBrush;

            e.Graphics.FillRectangle(setBrush, e.Bounds);
            SizeF sz = e.Graphics.MeasureString(tabMain.TabPages[e.Index].Text, e.Font);
            e.Graphics.DrawString(tabMain.TabPages[e.Index].Text, e.Font, Brushes.Black, e.Bounds.Left + (e.Bounds.Width - sz.Width) / 2, e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2 + 1);

            Rectangle rect = e.Bounds;
            rect.Offset(0, 1);
            rect.Inflate(0, -1);
            e.Graphics.DrawRectangle(Pens.DarkGray, rect);
            e.DrawFocusRectangle();
        }


        void buttonFind_Click(object sender, EventArgs e)
        {
            ClearResultTap();

            if (XmlBody == null)
            {
                AddMessageException(@"Incorrect XML-body!");
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
                if (XPathText.Text.Length > getNodeNamesValue.Length)
                {
                    XpathCollection temp = XmlExpression(XmlBody.CreateNavigator(), getNodeNamesValue);
                    _strLines = new XpathCollection();
                    foreach (XPathResults resultItem in temp)
                    {
                        _strLines.Add(new XPathResults
                        {
                            ID = resultItem.ID,
                            Node = resultItem.Node,
                            NodeName = "Empty",
                            NodeType = resultItem.NodeName.GetType().Name,
                            Value = resultItem.NodeName
                        });
                    }
                    temp.Clear();
                }
                else
                    _strLines = XmlExpression(XmlBody.CreateNavigator(), XPathText.Text);


                if (_strLines == null || _strLines.Count == 0)
                    return;
                tabMain.SelectTab(tabXPathResult);
                UpdateResultDataGrid(_strLines);
            }
            catch (Exception ex)
            {
                AddMessageException(ex.Message);
            }
        }

        void UpdateResultDataGrid(XpathCollection items)
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

        void AddMessageException(string strEx)
        {
            exceptionMessage.Text = strEx;
            if (!string.IsNullOrEmpty(strEx))
            {
                ResultTabBrush = solidRed;
                tabMain.SelectTab(tabXPathResult);
            }
            else
            {
                ResultTabBrush = solidTransparent;
            }
        }

        XpathCollection XmlExpression(XPathNavigator navigator, string xpath)
        {

            if (navigator == null)
                return null;
            XPathExpression expression = XPathExpression.Compile(xpath);
            XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
            //manager.AddNamespace("bk", "http://www.contoso.com/books");
            manager.AddNamespace(string.Empty, "urn:samples");
            expression.SetContext(manager);
            switch (expression.ReturnType)
            {
                case XPathResultType.NodeSet:
                    XPathNodeIterator nodes = navigator.Select(expression);
                    if (nodes.Count > 0)
                    {
                        XpathCollection strOut = new XpathCollection();
                        int i = 0;
                        while (nodes.MoveNext())
                        {
                            XPathNavigator current = nodes.Current;
                            if (current == null)
                                continue;
                            XPathResults res = new XPathResults
                            {
                                ID = i + 1,
                                NodeType = current.NodeType.ToString(),
                                NodeName = current.Name,
                                Value = current.Value
                            };
                            if (current is IHasXmlNode node)
                                res.Node = node.GetNode();
                            strOut.Add(res);
                            i++;
                        }
                        return strOut;
                    }
                    return null;
                default:
                    object o = navigator.Evaluate(expression);
                    if (o != null)
                    {
                        XpathCollection res = new XpathCollection
                        {
                            new XPathResults
                            {
                                ID = 0,
                                NodeType = expression.ReturnType.ToString(),
                                NodeName = "Empty",
                                Value = o.ToString()
                            }
                        };
                        if (o is IHasXmlNode node)
                        {
                            res[0].Node = node.GetNode();
                            res[0].NodeName = res[0].Node.NodeType.ToString();
                        }
                        return res;
                    }
                    return null;
            }
        }
    }
}
