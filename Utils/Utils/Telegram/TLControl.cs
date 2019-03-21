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
using TeleSharp.TL.Messages;
using TeleSharp.TL.Photos;
using TLSharp.Core;
using TLSharp.Core.Utils;

namespace Utils.Telegram
{
    public abstract class TLControl
    {
        //public event TLControlhandler OnProcessingError;
        protected virtual int ApiId { get; }
        protected virtual string ApiHash { get; }
        protected virtual string NumberToAuthenticate { get; set; }
        protected virtual string PasswordToAuthenticate { get; set; }
        protected TelegramClient Client { get; private set; }
        internal TLControlSessionStore Session { get; }

        public TLControl(int appiId, string apiHash, string numberToAthenticate = null, string passwordToAuthenticate = null)
        {
            if (apiHash == null)
                throw new ArgumentNullException("apiHash");

            ApiId = appiId;
            ApiHash = apiHash;
            NumberToAuthenticate = numberToAthenticate;
            PasswordToAuthenticate = passwordToAuthenticate;
            Session = new TLControlSessionStore();
            Client = new TelegramClient(ApiId, ApiHash, Session, nameof(TLControl));
            //Task task = Client.ConnectAsync();
            //task.RunSynchronously();
        }

        public async Task ConnectAsync(bool reconnect = false)
        {
            await Client.ConnectAsync(reconnect);
        }

        public async Task<TLUser> AuthUserAsync(Task<string> getCodeToAuthenticate)
        {
            if (getCodeToAuthenticate == null)
                throw new ArgumentNullException(nameof(getCodeToAuthenticate));

            if (NumberToAuthenticate == null)
                throw new AuthenticateException("TLControl need your number to authenticate.");

            var hash = await Client.SendCodeRequestAsync(NumberToAuthenticate);
            var code = await getCodeToAuthenticate;

            TLUser user = null;
            try
            {
                user = await Client.MakeAuthAsync(NumberToAuthenticate, hash, code);
            }
            catch (CloudPasswordNeededException ex)
            {
                if(PasswordToAuthenticate == null)
                    throw new AuthenticateException("TLControl need your password.");

                var password = await Client.GetPasswordSetting();

                user = await Client.MakeAuthWithPasswordAsync(password, PasswordToAuthenticate);
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

        public async Task<TLVector<TLAbsMessage>> GetMessagesAsync(TLAbsInputPeer item)
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



            //TLMessage
            var mdt = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime d1 = DateTime.ParseExact("20.03.2019 15:28:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture).ToUniversalTime();
            DateTime d2 = DateTime.ParseExact("20.03.2019 21:34:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture).ToUniversalTime();
            int start = (int)d1.Subtract(mdt).TotalSeconds;
            int end = (int)d2.Subtract(mdt).TotalSeconds;

            var getByDate = new TeleSharp.TL.Messages.TLRequestSearch()
            {
                Peer = item,
                MaxDate = start,
                MinDate = end,
                Q = "s",
                Filter = new TLInputMessagesFilterEmpty(),
                Limit = 100,
                Offset = 0
            };

            //TLChannelMessages search = await Client.SendRequestAsync<TLChannelMessages>(getByDate);
            TLMessages search = await Client.SendRequestAsync<TLMessages>(getByDate);

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

        public async Task SeachByWord(TLAbsInputPeer item, string searchWord)
        {
            //TLChannelMessages search = await Client.SendRequestAsync<TLChannelMessages>
            //(new TeleSharp.TL.Messages.TLRequestSearch()
            //{
            //    Peer = item,
            //    MaxDate = maxdate,
            //    MinDate = mindate,
            //    Q = searchWord,
            //    Filter = new TLInputMessagesFilterEmpty(),
            //    Limit = 100,
            //    Offset = 0
            //});
        }

        static string NormalizeContactNumber(string number)
        {
            return number.StartsWith("+") ?
                number.Substring(1, number.Length - 1) :
                number;
        }
    }
}