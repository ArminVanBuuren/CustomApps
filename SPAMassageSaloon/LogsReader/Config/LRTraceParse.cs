using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace LogsReader.Config
{
    [Serializable, XmlRoot("TraceParse")]
    public class LRTraceParse : TraceParse
    {
        private LRTraceParseItem[] _traceParsePatterns = new LRTraceParseItem[] {new LRTraceParseItem()};
        private XmlNode[] _startTraceWith = null;
        private XmlNode[] _endTraceWith = null;

        public LRTraceParse()
        {

        }

        internal LRTraceParse(string name)
        {
            switch (name)
            {
                case "MG":
                    Patterns = new[]
                    {
                        new LRTraceParseItem(@"(.+?)\s*(\[.+?\])\s*(.*?)(\<.+\>)(.*)") {Date = "$1", TraceName = "$2", Description = "$3$5", Message = "$4"},
                        new LRTraceParseItem(@"(.+?)\s*(\[.+?\])\s*(.+?)\s+(.+)") {Date = "$1", TraceName = "$2", Description = "$3", Message = "$4"},
                        new LRTraceParseItem(@"(.+?)\s*(\[.+?\])\s*(.+)") {Date = "$1", TraceName = "$2", Message = "$3"}
                    };
                    StartWith = new XmlNode[] { new XmlDocument().CreateCDataSection(@"^\d+[.]\d+[.]\d+\s+\d+[:]\d+[:]\d+\.\d+\s+\[") };
                    break;
                case "SPA":
                    Patterns = new[]
                    {
                        new LRTraceParseItem(@"(\d+?)\u0001(.+?)\u0001(.+?)\u0001(.+?)\u0001(.*?)\u0001(\d*)")
                            {ID = "$1", TraceName = "$2", Description = "$3", Date = "$4.$6", Message = "$5"}
                    };
                    break;
                case "MGA":
                    Patterns = new[]
                    {
                        new LRTraceParseItem(@"(\d+[-]\d+[-]\d+\s+\d+[:]\d+[:]\d+[,]\d+)\s+\((\w+)\).*?[-]{49,}(.+)")
                            {Date = "$1", TraceName = "$2", Message = "$3"}
                    };
                    StartWith = new XmlNode[] { new XmlDocument().CreateCDataSection(@"^(\d+[-]\d+[-]\d+\s+\d+[:]\d+[:]\d+[,]\d+)\s+\((\w+)\)") };
                    break;
            }
        }

        [XmlIgnore]
        internal override bool IsCorrectRegex
        {
            get => _traceParsePatterns != null && _traceParsePatterns.Length > 0 && _traceParsePatterns.All(x => x.IsCorrectRegex);
            set { }
        }

        [XmlElement("Pattern")]
        public LRTraceParseItem[] Patterns
        {
            get => _traceParsePatterns;
            set
            {
	            if (value == null) 
		            return;
	            if (value.Length > 0)
		            _traceParsePatterns = value;
            }
        }

        [XmlElement("StartTraceWith")]
        public XmlNode[] StartWith
        {
            get => _startTraceWith;
            set => StartTraceWith = GetCDataNode(value, false, out _startTraceWith);
        }

        [XmlIgnore] internal Regex StartTraceWith { get; private set; }

        [XmlElement("EndTraceWith")]
        public XmlNode[] EndWith
        {
            get => _endTraceWith;
            set => EndTraceWith = GetCDataNode(value, false, out _endTraceWith);
        }

        [XmlIgnore] internal Regex EndTraceWith { get; private set; }
    }
}