using System;
using System.Runtime.Serialization;

namespace Sqor.Utils.Web
{
    public class RestValidationException : Exception
    {
        public RestValidationException()
        {
        }

        public RestValidationException(string message) : base(message)
        {
        }

        public RestValidationException(string format, params object[] args) : base(string.Format(format, args))
        {
        }

        public RestValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RestValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}