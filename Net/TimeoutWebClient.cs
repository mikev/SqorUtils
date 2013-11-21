using System;
using System.Net;
using System.Collections.Generic;

namespace Sqor.Utils.Net
{
    public class TimeoutWebClient : WebClient
    {
        public int Timeout { get; set; }
        public int ReadWriteTimeout { get; set; }
        public DecompressionMethods AutomaticDecompression { get; set; }
        public List<Cookie> Cookies { get; set; }
    
        public TimeoutWebClient()
        {
            Cookies = new List<Cookie>();
        }
        
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            request.ServicePoint.Expect100Continue = false;
            request.Timeout = Timeout;
            request.ReadWriteTimeout = ReadWriteTimeout;
            request.AutomaticDecompression = AutomaticDecompression;
            
            foreach (var cookie in Cookies)
            {
                request.CookieContainer.Add(cookie);
            }
            
            return request;
        }
    }
}

