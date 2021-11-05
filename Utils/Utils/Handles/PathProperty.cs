using System.IO;

namespace Utils.Handles
{


    /// <summary>
    /// Класс для упрощения вывода пути файла, директории и полного пути к файлу или каталогу
    /// </summary>
    public class PathProperty
    {
        string InputPath { get; }
        public PathProperty(string input)
        {
            InputPath = input;
            FullPath = Path.GetFullPath(InputPath);
            FolderPath = Path.GetDirectoryName(FullPath);
            FileName = Path.GetFileName(InputPath);
        }
        public string FullPath { get; }
        public string FolderPath { get; }
        public string FileName { get; }

        public bool Exists
        {
            get
            {
                if (!string.IsNullOrEmpty(FileName))
                    return File.Exists(FullPath);
                return Directory.Exists(FullPath);
            }
        }

        public FileAttributes Attributes
        {
            get
            {
                if (!string.IsNullOrEmpty(FullPath))
                {
                    return File.GetAttributes(FullPath);
                }
                return FileAttributes.Offline;
            }
        }
    }
}
