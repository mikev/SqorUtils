using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Sqor.Utils.Injection;
using Sqor.Utils.Ios;
using Sqor.Utils.Logging;

namespace Sqor.Utils.Web
{
    public class InjectionControllerFactory : IControllerFactory
    {
        public event Action<IController> Injected;

        private IControllerFactory defaultFactory;
        private Container container;
        private string @namespace;
        private Assembly assembly;
        private Type errorController;

        public InjectionControllerFactory(Assembly assembly, IControllerFactory defaultFactory, Container container, string @namespace, Type errorController)
        {
            this.assembly = assembly;
            this.defaultFactory = defaultFactory;
            this.container = container;
            this.@namespace = @namespace;
            this.errorController = errorController;
        }

        public IController CreateController(RequestContext requestContext, string controllerName)
        {
            requestContext.HttpContext.Items[typeof(RequestContext)] = requestContext;
            var area = requestContext.RouteData.DataTokens["area"];

            try
            {
                var ns = @namespace + ".Controllers.";
                if (area != null)
                    ns = @namespace + ".Areas." + area + ".Controllers.";

                var typeName = ns + controllerName + "Controller";
                var controllerType = assembly.GetType(typeName, false, true);
                if (controllerType == null)
                {
                    controllerType = errorController;
                }
                if (controllerType == null)
                {
                    return null;
                }
                var controller = (IController)container.Get(controllerType);
                requestContext.HttpContext.Items[typeof(ControllerBase)] = controller;

                Injected.Fire(x => x(controller));
                
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