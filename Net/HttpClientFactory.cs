using System;
using System.Net.Http;

namespace Sqor.Utils.Net
{
    public static class HttpClientFactory
    {
        public static Func<HttpClient> Get { get; set; }

        static HttpClientFactory()
        {
            Get = (() => new HttpClient());
        }
    }
}

