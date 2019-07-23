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

namespace Utils.Messaging.Telegram
{
    public class TLControl : IDisposable
    {
        public static string SessionName => $"{nameof(TLControl)}Session";
        static readonly DateTime mdt = new DateTime(1970, 1, 1, 0, 0, 0);
        //public event TLControlhandler OnProcessingError;
        protected TelegramClient Client { get; private set; }
        internal TLControlSessionStore Session { get; }

        public TLControlUser UserHost { get; private set; }
        public bool IsAuthorized => Client.IsUserAuthorized();
        public bool IsConnected => Client != null && Client.IsConnected;

        public TLControl(int appiId, string apiHash)
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

            var fileName = Path.GetFileName(photoFilePath);

            var capt = caption;
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

            var capt = caption;
            if (capt.IsNullOrEmptyTrim())
                capt = fileName;

            var mimT = mimeType;
            if (mimT.IsNullOrEmptyTrim())
                mimT = GetMimeTypeByFile(fileName);

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
            var userName1 = userName[0] == '@' ? userName : "@" + userName;
            var userName2 = userName[0] == '@' ? userName.TrimStart('@') : userName;

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

            var messages = new List<TLMessage>();

            var startDate = ToIntDate(dateFrom);

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
            var floodExceptionNum = 0;
            var offset = 0;
            var utfStartDate = dateFrom > DateTime.Now ? DateTime.Now.ToUniversalTime() : dateFrom.ToUniversalTime();
            var start = (int) utfStartDate.Subtract(mdt).TotalSeconds;
            var end = -1;

            if (dateTo != null)
            {
                var utfEndDate = dateTo.Value.ToUniversalTime();
                end = (int)utfEndDate.Subtract(mdt).TotalSeconds;

                if (start > end)
                    throw new Exception($"{nameof(dateFrom)} must be less than {nameof(dateTo)}");
            }

            var messageColelction = new List<TLMessage>();

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

            var isContinue = messages.Messages.Count > 0;
            foreach (var msg in messages.Messages)
            {
                offset++;

                if (!(msg is TLMessage))
                    continue;

                var message = (TLMessage) msg;
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
                var res1 = await Client.SendRequestAsync<TLMessagesSlice>
                (new TLRequestGetHistory()
                {
                    Peer = item,
                    Limit = 500,
                    AddOffset = 0,
                    OffsetId = 0
                });

                var res2 = await Client.SendRequestAsync<TLMessagesSlice>
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
            var search = await Client.SendRequestAsync<TLChannelMessages>
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
            var utfStartDate = date.ToUniversalTime();
            var intDate = (int)utfStartDate.Subtract(mdt).TotalSeconds;
            return intDate;
        }

        public static DateTime ToDate(int date)
        {
            var dateTime = mdt.AddSeconds(date).ToLocalTime();
            return dateTime;
        }

        static string NormalizeContactNumber(string number)
        {
            return number.StartsWith("+") ?
                number.Substring(1, number.Length - 1) :
                number;
        }

        public static string GetMimeTypeByFile(string file)
        {
            var extension = Path.GetExtension(file)?.Trim('.');

            switch (extension)
            {
                case "acx": return "application/internet-property-stream";
                case "aif":
                case "aifc":
                case "aiff": return "audio/x-aiff";
                case "asf":
                case "asr":
                case "asx": return "video/x-ms-asf";
                case "au":
                case "snd": return "audio/basic";
                case "wmv":
                case "avi": return "video/x-msvideo";
                case "axs": return "application/olescript";
                case "bcpio": return "application/x-bcpio";
                case "bin":
                case "class":
                case "dms":
                case "exe":
                case "lha":
                case "lzh": return "application/octet-stream";
                case "bmp": return "image/bmp";
                case "cat": return "application/vnd.ms-pkiseccat";
                case "cdf": return "application/x-cdf";
                case "cer":
                case "crt":
                case "der": return "application/x-x509-ca-cert";
                case "clp": return "application/x-msclip";
                case "cmx": return "image/x-cmx";
                case "cod": return "image/cis-cod";
                case "cpio": return "application/x-cpio";
                case "crd": return "application/x-mscardfile";
                case "crl": return "application/pkix-crl";
                case "csh": return "application/x-csh";
                case "css": return "text/css";
                case "dcr": 
                case "dir": 
                case "dxr": return "application/x-director";
                case "dll": return "application/x-msdownload";
                case "doc":
                case "dot": return "application/msword";
                case "dvi": return "application/x-dvi";
                case "ai": 
                case "eps":
                case "ps": return "application/postscript";
                case "etx": return "text/x-setext";
                case "evy": return "application/envoy";
                case "fif": return "application/fractals";
                case "gif": return "image/gif";
                case "gtar": return "application/x-gtar";
                case "gz": return "application/x-gzip";
                case "hdf": return "application/x-hdf";
                case "hlp": return "application/winhlp";
                case "hqx": return "application/mac-binhex40";
                case "hta": return "application/hta";
                case "htc": return "text/x-component";
                case "htm": 
                case "html":
                case "stm": return "text/html";
                case "htt": return "text/webviewhtml";
                case "ico": return "image/x-icon";
                case "ief": return "image/ief";
                case "iii": return "application/x-iphone";
                case "ins": 
                case "isp": return "application/x-internet-signup";
                case "jfif": return "image/pipeg";
                case "jpe":
                case "jpeg": 
                case "jpg": return "image/jpeg";
                case "png": return "image/png";
                case "js": return "application/x-javascript";
                case "latex": return "application/x-latex";
                case "lsf":
                case "lsx": return "video/x-la-asf";
                case "m13":
                case "m14":
                case "mvb": return "application/x-msmediaview";
                case "m3u": return "audio/x-mpegurl";
                case "man": return "application/x-troff-man";
                case "mdb": return "application/x-msaccess";
                case "me": return "application/x-troff-me";
                case "mht":
                case "mhtml":
                case "nws": return "message/rfc822";
                case "mid": 
                case "rmi": return "audio/mid";
                case "mny": return "application/x-msmoney";
                case "mov":
                case "qt": return "video/quicktime";
                case "movie": return "video/x-sgi-movie";
                case "mp2": 
                case "mpa": 
                case "mpe":
                case "mpeg":
                case "mpg": 
                case "mpv2": return "video/mpeg";
                case "mp3":
                case "m4a": return "audio/mpeg";
                case "mpp": return "application/vnd.ms-project";
                case "ms": return "application/x-troff-ms";
                case "msg": return "application/vnd.ms-outlook";
                case "nc": return "application/x-netcdf";
                case "oda": return "application/oda";
                case "p10": return "application/pkcs10";
                case "p12":
                case "pfx": return "application/x-pkcs12";
                case "p7b": 
                case "spc": return "application/x-pkcs7-certificates";
                case "p7c": 
                case "p7m": return "application/x-pkcs7-mime";
                case "p7r": return "application/x-pkcs7-certreqresp";
                case "p7s": return "application/x-pkcs7-signature";
                case "pbm": return "image/x-portable-bitmap";
                case "pdf": return "application/pdf";
                case "pgm": return "image/x-portable-graymap";
                case "pko": return "application/ynd.ms-pkipko";
                case "pma": 
                case "pmc": 
                case "pml": 
                case "pmr": 
                case "pmw": return "application/x-perfmon";
                case "pnm": return "image/x-portable-anymap";
                case "pot":
                case "pps":
                case "ppt": return "application/vnd.ms-powerpoint";
                case "ppm": return "image/x-portable-pixmap";
                case "prf": return "application/pics-rules";
                case "pub": return "application/x-mspublisher";
                case "ra": 
                case "ram": return "audio/x-pn-realaudio";
                case "ras": return "image/x-cmu-raster";
                case "rgb": return "image/x-rgb";
                case "roff": 
                case "t": 
                case "tr": return "application/x-troff";
                case "rtf": return "application/rtf";
                case "rtx": return "text/richtext";
                case "scd": return "application/x-msschedule";
                case "sct": return "text/scriptlet";
                case "setpay": return "application/set-payment-initiation";
                case "setreg": return "application/set-registration-initiation";
                case "sh": return "application/x-sh";
                case "shar": return "application/x-shar";
                case "sit": return "application/x-stuffit";
                case "spl": return "application/futuresplash";
                case "src": return "application/x-wais-source";
                case "sst": return "application/vnd.ms-pkicertstore";
                case "stl": return "application/vnd.ms-pkistl";
                case "sv4cpio": return "application/x-sv4cpio";
                case "sv4crc": return "application/x-sv4crc";
                case "svg": return "image/svg+xml";
                case "swf": return "application/x-shockwave-flash";
                case "tar": return "application/x-tar";
                case "tcl": return "application/x-tcl";
                case "tex": return "application/x-tex";
                case "texi": 
                case "texinfo": return "application/x-texinfo";
                case "tgz": return "application/x-compressed";
                case "tif": 
                case "tiff": return "image/tiff";
                case "trm": return "application/x-msterminal";
                case "tsv": return "text/tab-separated-values";
                case "bas":
                case "c":
                case "h":
                case "txt": return "text/plain";
                case "uls": return "text/iuls";
                case "ustar": return "application/x-ustar";
                case "vcf": return "text/x-vcard";
                case "wav": return "audio/x-wav";
                case "wcm": 
                case "wdb": 
                case "wks": 
                case "wps": return "application/vnd.ms-works";
                case "wmf": return "application/x-msmetafile";
                case "wri": return "application/x-mswrite";
                case "flr":
                case "vrml":
                case "wrl":
                case "wrz":
                case "xaf":
                case "xof": return "x-world/x-vrml";
                case "xbm": return "image/x-xbitmap";
                case "xla":
                case "xlc":
                case "xlm":
                case "xls":
                case "xlt":
                case "xlw": return "application/vnd.ms-excel";
                case "xpm": return "image/x-xpixmap";
                case "xwd": return "image/x-xwindowdump";
                case "z": return "application/x-compress";
                case "xml": return "text/xml";
                case "webp": return "image/webp";
                case "zip":
                case "7z":
                case "dmg": return "application/zip";
                case "mp4": return "video/mp4";
            }

            return "text/plain";
        }

        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}