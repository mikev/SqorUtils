using System;

namespace Sqor.Utils.Web
{
    public class MinimumVersionAttribute : Attribute
    {
        public int Version { get; private set; }

        public MinimumVersionAttribute(int version)
        {
            Version = version;
        }
    }
}
