using System;
using System.Xml;
using System.Xml.XPath;
using Protas.Components.PerformanceLog;

namespace Protas.Components.DynamicArea
{
    public enum ParceMode
    {
        None = 0,
        Xml = 1
    }
    public class AreaConstructor : ShellLog3Net, IDisposable
    {
        static readonly internal string HeaderName = "$$$header$$$";
        public string FilePath { get; private set; }
        public bool IsCorrect { get; private set; } = false;
        public string Source { get; private set; }
        public XPathNavigator Navigator { get; private set; }
        public XmlDocument Document { get; private set; }
        public KineticPack Kinetic { get; private set; }
        public ParceMode Mode { get; }
        public AreaConstructor(string filePath, ParceMode mode, ILog3NetMain log) : base(log)
        {
            Mode = mode;
            if (Mode == ParceMode.Xml)
                LoadXmlFromFile(filePath);
        }
        public AreaConstructor(XmlReader xml, ILog3NetMain log) : base(log)
        {
            Mode = ParceMode.Xml;
            Document = new XmlDocument();
            Document.Load(xml);
            InitiateByXml(Document);
        }
        public AreaConstructor(XmlDocument xmlDoc, ILog3NetMain log) : base(log)
        {
            Mode = ParceMode.Xml;
            Document = xmlDoc;
            InitiateByXml(Document);
        }
        void LoadXmlFromFile(string filePath)
        {
            FilePath = filePath;
            Document = new XmlDocument();
            Document.Load(FilePath);
            InitiateByXml(Document);
        }
        void InitiateByXml(XmlDocument xml)
        {

        }
        public bool CompareXPack(AreaConstructor input)
        {

            return true;
        }
        KineticPack ProcessXmlInnerText(XmlNode node, ref string source, int nested)
        {
           
            return null;
        }

        public void Dispose()
        {
            Navigator = null;
            Document = null;
            Kinetic = null;
        }
    }
}
