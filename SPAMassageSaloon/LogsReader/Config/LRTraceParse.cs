using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using SPAMassageSaloon.Common;
using Utils;

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

    [Serializable, XmlRoot("Pattern")]
    public class LRTraceParseItem : TraceParse
    {
        private XmlNode[] _cdataItem = new XmlNode[] { new XmlDocument().CreateCDataSection("(.+)") };

        public LRTraceParseItem()
        {
            CDataItem = _cdataItem;
        }

        internal LRTraceParseItem(string regexPattern)
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
    public abstract class TraceParse
    {
        public static TimeSpan MATCH_TIMEOUT = new TimeSpan(0, 0, 10);

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
                ReportMessage.Show(string.Format(Properties.Resources.Txt_LRTraceParse_ErrPattern, text), MessageBoxIcon.Error, "TraceParse Reader");
                return null;
            }
            else
            {
                return new Regex(text, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline, MATCH_TIMEOUT);
            }
        }
    }
}