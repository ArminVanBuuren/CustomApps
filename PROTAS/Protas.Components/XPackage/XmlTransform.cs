using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Protas.Components.Functions;
using Protas.Components.PerformanceLog;

namespace Protas.Components.XPackage
{
    [Serializable]
    public class XmlTransform : ShellLog3Net, IDisposable
    {
        static readonly internal string HeaderName = "$$$header$$$";
        public string FilePath { get; private set; }
        public bool IsCorrect { get; private set; } = false;
        public string XmlSource { get; private set; }
        public XPathNavigator XPathNavigator { get; private set; }
        public XmlDocument XmlDoc { get; private set; }
        public XPack XPackage { get; private set; }
        public XmlTransform(string filePath, ILog3NetMain log) : base(log)
        {
            LoadFromFile(filePath);
        }
        public XmlTransform(XmlReader xml, ILog3NetMain log) : base(log)
        {
            XmlDoc = new XmlDocument();
            XmlDoc.Load(xml);
            Initiate(XmlDoc);
        }
        public XmlTransform(XmlDocument xmlDoc, ILog3NetMain log) : base(log)
        {
            XmlDoc = xmlDoc;
            Initiate(XmlDoc);
        }
        void LoadFromFile(string filePath)
        {
            FilePath = filePath;
            XmlDoc = new XmlDocument();
            XmlDoc.Load(FilePath);
            Initiate(XmlDoc);
        }
        void Initiate(XmlDocument xml)
        {
            try
            {
                string source = string.Empty;
                XPackage = new XPack(HeaderName);
                XPackage.AddChildNodes(ProcessXmlInnerText(xml.DocumentElement, ref source, 0));
                XPathNavigator = new XPathDocument(new StringReader(source)).CreateNavigator();
                XmlSource = source;
                IsCorrect = true;
                AddLogForm(Log3NetSeverity.Max, "IsCorrect:{1} Success Executed. XmlSource:{0}", XmlSource, IsCorrect);
            }
            catch(Exception ex)
            {
                IsCorrect = false;
                AddLogForm(Log3NetSeverity.Error, "IsCorrect:{2} Catched Exception:{0}{1}", ex.Message, ex.StackTrace, IsCorrect);
            }
        }
        public bool CompareXPack(XmlTransform input)
        {
            int cntChildPacks = XPackage.ChildPacks.Count;
            if (this == input)
                return true;
            if (cntChildPacks != input.XPackage.ChildPacks.Count)
                return false;
            if (cntChildPacks > 0)
            {
                for (int i = 0; i < cntChildPacks; i++)
                {
                    if (!XPackage.ChildPacks[i].CompareXPack(input.XPackage.ChildPacks[i]))
                        return false;
                }
            }
            return true;
        }
        XPack ProcessXmlInnerText(XmlNode node, ref string source, int nested)
        {
            string separators = string.Empty;
            for (int i = 0; i < nested; i++)
            {
                separators = separators + " ";
            }
            string str2 = string.Empty;
            if (node.Attributes == null)
            {
                string value;
                if (string.Equals(node.Name, "#text"))
                {
                    value = node.OuterXml.Trim();
                    source = source + value;
                    return new XPack(node.Name, value);
                }
                if (string.Equals(node.Name, "#comment"))
                {
                    value = ProtasFunk.ReplaceXmlSpecSymbls(node.OuterXml.Trim(), 2);
                    source = source + string.Format("{1}{2}{0}", value, Environment.NewLine, separators);
                }
                else if (string.Equals(node.Name, "#cdata-section"))
                {
                    value = ProtasFunk.ReplaceXmlSpecSymbls(node.OuterXml.Trim(), 2);
                    source = source + string.Format("{1}{2}{0}", value, Environment.NewLine, separators);
                }
                else
                {
                    string innerText = node.InnerText;
                    if (innerText.LastIndexOf("\n", StringComparison.Ordinal) > (innerText.Trim().Length - 1))
                    {
                        innerText = node.InnerText.Remove(innerText.LastIndexOf("\n", StringComparison.Ordinal));
                    }
                    value = ProtasFunk.ReplaceXmlSpecSymbls(innerText, 2);
                    source = source + string.Format("{0}", value);
                }
            }
            else
            {
                XPack pckobject = new XPack(node.Name);
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    str2 = str2 + string.Format(" {0}=\"{1}\"", attribute.Name, ProtasFunk.ReplaceXmlSpecSymbls(attribute.Value, 2));
                    pckobject.AddAttribute(attribute.Name, attribute.Value);
                }
                string str4 = (nested != 0) ? Environment.NewLine : string.Empty;
                if ((node.ChildNodes.Count <= 0) && string.IsNullOrEmpty(node.InnerText))
                {
                    if (string.IsNullOrEmpty(str2))
                    {
                        source = source + string.Format("{2}{0}<{1} />", separators, node.Name, str4);
                    }
                    else
                    {
                        source = source + string.Format("{3}{0}<{1}{2} />", separators, node.Name, str2, str4);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(str2))
                    {
                        source = source + string.Format("{2}{0}<{1}>", separators, node.Name, str4);
                    }
                    else
                    {
                        source = source + string.Format("{3}{0}<{1} {2}>", separators, node.Name, str2, str4);
                    }
                    nested += 3;
                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        XPack childObj = ProcessXmlInnerText(node2, ref source, nested);
                        if (childObj != null)
                            pckobject.AddChildNodes(childObj);
                    }
                    if (((node.FirstChild != null) && (node.ChildNodes.Count == 1)) && string.Equals(node.FirstChild.Name, "#text"))
                    {
                        source = source + string.Format("</{0}>", node.Name);
                    }
                    else
                    {
                        source = source + string.Format("{2}{0}</{1}>", separators, node.Name, Environment.NewLine);
                    }
                }
                return pckobject;
            }
            return null;
        }

        public void Dispose()
        {
            XPathNavigator = null;
            XmlDoc = null;
            XPackage = null;
        }
    }



}
