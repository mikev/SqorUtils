using System;
using System.Linq;
using System.Reflection;

namespace Sqor.Utils.TypeInfos
{
    public static class TypeInfoExtensions
    {
        public static PropertyInfo GetDeclaredProperty(this TypeInfo typeInfo, string name, bool caseSensitive = true)
        {
            if (caseSensitive)
            {
                return typeInfo.GetDeclaredProperty(name);
            }
            else
            {
                return typeInfo.DeclaredProperties.Single(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
        } 
    }
}