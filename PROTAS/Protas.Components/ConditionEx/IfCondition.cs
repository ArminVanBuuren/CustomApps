using System.Text;
using Protas.Components.Functions;
using Protas.Components.PerformanceLog;

namespace Protas.Components.ConditionEx
{
    public class IfCondition : ShellLog3Net
    {
        public bool СheckingResource { get; } = false;
        public IfCondition()
        {

        }
        public IfCondition(ShellLog3Net log):base(log)
        {
            
        }

        public IfCondition(bool checkingresource, ShellLog3Net log) : base(log)
        {
            СheckingResource = checkingresource;
        }
        public IfTarget ExpressionEx(string str)
        {
            AddLogForm(Log3NetSeverity.Max, "Input Expression={0}", str);
            _waitSymol = 0;
            _closeCommand = 0;
            if (string.IsNullOrEmpty(str))
                return new IfTarget(this);
            Builder pac = new Builder(this);
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
            bool isParty = ProtasFunk.IsParity(_waitSymol);
            if (isParty && _closeCommand == 0 && _waitSymol >= 4)
            {
                IfTarget bc = pac.ToBlock();
                //bool bb = bc.ResultCondition;
                if (bc.ValidationStructure && bc.StringConstructor != string.Empty)
                {
                    AddLog(Log3NetSeverity.Max, "Syntax And Patterns Is Correct");
                    return bc;
                }
            }
            AddLog(Log3NetSeverity.Max, "Syntax Is InCorrect");
            return null;
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
            else if (c == '\'')
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