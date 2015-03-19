using System;
using System.Reflection;
using System.Linq;

namespace Sqor.Utils.Injection
{
    public abstract class InjectionModule
    {
        public abstract void Register(Container container);
    }
}

