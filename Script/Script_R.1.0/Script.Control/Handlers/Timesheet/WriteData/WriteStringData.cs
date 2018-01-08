using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XPackage;

namespace Script.Control.Handlers.Timesheet.WriteData
{
    public class WriteStringData
    {
        private string Path { get; }
        private int countWrites = -1;
        public WriteStringData(string path)
        {
            Path = path;
        }
        /// <summary>
        /// Добавить заголовок для текстового документа
        /// </summary>
        /// <param name="lines"></param>
        public void AddDocHeader(string[] lines)
        {
            string[] strNew = GetEmptyArray(lines.Length + 2);
            Array.Copy(lines, 0, strNew, 1, lines.Length);
            WriteLines(strNew);
        }
        /// <summary>
        /// При создании таблицы - добавить заголовок таблицы
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="maxLengthSplitter"></param>
        public void AddTableHeader(string lines, int maxLengthSplitter)
        {
            string[] strNew = GetEmptyArray(5);
            strNew[2] = string.Join("", new string('=', maxLengthSplitter));
            strNew[3] = string.Format("{1,-%STEPS%}|{0}|".Replace("%STEPS%", ((maxLengthSplitter - lines.Length) / 2 - 1).ToString()), lines, " "); // string.Join("", new string('/', 70))
            strNew[4] = string.Join("", new string('=', maxLengthSplitter));
            WriteLines(strNew);
        }
        /// <summary>
        /// добавить текстовые названия колонок
        /// </summary>
        /// <param name="columnsName"></param>
        /// <param name="maxLenghSplitterByColumns"></param>
        public void AddColumnsName(string[] columnsName, int[] maxLenghSplitterByColumns)
        {
            string result = string.Empty;
            for (int i = 0; i < columnsName.Length; i++)
            {
                int startSteps = 0;
                int endSteps = 0;
                if (maxLenghSplitterByColumns[i] > columnsName[i].Length)
                {
                    startSteps = (maxLenghSplitterByColumns[i] - columnsName[i].Length) / 2;
                    endSteps = maxLenghSplitterByColumns[i] - startSteps - columnsName[i].Length;
                }
                result = result + string.Format("{1,-%STEPSTART%}{0}{1,-%STEPSEND%}|"
                                                        .Replace("%STEPSTART%", startSteps.ToString()).Replace("%STEPSEND%", endSteps.ToString()), columnsName[i], " ");
            }
            WriteLines(new[] { result });
        }
        /// <summary>
        /// Добавить нижний олонтикул тектовой таблицы
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="maxLengthSplitter"></param>
        public void AddTableFooter(string[] lines, int maxLengthSplitter)
        {
            string[] strNew = GetEmptyArray(lines.Length + 2);
            Array.Copy(lines, 0, strNew, 1, lines.Length);
            strNew[0] = string.Join("", new string('=', maxLengthSplitter));
            strNew[strNew.Length - 1] = string.Join("", new string('=', maxLengthSplitter));
            WriteLines(strNew);
        }

        /// <summary>
        /// добавить в одной ячейке несколько строк
        /// </summary>
        /// <param name="input">количество строк</param>
        /// <param name="maxLengthSplitter">длинаа всей ячейки</param>
        /// <returns></returns>
        public string GetRows(string[] input, int maxLengthSplitter)
        {
            //int maxLength = input.Max(x => x.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Max(y => y.Length));
            string result = string.Empty;
            int i = 0;
            foreach (string row in input)
            {
                i++;
                if (input.Length > i)
                    result = result + string.Format("{0}{2}{1}{2}", row, string.Join("", new string('-', maxLengthSplitter)), Environment.NewLine);
                else
                    result = result + row;
            }
            return result.Trim();
        }
        /// <summary>
        /// добавить колонки
        /// </summary>
        /// <param name="input">массив колонок с значениями</param>
        /// <param name="maxLenghSplitterByColumns">массив длинн колонок </param>
        /// <returns></returns>
        public string GetColumns(string[] input, int[] maxLenghSplitterByColumns)
        {
            int maxLines = 0;
            List<List<string>> table = new List<List<string>>();
            foreach (string cell in input)
            {
                List<string> cellNew = new List<string>();
                foreach (string line in cell.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    cellNew.Add(line);
                }
                if (cellNew.Count > maxLines)
                    maxLines = cellNew.Count;
                table.Add(cellNew);
            }

            string result = string.Format("{0}{1}", new string('-', maxLenghSplitterByColumns.Sum() + maxLenghSplitterByColumns.Length), Environment.NewLine);
            for (int i = 0; i < maxLines; i++)
            {
                //table.Max(x => x[i].Length);
                int indexOfColumn = 0;
                foreach (List<string> columnLines in table)
                {
                    string colimnLine = string.Empty;
                    if (columnLines.Count > i)
                        colimnLine = columnLines[i];
                    result = result + string.Format(("{0,-%STEPS%}|".Replace("%STEPS%", maxLenghSplitterByColumns[indexOfColumn].ToString())), colimnLine);
                    indexOfColumn++;
                }
                result = result + Environment.NewLine;
            }

            return result.Trim();
        }

        static string[] GetEmptyArray(int length)
        {
            string[] strNew = new string[length];
            for (int i = 0; i < strNew.Length; i++)
            {
                strNew[i] = string.Empty;
            }

            return strNew;
        }
        public void WriteLines(string[] lines)
        {
            countWrites++;
            using (StreamWriter outputWriter = new StreamWriter(Path, countWrites > 0, Functions.Enc))
            {
                foreach (string str in lines)
                {
                    outputWriter.WriteLine(str);
                }
            }
        }
    }

}
