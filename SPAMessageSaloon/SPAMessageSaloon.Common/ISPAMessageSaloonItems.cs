namespace SPAMessageSaloon.Common
{
    public enum NationalLanguage
    {
        English = 0,
        Russian = 1
    }

    public interface ISPAMessageSaloonItems
    {
        int ActiveProcessesCount { get; }
        int ActiveTotalProgress { get; }
        void ChangeLanguage(NationalLanguage language);
        void SaveData();
    }
}
