using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using TLSharp.Core;
using Utils.Handles;

namespace Utils.Telegram
{
    public class TLControlSessionStore : ISessionStore
    {
        public Session Load(string sessionUserId)
        {
            using (RegeditControl regedit = new RegeditControl(ASSEMBLY.ApplicationName))
            {
                if (regedit[sessionUserId] != null)
                {
                    var buffer = new byte[2048];
                    Array.Copy((byte[])regedit[sessionUserId, RegistryValueKind.Binary], buffer, 2048);
                    return Session.FromBytes(buffer, this, sessionUserId);
                }
            }

            return null;
        }

        public void Save(Session session)
        {
            using (RegeditControl regedit = new RegeditControl(ASSEMBLY.ApplicationName))
            {
                regedit[session.SessionUserId, RegistryValueKind.Binary] = session.ToBytes();
            }
        }
    }
}
