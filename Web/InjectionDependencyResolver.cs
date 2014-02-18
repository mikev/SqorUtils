using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Sqor.Utils.Injection;

namespace Sqor.Utils.Web
{
    public class InjectionDependencyResolver : IDependencyResolver
    {
        private Container container;

        public InjectionDependencyResolver(Container container)
        {
            this.container = container;
        }

        public void Dispose()
        {
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            yield return GetService(serviceType);
        }

        public object GetService(Type serviceType)
        {
            object result;
            container.TryGet(serviceType, out result);
            return result;
        }
    }
}