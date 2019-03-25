using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Telegram;

namespace TFSAssist
{
    public class TFSA_TLControl : TLControl
    {
        public TFSA_TLControl(int appiId, string apiHash, string numberToAthenticate = null, string passwordToAuthenticate = null) : base(appiId, apiHash, numberToAthenticate, passwordToAuthenticate)
        {
        }
    }
}
