using System;

namespace Utils.ConditionEx.Base
{
    public struct Parameter : IComparable
    {
        public static Parameter NaN = new Parameter(double.NaN.ToString(), TypeParam.NaN);

        public Parameter(string value, TypeParam type)
        {
            Value = value;
            Type = type;
        }
        public string Value { get; }
        public TypeParam Type { get; }

        public int CompareTo(object obj)
        {
            if (!(obj is Parameter))
                return 1;

            var input = (Parameter) obj;
            if (input.Value == Value && input.Type == Type)
                return 0;

            return 1;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is Parameter other && Equals(other);
        }

        public bool Equals(Parameter other)
        {
            return string.Equals(Value, other.Value) && Type == other.Type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Value != null ? Value.GetHashCode() : 0) * 397) ^ (int)Type;
            }
        }

        public static bool operator ==(Parameter first, Parameter second)
        {
            return first.Value == second.Value && first.Type == second.Type;
        }

        public static bool operator !=(Parameter first, Parameter second)
        {
            return !(first == second);
        }
    }
}