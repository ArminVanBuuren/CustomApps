using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Utils.XmlRtfStyle;

namespace XPathEvaluator
{
    public partial class MainWindow : Form
    {
        bool _xmlBodyChanged = false;
        XmlDocument _currentXmlBody;
        XpathCollection strLines;
        int prevSortedColun = -1;

        public MainWindow()
        {
            InitializeComponent();
            xmlBodyRichTextBox.TextChanged += XmlBodyRichTextBoxOnTextChanged;
            //xpathResultDataGrid.CellDoubleClick += XpathResultDataGrid_CellDoubleClick;

            xpathResultDataGrid.CellMouseDoubleClick += XpathResultDataGrid_CellMouseDoubleClick;
            //xpathResultDataGrid.DoubleClick += XpathResultDataGrid_DoubleClick;
            xpathResultDataGrid.ColumnHeaderMouseClick += XpathResultDataGrid_ColumnHeaderMouseClick;

            KeyPreview = true;
            KeyDown += XPathWindow_KeyDown;
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
                XmlObjectIndex xmlObject = RtfFromXml.GetPositionByXmlNode(_currentXmlBody, node);
                if (xmlObject != null)
                {
                    tabMain.SelectTab(tabXmlBody);
                    int fillTextLength = xmlObject.FillText.Replace("\n", "").Length;
                    string finded = xmlObject.FindedObject.Replace("\n", "");
                    int findedText = finded.Length;
                    int append = ((finded[0] == '\r') ? 1 : 0);
                    xmlBodyRichTextBox.Select(fillTextLength - findedText + append, findedText);
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
                if (string.IsNullOrEmpty(xmlBodyRichTextBox.Text))
                {
                    _currentXmlBody = null;
                    return;
                }
                _xmlBodyChanged = true;
                _currentXmlBody = RtfFromXml.GetXmlDocument(xmlBodyRichTextBox.Text);

                if (_currentXmlBody != null)
                {
                    int oldSelectionStart = xmlBodyRichTextBox.SelectionStart;
                    xmlBodyRichTextBox.Rtf = RtfFromXml.Convert(_currentXmlBody);
                    xmlBodyRichTextBox.SelectionStart = oldSelectionStart;
                    xmlBodyRichTextBox.HideSelection = false;
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
                tabMain.SelectTab(tabXPathResult);
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
