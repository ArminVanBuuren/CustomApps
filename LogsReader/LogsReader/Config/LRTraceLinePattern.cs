using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Utils;

namespace LogsReader.Config
{
    [XmlRoot("TraceLinePattern")]
    public class LRTraceLinePattern
    {
        private LRTraceLinePatternItem[] _traceLinePattern = new LRTraceLinePatternItem[] { new LRTraceLinePatternItem() };

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
                        new LRTraceLinePatternItem(@"(.+?)\s*(\[.+?\])\s*(.*?)(\<.+\>).*") {Date = "$1", Trace = "$2", Description = "$3", Message = "$4"},
                        new LRTraceLinePatternItem(@"(.+?)\s*(\[.+?\])\s*(.+?)\s+(.+)") {Date = "$1", Trace = "$2", Description = "$3", Message = "$4"},
                        new LRTraceLinePatternItem(@"(.+?)\s*(\[.+?\])\s*(.+)") {Date = "$1", Trace = "$2", Message = "$3"}
                    };
                    break;
                case "SPA":
                    Items = new[]
                    {
                        new LRTraceLinePatternItem(@"(\d+?)\u0001(.+?)\u0001(.+?)\u0001(.+?)\u0001(.*?)\u0001(\d*)")
                            {ID = "$1", Trace = "$2", Description = "$3", Date = "$4.$6", Message = "$5"}
                    };
                    break;
            }
        }

        [XmlIgnore] internal bool IsCorrectRegex => _traceLinePattern != null && _traceLinePattern.Length > 0 && _traceLinePattern.All(x => x.IsCorrectRegex);

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
    }

    [XmlRoot("Item")]
    public class LRTraceLinePatternItem
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
                IsCorrectRegex = false;

                if (value != null)
                {
                    if (value.Length > 0)
                    {
                        if (value[0].NodeType == XmlNodeType.CDATA)
                            _cdataItem = value;
                        else
                            _cdataItem = new XmlNode[] { new XmlDocument().CreateCDataSection(value[0].Value) };
                    }
                }

                if (_cdataItem == null || _cdataItem.Length == 0)
                    return;

                var text = _cdataItem[0].Value.ReplaceUTFCodeToSymbol();
                if (!REGEX.Verify(text))
                {
                    Utils.MessageShow($"Pattern \"{text}\" is incorrect", "TraceLinePattern Reader");
                    return;
                }
                else
                {
                    RegexItem = new Regex(text, RegexOptions.Compiled | RegexOptions.Singleline);
                }

                IsCorrectRegex = true;
            }
        }

        [XmlIgnore] internal bool IsCorrectRegex { get; set; }
        [XmlIgnore] internal Regex RegexItem { get; private set; }

        [XmlAttribute] public string ID { get; set; } = string.Empty;
        [XmlAttribute] public string Date { get; set; } = string.Empty;
        [XmlAttribute] public string Trace { get; set; } = string.Empty;
        [XmlAttribute] public string Description { get; set; } = string.Empty;
        [XmlAttribute] public string Message { get; set; } = string.Empty;
    }
}
