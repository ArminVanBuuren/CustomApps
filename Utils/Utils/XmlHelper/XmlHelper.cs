using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Utils.IOExploitation;

namespace Utils.XmlHelper
{
    public static class XmlHelper
    {
        public static XmlDocument LoadXml(string path, bool convertToLower = false)
        {
            string context = FilesEmployee.SafeReadFile(path, convertToLower);
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

            return null;
        }

        public static bool IsXml(string path, out XmlDocument xmldoc, out string source)
        {
            xmldoc = null;
            source = FilesEmployee.SafeReadFile(path);

            if (!IsXml(source, out xmldoc))
                return false;

            return true;
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
