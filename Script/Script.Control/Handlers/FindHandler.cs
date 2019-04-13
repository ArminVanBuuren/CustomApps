using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Script.Control.Handlers.Arguments;
using Script.Control.Handlers.SysObj.Based;
using XPackage;

namespace Script.Control.Handlers
{
    [IdentifierClass(typeof(FindHandler), "Считывает папки и файлы из указанного в пути.")]
    public class FindHandler : FindBase
    {
        delegate string[] GetSysObjects();
        delegate void ReadSysObjDeleg(string str);
        delegate bool CheckSizeOfSysObj(string path);
        

        GetSysObjects getSysObj;
        ReadSysObjDeleg readSysObj;
        CheckSizeOfSysObj checkSizeOfSysObj = null;


        [Identifier("Sub_Directories", "Выполнить ли поиск подпапок в исходной папке.", "False")]
        public bool SearchInSubDirs { get; } = false;

        [Identifier("Matches", "Регулярное выражение для поиска файлов или папок", "Необязательный")]
        public string PatternMatches { get; }

        [Identifier("Max_Size", "Максимальный размер файла или папки в MB", "Необязательный")]
        public string MaxSizeMB { get; }

        [Identifier("Min_Size", "Минимальный размер файла или папки в MB", "Необязательный")]
        public string MinSizeMB { get; }

        public bool CheckSize { get; }


        public FindHandler(XPack parentPack, XmlNode node, LogFill logFill) : base(parentPack, node, logFill)
        {
            SearchOption Option = SearchOption.TopDirectoryOnly;

            string subDirs = Attributes[GetXMLAttributeName(nameof(SearchInSubDirs))];
            bool _allowSubDir = false;
            if (bool.TryParse(subDirs, out _allowSubDir) && _allowSubDir)
            {
                Option = SearchOption.AllDirectories;
                SearchInSubDirs = true;
            }

            PatternMatches = Attributes[GetXMLAttributeName(nameof(PatternMatches))];

            double _max_size = 5000, _min_size = 0;
            string maxSize = Attributes[GetXMLAttributeName(nameof(MaxSizeMB))];
            if (maxSize != null && double.TryParse(maxSize, out _max_size))
                MaxSizeMB = _max_size.ToString(CultureInfo.InvariantCulture);

            string minSize = Attributes[GetXMLAttributeName(nameof(MinSizeMB))];
            if (minSize != null && double.TryParse(minSize, out _min_size))
                MinSizeMB = _min_size.ToString(CultureInfo.InvariantCulture);

            CheckSize = MaxSizeMB != null || MinSizeMB != null;

            string logMatchFrom = null;
            if (!GetDirFromParent)
            {
                logMatchFrom = MainDirectoryPath;
                switch (SysObjType)
                {
                    case FindType.Files:
                        getSysObj = () => Directory.GetFiles(MainDirectoryPath, "*", Option);
                        break;
                    case FindType.Directories:
                        getSysObj = () => Directory.GetDirectories(MainDirectoryPath, "*", Option);
                        break;
                    case FindType.All:
                        getSysObj = delegate
                                    {
                                        return null;
                                    };
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                FindBase parentSysObj = Parent as FindBase;
                logMatchFrom = "Parent Function Match Collection";

                if (SysObjType == FindType.Files && parentSysObj.SysObjType == FindType.Files)
                {
                    //если необходимо учитывать подпапки с файлами
                    if (Option == SearchOption.AllDirectories)
                        getSysObj = () => parentSysObj.Matches.Select(p => p.FullPath).ToArray();
                    else
                        getSysObj = () => parentSysObj.Matches.Where(c => string.IsNullOrEmpty(c.SubDirectoryPath)).Select(p => p.FullPath).ToArray();
                }
                //если обработка только папок то забираем только папки из родительского
                else if (SysObjType == FindType.Directories && parentSysObj.SysObjType == FindType.Directories)
                {
                    //если необходимо учитывать подпапки с папками
                    if (Option == SearchOption.AllDirectories)
                        getSysObj = () => parentSysObj.Matches.Select(p => p.FullPath).ToArray();
                    else
                        getSysObj = () => parentSysObj.Matches.Where(c => string.IsNullOrEmpty(c.SubDirectoryPath)).Select(p => p.FullPath).ToArray();
                }
                //если обработка только файлов то забираем все файлы из папок
                else if (SysObjType == FindType.Files && parentSysObj.SysObjType == FindType.Directories)
                {
                    getSysObj = delegate
                                {
                                    List<string> temp = new List<string>();
                                    foreach (SystemObjectMatch match in parentSysObj.Matches)
                                    {
                                        temp.AddRange(Directory.GetFiles(match.FullPath, "*", Option));
                                    }
                                    return temp.ToArray();
                                };
                }
                //условие не нужно parentMatches.Count > 0, т.к. родитель не нашел файлы в папке, а в данном случае нам нужен только каталог
                else if (SysObjType == FindType.Directories && parentSysObj.SysObjType == FindType.Files && !string.IsNullOrEmpty(MainDirectoryPath))
                    getSysObj = () => Directory.GetDirectories(MainDirectoryPath, "*", Option);

            }

            if (getSysObj == null)
            {
                throw new HandlerInitializationException("{0} Is Incorrect", logMatchFrom);
            }

            if (!string.IsNullOrEmpty(PatternMatches))
            {
                readSysObj = delegate (string subAndSysObjName)
                             {
                                 //проверяем регуляркой совпадение по названию
                                 if (Regex.IsMatch(subAndSysObjName, PatternMatches, RegexOptions.IgnoreCase))
                                 {
                                     SystemObjectMatch sysObj = new SystemObjectMatch(this, subAndSysObjName, SysObjType);
                                     Matches.Add(sysObj);
                                 }
                             };
            }
            else
            {
                readSysObj = delegate (string subAndSysObjName)
                             {
                                 SystemObjectMatch sysObj = new SystemObjectMatch(this, subAndSysObjName, SysObjType);
                                 Matches.Add(sysObj);
                             };
            }


            //проверем размер файла или папки если он был задан
            if (CheckSize)
            {
                Func<string, double, bool> GetSizeResult = (path, size) =>
                                                           {
                                                               if (size > _max_size || size < _min_size)
                                                               {
                                                                   AddLog(LogType.Error, this, "{4}:[{0}] Was Ignored. Current {4} Size - {1} MB. Max - {2} MB; Min - {3} MB.",
                                                                                   path, size, MaxSizeMB, MinSizeMB, SysObjType);
                                                                   return true;
                                                               }
                                                               return false;
                                                           };

                if (SysObjType == FindType.Files)
                {
                    checkSizeOfSysObj = delegate (string filePath)
                                        {
                                            double size = Math.Round(double.Parse((new FileInfo(filePath).Length / 1024 / 1024).ToString()));
                                            return GetSizeResult(filePath, size);
                                        };
                }
                else if (SysObjType == FindType.Directories)
                {
                    checkSizeOfSysObj = delegate (string dirPath)
                                        {
                                            double size = Math.Round(double.Parse((DirSize(new DirectoryInfo(dirPath)) / 1024 / 1024).ToString()));
                                            return GetSizeResult(dirPath, size);
                                        };
                }
            }

        }

        List<SystemObjectMatch> SearchFilesDirs(string dirPath, bool subDirs, FindType type)
        {
            List<SystemObjectMatch> list = new List<SystemObjectMatch>();

            Directory.GetFiles(dirPath, "*");

            return null;
        }

        public override void Execute()
        {
            foreach (string fullSysObjPath in getSysObj())
            {
                if (checkSizeOfSysObj != null)
                {
                    //если размер файла или папки не удовлетворяет условияю
                    if (!checkSizeOfSysObj(fullSysObjPath))
                        continue;
                }

                //удаляем полный путь к файлу или папке, т.к. он будет хранится в базовом классе с коллекцией на него
                string subAndSysObjName = fullSysObjPath.Replace(MainDirectoryPath, "").Trim('\\');
                readSysObj(subAndSysObjName);
            }
        }

        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }
    }
}
