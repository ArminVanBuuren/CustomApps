namespace SPAMessageSaloon.Common
{
    public enum NationalLanguage
    {
        English = 0,
        Russian = 1
    }

    public interface ISPAMessageSaloonItems
    {
        void ChangeLanguage(NationalLanguage language);
    }
}
