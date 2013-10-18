using System;
using System.Web.Mvc;
using System.Web.Mvc.Async;

namespace Sqor.Utils.Web
{
    public class BaseActionInvoker : AsyncControllerActionInvoker
    {
        protected override ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
        {
            var result = base.FindAction(controllerContext, controllerDescriptor, actionName);
            return result is AsyncActionDescriptor ? (ActionDescriptor)new BaseAsyncActionDescriptor((AsyncActionDescriptor)result) : result != null ? new BaseActionDescriptor(result) : null;
        }

        protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
        {
            // By default, the status description of an HttpUnauthorizedResult is not sent to the 
            // client.  The following code addresses that and sends back the description as the 
            // body.
            var statusCodeResult = actionResult as HttpStatusCodeResult;
            if (statusCodeResult != null)
            {
                actionResult = new HttpStatusCodeResult(statusCodeResult.StatusCode);
                controllerContext.HttpContext.Response.ContentType = "text/plain";
                controllerContext.HttpContext.Response.Write(statusCodeResult.StatusDescription);
            }

            base.InvokeActionResult(controllerContext, actionResult);
        }
    }
}