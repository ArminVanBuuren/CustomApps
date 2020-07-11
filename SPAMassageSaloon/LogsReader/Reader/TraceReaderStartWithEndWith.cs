using System.Collections.Generic;
using System.Linq;

namespace LogsReader.Reader
{
    public class TraceReaderStartWithEndWith : TraceReader
    {
	    public TraceReaderStartWithEndWith(LogsReaderPerformerBase control, string server, string filePath, string originalFolder)
	        : base(control, server, filePath, originalFolder) { }

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
				    Lines++;

					Found.AppendNextLine(line);
				    if (EndTraceLineWith.IsMatch(line))
				    {
					    Commit();
				    }
				    return;
			    }
		    }

		    AddLine(line);

			if (!IsMatched(line))
			    return;

		    Commit();


			Found = new DataTemplate(this, Lines, CurrentTransactionValue);
			// Попытки спарсить предыдущие сохраненные строки как начало трассировки
			var revercePastTraceLines = new Queue<string>(PastTraceLines.Reverse());
			while (Found.CountOfLines < MaxTraceLines && revercePastTraceLines.Count > 0)
			{
				var pastLine = revercePastTraceLines.Dequeue();
				Found.AppendPastLine(pastLine);
				if (StartTraceLineWith.IsMatch(pastLine))
					break;
			}

			if (SearchByTransaction)
				PastTraceLines = new Queue<string>(revercePastTraceLines.Reverse());
			else
				PastTraceLines.Clear(); // сразу очищаем прошлые данные, т.к. дальнейший поиск по транзакциям не будет выполняеться
		}
    }
}
