using TeleSharp.TL;

namespace Utils.Telegram
{
    public delegate void TLControlhandler(object sender, TLControlEventArgs args);

    public class TLControlEventArgs
    {
        public TLControlEventArgs(TLUser user, TLControlStatus status, bool actionResult)
        {
            User = user;
            Status = status;
            ActionResult = actionResult;
        }

        public TLUser User { get; }
        private TLControlStatus Status { get; }
        public bool ActionResult { get; }
    }

    public enum TLControlStatus
    {
        None = 0,
        AuthUser = 1,
        NewUser = 2
    }
}
