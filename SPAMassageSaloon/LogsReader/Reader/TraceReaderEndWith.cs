using System.Collections.Generic;
using System.Linq;

namespace LogsReader.Reader
{
	public class TraceReaderEndWith : TraceReader
	{
		public TraceReaderEndWith(LogsReaderPerformerBase control, string server, string filePath, string originalFolder)
			: base(control, server, filePath, originalFolder)
		{
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
					Lines++;

					Found.AppendNextLine(line);

					if (EndTraceLineWith.IsMatch(line))
						Commit();

					return;
				}
			}

			AddLine(line);
			if (!IsMatched(line))
				return;

			Commit();
			Found = new DataTemplate(this, Lines, CurrentTransactionValue);
			var revercePastTraceLines = new Queue<(long line, string input)>(PastTraceLines.Reverse());

			while (Found.CountOfLines < MaxTraceLines && revercePastTraceLines.Count > 0)
			{
				var pastLine = revercePastTraceLines.Peek();
				// Попытки спарсить сохраненные предыдущие строки как конец трассировки
				if (EndTraceLineWith.IsMatch(pastLine.input))
					break;

				Found.AppendPastLine(revercePastTraceLines.Dequeue().input);
			}

			if (SearchByTransaction)
				PastTraceLines = new Queue<(long line, string input)>(revercePastTraceLines.Reverse());
			else
				PastTraceLines.Clear(); // сразу очищаем прошлые данные, т.к. дальнейший поиск по транзакциям не будет выполняеться
		}
	}
}