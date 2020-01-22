using System;
using System.Text;
using Utils.ConditionEx.Base;
using Utils.ConditionEx.Collections;

namespace Utils.ConditionEx
{
    public class ExpressionBuilder
    {
        ConditionBlock _conditionBlock;
        int _parameretId = 0;
        readonly Expression _base;
        Expression _parent;

        internal ExpressionBuilder(DynamicObject dynamicObj)
        {
            if (dynamicObj != null)
            {
                _base = new Expression(dynamicObj)
                {
                    LogicalGroup = LogicalGroupType.And
                };
            }
            else
            {
                _base = new Expression()
                {
                    LogicalGroup = LogicalGroupType.And
                };
            }

            _parent = new Expression();
            _base.ExpressionList.AddChild(_parent);
            CreateNewCondition();
        }

        public static Expression Calculate(string expression, DynamicObject dynamicFunction = null)
        {
            var waitSymol = 0;
            var closeCommand = 0;
            if (expression.IsNullOrEmptyTrim())
                throw new Exception("Expression is epmty!");

            var normalizeExpression = XML.NormalizeXmlValueFast(expression);

            var builder = new ExpressionBuilder(dynamicFunction);
            var temp = new StringBuilder();
            var state = 0;
            foreach (var ch in normalizeExpression)
            {
                switch (state)
                {
                    case 0:
                    {
                        state = WaitStartOfParam(builder, temp, state, ch, ref waitSymol);
                        continue;
                    }
                    case 1:
                    {
                        state = WaitEndOfParam(builder, temp, state, ch, ref waitSymol);
                        continue;
                    }

                    case 2:
                    {
                        state = WaitResources(temp, state, ch, ref closeCommand);
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
                builder.CheckSymbol(temp.ToString());
                temp.Remove(0, temp.Length);
            }

            if (waitSymol.IsParity() && closeCommand == 0 && waitSymol >= 4)
            {
                var conditionEx = builder.ToBlock();
                return conditionEx;
            }

            throw new ExpressionBuilderException($"Syntax of expression '{expression}' is incorrect.");
        }

        static int WaitStartOfParam(ExpressionBuilder pac, StringBuilder temp, int state, char c, ref int waitSymol)
        {
            if (c == '\'')
            {
                waitSymol++;
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

        static int WaitEndOfParam(ExpressionBuilder pac, StringBuilder temp, int state, char c, ref int waitSymol)
        {
            if (c == '{')
            {
                temp.Append(c);
                return 2;
            }

            if (c == '\'')
            {
                waitSymol++;
                pac.AddConditionParameter(temp.ToString());
                temp.Remove(0, temp.Length);
                return 0;
            }
            temp.Append(c);
            return state;
        }

        static int WaitResources(StringBuilder temp, int state, char c, ref int closeCommand)
        {
            if (c == '{')
            {
                if (closeCommand == 0)
                    closeCommand++;
                closeCommand++;
            }
            else if (c == '}')
            {
                if (closeCommand > 0)
                    closeCommand--;
                temp.Append(c);
                if (closeCommand == 0)
                    return 1;
                return 2;
            }
            temp.Append(c);
            return state;
        }


        /// <summary>
        /// Получаем сформированный блок документ из всех condition
        /// </summary>
        /// <returns></returns>
        Expression ToBlock()
        {
            IsCreateNewCondition();
            var block = _parent;
            var countBlocks = 1;
            while (GetBaseBlock(block) != null)
            {
                countBlocks++;
                block = GetBaseBlock(block);
                if (countBlocks > 1000)
                {
                    break;
                }
            }
            if (countBlocks > 1000)
            {
                throw new Exception("Please check your expression!");
            }
            return block;
        }

        /// <summary>
        /// Получаем только родительские блоки
        /// </summary>
        /// <returns></returns>
        Expression GetBaseBlock(Expression blk)
        {
            return blk.Parent;
        }

        /// <summary>
        /// Если нашелся оператор AND
        /// </summary>
        void LogicalGroupAnd()
        {
            //если в текущем блоке есть подблоки выражений
            if (_parent.ExpressionList.Count > 0)
            {
                //если текущий Parent блок реализует группировку в виде скобок () то создаем новый блок 
                //и добавляем его в текущий Parent, потом пересоздаем новый блок в текущий блок Parent
                //и в новый уже добавляем обработанное условие
                if (_parent.LogicalGroup == LogicalGroupType.And)
                {
                    var newChildBlock = new Expression();
                    _parent.ExpressionList.AddChild(newChildBlock);
                    _parent = newChildBlock;
                }
                else if (_parent.LogicalGroup == LogicalGroupType.Or)
                {
                    //Если оператор AND, а в текущем блоке обрабатывается операция OR, то создаем новый блок с операцией AND
                    //и туда запихиваем в подблоки уже текущие обработанные операции с OR
                    LogicalGroupIfOpenBkt();
                    return;
                }
            }
            //добавляем в текущий Parent блок обработанное условие condition если в данном блоке не обрабатывется операторы OR и скобки 
            CreateNewCondition();
        }

        /// <summary>
        /// Если нашелся оператор OR
        /// </summary>
        void LogicalGroupOr()
        {
            if (_parent.Parent != null)
            {
                // Если в родительском блоке уже обрабатывался опереатор OR то добавляем еще один новый блок в родительский
                //и делаем новый блок текущим - Parent
                //или если блоков всего один и стоит оператор AND он стоит как безусловные если открылась скобка
                //то переприсваемваем оператор на OR
                if (_parent.Parent.LogicalGroup == LogicalGroupType.Or || _parent.Parent.ExpressionList.Count == 1)
                {
                    CreateNewCondition();
                    var newParentBlock = _parent.Parent;
                    newParentBlock.LogicalGroup = LogicalGroupType.Or;
                    _parent = new Expression();
                    newParentBlock.ExpressionList.AddChild(_parent);
                    return;
                }
            }

            CreateNewOrBlock();
        }

        /// <summary>
        /// Cоздаем новый блок если нашелся первый оператор OR
        /// </summary>
        void CreateNewOrBlock()
        {
            CreateNewCondition();
            //создаем новый родительский блок
            var newParentBlock = new Expression
            {
                //ставим метку что обрабатывается оператор OR
                LogicalGroup = LogicalGroupType.Or
            };

            if (_parent.Parent != null)
            {
                //если у данного блока есть родитель, то присваиваем этого родителя к создавшемуся блоку
                _parent.Parent.ExpressionList.AddChild(newParentBlock);
                newParentBlock.Parent.ExpressionList.RemoveChild(_parent);
            }
            //добавляем в подблок нового родителя, блок который уже сформировался
            newParentBlock.ExpressionList.AddChild(_parent);
            //создаем новый текущий блок и и добавляем его в новый создавшеся родительский
            //т.е это означает что первый параметр сравнивается с другим только через оператор OR
            //P1 (уже обработанный и добавленный в новый родительский) OR P2 (новый создавшеся блок который становится текущим Parent)
            _parent = new Expression();
            newParentBlock.ExpressionList.AddChild(_parent);
        }

        void LogicalGroupIfOpenBkt()
        {
            CreateNewCondition();
            //создаем новую коллекцию блоков в новом родительском блоке с оператором AND
            var newParentBlock = new Expression()
            {
                LogicalGroup = LogicalGroupType.And
            };
            if (_parent.Parent != null)
            {
                //переприсваеваем родительский блок
                _parent.Parent.ExpressionList.AddChild(newParentBlock);
                newParentBlock.Parent.ExpressionList.RemoveChild(_parent);
            }
            newParentBlock.ExpressionList.AddChild(_parent);

            if ((_parent.ExpressionList.Count > 0 && (_parent.LogicalGroup == LogicalGroupType.And || _parent.LogicalGroup == LogicalGroupType.Or)) || _parent.ConditionList.Count > 0)
            {
                //создаем новый блок который будет обрабатывать операции внутри наших скобок
                var newAndChild = new Expression();
                newParentBlock.ExpressionList.AddChild(newAndChild);
                _parent = newAndChild;
            }
        }

        void LogicalGroupIfCloseBkt()
        {
            //только записываем условия в текущий блок
            IsCreateNewCondition();
            if (_parent.Parent != null)
            {
                //возвращаемся к родительскому условию т.к. закрылись скобки и блок выражения завершен
                _parent = _parent.Parent;
            }
            else
            {
                throw new ArgumentException("Incorrect position of symbol \")\" in your expression!");
            }
        }

        void CreateNewCondition()
        {
            //если данное условие еще не заполнилось параметрами то не создаем новый 
            if (!IsCreateNewCondition())
            {
                return;
            }
            _conditionBlock = new ConditionBlock();
            CreateNewParameters();
        }

        bool IsCreateNewCondition()
        {
            //добавляем готовое условие к текущему блоку к примеру если условие 'p1'='p2' and ('p1'....) or 'p1'='p2'
            if (_conditionBlock != null)
            {
                //если параметры не заполнены то не доабвляем условие в текущий блок
                if (_conditionBlock.Count > 0)
                {
                    AddNewCondition(_parent);
                }
                else if (_conditionBlock.Parent == null)
                {
                    return false;
                }
            }
            _conditionBlock = null;
            return true;
        }

        //добавляем условие в текущий блок
        void AddNewCondition(Expression bc)
        {
            bc.ConditionList.AddChild(_conditionBlock);
        }

        void CreateNewParameters()
        {
            _parameretId = 0;
        }

        string _firstParam = string.Empty;
        ConditionOperatorType _operator = ConditionOperatorType.Unknown;
        /// <summary>
        /// Обрабатываем condition, подставляем первый и второй параметер
        /// </summary>
        void AddConditionParameter(string parameter)
        {
            _parameretId++;
            if (_parameretId == 1)
                _firstParam = parameter;
            else
            {
                _base.AddCondition(_conditionBlock, _firstParam, parameter, _operator);
                _parameretId = 0;
            }
        }

        /// <summary>
        /// Обрабатываем знак или операторы между параметрами это может быть знак равно, больше меньше
        /// или AND, OR или скобки означающаю группировку выражения
        /// </summary>
        void CheckSymbol(string str)
        {
            var i = 0;
            foreach (var sCheck in str.Split(' '))
            {
                if (string.IsNullOrEmpty(sCheck))
                    continue;

                var cbo = Condition.GetOperator(sCheck);
                if (cbo == ConditionOperatorType.Unknown)
                {
                    switch (Expression.GetLogicalGroup(sCheck))
                    {
                        case LogicalGroupType.And:
                            if (i == 0)
                                LogicalGroupAnd();
                            i++;
                            break;
                        case LogicalGroupType.Or:
                            if (i == 0)
                                LogicalGroupOr();
                            i++;
                            break;
                        case LogicalGroupType.BracketIsOpen:
                            LogicalGroupIfOpenBkt();
                            break;
                        case LogicalGroupType.BracketIsClose:
                            LogicalGroupIfCloseBkt();
                            break;
                        default:
                            _operator = ConditionOperatorType.Unknown;
                            break;
                    }
                }
                else
                {
                    if (_parameretId == 0)
                    {
                        CreateNewParameters();
                        _parameretId++;
                    }
                    _operator = cbo;
                    return;
                }
            }
        }
    }

    internal enum LogicalCondition
    {
        Operator = 0,
        Group = 1,
        Unknown = 2
    }

}
