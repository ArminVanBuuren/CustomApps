using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Protas.Components.ConditionEx;

namespace Protas.Components.XPackage
{
    enum XPackPathType
    {
        FindEqualsParamsName = 0,
        EmptyFindNext = 1,
        SelectAllChild = 2,
        AttributeValue = 3,
        ConditionResult = 4
    }

    class XPackPath
    {
        delegate bool PerformExpression(XPack pack, out List<XPack> result);
        PerformExpression performValidate;
        Regex regexAttribute = new Regex(@"\@\w+");
        Regex regexIfComm = new Regex(@"\'\@\w+\'=\'.*?\'");
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
                performValidate = GetAllChild;
            }
            else if (regexIfComm.IsMatch(Parameter))
            {
                performValidate = GetByIfCommander;
            }
            else if (regexAttribute.IsMatch(Parameter))
            {
                performValidate = GetAttributeValue;
            }
            else
                performValidate = GetEqualsChildParamsName;
        }
        public XPackPathType XType { get; } = XPackPathType.FindEqualsParamsName;
        public bool GetXPack(XPack pack, out List<XPack> result)
        {
            if (performValidate == null)
            {
                result = null;
                return false;
            }
            return performValidate.Invoke(pack, out result);
        }

        public bool GetAnyChildSatisfying(XPack pack, out List<XPack> result)
        {
            result = new List<XPack>();
            return GetAnyChildSatisfying2(pack, result);
        }

        bool GetAnyChildSatisfying2(XPack pack, List<XPack> result)
        {
            foreach (XPack po in pack.ChildPacks)
            {
                if (string.Equals(po.Name, Parameter, StringComparison.CurrentCultureIgnoreCase))
                    result.Add(po);
                else if (po.ChildPacks.Count > 0)
                    GetAnyChildSatisfying2(po, result);
            }
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
            foreach (XPack po in pack.ChildPacks)
            {
                if (string.Equals(po.Name, Parameter, StringComparison.CurrentCultureIgnoreCase))
                    result.Add(po);
            }
            return false;
        }
        bool GetByIfCommander(XPack pack, out List<XPack> result)
        {
            result = new List<XPack>();
            foreach (XPack xpck in pack.ChildPacks)
            {
                string paramResult = Parameter;
                foreach (KeyValuePair<string, string> attr in xpck.Attributes)
                {
                    paramResult = paramResult.Replace(string.Format("@{0}", attr.Key), attr.Value);
                }
                if (regexAttribute.Matches(paramResult).Count == 0)
                {
                    IfCondition ifCond = new IfCondition();
                    IfTarget iftr = ifCond.ExpressionEx(paramResult);
                    if (iftr.ResultCondition)
                        result.Add(xpck);
                }
            }
            return (result.Count > 0);
        }
        bool GetAttributeValue(XPack pack, out List<XPack> result)
        {
            result = new List<XPack>();
            foreach (XPack xpck in pack.ChildPacks)
            {
                string paramResult = Parameter;
                foreach (KeyValuePair<string, string> attr in xpck.Attributes)
                {
                    paramResult = paramResult.Replace(string.Format("@{0}", attr.Key), attr.Value);
                }
                if (regexAttribute.Matches(paramResult).Count == 0)
                {
                    result.Add(new XPack(Parameter, paramResult));
                }
            }
            return (result.Count > 0);
        }
        public string Parameter { get; }

        public override string ToString()
        {
            return string.Format("Parameter = \"{0}\" Type = \"{1}\"", Parameter, XType.ToString("g"));
        }
    }
}
