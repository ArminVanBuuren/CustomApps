using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

    public static class TYPES
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
}
