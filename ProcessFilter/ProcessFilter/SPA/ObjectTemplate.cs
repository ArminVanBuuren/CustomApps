using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Utils.WinForm.DataGridViewHelper;
using Utils;

namespace SPAFilter.SPA
{
    public abstract class ObjectTemplate : IComparable
    {
        private readonly FileInfo _fileInfo;

        [DGVColumn(ColumnPosition.First, "ID")]
        public int ID { get; set; }

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

        /// <summary>
        /// Если попал под фильтер, значит высвечиваем
        /// </summary>
        public bool IsFiltered { get; internal set; } = true;

        protected ObjectTemplate(string filePath, int id = 0)
        {
            ID = id;
            Name = IO.GetLastNameInPath(filePath, true);
            FilePath = filePath;
            _fileInfo = new FileInfo(filePath);
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

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (!(obj is ObjectTemplate objRes))
                return 1;

            if (ReferenceEquals(this, objRes))
                return 0;

            return Equals(objRes) ? 0 : 1;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ObjectTemplate objRes))
                return false;

            if (objRes.FilePath != null && this.FilePath != null && objRes.FilePath.Equals(this.FilePath, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            else if (objRes.FilePath == null && this.FilePath == null)
            {
                
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return FilePath.ToLower().GetHashCode();
        }
    }
}
