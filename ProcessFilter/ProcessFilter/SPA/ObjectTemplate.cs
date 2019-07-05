using System;
using System.IO;
using System.Text.RegularExpressions;
using Utils.WinForm.DataGridViewHelper;
using Utils;

namespace SPAFilter.SPA
{
    public abstract class ObjectTemplate
    {
        private readonly FileInfo _fileInfo;

        [DGVColumn(ColumnPosition.First, "ID")]
        public int ID { get; protected set; }

        [DGVColumn(ColumnPosition.After, "Name")]
        public virtual string Name { get; protected set; }

        [DGVColumn(ColumnPosition.Last, "Size [Kb]")]
        public virtual double FileSize
        {
            get
            {
                if (_fileInfo.Exists)
                    return Math.Round(((double)_fileInfo.Length / 1024), 2);

                return -1;
            }
        }

        [DGVColumn(ColumnPosition.Last, "File Path")]
        public virtual string FilePath { get; }

        protected ObjectTemplate(string path, int id)
        {
            ID = id;
            Name = path.GetLastNameInPath(true);
            FilePath = path;
            _fileInfo = new FileInfo(path);
        }

        protected ObjectTemplate(int id)
        {
            ID = id;
        }

        public override string ToString()
        {
            return Name;
        }

        internal static bool GetNameWithId(string fileName, out string newFileName, out int newId)
        {
            Match match = Regex.Match(fileName, @"(.+)\.\(\s*(\d+)\s*\)");
            if (match.Success && int.TryParse(match.Groups[2].Value, out int res))
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
