using System;
using System.Net;

namespace Sqor.Utils.Web
{
    public class HttpStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }

        public HttpStatusCodeException(HttpStatusCode statusCode) : base(statusCode.ToString())
        {
            StatusCode = statusCode;
        }

        public HttpStatusCodeException(HttpStatusCode statusCode, string statusDescription) : base(statusCode + ": " + statusDescription)
        {
            StatusCode = statusCode;
            StatusDescription = statusDescription;
        }
    }
}