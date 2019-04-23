using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TeleSharp.TL.Updates;
using TLSharp.Core;
using TLSharp.Core.Utils;

namespace Utils.Telegram
{
    public abstract class TLControl : IDisposable
    {
        private int _appiId;
        private string _apiHash;

        public static string SessionName => $"{nameof(TLControl)}Session";
        static readonly DateTime mdt = new DateTime(1970, 1, 1, 0, 0, 0);
        //public event TLControlhandler OnProcessingError;
        protected TelegramClient Client { get; private set; }
        internal TLControlSessionStore Session { get; }

        public TLControlUser UserHost { get; private set; }
        public bool IsAuthorized => Client.IsUserAuthorized();
        public bool IsConnected => Client != null && Client.IsConnected;

        protected TLControl(int appiId, string apiHash)
        {
            _appiId = appiId;
            _apiHash = apiHash;

            Session = new TLControlSessionStore();

            Client = new TelegramClient(_appiId, _apiHash, Session, SessionName);
        }

        public async Task ConnectAsync(bool reconnect = false)
        {
            await Client.ConnectAsync(reconnect);

            if (!IsAuthorized)
                throw new AuthorizationException("Authorize user first!");
            
            UserHost = new TLControlUser(Client.UserHost);
        }

        public async Task<TLUser> AuthUserAsync(Func<Task<string>> getCodeToAuthenticate, string phoneNumber, string password = null)
        {
            if (getCodeToAuthenticate == null)
                throw new ArgumentNullException(nameof(getCodeToAuthenticate));

            var hash = await SendCodeRequestAsync(phoneNumber);
            var code = await getCodeToAuthenticate.Invoke();

            return await MakeAuthAsync(phoneNumber, hash, code, password);
        }


        public async Task<string> SendCodeRequestAsync(string phoneNumber)
        {
            if (phoneNumber.IsNullOrEmptyTrim())
                throw new ArgumentException($"Argument '{nameof(phoneNumber)}' is empty. TLControl need your number to authenticate.");

            return await Client.SendCodeRequestAsync(phoneNumber);
        }

        public async Task<TLUser> MakeAuthAsync(string phoneNumber, string hash, string code, string password = null)
        {
            if (phoneNumber.IsNullOrEmptyTrim())
                throw new ArgumentException($"Argument '{nameof(phoneNumber)}' is empty. TLControl need your number to authenticate.");

            TLUser user;
            try
            {
                user = await Client.MakeAuthAsync(phoneNumber, hash, code);
            }
            catch (CloudPasswordNeededException)
            {
                if (password.IsNullOrEmptyTrim())
                    throw;

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
            var hash = await SendCodeRequestAsync(notRegisteredNumber);
            var code = await getCodeToAuthenticate;

            //var registeredUser = await Client.SignUpAsync(notRegisteredNumber, hash, code, firstName, lastName);

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

        public async Task SendBigFileAsync(TLAbsInputPeer destination, string filePath, string caption = null, string mimeType = null)
        {
            if (!File.Exists(filePath))
                throw new Exception($"File=[{filePath}] not found.");

            var fileName = Path.GetFileName(filePath);

            string capt = caption;
            if (capt.IsNullOrEmptyTrim())
                capt = fileName;

            string mimT = mimeType;
            if (mimT.IsNullOrEmptyTrim())
                mimT = GetMimeType(fileName);

            var fileResult = await Client.UploadFile(fileName, new StreamReader(filePath));

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
            TLMessagesSlice messages;
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

        static string GetMimeType(string file)
        {
            string mimeType = "text/plain";
            string extension = Path.GetExtension(file)?.Trim('.').ToLower();

            switch (extension)
            {
                case "acx": mimeType = "application/internet-property-stream"; break;
                case "ai": mimeType = "application/postscript"; break;
                case "aif": mimeType = "audio/x-aiff"; break;
                case "aifc": mimeType = "audio/x-aiff"; break;
                case "aiff": mimeType = "audio/x-aiff"; break;
                case "asf": mimeType = "video/x-ms-asf"; break;
                case "asr": mimeType = "video/x-ms-asf"; break;
                case "asx": mimeType = "video/x-ms-asf"; break;
                case "au": mimeType = "audio/basic"; break;
                case "wmv":
                case "avi": mimeType = "video/x-msvideo"; break;
                case "axs": mimeType = "application/olescript"; break;
                case "bas": mimeType = "text/plain"; break;
                case "bcpio": mimeType = "application/x-bcpio"; break;
                case "bin": mimeType = "application/octet-stream"; break;
                case "bmp": mimeType = "image/bmp"; break;
                case "c": mimeType = "text/plain"; break;
                case "cat": mimeType = "application/vnd.ms-pkiseccat"; break;
                case "cdf": mimeType = "application/x-cdf"; break;
                case "cer": mimeType = "application/x-x509-ca-cert"; break;
                case "class": mimeType = "application/octet-stream"; break;
                case "clp": mimeType = "application/x-msclip"; break;
                case "cmx": mimeType = "image/x-cmx"; break;
                case "cod": mimeType = "image/cis-cod"; break;
                case "cpio": mimeType = "application/x-cpio"; break;
                case "crd": mimeType = "application/x-mscardfile"; break;
                case "crl": mimeType = "application/pkix-crl"; break;
                case "crt": mimeType = "application/x-x509-ca-cert"; break;
                case "csh": mimeType = "application/x-csh"; break;
                case "css": mimeType = "text/css"; break;
                case "dcr": mimeType = "application/x-director"; break;
                case "der": mimeType = "application/x-x509-ca-cert"; break;
                case "dir": mimeType = "application/x-director"; break;
                case "dll": mimeType = "application/x-msdownload"; break;
                case "dms": mimeType = "application/octet-stream"; break;
                case "doc": mimeType = "application/msword"; break;
                case "dot": mimeType = "application/msword"; break;
                case "dvi": mimeType = "application/x-dvi"; break;
                case "dxr": mimeType = "application/x-director"; break;
                case "eps": mimeType = "application/postscript"; break;
                case "etx": mimeType = "text/x-setext"; break;
                case "evy": mimeType = "application/envoy"; break;
                case "exe": mimeType = "application/octet-stream"; break;
                case "fif": mimeType = "application/fractals"; break;
                case "flr": mimeType = "x-world/x-vrml"; break;
                case "gif": mimeType = "image/gif"; break;
                case "GIF": mimeType = "image/gif"; break;
                case "gtar": mimeType = "application/x-gtar"; break;
                case "gz": mimeType = "application/x-gzip"; break;
                case "h": mimeType = "text/plain"; break;
                case "hdf": mimeType = "application/x-hdf"; break;
                case "hlp": mimeType = "application/winhlp"; break;
                case "hqx": mimeType = "application/mac-binhex40"; break;
                case "hta": mimeType = "application/hta"; break;
                case "htc": mimeType = "text/x-component"; break;
                case "htm": mimeType = "text/html"; break;
                case "html": mimeType = "text/html"; break;
                case "htt": mimeType = "text/webviewhtml"; break;
                case "ico": mimeType = "image/x-icon"; break;
                case "ief": mimeType = "image/ief"; break;
                case "iii": mimeType = "application/x-iphone"; break;
                case "ins": mimeType = "application/x-internet-signup"; break;
                case "isp": mimeType = "application/x-internet-signup"; break;
                case "jfif": mimeType = "image/pipeg"; break;
                case "jpe": mimeType = "image/jpeg"; break;
                case "jpeg": mimeType = "image/jpeg"; break;
                case "jpg": mimeType = "image/jpeg"; break;
                case "png": mimeType = "image/png"; break;
                case "js": mimeType = "application/x-javascript"; break;
                case "latex": mimeType = "application/x-latex"; break;
                case "lha": mimeType = "application/octet-stream"; break;
                case "lsf": mimeType = "video/x-la-asf"; break;
                case "lsx": mimeType = "video/x-la-asf"; break;
                case "lzh": mimeType = "application/octet-stream"; break;
                case "m13": mimeType = "application/x-msmediaview"; break;
                case "m14": mimeType = "application/x-msmediaview"; break;
                case "m3u": mimeType = "audio/x-mpegurl"; break;
                case "man": mimeType = "application/x-troff-man"; break;
                case "mdb": mimeType = "application/x-msaccess"; break;
                case "me": mimeType = "application/x-troff-me"; break;
                case "mht": mimeType = "message/rfc822"; break;
                case "mhtml": mimeType = "message/rfc822"; break;
                case "mid": mimeType = "audio/mid"; break;
                case "mny": mimeType = "application/x-msmoney"; break;
                case "mov": mimeType = "video/quicktime"; break;
                case "movie": mimeType = "video/x-sgi-movie"; break;
                case "mp2": mimeType = "video/mpeg"; break;
                case "mp3": mimeType = "audio/mpeg"; break;
                case "m4a": mimeType = "audio/mpeg"; break;
                case "mpa": mimeType = "video/mpeg"; break;
                case "mpe": mimeType = "video/mpeg"; break;
                case "mpeg": mimeType = "video/mpeg"; break;
                case "mpg": mimeType = "video/mpeg"; break;
                case "mpp": mimeType = "application/vnd.ms-project"; break;
                case "mpv2": mimeType = "video/mpeg"; break;
                case "ms": mimeType = "application/x-troff-ms"; break;
                case "msg": mimeType = "application/vnd.ms-outlook"; break;
                case "mvb": mimeType = "application/x-msmediaview"; break;
                case "nc": mimeType = "application/x-netcdf"; break;
                case "nws": mimeType = "message/rfc822"; break;
                case "oda": mimeType = "application/oda"; break;
                case "p10": mimeType = "application/pkcs10"; break;
                case "p12": mimeType = "application/x-pkcs12"; break;
                case "p7b": mimeType = "application/x-pkcs7-certificates"; break;
                case "p7c": mimeType = "application/x-pkcs7-mime"; break;
                case "p7m": mimeType = "application/x-pkcs7-mime"; break;
                case "p7r": mimeType = "application/x-pkcs7-certreqresp"; break;
                case "p7s": mimeType = "application/x-pkcs7-signature"; break;
                case "pbm": mimeType = "image/x-portable-bitmap"; break;
                case "pdf": mimeType = "application/pdf"; break;
                case "pfx": mimeType = "application/x-pkcs12"; break;
                case "pgm": mimeType = "image/x-portable-graymap"; break;
                case "pko": mimeType = "application/ynd.ms-pkipko"; break;
                case "pma": mimeType = "application/x-perfmon"; break;
                case "pmc": mimeType = "application/x-perfmon"; break;
                case "pml": mimeType = "application/x-perfmon"; break;
                case "pmr": mimeType = "application/x-perfmon"; break;
                case "pmw": mimeType = "application/x-perfmon"; break;
                case "pnm": mimeType = "image/x-portable-anymap"; break;
                case "pot": mimeType = "application/vnd.ms-powerpoint"; break;
                case "ppm": mimeType = "image/x-portable-pixmap"; break;
                case "pps": mimeType = "application/vnd.ms-powerpoint"; break;
                case "ppt": mimeType = "application/vnd.ms-powerpoint"; break;
                case "prf": mimeType = "application/pics-rules"; break;
                case "ps": mimeType = "application/postscript"; break;
                case "pub": mimeType = "application/x-mspublisher"; break;
                case "qt": mimeType = "video/quicktime"; break;
                case "ra": mimeType = "audio/x-pn-realaudio"; break;
                case "ram": mimeType = "audio/x-pn-realaudio"; break;
                case "ras": mimeType = "image/x-cmu-raster"; break;
                case "rgb": mimeType = "image/x-rgb"; break;
                case "rmi": mimeType = "audio/mid"; break;
                case "roff": mimeType = "application/x-troff"; break;
                case "rtf": mimeType = "application/rtf"; break;
                case "rtx": mimeType = "text/richtext"; break;
                case "scd": mimeType = "application/x-msschedule"; break;
                case "sct": mimeType = "text/scriptlet"; break;
                case "setpay": mimeType = "application/set-payment-initiation"; break;
                case "setreg": mimeType = "application/set-registration-initiation"; break;
                case "sh": mimeType = "application/x-sh"; break;
                case "shar": mimeType = "application/x-shar"; break;
                case "sit": mimeType = "application/x-stuffit"; break;
                case "snd": mimeType = "audio/basic"; break;
                case "spc": mimeType = "application/x-pkcs7-certificates"; break;
                case "spl": mimeType = "application/futuresplash"; break;
                case "src": mimeType = "application/x-wais-source"; break;
                case "sst": mimeType = "application/vnd.ms-pkicertstore"; break;
                case "stl": mimeType = "application/vnd.ms-pkistl"; break;
                case "stm": mimeType = "text/html"; break;
                case "sv4cpio": mimeType = "application/x-sv4cpio"; break;
                case "sv4crc": mimeType = "application/x-sv4crc"; break;
                case "svg": mimeType = "image/svg+xml"; break;
                case "swf": mimeType = "application/x-shockwave-flash"; break;
                case "t": mimeType = "application/x-troff"; break;
                case "tar": mimeType = "application/x-tar"; break;
                case "tcl": mimeType = "application/x-tcl"; break;
                case "tex": mimeType = "application/x-tex"; break;
                case "texi": mimeType = "application/x-texinfo"; break;
                case "texinfo": mimeType = "application/x-texinfo"; break;
                case "tgz": mimeType = "application/x-compressed"; break;
                case "tif": mimeType = "image/tiff"; break;
                case "tiff": mimeType = "image/tiff"; break;
                case "tr": mimeType = "application/x-troff"; break;
                case "trm": mimeType = "application/x-msterminal"; break;
                case "tsv": mimeType = "text/tab-separated-values"; break;
                case "txt": mimeType = "text/plain"; break;
                case "uls": mimeType = "text/iuls"; break;
                case "ustar": mimeType = "application/x-ustar"; break;
                case "vcf": mimeType = "text/x-vcard"; break;
                case "vrml": mimeType = "x-world/x-vrml"; break;
                case "wav": mimeType = "audio/x-wav"; break;
                case "wcm": mimeType = "application/vnd.ms-works"; break;
                case "wdb": mimeType = "application/vnd.ms-works"; break;
                case "wks": mimeType = "application/vnd.ms-works"; break;
                case "wmf": mimeType = "application/x-msmetafile"; break;
                case "wps": mimeType = "application/vnd.ms-works"; break;
                case "wri": mimeType = "application/x-mswrite"; break;
                case "wrl": mimeType = "x-world/x-vrml"; break;
                case "wrz": mimeType = "x-world/x-vrml"; break;
                case "xaf": mimeType = "x-world/x-vrml"; break;
                case "xbm": mimeType = "image/x-xbitmap"; break;
                case "xla": mimeType = "application/vnd.ms-excel"; break;
                case "xlc": mimeType = "application/vnd.ms-excel"; break;
                case "xlm": mimeType = "application/vnd.ms-excel"; break;
                case "xls": mimeType = "application/vnd.ms-excel"; break;
                case "xlt": mimeType = "application/vnd.ms-excel"; break;
                case "xlw": mimeType = "application/vnd.ms-excel"; break;
                case "xof": mimeType = "x-world/x-vrml"; break;
                case "xpm": mimeType = "image/x-xpixmap"; break;
                case "xwd": mimeType = "image/x-xwindowdump"; break;
                case "z": mimeType = "application/x-compress"; break;
                case "xml": mimeType = "text/xml"; break;
                case "webp": mimeType = "image/webp"; break;
                case "zip": mimeType = "application/zip"; break;
                case "7z": mimeType = "application/zip"; break;
                case "dmg": mimeType = "application/zip"; break;
                case "mp4": mimeType = "video/mp4"; break;
            }

            return mimeType;
        }

        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}