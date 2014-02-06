using System.Web;
using System.Web.Routing;
using Sqor.Utils.Dictionaries;

namespace Sqor.Utils.Web
{
    public class IntegerRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            var routeValue = values.Get(parameterName);
            if (routeValue is int)
                return true;

            var routeString = routeValue as string;

            int result;
            if (routeString != null && int.TryParse(routeString, out result))
                return true;

            return false;
        }
    }
}
