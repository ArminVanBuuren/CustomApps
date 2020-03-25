using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogsReader.Reader
{
    public class TraceReaderSimple : TraceReader
    {
        protected sealed override int MaxTraceLines { get; }
        protected override Queue<string> PastTraceLines { get; }

        public TraceReaderSimple(string server, string filePath, LogsReader mainReader) : base(server, filePath, mainReader)
        {
            MaxTraceLines = CurrentSettings.MaxTraceLines;
            PastTraceLines = new Queue<string>(MaxTraceLines);
        }

        public override void ReadLine(string line)
        {
            if (Found != null)
            {
                // если стек лога превышает допустимый размер, то лог больше не дополняется
                if (FoundStackLines >= CurrentSettings.MaxTraceLines)
                {
                    if (!Found.IsMatched)
                        FoundResults.Add(Found);
                    FoundStackLines = 0;
                    Found = null;
                }
                else
                {
                    if (Found.IsMatched)
                    {
                        var appendedToEntireMessage = Found.EntireMessage + Environment.NewLine + line;
                        // Eсли строка не совпадает с паттерном строки, то текущая строка лога относится к предыдущему успешно спарсеному.
                        // Иначе строка относится к другому логу и завершается дополнение
                        if (IsMatch(line) == null && IsLineMatch(appendedToEntireMessage, out var newResult))
                        {
                            FoundStackLines++;
                            Found.MergeDataTemplates(newResult);
                            return;
                        }
                        else
                        {
                            FoundStackLines = 0;
                            Found = null;
                        }
                    }
                    else if (!Found.IsMatched)
                    {
                        // Если предыдущий фрагмент лога не спарсился удачано, то выполняются новые попытки спарсить лог
                        FoundStackLines++;
                        Found.AppendMessageAfter(Environment.NewLine + line);
                        if (IsLineMatch(Found.EntireMessage, out var afterSuccessResult))
                        {
                            // Паттерн успешно сработал и тепмлейт заменяется. И дальше продолжается проврерка на дополнение строк
                            FoundResults.Add(afterSuccessResult);
                            Found = afterSuccessResult;
                            PastTraceLines.Clear();
                            return;
                        }
                    }
                }
            }

            if (!IsMatchSearchPatternFunc.Invoke(line))
            {
                PastTraceLines.Enqueue(line);
                if (PastTraceLines.Count > CurrentSettings.MaxTraceLines)
                    PastTraceLines.Dequeue();
                return;
            }
            else
            {
                ++NumberOfFound;
                FoundStackLines = 1;
                Commit();
            }

            var isMatched = IsLineMatch(line, out var found);
            Found = found;

            if (isMatched)
            {
                FoundResults.Add(Found);
            }
            else
            {
                // Попытки спарсить текущую строку вместе с сохраненными предыдущими строками лога
                var reverceBeforeTraceLines = new Queue<string>(PastTraceLines.Reverse());
                while (FoundStackLines < CurrentSettings.MaxTraceLines && reverceBeforeTraceLines.Count > 0)
                {
                    FoundStackLines++;
                    Found.AppendMessageBefore(reverceBeforeTraceLines.Dequeue() + Environment.NewLine);

                    if (IsLineMatch(Found.EntireMessage, out var beforeResult))
                    {
                        Found = beforeResult;
                        FoundResults.Add(Found);
                        break;
                    }
                }
            }

            PastTraceLines.Clear();
        }
    }
}
