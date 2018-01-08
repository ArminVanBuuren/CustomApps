using System.Xml;
using Script.Control.Handlers.Arguments;
using Script.Control.Handlers.SysObj.Based;
using XPackage;

namespace Script.Control.Handlers
{
    public class PerformHandler : PerformerBase
    {
        [Identifier("Append_Sub_Directories", "Если в Родительском элементе был назначен поиск в подпапках, то необходимо ли создавать данные подпапки в директории назначения, или выполнить операцию только в директории назначения. Принимает значаения только True или False", false)]
        public bool AppendSubDirectories { get; } = false;

        public PerformHandler(XPack parentPack, XmlNode node, LogFill logFill) : base(parentPack, node, logFill)
        {
            string appendSubDirectories = Attributes[GetXMLAttributeName(nameof(AppendSubDirectories))];
            bool _appendSubDirectories = false;
            if (bool.TryParse(appendSubDirectories, out _appendSubDirectories) && _appendSubDirectories)
                AppendSubDirectories = true;

            //если файл или папка назначения не найдена то операция не выполняется
            if (GetDirFromParent && Options != ProcessingOptions.Delete)
                throw new HandlerInitializationException("Cant't Performing {0} To Same {1} From Parent. Attribute {1} Must Be Exist!", Options, GetXMLAttributeName(nameof(MainDirectoryPath)));
        }

        public override void Execute()
        {
            foreach (SystemObjectMatch match in ((FindBase)Parent).Matches)
            {
                string subPathAndSysObjName;
                //если не будет выполняться процесс удаления, то при других операциях нужно учитывать подпапки в результате, если они свойством AppendSubDirectories должны учитываться
                if (AppendSubDirectories || Options == ProcessingOptions.Delete)
                    subPathAndSysObjName = match.SubDirectoryPathWithCurrentName;
                else
                    subPathAndSysObjName = match.Name;

                SystemObjectMatch newSysObj = new SystemObjectMatch(this, subPathAndSysObjName, match.SysObjType);

                if (ProcessingWithDirectoryOrFile(match, newSysObj))
                {
                    Matches.Add(newSysObj);
                    AddLog(LogType.Success, this, match.ToString());
                }
            }
        }
    }
}
