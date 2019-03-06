using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class WEB
    {
        public static string WebHttpStringData(string uri, out HttpStatusCode resultHttp, HttpRequestCacheLevel cashLevel = HttpRequestCacheLevel.Default)
        {
            return WebHttpStringData(new Uri(uri), out resultHttp, cashLevel);
        }

        public static string WebHttpStringData(Uri uri, out HttpStatusCode resultHttp, HttpRequestCacheLevel cashLevel = HttpRequestCacheLevel.Default)
        {
            resultHttp = HttpStatusCode.BadRequest;
            SetDefaultPolicy(true, cashLevel);
            HttpWebResponse response = (HttpWebResponse)GetWebResponse(uri, cashLevel);
            try
            {
                resultHttp = response.StatusCode;
                if (resultHttp == HttpStatusCode.OK)
                {
                    //List<string> ddd = new List<string>();
                    //WebHeaderCollection dd = response.Headers;
                    //foreach (string s in dd)
                    //{
                    //    ddd.Add(response.GetResponseHeader(s));
                    //}


                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream;

                    if (response.CharacterSet == null)
                        readStream = new StreamReader(receiveStream);
                    else
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                    string resultStr = readStream.ReadToEnd();
                    readStream.Close();
                    return resultStr;
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                response.Close();
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
            HttpRequestCachePolicy cachePolicy = new HttpRequestCachePolicy(reqLevel); // Define a cache policy for this request only. 
            request.CachePolicy = cachePolicy;
            WebResponse response = request.GetResponse();
            return response;
        }

        /// <summary>
        /// Set a default policy level for the "http:" and "https" schemes.
        /// </summary>
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
    }
}
