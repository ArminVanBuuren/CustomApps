namespace Utils.WinForm.CustomProgressBar
{
    public interface IProgressBar
    {
        bool Visible { get; set; }
        int Maximum { get; set; }
        int Value { get; set; }
    }
}
