using System;
using System.Net;

namespace Utils.Handles
{
    [Serializable]
    public class WebDownload : WebClient
    {
        /// <summary>
        /// Time in milliseconds
        /// </summary>
        public int Timeout { get; set; } // default timeout is 100 seconds (ASP .NET is 90 seconds)

        public WebDownload() : this(60000) { }

        public WebDownload(int timeout)
        {
            Timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request != null)
            {
                request.Timeout = Timeout;
            }
            return request;
        }
    }
}
