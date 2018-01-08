using System;
using Script.Handlers;
using Microsoft.Office.Interop.Word;

namespace Script.Control.Handlers.Timesheet.WriteData
{
    public class WriteWordData
    {
        //https://stackoverflow.com/questions/18505198/how-to-merge-cells-of-a-table-created-in-ms-word-using-c-net
        int row = 1;
        private object wrdRng;
        _Application objWord;
        private _Document objDoc;
        private object oMissing;
        public string WordPathFile { get; }
        private Table WordTable { get; set; }
        object oEndOfDoc = "\\endofdoc";
        public WriteWordData(string path)
        {
            WordPathFile = path;
            oMissing = System.Reflection.Missing.Value;

            objWord = new Application();
            if (path == null)
                objWord.Visible = true;
            objDoc = objWord.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            objDoc.GrammarChecked = false;
        }


        public void AddDocHeader(string fullUserName, string header)
        {
            wrdRng = objDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            Paragraph objPara = objDoc.Content.Paragraphs.Add(ref wrdRng);
            objPara.Range.InsertParagraphAfter();
            objPara.Range.Text = fullUserName + "\v" + ReplaceWordText(header) + "\v";
            objPara.Range.Font.Size = 16;
            objPara.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
            //return;
            //Add header into the document
            foreach (Section section in objDoc.Sections)
            {
                //Get the header range and add the header details.
                Range headerRange = section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                headerRange.Fields.Add(headerRange, WdFieldType.wdFieldPage);
                headerRange.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                headerRange.Font.ColorIndex = WdColorIndex.wdRed;
                headerRange.Font.Size = 8;
                //headerRange.Font.Bold = 1;
                headerRange.Text = TimesheetHandler.TIMESHEET_NAME;
            }
        }

        public void AddTableHeader(string tableHeader, int numColumns)
        {
            row = 1;
            wrdRng = objDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            Paragraph objPara = objDoc.Content.Paragraphs.Add(ref wrdRng);
            objPara.Range.InsertParagraphAfter();
            WordTable = objDoc.Tables.Add((Range) wrdRng, 2, numColumns, ref oMissing, ref oMissing);
            WordTable.AllowAutoFit = true;
            WordTable.Borders.Enable = 1;
            WordTable.Range.ParagraphFormat.SpaceAfter = 7;
            WordTable.Range.GrammarChecked = false;
            WordTable.Rows[row].Range.Text = ReplaceWordText(tableHeader);
            WordTable.Rows[row].Range.Font.Bold = 1;
            WordTable.Rows[row].Range.Font.Size = 24;
            WordTable.Rows[row].Range.Font.Position = 1;
            WordTable.Rows[row].Range.Font.Name = "Times New Roman";


            WordTable.Columns[2].Width = WordTable.Columns[1].Width - 100 + WordTable.Columns[2].Width;
            WordTable.Columns[1].Width = 100;
            WordTable.Rows[row].Cells[1].Merge(WordTable.Rows[row].Cells[2]);
            WordTable.Cell(row, 1).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;

        }

        public void AddColumnsName(params string[] nameColumn)
        {
            row++;
            int columnNum = 0;
            foreach(string columnName in nameColumn)
            {
                columnNum++;
                WordTable.Rows[row].Range.Font.Bold = 1;
                WordTable.Rows[row].Range.Font.Italic = 1;
                WordTable.Rows[row].Range.Font.Size = 14;
                WordTable.Cell(row, columnNum).Range.Text = columnName;
                WordTable.Cell(row, columnNum).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
            }
        }

        public void AddCellsData(string projectName, string stringStat, string tfsCollection)
        {
            WordTable.Rows.Add();
            row++;
            WordTable.Rows[row].Range.Font.Bold = 0;
            WordTable.Rows[row].Range.Font.Italic = 0;
            WordTable.Rows[row].Range.Font.Size = 11;
            WordTable.Rows[row].Range.GrammarChecked = false;
            WordTable.Rows[row].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            WordTable.Cell(row, 1).WordWrap = true;
            WordTable.Cell(row, 1).Range.Text = ReplaceWordText(projectName);
            WordTable.Cell(row, 1).Range.Borders[WdBorderType.wdBorderTop].LineStyle =  WdLineStyle.wdLineStyleSingle;
            WordTable.Cell(row, 2).Range.Text = ReplaceWordText(stringStat);

            //WordTable.Cell(row, 1).Range.Collapse(WdCollapseDirection.wdCollapseEnd);
            //WordTable.Cell(row, 1).Range.MoveEnd(WdUnits.wdCharacter, -1);
            //WordTable.Cell(row, 2).Width = WordTable.Cell(row, 1).Width - 100 + WordTable.Cell(row, 2).Width;
            //WordTable.Cell(row, 1).Width = 100;

            WordTable.Rows.Add();
            row++;
            WordTable.Rows[row].Range.Font.Bold = 0;
            WordTable.Rows[row].Range.Font.Italic = 0;
            WordTable.Rows[row].Range.Font.Size = 8;
            WordTable.Rows[row].Range.GrammarChecked = false;
            WordTable.Rows[row].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            WordTable.Cell(row, 1).WordWrap = true;
            WordTable.Cell(row, 1).Range.Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleNone;
            WordTable.Cell(row, 1).Range.Text = string.Empty;
            WordTable.Cell(row, 2).Range.Text = ReplaceWordText(tfsCollection);

        }
        public void AddTableFooter(string footer)
        {
            WordTable.Rows.Add();
            row++;
            WordTable.Rows[row].Range.Text = ReplaceWordText(footer);
            WordTable.Rows[row].Range.Font.Bold = 1;
            WordTable.Rows[row].Range.Font.Size = 11;
            WordTable.Rows[row].Range.Font.Italic = 0;
            WordTable.Cell(row, 1).Range.Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;
            WordTable.Rows[row].Cells[1].Merge(WordTable.Rows[row].Cells[2]);
        }

        public void SaveDocument()
        {
            if (WordPathFile != null)
            {
                object missing = System.Reflection.Missing.Value;
                object wordPathFile = WordPathFile;
                objDoc.SaveAs(ref wordPathFile, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);
                objDoc.Close(ref missing, ref missing, ref missing);
                objDoc = null;
                objWord.Quit(ref missing, ref missing, ref missing);
                objWord = null;
            }
        }

        public string ReplaceWordText(string str)
        {
            return str.Replace(Environment.NewLine, "\v");
        }

        public void CreateDocument()
        {
            try
            {
                //Create an instance for word app
                Application winword = new Application();

                //Set animation status for word application
                winword.ShowAnimation = false;

                //Set status for word application is to be visible or not.
                winword.Visible = false;

                //Create a missing variable for missing value
                object missing = System.Reflection.Missing.Value;

                //Create a new document
                Document document = winword.Documents.Add(ref missing, ref missing, ref missing, ref missing);

                //Add header into the document
                foreach (Section section in document.Sections)
                {
                    //Get the header range and add the header details.
                    Range headerRange = section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Fields.Add(headerRange, WdFieldType.wdFieldPage);
                    headerRange.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.ColorIndex = WdColorIndex.wdDarkRed;
                    headerRange.Font.Size = 12;
                    //headerRange.Font.Bold = 1;
                    headerRange.Text = "Header text goes here";
                }

                //Add the footers into the document
                foreach (Section wordSection in document.Sections)
                {
                    //Get the footer range and add the footer details.
                    Range footerRange = wordSection.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    footerRange.Font.ColorIndex = WdColorIndex.wdDarkRed;
                    footerRange.Font.Size = 10;
                    footerRange.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                    footerRange.Text = "Footer text goes here";
                }

                //adding text to document
                document.Content.SetRange(0, 0);
                document.Content.Text = "This is test document " + Environment.NewLine;

                //Add paragraph with Heading 1 style
                Paragraph para1 = document.Content.Paragraphs.Add(ref missing);
                //object styleHeading1 = "Heading 1";
                //para1.Range.set_Style(ref styleHeading1);
                para1.Range.Text = "Para 1 text";
                para1.Range.InsertParagraphAfter();

                //Add paragraph with Heading 2 style
                Paragraph para2 = document.Content.Paragraphs.Add(ref missing);
                //object styleHeading2 = "Heading 2";
                //para2.Range.set_Style(ref styleHeading2);
                para2.Range.Text = "Para 2 text";
                para2.Range.InsertParagraphAfter();

                //Create a 5X5 table and insert some dummy record
                Table firstTable = document.Tables.Add(para1.Range, 5, 5, ref missing, ref missing);

                firstTable.Borders.Enable = 1;
                foreach (Row row in firstTable.Rows)
                {
                    foreach (Cell cell in row.Cells)
                    {
                        //Header row
                        if (cell.RowIndex == 1)
                        {
                            cell.Range.Text = "Column " + cell.ColumnIndex.ToString();
                            cell.Range.Font.Bold = 1;
                            //other format properties goes here
                            cell.Range.Font.Name = "verdana";
                            cell.Range.Font.Size = 10;
                            //cell.Range.Font.ColorIndex = WdColorIndex.wdGray25;                            
                            cell.Shading.BackgroundPatternColor = WdColor.wdColorGray25;
                            //Center alignment for the Header cells
                            cell.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                            cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;

                        }
                        //Data row
                        else
                        {
                            cell.Range.Text = (cell.RowIndex - 2 + cell.ColumnIndex).ToString();
                        }
                    }
                }


                //Save the document
                object filename = @"c:\temp1.docx";
                document.SaveAs2(ref filename);
                document.Close(ref missing, ref missing, ref missing);
                document = null;
                winword.Quit(ref missing, ref missing, ref missing);
                winword = null;
                //MessageBox.Show("Document created successfully !");
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                //MessageBox.Show(ex.Message);
            }
        }

        public void WriteWordData2(string path)
        {
            object missing = System.Reflection.Missing.Value;
            object start1 = 0;
            object styleTypeTable = WdStyleType.wdStyleTypeTable;
            _Application WordApp = new Application();
            WordApp.Visible = true;
            Document adoc = WordApp.Documents.Add(ref missing, ref missing, ref missing, ref missing);


            Paragraph objPara3;
            object oRng = adoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            objPara3 = adoc.Content.Paragraphs.Add(ref oRng);
            objPara3.Range.Text = "hello"; //add some text in paragraph
            objPara3.Format.SpaceAfter = 10; //defind some style
            objPara3.Range.InsertParagraphAfter(); //insert paragraph

            Tables tb = adoc.Tables;
            Object defaultTableBehavior = Type.Missing;
            Object autoFitBehavior = Type.Missing;
            //Range rng1 = adoc.Range(ref start1, ref missing);
            tb.Add((Range)oRng, 2, 2, ref missing, ref missing);

            Table tbl1 = adoc.Tables[1];
            tbl1.Borders.Enable = 1;
            tbl1.Cell(1, 1).Range.Text = "Hi";
            tbl1.Cell(1, 2).Range.Text = "Hi";
            tbl1.Cell(2, 1).Range.Text = "Hi";
            tbl1.Cell(2, 2).Range.Text = "Hi";


            object oRng2 = adoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            objPara3 = adoc.Content.Paragraphs.Add(ref oRng2);
            objPara3.Range.Text = "hello2"; //add some text in paragraph
            objPara3.Format.SpaceAfter = 10; //defind some style
            objPara3.Range.InsertParagraphAfter(); //insert paragraph


            object strt = tbl1.Range.End;
            //Range rng2 = adoc.Range(ref strt, ref strt);
            //rng1.Copy();
            Table tbl2 = tb.Add((Range)oRng2, 2, 2, ref missing, ref missing);
            tbl2.Borders.Enable = 1;
            tbl2.Cell(1, 1).Range.Text = "Second table";
            tbl2.Cell(1, 2).Range.Text = "Second table";
            tbl2.Cell(2, 1).Range.Text = "Second table";
            tbl2.Cell(2, 2).Range.Text = "Second table";
            try
            {
                object filename = @"C:\temp\MyWord.doc";
                adoc.SaveAs(ref filename, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing, ref missing);

            }
            catch
            {
                // ignored
            }
        }
        //public void ConvertTables()
        //{
        //    //var wordApplication = (Microsoft.Office.Interop.Word.Application)Marshal.GetActiveObject("Word.Application");


        //    string messageAlert = "";

        //    Application curApp = Globals.ThisAddIn.Application;

        //    Word.Document curDoc = curApp.ActiveDocument;
        //    if (curDoc.Tables.Count > 0)
        //    {

        //        Excel.Application xlApp = new Excel.Application();

        //        //Used for debugging.
        //        //xlApp.Visible = true;

        //        //Call the conversion tool
        //        for (int i = 1; i <= curDoc.Tables.Count; i++)
        //        {
        //            Word.Table tbl = curDoc.Tables[i];
        //            Word.Range tblLoc = tbl.Range;

        //            if (xlApp == null)
        //            {
        //                messageAlert = "Excel could not be started.  Check that your office installation and project references are correct.";
        //                break;
        //            }

        //            Excel.Workbook wb = xlApp.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
        //            Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets[1];


        //            if (ws == null)
        //            {
        //                messageAlert = "Worksheet could not be created.  Check that your office installation and project reference are correct.";
        //                break;
        //            }

        //            Range rng = tbl.ConvertToText(Separator: ";", NestedTables: false);

        //            string sData = rng.Text;



        //            string[] rows = sData.Split('\r');

        //            int r = 1, c = 1;
        //            int numRows = rows.Count();
        //            int numCols = rows[0].Split(';').Count();


        //            foreach (string row in rows)
        //            {
        //                string[] cells = row.Split(';');
        //                foreach (string cell in cells)
        //                {
        //                    ws.Cells[r, c].Value = cell;
        //                    c += 1;
        //                }
        //                r += 1;
        //                c = 1;
        //            }

        //            ws.SaveAs("C:\\temp\\test.xlsx");
        //            rng.Text = "";
        //            rng.InlineShapes.AddOLEObject(ClassType: "Excel.Sheet.12", FileName: "C:\\temp\\test.xlsx");


        //            ws.Range["A1", ws.Cells[numRows, numCols]].Value = "";
        //            ws.SaveAs("C:\\Temp\\test.xlsx");
        //        }
        //        xlApp.Quit();

        //        messageAlert = "Tables converted";
        //    }
        //    else
        //    {
        //        // No tables found
        //        messageAlert = "No tables found within the document";
        //    }


        //    //MessageBox.Show(messageAlert);

        //}


    }
}
