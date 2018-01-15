using Protas.Components.PerformanceLog;
using Protas.Components.Types;

namespace Protas.Components.ConditionEx.Base
{
    public class ConditionParameter : ShellLog3Net
    {
        Parameter _param;
        public ConditionParameter(string source, ShellLog3Net log) : base(log)
        {
            _param = new Parameter(source);
        }
        public string SourceParam => _param.Source;
        public Parameter DynamicParam => _param;
    }
}