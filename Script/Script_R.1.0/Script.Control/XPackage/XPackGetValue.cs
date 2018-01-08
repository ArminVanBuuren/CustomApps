using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XPackage
{
    public delegate string GetResourceValue(string input, XPack parent);
    public abstract class XPackGetValue
    {
        XPackDynamicResource _mainObj;
        public XPack Parent { get; protected set; }
        internal class XPackDynamicResource
        {
            public GetResourceValue DynamicFunction { get; set; }
        }
        public GetResourceValue DynamicFunction
        {
            get { return _mainObj.DynamicFunction; }
            protected set { _mainObj.DynamicFunction = value; }
        }
        protected string SourceValue { get; set; }
        /// <summary>
        /// Если аттрибут оперделен то true, если в данный момент мы получаем ресур по этому аттрибуту то этот аттрибут не определен
        /// </summary>
        bool IsDetermined { get; set; } = true;
        protected XPackGetValue():this(string.Empty)
        {

        }
        protected XPackGetValue(string sourceValue)
        {
            _mainObj = new XPackDynamicResource();
            SourceValue = sourceValue;
        }
        internal XPackGetValue(XPack currentPack, string sourceValue)
        {
            if(currentPack == null)
                throw new Exception("Current XPack Is Null");
            Parent = currentPack;
            _mainObj = currentPack._mainObj;
            SourceValue = sourceValue;
        }
        public string Value
        {
            get
            {
                //Если аттрибут оперделен то true, если в данный момент мы получаем ресурc по этому аттрибуту то этот аттрибут не определен и возвращаем пустоту
                //должен быть до try, т.к сработает finally и аттрибут определится
                if (!IsDetermined)
                    return string.Empty;
                try
                {
                    //чтобы получить ресурсы в этом аттрибуте и избежать бесконечного цикла то делаем данный аттрибут непоределенным
                    IsDetermined = false;
                    string result = DynamicFunction != null ? DynamicFunction(SourceValue, Parent) : SourceValue;
                    return result;
                }
                finally
                {
                    //как только заполнили значение аттрибута ресурсами, делаем его снова определенным
                    IsDetermined = true;
                }
            }
            set
            {
                SourceValue = value;
            }
        }
        public override string ToString()
        {
            return SourceValue;
        }
    }


    //https://stackoverflow.com/questions/30356401/pass-type-parameter-through-several-derived-classes-for-using-in-generic-static
    //public class A<TC> where TC : class, new()
    //{
    //    protected static ConcurrentDictionary<object, TC> _instances = new ConcurrentDictionary<object, TC>();

    //    public static TC Instance(object key)
    //    {
    //        return _instances.GetOrAdd(key, k => new TC());
    //    }
    //}
    //public class B1<T> : A<B1<T>>
    //{
    //    public virtual int Demo() { return 1; }
    //}
    //public class B2 : B1<B2>
    //{
    //    // Something that C does not do
    //}
    //public class C : B1<C>
    //{
    //    public override int Demo() { return 2; }
    //}

    //public abstract class Base
    //{
    //    public abstract void Use();
    //    public abstract object GetProp();
    //}
    //public abstract class GenericBase<T> : Base
    //{
    //    public T Prop { get; set; }

    //    public override object GetProp()
    //    {
    //        return Prop;
    //    }
    //}
    //public class StrBase : GenericBase<string>
    //{
    //    public override void Use()
    //    {
    //        Console.WriteLine("Using string: {0}", Prop);
    //    }
    //}
    //public class IntBase : GenericBase<int>
    //{
    //    public override void Use()
    //    {
    //        Console.WriteLine("Using int: {0}", Prop);
    //    }
    //}
    //i would like to get the type of the derived class from a static method of its base class.
    //class BaseClass
    //{
    //    static void Ping<T>() where T : BaseClass
    //    {
    //        Type t = typeof(T);
    //    }
    //}
}
