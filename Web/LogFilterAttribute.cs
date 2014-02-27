using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using Sqor.Utils.Json;
using Sqor.Utils.Logging;

namespace Sqor.Utils.Web
{
    public class LogFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.RequestContext.HttpContext.Request;

            this.LogInfo(request.HttpMethod + " " + request.RawUrl);
            if (filterContext.ActionParameters.Any())
            {
                var inputStream = request.InputStream;
                inputStream.Position = 0;
                var content = new StreamReader(inputStream).ReadToEnd();
                var s = content.Trim();
                s = s.Substring(0, Math.Min(s.Length, 10000));
                this.LogInfo("Input: " + s);
            }

            base.OnActionExecuting(filterContext);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);

            var jsonResult = filterContext.Result as JsonResult;
            if (jsonResult != null)
            {
                this.LogInfo("Output: " + JsonConvert.SerializeObject(jsonResult.Data).Trim());
            }

            var httpStatusResult = filterContext.Result as HttpStatusCodeResult;
            if (httpStatusResult != null)
            {
                this.LogInfo("Output: HTTP Status " + httpStatusResult.StatusCode + ": " + httpStatusResult.StatusDescription);
            }
        }

        private void WriteParameter(string name, object value, StringBuilder builder, int indent)
        {
            var indentString = new string(' ', indent);
            builder.Append(indentString);
            builder.Append(name);
            builder.Append(": ");
            WriteValue(value, builder, indent);
            builder.AppendLine();
        }

        private void WriteValue(object value, StringBuilder builder, int indent)
        {
            builder.Append(value);
        }
    }
}
