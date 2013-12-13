using System.Linq;
using System.Web.Mvc;
using Sqor.Utils.Generics;

namespace Sqor.Utils.Web
{
    public class NotNullFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            foreach (var parameter in filterContext.ActionDescriptor.GetParameters())
            {
                bool enforceNotNull = parameter.GetCustomAttributes(typeof(NotNullAttribute), true).Any() || (parameter.ParameterType.IsValueType && !parameter.ParameterType.IsNullableValueType());
                object value;
                var key = parameter.BindingInfo.Prefix ?? parameter.ParameterName;
                if (enforceNotNull && (!filterContext.ActionParameters.TryGetValue(parameter.ParameterName, out value) || value == null))
                {
//                    if (filterContext.RouteData.Values.ContainsKey(parameter.ParameterName))
//                    {
//                        filterContext.ActionParameters[parameter.ParameterName] = filterContext.RouteData.Values[parameter.ParameterName];
//                    }
//                    else
//                    {
                        throw new RestValidationException(string.Format("Parameter '{0}' cannot be null.", key));                    
//                    }
                }
            }
        }
    }
}