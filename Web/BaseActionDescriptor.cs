using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Sqor.Utils.Web
{
    public class BaseActionDescriptor : ActionDescriptor
    {
        private ActionDescriptor source;
        private List<ParameterDescriptor> parameterDescriptors = new List<ParameterDescriptor>();

        public BaseActionDescriptor(ActionDescriptor source)
        {
            this.source = source;
            parameterDescriptors.AddRange(source.GetParameters().Select(x => TransformParameter(x)));
        }

        public ActionDescriptor Source
        {
            get { return source; }
        }

        private ParameterDescriptor TransformParameter(ParameterDescriptor parameterDescriptor)
        {
            return new BaseParameterDescriptor(parameterDescriptor);
        }

        public override object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters)
        {
            return source.Execute(controllerContext, parameters);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return source.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return source.GetCustomAttributes(attributeType, inherit);
        }
#if !MONO
        public override IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache)
        {
            return source.GetFilterAttributes(useCache);
        }
#endif
        public override ParameterDescriptor[] GetParameters()
        {
            return parameterDescriptors.ToArray();
        }

        public override ICollection<ActionSelector> GetSelectors()
        {
            return source.GetSelectors();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return source.IsDefined(attributeType, inherit);
        }

        public override string ActionName
        {
            get { return source.ActionName; }
        }

        public override ControllerDescriptor ControllerDescriptor
        {
            get { return source.ControllerDescriptor; }
        }

        public override string UniqueId
        {
            get { return source.UniqueId; }
        }
    }
}