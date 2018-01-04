using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace TFSGeneration.Control.ConditionEx.Utils
{
	public class StaffFunk
	{
		public static string[] GetStringByXPath(XPathNavigator navigator, string xpath)
		{
			string[] strOut = new string[] { string.Empty };
			try
			{
				if (navigator == null)
					return strOut;
				if (navigator.NameTable == null)
					return strOut;
				XPathExpression expression = XPathExpression.Compile(xpath);
				XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
				//manager.AddNamespace("bk", "http://www.contoso.com/books");
				manager.AddNamespace(string.Empty, "urn:samples");
				expression.SetContext(manager);
				switch (expression.ReturnType)
				{

					case XPathResultType.NodeSet:
						XPathNodeIterator nodes = navigator.Select(expression);
						if (nodes.Count > 0)
						{
							strOut = new string[nodes.Count];
							int i = 0;
							while (nodes.MoveNext())
							{
								strOut[i] = nodes.Current.ToString();
								i++;
							}
						}
						return strOut;

					case XPathResultType.Number:
					case XPathResultType.Boolean:
					case XPathResultType.String:
						strOut[0] = navigator.Evaluate(expression)?.ToString();
						return strOut;
				}
			}
			catch
			{
				// ignored
			}
			return strOut;
		}
		/// <summary>
		/// Direction = 0 Замена всех знаков из нормального вида, в формат что не попадает под эксепшн xml
		/// Direction = 1 Замена всех знаков из в формата что не попадает под эксепшн xml, в нормальный вид
		/// Direction = 2 Замена знаков 4 знаков знаков из нормального вида из атрибутов, в формат что не попадает под эксепшн xml
		/// </summary>
		/// <param name="str"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public static string ReplaceXmlSpecSymbls(string str, int direction)
		{
			Regex reg;
			XmlFunc xf;
			if (direction == 0)
			{
				reg = new Regex(@"\&(.+?)\;", RegexOptions.IgnoreCase);
				xf = new XmlFunc(0);
			}
			else
			{
				reg = new Regex(@"(.+?)", RegexOptions.IgnoreCase);
				xf = new XmlFunc(direction);
			}
			MatchEvaluator evaluator = (xf.Replace);
			string strOut = reg.Replace(str, evaluator);
			return strOut;
		}


		public static bool ValidateXmlDocument(string docPath)
		{
			XmlSchemaSet schemas = new XmlSchemaSet();
			XDocument doc = XDocument.Load(docPath);
			string msg = "";
			doc.Validate(schemas, (o, e) =>
			{
				msg += e.Message + Environment.NewLine;
			});
			return (msg == string.Empty);
		}

		/// <summary>
		/// Четное или нечетно число
		/// </summary>
		public static bool IsParity(int dbl)
		{
			int a = dbl;
			int b;
			b = a / 2;
			int c = b * 2;
			if (c == a)
				return true;
			return false;
		}
		public static string TrimString(string str)
		{
			return str.Trim('\r', '\n', '\t', ' ');
		}
		public static bool IsObjectsSimilar(string param1, string param2)
		{
			int i = param2.IndexOf(param1, StringComparison.CurrentCultureIgnoreCase);
			if (i == -1)
			{
				i = param1.IndexOf(param2, StringComparison.CurrentCultureIgnoreCase);
			}
			return i != -1;
		}
		/// <summary>
		/// Выполняет операции складивания, деление и т.д.
		/// </summary>
		public static double Evaluate(string expression)
		{
			if (GetTypeEx.IsMathExpression(expression))
				return Evaluate(expression, TypeParam.MathEx);
			return double.NaN;
		}

		public static double Evaluate(string expression, TypeParam pType)
		{
			try
			{
				if (pType != TypeParam.MathEx)
				{
					return double.NaN;
				}
				string exp = expression.Replace(",", ".");
				var table = new DataTable();
				table.Columns.Add("expression", typeof(string), exp);
				DataRow row = table.NewRow();
				table.Rows.Add(row);
				double result = double.Parse((string)row["expression"]);
				return result;
			}
			catch
			{
				return double.NaN;
			}
		}

		public static RegexOptions GetRegOptions(string propRegOpt)
		{
			RegexOptions ropt = RegexOptions.None;
			int i = 0;
			foreach (string opt in propRegOpt.Split('|'))
			{
				RegexOptions rpt = GetRegOptionsEnum(opt);
				if (rpt == RegexOptions.None && i == 0)
					ropt = rpt;
				else if (rpt != RegexOptions.None)
					ropt |= rpt;
				i = 0;
			}
			return ropt;
		}
		static RegexOptions GetRegOptionsEnum(string str)
		{
			switch (str.ToLower().Trim())
			{
				case "multiline": return RegexOptions.Multiline;
				case "ignorecase": return RegexOptions.IgnoreCase;
				case "singleline": return RegexOptions.Singleline;
				case "righttoleft": return RegexOptions.RightToLeft;
				default: return RegexOptions.None;
			}
		}






	}

	public class ProtasActivator
	{
		class ParamsList
		{
			public List<object> Params { get; }
			public ParamsList(List<object> inputParams)
			{
				Params = inputParams;
			}
		}
		public static object CreateInstance(Type product, List<object> constructor)
		{
			List<ParamsList> availConstructors = new List<ParamsList>();

			foreach (ConstructorInfo info in product.GetConstructors())
			{
				int cntParams = 0;
				List<object> constructorParams = new List<object>();

				//проверяем типы входных параметров конструктора
				foreach (ParameterInfo paramInfo in info.GetParameters())
				{
					cntParams++;

					foreach (var param in constructor)
					{
						if (paramInfo.ParameterType.Equals(param.GetType()) && !constructorParams.Contains(param))
						{
							constructorParams.Add(param);
							break;
						}
					}
				}

				if (cntParams != constructorParams.Count)
					continue;
				availConstructors.Add(new ParamsList(constructorParams));
			}

			if (availConstructors.Count == 0)
				return Activator.CreateInstance(product);

			ParamsList constr = availConstructors.OrderBy(c => c.Params.Count).ToList()[0];
			return Activator.CreateInstance(product, constr.Params);



			//string _path = pp.FolderPath;
			//AppDomainSetup appDomainInfo = new AppDomainSetup();
			//appDomainInfo.ApplicationBase = _path;
			//appDomainInfo.PrivateBinPath = _path;
			//appDomainInfo.PrivateBinPathProbe = _path;
			//AppDomain domain = AppDomain.CreateDomain(TraceName, null, appDomainInfo);
			//IResourceContext context = domain.CreateInstanceFromAndUnwrap(pp.FullPath, className) as IResourceContext;
			//if (context == null)
			//{
			//    AddLogForm(Log3NetSeverity.Error, "Context Not Finded On Path = \"{0}:{1}\"", pp.FullPath, className);
			//    continue;
			//}
			//if (!context.Initialize(config))
			//{
			//    AddLog(Log3NetSeverity.Error, "Fail Inittialize Context");
			//    continue;
			//}

		}
	}


	//Класс для упрощения вывода пути файла, директории и полного пути к файлу или каталогу
	public class PathProperty
	{
		string InputPath { get; }
		public PathProperty(string input)
		{
			InputPath = input;
			FullPath = Path.GetFullPath(InputPath);
			FolderPath = Path.GetDirectoryName(FullPath);
			FileName = Path.GetFileName(InputPath);
		}
		public string FullPath { get; }
		public string FolderPath { get; }
		public string FileName { get; }

		public bool Exists
		{
			get
			{
				if (!string.IsNullOrEmpty(FileName))
					return File.Exists(FullPath);
				return Directory.Exists(FullPath);
			}
		}

		public FileAttributes Attributes
		{
			get
			{
				if (!string.IsNullOrEmpty(FullPath))
				{
					return File.GetAttributes(FullPath);
				}
				return FileAttributes.Offline;
			}
		}

	}
	public class XString
	{
		//public static string Format(string format, string[] stringParams, params object[] paramCollection)
		//{
		//    string stringFormatparams = GetParasFormat(stringParams, paramCollection.Length);
		//    object[] newParams = CheckParamNull(paramCollection);
		//    try
		//    {
		//        return string.Format(format, string.Format(stringFormatparams, newParams));
		//    }
		//    catch
		//    {
		//        return string.Format("Log Format Exception! Format=\"{0}\" {1}", format, string.Format(stringFormatparams, newParams));
		//    }
		//}

		public static string Format(string format, params object[] paramCollection)
		{
			object[] newParams = CheckParamNull(paramCollection);
			try
			{
				return string.Format(format, newParams);
			}
			catch
			{
				return string.Format("Log Format Exception! Format=\"{0}\" Params=\"{1}\"", format, string.Join(";", newParams));
			}
		}
		public static string Format(string[] stringParams, params object[] paramCollection)
		{
			return string.Format(string.Join(" ", GetParasFormat(stringParams, paramCollection.Length)), CheckParamNull(paramCollection));
		}

		static string GetParasFormat(string[] stringParams, int count)
		{
			string[] correctStrings = new string[count];
			for (int i = 0; i < count; i++)
			{
				if (string.IsNullOrEmpty(stringParams[i]))
					correctStrings[i] = "{" + i + "}";
				else
					correctStrings[i] = stringParams[i] + "=\"{" + i + "}\"";
			}
			return string.Join(" ", correctStrings);
		}
		static object[] CheckParamNull(object[] paramCollection)
		{
			for (int i = 0; i < paramCollection.Length; i++)
			{
				if (paramCollection[i] == null)
					paramCollection[i] = "{Null}";
			}
			return paramCollection;
		}

	}



}
