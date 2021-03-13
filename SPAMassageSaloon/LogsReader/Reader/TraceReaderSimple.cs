using System;
using System.Collections.Generic;
using System.Linq;

namespace LogsReader.Reader
{
	public class TraceReaderSimple : TraceReader
	{
		public TraceReaderSimple(LogsReaderPerformerBase control, string server, string filePath, string originalFolder)
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

							Lines++;
							return;
						}

						Found = null;
					}
					else if (!Found.IsMatched)
					{
						// Если предыдущий фрагмент лога не спарсился удачно, то выполняются новые попытки спарсить лог
						Found.AppendNextLine(line);
						if (IsTraceMatch(Found.TraceMessage, out var afterSuccessResult, Found))
						{
							// Паттерн успешно сработал и тепмлейт заменяется. И дальше продолжается проврерка на дополнение строк
							AddResult(afterSuccessResult);
							Found = afterSuccessResult;
							PastTraceLines.Clear();

							Lines++;
							return;
						}
					}
				}
			}

			if (!IsMatched(line))
			{
				AddLine(line);
				return;
			}

			Lines++;

			Commit();

			var isMatched = IsTraceMatch(line, out var found);
			Found = found;

			if (isMatched)
			{
				AddResult(Found);

				if (!SearchByTransaction)
					PastTraceLines.Clear(); // сразу очищаем прошлые данные, т.к. дальнейший поиск по транзакциям не будет выполняеться
			}
			else if (PastTraceLines.Count > 0)
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

				if (SearchByTransaction)
					PastTraceLines = new Queue<string>(revercePastTraceLines.Reverse());
				else
					PastTraceLines.Clear(); // сразу очищаем прошлые данные, т.к. дальнейший поиск по транзакциям не будет выполняеться
			}
		}
	}
}