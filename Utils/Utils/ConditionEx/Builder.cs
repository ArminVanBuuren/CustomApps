using Utils.ConditionEx.Base;
using Utils.ConditionEx.Collections;

namespace Utils.ConditionEx
{
    internal class Builder
    {
        IfTarget _parent;
        ConditionBlock _conditionBlock;
        int _parameretId = 0;
        public Builder()
        {
            IfTarget Base = new IfTarget()
            {
                LogicalGroup = LogicalGroup.And
            };
            _parent = new IfTarget();
            Base.ChildGroups.AddChild(_parent);
            CreateNewCondition();
        }

        /// <summary>
        /// Получаем сформированный блок документ из всех condition
        /// </summary>
        /// <returns></returns>
        public IfTarget ToBlock()
        {
            IsCreateNewCondition();
            IfTarget blk = _parent;
            int countBlocks = 1;
            while (GetBaseBlock(blk) != null)
            {
                countBlocks++;
                blk = GetBaseBlock(blk);
                if (countBlocks > 1000)
                {
                    break;
                }
            }
            if (countBlocks > 1000)
            {
                throw new System.ArgumentException("Not write conditionEx! Check your condition or this is Bug");
            }
            return blk;
        }
        /// <summary>
        /// Получаем только родительские блоки
        /// </summary>
        /// <returns></returns>
        IfTarget GetBaseBlock(IfTarget blk)
        {
            return blk.Parent;
        }

        /// <summary>
        /// Если нашелся оператор AND
        /// </summary>
        void LogicalGroupAnd()
        {
            //если в текущем блоке есть подблоки выражений
            if (_parent.ChildGroups.Count > 0)
            {
                //если текущий Parent блок реализует группировку в виде скобок () то создаем новый блок 
                //и добавляем его в текущий Parent, потом пересоздаем новый блок в текущий блок Parent
                //и в новый уже добавляем обработанное условие
                if (_parent.LogicalGroup == LogicalGroup.And)
                {
                    IfTarget newChildBlock = new IfTarget();
                    _parent.ChildGroups.AddChild(newChildBlock);
                    _parent = newChildBlock;
                }
                else if (_parent.LogicalGroup == LogicalGroup.Or)
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
        private void LogicalGroupOr()
        {
            if (_parent.Parent != null)
            {
                // Если в родительском блоке уже обрабатывался опереатор OR то добавляем еще один новый блок в родительский
                //и делаем новый блок текущим - Parent
                //или если блоков всего один и стоит оператор AND он стоит как безусловные если открылась скобка
                //то переприсваемваем оператор на OR
                if (_parent.Parent.LogicalGroup == LogicalGroup.Or || _parent.Parent.ChildGroups.Count == 1)
                {
                    CreateNewCondition();
                    IfTarget newParentBlock = _parent.Parent;
                    newParentBlock.LogicalGroup = LogicalGroup.Or;
                    _parent = new IfTarget();
                    newParentBlock.ChildGroups.AddChild(_parent);
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
            IfTarget newParentBlock = new IfTarget();
            //ставим метку что обрабатывается оператор OR
            newParentBlock.LogicalGroup = LogicalGroup.Or;
            if (_parent.Parent != null)
            {
                //если у данного блока есть родитель, то присваиваем этого родителя к создавшемуся блоку
                _parent.Parent.ChildGroups.AddChild(newParentBlock);
                newParentBlock.Parent.ChildGroups.RemoveChild(_parent);
            }
            //добавляем в подблок нового родителя, блок который уже сформировался
            newParentBlock.ChildGroups.AddChild(_parent);
            //создаем новый текущий блок и и добавляем его в новый создавшеся родительский
            //т.е это означает что первый параметр сравнивается с другим только через оператор OR
            //P1 (уже обработанный и добавленный в новый родительский) OR P2 (новый создавшеся блок который становится текущим Parent)
            _parent = new IfTarget();
            newParentBlock.ChildGroups.AddChild(_parent);
        }

        void LogicalGroupIfOpenBkt()
        {
            CreateNewCondition();
            //создаем новую коллекцию блоков в новом родительском блоке с оператором AND
            IfTarget newParentBlock = new IfTarget()
            {
                LogicalGroup = LogicalGroup.And
            };
            if (_parent.Parent != null)
            {
                //переприсваеваем родительский блок
                _parent.Parent.ChildGroups.AddChild(newParentBlock);
                newParentBlock.Parent.ChildGroups.RemoveChild(_parent);
            }
            newParentBlock.ChildGroups.AddChild(_parent);
            if (_parent.StringConstructor != string.Empty)
            {
                //создаем новый блок который будет обрабатывать операции внутри наших скобок
                IfTarget newAndChild = new IfTarget();
                newParentBlock.ChildGroups.AddChild(newAndChild);
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
                throw new System.ArgumentException("Not write position symbol \")\" in conditionEx!");
            }
        }

        private void CreateNewCondition()
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
                if (_conditionBlock.StringConstructor != string.Empty)
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
        private void AddNewCondition(IfTarget bc)
        {
            bc.BlockConditions.AddChild(_conditionBlock);
        }

        void CreateNewParameters()
        {
            _parameretId = 0;
        }

        string _firstParam = string.Empty;
        ConditionOperator _operator = ConditionOperator.Unknown;
        /// <summary>
        /// Обрабатываем condition, подставляем первый и второй параметер
        /// </summary>
        internal void AddConditionParameter(string parameter)
        {
            _parameretId++;
            if (_parameretId == 1)
                _firstParam = parameter;
            else
            {
                _conditionBlock.AddChild(_firstParam, parameter, _operator);
                _parameretId = 0;
            }
        }

        /// <summary>
        /// Обрабатываем знак или операторы между параметрами это может быть знак равно, больше меньше
        /// или AND, OR или скобки означающаю группировку выражения
        /// </summary>
        public void CheckSymbol(string str)
        {
            int i = 0;
            foreach (string sCheck in str.Split(' '))
            {
                if (string.IsNullOrEmpty(sCheck)) continue;
                ConditionOperator cbo = ComponentCondition.GetOperator(sCheck);
                if (cbo == ConditionOperator.Unknown)
                {
                    switch (ComponentCondition.GetLogicalGroup(sCheck))
                    {
                        case LogicalGroup.And:
                            if (i == 0)
                                LogicalGroupAnd();
                            i++;
                            break;
                        case LogicalGroup.Or:
                            if (i == 0)
                                LogicalGroupOr();
                            i++;
                            break;
                        case LogicalGroup.OpenBkt:
                            LogicalGroupIfOpenBkt();
                            break;
                        case LogicalGroup.CloseBkt:
                            LogicalGroupIfCloseBkt();
                            break;
                        default: break;
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

        public LogicalCondition CheckLogicalCondition(string str)
        {
            LogicalCondition logCond = LogicalCondition.Unknown;
            foreach (string sCheck in str.Split(' '))
            {
                ConditionOperator cbo = ComponentCondition.GetOperator(sCheck);
                if (cbo == ConditionOperator.Unknown)
                {
                    if (ComponentCondition.GetLogicalGroup(sCheck) != LogicalGroup.NaN)
                        return LogicalCondition.Group;
                }
                else
                {
                    return LogicalCondition.Operator;
                }
            }
            return logCond;
        }
    }
    internal enum LogicalCondition
    {
        Operator = 0,
        Group = 1,
        Unknown = 2
    }

}
