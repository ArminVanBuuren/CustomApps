using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Xml;
using Script.Control.Handlers.Arguments;
using XPackage;

namespace Script.Control.Handlers.SysObj.Based
{
    public abstract class PerformerBase : FindBase
    {
        [Identifier("Options", "Опция для выполнения операции. Может использоваться несколько опций.", ProcessingOptions.None, typeof(ProcessingOptions))]
        public ProcessingOptions Options { get; }
        delegate bool PerformingWithSysObject(string sourcePath, string destPath, bool overWrite);
        delegate bool FinalePerforming(SystemObjectMatch sourcePath, SystemObjectMatch destPath);
        FinalePerforming exec;
        /// <summary>
        /// Инициализируется от Родительского объекта SystemObjectBase
        /// </summary>
        /// <param name="parentPack"></param>
        /// <param name="node"></param>
        /// <param name="logFill"></param>
        /// <param name="option"></param>
        protected PerformerBase(XPack parentPack, XmlNode node, LogFill logFill, ProcessingOptions option) : base(parentPack, node, logFill, true)
        {
            Options = option;
            Initialize();
        }

        protected PerformerBase(XPack parentPack, XmlNode node, LogFill logFill) : base(parentPack, node, logFill)
        {
            //поиск типа для выполнения операции с файлами или папками
            var attr = GetIdentifier(nameof(Options));
            Options = GetProcessOption(Attributes[attr.Name]);
            if (Options == ProcessingOptions.None)
                throw new HandlerInitializationException(attr, true);

            Initialize();
        }
        static ProcessingOptions GetProcessOption(string type)
        {
            var processingOption = ProcessingOptions.None;

            if (string.IsNullOrEmpty(type))
                return ProcessingOptions.None;
            //Delete не может быть в комбинации с другими опциями
            if (Regex.IsMatch(type, "Delete", RegexOptions.IgnoreCase))
                return ProcessingOptions.Delete;

            if (Regex.IsMatch(type, "Replace", RegexOptions.IgnoreCase))
                processingOption |= ProcessingOptions.Replace;
            if (Regex.IsMatch(type, "Move", RegexOptions.IgnoreCase))
                processingOption |= ProcessingOptions.Move;
            //Copy не может быть в комбинации с Move, как и наоборот. Т.к. Copy не заменяет существующий файл, а Move заменяет
            if (Regex.IsMatch(type, "Copy", RegexOptions.IgnoreCase) && (processingOption & ProcessingOptions.Move) != ProcessingOptions.Move)
                processingOption |= ProcessingOptions.Copy;

            return processingOption;
        }

        void Initialize()
        {
            PerformingWithSysObject delete;
            PerformingWithSysObject copy;
            PerformingWithSysObject move;
            if (SysObjType == FindType.Directories)
            {
                delete = delegate (string sourcePath, string destPath, bool overWrite)
                {
                    Directory.Delete(destPath, true);
                    return true;
                };
                copy = delegate (string sourcePath, string destPath, bool overWrite)
                {
                    CopyDirectory(sourcePath, destPath, overWrite);
                    return true;
                };
                move = delegate (string sourcePath, string destPath, bool overWrite)
                {
                    CopyDirectory(sourcePath, destPath, overWrite);
                    Directory.Delete(sourcePath, true);
                    return true;
                };
            }
            else
            {
                delete = delegate (string sourcePath, string destPath, bool overWrite)
                {
                    File.Delete(destPath);
                    return true;
                };
                copy = delegate (string sourcePath, string destPath, bool overWrite)
                {
                    CopyFile(sourcePath, destPath, overWrite);
                    return true;
                };
                move = delegate (string sourcePath, string destPath, bool overWrite)
                {
                    CopyFile(sourcePath, destPath, overWrite);
                    File.Delete(sourcePath);
                    return true;

                };
            }


            if (Options == ProcessingOptions.Delete)
            {
                exec = delegate (SystemObjectMatch sourceSysObj, SystemObjectMatch destSysObj)
                {
                    delete(sourceSysObj.FullPath, destSysObj.FullPath, true);
                    return true;
                };
                return;
            }
            if (Options == (ProcessingOptions.Move | ProcessingOptions.Replace))
            {
                exec = delegate (SystemObjectMatch sourceSysObj, SystemObjectMatch destSysObj)
                {
                    if (CheckSystemObject(sourceSysObj, destSysObj))
                        return move(sourceSysObj.FullPath, destSysObj.FullPath, true);
                    return false;
                };
                return;
            }
            if (Options == ProcessingOptions.Move)
            {
                exec = delegate (SystemObjectMatch sourceSysObj, SystemObjectMatch destSysObj)
                {
                    if (CheckSystemObject(sourceSysObj, destSysObj))
                        return move(sourceSysObj.FullPath, destSysObj.FullPath, false);
                    return false;
                };
                return;
            }
            if (Options == ProcessingOptions.Replace)
            {
                exec = delegate (SystemObjectMatch sourceSysObj, SystemObjectMatch destSysObj)
                {
                    if (CheckSystemObject(sourceSysObj, destSysObj))
                        return copy(sourceSysObj.FullPath, destSysObj.FullPath, true);
                    return false;
                };
                return;
            }

            exec = delegate (SystemObjectMatch sourceSysObj, SystemObjectMatch destSysObj)
            {
                if (CheckSystemObject(sourceSysObj, destSysObj))
                    return copy(sourceSysObj.FullPath, destSysObj.FullPath, false);
                return false;
            };
        }
 

        bool CheckSystemObject(SystemObjectMatch sourceSysObj, SystemObjectMatch destSysObj)
        {
            //если пути источника и результата одинаковые то ничего не выполняем, т.к. дальше идет обработка копирования, переноса, замены
            if (sourceSysObj.FullPath.Equals(destSysObj.FullPath, StringComparison.CurrentCultureIgnoreCase))
            {
                AddLog(LogType.Error, this, "Source Path Of {0} And Destination Path Is Same", SysObjType);
                return false;
            }

            //если в процессе обработки не найдена папка или файл источника, откуда нужно выполнить операции копирования, переноса, замены
            if (!(SysObjType == FindType.Files ? File.Exists(sourceSysObj.FullPath) : Directory.Exists(sourceSysObj.FullPath)))
            {
                AddLog(LogType.Error, this, "Not Found Source {0}=[{1}]", SysObjType, sourceSysObj.FullPath);
                return false;
            }
            return true;
        }

        public bool ProcessingWithDirectoryOrFile(SystemObjectMatch sourceSysObj, SystemObjectMatch destSysObj)
        {
            return exec(sourceSysObj, destSysObj);
        }



        /// <summary>
        /// Копирование или замена папок с подпапками и файлами
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        public static void CopyDirectory(string sourcePath, string destPath, bool overWrite)
        {
            //создаем корневой каталог куда надо скопировать файлы и папки
            if (!Directory.Exists(destPath))
                Directory.CreateDirectory(destPath);

            foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                //создаем все папки в подпапке
                var destSubDirPath = dirPath.Replace(sourcePath, destPath);
                if (!Directory.Exists(destSubDirPath))
                    Directory.CreateDirectory(destSubDirPath);
            }

            foreach (var filePath in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                //копируем все файлы папки включая подпапки
                var destFilePath = filePath.Replace(sourcePath, destPath);
                CopyFile(filePath, destFilePath, overWrite);
            }
        }
        /// <summary>
        /// Копирование или замена файлов
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        public static void CopyFile(string sourcePath, string destPath, bool overWrite)
        {
            var isFileExist = File.Exists(destPath);
            if (isFileExist && !overWrite)
                throw new Exception(string.Format("File Destination Was Exist. Copy File From Source=[{0}] To Destination=[{1}] Is Impossible.", sourcePath, destPath));

            //если папка назначения не найдена то создаем папку
            var directoryPath = Path.GetDirectoryName(destPath);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            if (isFileExist)
                File.SetAttributes(destPath, FileAttributes.Normal);
            File.Copy(sourcePath, destPath, true);
            AddAllAccessPermissions(destPath);
        }

        public static void AddAllAccessPermissions(string filePath)
        {
	        var dInfo = new DirectoryInfo(filePath);
	        var dSecurity = dInfo.GetAccessControl();
	        dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
	        dInfo.SetAccessControl(dSecurity);

			var access = File.GetAccessControl(filePath);
            var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            access.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.ReadAndExecute, AccessControlType.Allow));
            //access.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            File.SetAccessControl(filePath, access);
        }
    }
}
