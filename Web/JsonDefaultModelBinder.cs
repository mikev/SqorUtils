using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Sqor.Utils.Json;
using Sqor.Utils.Streams;

namespace Sqor.Utils.Web
{
    public class JsonDefaultModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelName == "action")
            {
                var collection = (ValueProviderCollection)bindingContext.ValueProvider;
                var result = collection.Where(x => !(x is RouteDataValueProvider)).Select(x => x.GetValue(bindingContext.ModelName)).FirstOrDefault(x => x != null);
                return result != null ? result.RawValue : null;
            }

            if (bindingContext.ModelType == typeof(Stream))
            {
                var request = controllerContext.RequestContext.HttpContext.Request;
                var inputStream = request.InputStream;
                inputStream.Position = 0;
                var bytes = inputStream.ReadBytesToEnd();
                return new MemoryStream(bytes);
            }

            if (IsJsonType(bindingContext.ModelType))
            {
                var request = controllerContext.RequestContext.HttpContext.Request;
                var inputStream = request
                    .InputStream;
                inputStream.Position = 0;
                var content = new StreamReader(inputStream).ReadToEnd();
                var json = content.FromJson();

                if (bindingContext.ModelType.IsArray)
                {
                    var jsonArray = (JsonArray)json;
                    var elementType = bindingContext.ModelType.GetElementType();
                    var array = Array.CreateInstance(elementType, jsonArray.Count);
                    for (var i = 0; i < array.Length; i++)
                    {
                        var jsonObject = (JsonObject)jsonArray[i];
                        var element = jsonObject.To(elementType);
                        array.SetValue(element, i);
                    }
                    return array;
                }
                else
                {
                    var jsonObject = (JsonObject)json;
                    var result = jsonObject.To(bindingContext.ModelType);
                    return result;
                }
            }

            var model = base.BindModel(controllerContext, bindingContext);
            return model;
        }

        public bool IsJsonType(Type type)
        {
            if (type.IsArray)
                return IsJsonType(type.GetElementType());
            else if (type.IsValueType)
                return false;
            else if (type == typeof(string))
                return false;
            else
                return true;
        }
    }
}