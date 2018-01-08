using System;
using System.Linq;

namespace Script.Control.Handlers.Arguments
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class IdentifierAttribute : Attribute
    {
        protected string _defaultValue;
        protected object[] _possibleValues;
        protected string _description;
        //protected string _PossibleValuesStringFormat = " PossibleValues=[{0}];";
        //protected string _PossibleValueSeparator = "|";
        //protected string _DescriptionValuesStringFormat = "[{0}];{1}";
        public string Name { get; }
        public string DefaultValue => _defaultValue ?? PossibleValues;
        public virtual string PossibleValues => _possibleValues != null && _possibleValues.Length > 0 ? string.Format(" PossibleValues=[{0}];", string.Join("|", _possibleValues.Select(o => o.ToString()).ToArray())) : string.Empty;
        public virtual string Description => string.Format("[{0}];{1}", _description, PossibleValues);
        public IdentifierAttribute() : this(string.Empty, string.Empty)
        {
        }
        public IdentifierAttribute(string attrName, string description):this(attrName, description, string.Empty)
        {
        }
        public IdentifierAttribute(string attrName, string description, object defaultValue) : this(attrName, description, defaultValue, new object[]{})
        {
        }
        public IdentifierAttribute(string attrName, string description, object defaultValue, Type enums):this(attrName, description, defaultValue, Enum.GetValues(enums).OfType<object>().ToArray())
        {
        }

        /// <summary>
        /// Назначить имя XML аттрибута
        /// </summary>
        /// <param name="attrName">имя XML аттрибута</param>
        /// <param name="description">описание аттрибута</param>
        /// <param name="objects"></param>
        public IdentifierAttribute(string attrName, string description, object[] objects)
        {
            Name = attrName;
            _description = description;
            _possibleValues = objects;
        }
        /// <summary>
        /// Назначить имя XML аттрибута
        /// </summary>
        /// <param name="attrName">имя XML аттрибута</param>
        /// <param name="description">описание аттрибута</param>
        /// <param name="defaultValue"></param>
        public IdentifierAttribute(string attrName, string description, object defaultValue, object[] objects)
        {
            Name = attrName;
            _description = description;
            _defaultValue = defaultValue.ToString();
            _possibleValues = objects;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class IdentifierResourceAttribute : IdentifierAttribute
    {
        public override string PossibleValues => _possibleValues != null && _possibleValues.Length > 0 ? string.Format("[{0}];", string.Join(Environment.NewLine, _possibleValues.Select(o => o.ToString()).ToArray())) : string.Empty;
        public override string Description => string.Format("{0}\r\n{1}", _description, PossibleValues);
        public IdentifierResourceAttribute(string[] description) : base(string.Empty, "Ресурсы которые можно использовать в аттрибутах:", description)
        {

        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IdentifierClassAttribute : IdentifierAttribute
    {
        public override string PossibleValues => _possibleValues != null && _possibleValues.Length > 0 ? string.Format("[{0}];", string.Join(Environment.NewLine, _possibleValues.Select(o => o.ToString()).ToArray())) : string.Empty;
        public override string Description => string.Format("{0}\r\n{1}", _description, PossibleValues);
        public IdentifierClassAttribute(Type tp, string description) : base(tp.Name, description)
        {

        }
    }
}
