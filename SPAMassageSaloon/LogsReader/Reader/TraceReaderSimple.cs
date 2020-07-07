using System;
using System.Collections.Generic;
using System.Linq;

namespace LogsReader.Reader
{
    public class TraceReaderSimple : TraceReader
    {
	    public TraceReaderSimple(LogsReaderPerformerBase control, string server, string filePath, string originalFolder) 
	        : base(control, server, filePath, originalFolder) { }

        public override void ReadLine(string line)
        {
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
				        var appendedToTraceMessage = Found.TraceMessage + Environment.NewLine + line;
				        // Eсли строка не совпадает с паттерном строки, то текущая строка лога относится к предыдущему успешно спарсеному.
				        // Иначе строка относится к другому логу и завершается дополнение
				        if (!IsLineMatch(line) && IsTraceMatch(appendedToTraceMessage, out var newResult))
				        {
					        Found.MergeDataTemplates(newResult);

					        AddLine(line);
							return;
				        }

				        Found = null;
			        }
			        else if (!Found.IsMatched)
			        {
				        // Если предыдущий фрагмент лога не спарсился удачано, то выполняются новые попытки спарсить лог
				        Found.AppendNextLine(line);
				        if (IsTraceMatch(Found.TraceMessage, out var afterSuccessResult, Found))
				        {
					        // Паттерн успешно сработал и тепмлейт заменяется. И дальше продолжается проврерка на дополнение строк
					        AddResult(afterSuccessResult);
					        Found = afterSuccessResult;
					        PastTraceLines.Clear();

					        AddLine(line);
							return;
				        }
			        }
		        }
	        }

	        AddLine(line);

			if (!IsMatched(line))
		        return;

	        Commit();

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

			        if (IsTraceMatch(Found.TraceMessage, out var beforeResult))
			        {
				        Found = beforeResult;
				        AddResult(Found);
				        break;
			        }
		        }
	        }

	        // сразу очищаем прошлые данные, т.к. дальнейший поиск по транзакциям не будет выполняеться
			if (!SearchByTransaction)
		        PastTraceLines.Clear();
        }
    }
}
