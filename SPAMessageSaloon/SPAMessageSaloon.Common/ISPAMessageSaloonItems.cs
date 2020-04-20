namespace SPAMessageSaloon.Common
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
    }
}
