using FastColoredTextBoxNS;

namespace Utils.WinForm.Notepad
{
    public class BlankDocument
    {
        public string HeaderName { get; set; } = string.Empty;
        public string BodyText { get; set; } = string.Empty;
        public Language Language { get; set; } = Language.Custom;
    }
}
