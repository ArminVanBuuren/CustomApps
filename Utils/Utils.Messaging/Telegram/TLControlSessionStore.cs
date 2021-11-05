using System;
using System.Reflection;
using Microsoft.Win32;
using TLSharp.Core;
using Utils.Handles;

namespace Utils.Messaging.Telegram
{
    internal class TLControlSessionStore : ISessionStore
    {
        private readonly Assembly _runningApp;
        public TLControlSessionStore(Assembly runningApp)
        {
            _runningApp = runningApp;
        }

        public Session Load(string sessionUserId)
        {
            using (var regedit = new RegeditControl(_runningApp.GetAssemblyInfo().ApplicationName))
            {
                if (regedit[sessionUserId] != null)
                {
                    var buffer = new byte[2048];
                    var source = (byte[]) regedit[sessionUserId, RegistryValueKind.Binary];
                    Array.Copy(source, buffer, source.Length);
                    return Session.FromBytes(buffer, this, sessionUserId);
                }
            }

            return null;
        }

        public void Save(Session session)
        {
            using (var regedit = new RegeditControl(_runningApp.GetAssemblyInfo().ApplicationName))
            {
                regedit[session.SessionUserId, RegistryValueKind.Binary] = session.ToBytes();
            }
        }
    }
}
