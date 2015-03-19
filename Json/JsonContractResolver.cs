using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sqor.Utils.Strings;

namespace Sqor.Utils.Json
{
    public class JsonContractResolver : DefaultContractResolver
    {
        public JsonContractResolver()
        {
        }

        protected override JsonContract CreateContract(System.Type objectType)
        {
            var result = base.CreateContract(objectType);

            if (result is JsonObjectContract)
            {
                var jsonContract = (JsonObjectContract)result;
                var dictionaryProperty = objectType.GetRuntimeProperties().Select(p => p.GetMethod).Where(gm => !gm.IsStatic).SingleOrDefault(x => x.IsDefined(typeof(JsonExtensionDataAttribute)));
                if (dictionaryProperty != null)
                {
                    var oldSetter = jsonContract.ExtensionDataSetter;
                    jsonContract.ExtensionDataSetter = (o, key, value) =>
                    {
                        if (value == null)
                        {
                            var dictionary = (IDictionary)dictionaryProperty.Invoke(o, null);
                            if (dictionary != null)
                            {
                                dictionary[key] = null;
                            }
                        }
                        else
                        {
                            oldSetter(o, key, value);
                        }
                    };                    
                }
            }

            return result;
        }

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
