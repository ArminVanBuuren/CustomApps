using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace Utils
{
	[Flags]
	public enum TypeParam
	{
		NaN = 0,
		Null = 1,
		Number = 2,
		String = 4,
		Bool = 8,
		MathEx = 16
	}

	public static class CODE
	{
		public static TypeParam GetTypeParam(string input)
		{
			if (string.IsNullOrEmpty(input))
				return TypeParam.Null;
			if (BOOLEAN.IsBool(input))
				return TypeParam.Bool;
			if (NUMERIC.IsNumber(input))
				return TypeParam.Number;
			return MATH.IsMathExpression(input) ? TypeParam.MathEx : TypeParam.String;
		}

		class ParamsList
		{
			public List<object> Params { get; }

			public ParamsList(List<object> inputParams)
			{
				Params = inputParams;
			}
		}

		public static object CreateNewHandler(string assembly, string className, List<object> constructor)
		{
			GetType(assembly, className, out _, out var classType);

			return CreateInstance(classType, constructor);
		}

		/// <summary>
		/// В данном случае универсальный тип T должен быть базовым классом или интерфейсом, от которого наследуется класс из библиотеки
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="assembly"></param>
		/// <param name="className"></param>
		/// <param name="constructor"></param>
		/// <returns></returns>
		public static T CreateNewHandler<T>(string assembly, string className, List<object> constructor)
		{
			GetType(assembly, className, out var pluginAssembly, out var classType);

			var potentialContext = CreateInstance(classType, constructor);

			if (potentialContext == null)
				throw new Exception($"Assembly=[{pluginAssembly.ManifestModule.Name}]; Class=[{className}] not initialized!");

			if (!(potentialContext is T context))
				throw new Exception(
					$"Assembly=[{pluginAssembly.ManifestModule.Name}]; Type of class=[{className}] must be inherited from ScriptTemplate, Current=[{potentialContext.GetType()}]");

			return context;
		}

		static void GetType(string assembly, string className, out Assembly pluginAssembly, out Type classType)
		{
			var verifyAssembly = assembly;
			if (!File.Exists(verifyAssembly))
				throw new Exception($"Assembly=[{verifyAssembly}] not found!");

			pluginAssembly = Assembly.LoadFrom(verifyAssembly);
			classType = pluginAssembly.GetType(className);
			if (classType == null)
			{
				var @namespace = className.IndexOf(".", StringComparison.Ordinal) != -1 ? className.Substring(0, className.LastIndexOf(".", StringComparison.Ordinal)) : className;
				if (!GetTypeByNamespace(pluginAssembly, @namespace, className, out classType))
					throw new Exception($"Assembly=[{verifyAssembly}] doesn't have '{className}'");
			}
		}

		static bool GetTypeByNamespace(Assembly pluginAssembly, string @namespace, string className, out Type tp)
		{
			tp = null;
			var listTypesByNamespace = pluginAssembly.GetTypes().Where(t => string.Equals(t.Namespace, @namespace, StringComparison.InvariantCultureIgnoreCase)).ToList();
			var findedType = listTypesByNamespace.Where(x => x.FullName != null && x.FullName.Equals(className, StringComparison.InvariantCultureIgnoreCase)).ToList();

			if (findedType.Count == 0)
				return false;

			if (findedType.Count > 1)
				throw new Exception(
					$"Assembly=[{pluginAssembly.ManifestModule.Name}]; Can't identify [{className}]. Because in namespace=[{@namespace}] more than one!\n{string.Join(Environment.NewLine, findedType)}");

			tp = findedType[0];
			return true;
		}

		public static object CreateInstance(Type product, List<object> constructor)
		{
			var availConstructors = new List<ParamsList>();

			foreach (var info in product.GetConstructors())
			{
				var cntParams = 0;
				var constructorParams = new List<object>();

				//проверяем типы входных параметров конструктора
				foreach (var paramInfo in info.GetParameters())
				{
					cntParams++;

					foreach (var param in constructor)
					{
						if (paramInfo.ParameterType == param.GetType() && !constructorParams.Contains(param))
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

			var constr = availConstructors.OrderBy(c => c.Params.Count).ToList()[0];
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

		public static Type CreateClass(string sourceCode, string typeClassName, IEnumerable<string> assemblies = null)
		{
			var provider = new CSharpCodeProvider();

			var parameters = new CompilerParameters
			{
				GenerateInMemory = true //, GenerateExecutable = true
			};
			parameters.ReferencedAssemblies.Add("System.dll");
			if (assemblies != null)
			{
				foreach (var assembly in assemblies)
				{
					parameters.ReferencedAssemblies.Add(assembly);
				}
			}

			var results = provider.CompileAssemblyFromSource(parameters, sourceCode);

			if (results.Errors.HasErrors)
			{
				var sb = new StringBuilder();
				foreach (CompilerError error in results.Errors)
					sb.AppendLine($"Error ({error.ErrorNumber}): {error.ErrorText}");
				throw new InvalidOperationException(sb.ToString());
			}

			return results.CompiledAssembly.GetType(typeClassName);
		}

		public static MethodInfo CreateMethod(string staticMethodCode, string methodName, IEnumerable<string> assemblies = null)
		{
			const string code = @"using System;
                                  namespace Utils
                                  {{
                                      public class CODE_SIMPLE_FUNCTION
                                      {{
                                          {0}
                                      }}
                                  }}";
			var simpleFunc = CreateClass(string.Format(code, staticMethodCode), "Utils.CODE_SIMPLE_FUNCTION", assemblies);
			return simpleFunc.GetMethod(methodName);
		}

		public static Func<TIn, string> Calculate<TIn>(
			string template,
			Dictionary<string, Func<string[], string>> customFuncs,
			Func<TIn, string, string> getValueFunc)
		{
			var result = new List<FormatItem<TIn>>();

			var builder = new StringBuilder(template.Length);
			var funcName = new StringBuilder();

			var finded = 0;
			var bktCount = 0;
			var argOpened = 0;

			Func<string[], string> currentFunc = null;
			var args = new List<string>();
			var argCurrent = new StringBuilder();

			foreach (var ch in template)
			{
				switch (finded)
				{
					case 0 when ch == '{':
						finded++;
						continue;
					// завершение считывания функции
					case 1 when ch == '}' && (bktCount == 0 || bktCount == 2):
					{
						finded--;

						if (currentFunc == null && (!customFuncs.TryGetValue(funcName.ToString().Trim(), out currentFunc) || currentFunc == null))
						{
							builder.Append('{');
							builder.Append(funcName);
							builder.Append(ch);
						}

						AppenStringResult(builder, result, getValueFunc);
						builder.Clear();

						if (currentFunc != null)
							result.Add(new FormatItemFunc<TIn>(currentFunc, args, getValueFunc));

						currentFunc = null;
						args = new List<string>();
						bktCount = 0;

						funcName.Clear();
						continue;
					}
					case 1:
					{
						// если заполнилось имя функции, то дальше должно идти перечисление аргументов
						if (ch == '(' && bktCount == 0)
						{
							// если функция найдена
							if (customFuncs.TryGetValue(funcName.ToString().Trim(), out currentFunc))
							{
								bktCount++;
							}
							else
							{
								finded = 0;
								builder.Append('{');
								builder.Append(funcName);
								builder.Append(ch);
								funcName.Clear();
								continue;
							}
						}

						// funcName используется для заполнения всей стройки внутри {}
						funcName.Append(ch);

						void ArgClosed()
						{
							if (argCurrent.Length == 0)
								return;

							var arg = argCurrent.ToString().Trim();
							args.Add(arg.Substring(1, arg.Length - 2));
							argCurrent.Clear();
							argOpened = 0;
						}

						switch (ch)
						{
							// если аргументы закончились, то устанавливаем bktCount = 2, чтобы не учитываеть открытие следующих скобок
							// аргументы все должны быть закрытыми
							case ')' when bktCount == 1 && argOpened >= 2:
								ArgClosed();
								bktCount++;
								continue;
							// если заполняются аргументы
							case '\'':
								argOpened++;
								break;
							case ',' when argOpened >= 2:
								ArgClosed();
								continue;
						}

						if (argOpened >= 1)
							argCurrent.Append(ch);

						continue;
					}
					default:
						builder.Append(ch);
						break;
				}
			}

			if (funcName.Length > 0)
				builder.Append(funcName);
			funcName.Clear();

			AppenStringResult(builder, result, getValueFunc);
			builder.Clear();

			if (result.Count == 1)
				return result.First().Result;

			return (match) =>
			{
				var builder2 = new StringBuilder();
				foreach (var func in result)
					builder2.Append(func.Result(match));

				return builder2.ToString();
			};
		}

		static void AppenStringResult<TIn>(StringBuilder builder, ICollection<FormatItem<TIn>> result, Func<TIn, string, string> getValue)
		{
			if (builder.Length == 0)
				return;

			if (result.Count == 0 || !(result.Last() is FormatItemString<TIn>))
				result.Add(new FormatItemString<TIn>(builder.ToString(), getValue));
			else
				((FormatItemString<TIn>) result.Last()).AppendString(builder.ToString());
		}
	}
}