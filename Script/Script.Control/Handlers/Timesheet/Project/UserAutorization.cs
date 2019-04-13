using System;
using System.Net;
using Script.Control.Auxiliary;
using Script.Control.Handlers.Arguments;
using Script.Handlers;

namespace Script.Control.Handlers.Timesheet.Project
{
    [Serializable]
    public class UserAutorization
    {
        public string FullName { get; internal set; }
        private string _domain, _userName, _password;
        public string Domain => Crypto.DecryptStringAES(_domain, TimesheetHandler.TIMESHEET_NAME);

        [Identifier("User_Name", "Логин с Доменом для авторизации в TimeSheet", false)]
        public string UserName => Crypto.DecryptStringAES(_userName, TimesheetHandler.TIMESHEET_NAME);

        [Identifier("Password", "Пароль для авторизации в TimeSheet", false)]
        public string Password => Crypto.DecryptStringAES(_password, TimesheetHandler.TIMESHEET_NAME);
        public string DomainUserName => string.Format("{0}\\{1}", Domain, UserName);

        public UserAutorization(string userName, string password)
        {
            string[] temp_userName = userName.Split('\\');
            if (temp_userName.Length > 1)
            {
                _userName = Crypto.EncryptStringAES(temp_userName[1], TimesheetHandler.TIMESHEET_NAME);
                _domain = Crypto.EncryptStringAES(temp_userName[0], TimesheetHandler.TIMESHEET_NAME);
            }
            else
            {
                _userName = Crypto.EncryptStringAES(userName, TimesheetHandler.TIMESHEET_NAME);
            }
            _password = Crypto.EncryptStringAES(password, TimesheetHandler.TIMESHEET_NAME);
        }

        public NetworkCredential GetNetworkCredential()
        {
            if (!string.IsNullOrEmpty(Domain))
                return new NetworkCredential(UserName, Password);
            return new NetworkCredential(UserName, Password, Domain);
        }

    }
}
