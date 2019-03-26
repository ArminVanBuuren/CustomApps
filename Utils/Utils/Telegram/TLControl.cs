using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Channels;
using TeleSharp.TL.Contacts;
using TeleSharp.TL.Interfaces;
using TeleSharp.TL.Messages;
using TeleSharp.TL.Photos;
using TeleSharp.TL.Updates;
using TLSharp.Core;
using TLSharp.Core.Utils;

namespace Utils.Telegram
{
    public abstract class TLControl
    {
        public static string SessionName => $"{nameof(TLControl)}Session";
        static readonly DateTime mdt = new DateTime(1970, 1, 1, 0, 0, 0);
        //public event TLControlhandler OnProcessingError;
        protected TelegramClient Client { get; private set; }
        internal TLControlSessionStore Session { get; }

        public TLControlUser CurrentUser { get; private set; }

        protected TLControl(int appiId, string apiHash)
        {
            if (apiHash == null)
                throw new ArgumentNullException(nameof(apiHash));

            Session = new TLControlSessionStore();
            Client = new TelegramClient(appiId, apiHash, Session, SessionName);
            //Task task = Client.ConnectAsync();
            //task.RunSynchronously();
        }

        public async Task ConnectAsync(bool reconnect = false)
        {
            await Client.ConnectAsync(reconnect);
            CurrentUser = new TLControlUser(Client.CurrentUser);
        }

        public async Task<TLUser> AuthUserAsync(Task<string> getCodeToAuthenticate, string numberAuth, string password)
        {
            if (getCodeToAuthenticate == null)
                throw new ArgumentNullException(nameof(getCodeToAuthenticate));

            if (numberAuth.IsNullOrEmptyTrim())
                throw new AuthenticateException("TLControl need your number to authenticate.");

            var hash = await Client.SendCodeRequestAsync(numberAuth);
            var code = await getCodeToAuthenticate;

            TLUser user = null;
            try
            {
                user = await Client.MakeAuthAsync(numberAuth, hash, code);
            }
            catch (CloudPasswordNeededException ex)
            {
                if(password.IsNullOrEmptyTrim())
                    throw ex;

                var tlPassword = await Client.GetPasswordSetting();

                user = await Client.MakeAuthWithPasswordAsync(tlPassword, password);
            }
            catch (InvalidPhoneCodeException ex)
            {
                throw new Exception("CodeToAuthenticate is wrong", ex);
            }

            return user;
        }

        public async Task<TLUser> SignUpNewUserAsync(Task<string> getCodeToAuthenticate, string notRegisteredNumber, string firstName, string lastName)
        {
            var hash = await Client.SendCodeRequestAsync(notRegisteredNumber);
            var code = await getCodeToAuthenticate;

            var registeredUser = await Client.SignUpAsync(notRegisteredNumber, hash, code, firstName, lastName);

            var loggedInUser = await Client.MakeAuthAsync(notRegisteredNumber, hash, code);

            return loggedInUser;
        }

        public async Task SendMessageAsync(TLAbsInputPeer destination, string message, int millisecondsDelay = 3000)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            await Client.SendTypingAsync(destination);
            await Task.Delay(millisecondsDelay);
            await Client.SendMessageAsync(destination, message);
        }

        public async Task SendPhotoAsync(TLAbsInputPeer destination, string photoFilePath, string caption = null)
        {
            if(!File.Exists(photoFilePath))
                throw new Exception($"File=[{photoFilePath}] not found.");

            string fileName = Path.GetFileName(photoFilePath);

            string capt = caption;
            if (capt.IsNullOrEmptyTrim())
                capt = fileName;

            var fileResult = (TLInputFile)await Client.UploadFile(fileName, new StreamReader(photoFilePath));
            await Client.SendUploadedPhoto(destination, fileResult, capt);
        }

        public async Task SendBigFileAsync(TLAbsInputPeer destination, string filePath, string caption = null, string mimeType = "application/zip")
        {
            if (!File.Exists(filePath))
                throw new Exception($"File=[{filePath}] not found.");

            var fileName = Path.GetFileName(filePath);

            string capt = caption;
            if (capt.IsNullOrEmptyTrim())
                capt = fileName;

            string mimT = mimeType;
            if (mimT.IsNullOrEmptyTrim())
                mimT = fileName;

            var fileResult = (TLInputFileBig)await Client.UploadFile(fileName, new StreamReader(filePath));

            await Client.SendUploadedDocument(
                destination,
                fileResult,
                capt,
                mimT,
                new TLVector<TLAbsDocumentAttribute>());
        }

        public async Task<byte[]> DownloadFileFromDialogAsync(TLAbsInputPeer contact)
        {
            var res = await Client.SendRequestAsync<TLMessagesSlice>(new TLRequestGetHistory() { Peer = contact });

            var document = res.Messages
                .OfType<TLMessage>()
                .Where(m => m.Media != null)
                .Select(m => m.Media)
                .OfType<TLMessageMediaDocument>()
                .Select(md => md.Document)
                .OfType<TLDocument>()
                .FirstOrDefault();

            if (document == null)
                return null;

            var resFile = await Client.GetFile(
                new TLInputDocumentFileLocation()
                {
                    AccessHash = document.AccessHash,
                    Id = document.Id,
                    Version = document.Version
                }, document.Size);


            return resFile.Bytes;
        }

        public async Task<byte[]> DownloadTitlePhotoAsync(ITLControlItem item)
        {
            TLFileLocation photoLocation;
            if (item.GetType() == typeof(TLControlUser))
            {
                var photo = (TLUserProfilePhoto)((TLControlUser)item).User.Photo;
                photoLocation = (TLFileLocation)photo.PhotoBig;
            }
            else if (item.GetType() == typeof(TLControlChannel))
            {
                var photo = ((TLChatPhoto)((TLControlChannel)item).Channel.Photo);
                photoLocation = (TLFileLocation)photo.PhotoBig;
            }
            else if (item.GetType() == typeof(TLControlChat))
            {
                var photo = ((TLChatPhoto)((TLControlChat)item).Chat.Photo);
                photoLocation = (TLFileLocation)photo.PhotoBig;
            }
            else
            {
                throw new TLControlItemException($"Incorrect type [{item.GetType()}]");
            }

            var resFile = await Client.GetFile(new TLInputFileLocation()
            {
                LocalId = photoLocation.LocalId,
                Secret = photoLocation.Secret,
                VolumeId = photoLocation.VolumeId
            }, 1024);

            return resFile.Bytes;
        }

        public virtual async Task<bool> CheckPhones(string number)
        {
            return await Client.IsPhoneRegisteredAsync(number);
        }

        public async Task<TLControlUser> GetUserAsync(string number)
        {
            var normalizedDestNumber = NormalizeContactNumber(number);
            var contacts = await Client.GetContactsAsync();

            var user = contacts.Users
                .OfType<TLUser>()
                .FirstOrDefault(x => x.Phone == normalizedDestNumber);

            if (user == null)
            {
                return null;
            }

            return new TLControlUser(user);
        }

        public async Task<TLControlUser> GetUserByUserNameAsync(string userName, bool ignoreCase = true)
        {
            string userName1 = userName[0] == '@' ? userName : "@" + userName;
            string userName2 = userName[0] == '@' ? userName.TrimStart('@') : userName;

            var result = await Client.SearchUserAsync(userName1);

            var user = result.Users
                .Where(x => x.GetType() == typeof(TLUser))
                .OfType<TLUser>()
                .FirstOrDefault(x => x.Username.StringEquals(userName2, ignoreCase));

            //if (user == null)
            //{
            //    var contacts = await Client.GetContactsAsync();

            //    user = contacts.Users
            //        .Where(x => x.GetType() == typeof(TLUser))
            //        .OfType<TLUser>()
            //        .FirstOrDefault(x => x.Username.StringEquals(userName2, ignoreCase));
            //}

            if (user == null)
            {
                return null;
            }

            return new TLControlUser(user);
        }

        public async Task<ITLControlItem> GetChatAsync(string titleName, bool ignoreCase = true)
        {
            var dialogs = (TLDialogs)await Client.GetUserDialogsAsync();

            var channel = dialogs.Chats
                .OfType<TLChannel>()
                .FirstOrDefault(c => c.Title.StringEquals(titleName, ignoreCase));

            if (channel == null)
            {
                var chat = dialogs.Chats
                    .OfType<TLChat>()
                    .FirstOrDefault(c => c.Title.StringEquals(titleName, ignoreCase));

                if (chat == null)
                {
                    return null;
                }

                return new TLControlChat(chat);
            }

            return new TLControlChannel(channel);
        }

        async Task GetNewMessagesAsync()
        {
            //while (true)
            //{
            //    var state = await Client.SendRequestAsync<TLState>(new TLRequestGetState());
            //    if (state.UnreadCount > 0)
            //    {
            //        //null
            //    }
            //    await Task.Delay(500);
            //}

            //while (true)
            //{
            //    var state = await Client.SendRequestAsync<TLState>(new TLRequestGetState());
            //    var req = new TLRequestGetDifference() { Date = state.Date, Pts = state.Pts, Qts = state.Qts };
            //    var adiff = await Client.SendRequestAsync<TLAbsDifference>(req);
            //    if (!(adiff is TLDifferenceEmpty))
            //    {
            //        if (adiff is TLDifference)
            //        {
            //            var diff = adiff as TLDifference;
            //            Console.WriteLine("chats:" + diff.Chats.Count);
            //            Console.WriteLine("encrypted:" + diff.NewEncryptedMessages.Count);
            //            Console.WriteLine("new:" + diff.NewMessages.Count);
            //            Console.WriteLine("user:" + diff.Users.Count);
            //            Console.WriteLine("other:" + diff.OtherUpdates.Count);
            //        }
            //        else if (adiff is TLDifferenceTooLong)
            //            Console.WriteLine("too long");
            //        else if (adiff is TLDifferenceSlice)
            //            Console.WriteLine("slice");
            //    }
            //    await Task.Delay(500);
            //}

            //while (true)
            //{
            //    var state = await Client.SendRequestAsync<TLState>(new TLRequestGetState());
            //    var req = new TLRequestGetDifference() { Date = state.Date, Pts = state.Pts, Qts = state.Qts };
            //    var diff = await Client.SendRequestAsync<TLAbsDifference>(req) as TLDifference;
            //    if (diff != null)
            //    {
            //        foreach (var upd in diff.OtherUpdates.OfType<TLUpdateNewChannelMessage>())
            //            Console.WriteLine((upd.Message as TLMessage).Message);

            //        foreach (var ch in diff.Chats.OfType<TLChannel>().Where(x => !x.Left))
            //        {
            //            var ich = new TLInputChannel() { ChannelId = ch.Id, AccessHash = (long)ch.AccessHash };
            //            var readed = new TeleSharp.TL.Channels.TLRequestReadHistory() { Channel = ich, MaxId = -1 };
            //            await Client.SendRequestAsync<bool>(readed);
            //        }
            //    }
            //    await Task.Delay(500);
            //}
        }


        public async Task<List<TLMessage>> GetDifference(TLUser sender, TLAbsInputPeer where, DateTime dateFrom, int lastPts = 32)
        {
            var state = await Client.SendRequestAsync<TLState>(new TLRequestGetState());
            var req = new TLRequestGetDifference() {Date = state.Date, Pts = state.Pts - lastPts, Qts = state.Qts};
            var res = await Client.SendRequestAsync<TLAbsDifference>(req);

            //DateTime newDt0 = mdt.AddSeconds(startDate);
            //DateTime newDt1 = mdt.AddSeconds(state.Date);

            if (!(res is TLDifference))
                return null;

            List<TLMessage> messages = new List<TLMessage>();

            int startDate = ToIntDate(dateFrom);

            Func<object, bool> funkLocation = null;
            switch (where)
            {
                case TLInputPeerUser locUser:
                    funkLocation = (input) => input is TLPeerUser user && user.UserId == locUser.UserId;
                    break;
                case TLInputPeerChat locChat:
                    funkLocation = (input) => input is TLPeerChat chat && chat.ChatId == locChat.ChatId;
                    break;
                case TLInputPeerChannel locChannel:
                    funkLocation = (input) => input is TLPeerChannel channel && channel.ChannelId == locChannel.ChannelId;
                    break;
            }

            var diff = res as TLDifference;
            //var users = diff.Users.OfType<TLUser>().ToDictionary(x => new int?(x.Id), x => x);
            //var chats = diff.Chats.OfType<IChatChannel>().ToDictionary(x => new int?(x.Id), x => x);

            foreach (var message in diff.NewMessages.OfType<TLMessage>())
            {
                //!message.Message.IsNullOrEmptyTrim() &&
                if (message.FromId == sender.Id && message.Date > startDate && funkLocation(message.ToId))
                {
                    messages.Add(message);
                }
            }

            return messages;
        }

        public async Task<List<TLMessage>> GetAllMessagesByDateAsync(TLAbsInputPeer item, DateTime dateFrom, DateTime? dateTo = null, int limitsPerReq = 100)
        {
            int floodExceptionNum = 0;
            int offset = 0;
            DateTime utfStartDate = dateFrom > DateTime.Now ? DateTime.Now.ToUniversalTime() : dateFrom.ToUniversalTime();
            int start = (int) utfStartDate.Subtract(mdt).TotalSeconds;
            int end = -1;

            if (dateTo != null)
            {
                DateTime? utfEndDate = dateTo?.ToUniversalTime();
                end = (int)utfEndDate?.Subtract(mdt).TotalSeconds;

                if (start > end)
                    throw new Exception($"{nameof(dateFrom)} must be less than {nameof(dateTo)}");
            }

            List<TLMessage> messageColelction = new List<TLMessage>();

            getMessages:
            TLMessagesSlice messages = null;
            try
            {
                var req = new TLRequestGetHistory()
                {
                    Peer = item,
                    OffsetId = 0,
                    OffsetDate = 0,
                    AddOffset = offset,
                    Limit = limitsPerReq,
                    MaxId = 0,
                    MinId = 0
                };
                messages = await Client.SendRequestAsync<TLMessagesSlice>(req);
            }
            catch (TLSharp.Core.Network.FloodException)
            {
                if (++floodExceptionNum > 5)
                    throw;
                await Task.Delay(30000);
                goto getMessages;
            }

            bool isContinue = messages.Messages.Count > 0;
            foreach (var msg in messages.Messages)
            {
                offset++;

                if (!(msg is TLMessage))
                    continue;

                TLMessage message = (TLMessage) msg;
                if (start <= message.Date && (end == -1 || end >= message.Date))
                {
                    messageColelction.Add(message);
                }
                else if (start > message.Date)
                {
                    isContinue = false;
                    break;
                }
            }

            if (isContinue && messages.Count > offset)
                goto getMessages;

            return messageColelction;
        }

        public async Task<TLVector<TLAbsMessage>> GetMessagesTestAsync(TLAbsInputPeer item)
        {
            //var res1 = await Client.SendRequestAsync<TLMessagesSlice>(new TLRequestGetHistory() { Peer = item.Destination });

            //if (item is TLControlChat)
            {
                
                //TLChannel channel = ((TLControlChat)item).Chat;
                //var chan = await Client.SendRequestAsync<TeleSharp.TL.Messages.TLChatFull>(new TLRequestGetFullChannel()
                //{
                //    Channel = new TLInputChannel()
                //    {
                //        ChannelId = channel.Id, AccessHash = (long) channel.AccessHash
                //    }
                //});
                //TLInputPeerChannel inputPeer = new TLInputPeerChannel()
                //{
                //    ChannelId = channel.Id, AccessHash = (long) channel.AccessHash
                //};
            }
            
            //while (true)
            {
                TLMessagesSlice res1 = await Client.SendRequestAsync<TLMessagesSlice>
                (new TLRequestGetHistory()
                {
                    Peer = item,
                    Limit = 500,
                    AddOffset = 0,
                    OffsetId = 0
                });

                var offset = 0;
                TLMessagesSlice res2 = await Client.SendRequestAsync<TLMessagesSlice>
                (new TLRequestGetHistory()
                {
                    Peer = item,
                    Limit = 500,
                    AddOffset = res1.Messages.Count - 1,
                    OffsetId = 0
                });

                var msgs = res2.Messages;
                return msgs;
            }
        }

        public async Task<TLChannelMessages> SeachByWord(TLAbsInputPeer item, string searchWord)
        {
            TLChannelMessages search = await Client.SendRequestAsync<TLChannelMessages>
            (new TeleSharp.TL.Messages.TLRequestSearch()
            {
                Peer = item,
                //MaxDate = maxdate,
                //MinDate = mindate,
                Q = searchWord,
                Filter = new TLInputMessagesFilterEmpty(),
                Limit = 100,
                Offset = 0
            });
            return search;
        }

        public static int ToIntDate(DateTime date)
        {
            DateTime utfStartDate = date.ToUniversalTime();
            int intDate = (int)utfStartDate.Subtract(mdt).TotalSeconds;
            return intDate;
        }

        public static DateTime ToDate(int date)
        {
            DateTime dateTime = mdt.AddSeconds(date).ToLocalTime();
            return dateTime;
        }

        static string NormalizeContactNumber(string number)
        {
            return number.StartsWith("+") ?
                number.Substring(1, number.Length - 1) :
                number;
        }
    }
}