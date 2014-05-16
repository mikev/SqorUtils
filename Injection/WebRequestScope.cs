using System.Web;

#if !MONOTOUCH && !MONODROID

namespace Sqor.Utils.Injection.Scopes
{
    public class WebRequestScope : IScope
    {
        private static WebRequestScope instance = new WebRequestScope();

        public static WebRequestScope Instance
        {
            get { return instance; }
        }

        public object GetLock(Request request)
        {
            return null;
        }

        public ICache GetCache(Request request)
        {
            if (HttpContext.Current == null)
                return request.Context.Cache;

            var items = HttpContext.Current.Items;
            var cache = (ICache)items[typeof(WebRequestScope)];
            if (cache == null)
            {
                cache = new Cache();
                items[typeof(WebRequestScope)] = cache;
            }
            return cache;
        }

        public Context GetContext(Context current)
        {
            return current;
        }
    }
}

#endif