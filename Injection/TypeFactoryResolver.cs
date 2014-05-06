using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Sqor.Utils.Injection
{
    public class TypeFactoryResolver : IResolver
    {
        private Func<Request, Delegate> factory;
        private static MethodInfo requestCreateChildRequest;
        private static MethodInfo requestResolve;

        static TypeFactoryResolver()
        {
            requestCreateChildRequest = typeof(Request).GetMethod("CreateChildRequest");
            requestResolve = typeof(Request).GetMethod("Resolve");
        }

        public TypeFactoryResolver(Type type)
        {
            var requestParameter = Expression.Parameter(typeof(Request), "_");
            var typeParameter = Expression.Parameter(typeof(Type), "_");

            // Pseudo-code of what this expression tree is building toward
            // var childRequest = requestParameter.CreateChildRequest(type);
            var createChildRequest = Expression.Call(requestParameter, requestCreateChildRequest, typeParameter);

            // var resolution = childRequest.Resolve();
            var resolution = Expression.Call(createChildRequest, requestResolve);

            // var internalFactoryBody = (type)resolution;
            var internalFactoryBody = Expression.Convert(resolution, type);
            var internalFactoryType = typeof(Func<,>).MakeGenericType(typeof(Type), type);

            // Func<type> internalFactory = internalFactoryBody;
            var internalFactory = Expression.Lambda(internalFactoryType, internalFactoryBody, typeParameter);

            // Func<Request, Func<Type>> factory = requestParameter => internalFactory;
            var factory = Expression.Lambda<Func<Request, Delegate>>(internalFactory, requestParameter);

            this.factory = factory.Compile();
        }

        public object Instantiate(Request request)
        {
            return factory(request);
        }

        public void Activate(Request request, object o)
        {
        }
    }
}
