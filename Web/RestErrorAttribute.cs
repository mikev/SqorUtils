using System;
using System.Data.Entity.Validation;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Sqor.Utils.Logging;

namespace Sqor.Utils.Web
{
    public class RestErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            var restValidationException = filterContext.Exception as RestValidationException;
            var dbEntityValidationException = filterContext.Exception as DbEntityValidationException;
            var httpStatusCodeException = filterContext.Exception as HttpStatusCodeException;
            if (restValidationException != null)
            {
                this.LogInfo("Validation Failure: " + filterContext.Exception.Message);
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.BadRequest, filterContext.Exception.Message);
            }
            else if (dbEntityValidationException != null)
            {
                var builder = new StringBuilder();
                foreach (var error in dbEntityValidationException.EntityValidationErrors)
                {
                    builder.AppendLine(error.Entry.Entity + ":");
                    foreach (var validationError in error.ValidationErrors)
                    {
                        builder.AppendLine("    " + validationError.PropertyName + ": " + validationError.ErrorMessage);
                    }
                }
                this.LogInfo("Validation Failure: " + builder);
                filterContext.Result = new ContentResult { Content = builder.ToString() };
            }
            else if (httpStatusCodeException != null)
            {
                filterContext.Result = new HttpStatusCodeResult(httpStatusCodeException.StatusCode, httpStatusCodeException.StatusDescription);
                if (httpStatusCodeException.StatusCode == HttpStatusCode.Unauthorized)
                    filterContext.HttpContext.Response.AddHeader("WWW-Authenticate", "AccessToken realm=\"sqor\"");
            }
            else
            {
                this.LogInfo("Unhandled Exception: " + filterContext.Exception);
                var s = filterContext.Exception.ToString();
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.InternalServerError, s);
            }
            filterContext.ExceptionHandled = true;
        }
    }
}