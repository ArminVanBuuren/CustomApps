using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XPackage
{
    class XPackPath
    {
        delegate bool PerformExpression(XPack pack, out List<XPack> result);
        PerformExpression _performValidate;
        Regex _regexAttribute = new Regex(@"\@\w+");
        Regex _regexIfComm = new Regex(@"\'\@\w+\'=\'.*?\'");
        public string Parameter { get; }
        public XPackPath(string param)
        {
            Parameter = param;
            if (string.IsNullOrEmpty(Parameter))
            {
                XType = XPackPathType.EmptyFindNext;
            }
            else if (Parameter.Equals("*"))
            {
                XType = XPackPathType.SelectAllChild;
                _performValidate = GetAllChild;
            }
            else if (_regexIfComm.IsMatch(Parameter))
            {
                _performValidate = GetByIfCommander;
            }
            else if (_regexAttribute.IsMatch(Parameter))
            {
                _performValidate = GetAttributeValue;
            }
            else
                _performValidate = GetEqualsChildParamsName;
        }
        public XPackPathType XType { get; } = XPackPathType.FindEqualsParamsName;
        public bool GetXPack(XPack pack, out List<XPack> result)
        {
            if (_performValidate == null)
            {
                result = null;
                return false;
            }
            return _performValidate.Invoke(pack, out result);
        }

        public bool GetAnyChildSatisfying(XPack pack, out List<XPack> result)
        {
            result = new List<XPack>();
            return GetAnyChildSatisfying2(pack, result);
        }

        bool GetAnyChildSatisfying2(XPack pack, List<XPack> result)
        {
            foreach (var po in pack.ChildPacks)
            {
                if (string.Equals(po.Name, Parameter, StringComparison.CurrentCultureIgnoreCase))
                    result.Add(po);
                else if (po.ChildPacks.Count > 0)
                    GetAnyChildSatisfying2(po, result);
            }
            return (result.Count > 0);
        }



        public bool GetAnyParentSatisfying(XPack pack, out List<XPack> result)
        {
            result = new List<XPack>();
            return GetAnyParentSatisfying2(pack.Parent, result);
        }

        bool GetAnyParentSatisfying2(XPack pack, List<XPack> result)
        {
            if (string.Equals(pack.Name, Parameter, StringComparison.CurrentCultureIgnoreCase))
                result.Add(pack);
            else
                foreach (var po in pack.ChildPacks)
                {
                    if (string.Equals(po.Name, Parameter, StringComparison.CurrentCultureIgnoreCase))
                        result.Add(po);
                }

            if (pack.Parent != null && result.Count == 0)
                GetAnyParentSatisfying2(pack.Parent, result);
            return (result.Count > 0);
        }





        bool GetAllChild(XPack pack, out List<XPack> result)
        {
            result = new List<XPack>();
            result.AddRange(pack.ChildPacks);
            return false;
        }

        bool GetEqualsChildParamsName(XPack pack, out List<XPack> result)
        {
            result = new List<XPack>();
            foreach (var po in pack.ChildPacks)
            {
                if (string.Equals(po.Name, Parameter, StringComparison.CurrentCultureIgnoreCase))
                    result.Add(po);
            }
            return false;
        }
        bool GetByIfCommander(XPack pack, out List<XPack> result)
        {
            result = new List<XPack>();
            foreach (var xpck in pack.ChildPacks)
            {
                var paramResult = Parameter;
                foreach (var attr in xpck.Attributes)
                {
                    paramResult = paramResult.Replace(string.Format("@{0}", attr.Key), attr.Value);
                }
                if (_regexAttribute.Matches(paramResult).Count == 0)
                {
                    //IfCondition ifCond = new IfCondition();
                    //IfTarget iftr = ifCond.ExpressionEx(paramResult);
                    //if (iftr.ResultCondition)
                    //    result.Add(xpck);
                }
            }
            return (result.Count > 0);
        }
        bool GetAttributeValue(XPack pack, out List<XPack> result)
        {
            result = new List<XPack>();
            foreach (var xpck in pack.ChildPacks)
            {
                var paramResult = Parameter;
                foreach (var attr in xpck.Attributes)
                {
                    paramResult = paramResult.Replace(string.Format("@{0}", attr.Key), attr.Value);
                }
                if (_regexAttribute.Matches(paramResult).Count == 0)
                {
                    ///////////////////////////////////////result.Add(pack.GetNewXPack(Parameter, paramResult))//////////////////////////////////////////////
                    //result.Add(pack.GetNewXPack(Parameter, paramResult));
                    //надо возвращать XPackAttribute
                }
            }
            return (result.Count > 0);
        }
        

        public override string ToString()
        {
            return string.Format("Parameter = \"{0}\" Type = \"{1}\"", Parameter, XType.ToString("g"));
        }
    }
}
