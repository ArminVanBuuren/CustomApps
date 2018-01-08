using System.Xml;

namespace Script.Control.Handlers.GetValue
{
    internal delegate bool GetOrSetXmlNode(XmlNode attribute, ref string logContent);
    internal delegate void ExecutionBasedByParentType();
    public delegate void Exec();
}
