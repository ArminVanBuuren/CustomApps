﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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

        private int _prevSortedColumn = -1;

        public XPathCollection Result { get; private set; }

        public XmlDocument XmlBody { get; private set; }

        public int ActiveProcessesCount => 0;

        public int ActiveTotalProgress => 0;

        public XPathTesterForm()
        {
            InitializeComponent();

            base.Text = $"{base.Text} {this.GetAssemblyInfo().CurrentVersion}";

            xpathResultDataGrid.AutoGenerateColumns = false;
            xpathResultDataGrid.MouseClick += XpathResultDataGrid_SelectionChanged;
            xpathResultDataGrid.SelectionChanged += XpathResultDataGrid_SelectionChanged;
            xpathResultDataGrid.RowPrePaint += XpathResultDataGrid_RowPrePaint;
            xpathResultDataGrid.ColumnHeaderMouseClick += XpathResultDataGrid_ColumnHeaderMouseClick;

            KeyPreview = true;
            KeyDown += XPathWindow_KeyDown;
            
            editor.SizingGrip = false;
            editor.SetLanguages(new [] { Language.XML, Language.HTML }, Language.XML);
            _statusInfo = editor.AddToolStripLabel();
            _statusInfo.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            editor.TextChanged += XmlBodyRichTextBoxOnTextChanged;

            ApplySettings();
            CenterToScreen();
        }

        private void XpathResultDataGrid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (xpathResultDataGrid.RowCount > 0)
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

                var columnName = xpathResultDataGrid.Columns[e.ColumnIndex].Name;
                var result = Result.AsQueryable();

                if (_prevSortedColumn == e.ColumnIndex)
                {
                    Result = new XPathCollection(result.OrderBy(columnName));
                    _prevSortedColumn = e.ColumnIndex;
                }
                else
                {
                    Result = new XPathCollection(result.OrderByDescending(columnName));
                    _prevSortedColumn = -1;
                }

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
                    xpathResultDataGrid.MouseClick -= XpathResultDataGrid_SelectionChanged;
                    xpathResultDataGrid.SelectionChanged -= XpathResultDataGrid_SelectionChanged;
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
                    xpathResultDataGrid.MouseClick += XpathResultDataGrid_SelectionChanged;
                    xpathResultDataGrid.SelectionChanged += XpathResultDataGrid_SelectionChanged;
                    xpathResultDataGrid.ColumnHeaderMouseClick += XpathResultDataGrid_ColumnHeaderMouseClick;
                }
            }
        }

        void XmlBodyRichTextBoxOnTextChanged(object sender, EventArgs eventArgs)
        {
            lock (sync)
            {
                if (editor.Text.IsNullOrEmpty())
                {
                    XmlBody = null;
                    ReportStatus(string.Empty);
                    return;
                }

                try
                {
                    ClearResultTap();

                    var source = editor.Text;
                    if (!source.StartsWith("<") || !source.EndsWith(">"))
                    {
                        ReportStatus(string.Format(Resources.XmlIsIncorrect, string.Empty));
                        return;
                    }

                    try
                    {   
                        var document = new XmlDocument();
                        document.LoadXml(XML.RemoveUnallowable(source, " "));
                        XmlBody = document;
                    }
                    catch (Exception ex)
                    {
                        XmlBody = null;
                        ReportStatus(string.Format(Resources.XmlIsIncorrect, $". {ex.Message}"));
                    }
                }
                catch (Exception ex)
                {
                    ReportStatus(ex.Message);
                }
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

            if (string.IsNullOrEmpty(XPathText.Text))
            {
                ReportStatus(Resources.XPathIsEmpty);
                return;
            }

            ClearResultTap();

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
            _prevSortedColumn = -1;
        }

        void ReportStatus(string message, bool? isException = true)
        {
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
    }
}