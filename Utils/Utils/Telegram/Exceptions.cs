using System;

namespace Utils.Telegram
{
    public class AuthenticateException : Exception
    {
        internal AuthenticateException(string msg) : base(msg) { }
    }
    public class TLControlItemException : Exception
    {
        internal TLControlItemException(string msg) : base(msg) { }
    }
}
