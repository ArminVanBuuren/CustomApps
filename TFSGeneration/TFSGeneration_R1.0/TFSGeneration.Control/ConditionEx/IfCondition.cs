using System;
using System.Text;
using TFSAssist.Control.ConditionEx.Utils;

namespace TFSAssist.Control.ConditionEx
{
	public class IfConditionException : Exception
	{
		public IfConditionException(string message) : base(message)
		{
			
		}
	}

	public class IfCondition
    {
        public IfTarget ExpressionEx(string str)
        {
            _waitSymol = 0;
            _closeCommand = 0;
            if (string.IsNullOrEmpty(str))
                return new IfTarget();
            Builder pac = new Builder();
            StringBuilder temp = new StringBuilder();
            int state = 0;
            foreach (char ch in str)
            {
                switch (state)
                {
                    case 0:
                        {
                            state = WaitStartOfParam(pac, temp, state, ch);
                            continue;
                        }
                    case 1:
                        {
                            state = WaitEndOfParam(pac, temp, state, ch);
                            continue;
                        }

                    case 2:
                        {
                            state = WaitResources(temp, state, ch);
                            continue;
                        }
                    default:
                        {
                            continue;
                        }
                }
            }

            if (temp.Length > 0)
            {
                pac.CheckSymbol(temp.ToString());
                temp.Remove(0, temp.Length);
            }
            //проверяем на синтаксис
            bool isParty = StaffFunk.IsParity(_waitSymol);
            if (isParty && _closeCommand == 0 && _waitSymol >= 4)
            {
                IfTarget bc = pac.ToBlock();
                //bool bb = bc.ResultCondition;
                if (bc.ValidationStructure && bc.StringConstructor != string.Empty)
                {
                    return bc;
                }
            }
			throw new IfConditionException("Syntax Is InCorrect");
        }

        int _waitSymol = 0;
        //0
        int WaitStartOfParam(Builder pac, StringBuilder temp, int state, char c)
        {
            if (c == '\'')
            {
                _waitSymol++;
                if (temp.Length > 0)
                {
                    pac.CheckSymbol(temp.ToString());
                    temp.Remove(0, temp.Length);
                }
                return 1;
            }
            temp.Append(c);
            return state;
        }

        //1
        int WaitEndOfParam(Builder pac, StringBuilder temp, int state, char c)
        {
            if (c == '{')
            {
                temp.Append(c);
                return 2;
            }

            if (c == '\'')
            {
                _waitSymol++;
                pac.AddConditionParameter(temp.ToString());
                temp.Remove(0, temp.Length);
                return 0;
            }
            temp.Append(c);
            return state;
        }

        int _closeCommand = 0;
        //2
        int WaitResources(StringBuilder temp, int state, char c)
        {
            if (c == '{')
            {
                if (_closeCommand == 0)
                    _closeCommand++;
                _closeCommand++;
            }
            else if (c == '}')
            {
                if (_closeCommand > 0)
                    _closeCommand--;
                temp.Append(c);
                if (_closeCommand == 0)
                    return 1;
                return 2;
            }
            temp.Append(c);
            return state;
        }
    }
}