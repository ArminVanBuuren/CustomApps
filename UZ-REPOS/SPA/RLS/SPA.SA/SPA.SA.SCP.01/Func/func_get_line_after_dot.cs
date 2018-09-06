using System;
using System.Xml;
using FORIS.ScenarioProcessing.Base.Utilities.Functions;

namespace FORIS.ScenarioProcessing.Utilities.Functions
{
    public class func_get_line_after_dot : IDictionaryFunction
    {
		/// <summary>
		/// ����� ������ ����� ����� ��� ExternalGroupNumber ������ ���
		/// </summary>
		/// <param name="args">���������. ������: func_get_line_after_dot(expression)</param>
		/// <returns>c</returns>
		public String Invoke(XmlDocument request, params object[] args)
        {
            string expression = args[0].ToString();

	        return expression.Contains(".") 
				? expression.Substring(expression.IndexOf(".", StringComparison.Ordinal) + 1) 
				: expression;
        }
    }
}
