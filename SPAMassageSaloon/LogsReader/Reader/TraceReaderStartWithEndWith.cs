using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogsReader.Reader
{
    public class TraceReaderStartWithEndWith : TraceReader
    {
        public Regex StartTraceWith => CurrentSettings.TraceParse.StartTraceWith;
        public Regex EndTraceWith => CurrentSettings.TraceParse.EndTraceWith;

        public TraceReaderStartWithEndWith(string server, string filePath, string originalFolder, LogsReaderControl mainReader)
	        : base(server, filePath, originalFolder, mainReader) { }

        public override void ReadLine(string line)
        {
            Lines++;

            if (Found != null)
            {
                // если стек лога превышает допустимый размер, то лог больше не дополняется
                if (Found.CountOfLines >= MaxTraceLines)
                {
                    Commit();
                }
                else
                {
                    Found.AppendNextLine(line);
                    if (EndTraceWith.IsMatch(line))
                    {
                        Commit();
                    }
                    return;
                }
            }

            if (!IsMatchSearchPatternFunc.Invoke(line))
            {
                PastTraceLines.Enqueue(line);
                if (PastTraceLines.Count > MaxTraceLines)
                    PastTraceLines.Dequeue();
                return;
            }

            Commit();


            Found = new DataTemplate(this, Lines, line);
            if (!StartTraceWith.IsMatch(Found.TraceMessage))
            {
                // Попытки спарсить текущую строку вместе с сохраненными предыдущими строками лога
                var revercePastTraceLines = new Queue<string>(PastTraceLines.Reverse());
                while (Found.CountOfLines < MaxTraceLines && revercePastTraceLines.Count > 0)
                {
                    Found.AppendPastLine(revercePastTraceLines.Dequeue());
                    if (StartTraceWith.IsMatch(Found.TraceMessage))
                        break;
                }
            }

            PastTraceLines.Clear();
        }
    }
}
