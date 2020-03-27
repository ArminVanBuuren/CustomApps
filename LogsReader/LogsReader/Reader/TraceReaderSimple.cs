using System;
using System.Collections.Generic;
using System.Linq;

namespace LogsReader.Reader
{
    public class TraceReaderSimple : TraceReader
    {

        public TraceReaderSimple(string server, string filePath, LogsReaderPerformer mainReader) : base(server, filePath, mainReader)
        {

        }

        public override void ReadLine(string line)
        {
            Lines++;

            if (Found != null)
            {
                // если стек лога превышает допустимый размер, то лог больше не дополняется
                if (Found.CountOfLines >= MaxTraceLines)
                {
                    if (!Found.IsMatched)
                        AddResult(Found);
                    Found = null;
                }
                else
                {
                    if (Found.IsMatched)
                    {
                        var appendedToEntireTrace = Found.EntireTrace + Environment.NewLine + line;
                        // Eсли строка не совпадает с паттерном строки, то текущая строка лога относится к предыдущему успешно спарсеному.
                        // Иначе строка относится к другому логу и завершается дополнение
                        if (IsLineMatch(line) == null && IsTraceMatch(appendedToEntireTrace, out var newResult))
                        {
                            Found.MergeDataTemplates(newResult);
                            return;
                        }
                        else
                        {
                            Found = null;
                        }
                    }
                    else if (!Found.IsMatched)
                    {
                        // Если предыдущий фрагмент лога не спарсился удачано, то выполняются новые попытки спарсить лог
                        Found.AppendNextLine(line);
                        if (IsTraceMatch(Found.EntireTrace, out var afterSuccessResult, Found))
                        {
                            // Паттерн успешно сработал и тепмлейт заменяется. И дальше продолжается проврерка на дополнение строк
                            AddResult(afterSuccessResult);
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
                if (PastTraceLines.Count > MaxTraceLines)
                    PastTraceLines.Dequeue();
                return;
            }
            else
            {
                ++CountMatches;
                Commit();
            }

            var isMatched = IsTraceMatch(line, out var found);
            Found = found;

            if (isMatched)
            {
                AddResult(Found);
            }
            else
            {
                // Попытки спарсить текущую строку вместе с сохраненными предыдущими строками лога
                var revercePastTraceLines = new Queue<string>(PastTraceLines.Reverse());
                while (Found.CountOfLines < MaxTraceLines && revercePastTraceLines.Count > 0)
                {
                    Found.AppendPastLine(revercePastTraceLines.Dequeue());

                    if (IsTraceMatch(Found.EntireTrace, out var beforeResult))
                    {
                        Found = beforeResult;
                        AddResult(Found);
                        break;
                    }
                }
            }

            PastTraceLines.Clear();
        }
    }
}
