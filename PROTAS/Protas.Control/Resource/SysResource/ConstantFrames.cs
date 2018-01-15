using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Protas.Components.ConditionEx;
using Protas.Components.Functions;
using Protas.Control.Resource.Base;
using Protas.Components.Types;
using Protas.Components.XPackage;

namespace Protas.Control.Resource.SysResource
{
    internal class RCIsMatch : ResourceConstantFrame
    {
        XPack _result;
        internal static int MinCountParams = 2;
        public RCIsMatch(ResourceConstructor constructor) : base(constructor)
        {
            Regex reg;
            if (Constructor.Count == MinCountParams)
                reg = new Regex(Constructor[1]);
            else
                reg = new Regex(Constructor[1], ProtasFunk.GetRegOptions(Constructor[2]));
            _result = new XPack(string.Empty, reg.IsMatch(Constructor[0]).ToString());
            InitializeOrUpdateObjectFields(_result, reg);
        }
        public override XPack GetResult()
        {
            return _result;
        }
    }
    internal class RCMatches : ResourceConstantFrame
    {
        XPack _result;
        internal static int MinCountParams = 2;
        public RCMatches(ResourceConstructor constructor) : base(constructor)
        {
            Regex reg;
            if (Constructor.Count == MinCountParams)
                reg = new Regex(Constructor[1]);
            else
                reg = new Regex(Constructor[1], ProtasFunk.GetRegOptions(Constructor[2]));
            string strOut = string.Empty;
            foreach (Match match in reg.Matches(Constructor[0]))
            {
                strOut = string.Format("{0}{1}", strOut, match);
            }
            _result = new XPack(string.Empty, strOut);
            InitializeOrUpdateObjectFields(_result, reg);
        }
        public override XPack GetResult()
        {
            return _result; 
        }
    }
    internal class RCReplace : ResourceConstantFrame
    {
        XPack _result;
        internal static int MinCountParams = 3;
        public RCReplace(ResourceConstructor constructor) : base(constructor)
        {
            if (Constructor.Count == MinCountParams)
                _result = new XPack(string.Empty, Regex.Replace(Constructor[0], Constructor[1], Constructor[2]));
            else
                _result = new XPack(string.Empty, Regex.Replace(Constructor[0], Constructor[1], Constructor[2],
                    ProtasFunk.GetRegOptions(Constructor[3])));
        }
        public override XPack GetResult()
        {
             return _result;
        }
    }
    internal class RCIfOperator : ResourceConstantFrame
    {
        XPack _result;
        internal static int MinCountParams = 3;
        public RCIfOperator(ResourceConstructor constructor) : base(constructor)
        {
            string condition = Constructor[0];
            if (Constructor[0].Substring(0, 1)[0] != '(')
                condition = string.Format("'{0}", condition);
            if (Constructor[0].Substring(Constructor[0].Length - 1, 1)[0] != ')')
                condition = string.Format("{0}'", condition);
            IfTarget cExBlock = new IfCondition(this).ExpressionEx(condition);
            if (cExBlock == null)
            {
                _result = new XPack();
                return;
            }
            if (cExBlock.ResultCondition)
                _result = new XPack(string.Empty, Constructor[1].Trim('\''));
            else
                _result = new XPack(string.Empty, Constructor[2].Trim('\''));
        }
        public override XPack GetResult()
        {
            return _result;
        }
    }
    internal class RCSwitchOperator : ResourceConstantFrame
    {
        XPack _result;
        internal static int MinCountParams = 2;
        public RCSwitchOperator(ResourceConstructor constructor) : base(constructor)
        {
            if (string.IsNullOrEmpty(Constructor[1]))
                return;
            string switchParam = Constructor[0];
            int i = 0;
            List<string> parameters = Constructor[1].Split(',').ToList();
            foreach (string param in parameters)
            {
                i++;
                if (parameters.Count == i)
                {
                    _result = new XPack(string.Empty, param);
                    return;
                }
                List<string> comparers = param.Split('|').ToList();
                if (comparers.Count < 2)
                    continue;
                if (comparers[0] == switchParam)
                {
                    _result = new XPack(string.Empty, comparers[1]);
                    return;
                }
            }
            _result = new XPack();
        }
        public override XPack GetResult()
        {
             return _result;
        }
    }
    internal class RCXPath : ResourceConstantFrame
    {
        XPack _result;
        internal static int MinCountParams = 2;
        public RCXPath(ResourceConstructor constructor) : base(constructor)
        {
            string strOut = string.Empty;
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(Constructor[0].Trim('\''));
            XmlTransform xfile = new XmlTransform(xdoc, constructor);
            if (xfile.IsCorrect)
            {
                foreach (string str in ProtasFunk.GetStringByXPath(xfile.XPathNavigator, Constructor[1].Trim('\'')))
                {
                    strOut = string.Format("{0}{1}\r\n", strOut, str);
                }
            }
            _result = new XPack(string.Empty, strOut);
        }
        public override XPack GetResult()
        {
            return _result;
        }
    }
    internal class RCOSVersion : ResourceConstantFrame
    {
        XPack _result;
        internal static int MinCountParams = 0;
        public RCOSVersion(ResourceConstructor constructor) : base(constructor)
        {
            _result = new XPack(string.Empty, FStatic.GetOsVersion);
        }
        public override XPack GetResult()
        {
            return _result;
        }
    }
    internal class RCMachineName : ResourceConstantFrame
    {
        XPack _result;
        internal static int MinCountParams = 0;
        public RCMachineName(ResourceConstructor constructor) : base(constructor)
        {
            _result = new XPack(string.Empty, FStatic.GetMachineName);
        }
        public override XPack GetResult()
        {
            return _result;
        }
    }
    internal class RCMath : ResourceConstantFrame
    {
        XPack _result;
        internal static int MinCountParams = 1;
        public RCMath(ResourceConstructor constructor) : base(constructor)
        {
            _result = new XPack(string.Empty, new Parameter(Constructor[0]).Value);
        }
        public override XPack GetResult()
        {
            return _result;
        }
    }

}
