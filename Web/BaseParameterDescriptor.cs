using System;
using System.Linq;
using System.Web.Mvc;
using Sqor.Utils.Json;

namespace Sqor.Utils.Web
{
    public class BaseParameterDescriptor : ParameterDescriptor
    {
        private ParameterDescriptor source;
        private BaseParameterBindingInfo bindingInfo;

        public BaseParameterDescriptor(ParameterDescriptor source)
        {
            this.source = source;

            var jsonAttribute = source.GetCustomAttributes(typeof(JsonAttribute), true).Cast<JsonAttribute>().SingleOrDefault();
            bindingInfo = new BaseParameterBindingInfo(source.BindingInfo, prefix: jsonAttribute != null ? jsonAttribute.JsonKey : null);
        }

        public override ActionDescriptor ActionDescriptor
        {
            get { return source.ActionDescriptor; }
        }

        public override string ParameterName
        {
            get { return source.ParameterName; }
        }

        public override Type ParameterType
        {
            get { return source.ParameterType; }
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return source.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return source.GetCustomAttributes(attributeType, inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return source.IsDefined(attributeType, inherit);
        }

        public override ParameterBindingInfo BindingInfo
        {
            get { return bindingInfo; }
        }

        public override object DefaultValue
        {
            get { return source.DefaultValue; }
        }
    }
}
