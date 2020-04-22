using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogsReader.Reader
{
    public class TraceReaderStartWith : TraceReader
    {
        public Regex StartTraceWith => CurrentSettings.TraceParse.StartTraceWith;

        public TraceReaderStartWith(string server, string filePath, LogsReaderPerformer mainReader) : base(server, filePath, mainReader) { }

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
                    if (!StartTraceWith.IsMatch(line))
                    {
                        Found.AppendNextLine(line);
                        return;
                    }
                    else
                    {
                        Commit();
                    }
                }
            }

            if (!IsMatchSearchPatternFunc.Invoke(line))
            {
                PastTraceLines.Enqueue(line);
                if (PastTraceLines.Count > MaxTraceLines)
                    PastTraceLines.Dequeue();
                return;
            }
            else
            {
                Commit();
            }


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
