using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Script.Control.Handlers;
using Script.Control.Handlers.Arguments;
using XPackage;

namespace Script.Control
{
    [Flags]
    public enum FitBackType
    {
        Ignore = 0,
        CancelOperation = 1,
        RollBack = 2
    }

    [Flags]
    public enum LogType
    {
        None = 0,
        Success = 1,
        Warning = 2,
        Error = 4,
        Exception = 8,
        Info = 16,
        Console = 32,
        Failure = Error | Exception,
        Normal = Success | Warning | Error | Exception,
        Debug = Success | Warning | Error | Exception | Info
    }
    public delegate void DelegAddLog(LogType type, ScriptTemplate sourceObj, string stringformat, params object[] objects);
    public class LogFill
    {
        public DelegAddLog Exec_AddLog { get; }
        public LogFill(string scriptName, LogType severity, DelegAddLog funcAddLog)
        {
            ScriptName = scriptName;
            Severity = severity;
            Exec_AddLog = funcAddLog;
        }
        public string ScriptName { get; }
        public LogType Severity { get; }
        public int NumOfLogRecord { get; set; } = 0;
        public int ScriptExceptionsCount { get; set; } = 0;
        public string LogPath { get; set; }
        public override string ToString()
        {
            return string.Format("ScriptName='{0}'; Severity='{1}'; LogPath='{2}'", ScriptName, Severity, LogPath);
        }
    }

    [IdentifierClass(typeof(ScriptTemplate), "Пишет только лог. Для корректной обработки должен быть всегда главным объектом в конфиге")]
    public class ScriptTemplate : XPack
    {
        private const string CONFIG_NODE = "Config";
        private static string NAMESPACE_OF_HANDLERS = string.Format("{0}.Handlers", typeof(ScriptTemplate).Namespace);

        internal LogFill LogShell { get; set; }

        [Identifier("Name", "Имя скрипта", "Если удалить аттрибут имя будет иметь вид ScriptTemplate_1")]
        string ScriptName => LogShell.ScriptName;

        [Identifier("Severity", "Установить уровень логирования.", LogType.Console, typeof(LogType))]
        LogType Severity => LogShell.Severity;

        [Identifier("Log_Path", "Путь для записи лога", "Если удалить этот аттрибут то создется локальная папка с названием скрипта")]
        string LogPath
        {
            get
            {
                string logDir = Regex.Match(LogShell.LogPath, @"^.+\\").Value.Trim('\\');
                if (!Directory.Exists(logDir))
                {
                    LogShell.NumOfLogRecord = 0;
                    Directory.CreateDirectory(logDir);
                }
                if (!File.Exists(LogShell.LogPath))
                    LogShell.NumOfLogRecord = 0;
                return LogShell.LogPath;
            }
            set { LogShell.LogPath = value; }
        }

        [Identifier("FitBack", "В случае ошибки при выполнении, что необходимо делать для отката", FitBackType.Ignore, typeof(FitBackType))]
        protected FitBackType FitBack { get; }

        public int NumOfLogRecord
        {
            get { return LogShell.NumOfLogRecord; }
            set { LogShell.NumOfLogRecord = value; }
        }
        public int ScriptExceptionsCount
        {
            get { return LogShell.ScriptExceptionsCount; }
            set { LogShell.ScriptExceptionsCount = value; }
        }

        public ScriptTemplate(string filePath)
        {
            DynamicFunction = GetResources;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            PerformChild(xmlDoc);
            Initializator();
        }

        public void Initializator()
        {
            Execute();
            foreach (ScriptTemplate child in ChildPacks)
            {
                child.Initializator();
            }
        }

        public virtual void Execute()
        {

        }

        ScriptTemplate(XPack parentPack, XmlNode node) : base(parentPack, node)
        {
            DynamicFunction = GetResources;
            if (!node.Name.Equals(GetType().Name, StringComparison.CurrentCultureIgnoreCase))
                return;


            string scriptName = Attributes[GetXMLAttributeName(nameof(ScriptName))];
            if (string.IsNullOrEmpty(scriptName))
                scriptName = string.Format("{0}_{1}", GetType().Name,  CurrentUniqueIndex);
            string severity = Attributes[GetXMLAttributeName(nameof(Severity))];
            LogType _severity = GetSeverity(severity);

            if (_severity == LogType.None)
            {
                LogShell = new LogFill(scriptName, _severity, null);
            }
            else
            {
                DelegAddLog addlog = TemplateAddLog;
                LogShell = new LogFill(scriptName, _severity, addlog);
                string logPath = Attributes[GetXMLAttributeName(nameof(LogPath))];
                if (!string.IsNullOrEmpty(logPath))
                    LogPath = logPath;
                else
                    LogPath = Path.Combine(BaseArguments.LocalPath, "LOG", string.Format("{0}_{1:yyyyMMddHHmm}.log", scriptName, DateTime.Now));
            }
        }

        protected ScriptTemplate(XPack parentPack, XmlNode node, LogFill logFill) : base(parentPack, node)
        {
            DynamicFunction = GetResources;
            LogShell = logFill;
        }


        public override XPack GetNewXPack(XPack parentPack, XmlNode node)
        {
            if (node.Name.Equals(GetType().Name, StringComparison.CurrentCultureIgnoreCase))
            {
                ScriptTemplate scriptTemplate = new ScriptTemplate(parentPack, node);
                //if (Parent != null && Parent.Name.Equals(DEFAULT_MAIN_NAME))
                //    ((ScriptTemplate)Parent).LogShell = scriptTemplate.LogShell;
                //if (Name.Equals(CONFIG_NODE, StringComparison.CurrentCultureIgnoreCase))
                //    LogShell = scriptTemplate.LogShell;
                return scriptTemplate;
            }

            string assebmly = null;
            string className = null;
            //если указан динамический класс
            if (node.Attributes?["assembly"] != null)
            {
                var nodeAttribute = node.Attributes["assembly"];
                string[] asseblyAttr = nodeAttribute?.Value.Split(',');
                if (asseblyAttr?.Length == 2)
                {
                    assebmly = asseblyAttr[0].Trim();
                    className = asseblyAttr[1].Trim();
                }
            }

            if (string.IsNullOrEmpty(assebmly) && string.IsNullOrEmpty(className))
            {
                assebmly = Functions.AssemblyFile;
                className = string.Format("{0}.{1}Handler", NAMESPACE_OF_HANDLERS, node.Name);
            }

            return CreateNewHandler(assebmly, className, parentPack, node);

        }

        XPack CreateNewHandler(string assebly, string className, XPack parentPack, XmlNode node)
        {
            string verifyAssembly = assebly;
            if (!File.Exists(verifyAssembly))
                throw new HandlerInitializationException("Assembly=[{0}] Not Found!", verifyAssembly);

            Assembly pluginAssembly = Assembly.LoadFrom(verifyAssembly);
            Type classType = pluginAssembly.GetType(className);
            if (classType == null)
            {
                string nameSpace = className.IndexOf(".", StringComparison.Ordinal) != -1 ? className.Substring(0, className.LastIndexOf(".", StringComparison.Ordinal)) : className;
                if (!GetTypeByNameSpace(pluginAssembly, nameSpace, className, out classType))
                {
                    //если не найден ни один объект то создаем дефолтный скрипт темплейт
                    if(LogShell == null)
                        return new ScriptTemplate(parentPack, node);
                    else
                        return new ScriptTemplate(parentPack, node, LogShell);
                }
            }

            object potentialContext = Activator.CreateInstance(classType, parentPack, node, LogShell);

            if (potentialContext == null)
                throw new HandlerInitializationException("Assembly=[{0}]; Class=[{1}] Not Initialized!", pluginAssembly.ManifestModule.Name, className);

            ScriptTemplate context = potentialContext as ScriptTemplate;
            if (context == null)
                throw new HandlerInitializationException("Assembly=[{0}]; Type Of Class=[{1}] Must Be Inherited From ScriptTemplate, Current=[{2}]", pluginAssembly.ManifestModule.Name, className, potentialContext.GetType());

            return context;
        }

        bool GetTypeByNameSpace(Assembly pluginAssembly, string nameSpace, string className, out Type tp)
        {
            tp = null;
            List<Type> listTypesByNamespace = GetListTypesByNameSpace(pluginAssembly, nameSpace);
            List<Type> findedType = listTypesByNamespace.Where(x => x.FullName.Equals(className, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (findedType.Count == 0)
            {
                return false;
                //if (listTypesByNamespace.Count > 0)
                //    throw new HandlerInitializationException("Assembly=[{0}]; Can't Find Type [{1}]; Finded Types In NameSpace=[{2}]:{3}{4}", pluginAssembly.ManifestModule.Name, className, nameSpace, Environment.NewLine, string.Join(Environment.NewLine, listTypesByNamespace));
                //throw new HandlerInitializationException("Assembly=[{0}]; Can't Find Type [{1}]", pluginAssembly.ManifestModule.Name, className);
            }
            if (findedType.Count > 1)
                throw new HandlerInitializationException("Assembly=[{0}]; Can't Identify [{1}]. Because In Namespace=[{2}] More Than One!{3}{4}", pluginAssembly.ManifestModule.Name, className, nameSpace, Environment.NewLine, string.Join(Environment.NewLine, findedType));

            tp = findedType[0];
            return true;
        }

        static List<Type> GetListTypesByNameSpace(Assembly pluginAssembly, string nameSpace)
        {
            return pluginAssembly.GetTypes().Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }


        [IdentifierResource(new[] { "{localpath} - Получить локальную дерикторию",
                                    "{datetime} - Получить текущую дату с форматом 'yyyy.MM.dd hh:mm:ss'. Формат даты можно указать любую {datetime, '[формат даты]'}. ",
                                    "{[Имя ноды].[Имя аттрибута]} - Получить значения аттрибутов из текущего конфига. Например:{ScriptTemplate.Name} - из ноды ScriptTemplate получить значение аттрибута Name. Поиск происходит только в родительских нодах." })]
        protected string GetResources(string source, XPack parent)
        {
            string result = source;
            Regex resours = new Regex("{(.+?)}", RegexOptions.IgnoreCase);
            foreach (Match match in resours.Matches(result))
            {
                string tempValue = match.Value.Trim('{', '}', ' ');
                if (tempValue.Equals("localpath", StringComparison.CurrentCultureIgnoreCase))
                {
                    result = new Regex(match.Value, RegexOptions.IgnoreCase).Replace(result, BaseArguments.LocalPath, 1);
                    continue;
                }
                if (tempValue.StartsWith("datetime", StringComparison.CurrentCultureIgnoreCase))
                {
                    string formatDate = Regex.Replace(tempValue, @"datetime\s*\,\s*\'(.+?)\'", "$1", RegexOptions.IgnoreCase);
                    if (string.IsNullOrEmpty(formatDate) || formatDate.Length == tempValue.Length)
                        formatDate = "yyyy.MM.dd hh:mm:ss";
                    result = new Regex(match.Value, RegexOptions.IgnoreCase).Replace(result, DateTime.Now.ToString(formatDate), 1);
                    continue;
                }

                if (tempValue.Split('.').Length > 1)
                {
                    string[] path_list = tempValue.Split('.');
                    string find_by_path = string.Join("/", path_list.Take(path_list.Length - 1));
                    string find_by_attr_name = path_list[path_list.Length - 1];

                    List<XPack> listmatches = parent[find_by_path, FindMode.Up];
                    if (listmatches != null && listmatches.Count > 0)
                    {
                        XPackAttributeCollection findedAttr = listmatches[0].Attributes;
                        result = new Regex(match.Value, RegexOptions.IgnoreCase).Replace(result, findedAttr[find_by_attr_name], 1);
                        continue;
                    }
                }

                //Если был указан некорректный ресурс во время инициализации объекта
                //if (!IsCorrect)
                    throw new HandlerInitializationException("Resource=[{1}] Is Incorrect! Please Change Your Resource in Object=[{0}]", this, tempValue);
                //else
                //{
                //    //если во время обработки был найден некорректный ресурс при получении какого то аттрибута. Обычно это какие то левые созданные объекты
                //    AddLog(LogType.Warning, this, "Resource=[{1}] Is Incorrect! This Resource Replaced By Null in Object=[{0}]", this, tempValue);
                //    result = new Regex(match.Value, RegexOptions.IgnoreCase).Replace(result, string.Empty, 1);
                //}
            }
            return result;
        }

        protected void AddLog(Exception exception, ScriptTemplate sourceObj)
        {
            if (LogShell == null)
                throw new HandlerInitializationException("Incorrect [{0}]. Please Check It! Exception:[{1}]", this, exception);
            LogShell.Exec_AddLog?.Invoke(LogType.Exception, sourceObj, exception.ToString());
        }
        protected void AddLog(Exception exception, ScriptTemplate sourceObj, string stringformat, params object[] objects)
        {
            if (LogShell == null)
                throw new HandlerInitializationException("Incorrect [{0}]. Please Check It! Exception:[{1}] Log:[{2}]", this, exception, string.Format(stringformat, objects));
            LogShell.Exec_AddLog?.Invoke(LogType.Exception, sourceObj, exception.ToString(), string.Format(stringformat, objects));
        }
        protected void AddLog(LogType type, ScriptTemplate sourceObj, string stringformat, params object[] objects)
        {
            if (LogShell == null)
                throw new HandlerInitializationException("Incorrect [{0}]. Please Check It! Log:[{1}]", this, string.Format(stringformat, objects));
            LogShell.Exec_AddLog?.Invoke(type, sourceObj, stringformat, objects);
        }
        void TemplateAddLog(LogType type, ScriptTemplate sourceObj, string stringformat, params object[] objects)
        {
            try
            {
                if ((Severity) == LogType.Console)
                {
                    if (type == LogType.Exception)
                    {
                        LogShell.ScriptExceptionsCount++;
                        Console.WriteLine(stringformat);
                    }
                    return;
                }
                //определяет находится ли данный флаг лога в списке доступных флагов логирования. 
                //Выражение (logOpt?.ScriptLog?.Severity & type) - вытягивает из списка флагов ScriptLog.Severity определеный флаг type, если его нет то ответ 0 или не равен type
                if ((Severity & type) != type)
                    return;


                string log_input;
                if (type == LogType.Exception)
                {
                    log_input = string.Format("{0}:{1}; {2}", type, stringformat, objects.Length > 0 ? objects[0] : string.Empty);
                    LogShell.ScriptExceptionsCount++;
                }
                else
                    log_input = string.Format("{0}:{1}", type, string.Format(stringformat, objects));

                string mBaseParent = new StackTrace().GetFrame(3).GetMethod().Name.Replace(".ctor", "").Replace("<", "").Replace(">", "");
                string mBase = new StackTrace().GetFrame(2).GetMethod().Name;
                //string overridedObj = GetAllProperties(sourceObj);
                string strLog = string.Format("[{1:dd.MM.yyyy HH:mm:ss.ffffff}]; {2}=[{3}.{4}];{0}{5}{0}{6}", 
                    Environment.NewLine, DateTime.Now, ScriptName, mBaseParent, mBase, log_input, sourceObj);

                AddJustLog(this, strLog);

            }
            catch (Exception ex)
            {
                LogShell.ScriptExceptionsCount++;
                if (type == LogType.Exception)
                    Console.WriteLine("BaseException:[{0}]", stringformat);
                Console.WriteLine(ex.Message);
            }
        }

        static int _separationCount = 250;
        public static void AddJustLog(ScriptTemplate scriptTemplate, string strLog)
        {
            using (StreamWriter file = new StreamWriter(scriptTemplate.LogPath, scriptTemplate.NumOfLogRecord > 0, Functions.Enc))
            {
                string log = string.Format("[#{0}]", scriptTemplate.NumOfLogRecord++);
                log = string.Format("{0}{1}{2}{3}", log, string.Join("=", new string('=', _separationCount - log.Length)), Environment.NewLine, strLog);
                file.WriteLine(log);
            }
        }
        static LogType GetSeverity(string severity)
        {
            LogType defaultOption = LogType.Console;

            if (string.IsNullOrEmpty(severity))
                return defaultOption;
            if (severity.Equals("None", StringComparison.CurrentCultureIgnoreCase) || severity.Equals("Disable", StringComparison.CurrentCultureIgnoreCase))
                return LogType.None;

            LogType selectOption = LogType.None;
            int i = 0;
            foreach (string strType in severity.Split('|'))
            {
                foreach (LogType lgType in Enum.GetValues(typeof(LogType)))
                {
                    string ss1 = strType.Trim();
                    string ss2 = lgType.ToString("g");

                    if (ss1.Equals(ss2, StringComparison.CurrentCultureIgnoreCase))
                    {
                        i++;
                        if (i <= 1)
                            selectOption = lgType;
                        else
                            selectOption |= lgType;
                    }
                }
            }
            return selectOption;
        }

        public string GetXMLAttributeName(string propName)
        {
            return GetIdentifier(propName).Name;
        }
        public string GetXMLAttributeDescription(string propName)
        {
            return GetIdentifier(propName).Description;
        }
        public IdentifierAttribute GetIdentifier(string propName)
        {
            Type inheritType = GetType().UnderlyingSystemType;
            PropertyInfo prop = inheritType.GetProperty(propName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            foreach (object obj in prop.GetCustomAttributes(false))
            {
                IdentifierAttribute attrVoid = obj as IdentifierAttribute;
                if (attrVoid != null)
                    return attrVoid;
            }
            throw new HandlerInitializationException("Property {0} Is Not {1}", propName, typeof(IdentifierAttribute).Name);
            //return (IdentifierAttribute)prop.GetCustomAttributes(false).Where(x => x.GetType() == typeof(IdentifierAttribute)).First();// .ToList().ConvertAll(x => (IdentifierAttribute)x);
        }
        public static IdentifierAttribute GetVoidIdentifier(Type tp, string propName)
        {
            Type inheritType = tp.UnderlyingSystemType;
            MethodInfo methodInfos = tp.GetMethod(propName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            //MethodInfo mInfoMethod =
            //    typeof(ScriptTemplate).GetMethod(
            //        propName,
            //        BindingFlags.Instance | BindingFlags.NonPublic,
            //        Type.DefaultBinder,
            //        new[] { typeof(int), typeof(int) },
            //        null);
            //mInfoMethod.Invoke(new ScriptTemplate(), new object[] { null, null });

            foreach(object obj in methodInfos.GetCustomAttributes(false))
            {
                IdentifierAttribute attrVoid = obj as IdentifierAttribute;
                if (attrVoid != null)
                    return attrVoid;
            }

            throw new HandlerInitializationException("Property {0} Is Not {1}", propName, typeof(IdentifierAttribute).Name);
        }

        public static string GetExampleOfConfig()
        {
            string result = string.Format("<{0}>{1}", CONFIG_NODE, Environment.NewLine);
            int i = 0;
            result = result + string.Format("<!--{1}{0}{1}-->{1}", GetVoidIdentifier(typeof(ScriptTemplate), nameof(GetResources)).Description, Environment.NewLine);

            result = result + OpenNode(typeof(ScriptTemplate), ref i);
            result = result + OpenNode(typeof(FindHandler), ref i);
            result = result + OpenNode(typeof(FindHandler), ref i);
            result = result + OpenNode(typeof(GetValueHandler), ref i);
            result = result + CloseNode(typeof(GetValueHandler), ref i);
            result = result + CloseNode(typeof(FindHandler), ref i);
            result = result + CloseNode(typeof(FindHandler), ref i);
            result = result + CloseNode(typeof(ScriptTemplate), ref i);

            result = string.Format("{0}{1}", result, Environment.NewLine);
            result = result + OpenNode(typeof(ScriptTemplate), ref i);
            result = result + OpenNode(typeof(FindHandler), ref i);
            result = result + OpenNode(typeof(GetValueByRegexHandler), ref i);
            result = result + OpenNode(typeof(GetValueByXPathHandler), ref i);
            result = result + CloseNode(typeof(GetValueByXPathHandler), ref i);
            result = result + CloseNode(typeof(GetValueByRegexHandler), ref i);
            result = result + CloseNode(typeof(FindHandler), ref i);
            result = result + CloseNode(typeof(ScriptTemplate), ref i);

            result = string.Format("{0}{1}", result, Environment.NewLine);
            result = result + OpenNode(typeof(ScriptTemplate), ref i);
            result = result + OpenNode(typeof(TimesheetHandler), ref i);
            result = result + CloseNode(typeof(TimesheetHandler), ref i);
            result = result + CloseNode(typeof(ScriptTemplate), ref i);


            result = string.Format("{0}</{1}>", result, CONFIG_NODE);
            return result;
        }

        static string OpenNode(Type tp, ref int numSubGroup)
        {
            //создать отступы
            string separators = string.Empty;
            numSubGroup = numSubGroup + 4;
            if (numSubGroup > 0)
                separators = new string(' ', numSubGroup);

            //получаем описание класса
            string classDescription = null;
            object[] attrsClass = tp.GetCustomAttributes(true);
            foreach (object attr in attrsClass)
            {
                IdentifierAttribute identAttr = attr as IdentifierAttribute;
                if (identAttr != null)
                {
                    classDescription = identAttr.Description.Trim();
                    break;
                }
            }

            string _out = string.Empty;
            if (classDescription != null)
                _out = string.Format("{0}<!-- {1} -->{2}", separators, classDescription, Environment.NewLine);

            string _node = string.Format("{0}<{1} ", separators, tp.Name.Replace("Handler", ""));
            //просчитывается для корректного форматирования описания свойств в CDATA
            int ident = _node.Length;

            _out = string.Format("{0}{1}", _out, _node);
            
            //получаем все свойства класса
            PropertyInfo[] props = tp.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            string cdata = string.Empty;
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    IdentifierAttribute identAttr = attr as IdentifierAttribute;
                    if (identAttr != null)
                    {
                        CreateAttribute(ref _out, ref cdata, identAttr, ident);
                    }
                }
            }
            _out = string.Format("{0}>{1}{2}<![CDATA[{3}]]>{1}{1}", _out, Environment.NewLine, new string(' ', ident), cdata);
            return _out;
        }

        static void CreateAttribute(ref string _out, ref string cdata, IdentifierAttribute identAttr, int ident)
        {
            if(string.IsNullOrEmpty(identAttr.Name))
                return;

            _out = string.Format("{0}{1}=\"{2}\" ", _out, identAttr.Name, identAttr.DefaultValue);
            if (string.IsNullOrEmpty(cdata))
                cdata = string.Format("{0}={1}", identAttr.Name, identAttr.Description);
            else
                cdata = string.Format("{0}{1}{2}{3}={4}", cdata, Environment.NewLine, new string(' ', ident + 9), identAttr.Name, identAttr.Description);
        }

        static string CloseNode(Type tp, ref int numSubGroup)
        {
            string separators = string.Empty;
            if (numSubGroup > 0)
                separators = new string(' ', numSubGroup);
            string result = string.Format("{0}</{1}>{2}", separators, tp.Name.Replace("Handler", ""), Environment.NewLine);
            numSubGroup = numSubGroup - 4;
            return result;
        }


        //public override string ToString()
        //{
        //    return string.Format("{1}{0}{2}=[{3}]", Environment.NewLine, base.ToString(), GetType(), LogShell);
        //}
        //public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
        //{
        //    MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
        //    return expressionBody.Member.Name;
        //}
        //public static Dictionary<string, string> GetAuthors()
        //{
        //    Dictionary<string, string> _dict = new Dictionary<string, string>();

        //    PropertyInfo[] props = typeof(Book).GetProperties();
        //    foreach (PropertyInfo prop in props)
        //    {
        //        object[] attrs = prop.GetCustomAttributes(true);
        //        foreach (object attr in attrs)
        //        {
        //            AuthorAttribute authAttr = attr as AuthorAttribute;
        //            if (authAttr != null)
        //            {
        //                string propName = prop.Name;
        //                string auth = authAttr.Name;

        //                _dict.Add(propName, auth);
        //            }
        //        }
        //    }

        //    return _dict;
        //}
        //public static string GetAllProperties(object obj)
        //{
        //    PropertyInfo[] _PropertyInfos = obj.GetType().GetProperties();
        //    string result = string.Empty;

        //    foreach (var info in _PropertyInfos)
        //    {
        //        try
        //        {
        //            var value = info.GetValue(obj, null) ?? "(null)";
        //            result = result + string.Format("{0}='{1}' ", info.Name, value);
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //    return result;
        //}
    }
}
