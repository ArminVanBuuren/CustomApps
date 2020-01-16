using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Script.Control.Handlers.Arguments;
using XPackage;

namespace Script.Control.Handlers.SysObj.Based
{

    public abstract class FindBase : ScriptTemplate, IWriteValue
    {
        [Identifier("DirPath", "Основной путь к папке с файлами или папками", "Условно Обязательное; Если аттрибута не будет, то все данные берутся из Родительского объекта")]
        public virtual string MainDirectoryPath { get; }

        [Identifier("Type", "Основной путь к папке с файлами или папками", FindType.All, typeof(FindType))]
        public FindType SysObjType { get; protected set; }
        public List<SystemObjectMatch> Matches { get; }
        public bool GetDirFromParent { get; private set; } = false;


        /// <summary>
        /// Если обработчик не считывает тип объекта и исходный путь, то все берется у родителя
        /// </summary>
        /// <param name="parentPack"></param>
        /// <param name="node"></param>
        /// <param name="logFill"></param>
        /// <param name="getBaseAttributesFromParent"></param>
        protected FindBase(XPack parentPack, XmlNode node, LogFill logFill, bool getBaseAttributesFromParent) : base(parentPack, node, logFill)
        {
            CheckParentObject();
            MainDirectoryPath = ((FindBase)Parent).MainDirectoryPath;
            SysObjType = ((FindBase)Parent).SysObjType;
            Matches = new List<SystemObjectMatch>();
        }
        protected FindBase(XPack parentPack, XmlNode node, LogFill logFill) : base(parentPack, node, logFill)
        {
            var sysObjPath = Attributes[GetXMLAttributeName(nameof(MainDirectoryPath))];
            if (sysObjPath != null)
                MainDirectoryPath = Path.GetFullPath(sysObjPath);
            else
            {
                CheckParentObject();
                MainDirectoryPath = ((FindBase) Parent).MainDirectoryPath;
            }

            var sysObjType = Attributes[GetXMLAttributeName(nameof(SysObjType))];
            SysObjType = sysObjType != null ? GetType(sysObjType) : FindType.All;

            Matches = new List<SystemObjectMatch>();
        }

        void CheckParentObject()
        {
            GetDirFromParent = true;
            var parentSysObj = Parent as FindBase;
            if (parentSysObj == null)
                throw new HandlerInitializationException(this);
        }

        static FindType GetType(string option)
        {
            var comprarer = StringComparison.InvariantCultureIgnoreCase;
            var newOption = option.Trim();

            if (newOption.Equals("Files", comprarer) || newOption.Equals("File", comprarer))
                return FindType.Files;
            if (newOption.Equals("Directories", comprarer) || newOption.Equals("Directory", comprarer) || newOption.Equals("Dir", comprarer) || newOption.Equals("Folder", comprarer))
                return FindType.Directories;

            return FindType.All;
        }

        public virtual string GetOfWriteValue()
        {
            return string.Join(Environment.NewLine, Matches);
        }
    }
}
