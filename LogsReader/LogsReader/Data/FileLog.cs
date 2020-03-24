using System.IO;

namespace LogsReader.Data
{
    public class FileLog
    {
        public FileLog(string server, string filePath)
        {
            Server = server;
            FilePath = filePath;
            FileName = Path.GetFileName(FilePath);
        }

        public string Server { get; }
        public string FileName { get; }
        public string FilePath { get; }

        public override string ToString()
        {
            return $"\\{Server}\\{FilePath}";
        }
    }
}
