using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Utils.XmlHelper
{
    public static class XmlHelper
    {
        public static XmlDocument LoadXml(string path, bool toLower = false)
        {
            if (File.Exists(path))
            {
                string context;
                using (StreamReader sr = new StreamReader(path))
                {
                    context = toLower ? sr.ReadToEnd().ToLower() : sr.ReadToEnd();
                }

                if (!string.IsNullOrEmpty(context) && context.TrimStart().StartsWith("<"))
                {
                    try
                    {
                        XmlDocument xmlSetting = new XmlDocument();
                        xmlSetting.LoadXml(context);
                        return xmlSetting;
                    }
                    catch (Exception ex)
                    {
                        //null
                    }
                }
            }
            return null;
        }

        public static bool IsXml(string path, out XmlDocument xmldoc, out string source)
        {
            xmldoc = null;
            source = null;
            if (File.Exists(path))
            {
                string context;
                using (StreamReader sr = new StreamReader(path))
                {
                    context = sr.ReadToEnd();
                }
                if (IsXml(context, out xmldoc))
                {
                    source = context;
                    return true;
                }
            }
            return false;
        }

        public static bool IsXml(string source, out XmlDocument xmldoc)
        {
            xmldoc = null;
            if (!string.IsNullOrEmpty(source) && source.TrimStart().StartsWith("<"))
            {
                try
                {
                    xmldoc = new XmlDocument();
                    xmldoc.LoadXml(source);
                    return true;
                }
                catch (Exception ex)
                {
                    //null
                }
            }
            return false;
        }
    }
}
