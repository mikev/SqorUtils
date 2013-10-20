using System.Collections.Generic;
using System.Web.Mvc;

namespace Sqor.Utils.Web
{
    public class BaseParameterBindingInfo : ParameterBindingInfo
    {
        private ParameterBindingInfo source;
        private IModelBinder binder;
        private string[] exclude;
        private string[] include;
        private string prefix;

        public BaseParameterBindingInfo(ParameterBindingInfo source, IModelBinder binder = null, string[] exclude = null, string[] include = null, string prefix = null)
        {
            this.source = source;
            this.binder = binder;
            this.exclude = exclude;
            this.include = include;
            this.prefix = prefix;
        }

        public override IModelBinder Binder
        {
            get { return binder ?? source.Binder; }
        }

        public override ICollection<string> Exclude
        {
            get { return exclude ?? source.Exclude; }
        }

        public override ICollection<string> Include
        {
            get { return include ?? source.Include; }
        }

        public override string Prefix
        {
            get { return prefix ?? source.Prefix; }
        }
    }
}
