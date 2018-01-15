using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Protas.Components.PerformanceLog
{
    public interface ILog3NetMain
    {
        Log3Net MainLog3Net { get; }
        string TraceNamePostfix { get; }
        string TraceMessagePrefix { get; }
    }

    public class ShellLog3Net : ILog3NetMain
    {
        const string DefRoot = "UnknownRootTrace";
        const string DefTrace = "UnknownTrace";
        const string DefMeth = "UnknownMethod";
        const string DefInit = "Initialization";
        string _rootName = string.Empty;
        string _traceName = string.Empty;
        string _methodName = string.Empty;
        public ShellLog3Net()
        {

        }
        public ShellLog3Net(ILog3NetMain logInst)
        {
            if (logInst == null)
                return;
            MainLog3Net = logInst.MainLog3Net;
            TraceNamePostfix = logInst.TraceNamePostfix;
            TraceMessagePrefix = logInst.TraceMessagePrefix;
        }
        public void InitInstanceOnStatistic(object instance)
        {
            IObjectStatistic IsIObjectStat = instance as IObjectStatistic;
            if(IsIObjectStat != null)
            {
                MainLog3Net.AddStatisticInstance((IObjectStatistic)instance);
            }
        }
        public void TraceAddPostfix(string str)
        {
            TraceNamePostfix = string.Format("{0}{1}", TraceNamePostfix, str);
        }

        public void AddMessagePrefix(string str)
        {
            if (!string.IsNullOrEmpty(str))
                TraceMessagePrefix =  string.Format("{1}[{0}]:", str, TraceMessagePrefix);
        }

        public Log3Net MainLog3Net { get; }
        public string RootName => _rootName;
        public string TraceName
        {
            get
            {
                if (string.IsNullOrEmpty(TraceNamePostfix) && !string.IsNullOrEmpty(_traceName))
                    return _traceName;
                if (!string.IsNullOrEmpty(TraceNamePostfix) && string.IsNullOrEmpty(_traceName))
                    return TraceNamePostfix;
                return string.Format("{0}.{1}", _traceName, TraceNamePostfix);
            }
        }
        public string MethodName => _methodName;
        public string TraceNamePostfix { get; private set; }
        public string TraceMessagePrefix { get; private set; }

        int _countExeptions = 0;


        public void AddLog(Log3NetSeverity severity, object message)
        {
            try
            {
                if (MainLog3Net == null || MainLog3Net.LogSeverity == Log3NetSeverity.Disable || MainLog3Net.LogSeverity.GetHashCode() < severity.GetHashCode())
                    return;
                GetSettings();
                MainLog3Net.AddLog(severity, RootName, TraceName, MethodName, string.Format("{0}{1}", TraceMessagePrefix, message));
            }
            catch (Exception ex)
            {
                _countExeptions++;
                if (_countExeptions > 10)
                    Log3Net.AddEventLog(true, ex);
            }
        }

        public void AddEx(Exception exception)
        {
            try
            {
                if (MainLog3Net == null || MainLog3Net.LogSeverity == Log3NetSeverity.Disable || MainLog3Net.LogSeverity.GetHashCode() < Log3NetSeverity.Error.GetHashCode())
                    return;
                GetSettings();

                MainLog3Net.AddLog(Log3NetSeverity.Error, RootName, TraceName, MethodName, string.Format("Exception! Message=\"{0}\"\r\nData=\"{1}\"\r\nStackTrace=\"{2}\"", exception.Message, exception.Data, exception.StackTrace));
            }
            catch (Exception ex)
            {
                _countExeptions++;
                if (_countExeptions > 10)
                    Log3Net.AddEventLog(true, ex);
            }
        }

        public void AddLogObj(Log3NetSeverity severity, string stringFormat, params object[] objects)
        {
            try
            {
                if (MainLog3Net == null || MainLog3Net.LogSeverity == Log3NetSeverity.Disable || MainLog3Net.LogSeverity.GetHashCode() < severity.GetHashCode())
                    return;
                GetSettings();

                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i] == null)
                        objects[i] = "\"{NuLL}\"";
                    else if (objects[i].Equals(typeof(string)) || objects[i].Equals(typeof(int)) || objects[i].Equals(typeof(bool)))
                        continue;
                    else if (objects[i].Equals(typeof(IList)) || objects[i].Equals(typeof(ICollection)) || objects[i].Equals(typeof(Array)))
                        objects[i] = string.Format("\"{0}({1})\"", objects[i].GetType().Name, string.Join(",", (CollectionBase)objects[i]));
                    else if (objects[i].Equals(typeof(Type)))
                        objects[i] = string.Format("\"{0}\"", objects[i]);
                    else
                        objects[i] = string.Format("\"{0}({1})\"", objects[i].GetType().Name, objects[i]);
                }

                AddLogByStringFormat(severity, stringFormat, objects);

            }
            catch (Exception ex)
            {
                _countExeptions++;
                if (_countExeptions > 10)
                    Log3Net.AddEventLog(true, ex);
            }
        }
        public void AddLogForm(Log3NetSeverity severity, string stringFormat, params object[] objects)
        {
            try
            {
                if (MainLog3Net == null || MainLog3Net.LogSeverity == Log3NetSeverity.Disable || MainLog3Net.LogSeverity.GetHashCode() < severity.GetHashCode())
                    return;
                GetSettings();

                AddLogByStringFormat(severity, stringFormat, objects);
            }
            catch (Exception ex)
            {
                _countExeptions++;
                if (_countExeptions > 10)
                    Log3Net.AddEventLog(true, ex);
            }
        }

        void AddLogByStringFormat(Log3NetSeverity severity, string stringFormat, params object[] objects)
        {
            try
            {
                string message = string.Format(stringFormat, objects);
                MainLog3Net.AddLog(severity, RootName, TraceName, MethodName, string.Format("{0}{1}", TraceMessagePrefix, message));
            }
            catch (FormatException)
            {
                MainLog3Net.AddLog(severity, RootName, TraceName, MethodName, string.Format("{0}Log Format Exception! | {1} | {2}", TraceMessagePrefix, stringFormat, string.Join(",", objects)));
            }
        }


        void GetSettings()
        {
            try
            {
                MethodBase mBase = new StackTrace().GetFrame(2).GetMethod();
                if (mBase.DeclaringType != null)
                {
                    string temp = mBase.DeclaringType.FullName;
                    string[] strArr = temp.Split('.');
                    if (strArr.Length <= 1)
                    {
                        _rootName = _traceName = temp;
                    }
                    else
                    {
                        _rootName = string.Join(".", strArr.Take(strArr.Length - 1));
                        _traceName = strArr[strArr.Length - 1];
                    }
                }
                _methodName = mBase.Name == ".ctor" ? DefInit : mBase.Name;
            }
            catch
            {
                _rootName = string.IsNullOrEmpty(_rootName) ? DefRoot : _rootName;
                _traceName = string.IsNullOrEmpty(_traceName) ? DefTrace : _traceName;
                _methodName = string.IsNullOrEmpty(_methodName) ? DefMeth : _methodName;
            }
        }


        public static class MemberInfoGetting
        {
            public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
            {
                MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
                return expressionBody.Member.Name;
            }
        }

    }
}
