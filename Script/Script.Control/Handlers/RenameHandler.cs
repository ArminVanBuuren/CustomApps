using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Script.Control.Handlers.Arguments;
using Script.Control.Handlers.SysObj.Based;
using XPackage;

namespace Script.Control.Handlers
{
    [IdentifierClass(typeof(RenameHandler), "Выполняет изменение имени файла согласно настройкам")]
    public class RenameHandler : PerformerBase
    {
        [Identifier("Matches", "Регулярное выражение по фильтру названий папок или файлов", "Обязательный")]
        public string PatternMatches { get; }

        [Identifier("Replacement", "Шаблон замены на новое название. Также можно использовать группы по шаблону регулярного выражения", "Обязательный")]
        public string Replacement { get; protected set; }
        public RenameHandler(XPack parentPack, XmlNode node, LogFill logFill) : base(parentPack, node, logFill, ProcessingOptions.Move)
        {
            PatternMatches = Attributes[GetXMLAttributeName(nameof(PatternMatches))];
            if (string.IsNullOrEmpty(PatternMatches))
                throw new HandlerInitializationException(GetIdentifier(nameof(PatternMatches)), true);

            Replacement = Attributes[GetXMLAttributeName(nameof(Replacement))];
            if (string.IsNullOrEmpty(Replacement))
                throw new HandlerInitializationException(GetIdentifier(nameof(PatternMatches)), true);

            FindBase parentSysObj = Parent as FindBase;
            if (parentSysObj == null)
                throw new HandlerInitializationException(this);
        }

        public override void Execute()
        {
            foreach (SystemObjectMatch match in ((FindBase)Parent).Matches.OrderByDescending(x => x.FullPath))
            {
                int d;
                if (Regex.IsMatch(match.Name, "ICSharpCode", RegexOptions.IgnoreCase) && PatternMatches== "ICSharpCode" && SysObjType == FindType.Directories)
                    d = 0;
                //замена имени файла
                string newSysObjName = Regex.Replace(match.Name, PatternMatches, Replacement, RegexOptions.IgnoreCase);

                //если имя результирующего файла или папки не изменилось то ищем следующий файл
                if (newSysObjName.Equals(match.Name, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                SystemObjectMatch newSysObj;
                //если данный файл или папка находится не в подпапке
                if (string.IsNullOrEmpty(match.SubDirectoryPath))
                    newSysObj = new SystemObjectMatch(this, newSysObjName, match.SysObjType);
                else
                    //если данный файл или папка находится в подпапке
                    newSysObj = new SystemObjectMatch(this, string.Format("{0}\\{1}", match.SubDirectoryPath.Trim('\\'), newSysObjName), match.SysObjType);

                //выполняем перенос нового файла с удалением старого
                if (ProcessingWithDirectoryOrFile(match, newSysObj))
                {
                    Matches.Add(newSysObj);
                    AddLog(LogType.Success, this, ToString());
                }
            }
        }

        private object List<T>(FindBase parent)
        {
            throw new NotImplementedException();
        }
    }
}
