using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

namespace Utils.Telegram
{
    public interface ITLControlItem
    {
        TLAbsInputPeer Destination { get; }
    }

    public class TLControlUser : ITLControlItem
    {
        public TLUser User { get; }
        public TLAbsInputPeer Destination { get; }

        internal TLControlUser(TLUser user)
        {
            User = user;
            Destination = new TLInputPeerUser() { UserId = User.Id };
        }
    }

    public class TLControlChannel : ITLControlItem
    {
        public TLChannel Channel { get; }
        public TLAbsInputPeer Destination { get; }

        internal TLControlChannel(TLChannel chat)
        {
            Channel = chat;
            Destination = new TLInputPeerChannel() { ChannelId = chat.Id, AccessHash = chat.AccessHash.Value };
        }
    }

    public class TLControlChat : ITLControlItem
    {
        public TLChat Chat { get; }
        public TLAbsInputPeer Destination { get; }

        internal TLControlChat(TLChat chat)
        {
            Chat = chat;
            Destination = new TLInputPeerChat() { ChatId = chat.Id };
        }
    }
}
