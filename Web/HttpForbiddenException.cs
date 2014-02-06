using System.Net;

namespace Sqor.Utils.Web
{
    public class HttpForbiddenException : HttpStatusCodeException
    {
        public HttpForbiddenException(string statusDescription = null) : base(HttpStatusCode.Forbidden, statusDescription)
        {
        }
    }
}
