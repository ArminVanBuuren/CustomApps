using FastColoredTextBoxNS;

namespace Utils.WinForm.Notepad
{
    public class BlankDocument
    {
        public string HeaderName { get; set; }
        public string BodyText { get; set; }
        public Language Language { get; set; } = Language.Custom;
    }
}
