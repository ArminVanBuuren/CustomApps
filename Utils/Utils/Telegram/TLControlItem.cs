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
        public TLAbsInputPeer Destination { get; set; }

        internal TLControlUser(TLUser user)
        {
            User = user;
            Destination = new TLInputPeerUser() { UserId = User.Id };
        }

        public static async Task<TLUser> GetUserAsync(TelegramClient client, string destinationNumber)
        {
            var normalizedDestNumber = NormalizeContactNumber(destinationNumber);
            var contacts = await client.GetContactsAsync();

            var user = contacts.Users
                .OfType<TLUser>()
                .FirstOrDefault(x => x.Phone == normalizedDestNumber);

            if (user == null)
            {
                throw new System.Exception($"Number was not found in Contacts List of user=[{destinationNumber}]");
            }

            return user;
        }

        static string NormalizeContactNumber(string number)
        {
            return number.StartsWith("+") ?
                number.Substring(1, number.Length - 1) :
                number;
        }
    }

    public class TLControlChat : ITLControlItem
    {
        public TLChannel Chat { get; }
        public TLAbsInputPeer Destination { get; set; }

        internal TLControlChat(TLChannel chat)
        {
            Chat = chat;
            Destination = new TLInputPeerChannel() { ChannelId = Chat.Id, AccessHash = Chat.AccessHash.Value };
        }

        public static async Task<TLChannel> GetUserDialogAsync(TelegramClient client, string titleName, bool ignoreCase)
        {
            var dialogs = (TLDialogs)await client.GetUserDialogsAsync();

            var chat = dialogs.Chats
                .OfType<TLChannel>()
                .FirstOrDefault(c => c.Title.StringEquals(titleName, ignoreCase));

            if (chat == null)
            {
                throw new System.Exception($"Chat=[{titleName}] was not found in Contacts List of current user.");
            }
            return chat;
        }
    }
}
