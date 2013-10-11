using System;
using System.Runtime.Serialization;

namespace Sqor.Utils.Web
{
    public class HttpForbiddenException : Exception
    {
        public HttpForbiddenException()
        {
        }

        public HttpForbiddenException(string message) : base(message)
        {
        }

        public HttpForbiddenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected HttpForbiddenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}