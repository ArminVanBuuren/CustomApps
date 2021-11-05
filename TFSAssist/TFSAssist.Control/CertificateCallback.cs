using System.Net;
using Utils;

namespace TFSAssist.Control
{
    public static class CertificateCallback
    {
        static CertificateCallback()
        {
            ServicePointManager.ServerCertificateValidationCallback = WEB.CertificateValidationCallBack;
        }

        public static void Initialize()
        {
        }
    }
}