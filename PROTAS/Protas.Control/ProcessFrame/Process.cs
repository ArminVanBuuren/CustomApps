using System;
using System.Collections.Generic;
using System.Reflection;
using Protas.Components.Functions;
using Protas.Components.PerformanceLog;
using Protas.Control.ProcessFrame.Components;
using Protas.Control.ProcessFrame.Triggers;
using Protas.Control.Resource;
using Protas.Control.Resource.Base;
using Protas.Components.XPackage;

namespace Protas.Control.ProcessFrame
{

    internal class Process : ShellLog3Net, IProcessor
    {
        public const string NODE_CONTEXT_LIST = @"ContextList";
        public const string NODE_TRIGGER_LIST = @"TriggerList";
        public const string NODE_HANDLER_LIST = @"HandlerList";
        public const string NODE_CONTEXT = @"Context";
        public const string NODE_TRIGGER = @"Trigger";
        public const string NODE_HANDLER = @"Handler";
        public const string ATTR_CONTEXT_CLASS = @"Class";
        public const string ATTR_CONTEXT_ASSAMBLY = @"Assembly";

        //данные константы должны быть написаны в нижнем регистре
        public const string NODE_TRIGGER_CORE = @"core";
        public const string NODE_TRIGGER_TIMER = @"timer";
        CollectionXPack P_HandlerList { get; }
        /// <summary>
        /// Имя процесса. Названия должны быть равны /Config/CollectionProcess/Process == /Config/CollectionCluster/Cluster
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Базовый ресурс из которого мы получаем дочерние ресурсы
        /// </summary>
        public ResourceBase Resource { get; }

        /// <summary>
        /// Общее количество контекстов
        /// </summary>
        public ResourceContextPack ContextCollection { get; } = new ResourceContextPack();

        /// <summary>
        /// Только юзерные контексты
        /// </summary>
        
        public List<IProcessor> TriggerCollection { get; } = new List<IProcessor>();
        public Dictionary<string, IHandler> HandlerCollection { get; } = new Dictionary<string, IHandler>(StringComparer.CurrentCultureIgnoreCase);
        public Dictionary<string, IHandler> RemoteHandlerCollection { get; } = new Dictionary<string, IHandler>(StringComparer.CurrentCultureIgnoreCase);
        Dispatcher Main { get; }

        public bool IsCorrect { get; } = false;

        public Process(XPack proc, XPack clust, ResourceBase resourcePack, StaticContext macros, Dispatcher main) : base(main)
        {
            try
            {
                Name = proc.Attributes["name"];
                TraceAddPostfix(string.Format("[{0}:{1}]", Dispatcher.NODE_PROCESS, Name));
                Resource = resourcePack;
                Main = main;
                AddLog(Log3NetSeverity.Debug, "Start...");

                //для начала находим хэндлеры и проверяем на уникальность имен
                //добавялем коллекцию паков на хэндлеры и проверяем на уникальность значения в аттрибуте Name
                P_HandlerList = new CollectionXPack();
                foreach (XPack handler in clust[NODE_HANDLER_LIST])
                {
                    P_HandlerList.AddRange(handler.ChildPacks);
                }
                if (!P_HandlerList.IsUniqueAttributeValueByKey("name"))
                {
                    AddLogForm(Log3NetSeverity.Error, "{2} Name's Is Not Unique On {0}=\"{1}\"", Dispatcher.NODE_CLUSTER, clust.Attributes["name"], NODE_HANDLER);
                    return;
                }

                //Сначала добавляем дефолтные контексты
                //необходим именно такой порядок, т.к. в первую очередь должны выбираться ресурсы из => системного конетекса => статичных в процессе => макроса => непосредетсвенного контекста => потом уже динамичные
                ContextCollection.AddPrimary(string.Empty, resourcePack.BaseContext);
                //добавляекм коллекцию макросов
                ContextCollection.AddPrimary(StaticContext.NODE_MACROS_STATIC, macros);
                //добавляем в первую очередь конекст с макросами который находятся в объявленном процессе
                if (proc.ChildPacks.Count > 0)
                {
                    StaticContext macrosAndProcMac = new StaticContext(proc.ChildPacks);
                    ContextCollection.AddPrimary(StaticContext.NODE_MACROS_STATIC, macrosAndProcMac);
                }



                //Находим контексты кластера
                foreach (XPack cntx in clust[NODE_CONTEXT_LIST])
                {
                    try
                    {
                        //имя контекста должны выгляедеть через точку имя <Context.system/> - в данно случше имя system имя образения к контексту
                        string[] NodeNameAndContextName = cntx.Name.Split('.');
                        if (!NodeNameAndContextName[0].Equals(NODE_CONTEXT, StringComparison.CurrentCultureIgnoreCase))
                            continue;

                        if (NodeNameAndContextName.Length < 2)
                        {
                            AddLogForm(Log3NetSeverity.Error, "It Must Include The Name Of The {0} Through The Point", NODE_CONTEXT);
                            continue;
                        }

                        string assemblyPath;
                        string className;
                        if (!cntx.Attributes.TryGetValue(ATTR_CONTEXT_ASSAMBLY, out assemblyPath))
                        {
                            AddLogForm(Log3NetSeverity.Error, "Not Finded Mandatory Attribute=\"{0}\" In {1} Config.", ATTR_CONTEXT_ASSAMBLY, NODE_CONTEXT);
                            continue;
                        }
                        if (!cntx.Attributes.TryGetValue(ATTR_CONTEXT_CLASS, out className))
                        {
                            AddLogForm(Log3NetSeverity.Error, "Not Finded Mandatory Attribute=\"{0}\" In {1} Config.", ATTR_CONTEXT_CLASS, NODE_CONTEXT);
                            continue;
                        }

                        PathProperty pp = new PathProperty(assemblyPath);
                        Assembly pluginAssembly = Assembly.LoadFrom(pp.FullPath);
                        Type classType = pluginAssembly.GetType(className);


                        //для инициализации контекста - узурпируем пак и удаляем системные параметры по конфигурации class и assembly
                        //получится такой вид например 
                        //<Context.sys class="Protas.System.Statistic" assembly="C:\Project\Protas.System\bin\Debug\Protas.System.dll">
                        //<Connection username = "admin" password="qweerty123" domain="adgtd"/>
                        //</Context.sys>
                        XPack configContext = new XPack(NODE_CONTEXT, cntx.Value);
                        XPack.CreateXPackCopy(ref configContext, cntx);
                        configContext.Attributes.Remove(ATTR_CONTEXT_ASSAMBLY);
                        configContext.Attributes.Remove(ATTR_CONTEXT_CLASS);


                        //инициализируем сам конеткст, где в конструкторе самого контекста должны быть параметры для по конфигурации контекста и свойства для для записи лога
                        List<object> defaultClassConstructor = new List<object> { configContext, (ILog3NetMain)this };
                        object potentialContext = ProtasActivator.CreateInstance(classType, defaultClassConstructor);

                        if (potentialContext == null)
                        {
                            AddLogObj(Log3NetSeverity.Error, "Error! Returned Null When Create Instance {0}! Assembly.Location=\"{1}\" AssemblyQualifiedName=\"{2}\"", typeof(IResourceContext), classType.Assembly.Location, classType.AssemblyQualifiedName);
                            continue;
                        }

                        IResourceContext context = potentialContext as IResourceContext;
                        if (context == null)
                        {
                            AddLogForm(Log3NetSeverity.Error, "Error! Returned Object Is Not {0}! Returned Object {1} Assembly.Location=\"{2}\" AssemblyQualifiedName=\"{3}\"", typeof(IResourceContext), potentialContext, classType.Assembly.Location, classType.AssemblyQualifiedName);
                            continue;
                        }

                        if (!context.IsIntialized)
                        {
                            AddLogForm(Log3NetSeverity.Error, "Error! Returned {0} {1} Not Initialized! Assembly.Location=\"{2}\" AssemblyQualifiedName=\"{3}\"", typeof(IResourceContext), potentialContext, classType.Assembly.Location, classType.AssemblyQualifiedName);
                            continue;
                        }

                        ContextCollection.AddPrimary(NodeNameAndContextName[1], context);
                    }
                    catch (Exception ex)
                    {
                        AddEx(ex);
                    }
                }


                //Инициируем триггеры
                List<XPack> P_TriggerList = clust[NODE_TRIGGER_LIST];
                if (P_TriggerList != null)
                    foreach (XPack trigger in clust[NODE_TRIGGER_LIST])
                        AssignTriggers(trigger.ChildPacks);



                if (TriggerCollection.Count == 0 && P_HandlerList.Count == 0)
                {
                    AddLogForm(Log3NetSeverity.Error, "Not Finded Any {2}s And {3}s On {0}=\"{1}\"", Dispatcher.NODE_CLUSTER, clust.Attributes["name"], NODE_HANDLER, NODE_TRIGGER);
                    return;
                }


                IsCorrect = true;
            }
            catch (Exception ex)
            {
                AddEx(ex);
            }
            finally
            {
                AddLogForm(Log3NetSeverity.Debug, "Finnaly. {0} {1} Initialized", Dispatcher.NODE_PROCESS, (IsCorrect) ? "Success" : "Failure");
            }
        }

        /// <summary>
        /// Инициируем триггеры
        /// </summary>
        void AssignTriggers(List<XPack> triggers)
        {
            foreach (XPack triggerPack in triggers)
            {
                try
                {
                    //Split тут необходим чтобы разделять что название триггера, а что имя динамичной сессии
                    switch (triggerPack.Name.ToLower().Split('.')[0])
                    {
                        case NODE_TRIGGER_CORE:
                            {
                                CoreTrigger core = new CoreTrigger(TriggerCollection.Count, triggerPack, this);
                                TriggerCollection.Add(core);
                            }
                            break;
                        case NODE_TRIGGER_TIMER:
                            {
                                TimerTrigger timer = new TimerTrigger(TriggerCollection.Count, triggerPack, this);
                                TriggerCollection.Add(timer);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    AddEx(ex);
                }
            }
        }

        /// <summary>
        /// Возвращет уже созданный хэндлер или создает новый и добавляет в коллекцию хэндлеров для текущего процесса
        /// </summary>
        /// <param name="inputHndName">Имя</param>
        /// <returns></returns>
        internal IHandler GetIHandler(string inputHndName)
        {
            //находим уже созданный хэндлер
            IHandler existing = HandlerCollection[inputHndName];
            if (existing != null)
                return existing;

            //сначала инициализируем все хэндлеры
            foreach (XPack handlerPack in P_HandlerList)
            {
                string nameHandler;
                //если не найден аттрибут name в перечислениях хэндлеров
                if (!handlerPack.Attributes.TryGetValue("name", out nameHandler))
                    continue;

                string[] nameHandAndContextName = nameHandler.Split('.');
                if (nameHandAndContextName.Length < 2)
                {
                    AddLogForm(Log3NetSeverity.Error, "Through The Point Must Be Included The Key Of The {1} For {2}=\"{0}\"", nameHandAndContextName[0], NODE_CONTEXT, NODE_HANDLER);
                    continue;
                }

                //если не совпадает имя необходимого хэндлера с списком хэндлеров
                if (!nameHandAndContextName[0].Equals(inputHndName, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                //создаем копию пака хэндлера без системных параметров - аттрибута name и названия конеткста через точку - как например - <MSMQHnd.system - system удаляется
                XPack copyHandlerPack = new XPack(nameHandAndContextName[0], handlerPack.Value);
                XPack.CreateXPackCopy(ref copyHandlerPack, handlerPack);
                //удаляем аттрибут name и передаем этот XPACK в инициализацию хэндлера (Это не обязательная операция конечно, это чисто для инкапсуляции используется)
                copyHandlerPack.Attributes.Remove("name");

                //инициализируем хэндлер
                IHandler newHandler = ContextCollection.GetHandler(nameHandAndContextName[1], copyHandlerPack);
                if (newHandler == null)
                { 
                    AddLogForm(Log3NetSeverity.Error, "{3}=\"{0}\" Not Found In {1} With Key=\"{2}\"", nameHandAndContextName[0], NODE_CONTEXT, nameHandAndContextName[1], NODE_HANDLER);
                    return null;
                }
                HandlerCollection.Add(nameHandler, newHandler);
                return newHandler;
            }

            return null;
        }

        /// <summary>
        /// Возвращет уже созданный удаленный таск или создает новый и добавляет в тасков хэндлеров для текущего процесса
        /// </summary>
        /// <param name="inputHndName"></param>
        /// <returns></returns>
        internal IHandler GetRemoteBinding(string inputHndName)
        {
            return Main.GetRemoteBinding(inputHndName);
        }


        public bool Start()
        {
            return false;
        }
        public bool Stop()
        {
            return false;
        }

        public void Dispose()
        {

        }

        public override string ToString()
        {
            return XString.Format(new [] { Dispatcher.NODE_PROCESS, NODE_TRIGGER, NODE_HANDLER }, Name, TriggerCollection.Count, HandlerCollection.Count);
        }
    }
}
