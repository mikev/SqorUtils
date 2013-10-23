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

        protected override void HandleUnknownAction(string actionName)
        {
            HttpContext.Response.ContentType = "text/plain";
            HttpContext.Response.Write("No rest endpoint found at: " + HttpContext.Request.Url);
        }
    }
}