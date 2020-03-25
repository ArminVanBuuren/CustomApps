using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogsReader.Reader
{
    public class TraceReaderStartWith : TraceReader
    {
        protected sealed override int MaxTraceLines { get; }
        protected override Queue<string> PastTraceLines { get; }

        public Regex StartLineWith => CurrentSettings.TraceLinePattern.StartLineWith;

        //public Regex EndLineWith => CurrentSettings.TraceLinePattern.EndLineWith;

        public TraceReaderStartWith(string server, string filePath, LogsReader mainReader) : base(server, filePath, mainReader)
        {
            MaxTraceLines = 20000;
            PastTraceLines = new Queue<string>(MaxTraceLines);
        }

        public override void ReadLine(string line)
        {
            if (Found != null)
            {
                // если стек лога превышает допустимый размер, то лог больше не дополняется
                if (Found.CountOfLines >= MaxTraceLines)
                {
                    Commit();
                }
                else
                {
                    if (!StartLineWith.IsMatch(line))
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
                ++CountMatches;
                Commit();
            }


            Found = new DataTemplate(this, line);
            if (!StartLineWith.IsMatch(Found.EntireTrace))
            {
                // Попытки спарсить текущую строку вместе с сохраненными предыдущими строками лога
                var revercePastTraceLines = new Queue<string>(PastTraceLines.Reverse());
                while (Found.CountOfLines < MaxTraceLines && revercePastTraceLines.Count > 0)
                {
                    Found.AppendPastLine(revercePastTraceLines.Dequeue());
                    if (StartLineWith.IsMatch(Found.EntireTrace))
                        break;
                }
            }

            PastTraceLines.Clear();
        }
    }
}
