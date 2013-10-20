using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Sqor.Utils.Enumerables;

namespace Sqor.Utils.Web
{
    public class NotAreaConstraint : IRouteConstraint
    {
        private HashSet<string> areas;

        public NotAreaConstraint(params string[] areas)
        {
            this.areas = areas.ToHashSet();
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (parameterName == "controller")
            {
                var value = (string)values[parameterName];
                if (!areas.Contains(value))
                    return true;
                else 
                    return false;
            }
            else
            {
                return true;
            }
        }
    }
}