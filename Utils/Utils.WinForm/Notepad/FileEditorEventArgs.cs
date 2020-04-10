using System;
using System.IO;

namespace Utils.WinForm.Notepad
{
    public class FileEditorEventArgs : EventArgs
    {
        internal FileEditorEventArgs(FileEditor editor, WatcherChangeTypes type)
        {
            ChangeType = type;
            NewFilePath = OldFilePath = editor.FilePath;
            NewText = OldText = editor.Text;
            NewSource = OldSource = editor.Source;
        }

        public string OldFilePath { get; internal set; }
        public string NewFilePath { get; internal set; }

        public string OldSource { get; internal set; }
        public string NewSource { get; internal set; }

        public string OldText { get; internal set; }
        public string NewText { get; internal set; }
        public WatcherChangeTypes ChangeType { get; }
    }
}
