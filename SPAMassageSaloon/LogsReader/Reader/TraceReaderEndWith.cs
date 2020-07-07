using System.Collections.Generic;
using System.Linq;

namespace LogsReader.Reader
{
    public class TraceReaderEndWith : TraceReader
    {
	    public TraceReaderEndWith(LogsReaderPerformerBase control, string server, string filePath, string originalFolder) 
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
				    Found.AppendNextLine(line);
				    if (EndTraceWith.IsMatch(line))
				    {
					    Commit();
				    }

				    AddLine(line);
					return;
			    }
		    }

		    AddLine(line);

			if (!IsMatched(line))
			    return;

		    Commit();


		    Found = new DataTemplate(this, Lines, line, CurrentTransactionValue);

		    var revercePastTraceLines = new Queue<string>(PastTraceLines.Reverse());
		    while (Found.CountOfLines < MaxTraceLines && revercePastTraceLines.Count > 0)
		    {
			    // Попытки спарсить текущую строку вместе с сохраненными предыдущими строками лога
			    if (EndTraceWith.IsMatch(Found.TraceMessage))
				    break;
			    Found.AppendPastLine(revercePastTraceLines.Dequeue());
		    }

			// сразу очищаем прошлые данные, т.к. дальнейший поиск по транзакциям не будет выполняеться
		    if (!SearchByTransaction)
			    PastTraceLines.Clear();
	    }
    }
}
