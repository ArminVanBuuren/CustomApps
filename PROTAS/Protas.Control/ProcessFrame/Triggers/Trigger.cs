using System;
using System.Collections.Generic;
using Protas.Components.PerformanceLog;
using Protas.Components.XPackage;
using Protas.Control.ProcessFrame.Components;
using Protas.Control.Resource;
using Protas.Control.Resource.Base;

namespace Protas.Control.ProcessFrame.Triggers
{
    internal abstract class Trigger : ShellLog3Net, IDisposable
    {
        public const string NODE_CASE = @"Case";
        public const string NODE_REMOTE = @"Remote";
        public const string ATTR_TRIGGER_CALL = @"Call";

        public delegate ResourceComplex getComplex(XPack result);
        public getComplex GetComplex;
        ~Trigger()
        {
            AddLogForm(Log3NetSeverity.Debug, "Disposed....");
        }
        public int Id { get; }
        protected Dictionary<string, IHandler> UseHandlers { get; } = new Dictionary<string, IHandler>();
        protected XPack MainPack { get; }
        protected Process MainProcess { get; }

        protected string TriggerName { get; }
        protected string ContextName { get; }
        protected ResourceContextPack TriggerContexts { get; }
        private ResourceComplex TriggerComplex { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Уникальный номер триггера в процессе</param>
        /// <param name="pack">пакет с динамическими паораметрами</param>
        /// <param name="process">Родительский процесс</param>
        protected Trigger(int id, XPack pack, Process process) : base(process)
        {
            Id = id;
            MainPack = pack;
            MainProcess = process;
            string[] packNameAndContextName = pack.Name.Split('.');
            TriggerName = packNameAndContextName[0];

            if (packNameAndContextName.Length > 1)
            {
                //название контекста для триггера
                ContextName = packNameAndContextName[1];
                GetComplex = GetComplexWithClearAndAddSecondary;
                TraceAddPostfix(string.Format("[{0}.{1}:{2}]", ContextName, TriggerName, Id));
            }
            else
            {
                TraceAddPostfix(string.Format("[{0}:{1}]", TriggerName, Id));
                GetComplex = GetComplexWithClear;
            }


            //забираем из основного класса процесса, необходимые хэндлеры для данного триггера
            foreach (XPack casePack in MainPack.ChildPacks)
            {
                string callName;
                if (!casePack.Attributes.TryGetValue(ATTR_TRIGGER_CALL, out callName))
                    continue;

                //Инициализируем новые хэндлеры
                IHandler hnd = null;
                //если необхимо вызвать удаленный таск
                if (casePack.Name.Equals(NODE_REMOTE, StringComparison.CurrentCultureIgnoreCase))
                    hnd = MainProcess.GetRemoteBinding(callName);
                //если необходимо вызвать конкретный хендлер
                else if (casePack.Name.Equals(NODE_CASE, StringComparison.CurrentCultureIgnoreCase))
                    hnd = MainProcess.GetIHandler(callName);

                if (hnd != null)
                    UseHandlers.Add(callName, hnd);
            }

            if (UseHandlers.Count == 0)
                throw new Exception("Call Handlers Not Finded Or Call Handlers Is Incorrect");

            TriggerContexts = new ResourceContextPack(MainProcess.ContextCollection);
            TriggerComplex = MainProcess.Resource.GetComlex(TriggerContexts);
        }

        ResourceComplex GetComplexWithClear(XPack result)
        {
            TriggerContexts.ClearSecondary();
            return TriggerComplex;
        }

        /// <summary>
        /// Создаем новый динамичный контекст. На самом деле он статичный, мы создаем только лишь новую оболочку по случаю прихода нового результата. 
        /// Чтобы при обработке последующих хэндлеров, они могли основываться на актуальных значений. (ни новых не старых, а именно в момент элапсирования триггера)
        /// </summary>
        /// <param name="result">Текущий результат</param>
        /// <returns></returns>
        ResourceComplex GetComplexWithClearAndAddSecondary(XPack result)
        {
            TriggerContexts.ClearSecondary();
            TriggerContexts.AddSecondary(ContextName, new ContextDynamic(MainPack.Attributes, result));
            return TriggerComplex;
        }

        /// <summary>
        /// Подготовка ресурсов к вызову хендлера, Сам процес вызова, с проверкой ConditionEx.
        /// </summary>
        /// <param name="complex">Актуальный комплексный ресурс со всеми, актуальными и необходимыми контекстами</param>
        protected void CallHandlers(ResourceComplex complex)
        {
            //if (CheckConditionVoid != null && !CheckConditionVoid.Invoke(complex))
            //    return;

            foreach (KeyValuePair<string, IHandler> handlerAttribute in UseHandlers)
            {
                AddLogForm(Log3NetSeverity.Debug, "Start Handler=\"{0}\"", handlerAttribute.Key);
                handlerAttribute.Value.Run(complex);
            }
        }



        public virtual void Dispose()
        {

        }
    }
}
