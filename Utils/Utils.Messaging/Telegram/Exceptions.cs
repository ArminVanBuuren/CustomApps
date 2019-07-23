using System;

namespace Utils.Messaging.Telegram
{
    public class AuthorizationException : Exception
    {
        internal AuthorizationException(string msg) : base(msg) { }
    }

    public class AuthenticateException : Exception
    {
        internal AuthenticateException(string msg) : base(msg) { }
    }

    public class TLControlItemException : Exception
    {
        internal TLControlItemException(string msg) : base(msg) { }
    }
}
