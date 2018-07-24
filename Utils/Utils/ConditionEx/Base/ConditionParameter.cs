using Utils.ConditionEx.Utils;

namespace Utils.ConditionEx.Base
{
    public class ConditionParameter
    {
        Parameter _param;
        public ConditionParameter(string source)
        {
            _param = new Parameter(source);
        }
        public string SourceParam => _param.Source;
        public Parameter DynamicParam => _param;
    }
}