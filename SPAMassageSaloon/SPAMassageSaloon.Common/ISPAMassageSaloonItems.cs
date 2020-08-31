namespace SPAMassageSaloon.Common
{
    public interface ISaloonForm : IUserForm
    {
        int ActiveProcessesCount { get; }

        int ActiveTotalProgress { get; }
    }

    public interface IUserForm
    {
        void ApplySettings();

        void SaveData();

        void ChangeTheme(Themes theme);
    }

    public enum Themes
    {
        Default = 0,
        Dark = 1
    }
}
