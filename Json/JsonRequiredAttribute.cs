using System;

namespace Sqor.Utils.Json
{
    /// <summary>
    /// Decorate on an input property to indicate that it should be required (and not treated as nullable 
    /// even if it is).
    /// </summary>
    public class JsonRequiredAttribute : Attribute
    {
    }
}