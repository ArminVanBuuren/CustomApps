using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;

namespace Utils
{
    public static class WEB
    {
        static string[] RestrictedHeaders = new string[] {
            "Accept",
            "Connection",
            "Content-Length",
            "Content-Type",
            "Date",
            "Expect",
            "Host",
            "If-Modified-Since",
            "Keep-Alive",
            "Proxy-Connection",
            "Range",
            "Referer",
            "Transfer-Encoding",
            "User-Agent"
        };

        static readonly Dictionary<string, PropertyInfo> HeaderProperties = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

        static WEB()
        {
            Type type = typeof(HttpWebRequest);
            foreach (string header in RestrictedHeaders)
            {
                string propertyName = header.Replace("-", "");
                PropertyInfo headerProperty = type.GetProperty(propertyName);
                HeaderProperties[header] = headerProperty;
            }
        }

        public static void SetRawHeader(this WebRequest request, string name, string value)
        {
            if (HeaderProperties.ContainsKey(name))
            {
                PropertyInfo property = HeaderProperties[name];
                if (property.PropertyType == typeof(DateTime))
                    property.SetValue(request, DateTime.Parse(value), null);
                else if (property.PropertyType == typeof(bool))
                    property.SetValue(request, Boolean.Parse(value), null);
                else if (property.PropertyType == typeof(long))
                    property.SetValue(request, Int64.Parse(value), null);
                else
                    property.SetValue(request, value, null);
            }
            else
            {
                request.Headers[name] = value;
            }
        }

        public static string WebHttpStringData(string uri, out HttpWebResponse httpWebResponce, HttpRequestCacheLevel cashLevel = HttpRequestCacheLevel.Default)
        {
            return WebHttpStringData(new Uri(uri), out httpWebResponce, cashLevel);
        }

        public static string WebHttpStringData(Uri uri, out HttpWebResponse httpWebResponce, HttpRequestCacheLevel cashLevel = HttpRequestCacheLevel.Default)
        {
            httpWebResponce = null;
            SetDefaultPolicy(true, cashLevel);

            using (HttpWebResponse response = (HttpWebResponse) GetWebResponse(uri, cashLevel))
            {
                if (response == null)
                    return null;

                httpWebResponce = response;
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                Stream receiveStream = response.GetResponseStream();
                if (receiveStream == null)
                    return null;

                if (response.CharacterSet == null)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream))
                    {
                        return readStream.ReadToEnd();
                    }
                }
                else
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet)))
                    {
                        return readStream.ReadToEnd();
                    }
                }
            }
        }

        public static HttpWebResponse GetHttpWebResponse(string uri, NetworkCredential autorization, HttpRequestCacheLevel reqLevel = HttpRequestCacheLevel.Default)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            return GetHttpWebResponse(request, autorization, reqLevel);
        }

        public static HttpWebResponse GetHttpWebResponse(Uri uri, NetworkCredential autorization, HttpRequestCacheLevel reqLevel = HttpRequestCacheLevel.Default)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            return GetHttpWebResponse(request, autorization, reqLevel);
        }

        static HttpWebResponse GetHttpWebResponse(HttpWebRequest request, NetworkCredential autorization, HttpRequestCacheLevel reqLevel)
        {
            request.SetRawHeader("User-Agent", "Mozilla/5.0"); // некоторые сайты не дают к себе доступ если клиент использует неизвестный браузер
            request.Headers.Add("AUTHORIZATION", "Basic YTph");
            request.ContentType = "text/html";
            request.Credentials = autorization;
            request.PreAuthenticate = true;
            request.Method = "GET";

            HttpRequestCachePolicy cachePolicy = new HttpRequestCachePolicy(reqLevel); // Define a cache policy for this request only. 
            request.CachePolicy = cachePolicy;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response;
        }

        public static WebResponse GetWebResponse(string uri, HttpRequestCacheLevel reqLevel = HttpRequestCacheLevel.Default)
        {
            WebRequest request = WebRequest.Create(uri);
            return GetWebResponse(request, reqLevel);
        }

        public static WebResponse GetWebResponse(Uri uri, HttpRequestCacheLevel reqLevel = HttpRequestCacheLevel.Default)
        {
            WebRequest request = WebRequest.Create(uri);
            return GetWebResponse(request, reqLevel);
        }

        static WebResponse GetWebResponse(WebRequest request, HttpRequestCacheLevel reqLevel)
        {
            request.SetRawHeader("User-Agent", "Mozilla/5.0"); // некоторые сайты не дают к себе доступ если клиент использует неизвестный браузер
            HttpRequestCachePolicy cachePolicy = new HttpRequestCachePolicy(reqLevel); // Define a cache policy for this request only. 
            request.CachePolicy = cachePolicy;
            WebResponse response = request.GetResponse();
            return response;
        }

        /// <summary>
        /// Set a default policy level for the "http:" and "https" schemes.
        /// </summary>
        /// <param name="defaultCertificateValidation"></param>
        /// <param name="defaultLevel"></param>
        public static void SetDefaultPolicy(bool defaultCertificateValidation = true, HttpRequestCacheLevel defaultLevel = HttpRequestCacheLevel.Default)
        {
            //Когда мне нужен клиент, который может подключиться к как можно большему количеству серверов (а не быть как можно более безопасным), я использую это (вместе с настройкой обратного вызова проверки)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            //Set a default policy level for the "http:" and "https" schemes.
            HttpRequestCachePolicy policy = new HttpRequestCachePolicy(defaultLevel);
            HttpWebRequest.DefaultCachePolicy = policy;

            // Сертификат 
            if (defaultCertificateValidation)
                ServicePointManager.ServerCertificateValidationCallback = (s, ce, ch, ssl) => true;
        }

        public static bool CertificateValidationCallBack(
            object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            // If there are errors in the certificate chain, look at each error to determine the cause.
            if ((sslPolicyErrors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain != null && chain.ChainStatus != null)
                {
                    foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus)
                    {
                        if ((certificate.Subject == certificate.Issuer) && (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot))
                        {
                            // Self-signed certificates with an untrusted root are valid. 
                        }
                        else
                        {
                            if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError)
                            {
                                // If there are any other errors in the certificate chain, the certificate is invalid,
                                // so the method returns false.
                                return false;
                            }
                        }
                    }
                }

                // When processing reaches this line, the only errors in the certificate chain are 
                // untrusted root errors for self-signed certificates. These certificates are valid
                // for default Exchange server installations, so return true.
                return true;
            }
            else
            {
                // In all other cases, return false.
                return false;
            }
        }
    }
}
