using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA
{
    public class DriveTemplate : ObjectTemplate
    {
        private readonly FileInfo _fileInfo;

        public override string UniqueName
        {
            get => FilePath;
            protected set { }
        }

        [DGVColumn(ColumnPosition.After, "Size[Kb]")]
        public double FileSize
        {
            get
            {
                if (_fileInfo != null && _fileInfo.Exists)
                    return Math.Round(((double)_fileInfo.Length / 1024), 2);

                return -1;
            }
        }

        [DGVColumn(ColumnPosition.After, "CreationTime", "dd.MM.yyyy HH:mm:ss")]
        public DateTime CreationTime
        {
            get
            {
                if (_fileInfo != null && _fileInfo.Exists)
                    return _fileInfo.CreationTime;

                return DateTime.MinValue;
            }
        }

        [DGVColumn(ColumnPosition.After, "LastWriteTime", "dd.MM.yyyy HH:mm:ss")]
        public DateTime LastWriteTime
        {
            get
            {
                if (_fileInfo != null && _fileInfo.Exists)
                    return _fileInfo.LastWriteTime;

                return DateTime.MinValue;
            }
        }

        [DGVColumn(ColumnPosition.Last, "File Path")]
        public virtual string FilePath { get; }

        protected DriveTemplate(string filePath)
        {
            Name = IO.GetLastNameInPath(filePath, true);
            FilePath = filePath;
            _fileInfo = new FileInfo(filePath);
        }

        internal static bool GetNameWithId(string fileName, out string newFileName, out int newId)
        {
            var match = Regex.Match(fileName, @"(.+)\.\(\s*(\d+)\s*\)");
            if (match.Success && int.TryParse(match.Groups[2].Value, out var res))
            {
                newFileName = match.Groups[1].Value;
                newId = res;
                return true;
            }

            newFileName = fileName;
            newId = -1;
            return false;
        }
    }
}
