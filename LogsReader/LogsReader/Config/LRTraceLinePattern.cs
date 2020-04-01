using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Utils;

namespace LogsReader.Config
{
    [XmlRoot("TraceLinePattern")]
    public class LRTraceLinePattern : TraceLinePattern
    {
        private LRTraceLinePatternItem[] _traceLinePattern = new LRTraceLinePatternItem[] {new LRTraceLinePatternItem()};
        private XmlNode[] _startLineWith = null;
        //private XmlNode[] _endLineWith = null;

        public LRTraceLinePattern()
        {

        }

        internal LRTraceLinePattern(string name)
        {
            switch (name)
            {
                case "MG":
                    Items = new[]
                    {
                        new LRTraceLinePatternItem(@"(.+?)\s*(\[.+?\])\s*(.*?)(\<.+\>)(.*)") {Date = "$1", TraceName = "$2", Description = "$3$5", Message = "$4"},
                        new LRTraceLinePatternItem(@"(.+?)\s*(\[.+?\])\s*(.+?)\s+(.+)") {Date = "$1", TraceName = "$2", Description = "$3", Message = "$4"},
                        new LRTraceLinePatternItem(@"(.+?)\s*(\[.+?\])\s*(.+)") {Date = "$1", TraceName = "$2", Message = "$3"}
                    };
                    StartWith = new XmlNode[] { new XmlDocument().CreateCDataSection(@"\d+[.]\d+[.]\d+\s+\d+[:]\d+[:]\d+\.\d+\s+\[") };
                    break;
                case "SPA":
                    Items = new[]
                    {
                        new LRTraceLinePatternItem(@"(\d+?)\u0001(.+?)\u0001(.+?)\u0001(.+?)\u0001(.*?)\u0001(\d*)")
                            {ID = "$1", TraceName = "$2", Description = "$3", Date = "$4.$6", Message = "$5"}
                    };
                    break;
                case "MGA":
                    Items = new[]
                    {
                        new LRTraceLinePatternItem(@"(\d+[-]\d+[-]\d+\s+\d+[:]\d+[:]\d+[,]\d+)\s+\((\w+)\).*?\n[-]{49,}(.+)($|[-]{49,})")
                            {Date = "$1", TraceName = "$2", Message = "$3"}
                    };
                    StartWith = new XmlNode[] { new XmlDocument().CreateCDataSection(@"^(\d+[-]\d+[-]\d+\s+\d+[:]\d+[:]\d+[,]\d+)\s+\((\w+)\)") };
                    //EndWith = new XmlNode[] { new XmlDocument().CreateCDataSection(@"^[-]{49,}\s*$") };
                    break;
            }
        }

        [XmlIgnore]
        internal override bool IsCorrectRegex
        {
            get => _traceLinePattern != null && _traceLinePattern.Length > 0 && _traceLinePattern.All(x => x.IsCorrectRegex);
            set { }
        }

        [XmlElement("Item")]
        public LRTraceLinePatternItem[] Items
        {
            get => _traceLinePattern;
            set
            {
                if (value != null)
                {
                    if (value.Length > 0)
                        _traceLinePattern = value;
                }
            }
        }

        [XmlElement("StartLineWith")]
        public XmlNode[] StartWith
        {
            get => _startLineWith;
            set => StartLineWith = GetCDataNode(value, false, out _startLineWith);
        }

        [XmlIgnore] internal Regex StartLineWith { get; private set; }

        //[XmlElement("EndLineWith")]
        //public XmlNode[] EndWith
        //{
        //    get => _endLineWith;
        //    set => EndLineWith = GetCDataNode(value, false, out _endLineWith);
        //}

        //[XmlIgnore] internal Regex EndLineWith { get; private set; }
    }

    [XmlRoot("Item")]
    public class LRTraceLinePatternItem : TraceLinePattern
    {
        private XmlNode[] _cdataItem = new XmlNode[] { new XmlDocument().CreateCDataSection("(.+)") };

        public LRTraceLinePatternItem()
        {
            CDataItem = _cdataItem;
        }

        internal LRTraceLinePatternItem(string regexPattern)
        {
            CDataItem = new XmlNode[] { new XmlDocument().CreateCDataSection(regexPattern) };
        }


        [XmlText]
        public XmlNode[] CDataItem
        {
            get => _cdataItem;
            set
            {
                RegexItem = GetCDataNode(value, true, out _cdataItem);
                if (RegexItem != null)
                {
                    IsCorrectRegex = true;
                    return;
                }
                IsCorrectRegex = false;
            }
        }

        [XmlIgnore] internal override bool IsCorrectRegex { get; set; }
        [XmlIgnore] internal Regex RegexItem { get; private set; }

        [XmlAttribute] public string ID { get; set; } = string.Empty;
        [XmlAttribute] public string Date { get; set; } = string.Empty;
        [XmlAttribute] public string TraceName { get; set; } = string.Empty;
        [XmlAttribute] public string Description { get; set; } = string.Empty;
        [XmlAttribute] public string Message { get; set; } = string.Empty;
    }

    [Serializable]
    public abstract class TraceLinePattern
    {
        public static TimeSpan MATCH_TIMEOUT = new TimeSpan(0, 0, 15);

        internal abstract bool IsCorrectRegex { get; set; }

        protected Regex GetCDataNode(XmlNode[] input, bool isMandatory, out XmlNode[] cdataResult)
        {
            cdataResult = null;
            if (input != null)
                if (input.Length > 0)
                    cdataResult = input[0].NodeType == XmlNodeType.CDATA ? input : new XmlNode[] { new XmlDocument().CreateCDataSection(input[0].Value) };

            if (cdataResult == null || cdataResult.Length == 0)
                return null;

            var text = cdataResult[0].Value.ReplaceUTFCodeToSymbol();
            if (text.IsNullOrEmptyTrim() && !isMandatory)
                return null;

            if (!REGEX.Verify(text))
            {
                Utils.MessageShow($"Pattern \"{text}\" is incorrect", "TraceLinePattern Reader");
                return null;
            }
            else
            {
                return new Regex(text, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline, MATCH_TIMEOUT);
            }
        }
    }
}