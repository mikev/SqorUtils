using Newtonsoft.Json.Serialization;
using Sqor.Utils.Strings;

namespace Sqor.Utils.Json
{
    public class JsonContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Ensures lowercase property names, in addition to using underscores as the separator
        /// </summary>
        protected override string ResolvePropertyName(string propertyName)
        {
            // Temporary hack since this name was not being underscored.
            if (propertyName == "HACKUserId")
                return "userId";

            return propertyName.Underscore().ToLower();
        }

        public string ConvertPropertyName(string propertyName)
        {
            return ResolvePropertyName(propertyName);
        }
    }
}
