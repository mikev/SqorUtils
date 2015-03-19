#if FULL
using System;
using System.Web;
using Sqor.Utils.Functional;
using Sqor.Utils.Strings;

namespace Sqor.Utils.Web
{
    /// <summary>
    /// Note all the methods are duplicated here because (sadly) HttpContextBase and HttpContext have the exact same
    /// methods but are not type compatible.  
    /// </summary>
    public static class CookieExtensions
    {
        public static void DeleteCookie(this HttpContext context, string group)
        {
            context.Response.Cookies.Set(new HttpCookie(group) { Expires = DateTime.Now.AddDays(-1) });
        }

        public static void DeleteCookie(this HttpContextBase context, string group)
        {
            context.Response.Cookies.Set(new HttpCookie(group) { Expires = DateTime.Now.AddDays(-1) });
        }

        public static string GetCookieValue(this HttpContext context, string group, string name)
        {
            HttpCookie cookie = context.Request.Cookies[group];

            if (cookie != null && !cookie.Value.IsNullOrEmpty())
                return cookie[name] == null ? null : HttpContext.Current.Server.UrlDecode(cookie[name]);

            return null;
        }

        public static void SetCookieValue(this HttpContext context, string groupName, string propName, string propValue)
        {
            SetCookieValue(context, groupName, propName, propValue, LifeTime.Session, 60 * 15);
        }

        public static void SetCookieValue(this HttpContext context, string groupName, string propName, string propValue, LifeTime lifeTime, int duration)
        {
            HttpCookie cookie =
                context.Response.Cookies[groupName].If(o => o.Value != null) ??
                context.Request.Cookies[groupName] ??
                new HttpCookie(groupName);

            if (lifeTime == LifeTime.Permanent)
                cookie.Expires = DateTime.Now + new TimeSpan(duration, 0, 0, 0);

            if (cookie.Value == string.Empty)
                cookie.Values.Clear();

            cookie.Path = "/";

            cookie[propName] = context.Server.UrlEncode(propValue);

            context.Response.Cookies.Set(cookie);
        }

        public static string GetCookieValue(this HttpContextBase context, string group, string name)
        {
            HttpCookie cookie = context.Request.Cookies[group];

            if (cookie != null && !cookie.Value.IsNullOrEmpty())
                return cookie[name] == null ? null : HttpContext.Current.Server.UrlDecode(cookie[name]);

            return null;
        }

        public static void SetCookieValue(this HttpContextBase context, string groupName, string propName, string propValue)
        {
            SetCookieValue(context, groupName, propName, propValue, LifeTime.Session, 60 * 15);
        }

        public static void SetCookieValue(this HttpContextBase context, string groupName, string propName, string propValue, LifeTime lifeTime, int duration)
        {
            HttpCookie cookie =
                context.Response.Cookies[groupName].If(o => o.Value != null) ??
                context.Request.Cookies[groupName] ??
                new HttpCookie(groupName);

            if (lifeTime == LifeTime.Permanent)
                cookie.Expires = DateTime.Now + new TimeSpan(duration, 0, 0, 0);

            if (cookie.Value == string.Empty)
                cookie.Values.Clear();

            cookie.Path = "/";

            cookie[propName] = context.Server.UrlEncode(propValue);
            context.Response.Cookies.Set(cookie);
        }
    }

    public enum LifeTime
    {
        /// <summary>
        /// Session
        /// </summary>
        Session,

        /// <summary>
        /// Permanent
        /// </summary>
        Permanent
    }
}
#endif