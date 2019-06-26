using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utils.TYPES;

namespace Utils.ConditionEx.Base
{
    public struct DynamicParameter
    {
        const int MILLISECONDS_DELAY = 10;

        DateTime _lastRequest;
        private Parameter _dynamicValue;
        private readonly Expression _base;

        public string Source { get; }
        public Parameter Result
        {
            get
            {
                var req = DateTime.Now.Subtract(_lastRequest);
                if (req.TotalMilliseconds <= MILLISECONDS_DELAY)
                    return _dynamicValue;

                var input = _base?.DynamicObject != null ? _base.DynamicObject.GetValue.Invoke(Source) : Source;
                var type = TYPES.GetTypeParam(input);

                switch (type)
                {
                    case TypeParam.MathEx:
                    {
                        double doubleResult = MATH.Calculate(input);
                        if (double.IsNaN(doubleResult))
                        {
                            type = TypeParam.String;
                        }
                        else
                        {
                            input = doubleResult.ToString();
                            type = TypeParam.Number;
                        }

                        break;
                    }
                    case TypeParam.Bool:
                        input = input.Trim();
                        break;
                }

                _lastRequest = DateTime.Now;
                _dynamicValue = new Parameter(input, type);
                return _dynamicValue;
            }
            private set => _dynamicValue = value;
        }

        public DynamicParameter(string input, Expression @base)
        {
            Source = input;
            _base = @base;
            _lastRequest = DateTime.MinValue;
            _dynamicValue = Parameter.NaN;
        }

        public override string ToString()
        {
            return Source;
        }
    }
}
