using System.IO;
using System.Xml;
using Script.Control.Handlers.Arguments;
using XPackage;

namespace Script.Control.Handlers
{
    public class WriteFileHandler : ScriptTemplate
    {
        [Identifier("File_Path", "Путь к файлу для записи.", "Обязательное;")]
        public string FilePath { get; }
        [Identifier("Append", "True = создаем и дополняем уже существующий файл; False = создаем и заменяем существующий файл на новый", "False")]
        public bool Append { get; } = false;

        public WriteFileHandler(XPack parentPack, XmlNode node, LogFill logFill) : base(parentPack, node, logFill)
        {
            FilePath = Attributes[GetXMLAttributeName(nameof(FilePath))];
            bool _tempAppend;
            if (bool.TryParse(Attributes[GetXMLAttributeName(nameof(Append))], out _tempAppend) && _tempAppend)
                Append = true;

            IWriteValue _parent = Parent as IWriteValue;
            if (_parent == null)
                throw new HandlerInitializationException(this);
        }

        public override void Execute()
        {
            string writeContent = ((IWriteValue) Parent).GetOfWriteValue();
            if (!string.IsNullOrEmpty(writeContent))
            {
                if (Append)
                    File.Delete(FilePath);

                using (StreamWriter outputWriter = File.AppendText(FilePath))
                {
                    outputWriter.Write(writeContent);
                }
            }
        }
    }
}
