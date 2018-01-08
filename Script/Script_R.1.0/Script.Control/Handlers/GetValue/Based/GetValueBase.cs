using System;
using System.Xml;
using Script.Control.Handlers.Arguments;
using Script.Control.Handlers.SysObj.Based;
using Script.Handlers.GetValue.Based;
using XPackage;

namespace Script.Control.Handlers.GetValue.Based
{
    public class GetValueBase : FindBase
    {
        [Identifier]
        public override string MainDirectoryPath { get; }

        /// <summary>
        /// Инициализируется от Родительского объекта SystemObjectBase
        /// </summary>
        /// <param name="parentPack"></param>
        /// <param name="node"></param>
        /// <param name="logFill"></param>
        public GetValueBase(XPack parentPack, XmlNode node, LogFill logFill) : base(parentPack, node, logFill, true)
        {

        }
        public override string GetOfWriteValue()
        {
            string result = string.Empty;
            foreach (InnerTextMatch match in Matches)
            {
                if (match.Values != null)
                    result = result + string.Format("{3}File:{0}{1}{2}{3}", match.FullPath, Environment.NewLine, string.Join(Environment.NewLine, match.Values), new string('=', 100));
                else
                    result = result + string.Format("{0}{1}", Environment.NewLine, match);
            }
            return result.Trim();
        }
    }
}
