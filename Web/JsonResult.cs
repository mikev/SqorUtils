using System.Web.Mvc;
using Sqor.Utils.Json;

namespace Sqor.Utils.Web
{
    public class JsonResult<T> : ActionResult<T>
    {
        public T Data { get; private set; }

        public JsonResult(T data)
        {
            Data = data;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/json";
            var json = Data.ToJson();
            context.HttpContext.Response.Output.Write(json);
        }
    }
}
