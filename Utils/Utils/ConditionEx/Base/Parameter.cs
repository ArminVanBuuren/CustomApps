namespace Utils.ConditionEx.Base
{
	public struct Parameter
	{
        public string Source { get; }
        public string Value { get; }
        public TYPES.TypeParam Type { get; }

        public Parameter(string input)
		{
			Source = input;

            Type = TYPES.GetType(Source);
            switch (Type)
            {
                case TYPES.TypeParam.MathEx:
                {
                    double strNum = MATH.Evaluate(input, Type);
                    Value = strNum.ToString();
                    Type = TYPES.TypeParam.Number;
                    break;
                }
                case TYPES.TypeParam.Bool:
                    Value = input.Trim();
                    break;
                default:
                    Value = input;
                    break;
            }
		}
	}
}