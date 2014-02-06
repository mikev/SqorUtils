using System.Net;

namespace Sqor.Utils.Web
{
    public class HttpUnauthorizedException : HttpStatusCodeException
    {
        public HttpUnauthorizedException() : base(HttpStatusCode.Unauthorized)
        {
        }

        public HttpUnauthorizedException(string statusDescription) : base(HttpStatusCode.Unauthorized, statusDescription)
        {
        }
    }
}
