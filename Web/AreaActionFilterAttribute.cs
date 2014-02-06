using System;
using System.Web.Mvc;
using Sqor.Utils.Dictionaries;

namespace Sqor.Utils.Web
{
    public class AreaActionFilterAttribute : ActionFilterAttribute
    {
        public string Area { get; private set; }

        public AreaActionFilterAttribute(string area)
        {
            Area = area;
        }

        protected virtual void OnAreaActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
        }

        protected virtual void OnAreaActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }

        protected virtual void OnAreaResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
        }

        protected virtual void OnAreaResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);
        }

        public sealed override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var area = (string)filterContext.RouteData.DataTokens["area"];
            if (area != null && area.Equals(Area, StringComparison.InvariantCultureIgnoreCase))
                OnAreaActionExecuted(filterContext);
        }

        public sealed override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var area = (string)filterContext.RouteData.DataTokens["area"];
            if (area != null && area.Equals(Area, StringComparison.InvariantCultureIgnoreCase))
                OnAreaActionExecuting(filterContext);
        }

        public sealed override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var area = (string)filterContext.RouteData.DataTokens["area"];
            if (area != null && area.Equals(Area, StringComparison.InvariantCultureIgnoreCase))
                OnAreaResultExecuted(filterContext);
        }

        public sealed override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var area = (string)filterContext.RouteData.DataTokens["area"];
            if (area != null && area.Equals(Area, StringComparison.InvariantCultureIgnoreCase))
                OnAreaResultExecuting(filterContext);
        }
    }
}
