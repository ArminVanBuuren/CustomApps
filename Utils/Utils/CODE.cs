﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
            if (IsBool(input))
                return TypeParam.Bool;
            if (NUMBER.IsNumber(input))
                return TypeParam.Number;
            return MATH.IsMathExpression(input) ? TypeParam.MathEx : TypeParam.String;
        }

        static readonly Regex _isTime = new Regex(@"^(([0-1]|)[0-9]|2[0-3])\:[0-5][0-9]((\:[0-5][0-9])|)$", RegexOptions.Compiled);
        public static bool IsTime(string input)
        {
            return _isTime.IsMatch(input);
        }

        public static bool IsBool(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            var trimVal = input.Trim();
            return trimVal.Like("true") || trimVal.Like("false");
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
                throw new Exception($"Assembly=[{pluginAssembly.ManifestModule.Name}]; Type of class=[{className}] must be inherited from ScriptTemplate, Current=[{potentialContext.GetType()}]");

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
                throw new Exception($"Assembly=[{pluginAssembly.ManifestModule.Name}]; Can't identify [{className}]. Because in namespace=[{@namespace}] more than one!\n{string.Join(Environment.NewLine, findedType)}");

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

    }
}
