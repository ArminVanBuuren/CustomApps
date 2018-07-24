using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Utils.ConditionEx;
using Utils;

namespace TFSAssist.Control.DataBase.Settings
{
    [XmlRoot("CreateTFS")]
    public class CreateTFS
    {
        [XmlElement("TeamProject", IsNullable = false)]
        public TeamProjectCondition[] TeamProjects { get; set; }

        public override string ToString()
        {
            return string.Format("Projects:[{0}]", TeamProjects?.Length ?? 0);
        }
    }

    /// <summary>
    /// Поле с условиями аттрибутом Condition="#{'true'='true'}"
    /// </summary>
    public class ConditionClass
    {
        /// <summary>
        /// Исходное значение условия
        /// </summary>
        [XmlAttribute]
        public string Condition { get; set; }

        /// <summary>
        /// Выполняется замена спец параметров на их значение и получаем результат условия.
        /// </summary>
        /// <param name="getParcedValue"></param>
        /// <returns></returns>
        internal bool GetConditionResult(GetParcedValue getParcedValue)
        {
            if (Condition.IsNullOrEmpty())
                return true;

            string value = getParcedValue(Condition);

            try
            {
                IfCondition ifCond = new IfCondition();
                IfTarget target = ifCond.ExpressionEx(value);
                return target.ResultCondition;
            }
            catch (Exception ex)
            {
                throw new TFSFieldsException("Condition=[{0}]=[{1}] Is Incorrect!\r\n{2}", Condition, value, ex.Message);
            }
        }

        [XmlAttribute]
        public string Value { get; set; }
    }


    /// <summary>
    /// Создать ТФС проект при условии
    /// </summary>
    [XmlRoot("TeamProject")]
    public class TeamProjectCondition : ConditionClass
    {
        [XmlElement("WorkItem", IsNullable = false)]
        public WorkItemCondition[] WorkItems { get; set; }

        public override string ToString()
        {
            return string.Format("{1} WorkItems:[{0}]", WorkItems?.Length ?? 0, Value);
        }
    }

    /// <summary>
    /// Создать в ТФС проекте рабочий элемент
    /// </summary>
    [XmlRoot("WorkItem")]
    public class WorkItemCondition : ConditionClass
    {
        [XmlElement("Field", IsNullable = false)]
        public FieldCondition[] Fields { get; set; }

        public override string ToString()
        {
            return string.Format("{1} TFSFields:[{0}]", Fields?.Length ?? 0, Value);
        }
    }

    /// <summary>
    /// Поля рабочего элемента
    /// </summary>
    [XmlRoot("Field")]
    public class FieldCondition
    {
        private string _name;
        private string _value;

        /// <summary>
        /// Имя поля обязательно должно быть заполненно
        /// </summary>
        [XmlAttribute]
        public string Name
        {
            get
            {
                if (_name.IsNullOrEmpty())
                    throw new ArgumentException(string.Format("Field is incorrect! Attribute '{0}' must be declared and shouldn't be empty.", nameof(Name)));
                return _name;
            }
            set
            {
                if (value.IsNullOrEmpty())
                    throw new ArgumentException(string.Format("Field is incorrect! Attribute '{0}' must be declared and shouldn't be empty.", nameof(Name)));
                _name = value;
            }
        }

        /// <summary>
        /// Значение поля
        /// </summary>
        [XmlAttribute]
        public string Value
        {
            get
            {
                if (_value == null)
                    throw new TFSFieldsException(string.Format("Field=[{0}] is incorrect! Attribute '{1}' must be declared.", Name, nameof(Value)));
                return _value;
            }
            set
            {
                if (value == null)
                    throw new TFSFieldsException(string.Format("Field=[{0}] is incorrect! Attribute '{1}' must be declared.", Name, nameof(Value)));
                _value = value;
            }
        }

        /// <summary>
        /// Если существет условие для определения значения, то проверяются все условия иначе возвращается просто Value
        /// </summary>
        /// <param name="getParcedValue"></param>
        /// <returns></returns>
        internal string GetSwitchValue(GetParcedValue getParcedValue)
        {
            if (Switch.IsNullOrEmpty() && Items.Count <= 0)
                return getParcedValue(Value);


            string switchValue = getParcedValue(Switch).Trim();
            foreach (MappingValue mapValue in Items)
            {
                if (mapValue.Case.Trim().Equals(switchValue, StringComparison.CurrentCultureIgnoreCase))
                {
                    return getParcedValue(mapValue.Value);
                }
            }


            return getParcedValue(Value);
        }

        /// <summary>
        /// Задать динамическое значение в условие
        /// </summary>
        [XmlAttribute]
        public string Switch { get; set; }

        
        [XmlElement("Map")]
        public List<MappingValue> Items { get; set; } = new List<MappingValue>();

        public override string ToString()
        {
            return string.Format("[{2}] Name:[{0}]; Value=[{1}]", Name, Value, Switch == null ? "Regular" : "Switch");
        }
    }

    [XmlRoot("Map")]
    public class MappingValue
    {
        private string _case;
        private string _value;

        [XmlAttribute]
        public string Case
        {
            get
            {
                if (_case == null)
                    throw new TFSFieldsException(string.Format("Map is incorrect! Attribute '{0}' must be declared.", nameof(Case)));
                return _case;
            }
            set
            {
                if (value == null)
                    throw new TFSFieldsException(string.Format("Map is incorrect! Attribute '{0}' must be declared.", nameof(Case)));
                _case = value;
            }
        }

        [XmlAttribute]
        public string Value
        {
            get
            {
                if (_value == null)
                    throw new TFSFieldsException(string.Format("Map=[{0}] is incorrect! Attribute '{1}' must be declared.", Case, nameof(Value)));
                return _value;
            }
            set
            {
                if (value == null)
                    throw new TFSFieldsException(string.Format("Map=[{0}] is incorrect! Attribute '{1}' must be declared.", Case, nameof(Value)));
                _value = value;
            }
        }

        public override string ToString()
        {
            return string.Format("Case:[{0}]; Value=[{1}]", Case, Value);
        }
    }
}