using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utils
{
    public static class TYPES
    {
        [Flags]
        public enum TypeParam
        {
            NaN = 0,
            Null = 1,
            Number = 2,
            String = 4,
            Bool = 8,
            MathEx = 16,
            //FArray = 32
        }

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

        static readonly Regex _isTime = new Regex(@"^(([0-1]|)[0-9]|2[0-3])\:[0-5][0-9]((\:[0-5][0-9])|)$", RegexOptions.Compiled);
        public static bool IsTime(string input)
        {
            return _isTime.IsMatch(input);
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

        public static bool IsBool(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return string.Equals(input.Trim(), "true", StringComparison.CurrentCultureIgnoreCase) || string.Equals(input.Trim(), "false", StringComparison.CurrentCultureIgnoreCase);
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
