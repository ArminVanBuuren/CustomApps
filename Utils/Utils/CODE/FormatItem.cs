using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Utils
{
	internal class FormatItem<TIn>
	{
		public Func<TIn, string> Result { get; protected set; }
	}

	internal class FormatItemString<TIn> : FormatItem<TIn>
	{
		private readonly StringBuilder _source;

		public FormatItemString(string source, Func<TIn, string, string> getValue)
		{
			_source = new StringBuilder(source);
			Result = match => getValue(match, _source.ToString());
		}

		public void AppendString(string input)
		{
			_source.Append(input);
		}
	}

	internal class FormatItemFunc<TIn> : FormatItem<TIn>
	{
		private IReadOnlyList<string> Args { get; }

		public FormatItemFunc(Func<string[], string> func, IList<string> args, Func<TIn, string, string> getValue)
		{
			Args = new ReadOnlyCollection<string>(args);
			Result = match =>
			{
				var argsValue = new string[Args.Count];
				for (var i = 0; i < Args.Count; i++)
					argsValue[i] = getValue(match, Args[i]);
				return getValue(match, func.Invoke(argsValue));
			};
		}
	}
}
