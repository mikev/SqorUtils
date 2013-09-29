using System;

namespace Sqor.Utils.Injection
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
    }
}