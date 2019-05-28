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
            Session = new TLControlSessionStore();
            Client = new TelegramClient(appiId, apiHash, Session, SessionName);
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
                if (message.FromId == sender.Id && message.Date > startDate && funkLocation != null && funkLocation(message.ToId))
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
                DateTime utfEndDate = dateTo.Value.ToUniversalTime();
                end = (int)utfEndDate.Subtract(mdt).TotalSeconds;

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
            (new TLRequestSearch()
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
            string extension = Path.GetExtension(file)?.Trim('.');

            if (extension == null || extension.IsNullOrEmpty())
                return "text/plain";

            return EXTENSION_LIST.TryGetValue(extension, out var result) ? result : "text/plain";
        }

        public static readonly Dictionary<string, string> EXTENSION_LIST = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase)
        {
            {"acx", "application/internet-property-stream"},
            {"ai", "application/postscript"},
            {"aif", "audio/x-aiff"},
            {"aifc", "audio/x-aiff"},
            {"aiff", "audio/x-aiff"},
            {"asf", "video/x-ms-asf"},
            {"asr", "video/x-ms-asf"},
            {"asx", "video/x-ms-asf"},
            {"au", "audio/basic"},
            {"wmv", "video/x-msvideo"},
            {"avi", "video/x-msvideo"},
            {"axs", "application/olescript"},
            {"bas", "text/plain"},
            {"bcpio", "application/x-bcpio"},
            {"bin", "application/octet-stream"},
            {"bmp", "image/bmp"},
            {"c", "text/plain"},
            {"cat", "application/vnd.ms-pkiseccat"},
            {"cdf", "application/x-cdf"},
            {"cer", "application/x-x509-ca-cert"},
            {"class", "application/octet-stream"},
            {"clp", "application/x-msclip"},
            {"cmx", "image/x-cmx"},
            {"cod", "image/cis-cod"},
            {"cpio", "application/x-cpio"},
            {"crd", "application/x-mscardfile"},
            {"crl", "application/pkix-crl"},
            {"crt", "application/x-x509-ca-cert"},
            {"csh", "application/x-csh"},
            {"css", "text/css"},
            {"dcr", "application/x-director"},
            {"der", "application/x-x509-ca-cert"},
            {"dir", "application/x-director"},
            {"dll", "application/x-msdownload"},
            {"dms", "application/octet-stream"},
            {"doc", "application/msword"},
            {"dot", "application/msword"},
            {"dvi", "application/x-dvi"},
            {"dxr", "application/x-director"},
            {"eps", "application/postscript"},
            {"etx", "text/x-setext"},
            {"evy", "application/envoy"},
            {"exe", "application/octet-stream"},
            {"fif", "application/fractals"},
            {"flr", "x-world/x-vrml"},
            {"gif", "image/gif"},
            {"GIF", "image/gif"},
            {"gtar", "application/x-gtar"},
            {"gz", "application/x-gzip"},
            {"h", "text/plain"},
            {"hdf", "application/x-hdf"},
            {"hlp", "application/winhlp"},
            {"hqx", "application/mac-binhex40"},
            {"hta", "application/hta"},
            {"htc", "text/x-component"},
            {"htm", "text/html"},
            {"html", "text/html"},
            {"htt", "text/webviewhtml"},
            {"ico", "image/x-icon"},
            {"ief", "image/ief"},
            {"iii", "application/x-iphone"},
            {"ins", "application/x-internet-signup"},
            {"isp", "application/x-internet-signup"},
            {"jfif", "image/pipeg"},
            {"jpe", "image/jpeg"},
            {"jpeg", "image/jpeg"},
            {"jpg", "image/jpeg"},
            {"png", "image/png"},
            {"js", "application/x-javascript"},
            {"latex", "application/x-latex"},
            {"lha", "application/octet-stream"},
            {"lsf", "video/x-la-asf"},
            {"lsx", "video/x-la-asf"},
            {"lzh", "application/octet-stream"},
            {"m13", "application/x-msmediaview"},
            {"m14", "application/x-msmediaview"},
            {"m3u", "audio/x-mpegurl"},
            {"man", "application/x-troff-man"},
            {"mdb", "application/x-msaccess"},
            {"me", "application/x-troff-me"},
            {"mht", "message/rfc822"},
            {"mhtml", "message/rfc822"},
            {"mid", "audio/mid"},
            {"mny", "application/x-msmoney"},
            {"mov", "video/quicktime"},
            {"movie", "video/x-sgi-movie"},
            {"mp2", "video/mpeg"},
            {"mp3", "audio/mpeg"},
            {"m4a", "audio/mpeg"},
            {"mpa", "video/mpeg"},
            {"mpe", "video/mpeg"},
            {"mpeg", "video/mpeg"},
            {"mpg", "video/mpeg"},
            {"mpp", "application/vnd.ms-project"},
            {"mpv2", "video/mpeg"},
            {"ms", "application/x-troff-ms"},
            {"msg", "application/vnd.ms-outlook"},
            {"mvb", "application/x-msmediaview"},
            {"nc", "application/x-netcdf"},
            {"nws", "message/rfc822"},
            {"oda", "application/oda"},
            {"p10", "application/pkcs10"},
            {"p12", "application/x-pkcs12"},
            {"p7b", "application/x-pkcs7-certificates"},
            {"p7c", "application/x-pkcs7-mime"},
            {"p7m", "application/x-pkcs7-mime"},
            {"p7r", "application/x-pkcs7-certreqresp"},
            {"p7s", "application/x-pkcs7-signature"},
            {"pbm", "image/x-portable-bitmap"},
            {"pdf", "application/pdf"},
            {"pfx", "application/x-pkcs12"},
            {"pgm", "image/x-portable-graymap"},
            {"pko", "application/ynd.ms-pkipko"},
            {"pma", "application/x-perfmon"},
            {"pmc", "application/x-perfmon"},
            {"pml", "application/x-perfmon"},
            {"pmr", "application/x-perfmon"},
            {"pmw", "application/x-perfmon"},
            {"pnm", "image/x-portable-anymap"},
            {"pot", "application/vnd.ms-powerpoint"},
            {"ppm", "image/x-portable-pixmap"},
            {"pps", "application/vnd.ms-powerpoint"},
            {"ppt", "application/vnd.ms-powerpoint"},
            {"prf", "application/pics-rules"},
            {"ps", "application/postscript"},
            {"pub", "application/x-mspublisher"},
            {"qt", "video/quicktime"},
            {"ra", "audio/x-pn-realaudio"},
            {"ram", "audio/x-pn-realaudio"},
            {"ras", "image/x-cmu-raster"},
            {"rgb", "image/x-rgb"},
            {"rmi", "audio/mid"},
            {"roff", "application/x-troff"},
            {"rtf", "application/rtf"},
            {"rtx", "text/richtext"},
            {"scd", "application/x-msschedule"},
            {"sct", "text/scriptlet"},
            {"setpay", "application/set-payment-initiation"},
            {"setreg", "application/set-registration-initiation"},
            {"sh", "application/x-sh"},
            {"shar", "application/x-shar"},
            {"sit", "application/x-stuffit"},
            {"snd", "audio/basic"},
            {"spc", "application/x-pkcs7-certificates"},
            {"spl", "application/futuresplash"},
            {"src", "application/x-wais-source"},
            {"sst", "application/vnd.ms-pkicertstore"},
            {"stl", "application/vnd.ms-pkistl"},
            {"stm", "text/html"},
            {"sv4cpio", "application/x-sv4cpio"},
            {"sv4crc", "application/x-sv4crc"},
            {"svg", "image/svg+xml"},
            {"swf", "application/x-shockwave-flash"},
            {"t", "application/x-troff"},
            {"tar", "application/x-tar"},
            {"tcl", "application/x-tcl"},
            {"tex", "application/x-tex"},
            {"texi", "application/x-texinfo"},
            {"texinfo", "application/x-texinfo"},
            {"tgz", "application/x-compressed"},
            {"tif", "image/tiff"},
            {"tiff", "image/tiff"},
            {"tr", "application/x-troff"},
            {"trm", "application/x-msterminal"},
            {"tsv", "text/tab-separated-values"},
            {"txt", "text/plain"},
            {"uls", "text/iuls"},
            {"ustar", "application/x-ustar"},
            {"vcf", "text/x-vcard"},
            {"vrml", "x-world/x-vrml"},
            {"wav", "audio/x-wav"},
            {"wcm", "application/vnd.ms-works"},
            {"wdb", "application/vnd.ms-works"},
            {"wks", "application/vnd.ms-works"},
            {"wmf", "application/x-msmetafile"},
            {"wps", "application/vnd.ms-works"},
            {"wri", "application/x-mswrite"},
            {"wrl", "x-world/x-vrml"},
            {"wrz", "x-world/x-vrml"},
            {"xaf", "x-world/x-vrml"},
            {"xbm", "image/x-xbitmap"},
            {"xla", "application/vnd.ms-excel"},
            {"xlc", "application/vnd.ms-excel"},
            {"xlm", "application/vnd.ms-excel"},
            {"xls", "application/vnd.ms-excel"},
            {"xlt", "application/vnd.ms-excel"},
            {"xlw", "application/vnd.ms-excel"},
            {"xof", "x-world/x-vrml"},
            {"xpm", "image/x-xpixmap"},
            {"xwd", "image/x-xwindowdump"},
            {"z", "application/x-compress"},
            {"xml", "text/xml"},
            {"webp", "image/webp"},
            {"zip", "application/zip"},
            {"7z", "application/zip"},
            {"dmg", "application/zip"},
            {"mp4", "video/mp4"}
        };



        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}