using System;

namespace TFSGeneration.Control.ConditionEx.Utils
{
	public class Parameter : IDisposable
	{
		string _value = string.Empty;
		TypeParam _type;
		public Parameter(string input)
		{
			Source = input;
			GetValue(Source);
		}
		public string Source { get; }
		public string Value => _value;
		public TypeParam Type => _type;

		void GetValue(string input)
		{
			_type = GetTypeEx.GetType(input);
			if (_type == TypeParam.MathEx)
			{
				double strNum = StaffFunk.Evaluate(input, _type);
				_value = strNum.ToString();
				_type = TypeParam.Number;
			}
			else if (_type == TypeParam.Bool)
				_value = input.Trim();
			else
				_value = input;
		}

		public void Dispose()
		{

		}
	}
}