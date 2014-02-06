using System;

namespace Sqor.Utils.Web
{
    public class MaximumVersionAttribute : Attribute
    {
        public int Version { get; private set; }

        public MaximumVersionAttribute(int version)
        {
            Version = version;
        }
    }
}
