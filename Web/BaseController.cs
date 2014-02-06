using System.Net;
using System.Web.Mvc;

namespace Sqor.Utils.Web
{
    public class BaseController : Controller
    {
        public JsonResult<T> Json<T>(T data)
        {
            return new JsonResult<T>(data);
        }
        
        public HttpStatusCodeResult StatusCode(HttpStatusCode statusCode)
        {
            return new HttpStatusCodeResult((int)statusCode);
        }
    }
}
