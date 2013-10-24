using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Async;

namespace Sqor.Utils.Web
{
    public class BaseActionInvoker : AsyncControllerActionInvoker
    {
        protected override ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
        {
            var actionMethods = controllerDescriptor.ControllerType.GetMethods()
                .Where(x => x.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase) || 
                    (x.IsDefined(typeof(ActionNameAttribute)) && x.GetCustomAttribute<ActionNameAttribute>().Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase)))
                .ToArray();
            if (actionMethods.Length == 0)
                return null;
            else
            {
                MethodInfo method;
                if (actionMethods.Length == 1)
                {
                    method = actionMethods.First();                    
                }
                else
                {
                    // Narrow down to only methods that apply based on HTTP method
                    Type expectedAttribute;
                    switch (controllerContext.HttpContext.Request.HttpMethod)
                    {
                        case "GET": 
                            expectedAttribute = typeof(HttpGetAttribute);
                            break;
                        case "POST": 
                            expectedAttribute = typeof(HttpPostAttribute);
                            break;
                        case "PUT": 
                            expectedAttribute = typeof(HttpPutAttribute);
                            break;
                        case "DELETE": 
                            expectedAttribute = typeof(HttpDeleteAttribute);
                            break;
                        default:
                            throw new InvalidOperationException("Unexpected HTTP method: " + controllerContext.HttpContext.Request.HttpMethod);
                    }
                    var actionMethodsByHttpMethod = actionMethods.Where(x => x.IsDefined(expectedAttribute)).ToArray();
                    var defaultActionMethods = actionMethods.Where(x => !x.GetCustomAttributes().Any(y => y is ActionMethodSelectorAttribute)).ToArray();

                    var methods = actionMethodsByHttpMethod.Any() ? actionMethodsByHttpMethod : defaultActionMethods;
                    if (methods.Length == 1)
                    {
                        method = methods.Single();
                    }
                    else
                    {
                        // Apply versioning
                        var versionHeader = controllerContext.HttpContext.Request.Headers["sqor-api-version"];
                        var version = versionHeader == null ? 1 : int.Parse(versionHeader);
                        var methodVersions = methods.ToDictionary(x => x, x => Tuple.Create(
                            !x.IsDefined(typeof(MinimumVersionAttribute)) ? 1 : x.GetCustomAttribute<MinimumVersionAttribute>().Version,
                            !x.IsDefined(typeof(MaximumVersionAttribute)) ? int.MaxValue : x.GetCustomAttribute<MaximumVersionAttribute>().Version
                        ));
                        methods = methods
                            .Select(x => new { Method = x, Versions = methodVersions[x] })
                            .Where(x => version >= x.Versions.Item1 && version <= x.Versions.Item2)
                            .Select(x => x.Method)
                            .ToArray();

                        if (methods.Length > 1)
                            throw new AmbiguousMatchException(string.Format("Could not find appropriate method for action '{0}' in controller '{1}' with HTTP method {2} and minimum version of {3}", actionName, controllerDescriptor.ControllerName, controllerContext.HttpContext.Request.HttpMethod, version));
                        method = methods.Single();
                    }
                }
                var result = CreateActionDescriptor(method, actionName, controllerDescriptor);
                return result is AsyncActionDescriptor ? (ActionDescriptor)new BaseAsyncActionDescriptor((AsyncActionDescriptor)result) : result != null ? new BaseActionDescriptor(result) : null;
            }
        }

        private ActionDescriptor CreateActionDescriptor(MethodInfo method, string actionName, ControllerDescriptor controllerDescriptor)
        {
            if (typeof(Task).IsAssignableFrom(method.ReturnType))
                return new TaskAsyncActionDescriptor(method, actionName, controllerDescriptor);
            else
                return new ReflectedActionDescriptor(method, actionName, controllerDescriptor);
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