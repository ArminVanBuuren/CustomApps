namespace TeleSharp.TL.Interfaces
{
    public interface IChatChannel
    {
        int Id { get; set; }
        bool Creator { get; set; }
        bool Kicked { get; set; }
        bool Left { get; set; }
        string Title { get; set; }
        TLAbsChatPhoto Photo { get; set; }
        int Date { get; set; }
        int Version { get; set; }
    }
}
