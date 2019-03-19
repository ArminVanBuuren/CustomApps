using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Contacts;
using TeleSharp.TL.Messages;
using TeleSharp.TL.Photos;
using TLSharp.Core;
using TLSharp.Core.Utils;

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
        AuthUser = 1
    }

    public class AuthenticateException : Exception
    {
        internal AuthenticateException(string msg) : base(msg) { }
    }
    public class TLControlItemException : Exception
    {
        internal TLControlItemException(string msg) : base(msg) { }
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
    }

    public interface ITLControlItem
    {
        TLAbsInputPeer Destination { get; }
    }

    public abstract class TLControl
    {
        protected virtual int ApiId { get; set; }
        protected virtual string ApiHash { get; set; }
        protected virtual string NumberToAuthenticate { get; set; }
        protected virtual string PasswordToAuthenticate { get; set; }
        protected TelegramClient Client { get; private set; }

        public TLControl(int appiId, string apiHash, string numberToAthenticate = null, string passwordToAuthenticate = null)
        {
            ApiId = appiId;
            ApiHash = apiHash;
            NumberToAuthenticate = numberToAthenticate;
            PasswordToAuthenticate = passwordToAuthenticate;
            Client = NewClient(ApiId, ApiHash);
            Task task = Client.ConnectAsync();
            task.RunSynchronously();
        }

        static TelegramClient NewClient(int appiId, string apiHash)
        {
            try
            {
                return new TelegramClient(appiId, apiHash);
            }
            catch (MissingApiConfigurationException ex)
            {
                throw new Exception($"Please add your API settings.", ex);
            }
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

        public async Task<TLUser> SignUpNewUser(Task<string> getCodeToAuthenticate, string notRegisteredNumber, string firstName, string lastName)
        {
            var hash = await Client.SendCodeRequestAsync(notRegisteredNumber);
            var code = await getCodeToAuthenticate;

            var registeredUser = await Client.SignUpAsync(notRegisteredNumber, hash, code, firstName, lastName);

            if (Client.IsUserAuthorized())
            {

            }

            var loggedInUser = await Client.MakeAuthAsync(notRegisteredNumber, hash, code);

            return loggedInUser;
        }

        public async Task<TLUser> Login()
        {
            var logged = await Client.MakeAuthAsync(notRegisteredNumber, hash, code);

            return logged;
        }

        public async void SendMessageAsync(TLAbsInputPeer destination, string message)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            await Client.SendTypingAsync(destination);
            await Task.Delay(3000);
            await Client.SendMessageAsync(destination, message);
        }

        public async void SendPhotoAsync(TLAbsInputPeer destination, string photoFilePath, string caption = null)
        {
            if(destination == null)
                throw new ArgumentNullException(nameof(destination));
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
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
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

        public async Task<byte[]> DownloadFileFromContactAsync(TLAbsInputPeer contact)
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

        public async Task<byte[]> DownloadPhotoFromContactAsync(ITLControlItem item)
        {
            TLFileLocation photoLocation;
            if (item.GetType() == typeof(TLControlUser))
            {
                var photo = (TLUserProfilePhoto)((TLControlUser)item).User.Photo;
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

        public async Task<ITLControlItem> GetDestination<T>(string destination, bool ignoreCase = false)
        {
            if (typeof(T) == typeof(TLControlUser))
            {
                var user = await GetUserAsync(destination);
                return new TLControlUser(user);
            }
            else if (typeof(T) == typeof(TLControlChat))
            {
                var chat = await GetUserDialogAsync(destination, ignoreCase);
                return new TLControlChat(chat);
            }
            else
            {
                throw new TLControlItemException($"Incorrect type [{typeof(T)}]");
            }
        }

        protected async Task<TLUser> GetUserAsync(string destinationNumber)
        {
            var normalizedDestNumber = NormalizeContactNumber(destinationNumber);
            var contacts = await Client.GetContactsAsync();

            var user = contacts.Users
                .OfType<TLUser>()
                .FirstOrDefault(x => x.Phone == normalizedDestNumber);

            if (user == null)
            {
                throw new System.Exception($"Number was not found in Contacts List of user=[{destinationNumber}]");
            }

            return user;
        }

        protected string NormalizeContactNumber(string number)
        {
            return number.StartsWith("+") ?
                number.Substring(1, number.Length - 1) :
                number;
        }

        protected async Task<TLChannel> GetUserDialogAsync(string titleName, bool ignoreCase)
        {
            var dialogs = (TLDialogs)await Client.GetUserDialogsAsync();

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