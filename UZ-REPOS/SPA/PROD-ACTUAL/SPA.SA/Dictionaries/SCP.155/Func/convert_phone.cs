using System;
using System.Xml;
using FORIS.ScenarioProcessing.Base.Utilities.Functions;
using System.Text.RegularExpressions;

namespace FORIS.ScenarioProcessing.Utilities.Functions
{
	public class convert_phone : IDictionaryFunction
	{
		/// <summary>
		///  Преобразование телефонного номера из
		/// [+38050]*******
		/// [38050]*******
		/// [050]*******
		/// [50]*******
		/// в телефонный номер [38050]*******
		/// </summary>
		/// <param name="phone">телефонный номер</param>
		public String Invoke(params object[] args)
		{
			string expression = args[0].ToString();

			return string.Concat("380", expression.Trim().Substring(expression.Length - 9));
		}
	}
}
