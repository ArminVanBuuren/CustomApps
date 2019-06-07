using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
        private SolidBrush solidRed = new SolidBrush(Color.PaleVioletRed);
        private SolidBrush solidTransparent = new SolidBrush(Color.Transparent);

        bool _xmlBodyChanged = false;
        XmlDocument _currentXmlBody;
        XpathCollection strLines;
        int prevSortedColun = -1;

        public MainWindow()
        {
            InitializeComponent();
            fctb.TextChanged += XmlBodyRichTextBoxOnTextChanged;
            //xpathResultDataGrid.CellDoubleClick += XpathResultDataGrid_CellDoubleClick;

            xpathResultDataGrid.CellMouseDoubleClick += XpathResultDataGrid_CellMouseDoubleClick;
            //xpathResultDataGrid.DoubleClick += XpathResultDataGrid_DoubleClick;
            xpathResultDataGrid.ColumnHeaderMouseClick += XpathResultDataGrid_ColumnHeaderMouseClick;

            this.tabMain.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.tabMain.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControl1_DrawItem);


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

        private object sync = new object();
        private bool isInsert = false;
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
        private void XmlBodyRichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // флаг то что текст был проинсертин в окно CNTR+V, чтобы можно было сразу корреткно отформатировать
            if (e.Control && KeyIsDown(Keys.ControlKey) && KeyIsDown(Keys.V))
            {
                lock (sync)
                {
                    isInsert = true;
                }
            }
        }

        private bool KeyIsDown(Keys key)
        {
            return (GetAsyncKeyState(key) < 0);
        }


        void XpathResultDataGrid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                //xpathResultDataGrid.Sort(xpathResultDataGrid.Columns[e.ColumnIndex], ListSortDirection.Ascending);
                IEnumerable<XPathResults> OrderedItems = null;
                if (prevSortedColun != e.ColumnIndex)
                {
                    if (e.ColumnIndex == 0)
                        OrderedItems = from p in strLines.AsEnumerable() orderby p.ID descending select p;
                    else if (e.ColumnIndex == 1)
                        OrderedItems = from p in strLines.AsEnumerable() orderby p.NodeType descending select p;
                    else if (e.ColumnIndex == 2)
                        OrderedItems = from p in strLines.AsEnumerable() orderby p.NodeName descending select p;
                    else if (e.ColumnIndex == 3)
                        OrderedItems = from p in strLines.AsEnumerable() orderby p.Value descending select p;
                    prevSortedColun = e.ColumnIndex;
                }
                else
                {
                    if (e.ColumnIndex == 0)
                        OrderedItems = from p in strLines.AsEnumerable() orderby p.ID select p;
                    else if (e.ColumnIndex == 1)
                        OrderedItems = from p in strLines.AsEnumerable() orderby p.NodeType select p;
                    else if (e.ColumnIndex == 2)
                        OrderedItems = from p in strLines.AsEnumerable() orderby p.NodeName select p;
                    else if (e.ColumnIndex == 3)
                        OrderedItems = from p in strLines.AsEnumerable() orderby p.Value select p;
                    prevSortedColun = -1;
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

            }
        }

        private void XpathResultDataGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView data = (DataGridView)sender;
            if (data.CurrentRow?.Cells.Count < 5)
                return;
            XmlNode node = data.CurrentRow?.Cells[4].Value as XmlNode;
            if (node != null && _currentXmlBody != null)
            {
                XmlNodeResult xmlObject = RtfFromXml.GetPositionByXmlNode(fctb.Text, _currentXmlBody, node);
                if (xmlObject != null)
                {
                    tabMain.SelectTab(tabXmlBody);
                    Range range = fctb.GetRange(xmlObject.InnerText.Length - xmlObject.FindedText.Length, xmlObject.InnerText.Length);

                    fctb.Selection = range;
                    fctb.DoSelectionVisible();
                    fctb.Invalidate();
                }
            }
        }

        private Brush _mainTabBrush = new SolidBrush(Color.Transparent);
        private Brush MainTabBrush
        {
            get => _mainTabBrush;
            set
            {
                _mainTabBrush = value;
                tabMain.Invalidate();
            }
        }

        private Brush _resultTabBrush = new SolidBrush(Color.Transparent);
        private Brush ResultTabBrush
        {
            get => _resultTabBrush;
            set
            {
                _resultTabBrush = value;
                tabMain.Invalidate();
            }
        }

        MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));
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
                    //lock (sync)
                    //{
                    //    if (isInsert)
                    //    {
                    //        string formatting = RtfFromXml.GetXmlString(document);
                    //        fctb.Text = formatting;
                    //        isInsert = false;
                    //    }
                    //}

                    _currentXmlBody = document;
                    
                    MainTabBrush = solidTransparent;
                }
                else
                {
                    _currentXmlBody = null;
                    MainTabBrush = solidRed;
                    isInsert = false;
                }

                fctb.Language = Language.XML;
                fctb.ClearStylesBuffer();
                fctb.Range.ClearStyle(StyleIndex.All);
                fctb.AddStyle(SameWordsStyle);
                fctb.OnSyntaxHighlight(new TextChangedEventArgs(fctb.Range));


                //fctb.SyntaxHighlighter.InitStyleSchema(Language.XML);
                //fctb.SyntaxHighlighter.XMLSyntaxHighlight(fctb.Range);
                //fctb.Range.ClearFoldingMarkers();
                

                
                //if (_currentXmlBody != null)
                //{
                //    int oldSelectionStart = xmlBodyRichTextBox.SelectionStart;
                //    xmlBodyRichTextBox.Rtf = RtfFromXml.Convert(_currentXmlBody);
                //    xmlBodyRichTextBox.SelectionStart = oldSelectionStart;
                //    xmlBodyRichTextBox.HideSelection = false;
                //}
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
            if (_currentXmlBody == null)
                return;


            string formatting = RtfFromXml.GetXmlString(_currentXmlBody);
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
            if (_currentXmlBody == null)
            {
                AddMessageException(@"Incorrect XML Body");
                return;
            }
            if (string.IsNullOrEmpty(XPathText.Text))
            {
                AddMessageException(@"XPath Expression Is Empty");
                return;
            }
            try
            {

                string getNodeNamesValue = Regex.Replace(XPathText.Text, @"^\s*(name|local-name)\s*\((.+?)\)$", "$2", RegexOptions.IgnoreCase);
                if (XPathText.Text.Length > getNodeNamesValue.Length)
                {
                    XpathCollection temp = XmlExpression(_currentXmlBody.CreateNavigator(), getNodeNamesValue);
                    strLines = new XpathCollection();
                    foreach (XPathResults resultItem in temp)
                    {
                        strLines.Add(new XPathResults
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
                    strLines = XmlExpression(_currentXmlBody.CreateNavigator(), XPathText.Text);


                if (strLines == null || strLines.Count == 0)
                    return;
                tabMain.SelectTab(tabXPathResult);
                UpdateResultDataGrid(strLines);
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
            prevSortedColun = -1;
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
                            IHasXmlNode node = current as IHasXmlNode;
                            if (node != null)
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
                        IHasXmlNode node = o as IHasXmlNode;
                        if (node != null)
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
