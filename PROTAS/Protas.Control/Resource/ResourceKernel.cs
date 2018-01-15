using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Protas.Components.Functions;
using Protas.Components.PerformanceLog;
using Protas.Control.Resource.Base;
using Protas.Components.XPackage;

namespace Protas.Control.Resource
{
    public class ResourceKernel : ShellLog3Net, IDisposable
    {
        ~ResourceKernel()
        {
            AddLogForm(Log3NetSeverity.Debug, "{0} Disposed", ToString());
        }

        Thread monitorThread;
        ResourceMode _mode = ResourceMode.External;
        string _input;
        public ResourceEvent ResourcesChanged { get; set; } = new ResourceEvent();
        public int Id { get; private set; }
        public string UniqueId => (Main == null) ? "self" : string.Format("{0}_{1}", Main.UniqueId, Id);
        public ResourceBase Main { get; private set; }
        public bool IsResource => SegmentCollection.IsResource;
        internal ResourceSegmentCollection SegmentCollection { get; set; }
        internal ResourceContextPack Contexts { get; private set; }
        internal ResourceHandlerCollection HandlerCollection => Main.HandlerCollection;
        internal List<ResourceSignature> SignatureCollection => Main.SignatureCollection;
        /// <summary>
        /// список ресурсов которые нельзя вызывать и они будут возвращать пустоту
        /// данная проверка необходима если возникают архитектурные конфликты когда ресурсы используется как рекурсивные
        /// т.е. вызывают сами себя в бесконечности в непосредственных ресурсах, эту проверку на архитектурную зависимость должны делать вне данного класса
        /// </summary>
        internal List<ResourceShell> BlackList { get; } = new List<ResourceShell>();
        internal ResourceParcer Parcer { get; set; }

        internal bool AnyHandlerEventElapsed
        {
            get
            {
                lock (this)
                {
                    foreach (ResourceHandler handler in HandlerCollection)
                    {
                        if (handler.HandlerEventElapsed)
                            return true;
                    }
                    return false;
                }
            }
        }


        public string Input
        {
            get { return _input; }
            private set
            {
                lock (this)
                {
                    _input = value;
                    if (SegmentCollection != null)
                        SegmentCollection.Dispose();

                    if (string.IsNullOrEmpty(_input))
                        SegmentCollection = new ResourceSegmentCollection(this);
                    else
                        SegmentCollection = Parcer.GetByExpression(_input);
                    InitializeHandlers();
                }
            }
        }

        public string Source
        {
            get
            {
                try
                {
                    AddLog(Log3NetSeverity.Max, "Start");
                    string res = SegmentCollection.Source;
                    AddLogForm(Log3NetSeverity.Debug, "Source:{0}", res);
                    return res;
                }
                catch (Exception ex)
                {
                    AddLogForm(Log3NetSeverity.Error, "Return Input | Exception:{0}\r\n{1}", ex.Message, ex.Data);
                    return Input;
                }
                finally
                {
                    AddLog(Log3NetSeverity.Max, "End");
                }
            }
        }
        public string Result
        {
            get
            {
                try
                {
                    if (!IsResource)
                        return Input;
                    AddLog(Log3NetSeverity.Max, "Start");
                    if (IsAlreadyElapsed)
                    {
                        AddLogForm(Log3NetSeverity.Debug, "Resource Changed | Result:{0}", _prevResult);
                        return _prevResult;
                    }
                    string result = SegmentCollection.Result;
                    if (ProcessingMode == ResourceMode.External)
                        AddLogForm(Log3NetSeverity.Debug, "Result:{0}", result);
                    return result;
                }
                catch (Exception ex)
                {
                    AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.Data);
                    return string.Empty;
                }
                finally
                {
                    AddLog(Log3NetSeverity.Max, "End");
                }
            }
        }

        public FinalResultMode ResultMode { get; set; } = FinalResultMode.All;
        /// <summary>
        /// Ресурсный режим, он может быть обычным (Default) Result сам не обновляется, в остальных случаех обновляется сам и возвращеает эвент в случае изменения результата ресурса
        /// </summary>
        public ResourceMode ProcessingMode
        {
            get { return _mode; }
            set
            {
                if (value == _mode)
                    return;
                _mode = value;
                if (value == ResourceMode.Core)
                {
                    InitializeHandlers();
                    monitorThread = new Thread(ThreadRun);
                    monitorThread.Start();
                }
                else
                {
                    //monitorThread.Abort();
                    InitializeHandlers();
                }
            }
        }

        internal Queue<REsPAck> QueueInitiation { get; } = new Queue<REsPAck>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="main"></param>
        /// <param name="id"></param>
        /// <param name="input">строка с ресурсами</param>
        /// <param name="contexts"></param>
        internal ResourceKernel(ResourceBase main, int id, string input, ResourceContextPack contexts) : base(main)
        {
            Initialize(main, id, input, contexts);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="main"></param>
        /// <param name="id"></param>
        /// <param name="input">строка с ресурсами</param>
        /// <param name="blackList"></param>
        /// <param name="contexts"></param>
        internal ResourceKernel(ResourceBase main, int id, string input, List<ResourceShell> blackList, ResourceContextPack contexts) : base(main)
        {
            BlackList = blackList;
            Initialize(main, id, input, contexts);
        }
        void Initialize(ResourceBase main, int id, string input, ResourceContextPack contexts)
        {
            try
            {
                AddLog(Log3NetSeverity.Max, "Start");
                AddMessagePrefix(string.Format("RES:{0}", id));
                Id = id;
                Main = main;
                Contexts = contexts;
                Parcer = new ResourceParcer(this);
                Input = input;
            }
            catch (Exception ex)
            {
                AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.Data);
            }
            finally
            {
                AddLog(Log3NetSeverity.Max, "End");
            }
        }
        /// <summary>
        /// Запустить ресурс в каком то из режимов
        /// </summary>
        void InitializeHandlers()
        {
            try
            {
                AddLog(Log3NetSeverity.Max, "Start Initialize Handlers");
                SegmentCollection.InitializeHandlers();
            }
            catch (Exception ex)
            {
                AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                AddLog(Log3NetSeverity.Max, "End Initialize Handlers");
            }
        }





        internal class REsPAck
        {
            public REsPAck(ResourceSegment segment, ResourceHandler handler, XPack result)
            {
                Segment = segment;
                Handler = handler;
                Result = result;
            }
            public ResourceSegment Segment { get; }
            public ResourceHandler Handler { get; }
            public XPack Result { get; }

            public override int GetHashCode()
            {
                return Handler.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                REsPAck newObj = obj as REsPAck;
                if (newObj != null)
                {
                    return true;
                }
                return base.Equals(obj);
            }
        }



        public void ThreadRun()
        {
            while (ProcessingMode == ResourceMode.Core)
            {
                lock (((ICollection)QueueInitiation).SyncRoot)
                {
                    Thread.Sleep(1);
                    if (QueueInitiation.Count > 0)
                    {
                        REsPAck item = QueueInitiation.Dequeue();
                        item?.Segment.IsBindingHandlerChanged2(item.Handler, item.Result);
                    }
                }
            }
        }
        /// <summary>
        /// Данное свойство необходимо чтобы следить за ядром, если в ядре больше одного ресурса и в случае чего одновременно срабатывают все эвенты ресурсов
        /// в одну итерацию то возврщаем на выход первое обработавшее которое собрала значение со всех ресурсов и после чего нет необходимости получать их заново в туже итерацию
        /// как только какой в какой либо сигнатруре сново сработает эвент, то свойство обнулится. И заново пойдет обработка на проверку и получение результатов по остальным сегментам и тд..
        /// </summary>
        internal bool Lock { get; set; } = false;
        string _prevResult = string.Empty;
        internal bool IsAlreadyElapsed { get; private set; } = false;
        internal void IsResourceChanged(ResourceSegment segment, ResourceHandler initialHandler)
        {
            if (ResourcesChanged == null)
                return;
            try
            {
                AddLog(Log3NetSeverity.Max, "Start");
                //проверяем если входящий сегмент имеет тип не уникального результата а остальные сегменты имеют тип уникального результата, то проверяем весь результат на уникальность
                if (SegmentCollection.IsAnyUniqueType || ResultMode == FinalResultMode.Unique)
                {
                    string actualResult = Result;
                    // проверям если уникальный результат, если не уникальный то ретурним этот метод и не элапсируем эвент
                    if (actualResult.Equals(_prevResult))
                    {
                        AddLogForm(Log3NetSeverity.Max, "Segment Not Changed. Prev Value Is Identical To Current");
                        return;
                    }
                    _prevResult = actualResult;
                }
                else
                {
                    _prevResult = Result;
                }
                if (QueueInitiation.Count > 0)
                    QueueInitiation.Clear();
                IsAlreadyElapsed = true;
                //элапсируем эвент
                XPack result = new XPack("Result", _prevResult);
                ResourcesChanged?.Invoke(this, result);
            }
            catch (Exception ex)
            {
                AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                IsAlreadyElapsed = false;
                AddLog(Log3NetSeverity.Max, "End");
            }
        }

        public override string ToString()
        {
            return XString.Format(new[] { "ResourceKernel", "" }, Input, base.ToString());
        }

        /// <summary>
        /// Метод для сбора статистики.
        /// </summary>
        internal void GetStatistic()
        {
            foreach (ResourceHandler handler in HandlerCollection)
                handler.GetStatistic();
        }
        public void Dispose()
        {
            if (ProcessingMode == ResourceMode.Core)
                monitorThread.Abort();
            SegmentCollection.Dispose();
            ResourcesChanged = null;
        }
    }
}