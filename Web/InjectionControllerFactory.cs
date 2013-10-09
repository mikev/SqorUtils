using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Sqor.Utils.Injection;
using Sqor.Utils.Logging;

namespace Sqor.Utils.Web
{
    public class InjectionControllerFactory : IControllerFactory
    {
        private IControllerFactory defaultFactory;
        private Container container;
        private string @namespace;
        private Assembly assembly;

        public InjectionControllerFactory(Assembly assembly, IControllerFactory defaultFactory, Container container, string @namespace)
        {
            this.assembly = assembly;
            this.defaultFactory = defaultFactory;
            this.container = container;
            this.@namespace = @namespace;
        }

        public IController CreateController(RequestContext requestContext, string controllerName)
        {
            requestContext.HttpContext.Items[typeof(RequestContext)] = requestContext;

            var area = requestContext.RouteData.DataTokens["area"];

            requestContext.HttpContext.Items[typeof(RequestContext)] = requestContext;
            try
            {
                var ns = @namespace + ".Controllers.";
                if (area != null)
                    ns = @namespace + ".Areas." + area + ".Controllers.";

                var typeName = ns + controllerName + "Controller";
                var controllerType = assembly.GetType(typeName, true, true);
                if (controllerType == null)
                {
                    Logger.Instance.LogError("Error finding controller: " + typeName);

                    throw new InvalidOperationException("Error finding controller: " + typeName);                    
                }
                var controller = (IController)container.Get(controllerType);
                requestContext.HttpContext.Items[typeof(ControllerBase)] = controller;
                
                return controller;
            }
            catch (HttpException e)
            {
                // Assume controller not found, but log just in case
                Logger.Instance.LogError("Error finding controller: " + controllerName, e);

                throw new InvalidOperationException("Error finding controller: " + controllerName, e);
            }
        }

        public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
        {
            return defaultFactory.GetControllerSessionBehavior(requestContext, controllerName);
        }

        public void ReleaseController(IController controller)
        {
            defaultFactory.ReleaseController(controller);
        }
    }
}