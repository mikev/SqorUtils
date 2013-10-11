using System;
using System.Net;
using System.Runtime.Serialization;

namespace Sqor.Utils.Web
{
    public class HttpUnauthorizedException : Exception
    {
        public HttpUnauthorizedException()
        {
        }

        public HttpUnauthorizedException(string message) : base(message)
        {
        }

        public HttpUnauthorizedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected HttpUnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}