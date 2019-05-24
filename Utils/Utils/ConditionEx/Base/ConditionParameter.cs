namespace Utils.ConditionEx.Base
{
    public class ConditionParameter
    {
        public ConditionParameter(string source)
        {
            DynamicParam = new Parameter(source);
        }
        public string SourceParam => DynamicParam.Source;
        public Parameter DynamicParam { get; }
    }
}