using System.Collections.Generic;
using Protas.Components.PerformanceLog;
using Protas.Control.Resource;
using Protas.Components.XPackage;

namespace Protas.Control.ProcessFrame
{
    internal abstract class ConditionExShell : ShellLog3Net
    {
        public const string ATTR_CONDITION = "Condition";
        protected delegate KeyValuePair<bool, XPack> CheckCondition(ResourceComplex complex);
        protected CheckCondition CheckConditionVoid;
        public string ConditionEx { get; }
        public ConditionExShell(XPack pack, ILog3NetMain log) : base(log)
        {
            ConditionEx = pack.Attributes[ATTR_CONDITION];
            //если в случае инициирования(элапсирования) триггера нам необходима определенная проверка для вызова хендлеровов
            if (!string.IsNullOrEmpty(ConditionEx))
                CheckConditionVoid = CheckConditionEx;
        }

        /// <summary>
        /// В случае инициирования(элапсирования) проверям условия триггера ConditionEx
        /// </summary>
        /// <param name="complex">готовый пакет с ресурсами</param>
        /// <returns></returns>
        protected KeyValuePair<bool, XPack> CheckConditionEx(ResourceComplex complex)
        {
            bool condResult;
            string resultCondition = complex.GetResourceValues(ConditionEx);
            if (string.IsNullOrEmpty(resultCondition))
                condResult = false;
            else
                bool.TryParse(resultCondition, out condResult);
            AddLogForm(Log3NetSeverity.Debug, "Result Condition=\"{0}\"", condResult);

            return new KeyValuePair<bool, XPack>(condResult, new XPack("ConditionResult", string.Format("Condition=\"{0}\", Result=\"{1}\"", resultCondition, condResult)));
        }
    }
}
