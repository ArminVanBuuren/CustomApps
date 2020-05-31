using System.Collections.Generic;
using System.Linq;

namespace LogsReader.Reader
{
    public class TraceReaderStartWithEndWith : TraceReader
    {
	    public TraceReaderStartWithEndWith(LogsReaderControl control, string server, string filePath, string originalFolder)
	        : base(control, server, filePath, originalFolder) { }

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

		    if (!IsMatched(line))
			    return;

		    Commit();


		    Found = new DataTemplate(this, Lines, line, CurrentTransactionValue);
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

		    // сразу очищаем прошлые данные, т.к. дальнейший поиск по транзакциям не будет выполняеться
			if (!SearchByTransaction)
			    PastTraceLines.Clear();
	    }
    }
}
