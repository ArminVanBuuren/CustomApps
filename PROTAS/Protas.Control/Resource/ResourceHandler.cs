using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Protas.Components.Functions;
using Protas.Components.PerformanceLog;
using Protas.Control.Resource.Base;
using Protas.Components.XPackage;

namespace Protas.Control.Resource
{
    internal class ResourceHandler : ShellLog3Net, IDisposable
    {
        ~ResourceHandler()
        {
            AddLog(Log3NetSeverity.MaxErr, Header);
        }
        internal List<Task> list = new List<Task>();
        public RSegmentEvent HandlerChanged { get; set; } = new RSegmentEvent();
        XPack _result;
        ulong _totalAmount = 0;
        delegate XPack GetDynamicResult();
        GetDynamicResult _getResult;
        int _countOfCoreModeUse = 0;
        public int Id { get; }
        public string Header { get; }
        public ResourceShell Shell { get; }
        public ResourceConstructor Constructor { get; }
        /// <summary>
        /// Контекстный ресурс, непосредственно который мы будем вызыватьы
        /// </summary>
        public IResource Resource { get; }
        public bool IsResource { get; } = false;
        public bool HandlerEventElapsed { get; private set; } = false;
        /// <summary>
        /// Дата последнего присваивания или удаления ссылки экземпляра данной сигнатуры к сегменту
        /// </summary>
        public DateTime TimeForLastAppeal { get; private set; } = DateTime.Now;
        //int fatalerr = 1;
        ///// <summary>
        ///// Свосйстово возвращает количество сегментов которые используют данную сигнатуру в режиме ядра
        ///// </summary>
        internal int UseCoreMode
        {
            get { return _countOfCoreModeUse; }
            set
            {
                if (!IsResource || Resource?.Type == ResultType.Constant || Resource == null)
                    return;
                _countOfCoreModeUse = value;
                if (_countOfCoreModeUse <= 0)
                {
                    if (Resource.IsTrigger)
                    {
                        Resource.IsTrigger = false;
                        //if (_countOfCoreModeUse < 0)
                        //{
                        //    AddLogForm(Log3NetSeverity.Fatal, "Fatal Error Count:{0}", fatalerr++);
                        //    SourceLog3Net.LogSeverity = Log3NetSeverity.Max;
                        //}
                        AddLogForm(Log3NetSeverity.MaxErr, "Is Off | CountUse:{0}", _countOfCoreModeUse);
                    }
                }
                else if (!Resource.IsTrigger)
                {
                    Resource.IsTrigger = true;
                    AddLogForm(Log3NetSeverity.MaxErr, "Is On | CountUse:{0}", _countOfCoreModeUse);
                }
            }
        }

        /// <summary>
        /// Определяем из строки Название операции и её свойства (функциональные, процедурные или оба)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="constructor">Пакет необходимых пааметров, контекст</param>
        /// <param name="log"></param>
        /// <param name="shell"></param>
        public ResourceHandler(int id, ResourceShell shell, ResourceConstructor constructor, Log3Net log) : base(log)
        {
            try
            {
                Id = id;
                Shell = shell;
                Constructor = constructor;

                if (Shell == null || Constructor == null)
                {
                    AddLogForm(Log3NetSeverity.Error, "Exception: Shell={0} Or Constructor={1} Is NULL", Shell, Constructor);
                    return;
                }

                Header = string.Format("{0}:{1}({2})", Shell.ContextName, Shell.ResourceName, Constructor.AsLine);

                string ste = string.Format("HND:{0}%%{1}%%", Id, Header);
                AddMessagePrefix(ste);


                //инициализируем непосредтвекнный ресурс IResource по дефолтному конструктору (в данном случае с ResourceConstructor и Log3Net) или вообще без конструктора
                if (Shell.Entity.Mode == EntityMode.None)
                {
                    List<object> defaultClassConstructor = new List<object> { Constructor, log };
                    object potentialResource = ProtasActivator.CreateInstance(Shell.Entity.ResourceLink, defaultClassConstructor);
                    if (potentialResource == null)
                    {
                        AddLogObj(Log3NetSeverity.Error, "Exception When Create Instance Of {0} With Default Constructor {1}", Shell.Entity, defaultClassConstructor);
                        return;
                    }

                    Resource = potentialResource as IResource;
                    if (Resource == null)
                    {
                        AddLogObj(Log3NetSeverity.Error, "Exception When Initialize {0}! Created Instance Type Is {1}", typeof(IResource), potentialResource);
                        return;
                    }
                }
                //если у нас проинициализировался уже готовый ресурс
                else if (Shell.Entity.Mode == EntityMode.Ready)
                {
                    Resource = Shell.Entity.ReadyResource;

                    if (Resource == null)
                    {
                        AddLogForm(Log3NetSeverity.Error, "Exception When Initialize {0}! ReadyResource Is Null", Shell.Entity);
                        return;
                    }
                }
                //Создание необычного ресурса. Где необходима инициализация непосредственно из конекста
                else
                {
                    Resource = Shell.Context.GetResource(Shell.Entity.ResourceLink, Constructor);

                    if (Resource == null)
                    {
                        AddLogObj(Log3NetSeverity.Error, "Exception When Get Instance From {0}!", Shell.Context);
                        return;
                    }
                }


                //если ресурс успешно проинициализировался
                if (Resource.IsIntialized)
                {
                    IsResource = true;
                    //если тип непосредственного ресурса является статичным (как переменная) то всегда возвращаем статичный рещультат что получили при первом запросе
                    if (Resource.Type == ResultType.Constant)
                    {
                        _result = Resource.GetResult();
                        _getResult = GetConstantResult;
                    }
                    else
                    {
                        if (Resource.ResourceChanged == null)
                            Resource.ResourceChanged = new RHandlerEvent();
                        Resource.ResourceChanged += new RHandlerEvent.CallRHandler(IsResourceChanged);
                        //Resource.ResourceChanged.AddCallback(IsResourceChanged); //+= new ResourceEventHandler.CallHandler(IsResourceChanged);
                        //т.к. наш ресурс не константа, то нужно анализировать время получения результата ресурса
                        _getResult = GetResultWithCollectionStatistic;
                    }
                    AddLogObj(Log3NetSeverity.MaxErr, "Resource Successfully Initialized By {0}", Shell.Entity);
                }
            }
            catch (Exception ex)
            {
                AddEx(ex);
            }
            finally
            {
                if (!IsResource)
                {
                    //инициализируем возврат метода где результат является пустым статичным значением, т.к. наше ресурс не создался или ошибочно проинициализировался
                    _result = new XPack();
                    _getResult = GetConstantResult;
                    AddLogObj(Log3NetSeverity.Debug, "Resource UnSuccessfully Initialized By {0} Resource={1}", Shell.Entity, Resource);
                }
            }
        }


        /// <summary>
        /// Возврашаем обратно любые значения результатов
        /// </summary>
        /// <param name="provokingResource">Источник объкта который элапсирует эвент</param>
        /// <param name="result">Результат непосредственного ресурса</param>
        void IsResourceChanged(IResource provokingResource, XPack result)
        {
            //не менять порядок выполнения!!!
            //Main.Type == ResourceMode.Independent это условие проверяем на всякий случай, с самостоятельным модом ресурса эвенты не должны приходить
            //пропускаем проверку - если эвент данной сигнатуры уже выполняется (как в методе IsSignatureChangedUnique), т.к. не важно выпоняется эвент или нет новый результат должен быть получен, (если брать пример с HLR, то все входные данные должен обабатывать безусловно)
            if (HandlerChanged == null || provokingResource == null || !ReferenceEquals(Resource, provokingResource) || result == null ||  Resource.Type == ResultType.Constant)
                return;
            try
            {
                HandlerEventElapsed = true;
                AddLogForm(Log3NetSeverity.Max, "Start | PrevResult = \"({1})\"", _result);
                //получаем результат ресурса
                _result = result;
                //инициируем эвент в сегменте
                HandlerChanged?.Invoke(this, _result);
            }
            catch (Exception ex)
            {
                AddEx(ex);
            }
            finally
            {
                HandlerEventElapsed = false;
                AddLogForm(Log3NetSeverity.Max, "End | LastResult = \"({1})\"", _result);
            }
        }

        /// <summary>
        /// результат метода динамического ресурса
        /// </summary>
        public XPack GetResult()
        {
            if (HandlerEventElapsed)
            {
                //если определение ресурса не бесконечное новое то возврщаем старое значение ресурса что у нас получилось в вызове эвентного метода IsSignatureChanged
                if (Resource.Type == ResultType.Specific)
                    return _result;
            }
            return _getResult.Invoke();
        }

        /// <summary>
        /// возвращаем статичный результат если ресурс не был найлен или является статичным (permanent)
        /// </summary>
        /// <returns></returns>
        XPack GetConstantResult()
        {
            TimeForLastAppeal = DateTime.Now;
            return _result;
        }

        /// <summary>
        /// вызываем непосрдественный ресурс и возвращаем результат его выполнения и собираем статистику
        /// </summary>
        /// <returns></returns>
        XPack GetResultWithCollectionStatistic()
        {
            try
            {
                TimeForLastAppeal = DateTime.Now;
                _result = Resource.GetResult();
                RefreshStatistic(TimeForLastAppeal);
            }
            catch (Exception ex)
            {
                GetResultException(ex);
            }
            return _result;
        }

        void GetResultException(Exception ex)
        {
            //ошибка при вызове результата непосредственного ресурса
            AddLogForm(Log3NetSeverity.Error, "Failure To Get Resource Result. Exception:{0}\r\n{1}", ex.Message, ex.StackTrace);
        }

        void RefreshStatistic(DateTime dateStart)
        {
            //если не пишем репорты то не собираем статистику
            if (MainLog3Net == null || MainLog3Net?.ReportTimeOutMsec <= 0)
                return;
            ulong currentInterval = 0;
            try
            {
                //проверяем чтобы количество вызовов не было больше допустимой длинны переменной
                CountProcessRuns++;
            }
            catch (Exception ex)
            {
                //если общее колличестуо секунд превысило значние в ulong то выполняется эксепшн
                AddLogForm(Log3NetSeverity.MaxErr, "CountRuns Or TotalAmount Is Too Large. Parameters Reset. Exception:{0}\r\n{1}", ex.Message, ex.StackTrace);
                CountProcessRuns = 0;
                _totalAmount = 0;
            }
            finally
            {
                try
                {
                    //пробуем получить колличество миилисекунд на выполнение метода, если метод выполнялся очень долго то выполняется эксепшн
                    //проверяем чтобы количество миллисекунд не было больше допустимой длинны переменной
                    ulong.TryParse(DateTime.Now.Subtract(dateStart).TotalMilliseconds.ToString(CultureInfo.InvariantCulture), out currentInterval);
                }
                catch (Exception ex)
                {
                    AddLogForm(Log3NetSeverity.MaxErr, "Interval To Get Resource Result - Too Large. Exception:{0}\r\n{1}", ex.Message, ex.StackTrace);
                    currentInterval = 0;
                }
                finally
                {
                    _totalAmount = _totalAmount + currentInterval;
                }
            }
        }

        public void GetStatistic()
        {
            if (IsResource && Resource?.Type != ResultType.Constant && CountProcessRuns > 0)
                AddLogForm(Log3NetSeverity.Report, "Count Process Runs:{0}\r\nAverage Interval ProcessMSec:{1}",
                    CountProcessRuns,
                    AverageIntervalProcessMSec);
            CountProcessRuns = 0;
            _totalAmount = 0;
        }

        /// <summary>
        /// колличество вызовов динамического метода
        /// </summary>
        public ulong CountProcessRuns { get; set; } = 0;

        /// <summary>
        /// общее колличесо секунд на выполнение метода, за все вызовы
        /// </summary>
        public double AverageIntervalProcessMSec
        {
            get
            {
                if (CountProcessRuns == 0)
                    return 0;
                return (_totalAmount / CountProcessRuns) * 1;
            }
        }
        public override string ToString()
        {
            return Header;
        }

        public void Dispose()
        {
            if (IsResource)
                Resource.Dispose();
        }
    }
}