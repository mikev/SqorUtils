using System.Web.Mvc;

namespace Sqor.Utils.Web
{
    public class BaseActionInvoker : ControllerActionInvoker
    {
        protected override ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
        {
            var result = base.FindAction(controllerContext, controllerDescriptor, actionName);
            return result != null ? new BaseActionDescriptor(result) : null;
        }

        protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
        {
            // By default, the status description of an HttpUnauthorizedResult is not sent to the 
            // client.  The following code addresses that and sends back the description as the 
            // body.
            var unauthorizedResult = actionResult as HttpUnauthorizedResult;
            if (unauthorizedResult != null)
            {
                controllerContext.HttpContext.Response.Write(unauthorizedResult.StatusDescription);
            }

            base.InvokeActionResult(controllerContext, actionResult);
        }
    }
}