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
            var httpUnauthorizedException = filterContext.Exception as HttpUnauthorizedException;
            var httpForbiddenException = filterContext.Exception as HttpForbiddenException;
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
            else if (httpUnauthorizedException != null)
            {
                filterContext.Result = new HttpUnauthorizedResult(httpUnauthorizedException.Message);
                filterContext.HttpContext.Response.AddHeader("WWW-Authenticate", "AccessToken realm=\"sqor\"");
            }
            else if (httpForbiddenException != null)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden, httpForbiddenException.Message);
            }
            else
            {
                this.LogInfo("Unhandled Exception: " + filterContext.Exception);
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.InternalServerError, filterContext.Exception.ToString());
            }
            filterContext.ExceptionHandled = true;
        }
    }
}