using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Utils.ConditionEx.Utils
{
	public enum TypeParam
	{
		Null = 0,
		Number = 1,
		String = 2,
		Bool = 3,
		MathEx = 4,
		//FArray = 5
	}
	public class GetTypeEx
	{
		public static TypeParam GetType(string input)
		{
			if (string.IsNullOrEmpty(input))
				return TypeParam.Null;
			if (IsBool(input))
				return TypeParam.Bool;
			if (IsNumber(input))
				return TypeParam.Number;
			if (IsMathExpression(input))
				return TypeParam.MathEx;
			//if (FBundle.IsFArray(str)) 
			//return ParamType.FArray;
			return TypeParam.String;
		}
		public static bool IsBool(string input)
		{
			if (string.IsNullOrEmpty(input))
				return false;
			if (string.Equals(input.Trim(), "true", StringComparison.CurrentCultureIgnoreCase)
				|| string.Equals(input.Trim(), "false", StringComparison.CurrentCultureIgnoreCase))
				return true;
			return false;
		}
		public static bool IsNumber(string input)
		{
			if (string.IsNullOrEmpty(input))
				return false;
			char[] charIn = input.ToCharArray(0, input.Length);
			int i = 0, j = 0;
			foreach (char ch in charIn)
			{
				if (ch == ' ')
					continue;
				if (!char.IsNumber(ch))
				{
					if ((ch == '.' || ch == ',') && i == 0)
						i++;
					else if (ch == '-' && j == 0)
					{
						//пропуск
					}
					else
						return false;
				}
				j++;
			}
			return true;
		}

		public static bool IsMathExpression(string input)
		{
			if (string.IsNullOrEmpty(input))
				return false;
			StringBuilder temp = new StringBuilder();
			char[] charIn = input.ToCharArray(0, input.Length);
			int nextIsNum = 0;
			foreach (char ch in charIn)
			{
				if (ch == ' ')
					continue;
				if ((ch == '+' || ch == '-' || ch == '*' || ch == '/') && nextIsNum == 0)
				{
					if (!IsNumber(temp.ToString()))
						return false;
					temp.Remove(0, temp.Length);
					nextIsNum++;
				}
				else
				{
					temp.Append(ch);
					nextIsNum = 0;
				}
			}
			return IsNumber(temp.ToString());
		}

		public static bool IsFArray(string input)
		{
			string[] sArr = input.Split('\n');
			if (sArr.Length == 0) return false;
			foreach (string st in sArr)
			{
				string[] sArrInLine = st.Split('|');
				if (sArrInLine.Length == 3)
				{
					if (!IsNumber(sArrInLine[0]))
						return false;
				}
				else
					return false;
			}
			return true;
		}

		static Regex _isTime = new Regex(@"^(([0-1]|)[0-9]|2[0-3])\:[0-5][0-9]((\:[0-5][0-9])|)$");
		public static bool IsTime(string input)
		{
			return _isTime.IsMatch(input);
		}
	}
}
