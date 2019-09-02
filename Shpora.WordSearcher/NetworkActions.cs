using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Shpora.WordSearcher
{
    public static class NetworkActions
    {
        public static HttpWebResponse POST(string serverURI, string postData, string token)
        {
            var encoding = new UTF8Encoding();
            var postByteArray = encoding.GetBytes(postData);

            var req = WebRequest.Create(serverURI) as HttpWebRequest;
            req.Method = "POST";
            req.ContentType = " application/json; charset=UTF-8";
            req.Headers.Add("Authorization", " token " + token);
            req.ContentLength = postByteArray.Length;

            using (Stream postStream = req.GetRequestStream())
                postStream.Write(postByteArray, 0, postByteArray.Length);

            return req.GetResponse() as HttpWebResponse;
        }


        public static HttpWebResponse GET(string serverURI, string token)
        {
            var req = WebRequest.Create(serverURI) as HttpWebRequest;
            req.Headers.Add("Authorization", " token " + token);
            return req.GetResponse() as HttpWebResponse;
        }
    }
}
