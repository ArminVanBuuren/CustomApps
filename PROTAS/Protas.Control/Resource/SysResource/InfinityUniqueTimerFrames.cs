using System;
using System.Collections.Generic;
using System.Linq;
using Protas.Components.Functions;
using Protas.Control.Resource.Base;
using Protas.Components.Types;
using Protas.Components.XPackage;

namespace Protas.Control.Resource.SysResource
{
    internal class RIUNow : ResourceInfinityUniqueTimerFrame
    {
        XPack _result;
        DateTime _timeResult;
        internal static int MinCountParams = 0;
        public RIUNow(ResourceConstructor constructor) : base(constructor)
        {
            Interval = 5;
            _timeResult = DateTime.Now;
            _result = new XPack(string.Empty, (Constructor.Count == 0) ? _timeResult.ToString("dd.MM.yyyy HH:mm:ss") : _timeResult.ToString(Constructor[0]));
            InitializeOrUpdateObjectFields(_result, _timeResult);
        }

        public override XPack GetResult()
        {
            DateTime dNow = DateTime.Now;
            double temp = dNow.Subtract(_timeResult).TotalMilliseconds;
            if (temp <= Interval)
                return _result;
            _timeResult = dNow;
            UpdateDate(dNow);
            return _result;
        }

        void UpdateDate(DateTime dNow)
        {
            _result.Value = (Constructor.Count == 0) ? dNow.ToString("dd.MM.yyyy HH:mm:ss") : dNow.ToString(Constructor[0]);
            InitializeOrUpdateObjectFields(_result, dNow);
        }
    }

    internal class RIURandom : ResourceInfinityUniqueTimerFrame
    {
        Random _rand = new Random(int.Parse(DateTime.Now.ToString("fffff")));
        int _minValue = -999999999;
        int _maxValue = 999999999;
        XPack _result;
        internal static int MinCountParams = 0;
        public RIURandom(ResourceConstructor constructor) : base(constructor)
        {
            Interval = 50;
            _result = new XPack();
            if (Constructor.Count == 1)
                _minValue = GetIntVal(Constructor[0]);
            else if (Constructor.Count > 1)
            {
                int i1 = GetIntVal(Constructor[0]);
                int i2 = GetIntVal(Constructor[1]);
                if (i1 < i2 && i2 >= 0)
                {
                    _minValue = i1;
                    _maxValue = i2 + 1;
                }
            }
        }
        public override XPack GetResult()
        {
            _result.Value = _rand.Next(_minValue, _maxValue).ToString();
            return _result;
        }

        int GetIntVal(string str)
        {
            if (!GetTypeEx.IsNumber(str))
                return 0;
            int iparam;
            int.TryParse(str.Substring(0, (str.Length > 9) ? 9 : str.Length), out iparam);
            return iparam;
        }
    }

    internal class RIUNewGuid : ResourceInfinityUniqueTimerFrame
    {
        Random _rand = new Random(int.Parse(DateTime.Now.ToString("fffff")));
        readonly string _abc = "a;b;c;d;e;f;g;h;i;j;k;l;m;n;o;p;q;r;s;t;u;v;w;x;y;z";
        List<string> _arrAbc;
        List<int> _numParams;
        List<int> _default = new List<int> {8, 13, 18, 23};
        internal static int MinCountParams = 0;
        public RIUNewGuid(ResourceConstructor constructor) : base(constructor)
        {
            Interval = 100;
            _arrAbc = _abc.Split(';').ToList();
            List<string> numericParams = new List<string>();
            foreach (string param in Constructor)
            {
                if (GetTypeEx.IsNumber(param))
                    numericParams.Add(param);
            }
            _numParams = new List<int>();
            foreach (string s in numericParams)
            {
                int param;
                int.TryParse(s, out param);
                _numParams.Add(param);
            }
        }

        public override XPack GetResult()
        {
            if (_numParams.Count > 0)
                return new XPack(string.Empty, GetGuid(_numParams));
            return new XPack(string.Empty, GetGuid(_default));
        }

        string GetGuid(List<int> intParams)
        {

            int i = 0;
            string result = string.Empty;
            foreach (int necessaryLength in intParams)
            {
                i++;
                int length = 0;
                string _out = string.Empty;
                while (length < necessaryLength)
                {
                    length++;
                    if (_rand.Next(1, 3) == 1)
                        _out = _out + GetRandomChar(_rand);
                    else
                        _out = _out + _rand.Next(0, 9);
                }
                if (intParams.Count == i)
                    result = result + _out;
                else
                    result = result + _out + "-";
            }
            return result;
        }

        string GetRandomChar(Random rnd)
        {
            int index = rnd.Next(0, _arrAbc.Count() - 1);
            if (rnd.Next(1, 2) == 1)
                return _arrAbc[index];
            return _arrAbc[index].ToUpper();
        }
    }

    internal class RIULocalRAM : ResourceInfinityUniqueTimerFrame
    {
        internal static int MinCountParams = 0;
        XPack _result;
        public RIULocalRAM(ResourceConstructor constructor) : base(constructor, 2)
        {
            Interval = 4000;
            _result = new XPack();
            InitializeOrUpdateObjectFields(_result, FStatic.GetLocalRam());
        }
        public override XPack GetResult()
        {
            InitializeOrUpdateObjectFields(_result, FStatic.GetLocalRam());
            return _result;
        }
    }
}
