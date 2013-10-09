using System.Data.Entity.Validation;
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
            if (restValidationException != null)
            {
                this.LogInfo("Validation Failure: " + filterContext.Exception.Message);
                filterContext.Result = new ContentResult { Content = filterContext.Exception.Message };
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
            else
            {
                this.LogInfo("Validation Failure: " + filterContext.Exception);
                filterContext.Result = new ContentResult { Content = filterContext.Exception.ToString() };
            }
            filterContext.ExceptionHandled = true;
        }
    }
}