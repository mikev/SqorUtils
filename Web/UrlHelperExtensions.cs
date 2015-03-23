#if FULL
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Sqor.Utils.Dictionaries;

namespace Sqor.Utils.Web
{
    public static class UrlHelperExtensions
    {
        public static string Tweak(this UrlHelper urlHelper, object values = null)
        {
            var routeValues = new RouteValueDictionary(urlHelper.RequestContext.RouteData.Values);

            var queryString = urlHelper.RequestContext.HttpContext.Request.QueryString;
            foreach (string key in queryString.Keys)
            {
                var queryStringValues = queryString.GetValues(key);
                if (queryStringValues != null && queryStringValues.Length > 1)
                {
                    routeValues[key] = queryStringValues;
                }
                else if (queryStringValues != null && queryStringValues.Length > 0)
                {
                    routeValues[key] = queryStringValues[0];
                }
                else
                {
                    routeValues[key] = "";
                }
            }
            if (values != null)
            {
                var newValues = values.ToDictionary();
                routeValues.AddRange(newValues);
            }
            return urlHelper.RouteUrl(routeValues);
        }         

        public static string RouteUrl(UrlHelper urlHelper, RouteValueDictionary routeValues, string routeName = null)
        {
            var arrayValues = new RouteValueDictionary(routeValues.Where(x => x.Value is Array).ToDictionary(x => x.Key, x => x.Value));
            routeValues = new RouteValueDictionary(routeValues.Where(x => !(x.Value is Array)).ToDictionary(x => x.Key, x => x.Value));

            var routeUrl = routeName != null ? urlHelper.RouteUrl(routeName, routeValues) : urlHelper.RouteUrl(routeValues);
            var url = new StringBuilder(routeUrl);
            var alreadyHasQuery = routeUrl.Contains("?");

            Action<string, string> append = (name, value) =>
            {
                if (!alreadyHasQuery)
                {
                    url.Append('?');
                    alreadyHasQuery = true;
                }
                else
                    url.Append('&');

                url.Append(name);
                url.Append('=');
                url.Append(urlHelper.Encode(value));
            };

            foreach (var item in arrayValues)
            {
                var array = (Array)item.Value;
                foreach (var value in array)
                {
                    append(item.Key, value.ToString());
                }
            }

            return url.ToString();
        }
    }
}
#endif